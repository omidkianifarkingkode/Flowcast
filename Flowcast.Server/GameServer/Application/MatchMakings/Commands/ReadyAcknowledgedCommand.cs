using Application.Abstractions.Messaging;
using Application.Abstractions.Realtime;
using Application.MatchMakings.Shared;
using Domain.Matchmaking;
using Domain.Sessions;
using MessagePack;
using Realtime.Transport.Messaging;
using SharedKernel;

namespace Application.MatchMakings.Commands;

[MessagePackObject]
[RealtimeMessage(MatchmakingV1.Ready)]
public record AcknowledgeReadyCommand : ICommand<AcknowledgeReadyResult>, IPayload
{
    [Key(0)] public PlayerId PlayerId { get; set; }
    [Key(1)] public MatchId MatchId { get; set; }

    public AcknowledgeReadyCommand(PlayerId playerId, MatchId matchId)
    {
        PlayerId = playerId;
        MatchId = matchId;
    }
}

public sealed record AcknowledgeReadyResult(
    Match Match,
    bool AllReady,
    DateTime? ReadyDeadlineUtc);

public sealed class AcknowledgeReadyHandler(
    ITicketRepository tickets,
    IMatchRepository matches,
    ILivenessProbe liveness,
    IDateTimeProvider clock,
    IMatchmakingNotifier notifier)
    : ICommandHandler<AcknowledgeReadyCommand, AcknowledgeReadyResult>
{
    public async Task<Result<AcknowledgeReadyResult>> Handle(AcknowledgeReadyCommand cmd, CancellationToken ct)
    {
        // 0) Liveness gate — for READY this should be strict
        if (!liveness.IsHealthy(cmd.PlayerId))
        {
            await NotifyReadyFailAsync(cmd.PlayerId, cmd.MatchId, "mm.not_healthy", "Player connection not healthy.", ct);
            return Result.Failure<AcknowledgeReadyResult>(MatchmakingErrors.NotHealthy);
        }

        // 1) Load match
        var mr = await matches.GetById(cmd.MatchId, ct);
        if (mr.IsFailure)
        {
            await NotifyReadyFailAsync(cmd.PlayerId, cmd.MatchId, mr.Error.Code, mr.Error.Description, ct);
            return Result.Failure<AcknowledgeReadyResult>(mr.Error);
        }

        var match = mr.Value;

        // 2) Acknowledge
        var r1 = match.AcknowledgeReady(cmd.PlayerId, clock.UtcNow);
        if (r1.IsFailure)
        {
            await NotifyReadyFailAsync(cmd.PlayerId, cmd.MatchId, r1.Error.Code, r1.Error.Description, ct);
            return Result.Failure<AcknowledgeReadyResult>(r1.Error);
        }

        // 3) Try confirm if all ready
        var r2 = match.ConfirmIfAllReady(clock.UtcNow, out var confirmed);
        if (r2.IsFailure)
        {
            await NotifyReadyFailAsync(cmd.PlayerId, cmd.MatchId, r2.Error.Code, r2.Error.Description, ct);
            return Result.Failure<AcknowledgeReadyResult>(r2.Error);
        }

        await matches.Save(match, ct);

        if (confirmed)
        {
            // 4) Consume all tickets (if PendingReady)
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

        // 6) Not all ready yet — notify progress to both (or at least to the acknowledging player)
        var readySnapshot = match.ReadyPlayers;
        foreach (var pid in match.Players)
            await notifier.ReadyAcknowledged(pid, match, readySnapshot, match.ReadyDeadlineUtc, ct);

        return new AcknowledgeReadyResult(match, AllReady: false, ReadyDeadlineUtc: match.ReadyDeadlineUtc);
    }

    private Task NotifyReadyFailAsync(PlayerId player, MatchId matchId, string code, string message, CancellationToken ct) =>
        notifier.ReadyAcknowledgeFail(player, matchId, code, message, ct);
}
