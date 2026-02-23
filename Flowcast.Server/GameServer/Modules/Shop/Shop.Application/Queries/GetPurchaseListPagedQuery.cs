using Shared.Application.Messaging;
using SharedKernel;
using SharedKernel.Pagination;
using Shop.Application.Repositories;

namespace Shop.Application.Queries;

public sealed record GetPurchaseListPagedQuery(PaginationFilter Filter) : IQuery<PagedResult<PurchaseListItemDto>>;

public sealed class GetPurchaseListPagedQueryHandler(IPurchaseRepository purchases) 
    : IQueryHandler<GetPurchaseListPagedQuery, PagedResult<PurchaseListItemDto>>
{
    public async Task<Result<PagedResult<PurchaseListItemDto>>> Handle(GetPurchaseListPagedQuery query, CancellationToken ct)
    {
        var paged = await purchases.GetAllPurchasesPaged(query.Filter, asTracking: false, includeAttempts: true, ct: ct);

        var items = paged.Items
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

        var result = new PagedResult<PurchaseListItemDto>(
            items,
            paged.TotalCount,
            paged.CurrentPage,
            paged.PageSize
        );

        return Result.Success(result);
    }
}