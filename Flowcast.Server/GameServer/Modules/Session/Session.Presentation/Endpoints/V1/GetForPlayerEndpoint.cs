using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Session.Application.Queries;
using Session.Domain;
using Shared.Application.Messaging;
using Shared.Presentation.Endpoints;
using SharedKernel;
using SharedKernel.Primitives;

namespace Session.Presentation.Endpoints.V1;

public sealed class GetForPlayerEndpoint : IEndpoint
{
    public const string Method = "GET";
    public const string Route = "/sessions/players/{playerId:guid}";

    public record Request(Guid PlayerId);

    public record Response(Guid SessionId, string Mode, string Status, int PlayerCount, DateTime CreatedAtUtc);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(Route,
            async ([FromRoute] Guid playerId, IQueryHandler<GetSessionByPlayerQuery, SessionEntity> handler,
                   HttpContext httpContext, CancellationToken ct) =>
        {
            var query = ToQuery(playerId);

            var result = await handler.Handle(query, ct);

            return result.Match(
                session => Results.Ok(ToResponse(session)),
                error => CustomResults.Problem(error, httpContext)
            );
        })
        .RequireAuthorization() // adjust policy if needed
        .WithTags(Consts.Tag)
        .WithName("Sessions_GetForPlayer")
        .WithSummary("Get the active session for a player")
        .WithDescription("Returns the player's active session, if any.")
        .MapToApiVersion(1.0);
    }

    // Mapping Section

    public static GetSessionByPlayerQuery ToQuery(Guid playerId)
        => new(new PlayerId(playerId));

    public static Response ToResponse(SessionEntity session)
        => new(
            SessionId: session.Id.Value,
            Mode: session.Mode?.ToString() ?? string.Empty,
            Status: session.Status.ToString(),
            PlayerCount: session.Participants.Count,
            CreatedAtUtc: session.CreatedAtUtc
        );
}
