using Application.Abstractions.Messaging;
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
    ITicketRepository ticketRepository,
    IMatchRepository matchRepository,
    ISessionRepository sessionRepository,
    ILivenessProbe liveness,
    IDateTimeProvider clock,
    IMatchmakingNotifier notifier,
    IOptions<MatchmakingOptions> options)
    : ICommandHandler<FindMatchCommand, FindMatchResult>
{
    public async Task<Result<FindMatchResult>> Handle(FindMatchCommand command, CancellationToken cancellationToken)
    {
        // 0) Optional gate: player must not be in an active session
        var activeSession = await sessionRepository.GetActiveByPlayer(command.PlayerId, cancellationToken);
        if (activeSession.IsSuccess && activeSession.Value is not null)
            return Result.Failure<FindMatchResult>(MatchErrors.PlayerAlreadyInSession);

        // 1) Liveness gate
        if (options.Value.RequireHealthyConnection && !liveness.IsHealthy(command.PlayerId))
            return Result.Failure<FindMatchResult>(MatchErrors.PlayerNotHealthy);

        // 2) Idempotency: if open ticket exists, return its state
        var existing = await ticketRepository.GetOpenByPlayer(command.PlayerId, command.Mode, cancellationToken);
        if (existing.IsSuccess && existing.Value is { } open)
        {
            Match? reserved = null;
            DateTime? deadline = null;

            if (open.State == TicketState.PendingReady && open.MatchId is { } mid)
            {
                var mr = await matchRepository.GetById(mid, cancellationToken);
                if (mr.IsSuccess) { reserved = mr.Value; deadline = reserved.ReadyDeadlineUtc; }
            }
            return new FindMatchResult(open, reserved, deadline);
        }

        // 3) Create new ticket
        var ticket = Ticket.Create(command.PlayerId, command.Mode, clock.UtcNow);
        await ticketRepository.Save(ticket, cancellationToken);

        // 4) Try FIFO pair within same transaction scope (single node simple case)
        //    Strategy: find oldest other Searching ticket in same mode
        var queue = await ticketRepository.GetSearchingByMode(command.Mode, cancellationToken);
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

        await matchRepository.Save(match, cancellationToken);
        await ticketRepository.Save(ticket, cancellationToken);
        await ticketRepository.Save(other, cancellationToken);

        // 6) Notify both players
        var deadline = match.ReadyDeadlineUtc ?? clock.UtcNow.AddSeconds(options.Value.ReadyWindowSeconds);
        await notifier.MatchFound(ticket.PlayerId, match, deadline, cancellationToken);
        await notifier.MatchFound(other.PlayerId, match, deadline, cancellationToken);

        return new FindMatchResult(ticket, match, match.ReadyDeadlineUtc);
    }
}
