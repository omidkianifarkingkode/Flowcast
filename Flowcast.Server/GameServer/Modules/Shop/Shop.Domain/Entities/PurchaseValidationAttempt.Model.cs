namespace Shop.Domain.Entities;

public sealed partial class PurchaseValidationAttempt
{
    public long Id { get; private set; }
    public PurchaseId PurchaseId { get; private set; }
    public int AttemptNo { get; private set; } = 0;
    public DateTimeOffset StartedAtUtc { get; private set; }
    public DateTimeOffset? FinishedAtUtc { get; private set; } 
    public string? ErrorCode { get; private set; } = string.Empty;
    public string? ErrorMessage { get; private set; } = string.Empty;

}
