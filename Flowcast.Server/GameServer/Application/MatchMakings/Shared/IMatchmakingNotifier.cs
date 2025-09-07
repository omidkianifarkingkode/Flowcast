using Domain.Matchmaking;
using Domain.Sessions;

namespace Application.MatchMakings.Shared;

/// Outbound notifier for realtime pushes (keeps transport out of handlers)
public interface IMatchmakingNotifier
{
    Task MatchQueued(PlayerId player, Ticket ticket, CancellationToken ct);
    Task MatchFound(PlayerId player, Match match, DateTime readyDeadlineUtc, CancellationToken ct);
    Task MatchFoundFail(PlayerId player, string mode, string reasonCode, string message, bool retryable, CancellationToken ct);

    Task MatchAborted(PlayerId player, Match match, string reason, CancellationToken ct);
    Task MatchConfirmed(PlayerId player, Match match, CancellationToken ct);

    Task CancelMatchFail(PlayerId player, string mode, string reasonCode, string message, CancellationToken ct);
    Task TicketCancelled(PlayerId player, Ticket ticket, CancellationToken ct);

    Task ReadyAcknowledgeFail(PlayerId player, MatchId matchId, string reasonCode, string message, CancellationToken ct);
    Task ReadyAcknowledged(PlayerId player, Match match, IReadOnlySet<PlayerId> readyPlayers, DateTime? readyDeadlineUtc, CancellationToken ct);
}
