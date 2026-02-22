using Shared.Application.Messaging;
using Shop.Domain.Entities;
using Shop.Domain.Enums;

namespace Shop.Application.Features.Commands;

public sealed record CreatePurchaseResult(PurchaseId purchaseId, bool IsNew);

public sealed record CreatePurchaseCommand(
    OrderId OrderId,
    Store Store,
    PurchaseToken PurchaseToken,
    PurchaseSignature? Signature,
    string ProductId,
    string Receipt,
    string Payload,
    string UserId,
    DateTimeOffset PurchaseAtUtc,
    bool IsSandbox,
    Dictionary<string, object>? Metadata = null


) : ICommand<CreatePurchaseResult>;
