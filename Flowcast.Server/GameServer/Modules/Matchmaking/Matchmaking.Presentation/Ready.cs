namespace Contracts.V1.Matchmaking;

// 3) Ready — player acknowledges ready for a specific match
public static class Ready
{
    public const string Method = "POST";
    public const string Route = "matchmaking/ready";

    public record Request(Guid PlayerId, Guid MatchId);

    public record Response(
        Guid MatchId,
        string MatchStatus,             // ReadyCheck|Confirmed|Aborted
        DateTime? ReadyDeadlineUtc,
        IReadOnlyList<Guid> ReadyPlayers,
        bool AllReady,
        bool SessionAllocationPending   // true when Confirmed (server will allocate Session next)
    );
}