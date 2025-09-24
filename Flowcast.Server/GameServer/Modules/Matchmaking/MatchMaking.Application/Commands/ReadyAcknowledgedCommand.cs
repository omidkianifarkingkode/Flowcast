using Matchmaking.Contracts;
using Matchmaking.Domain;
using MatchMaking.Application.Shared;
using MessagePack;
using Microsoft.Extensions.Logging;
using Realtime.Transport.Messaging;
using Shared.Application.Messaging;
using Shared.Application.Realtime;
using Shared.Application.Services;
using SharedKernel;
using SharedKernel.Primitives;

namespace MatchMaking.Application.Commands;

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
    IMatchmakingNotifier notifier,
    ILogger<AcknowledgeReadyHandler> logger)
    : ICommandHandler<AcknowledgeReadyCommand, AcknowledgeReadyResult>
{
    public async Task<Result<AcknowledgeReadyResult>> Handle(AcknowledgeReadyCommand command, CancellationToken cancellationToken)
    {
        // 0) Liveness gate — for READY this should be strict
        if (!liveness.IsHealthy(command.PlayerId))
        {

            await notifier.ReadyAcknowledgeFail(command.PlayerId, command.MatchId, MatchmakingErrors.NotHealthy.Code,
                                                MatchmakingErrors.NotHealthy.Description, cancellationToken);
            return Result.Failure<AcknowledgeReadyResult>(MatchmakingErrors.NotHealthy);
        }

        // 1) Load match
        var matchResult = await matches.GetById(command.MatchId, cancellationToken);
        if (matchResult.IsFailure)
        {
            logger.LogWarning("ReadyAck failed to load match {MatchId}. Error: {Code} - {Desc}",
                command.MatchId, matchResult.Error.Code, matchResult.Error.Description);

            await notifier.ReadyAcknowledgeFail(command.PlayerId, command.MatchId, matchResult.Error.Code,
                                                matchResult.Error.Description, cancellationToken);
            return Result.Failure<AcknowledgeReadyResult>(matchResult.Error);
        }

        var match = matchResult.Value;

        // 2) Acknowledge
        var r1 = match.AcknowledgeReady(command.PlayerId, clock.UtcNow);
        if (r1.IsFailure)
        {
            await notifier.ReadyAcknowledgeFail(command.PlayerId, command.MatchId, r1.Error.Code, r1.Error.Description,
                                                cancellationToken);
            return Result.Failure<AcknowledgeReadyResult>(r1.Error);
        }

        // 3) Try confirm if all ready
        var r2 = match.ConfirmIfAllReady(clock.UtcNow, out var confirmed);
        if (r2.IsFailure)
        {
            await notifier.ReadyAcknowledgeFail(command.PlayerId, command.MatchId, r2.Error.Code, r2.Error.Description,
                                                cancellationToken);
            return Result.Failure<AcknowledgeReadyResult>(r2.Error);
        }

        if (confirmed)
        {
            // 4) Consume all tickets (if PendingReady)
            await ConsumeTickets(match, cancellationToken);

            await matches.Save(match, cancellationToken);

            // Notify all; session allocation will proceed in a separate pipeline (outbox/handler)
            foreach (var pid in match.Players)
                await notifier.MatchConfirmed(pid, match, cancellationToken);

            logger.LogInformation("Match {MatchId} confirmed. Session allocation will be triggered via outbox.", match.Id);

            return new AcknowledgeReadyResult(match, AllReady: true, ReadyDeadlineUtc: match.ReadyDeadlineUtc);
        }

        // 6) Not all ready yet — notify progress to both (or at least to the acknowledging player)
        await matches.Save(match, cancellationToken);

        var readySnapshot = match.ReadyPlayers;
        foreach (var pid in match.Players)
            await notifier.ReadyAcknowledged(pid, match, readySnapshot, match.ReadyDeadlineUtc, cancellationToken);

        return new AcknowledgeReadyResult(match, AllReady: false, ReadyDeadlineUtc: match.ReadyDeadlineUtc);
    }

    private async Task ConsumeTickets(Match match, CancellationToken cancellationToken)
    {
        foreach (var pid in match.Players)
        {
            var open = await tickets.GetOpenByPlayer(pid, match.Mode, cancellationToken);

            if (open.IsFailure && open.Error is not null &&
                !string.Equals(open.Error.Code, TicketErrors.NotFound.Code, StringComparison.OrdinalIgnoreCase))
            {
                logger.LogWarning("Ticket fetch failed during consume for Player {PlayerId} in mode {Mode}. Error: {Code} - {Desc}",
                    pid, match.Mode, open.Error.Code, open.Error.Description);

                continue;
            }

            if (open.IsSuccess && open.Value is { } t && t.State == TicketState.PendingReady)
            {
                var consumeResult = t.Consume();
                if (consumeResult.IsFailure)
                {
                    logger.LogWarning("Ticket consume failed for Player {PlayerId} ticket {TicketId}. Error: {Code} - {Desc}",
                        pid, t.Id, consumeResult.Error.Code, consumeResult.Error.Description);
                }
                else
                {
                    await tickets.Save(t, cancellationToken);
                }
            }
        }
    }
}
