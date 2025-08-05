namespace Domain.Sessions.ValueObjects;

public record MatchSettings
{
    public int TickRate { get; init; } = 60;
    public int InputDelayFrames { get; init; } = 2;

    public static MatchSettings Default => new();
}
