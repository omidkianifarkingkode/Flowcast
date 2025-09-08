using Application.Abstractions.Messaging;
using Application.Sessions.Commands;
using Domain.Matchmaking;
using Domain.Sessions;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.MatchMakings.EventHandlers;

public sealed class OnMatchConfirmed_CreateSession(
    ICommandHandler<CreateSessionCommand, SessionId> createSession,   // call your handler directly
    ILogger<OnMatchConfirmed_CreateSession> logger)
    : IDomainEventHandler<MatchConfirmed>
{
    public async Task Handle(MatchConfirmed evt, CancellationToken ct)
    {
        var players = evt.Players
            .Select(pid => new CreateSessionCommand.PlayerInfo(
                pid.Value,
                $"Player-{pid.Value.ToString()[..8]}")) // TODO: swap with directory/name service later
            .ToList();

        var cmd = new CreateSessionCommand(
            players: players,
            mode: evt.Mode,
            matchSettings: MatchSettings.Default // or null to use handler default
                                                 // startBarrier: "ConnectedAndLoaded",
                                                 // joinDeadlineSeconds: 15
        );

        var res = await createSession.Handle(cmd, ct);
        if (res.IsFailure)
        {
            logger.LogError("Session creation failed for Match {MatchId}. {Code} - {Desc}",
                evt.MatchId, res.Error.Code, res.Error.Description);
        }
        else
        {
            logger.LogInformation("Session {SessionId} created for Match {MatchId}.",
                res.Value, evt.MatchId);
        }
    }
}

