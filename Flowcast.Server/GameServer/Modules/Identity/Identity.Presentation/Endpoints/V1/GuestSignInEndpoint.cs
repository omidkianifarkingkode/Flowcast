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

public sealed class GuestSignInEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(GuestSignIn.Route,
            async (GuestSignIn.Request request,
                   ICommandHandler<GuestSignInCommand, AuthResult> handler,
                   HttpContext ctx,
                   CancellationToken ct) =>
            {
                var command = new GuestSignInCommand(MetadataItem.ToDictionary(request.Metadata));
                var result = await handler.Handle(command, ct);

                return result.Match(
                    auth => Results.Ok(ToResponse(auth)),
                    error => CustomResults.Problem(error, ctx));
            })
            .AllowAnonymous()
            .MapToApiVersion(1.0)
            .WithTags(ApiInfo.Tag)
            .WithSummary(GuestSignIn.Summary)
            .WithDescription(GuestSignIn.Description);
    }

    private static GuestSignIn.Response ToResponse(AuthResult auth) =>
        new(auth.AccountId, auth.AccessToken, auth.RefreshToken, new DateTimeOffset(auth.ExpiresAtUtc));
}
