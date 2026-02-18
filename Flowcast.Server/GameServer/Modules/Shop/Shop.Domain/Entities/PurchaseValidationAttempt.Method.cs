namespace Shop.Domain.Entities;

public sealed partial class PurchaseValidationAttempt
{
    private PurchaseValidationAttempt(){ }
    private PurchaseValidationAttempt(PurchaseId purchaseId, int attemptNo)
    {
        PurchaseId = purchaseId;
        AttemptNo = attemptNo;
        StartedAtUtc = DateTime.UtcNow;
    }

    public static PurchaseValidationAttempt Start(PurchaseId purchaseId, int attemptNo)
        => new PurchaseValidationAttempt(purchaseId, attemptNo);

    public void CompleteFailure(string code, string message, DateTimeOffset now)
    {
        ErrorCode = code;
        ErrorMessage = message;
        FinishedAtUtc = now;
    }

    public void CompleteSuccess(DateTimeOffset now)
    {
        FinishedAtUtc = now;
        ErrorCode = null;
        ErrorMessage = null;
    }
}
