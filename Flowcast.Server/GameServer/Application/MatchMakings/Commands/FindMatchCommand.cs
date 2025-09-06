using Application.Abstractions.Messaging;
using Application.Abstractions.Realtime;
using Application.MatchMakings.Shared;
using Domain.Matchmaking;
using Domain.Sessions;
using Microsoft.Extensions.Options;
using SharedKernel;

namespace Application.MatchMakings.Commands;

public record FindMatchCommand(PlayerId PlayerId, string Mode, string? IdempotencyKey = null)
    : ICommand<FindMatchResult>;

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
                retryable: false, corrId: null, cancellationToken);

            return Result.Failure<FindMatchResult>(MatchmakingErrors.AlreadyInSession);
        }

        // 1) Liveness gate => FAIL (usually retryable when connection gets healthy)
        if (options.Value.RequireHealthyConnection && !liveness.IsHealthy(command.PlayerId))
        {
            await notifier.MatchFoundFail(
                command.PlayerId, command.Mode,
                MatchmakingErrors.NotHealthy.Code,
                MatchmakingErrors.NotHealthy.Description,
                retryable: true, corrId: null, cancellationToken);

            return Result.Failure<FindMatchResult>(MatchmakingErrors.NotHealthy);
        }


        // 2) Idempotency: if open ticket exists, return its state
        var existing = await tickets.GetOpenByPlayer(command.PlayerId, command.Mode, cancellationToken);
        if (existing.IsSuccess)
        {
            var open = existing.Value;
            Match? reserved = null;
            DateTime? deadline = null;

            if (open.State == TicketState.PendingReady && open.MatchId is { } mid)
            {
                var matchResult = await matches.GetById(mid, cancellationToken);
                if (matchResult.IsSuccess)
                {
                    reserved = matchResult.Value;
                    deadline = reserved.ReadyDeadlineUtc;

                    await notifier.MatchFound(command.PlayerId, reserved, deadline ?? clock.UtcNow, corrId: null, cancellationToken);
                }
                else
                {
                    // stale link: treat as queued
                    await notifier.MatchQueued(command.PlayerId, open, corrId: null, cancellationToken);
                }
            }
            else
            {
                // still searching
                await notifier.MatchQueued(command.PlayerId, open, corrId: null, cancellationToken);
            }

            return new FindMatchResult(open, reserved, deadline);
        }

        // 3) Create new ticket → notify queued
        var ticket = Ticket.Create(command.PlayerId, command.Mode, clock.UtcNow);
        await tickets.Save(ticket, cancellationToken);

        await notifier.MatchQueued(command.PlayerId, ticket, corrId: null, cancellationToken);

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
            return new FindMatchResult(ticket, null, null); // enqueued only

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
        await notifier.MatchFound(ticket.PlayerId, match, readyDeadline, corrId: null, cancellationToken);
        await notifier.MatchFound(other.PlayerId, match, readyDeadline, corrId: null, cancellationToken);

        return new FindMatchResult(ticket, match, match.ReadyDeadlineUtc);
    }
}
