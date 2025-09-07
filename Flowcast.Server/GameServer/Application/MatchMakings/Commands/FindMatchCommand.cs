using Application.Abstractions.Messaging;
using Application.Abstractions.Realtime;
using Application.MatchMakings.Shared;
using Domain.Matchmaking;
using Domain.Sessions;
using MessagePack;
using Microsoft.Extensions.Options;
using Realtime.Transport.Messaging;
using SharedKernel;

namespace Application.MatchMakings.Commands;

[MessagePackObject]
[RealtimeMessage(MatchmakingV1.FindMatch)]
public record FindMatchCommand : ICommand<FindMatchResult>, IPayload
{
    [Key(0)] public PlayerId PlayerId { get; set; }
    [Key(1)] public string Mode { get; set; } = "";

    public FindMatchCommand(PlayerId playerId, string mode)
    {
        PlayerId = playerId;
        Mode = mode;
    }
}

public sealed record FindMatchResult(
    Ticket Ticket,
    Match? Match,
    DateTime? ReadyDeadlineUtc);

public sealed class FindMatchHandler(
    ITicketRepository tickets,
    IMatchRepository matches,
    ISessionRepository sessions,
    ILivenessProbe liveness,
    IDateTimeProvider clock,
    IMatchmakingNotifier notifier,
    IOptions<MatchmakingOptions> options)
    : ICommandHandler<FindMatchCommand, FindMatchResult>
{
    public async Task<Result<FindMatchResult>> Handle(FindMatchCommand command, CancellationToken cancellationToken)
    {
        // 0) Already in active session => FAIL
        var activeSession = await sessions.GetActiveByPlayer(command.PlayerId, cancellationToken);

        if (activeSession.IsSuccess && activeSession.Value is not null)
        {
            await notifier.MatchFoundFail(
                command.PlayerId, command.Mode,
                MatchmakingErrors.AlreadyInSession.Code,
                MatchmakingErrors.AlreadyInSession.Description,
                retryable: false, cancellationToken);

            return Result.Failure<FindMatchResult>(MatchmakingErrors.AlreadyInSession);
        }

        // 1) Liveness gate => FAIL (usually retryable when connection gets healthy)
        if (options.Value.RequireHealthyConnection && !liveness.IsHealthy(command.PlayerId))
        {
            await notifier.MatchFoundFail(
                command.PlayerId, command.Mode,
                MatchmakingErrors.NotHealthy.Code,
                MatchmakingErrors.NotHealthy.Description,
                retryable: true, cancellationToken);

            return Result.Failure<FindMatchResult>(MatchmakingErrors.NotHealthy);
        }


        // 2) Idempotency: if open ticket exists, return its state
        var existingTicket = await tickets.GetOpenByPlayer(command.PlayerId, command.Mode, cancellationToken);
        if (existingTicket.IsSuccess)
        {
            var openTicket = existingTicket.Value;
            Match? reservedMatch = null;
            DateTime? deadline = null;

            if (openTicket.State == TicketState.PendingReady && openTicket.MatchId is { } matchId)
            {
                var matchResult = await matches.GetById(matchId, cancellationToken);
                if (matchResult.IsSuccess)
                {
                    reservedMatch = matchResult.Value;
                    deadline = reservedMatch.ReadyDeadlineUtc;

                    await notifier.MatchFound(command.PlayerId, reservedMatch, deadline ?? clock.UtcNow, cancellationToken);
                }
                else
                {
                    // stale link: treat as queued
                    await notifier.MatchQueued(command.PlayerId, openTicket, cancellationToken);
                }
            }
            else
            {
                // still searching
                await notifier.MatchQueued(command.PlayerId, openTicket, cancellationToken);
            }

            return new FindMatchResult(openTicket, reservedMatch, deadline);
        }

        // 3) Create new ticket → notify queued
        var ticket = Ticket.Create(command.PlayerId, command.Mode, clock.UtcNow);
        await tickets.Save(ticket, cancellationToken);


        // 4) Try FIFO pair within same transaction scope (single node simple case)
        //    Strategy: find oldest other Searching ticket in same mode
        var queue = await tickets.GetSearchingByMode(command.Mode, cancellationToken);
        var other = queue.IsSuccess
            ? queue.Value
                .Where(t => t.PlayerId != command.PlayerId && t.Id != ticket.Id)
                .OrderBy(t => t.EnqueuedAtUtc)
                .FirstOrDefault()
            : null;

        if (other is null)
        {
            await notifier.MatchQueued(command.PlayerId, ticket, cancellationToken);

            return new FindMatchResult(ticket, null, null); // enqueued only
        }

        // 5) Pair: create Match, move both tickets to PendingReady, begin ready window
        var match = Match.Create(command.Mode, ticket.PlayerId, other.PlayerId, clock.UtcNow);
        var readyWindow = new ReadyWindow(TimeSpan.FromSeconds(options.Value.ReadyWindowSeconds));
        _ = match.BeginReadyCheck(readyWindow, clock.UtcNow);
        _ = ticket.MoveToPendingReady(match.Id, clock.UtcNow);
        _ = other.MoveToPendingReady(match.Id, clock.UtcNow);

        await matches.Save(match, cancellationToken);
        await tickets.Save(ticket, cancellationToken);
        await tickets.Save(other, cancellationToken);

        // 6) Notify both players
        var readyDeadline = match.ReadyDeadlineUtc ?? clock.UtcNow.AddSeconds(options.Value.ReadyWindowSeconds);
        await notifier.MatchFound(ticket.PlayerId, match, readyDeadline, cancellationToken);
        await notifier.MatchFound(other.PlayerId, match, readyDeadline, cancellationToken);

        return new FindMatchResult(ticket, match, match.ReadyDeadlineUtc);
    }
}
