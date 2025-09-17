using Identity.API.Businesses.Commands;
using Identity.Contracts.V1;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SharedKernel;

namespace Identity.API.Endpoints;

internal static class DeviceSignInEndpoint
{
    public static IEndpointRouteBuilder MapDeviceSignInEndpoint(this IEndpointRouteBuilder routes)
    {
        routes.MapPost(DeviceSignIn.Route,
            async (DeviceSignIn.Request request, DeviceSignInCommandHandler handler, CancellationToken ct) =>
            {
                var command = new DeviceSignInCommand(request.DeviceId, request.Meta);
                var result = await handler.Handle(command, ct);

                return result.Match(
                    auth => Results.Ok(new DeviceSignIn.Response(auth.AccountId, auth.AccessToken, auth.RefreshToken, auth.ExpiresAtUtc)),
                    CustomResults.Problem);

            })
           .AllowAnonymous()
           .MapToApiVersion(1.0)
           .WithTags(Consts.Identity)
           .WithSummary(DeviceSignIn.Summary)
           .WithDescription(DeviceSignIn.Description);

        return routes;
    }
}
