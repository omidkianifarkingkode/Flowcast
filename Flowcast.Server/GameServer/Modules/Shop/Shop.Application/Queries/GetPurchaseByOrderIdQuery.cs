using Shared.Application.Messaging;
using SharedKernel;
using Shop.Application.Repositories;
using Shop.Domain.Entities;
using Shop.Domain.Shared;

namespace Shop.Application.Queries;

public sealed record GetPurchaseByOrderIdQuery(OrderId OrderId) : IQuery<PurchaseListItemDto>;

public sealed class GetPurchaseByOrderIdQueryHandler(IPurchaseRepository purchases)
    : IQueryHandler<GetPurchaseByOrderIdQuery, PurchaseListItemDto>
{
    public async Task<Result<PurchaseListItemDto>> Handle(GetPurchaseByOrderIdQuery query, CancellationToken ct)
    {
        var existingPurchase = await purchases.GetByOrderId(query.OrderId, asTracking: false, includeAttempts: true, ct: ct);

        if (existingPurchase is null)
            return Result.Failure<PurchaseListItemDto>(PurchaseErrors.PurchaseNotFoundByOrderId(query.OrderId));

        var dto = new PurchaseListItemDto(
            Id: existingPurchase.Id,
            OrderId: existingPurchase.OrderId,
            Store: existingPurchase.Store,
            ProductId: existingPurchase.ProductId,
            UserId: existingPurchase.UserId,
            State: existingPurchase.State,
            PurchaseAtUtc: existingPurchase.PurchaseAtUtc,
            CreatedAtUtc: existingPurchase.CreatedAtUtc,
            ValidationAttemptsCount: existingPurchase.ValidationAttempts.Count
        );

        return Result.Success(dto);
    }
}

