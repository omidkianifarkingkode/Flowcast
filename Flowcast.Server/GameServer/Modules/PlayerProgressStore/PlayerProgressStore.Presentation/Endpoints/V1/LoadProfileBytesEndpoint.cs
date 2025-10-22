using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using PlayerProgressStore.Application.Queries;
using PlayerProgressStore.Contracts;
using PlayerProgressStore.Contracts.V1;
using PlayerProgressStore.Contracts.V1.Shared;
using PlayerProgressStore.Domain;
using Shared.Application.Authentication;
using Shared.Application.Messaging;
using Shared.Presentation.Endpoints;
using SharedKernel;

namespace PlayerProgressStore.Presentation.Endpoints.V1;

public sealed class LoadProfileBytesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(LoadProfileBytes.Route,
            async ([AsParameters] LoadProfileBytes.Request request,
                   [FromServices] IQueryHandler<LoadProfileQuery, PlayerNamespace[]> handler,
                   [FromServices] IUserContext userContext,
                   HttpContext httpContext,
                   CancellationToken ct) =>
        {
            var query = ToQuery(request, userContext.UserId);

            var result = await handler.Handle(query, ct);

            return result.Match(
                _ => Results.Ok(ToResponse(result.Value)),
                error => CustomResults.Problem(error, httpContext)
            );
        })
        .RequireAuthorization()
        .WithTags(ApiInfo.Tag)
        .WithSummary(LoadProfileBytes.Summary)
        .WithDescription(LoadProfileBytes.Description)
        .MapToApiVersion(1.0);
    }

    private static LoadProfileQuery ToQuery(LoadProfileBytes.Request request, string userId)
    {
        return new LoadProfileQuery(userId, request.Namespaces);
    }

    private static LoadProfileBytes.Response ToResponse(PlayerNamespace[] playerNamespaces)
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

        return new LoadProfileBytes.Response(docs);
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
