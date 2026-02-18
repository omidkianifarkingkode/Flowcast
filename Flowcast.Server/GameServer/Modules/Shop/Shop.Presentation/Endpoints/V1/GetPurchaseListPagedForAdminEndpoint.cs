using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Application.Messaging;
using Shared.Presentation.Endpoints;
using SharedKernel;
using SharedKernel.Pagination;
using Shop.Application.Queries;
using Shop.Contracts;
using Shop.Contracts.V1;

namespace Shop.Presentation.Endpoints.V1;

public sealed class GetPurchaseListPagedForAdminEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
            GetPurchaseListPagedForAdmin.Route,
            async (
                [AsParameters] GetPurchaseListPagedForAdmin.Request request,
                IQueryHandler<GetPurchaseListPagedQuery, PagedResult<PurchaseListItemDto>> handler,
                HttpContext http,
                CancellationToken ct) =>
            {
                var filter = new PaginationFilter
                {
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    SortBy = request.SortBy,
                    IsDescending = request.IsDescending,
                    SearchTerm = request.SearchTerm,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate
                };

                var query = new GetPurchaseListPagedQuery(filter);
                var result = await handler.Handle(query, ct);

                return result.Match(
                    paged => Results.Ok(ToResponse(paged)),
                    error => CustomResults.Problem(error, http)
                );
            })
           // .RequireAuthorization()
            .WithTags(ApiInfo.Tag)
            .WithSummary(GetPurchaseListPagedForAdmin.Summary)
            .WithDescription(GetPurchaseListPagedForAdmin.Description)
            .MapToApiVersion(1.0);
    }

    private static GetPurchaseListPagedForAdmin.Response ToResponse(PagedResult<PurchaseListItemDto> paged)
    {
        var items = paged.Items
            .Select(p => new GetPurchaseListPagedForAdmin.Item(
                Id: p.Id,
                OrderId: p.OrderId.Value,
                Store: p.Store.ToString(),
                ProductId: p.ProductId,
                UserId: p.UserId,
                State: p.State.ToString(),
                PurchaseAtUtc: p.PurchaseAtUtc,
                CreatedAtUtc: p.CreatedAtUtc,
                ValidationAttemptsCount: p.ValidationAttemptsCount
            ))
            .ToList();

        var result = new PagedResult<GetPurchaseListPagedForAdmin.Item>(
            items,
            paged.TotalCount,
            paged.CurrentPage,
            paged.PageSize
        );

        return new GetPurchaseListPagedForAdmin.Response(result);
    }
}