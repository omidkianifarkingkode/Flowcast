using Application.Abstractions.Messaging;
using Application.Sessions.Queries;
using Contracts.V1.Sessions;
using Domain.Sessions;
using Microsoft.AspNetCore.Mvc;
using Presentation.Infrastructure;
using SharedKernel;

namespace Presentation.Endpoints.Sessions;

internal sealed class GetByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(Get.Route,
            async ([FromRoute] Guid sessionId, IQueryHandler<GetSessionByIdQuery, Session> handler, CancellationToken ct) =>
        {
            var query = ToQuery(sessionId);

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

    public static GetSessionByIdQuery ToQuery(Guid sessionId)
        => new(new SessionId(sessionId));

    public static Get.Response ToResponse(Session session)
    {
        var participants = session.Participants
            .Select(p => new Get.Response.ParticipantInfo(
                Id: p.Id.Value,
                DisplayName: p.DisplayName,
                Status: p.Status.ToString()))
            .ToList();

        return new Get.Response(
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
