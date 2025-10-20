using Identity.Application.Commands;
using Identity.Contracts;
using Identity.Contracts.V1;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Application.Messaging;
using Shared.Presentation.Endpoints;
using SharedKernel;

namespace Identity.Presentation.Endpoints.V1;

public sealed class LogoutEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(Logout.Route,
            async (Logout.Request request,
                   ICommandHandler<LogoutCommand> handler,
                   HttpContext http,
                   CancellationToken ct) =>
            {
                var command = new LogoutCommand(request.RefreshToken);
                var result = await handler.Handle(command, ct);

                return result.Match(
                    () => Results.Ok(new Logout.Response(true)),
                    error => CustomResults.Problem(error, http));

            })
           .AllowAnonymous()
           .MapToApiVersion(1.0)
           .WithTags(ApiInfo.Tag)
           .WithSummary(Logout.Summary)
           .WithDescription(Logout.Description);
    }
}
