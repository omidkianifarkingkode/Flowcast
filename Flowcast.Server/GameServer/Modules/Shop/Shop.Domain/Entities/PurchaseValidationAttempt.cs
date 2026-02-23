using SharedKernel;

namespace Shop.Domain.Entities;

public sealed class PurchaseValidationAttempt
{
    public long Id { get; private set; }
    public PurchaseId PurchaseId { get; private set; }
    public int AttemptNo { get; private set; }
    public DateTimeOffset StartedAtUtc { get; private set; }
    public DateTimeOffset? FinishedAtUtc { get; private set; }

    public string? ErrorCode { get; private set; }
    public string? ErrorMessage { get; private set; }


    private PurchaseValidationAttempt() { } // EF

    private PurchaseValidationAttempt(
        PurchaseId purchaseId,
        int attemptNo,
        DateTimeOffset startedAtUtc)
    {
        PurchaseId = purchaseId;
        AttemptNo = attemptNo;
        StartedAtUtc = startedAtUtc;
    }

    public static Result<PurchaseValidationAttempt> Start(
        PurchaseId purchaseId,
        int attemptNo,
        DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(purchaseId.Value))
            return Result.Failure<PurchaseValidationAttempt>(
                Error.Validation(
                    "PurchaseAttempt.InvalidPurchaseId",
                    "PurchaseId cannot be empty"));

        if (attemptNo <= 0)
            return Result.Failure<PurchaseValidationAttempt>(
                Error.Validation(
                    "PurchaseAttempt.InvalidAttemptNo",
                    "Attempt number must be greater than zero"));

        return Result.Success(
            new PurchaseValidationAttempt(purchaseId, attemptNo, now));
    }

    public void CompleteSuccess(DateTimeOffset now)
    {
        EnsureNotFinished();
        FinishedAtUtc = now;
        ErrorCode = null;
        ErrorMessage = null;
    }

    public void CompleteFailure(string code, string message, DateTimeOffset now)
    {
        EnsureNotFinished();
        ErrorCode = code;
        ErrorMessage = message;
        FinishedAtUtc = now;
    }

    private void EnsureNotFinished()
    {
        if (FinishedAtUtc.HasValue)
            throw new InvalidOperationException("Validation attempt already completed");
    }
}
