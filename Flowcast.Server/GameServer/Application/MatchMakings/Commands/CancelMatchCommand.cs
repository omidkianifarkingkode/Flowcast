using Application.Abstractions.Messaging;
using Application.MatchMakings.Shared;
using Domain.Matchmaking;
using Domain.Sessions;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.MatchMakings.Commands;

public record CancelMatchCommand(PlayerId PlayerId, string Mode) : ICommand<CancelMatchResult>;
public sealed record CancelMatchResult(Ticket Ticket);

public sealed class CancelMatchHandler(
    ITicketRepository tickets,
    IMatchRepository matches,
    IDateTimeProvider clock,
    IMatchmakingNotifier notifier)
    : ICommandHandler<CancelMatchCommand, CancelMatchResult>
{
    public async Task<Result<CancelMatchResult>> Handle(CancelMatchCommand cmd, CancellationToken ct)
    {
        var open = await tickets.GetOpenByPlayer(cmd.PlayerId, cmd.Mode, ct);
        if (open.IsFailure || open.Value is null)
            return Result.Failure<CancelMatchResult>(Error.NotFound("mm.ticket_not_found", "No open ticket for player/mode."));

        var ticket = open.Value;

        if (ticket.State == TicketState.Searching)
        {
            _ = ticket.Cancel(clock.UtcNow);
            await tickets.Save(ticket, ct);
            return new CancelMatchResult(ticket);
        }

        if (ticket.State == TicketState.PendingReady && ticket.MatchId is { } mid)
        {
            // Abort match and requeue peer
            var mr = await matches.GetById(mid, ct);
            if (mr.IsSuccess)
            {
                var match = mr.Value;
                match.Abort(AbortReason.PeerCancel, clock.UtcNow);
                await matches.Save(match, ct);

                var peerId = match.Players.First(p => p != cmd.PlayerId);
                await notifier.MatchAborted(peerId, match, "peer_cancel", ct);
            }

            _ = ticket.Cancel(clock.UtcNow);
            await tickets.Save(ticket, ct);
            return new CancelMatchResult(ticket);
        }

        // Already consumed/cancelled/failed — idempotent
        return new CancelMatchResult(ticket);
    }
}
