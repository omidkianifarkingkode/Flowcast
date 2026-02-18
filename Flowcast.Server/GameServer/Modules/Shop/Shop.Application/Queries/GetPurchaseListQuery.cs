using Shared.Application.Messaging;
using SharedKernel;
using Shop.Application.Repositories;
using Shop.Domain.Entities;
using Shop.Domain.Enums;

namespace Shop.Application.Queries;

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

public sealed class GetPurchaseListQueryHandler(IPurchaseRepository purchases) 
    : IQueryHandler<GetPurchaseListQuery, IReadOnlyList<PurchaseListItemDto>>
{
    public async Task<Result<IReadOnlyList<PurchaseListItemDto>>> Handle(GetPurchaseListQuery query, CancellationToken ct)
    {
        var all = await purchases.GetAll(asTracking: false, includeAttempts: true, ct: ct);

        var result = all
            .Select(p => new PurchaseListItemDto(
                Id: p.Id,
                OrderId: p.OrderId,
                Store: p.Store,
                ProductId: p.ProductId,
                UserId: p.UserId,
                State: p.State,
                PurchaseAtUtc: p.PurchaseAtUtc,
                CreatedAtUtc: p.CreatedAtUtc,
                ValidationAttemptsCount: p.ValidationAttempts.Count
                ))
            .ToList();

        return Result.Success<IReadOnlyList<PurchaseListItemDto>>(result);
    }
}



