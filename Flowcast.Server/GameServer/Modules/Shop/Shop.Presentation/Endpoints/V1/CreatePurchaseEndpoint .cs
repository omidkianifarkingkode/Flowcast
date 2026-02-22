using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Application.Messaging;
using Shared.Presentation.Endpoints;
using SharedKernel;
using Shop.Application.Commands;
using Shop.Contracts;
using Shop.Contracts.V1;
using Shop.Domain.Entities;
using Shop.Domain.Enums;

namespace Shop.Presentation.Endpoints.V1
{
    public sealed class CreatePurchaseEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost(CreatePurchase.Route,
                async (CreatePurchase.Request request,
                       ICommandHandler<CreatePurchaseCommand, CreatePurchaseResult> handler,
                       HttpContext httpContext,
                       CancellationToken ct) =>
                {
                    var command = ToCommand(request);
                    var result = await handler.Handle(command, ct);

                    return result.Match(
                        success =>
                        {
                            var (purchaseId, isNew) = success;

                            if (!isNew)
                                return Results.Ok(new CreatePurchase.Response(purchaseId.Value));

                            return Results.Created($"{CreatePurchase.Route}/{purchaseId}", new CreatePurchase.Response(purchaseId.Value));
                        },
                        error => CustomResults.Problem(error, httpContext)
                        );
                })
                //.RequireAuthorization()
                .MapToApiVersion(1.0)
                .WithTags(ApiInfo.Tag)
                .WithSummary(CreatePurchase.Summary)
                .WithDescription(CreatePurchase.Description);
        }

        private static CreatePurchaseCommand ToCommand(CreatePurchase.Request request)
            => new(
                OrderId: OrderId.Create(request.OrderId),
                Store: Enum.Parse<Store>(request.Store),
                PurchaseToken: PurchaseToken.Create(request.PurchaseToken),
                Signature: string.IsNullOrWhiteSpace(request.Signature)
                    ? PurchaseSignature.Create("")
                    : PurchaseSignature.Create(request.Signature),
                ProductId: request.ProductId,
                Receipt: request.Receipt,
                Payload: request.Payload,
                UserId: request.UserId,
                PurchaseAtUtc: request.PurchaseAtUtc,
                IsSandbox: request.IsSandbox,
                Metadata: request.Metadata
            );
    }
}
