using Application.Abstractions.Messaging;
using Application.MatchMakings.Shared;
using Domain.Matchmaking;
using Domain.Sessions;
using MessagePack;
using Realtime.Transport.Messaging;
using SharedKernel;

namespace Application.MatchMakings.Commands;

[MessagePackObject]
[RealtimeMessage(MatchmakingV1.Cancel)]
public record CancelMatchCommand : ICommand<CancelMatchResult>, IPayload
{
    [Key(0)] public PlayerId PlayerId { get; set; }
    [Key(1)] public string Mode { get; set; } = "";

    public CancelMatchCommand(PlayerId playerId, string mode)
    {
        PlayerId = playerId;
        Mode = mode;
    }
}
public sealed record CancelMatchResult(Ticket Ticket);

public sealed class CancelMatchHandler(
    ITicketRepository tickets,
    IMatchRepository matches,
    IDateTimeProvider clock,
    IMatchmakingNotifier notifier)
    : ICommandHandler<CancelMatchCommand, CancelMatchResult>
{
    public async Task<Result<CancelMatchResult>> Handle(CancelMatchCommand command, CancellationToken cancellationToken)
    {
        // 0) Fetch the open ticket (Searching | PendingReady)
        var openTicketResult = await tickets.GetOpenByPlayer(command.PlayerId, command.Mode, cancellationToken);
        if (openTicketResult.IsFailure || openTicketResult.Value is null)
        {
            await notifier.CancelMatchFail(command.PlayerId, command.Mode,
                reasonCode: "mm.ticket_not_found",
                message: "No open ticket for player/mode.",
                cancellationToken);
            return Result.Failure<CancelMatchResult>(TicketErrors.NotFound);
        }

        var ticket = openTicketResult.Value;

        // 1) If Searching → cancel ticket, notify requester
        if (ticket.State == TicketState.Searching)
        {
            _ = ticket.Cancel(clock.UtcNow);
            await tickets.Save(ticket, cancellationToken);

            await notifier.TicketCancelled(command.PlayerId, ticket, cancellationToken);
            return new CancelMatchResult(ticket);
        }

        // 2) If PendingReady → abort match (peer cancel), cancel ticket, notify both sides
        if (ticket.State == TicketState.PendingReady && ticket.MatchId is { } matchId)
        {
            await AbortMatchAndNotifyPeerAsync(command.PlayerId, matchId, cancellationToken);

            ticket.Cancel(clock.UtcNow);
            await tickets.Save(ticket, cancellationToken);

            await notifier.TicketCancelled(command.PlayerId, ticket, cancellationToken);
            return new CancelMatchResult(ticket);
        }

        // 3) Already consumed/cancelled/failed — idempotent success; still notify requester snapshot
        await notifier.TicketCancelled(command.PlayerId, ticket, cancellationToken);
        return new CancelMatchResult(ticket);
    }

    private async Task AbortMatchAndNotifyPeerAsync(PlayerId requester, MatchId matchId, CancellationToken cancellationToken)
    {
        var matchResult = await matches.GetById(matchId, cancellationToken);
        if (matchResult.IsFailure) return;

        var match = matchResult.Value;

        // Abort (idempotent in domain)
        match.Abort(AbortReason.PeerCancel, clock.UtcNow);
        await matches.Save(match, cancellationToken);

        // Notify the peer (the requester will get TicketCancelled separately)
        var peerId = match.Players.First(p => p != requester);
        await notifier.MatchAborted(peerId, match, "peer_cancel", cancellationToken);
    }
}
