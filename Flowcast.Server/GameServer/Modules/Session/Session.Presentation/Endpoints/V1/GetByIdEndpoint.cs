using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Session.Application.Queries;
using Session.Domain;
using Shared.API.Endpoints;
using Shared.Application.Messaging;
using SharedKernel;

namespace Session.Presentation.Endpoints.V1;

public sealed class GetByIdEndpoint : IEndpoint
{
    public const string Method = "GET";
    public const string Route = "sessions/{sessionId}";

    public record Response(
        Guid SessionId,
        string Mode,
        string Status,                  // Waiting|InProgress|Aborted|Ended
        string StartBarrier,            // ConnectedOnly|ConnectedAndLoaded|Timer
        DateTime CreatedAtUtc,
        DateTime? StartedAtUtc,
        DateTime? EndedAtUtc,
        DateTime? JoinDeadlineUtc,
        string? CloseReason,            // Completed|NoShow|ServerFailure|AdminTerminate|PlayerAbandon
        List<Response.ParticipantInfo> Participants
    )
    {
        public record ParticipantInfo(Guid Id, string DisplayName, string Status); // Invited|Connected|Loaded|Disconnected
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(Route,
            async ([FromRoute] Guid sessionId, IQueryHandler<GetSessionByIdQuery, SessionEntity> handler,
                   HttpContext httpContext, CancellationToken ct) =>
        {
            var query = ToQuery(sessionId);

            var result = await handler.Handle(query, ct);

            return result.Match(
                session => Results.Ok(ToResponse(session)),
                error => CustomResults.Problem(error, httpContext)
            );
        })
        .RequireAuthorization() // adjust policy if needed
        .WithTags(Consts.Tag)
        .WithName("Sessions_GetById")
        .WithSummary("Get a session by id")
        .WithDescription("Returns the session and its participants.")
        .MapToApiVersion(1.0);
    }

    // Mapping Section

    public static GetSessionByIdQuery ToQuery(Guid sessionId)
        => new(new SessionId(sessionId));

    public static Response ToResponse(SessionEntity session)
    {
        var participants = session.Participants
            .Select(p => new Response.ParticipantInfo(
                Id: p.Id.Value,
                DisplayName: p.DisplayName,
                Status: p.Status.ToString()))
            .ToList();

        return new Response(
            SessionId: session.Id.Value,
            Mode: session.Mode?.ToString() ?? string.Empty,
            Status: session.Status.ToString(),
            StartBarrier: session.Barrier.ToString() ?? string.Empty,
            CreatedAtUtc: session.CreatedAtUtc,
            StartedAtUtc: session.StartedAtUtc,
            EndedAtUtc: session.EndedAtUtc,
            JoinDeadlineUtc: session.JoinDeadlineUtc,
            CloseReason: session.CloseReason?.ToString() ?? string.Empty,
            Participants: participants
        );
    }
}
