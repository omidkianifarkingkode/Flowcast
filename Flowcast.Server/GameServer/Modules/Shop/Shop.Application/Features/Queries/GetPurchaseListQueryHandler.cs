using Shared.Application.Messaging;
using SharedKernel;
using Shop.Application.IRepositories;

namespace Shop.Application.Features.Queries;

public sealed class GetPurchaseListQueryHandler(
    IPurchaseRepository purchaseRepo
    ) : IQueryHandler<GetPurchaseListQuery, IReadOnlyList<PurchaseListItemDto>>
{
    public async Task<Result<IReadOnlyList<PurchaseListItemDto>>> Handle(GetPurchaseListQuery query, CancellationToken ct)
    {
        try
        {
            var purchases = await purchaseRepo.GetAllPurchase(ct);

            var result = purchases
                .Select(p => new PurchaseListItemDto(
                    Id: p.Id,
                    OrderId: p.OrderId,
                    Store: p.Store,
                    ProductId: p.ProductId,
                    UserId: p.UserId,
                    State: p.State,
                    PurchaseAtUtc: p.PurchaseAtUtc,
                    CreatedAtUtc: p.CreatedAtUtc,
                    ValidationAttemptsCount: p.PurchaseValidationAttempts.Count
                    ))
                .ToList();

            return Result.Success<IReadOnlyList<PurchaseListItemDto>>(result);
        }
        catch(Exception ex)
        {
            return Result.Failure<IReadOnlyList<PurchaseListItemDto>>(
                Error.NotFound(
                    Error.CodeNotFound,
                    ex.Message
                    ));
        }
    }
}
