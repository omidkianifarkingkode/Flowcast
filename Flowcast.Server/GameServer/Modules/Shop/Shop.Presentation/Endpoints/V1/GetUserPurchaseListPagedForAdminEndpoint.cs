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

public sealed class GetUserPurchaseListPagedForAdminEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
            GetUserPurchaseListPagedForAdmin.Route,
            async (
                [AsParameters] GetUserPurchaseListPagedForAdmin.Request request,
                IQueryHandler<GetUserPurchaseListPagedQuery, PagedResult<PurchaseListItemDto>> handler,
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

                var query = new GetUserPurchaseListPagedQuery(request.UserId, filter);
                var result = await handler.Handle(query, ct);

                return result.Match(
                    paged => Results.Ok(ToResponse(paged)),
                    error => CustomResults.Problem(error, http)
                );
            })
            //.RequireAuthorization()
            .WithTags(ApiInfo.Tag)
            .WithSummary(GetUserPurchaseListPagedForAdmin.Summary)
            .WithDescription(GetUserPurchaseListPagedForAdmin.Description)
            .MapToApiVersion(1.0);
    }

    private static GetUserPurchaseListPagedForAdmin.Response ToResponse(
        PagedResult<PurchaseListItemDto> paged)
    {
        var items = paged.Items
            .Select(p => new GetUserPurchaseListPagedForAdmin.Item(
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

        var result = new PagedResult<GetUserPurchaseListPagedForAdmin.Item>(
            items,
            paged.TotalCount,
            paged.CurrentPage,
            paged.PageSize
        );

        return new GetUserPurchaseListPagedForAdmin.Response(result);
    }
}