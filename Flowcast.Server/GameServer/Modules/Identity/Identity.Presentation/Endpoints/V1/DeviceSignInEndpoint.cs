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

public sealed class DeviceSignInEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(DeviceSignIn.Route,
            async (DeviceSignIn.Request request,
                   ICommandHandler<DeviceSignInCommand, AuthResult> handler,
                   HttpContext ctx,
                   CancellationToken ct) =>
            {
                var command = ToCommand(request);
                var result = await handler.Handle(command, ct);

                return result.Match(
                    auth => Results.Ok(ToResponse(result.Value)),
                    error => CustomResults.Problem(error, ctx));

            })
           .AllowAnonymous()
           .MapToApiVersion(1.0)
           .WithTags(ApiInfo.Tag)
           .WithSummary(DeviceSignIn.Summary)
           .WithDescription(DeviceSignIn.Description);
    }

    private static DeviceSignInCommand ToCommand(DeviceSignIn.Request request)
    {
        var command = new DeviceSignInCommand(request.DeviceId, request.Meta);

        return command;
    }

    private static DeviceSignIn.Response ToResponse(AuthResult auth)
    {
        return new DeviceSignIn.Response(auth.AccountId, auth.AccessToken, auth.RefreshToken, auth.ExpiresAtUtc);
    }
}
