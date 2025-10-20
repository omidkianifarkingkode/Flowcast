using Identity.Application.Commands;
using Identity.Contracts;
using Identity.Contracts.V1;
using Identity.Contracts.V1.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Application.Messaging;
using Shared.Presentation.Endpoints;
using SharedKernel;

namespace Identity.Presentation.Endpoints.V1;

public sealed class GoogleSignInEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(GoogleSignIn.Route,
            async (GoogleSignIn.Request request,
                   ICommandHandler<GoogleSignInCommand, AuthResult> handler,
                   HttpContext http,
                   CancellationToken ct) =>
            {
                var command = ToCommand(request);
                var result = await handler.Handle(command, ct);

                return result.Match(
                    auth => Results.Ok(ToResponse(result.Value)),
                    error => CustomResults.Problem(error, http));

            })
           .AllowAnonymous()
           .MapToApiVersion(1.0)
           .WithTags(ApiInfo.Tag)
           .WithSummary(GoogleSignIn.Summary)
           .WithDescription(GoogleSignIn.Description);
    }

    private static GoogleSignInCommand ToCommand(GoogleSignIn.Request request)
    {
        var command = new GoogleSignInCommand(IdentityProvider.Google.MapToDomain(), request.IdToken, request.Meta);

        return command;
    }

    private static GoogleSignIn.Response ToResponse(AuthResult auth)
    {
        return new GoogleSignIn.Response(auth.AccountId, auth.AccessToken, auth.RefreshToken, auth.ExpiresAtUtc);
    }
}
