using Shared.Application.Services;
using SharedKernel;

namespace ZP.Core.Entities;

public sealed partial class PaymentRequest
{
    private PaymentRequest() { }

    public static Result<PaymentRequest> Create(
        PaymentRequestId id,
        Authority authority,
        OrderId orderId,
        string userId,
        int amount,
        string description,
        IDateTimeProvider dateTimeProvider,
        string? currency = null,
        string? idempotencyKey = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Result.Failure<PaymentRequest>(Error.Validation("PaymentRequest.UserIdRequired", "UserId cannot be empty."));
        if (userId.Length > 128)
            return Result.Failure<PaymentRequest>(Error.Validation("PaymentRequest.UserIdTooLong", "UserId length must be at most 128."));
        if (amount <= 0)
            return Result.Failure<PaymentRequest>(Error.Validation("PaymentRequest.AmountPositive", "Amount must be positive."));
        if (string.IsNullOrWhiteSpace(description))
            return Result.Failure<PaymentRequest>(Error.Validation("PaymentRequest.DescriptionRequired", "Description cannot be empty."));
        if (description.Length > 500)
            return Result.Failure<PaymentRequest>(Error.Validation("PaymentRequest.DescriptionTooLong", "Description length must be at most 500."));
        if (idempotencyKey is { Length: > 128 })
            return Result.Failure<PaymentRequest>(Error.Validation("PaymentRequest.IdempotencyKeyTooLong", "IdempotencyKey length must be at most 128."));
        return Result.Success(new PaymentRequest
        {
            Id = id,
            Authority = authority,
            OrderId = orderId,
            UserId = userId.Trim(),
            Amount = amount,
            Currency = currency,
            Description = description.Trim(),
            Status = PaymentRequestStatus.Pending,
            CreatedAtUtc = dateTimeProvider.UtcNowOffset,
            IdempotencyKey = string.IsNullOrWhiteSpace(idempotencyKey) ? null : idempotencyKey.Trim()
        });
    }

    public Result MarkVerified(RefId refId, DateTimeOffset atUtc)
    {
        if (Status == PaymentRequestStatus.Verified)
            return Result.Failure(Error.Conflict("PaymentRequest.AlreadyVerified", "Payment request is already verified."));
        if (Status == PaymentRequestStatus.Failed)
            return Result.Failure(Error.Conflict("PaymentRequest.CannotVerifyFailed", "Cannot verify a failed payment request."));
        RefId = refId;
        VerifiedAtUtc = atUtc;
        Status = PaymentRequestStatus.Verified;
        return Result.Success();
    }

    public Result MarkFailed()
    {
        if (Status == PaymentRequestStatus.Verified)
            return Result.Failure(Error.Conflict("PaymentRequest.CannotFailVerified", "Cannot mark verified payment request as failed."));
        if (Status == PaymentRequestStatus.Failed)
            return Result.Failure(Error.Conflict("PaymentRequest.AlreadyFailed", "Payment request is already marked as failed."));
        Status = PaymentRequestStatus.Failed;
        return Result.Success();
    }

    /// <summary>
    /// Process callback method with check the status from the entity.
    /// </summary>
    /// <param name="isOkStatus"></param>
    /// <param name="verifiedRefId"></param>
    /// <param name="verifiedAt"></param>
    /// <returns></returns>
    public Result<PaymentCallbackOutcome> ProcessCallback(bool isOkStatus, long? verifiedRefId, DateTimeOffset verifiedAt)
    {
        if (isOkStatus && verifiedRefId.HasValue)
        {
            if (Status == PaymentRequestStatus.Verified)
                return Result.Success(new PaymentCallbackOutcome(true, verifiedRefId));
            MarkVerified(ZP.Core.Entities.RefId.Create(verifiedRefId.Value), verifiedAt);
            return Result.Success(new PaymentCallbackOutcome(true, verifiedRefId));
        }
        if (Status != PaymentRequestStatus.Verified && Status != PaymentRequestStatus.Failed)
            MarkFailed();
        return Result.Success(new PaymentCallbackOutcome(false, null));
    }
}

public sealed record PaymentCallbackOutcome(bool Success, long? RefId);
