using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Presentation.Endpoints;
using SharedKernel;
using ZP.Contracts;
using ZP.Core.Commands;
using Shared.Application.Messaging;

namespace ZP.Apphost.Endpoints.V1;

public sealed class RequestPaymentEndpoint : IEndpoint
{
    public const string Route = "zarinpal/payment/request";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(Route,
                async (
                    RequestPaymentRequest request,
                    ICommandHandler<RequestPaymentCommand, RequestPaymentCommandResult> handler,
                    HttpContext httpContext,
                    CancellationToken ct) =>
                {
                    var command = new RequestPaymentCommand(
                        request.UserId,
                        request.ProductId,
                        request.Amount,
                        request.Description,
                        request.Mobile,
                        request.Email,
                        request.IdempotencyKey);
                    var result = await handler.Handle(command, ct);
                    return result.Match(
                        success => success.Created
                            ? Results.Json(success.Response, statusCode: 201)
                            : Results.Ok(success.Response),
                        error => CustomResults.Problem(error, httpContext));
                })
            .WithTags("Zarinpal")
            .WithSummary("Request payment")
            .WithDescription("Creates a payment request and returns payment URL and authority.")
            .MapToApiVersion(1.0);
    }
}
