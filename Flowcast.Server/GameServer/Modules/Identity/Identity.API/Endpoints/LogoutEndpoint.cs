using Identity.API.Businesses.Commands;
using Identity.Contracts.V1;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SharedKernel;
using static System.Net.WebRequestMethods;

namespace Identity.API.Endpoints;

internal static class LogoutEndpoint
{
    public static IEndpointRouteBuilder MapLogoutEndpoint(this IEndpointRouteBuilder routes)
    {
        routes.MapPost(Logout.Route,
            async (Logout.Request request, LogoutCommandHandler handler, HttpContext http, CancellationToken ct) =>
            {
                var command = new LogoutCommand(request.RefreshToken);
                var result = await handler.Handle(command, ct);

                return result.Match(
                    auth => Results.Ok(new Logout.Response(true)),
                    error => CustomResults.Problem(error, http));

            })
           .AllowAnonymous()
           .MapToApiVersion(1.0)
           .WithTags(Consts.Identity)
           .WithSummary(Logout.Summary)
           .WithDescription(Logout.Description);

        return routes;
    }
}
