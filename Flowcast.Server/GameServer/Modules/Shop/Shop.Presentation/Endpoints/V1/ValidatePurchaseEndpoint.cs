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

namespace Shop.Presentation.Endpoints.V1;

public sealed class ValidatePurchaseEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(ValidatePurchase.Route,
            async (string purchaseId,
                ICommandHandler<ValidatePurchaseCommand, ValidatePurchaseResult> handler,
                HttpContext httpContext,
                CancellationToken ct) =>
            {
                var command = new ValidatePurchaseCommand(PurchaseId.FromString(purchaseId));
                var result = await handler.Handle(command, ct);

                return result.Match(
                    success => Results.Ok(ToResponse(result.Value)),
                    error => CustomResults.Problem(error, httpContext)
                );
            })
            //   .RequireAuthorization()
            .WithTags(ApiInfo.Tag)
            .WithSummary(ValidatePurchase.Summary)
            .WithDescription(ValidatePurchase.Description)
            .MapToApiVersion(1.0);
    }

    private static ValidatePurchase.Response ToResponse(ValidatePurchaseResult result)
        => new(
            PurchaseId: result.PurchaseId,
            State: result.State.ToString(),
            UpdatedAtUtc: result.UpdatedAtUtc,
            ValidationAttemptsCount: result.ValidationAttemptsCount
        );
}
