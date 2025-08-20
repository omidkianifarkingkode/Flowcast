using Domain.Matchmaking;
using Domain.Sessions;

namespace Application.MatchMakings.Shared;

/// Outbound notifier for realtime pushes (keeps transport out of handlers)
public interface IMatchmakingNotifier
{
    Task MatchFound(PlayerId player, Match match, DateTime readyDeadlineUtc, CancellationToken ct);
    Task MatchAborted(PlayerId player, Match match, string reason, CancellationToken ct);
    Task MatchConfirmed(PlayerId player, Match match, CancellationToken ct);
}

/// Liveness view (can be backed by IUserConnectionRegistry + health window policy)
public interface ILivenessProbe
{
    bool IsHealthy(PlayerId playerId); // last Pong within window etc.
}
