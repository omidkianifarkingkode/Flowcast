using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Application.Messaging;
using Shared.Presentation.Endpoints;
using Shop.Application.Features.Commands;
using Shop.Contract.V1;
using SharedKernel;
using Shop.Domain.Entities;

namespace Shop.Presentation.Endpoints.V1
{
    public sealed class CreatePurchaseEndpoint : IEndpoint
    {
        public const string Method = CreatePurchase.Method;
        public const string Route = "/" + CreatePurchase.Route;

        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost(Route,
                async (
                    CreatePurchase.Request request,
                    ICommandHandler<CreatePurchaseCommand, CreatePurchaseResult> handler,
                    HttpContext httpContext,
                    CancellationToken ct
                ) =>
                {
                    var command = ToCommand(request);

                    var result = await handler.Handle(command, ct);

                    return result.Match(
                        success =>
                        {
                            var (purchaseId, isNew) = success;

                            Console.WriteLine($"PurchaseId.Value = '{purchaseId.Value}'");

                            return isNew
                            ? Results.Created(
                            $"{Route}/{purchaseId}",
                            new CreatePurchase.Response(purchaseId.Value)
                            )
                            : Results.Ok(
                            new CreatePurchase.Response(purchaseId.Value)
                            );
                        },
                        error => CustomResults.Problem(error, httpContext)
                        );
                })
                //.RequireAuthorization()
                .WithTags("Shop")
                .WithSummary(CreatePurchase.Summary)
                .WithDescription(CreatePurchase.Description)
                .MapToApiVersion(1.0);
        }

        private static CreatePurchaseCommand ToCommand(CreatePurchase.Request request)
        => new(
            OrderId: OrderId.Create(request.OrderId),
            Store: Store.Create(request.Store),
            PurchaseToken: PurchaseToken.Create(request.PurchaseToken),
            Signature: request.Signature is null
                ? null
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
