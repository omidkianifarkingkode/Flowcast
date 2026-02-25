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
using System.Security.Claims; 

namespace Identity.Presentation.Endpoints.V1;

public sealed class LinkGoogleAccountEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut(LinkGoogleAccount.Route,
            async (LinkGoogleAccount.Request request,
                   ICommandHandler<LinkGoogleAccountCommand> handler,
                   ClaimsPrincipal user,
                   HttpContext http,
                   CancellationToken ct) =>
            {
                // Extract AccountId from JWT claims (Ensure your Auth setup populates this)
                if(!Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var accountId))
                    return Results.Unauthorized();

                var command = new LinkGoogleAccountCommand(
                    accountId,
                    request.GoogleId,
                    request.DisplayName,
                    MetadataItem.ToDictionary(request.Metadata));

                var result = await handler.Handle(command, ct);

                return result.Match(
                    () => Results.Ok(),
                    error => CustomResults.Problem(error, http));
            })
            .RequireAuthorization() // Must be logged in (as Guest or other) to link
            .MapToApiVersion(1.0)
            .WithTags(ApiInfo.Tag)
            .WithSummary(LinkGoogleAccount.Summary)
            .WithDescription(LinkGoogleAccount.Description);
    }
}