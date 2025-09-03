using Application.Abstractions.Messaging;
using Application.Sessions.Queries;
using Contracts.V1.Sessions;
using Domain.Sessions;
using Microsoft.AspNetCore.Mvc;
using Presentation.Infrastructure;
using SharedKernel;

namespace Presentation.Endpoints.Sessions;

internal sealed class GetForPlayerEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(GetByPlayer.Route,
            async ([FromRoute] Guid playerId, IQueryHandler<GetSessionByPlayerQuery, Session> handler, CancellationToken ct) =>
        {
            var query = ToQuery(playerId);

            var result = await handler.Handle(query, ct);

            return result.Match(
                session => Results.Ok(ToResponse(session)),
                CustomResults.Problem
            );
        })
        .WithTags(Tags.Sessions)
        .MapToApiVersion(1.0);
    }

    // Mapping Section

    public static GetSessionByPlayerQuery ToQuery(Guid playerId)
        => new(new PlayerId(playerId));

    public static GetByPlayer.Response ToResponse(Session session)
        => new(
            SessionId: session.Id.Value,
            Mode: session.Mode?.ToString() ?? string.Empty,
            Status: session.Status.ToString(),
            PlayerCount: session.Participants.Count,
            CreatedAtUtc: session.CreatedAtUtc
        );
}
