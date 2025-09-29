namespace Contracts.V1.Matchmaking;

// 5) GetMatch — polling or diagnostics
public static class GetMatch
{
    public const string Method = "GET";
    public const string Route = "matchmaking/matches/{matchId}";

    public record Request(Guid MatchId);

    public record Response(
        Guid MatchId,
        string Mode,
        string MatchStatus,         // Proposed|ReadyCheck|Confirmed|Aborted
        DateTime CreatedAtUtc,
        DateTime? ReadyDeadlineUtc,
        IReadOnlyList<Guid> Players,
        IReadOnlyList<Guid> ReadyPlayers,
        string? AbortReason        // None|PeerCancel|Timeout|Liveness|System
    );
}