using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Session.Application.Commands;
using Session.Domain;
using Shared.API.Endpoints;
using Shared.Application.Messaging;
using SharedKernel;
using SharedKernel.Primitives;

namespace Session.Presentation.Endpoints.V1;

public sealed class MarkParticipantLoadedEndpoint : IEndpoint
{
    public const string Method = "PUT";
    public const string Route = "/sessions/{sessionId:guid}/loaded";

    public record Request(Guid PlayerId);

    public record Response(Guid SessionId, Response.ParticipantInfo Participant, string SessionStatus, DateTime? StartedAtUtc)
    {
        public record ParticipantInfo(Guid Id, string DisplayName, string Status);
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(Route,
            async ([FromRoute] Guid sessionId, Request request,
                   ICommandHandler<MarkParticipantLoadedCommand, SessionEntity> handler,
                   HttpContext httpContext, CancellationToken ct) =>
            {
                var command = ToCommand(sessionId, request);

                var result = await handler.Handle(command, ct);

                return result.Match(
                    session => Results.Ok(ToResponse(session, request.PlayerId)),
                    error => CustomResults.Problem(error, httpContext)
                );
            })
        .RequireAuthorization()
        .WithTags(Consts.Tag)
        .WithSummary("Mark participant as loaded")
        .WithDescription("Marks the participant in the session as loaded. Idempotent.")
        .MapToApiVersion(1.0);
    }

    // Mapping Section

    public static MarkParticipantLoadedCommand ToCommand(Guid sessionId, Request request)
        => new(new SessionId(sessionId), new PlayerId(request.PlayerId));

    public static Response ToResponse(SessionEntity session, Guid playerId)
    {
        var p = session.Participants.First(x => x.Id.Value == playerId);
        return new Response(
            SessionId: session.Id.Value,
            Participant: new Response.ParticipantInfo(
                               Id: p.Id.Value,
                               DisplayName: p.DisplayName,
                               Status: p.Status.ToString()),
            SessionStatus: session.Status.ToString(),
            StartedAtUtc: session.StartedAtUtc
        );
    }
}
