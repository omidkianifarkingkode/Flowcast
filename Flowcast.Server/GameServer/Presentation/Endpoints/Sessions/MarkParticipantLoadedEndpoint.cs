using Application.Sessions.Commands;
using Contracts.V1.Sessions;
using Domain.Sessions;
using Presentation.Infrastructure;
using SharedKernel;
using SharedKernel.Messaging;

namespace Presentation.Endpoints.Sessions;

internal sealed class MarkParticipantLoadedEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(Loaded.Route,
            async (Loaded.Request request,
                   ICommandHandler<MarkParticipantLoadedCommand, Session> handler,
                   CancellationToken ct) =>
            {
                var command = ToCommand(request);

                var result = await handler.Handle(command, ct);

                return result.Match(
                    session => Results.Ok(ToResponse(session, request.PlayerId)),
                    CustomResults.Problem
                );
            })
        .WithTags(Tags.Sessions)
        .MapToApiVersion(1.0);
    }

    // Mapping Section

    public static MarkParticipantLoadedCommand ToCommand(Loaded.Request request)
        => new(new SessionId(request.SessionId), new PlayerId(request.PlayerId));

    public static Loaded.Response ToResponse(Session session, Guid playerId)
    {
        var p = session.Participants.First(x => x.Id.Value == playerId);
        return new Loaded.Response(
            SessionId: session.Id.Value,
            Participant: new Loaded.Response.ParticipantInfo(
                               Id: p.Id.Value,
                               DisplayName: p.DisplayName,
                               Status: p.Status.ToString()),
            SessionStatus: session.Status.ToString(),
            StartedAtUtc: session.StartedAtUtc
        );
    }
}
