using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Application.Messaging;
using Shared.Presentation.Endpoints;
using SharedKernel;
using Shop.Application.Queries;
using Shop.Contracts;
using Shop.Contracts.V1;
using Shop.Domain.Entities;

namespace Shop.Presentation.Endpoints.V1;

public sealed class GetPurchaseByOrderIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
            GetPurchaseByOrderId.Route,
            async (
                string orderId,
                IQueryHandler<GetPurchaseByOrderIdQuery, PurchaseListItemDto> handler,
                HttpContext http,
                CancellationToken ct) =>
            {
                var query = new GetPurchaseByOrderIdQuery(OrderId.Create(orderId));
                var result = await handler.Handle(query, ct);

                return result.Match(
                    purchase => Results.Ok(ToResponse(purchase)),
                    error => CustomResults.Problem(error, http)
                );
            })
            //.RequireAuthorization()
            .WithTags(ApiInfo.Tag)
            .MapToApiVersion(1.0)
            .WithSummary(GetPurchaseByOrderId.Summary)
            .WithDescription(GetPurchaseByOrderId.Description);
    }

    private static GetPurchaseByOrderId.Response ToResponse(PurchaseListItemDto purchase)
        => new(
            Id: purchase.Id,
            OrderId: purchase.OrderId.Value,
            Store: purchase.Store.ToString(),
            ProductId: purchase.ProductId,
            UserId: purchase.UserId,
            State: purchase.State.ToString(),
            PurchaseAtUtc: purchase.PurchaseAtUtc,
            CreatedAtUtc: purchase.CreatedAtUtc,
            ValidationAttemptsCount: purchase.ValidationAttemptsCount
        );
}
