using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Text.Json;
using PlayerProgressStore.Application.Commands;
using PlayerProgressStore.Contracts;
using PlayerProgressStore.Contracts.V1;
using PlayerProgressStore.Contracts.V1.Shared;
using PlayerProgressStore.Domain;
using Shared.Application.Authentication;
using Shared.Application.Messaging;
using Shared.Presentation.Endpoints;
using SharedKernel;

namespace PlayerProgressStore.Presentation.Endpoints.V1;

public sealed class SaveProfileEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(SaveProfile.Route,
            async ([FromBody] SaveProfile.Request request, [FromServices] ICommandHandler<SaveProfileCommand, PlayerNamespace[]> handler,
                   [FromServices] IUserContext userContext, HttpContext httpContext, CancellationToken ct) =>
            {
                var command = ToCommand(request, userContext.UserId);

                var result = await handler.Handle(command, ct);

                return result.Match(
                id => Results.Ok(ToResponse(result.Value)),
                error => CustomResults.Problem(error, httpContext)
            );
            })
        .RequireAuthorization()
        .WithTags(ApiInfo.Tag)
        .WithSummary(SaveProfile.Summary)
        .WithDescription(SaveProfile.Description)
        .MapToApiVersion(1.0);
    }


    private static SaveProfileCommand ToCommand(SaveProfile.Request request, string userId)
    {
        var dto = request.Namespaces
            .Select(x => new NamespaceWriteDto(
                x.Namespace,
                x.Document,
                x.Progress,
                x.ClientVersion,
                x.ClientHash))
            .ToArray();

        var command = new SaveProfileCommand(userId, dto);

        return command;
    }

    private static SaveProfile.Response ToResponse(PlayerNamespace[] playerNamespaces)
    {
        var docs = playerNamespaces
            .Select(x => new NamespaceDocument(
                x.Namespace,
                ParseDocument(x.Document),
                x.Version.Value,
                x.Progress.Value,
                x.Hash.Value,
                x.UpdatedAtUtc))
            .ToArray();

        return new SaveProfile.Response(docs);
    }

    private static JsonElement ParseDocument(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            using var empty = JsonDocument.Parse("{}");
            return empty.RootElement.Clone();
        }

        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }

}
