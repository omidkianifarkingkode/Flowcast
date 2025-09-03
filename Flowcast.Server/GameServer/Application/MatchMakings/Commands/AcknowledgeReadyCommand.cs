using Application.Abstractions.Messaging;
using Application.MatchMakings.Shared;
using Domain.Matchmaking;
using Domain.Sessions;
using SharedKernel;

namespace Application.MatchMakings.Commands;

public record AcknowledgeReadyCommand(PlayerId PlayerId, MatchId MatchId) : ICommand<AcknowledgeReadyResult>;

public sealed record AcknowledgeReadyResult(
    Match Match,
    bool AllReady,
    DateTime? ReadyDeadlineUtc);

public sealed class AcknowledgeReadyHandler(
    ITicketRepository tickets,
    IMatchRepository matches,
    ISessionRepository sessions,          // may be used by a separate processor upon confirmation
    ILivenessProbe liveness,
    IDateTimeProvider clock,
    IMatchmakingNotifier notifier)
    : ICommandHandler<AcknowledgeReadyCommand, AcknowledgeReadyResult>
{
    public async Task<Result<AcknowledgeReadyResult>> Handle(AcknowledgeReadyCommand cmd, CancellationToken ct)
    {
        // Liveness gate for ready
        if (!liveness.IsHealthy(cmd.PlayerId))
            return Result.Failure<AcknowledgeReadyResult>(Error.Conflict("mm.not_healthy", "Player connection not healthy."));

        var mr = await matches.GetById(cmd.MatchId, ct);
        if (mr.IsFailure)
            return Result.Failure<AcknowledgeReadyResult>(mr.Error);

        var match = mr.Value;

        var r1 = match.AcknowledgeReady(cmd.PlayerId, clock.UtcNow);
        if (r1.IsFailure)
            return Result.Failure<AcknowledgeReadyResult>(r1.Error);

        var r2 = match.ConfirmIfAllReady(clock.UtcNow, out var confirmed);
        if (r2.IsFailure)
            return Result.Failure<AcknowledgeReadyResult>(r2.Error);

        await matches.Save(match, ct);

        if (confirmed)
        {
            // Mark both players' tickets consumed
            foreach (var pid in match.Players)
            {
                var open = await tickets.GetOpenByPlayer(pid, match.Mode, ct);
                if (open.IsSuccess && open.Value is { } t && t.State == TicketState.PendingReady)
                {
                    _ = t.Consume();
                    await tickets.Save(t, ct);
                }
            }

            // Notify both; session allocation will proceed in a separate pipeline (outbox/handler)
            foreach (var pid in match.Players)
                await notifier.MatchConfirmed(pid, match, ct);

            return new AcknowledgeReadyResult(match, AllReady: true, ReadyDeadlineUtc: match.ReadyDeadlineUtc);
        }

        // Not all ready yet — notify peer progress optionally via notifier, return state
        return new AcknowledgeReadyResult(match, AllReady: false, ReadyDeadlineUtc: match.ReadyDeadlineUtc);
    }
}
