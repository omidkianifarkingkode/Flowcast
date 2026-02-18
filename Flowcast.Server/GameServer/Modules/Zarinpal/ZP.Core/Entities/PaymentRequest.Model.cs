using SharedKernel;

namespace ZP.Core.Entities;

public sealed partial class PaymentRequest : Entity<PaymentRequestId>
{
    public Authority Authority { get; private set; }
    public OrderId OrderId { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public int Amount { get; private set; }
    public string? Currency { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public PaymentRequestStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public RefId? RefId { get; private set; }
    public DateTimeOffset? VerifiedAtUtc { get; private set; }
    public string? IdempotencyKey { get; private set; }
}
