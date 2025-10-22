using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
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

public sealed class SaveProfileBytesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(SaveProfileBytes.Route,
            async ([FromBody] SaveProfileBytes.Request request,
                   [FromServices] ICommandHandler<SaveProfileCommand, PlayerNamespace[]> handler,
                   [FromServices] IUserContext userContext,
                   HttpContext httpContext,
                   CancellationToken ct) =>
            {
                var command = ToCommand(request, userContext.UserId);

                var result = await handler.Handle(command, ct);

                return result.Match(
                    _ => Results.Ok(ToResponse(result.Value)),
                    error => CustomResults.Problem(error, httpContext)
                );
            })
        .RequireAuthorization()
        .WithTags(ApiInfo.Tag)
        .WithSummary(SaveProfileBytes.Summary)
        .WithDescription(SaveProfileBytes.Description)
        .MapToApiVersion(1.0);
    }

    private static SaveProfileCommand ToCommand(SaveProfileBytes.Request request, string userId)
    {
        var dto = request.Namespaces
            .Select(x => new NamespaceWriteDto(
                x.Namespace,
                ParseDocument(x.Document),
                x.Progress,
                x.ClientVersion,
                x.ClientHash))
            .ToArray();

        return new SaveProfileCommand(userId, dto);
    }

    private static SaveProfileBytes.Response ToResponse(PlayerNamespace[] playerNamespaces)
    {
        var docs = playerNamespaces
            .Select(x => new NamespaceBinaryDocument(
                x.Namespace,
                ToBytes(x.Document),
                x.Version.Value,
                x.Progress.Value,
                x.Hash.Value,
                x.UpdatedAtUtc))
            .ToArray();

        return new SaveProfileBytes.Response(docs);
    }

    private static JsonElement ParseDocument(byte[]? document)
    {
        if (document is null || document.Length == 0)
        {
            using var empty = JsonDocument.Parse("{}");
            return empty.RootElement.Clone();
        }

        using var doc = JsonDocument.Parse(document);
        return doc.RootElement.Clone();
    }

    private static byte[] ToBytes(string document)
    {
        if (string.IsNullOrEmpty(document))
        {
            return Array.Empty<byte>();
        }

        return Encoding.UTF8.GetBytes(document);
    }
}
