using Identity.API.Businesses.Commands;
using Identity.Contracts.V1;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SharedKernel;

namespace Identity.API.Endpoints;

internal static class RefreshEndpoint
{
    public static IEndpointRouteBuilder MapRefreshEndpoint(this IEndpointRouteBuilder routes)
    {
        routes.MapPost(Refresh.Route,
            async (Refresh.Request request, RefreshCommandHandler handler, CancellationToken ct) =>
            {
                var command = new RefreshCommand(request.RefreshToken);
                var result = await handler.Handle(command, ct);

                return result.Match(
                    auth => Results.Ok(new Refresh.Response(auth.AccountId, auth.AccessToken, auth.RefreshToken, auth.ExpiresAtUtc)),
                    CustomResults.Problem);

            })
           .AllowAnonymous()
           .MapToApiVersion(1.0)
           .WithTags(Consts.Identity)
           .WithSummary(Refresh.Summary)
           .WithDescription(Refresh.Description);

        return routes;
    }
}
