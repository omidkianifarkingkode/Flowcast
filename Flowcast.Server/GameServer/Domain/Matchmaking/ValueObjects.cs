namespace Domain.Matchmaking;

public readonly record struct TicketId(Guid Value)
{
    public static TicketId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString("N");
}

public readonly record struct MatchId(Guid Value)
{
    public static MatchId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString("N");
}

/// Ready window duration for a Match proposal.
public sealed class ReadyWindow
{
    public TimeSpan Duration { get; }
    public ReadyWindow(TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(duration));
        Duration = duration;
    }
}
