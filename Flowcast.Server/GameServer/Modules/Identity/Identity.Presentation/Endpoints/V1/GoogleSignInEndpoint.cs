using Identity.Application.Commands;
using Identity.Contracts;
using Identity.Contracts.V1;
using Identity.Contracts.V1.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Application.Messaging;
using Shared.Presentation.ApiGuard;
using Shared.Presentation.Endpoints;
using SharedKernel;

namespace Identity.Presentation.Endpoints.V1;

public sealed class GoogleSignInEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(GoogleSignIn.Route,
            async (GoogleSignIn.Request request,
                   ICommandHandler<LoginByGoogleIdCommand, AuthResult> handler,
                   HttpContext http,
                   CancellationToken ct) =>
            {
                var command = new LoginByGoogleIdCommand(request.UserId, MetadataItem.ToDictionary(request.Metadata));
                var result = await handler.Handle(command, ct);

                return result.Match(
                    auth => Results.Ok(ToResponse(auth)),
                    error => CustomResults.Problem(error, http));
            })
           .AllowAnonymous()
          // .AllowOnlyProduction()
           .MapToApiVersion(1.0)
           .WithTags(ApiInfo.Tag)
           .WithSummary(GoogleSignIn.Summary)
           .WithDescription(GoogleSignIn.Description);
    }

    private static GoogleSignIn.Response ToResponse(AuthResult auth) =>
        new(auth.AccountId, auth.AccessToken, auth.RefreshToken, new DateTimeOffset(auth.ExpiresAtUtc));
}
