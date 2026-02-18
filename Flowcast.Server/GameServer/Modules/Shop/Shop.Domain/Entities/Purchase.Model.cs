using SharedKernel;
using Shop.Domain.Enums;

namespace Shop.Domain.Entities;

public sealed partial class Purchase : Entity<PurchaseId>
{
    public OrderId OrderId { get; private set; }
    public Store Store { get; private set; }

    public PurchaseToken PurchaseToken { get; private set; }
    public PurchaseSignature? Signature { get; private set; } = null!;

    public string ProductId { get; private set; }
    public string Receipt { get; private set; }
    public string Payload { get; private set; }

    public DateTimeOffset PurchaseAtUtc { get; private set; }
    public string UserId { get; private set; }
    private bool _isSandBox;
    public bool IsSandbox => _isSandBox;
    public PurchaseState State { get; private set; }
    public Dictionary<string, object> Meta { get; private set; } = new();

    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? UpdatedAtUtc { get; private set; }

    private readonly List<PurchaseValidationAttempt> _validationAttempts = new();
    public IReadOnlyCollection<PurchaseValidationAttempt> PurchaseValidationAttempts => _validationAttempts.AsReadOnly();

}
