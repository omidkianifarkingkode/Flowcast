using Identity.Application.Commands;
using Identity.Contracts;
using Identity.Contracts.V1;
using Identity.Presentation.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Application.Messaging;
using Shared.Presentation.Endpoints;
using SharedKernel;

namespace Identity.Presentation.Endpoints.V1;

public sealed class LinkEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(Link.Route,
            async (Link.Request request, ICommandHandler<LinkCommand> handler, HttpContext http, CancellationToken ct) =>
            {
                var accountId = http.GetAccountId();
                if (accountId.IsFailure)
                    return CustomResults.Problem(accountId, http);

                var command = ToCommand(request, accountId.Value);
                var result = await handler.Handle(command, ct);

                return result.Match(
                    () => Results.Ok(ToResponse(accountId.Value, true)),
                    error => CustomResults.Problem(error, http));

            })
           //.AllowAnonymous() => requires auth
           .MapToApiVersion(1.0)
           .WithTags(ApiInfo.Tag)
           .WithSummary(Link.Summary)
           .WithDescription(Link.Description);
    }

    private static LinkCommand ToCommand(Link.Request request, Guid accountId)
    {
        var command = new LinkCommand(accountId, request.Provider.MapToDomain(), request.IdToken, request.DisplayName, request.Meta);

        return command;
    }

    private static Link.Response ToResponse(Guid accountId, bool isLinked)
    {
        return new Link.Response(accountId, isLinked);
    }
}
