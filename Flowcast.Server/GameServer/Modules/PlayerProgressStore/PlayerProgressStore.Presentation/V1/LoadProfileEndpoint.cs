using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PlayerProgressStore.Application.Queries;
using PlayerProgressStore.Contracts;
using PlayerProgressStore.Contracts.V1;
using PlayerProgressStore.Contracts.V1.Shared;
using PlayerProgressStore.Domain;
using Shared.API.Endpoints;
using Shared.Application.Authentication;
using Shared.Application.Messaging;
using SharedKernel;

namespace PlayerProgressStore.Presentation.V1;

public sealed class LoadProfileEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(LoadProfile.Route,
            async (LoadProfile.Request request, IQueryHandler<LoadProfileQuery, PlayerNamespace[]> handler,
                   IUserContext userContext, HttpContext httpContext, CancellationToken ct) =>
        {
            var query = ToQuery(request, userContext.UserId);

            var result = await handler.Handle(query, ct);

            return result.Match(
                id => Results.Ok(ToResponse(result.Value)),
                error => CustomResults.Problem(error, httpContext)
            );
        })
        .RequireAuthorization()
        .WithTags(ApiInfo.Tag)
        .WithSummary(LoadProfile.Summary)
        .WithDescription(LoadProfile.Description)
        .MapToApiVersion(1.0);
    }


    private static LoadProfileQuery ToQuery(LoadProfile.Request request, string userId)
    {
        var query = new LoadProfileQuery(userId, request.Namespaces);

        return query;
    }

    private static LoadProfile.Response ToResponse(PlayerNamespace[] playerNamespaces) 
    {
        var docs = playerNamespaces
            .Select(x => new NamespaceDocument(
                x.Namespace,
                x.Document,
                x.Version.Value,
                x.Progress.Value,
                x.Hash.Value,
                x.UpdatedAtUtc))
            .ToArray();

        return new LoadProfile.Response(docs);
    }
}