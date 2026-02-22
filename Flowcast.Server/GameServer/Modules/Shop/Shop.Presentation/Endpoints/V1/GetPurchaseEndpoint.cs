using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Application.Messaging;
using Shared.Presentation.Endpoints;
using Shop.Contracts.V1;
using SharedKernel;
using Shop.Application.Queries;
using Shop.Contracts;

namespace Shop.Presentation.Endpoints.V1;

public sealed class GetPurchaseEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
            GetPurchaseList.Route,
            async (
                IQueryHandler<GetPurchaseListQuery, IReadOnlyList<PurchaseListItemDto>> handler,
                HttpContext http,
                CancellationToken ct) =>
            {
                var query = new GetPurchaseListQuery();

                var result = await handler.Handle(query, ct);

                return result.Match(
                    purchases => Results.Ok(ToResponse(purchases)),
                    error => CustomResults.Problem(error, http)
                );
            })
            //.RequireAuthorization()
            .WithTags(ApiInfo.Tag)
            .MapToApiVersion(1.0)
            .WithSummary(GetPurchaseList.Summary)
            .WithDescription(GetPurchaseList.Description);
    }

    private static IReadOnlyList<GetPurchaseList.Response> ToResponse(
        IReadOnlyList<PurchaseListItemDto> purchases)
    {
        return purchases
            .Select(p => new GetPurchaseList.Response(
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
    }
}
