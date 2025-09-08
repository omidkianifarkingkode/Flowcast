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
        if (existingTicket.IsSuccess && existingTicket.Value is not null)
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

                    var winSec = Math.Max(1, options.Value.ReadyWindowSeconds);
                    deadline = reservedMatch.ReadyDeadlineUtc ?? clock.UtcNow.AddSeconds(winSec);

                    await notifier.MatchFound(command.PlayerId, reservedMatch, deadline ?? clock.UtcNow, cancellationToken);
                }
                else
                {
                    // heal stale PendingReady → mark Failed and requeue with fresh ticket
                    _ = openTicket.Fail(clock.UtcNow);
                    await tickets.Save(openTicket, cancellationToken);

                    var healed = Ticket.Create(command.PlayerId, command.Mode, clock.UtcNow);
                    await tickets.Save(healed, cancellationToken);

                    await notifier.MatchQueued(command.PlayerId, healed, cancellationToken);
                    return new FindMatchResult(healed, null, null);
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
                // ensure Searching + exclude self + (optional)peer liveness gate
                .Where(t => t.State == TicketState.Searching
                    && t.PlayerId != command.PlayerId 
                    && t.Id != ticket.Id
                    && (!options.Value.RequireHealthyConnection || liveness.IsHealthy(t.PlayerId)))
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
        if(match.BeginReadyCheck(readyWindow, clock.UtcNow) is var r1 && r1.IsFailure)
            return Result.Failure<FindMatchResult>(r1.Error);
        if (ticket.MoveToPendingReady(match.Id, clock.UtcNow) is var r2 && r2.IsFailure)
            return Result.Failure<FindMatchResult>(r2.Error);
        if (other.MoveToPendingReady(match.Id, clock.UtcNow) is var r3 && r3.IsFailure)
            return Result.Failure<FindMatchResult>(r3.Error);

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
