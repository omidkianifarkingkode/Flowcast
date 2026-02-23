using Shared.Application.Messaging;
using Shop.Domain.Entities;
using Shop.Domain.Enums;

namespace Shop.Application.Features.Queries;

public sealed record GetPurchaseListQuery() : IQuery<IReadOnlyList<PurchaseListItemDto>>;

public record PurchaseListItemDto
(
    PurchaseId Id,
    OrderId OrderId,
    Store Store,
    string ProductId,
    string UserId,
    PurchaseState State,
    DateTimeOffset PurchaseAtUtc,
    DateTimeOffset CreatedAtUtc,
    int ValidationAttemptsCount
    );


