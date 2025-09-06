using Domain.Matchmaking;
using Domain.Sessions;

namespace Application.MatchMakings.Shared;

/// Outbound notifier for realtime pushes (keeps transport out of handlers)
public interface IMatchmakingNotifier
{
    // FindMatch outcomes
    Task MatchQueued(PlayerId player, Ticket ticket, string? corrId, CancellationToken ct);
    Task MatchFound(PlayerId player, Match match, DateTime readyDeadlineUtc, string? corrId, CancellationToken ct);
    Task MatchFoundFail(PlayerId player, string mode, string reasonCode, string message, bool retryable, string? corrId, CancellationToken ct);

    Task MatchAborted(PlayerId player, Match match, string reason, CancellationToken ct);
    Task MatchConfirmed(PlayerId player, Match match, CancellationToken ct);
}
