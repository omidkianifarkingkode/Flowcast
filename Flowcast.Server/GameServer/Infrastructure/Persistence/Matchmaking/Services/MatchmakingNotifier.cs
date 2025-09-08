using Application.MatchMakings.Shared;
using Domain.Matchmaking;
using Domain.Sessions;
using Realtime.Transport.Messaging.Sender;
using Match = Domain.Matchmaking.Match;

namespace Infrastructure.Persistence.Matchmaking.Services;

public sealed class MatchmakingNotifier(IRealtimeMessageSender messenger) : IMatchmakingNotifier
{
    public Task MatchQueued(PlayerId player, Ticket ticket, CancellationToken ct)
        => messenger.SendToUserAsync(
            player.Value.ToString("D"),
            MatchQueuedCommand.Type,
            new MatchQueuedCommand
            {
                TicketId = ticket.Id.Value,
                Mode = ticket.Mode,
                EnqueuedAtUtc = ticket.EnqueuedAtUtc
            },
            ct);

    public Task MatchFound(PlayerId player, Match match, DateTime readyDeadlineUtc, CancellationToken ct)
        => messenger.SendToUserAsync(
            player.Value.ToString("D"),
            MatchFoundCommand.Type,
            new MatchFoundCommand
            {
                MatchId = match.Id.Value,
                Mode = match.Mode,
                Players = match.Players.Select(p => p.Value).ToArray(),
                ReadyDeadlineUnixMs = new DateTimeOffset(readyDeadlineUtc).ToUnixTimeMilliseconds()
            },
            ct);

    public Task MatchFoundFail(PlayerId player, string mode, string reasonCode, string message, bool retryable, CancellationToken ct)
        => messenger.SendToUserAsync(
            player.Value.ToString("D"),
            MatchFoundFailCommand.Type,
            new MatchFoundFailCommand
            {
                Mode = mode,
                ReasonCode = reasonCode,
                Message = message,
                Retryable = retryable
            },
            ct);


    public Task MatchConfirmed(PlayerId player, Match match, CancellationToken ct) =>
        messenger.SendToUserAsync(
            player.Value.ToString("D"),
            MatchConfirmedCommand.Type,
            new MatchConfirmedCommand
            {
                MatchId = match.Id.Value,
                Mode = match.Mode,
                Players = match.Players.Select(p => p.Value).ToArray()
            },
            ct);

    public Task MatchAborted(PlayerId player, Match match, AbortReason reason, CancellationToken ct) =>
        messenger.SendToUserAsync(
            player.Value.ToString("D"),
            MatchAbortedCommand.Type,
            new MatchAbortedCommand
            {
                MatchId = match.Id.Value,
                Reason = reason.ToString()
            },
            ct);

    public Task CancelMatchFail(PlayerId player, string mode, string reasonCode, string message, CancellationToken ct) =>
        messenger.SendToUserAsync(
            player.Value.ToString("D"),
            CancelMatchFailCommand.Type,
            new CancelMatchFailCommand
            {
                Mode = mode,
                ReasonCode = reasonCode,
                Message = message
            },
            ct);

    public Task TicketCancelled(PlayerId player, Ticket ticket, CancellationToken ct) =>
        messenger.SendToUserAsync(
            player.Value.ToString("D"),
            TicketCancelledCommand.Type,
            new TicketCancelledCommand
            {
                TicketId = ticket.Id.Value,
                Mode = ticket.Mode,
                EnqueuedAtUtc = ticket.EnqueuedAtUtc,
                State = ticket.State.ToString()
            },
            ct);

    public Task ReadyAcknowledgeFail(PlayerId player, MatchId matchId, string reasonCode, string message, CancellationToken ct) =>
        messenger.SendToUserAsync(
            player.Value.ToString("D"),
            ReadyAcknowledgeFailCommand.Type,
            new ReadyAcknowledgeFailCommand
            {
                MatchId = matchId.Value,
                ReasonCode = reasonCode,
                Message = message
            },
            ct);

    public Task ReadyAcknowledged(PlayerId player, Match match, IReadOnlySet<PlayerId> readyPlayers, DateTime? readyDeadlineUtc, CancellationToken ct) =>
        messenger.SendToUserAsync(
            player.Value.ToString("D"),
            ReadyAcknowledgedCommand.Type,
            new ReadyAcknowledgedCommand
            {
                MatchId = match.Id.Value,
                ReadyPlayers = readyPlayers.Select(p => p.Value).ToArray(),
                ReadyDeadlineUnixMs = readyDeadlineUtc.HasValue
                ? new DateTimeOffset(readyDeadlineUtc.Value).ToUnixTimeMilliseconds() : default
            },
            ct);
}
