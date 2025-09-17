using Identity.API.Businesses.Commands;
using Identity.Contracts.V1;
using Identity.Contracts.V1.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SharedKernel;

namespace Identity.API.Endpoints;

internal static class GoogleSignInEndpoint
{
    public static IEndpointRouteBuilder MapGoogleSignInEndpoint(this IEndpointRouteBuilder routes)
    {
        routes.MapPost(GoogleSignIn.Route,
            async (GoogleSignIn.Request request, GoogleSignInCommandHandler handler, CancellationToken ct) =>
            {
                var command = new GoogleSignInCommand(IdentityProvider.Google, request.IdToken, request.Meta);
                var result = await handler.Handle(command, ct);

                return result.Match(
                    auth => Results.Ok(new GoogleSignIn.Response(auth.AccountId, auth.AccessToken, auth.RefreshToken, auth.ExpiresAtUtc)),
                    CustomResults.Problem);

            })
           .AllowAnonymous()
           .MapToApiVersion(1.0)
           .WithTags(Consts.Identity)
           .WithSummary(GoogleSignIn.Summary)
           .WithDescription(GoogleSignIn.Description);

        return routes;
    }
}
