using Application.Abstractions.Messaging;
using Application.Sessions.Commands;
using Contracts.V1.Sessions;
using Domain.Sessions;
using Presentation.Infrastructure;
using SharedKernel;

namespace Presentation.Endpoints.Sessions;

internal sealed class LeaveEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(Leave.Route,
            async (Leave.Request request, ICommandHandler<LeaveSessionCommand, LeaveSessionResult> handler, CancellationToken ct) =>
        {
            var command = ToCommand(request);

            var result = await handler.Handle(command, ct);

            return result.Match(
                leaveResult => Results.Ok(ToResponse(leaveResult)),
                CustomResults.Problem
            );
        })
        .WithTags(Tags.Sessions)
        .MapToApiVersion(1.0);
    }

    // Mapping Section

    public static LeaveSessionCommand ToCommand(Leave.Request request)
        => new(
            new SessionId(request.SessionId),
            new PlayerId(request.PlayerId)
        );

    public static Leave.Response ToResponse(LeaveSessionResult leaveResult)
        => new(
            SessionId: leaveResult.Session.Id.Value,
            Player: new Leave.Response.PlayerInfo(
                               Id: leaveResult.Participant.Id.Value,
                               DisplayName: leaveResult.Participant.DisplayName),
            WasLastPlayer: leaveResult.WasLastPlayer
        );
}