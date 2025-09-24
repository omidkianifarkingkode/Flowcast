using Matchmaking.Contracts;
using Matchmaking.Domain;
using MatchMaking.Application.Shared;
using MessagePack;
using Microsoft.Extensions.Logging;
using Realtime.Transport.Messaging;
using Shared.Application.Messaging;
using Shared.Application.Services;
using SharedKernel;
using SharedKernel.Primitives;

namespace MatchMaking.Application.Commands;

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
    IMatchmakingNotifier notifier,
    ILogger<CancelMatchHandler> logger)
    : ICommandHandler<CancelMatchCommand, CancelMatchResult>
{
    public async Task<Result<CancelMatchResult>> Handle(CancelMatchCommand command, CancellationToken cancellationToken)
    {
        // 0) Fetch the open ticket (Searching | PendingReady)
        var openTicketResult = await tickets.GetOpenByPlayer(command.PlayerId, command.Mode, cancellationToken);
        if (openTicketResult.IsFailure || openTicketResult.Value is null)
        {
            await notifier.CancelMatchFail(command.PlayerId, command.Mode,
                reasonCode: TicketErrors.NotFound.Code,
                message: TicketErrors.NotFound.Description,
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
        if (matchResult.IsFailure)
        {
            logger.LogWarning("Failed to fetch match with ID {MatchId}. Error: {ErrorCode} - {ErrorDescription}",
                matchId, matchResult.Error.Code, matchResult.Error.Description);
            return;
        }

        var match = matchResult.Value;

        // Abort (idempotent in domain)
        match.Abort(AbortReason.PeerCancel, clock.UtcNow);
        await matches.Save(match, cancellationToken);

        // Notify the peer (the requester will get TicketCancelled separately)
        var peerId = match.Players.FirstOrDefault(p => p != requester);
        if (peerId.Equals(default))
        {
            logger.LogWarning("No valid peer found in match {MatchId} when attempting to abort the match.", matchId);
            return;  // If no peerId is valid, log it and return
        }

        await notifier.MatchAborted(peerId, match, AbortReason.PeerCancel, cancellationToken);
    }
}
