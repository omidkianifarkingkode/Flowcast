using Identity.API.Businesses.Commands;
using Identity.API.Extensions;
using Identity.Contracts.V1;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SharedKernel;

namespace Identity.API.Endpoints;

internal static class LinkEndpoint
{
    public static IEndpointRouteBuilder MapLinkEndpoint(this IEndpointRouteBuilder routes)
    {
        routes.MapPost(Link.Route,
            async (Link.Request request, LinkCommandHandler handler, HttpContext http, CancellationToken ct) =>
            {
                var accountId = http.GetAccountId();

                var command = new LinkCommand(accountId, request.Provider, request.IdToken, request.DisplayName, request.Meta);
                var result = await handler.Handle(command, ct);

                return result.Match(
                    auth => Results.Ok(new Link.Response(accountId, Linked: true)),
                    CustomResults.Problem);

            })
           //.AllowAnonymous() => requires auth
           .MapToApiVersion(1.0)
           .WithTags(Consts.Identity)
           .WithSummary(Link.Summary)
           .WithDescription(Link.Description);

        return routes;
    }
}
