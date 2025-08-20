namespace Domain.Sessions;

public readonly record struct SessionId(Guid Value)
{
    public static SessionId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString("N");
}

public readonly record struct PlayerId(Guid Value)
{
    public static PlayerId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString("N");
}

public record RollbackRequest(ulong CurrentServerFrame, string Reason);

public sealed record MatchSettings
{
    public int TickRate { get; init; } = 60;
    public int InputDelayFrames { get; init; } = 2;

    public static MatchSettings Default => new();
}

public interface IGameCommand
{
    long Id { get; }             // or ulong if you prefer; keep consistent project-wide
    ulong Frame { get; set; }     // server tick index
    PlayerId PlayerId { get; }    // domain PlayerId
    long CreateTimeMs { get; }    // unix ms; or use DateTime if you prefer
}

public sealed class GameCommand : IGameCommand
{
    public long Id { get; init; }
    public ulong Frame { get; set; }
    public PlayerId PlayerId { get; init; }
    public long CreateTimeMs { get; init; }
    public ReadOnlyMemory<byte> Payload { get; init; }
}

public sealed record StateHashReport(ulong Frame, uint Hash, PlayerId PlayerId);