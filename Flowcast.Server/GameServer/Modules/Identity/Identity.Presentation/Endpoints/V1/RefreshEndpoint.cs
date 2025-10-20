using Identity.Application.Commands;
using Identity.Contracts;
using Identity.Contracts.V1;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Application.Messaging;
using Shared.Presentation.Endpoints;
using SharedKernel;

namespace Identity.Presentation.Endpoints.V1;

public sealed class RefreshEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(Refresh.Route,
            async (Refresh.Request request,
                   ICommandHandler<RefreshCommand, AuthResult> handler,
                   HttpContext http,
                   CancellationToken ct) =>
            {
                var command = new RefreshCommand(request.RefreshToken);
                var result = await handler.Handle(command, ct);

                return result.Match(
                    auth => Results.Ok(ToResponse(result.Value)),
                    error => CustomResults.Problem(error, http));

            })
           .AllowAnonymous()
           .MapToApiVersion(1.0)
           .WithTags(ApiInfo.Tag)
           .WithSummary(Refresh.Summary)
           .WithDescription(Refresh.Description);
    }

    private static Refresh.Response ToResponse(AuthResult auth)
    {
        return new Refresh.Response(auth.AccountId, auth.AccessToken, auth.RefreshToken, auth.ExpiresAtUtc);
    }
}
