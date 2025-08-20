namespace Application.MatchMakings.Shared;

public sealed class MatchmakingOptions
{
    public int ReadyWindowSeconds { get; init; } = 15;
    public bool RequireHealthyConnection { get; init; } = true; // gate enqueue/ready on liveness
}
