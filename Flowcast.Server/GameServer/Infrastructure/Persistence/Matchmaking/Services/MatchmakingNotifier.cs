using Application.MatchMakings.Shared;
using Domain.Matchmaking;
using Domain.Sessions;
using Realtime.Transport.Messaging.Sender;
using Match = Domain.Matchmaking.Match;

namespace Infrastructure.Persistence.Matchmaking.Services;

public sealed class MatchmakingNotifier(IRealtimeMessageSender messenger) : IMatchmakingNotifier
{
    public Task MatchQueued(PlayerId player, Ticket ticket, string? corrId, CancellationToken ct)
        => messenger.SendToUserAsync(
            player.Value.ToString("D"),
            MatchQueuedCmd.Type,
            new MatchQueuedCmd
            {
                TicketId = ticket.Id.Value,
                Mode = ticket.Mode,
                EnqueuedAtUtc = ticket.EnqueuedAtUtc,
                CorrId = corrId
            },
            ct);

    public Task MatchFound(PlayerId player, Match match, DateTime readyDeadlineUtc, string? corrId, CancellationToken ct)
        => messenger.SendToUserAsync(
            player.Value.ToString("D"),
            MatchFoundCmd.Type,
            new MatchFoundCmd
            {
                MatchId = match.Id.Value,
                Mode = match.Mode,
                Players = match.Players.Select(p => p.Value).ToArray(),
                ReadyDeadlineUnixMs = new DateTimeOffset(readyDeadlineUtc).ToUnixTimeMilliseconds()
            },
            ct);

    public Task MatchFoundFail(PlayerId player, string mode, string reasonCode, string message, bool retryable, string? corrId, CancellationToken ct)
        => messenger.SendToUserAsync(
            player.Value.ToString("D"),
            MatchFoundFailCmd.Type,
            new MatchFoundFailCmd
            {
                Mode = mode,
                ReasonCode = reasonCode,
                Message = message,
                Retryable = retryable,
                CorrId = corrId
            },
            ct);

    // keep your existing ones
    public Task MatchConfirmed(PlayerId player, Match match, CancellationToken ct) =>
        messenger.SendToUserAsync(
            player.Value.ToString("D"),
            MatchConfirmedCmd.Type,
            new MatchConfirmedCmd
            {
                MatchId = match.Id.Value,
                Mode = match.Mode,
                Players = match.Players.Select(p => p.Value).ToArray()
            },
            ct);

    public Task MatchAborted(PlayerId player, Match match, string reason, CancellationToken ct) =>
        messenger.SendToUserAsync(
            player.Value.ToString("D"),
            MatchAbortedCmd.Type,
            new MatchAbortedCmd
            {
                MatchId = match.Id.Value,
                Reason = reason
            },
            ct);
}
