namespace Contracts.V1.Matchmaking;

// 1) FindMatch — enqueue (and try FIFO pair immediately)
public static class FindMatch
{
    public const string Method = "POST";
    public const string Route = "matchmaking/find";

    public record Request(Guid PlayerId, string Mode, string? IdempotencyKey = null);

    public record Response(
        Guid TicketId,
        string TicketState,            // Searching|PendingReady|Consumed|Cancelled|Failed
        string Mode,
        DateTime EnqueuedAtUtc,
        Guid? MatchId,
        string? MatchStatus,           // Proposed|ReadyCheck|Confirmed|Aborted
        DateTime? ReadyDeadlineUtc
    );
}