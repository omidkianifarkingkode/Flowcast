using Identity.Application.Commands;
using Identity.Contracts;
using Identity.Contracts.V1;
using Identity.Domain.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Application.Messaging;
using Shared.Presentation.Endpoints;
using SharedKernel;
using System.Security.Claims;

namespace Identity.Presentation.Endpoints.V1;

public sealed class UnlinkGoogleAccountEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete(UnlinkGoogleAccount.Route,
            async (ICommandHandler<UnlinkProviderCommand> handler,
                   ClaimsPrincipal user,
                   HttpContext http,
                   CancellationToken ct) =>
            {
                if(!Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var accountId))
                    return Results.Unauthorized();

                var command = new UnlinkProviderCommand(accountId, IdentityProvider.Google);
                var result = await handler.Handle(command, ct);

                return result.Match(
                    () => Results.NoContent(),
                    error => CustomResults.Problem(error, http));
            })
            .RequireAuthorization()
            .MapToApiVersion(1.0)
            .WithTags(ApiInfo.Tag)
            .WithSummary(UnlinkGoogleAccount.Summary)
            .WithDescription(UnlinkGoogleAccount.Description);
    }
}