using SharedKernel.Primitives;

namespace Session.Domain;

public readonly record struct SessionId(Guid Value)
{
    public static SessionId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString("N");
}

public record RollbackRequest(ulong CurrentServerFrame, string Reason);

public sealed record MatchSettings
{
    public int TickRate { get; init; } = 60;
    public int InputDelayFrames { get; init; } = 2;

    public static MatchSettings Default => new();
}

public sealed record StateHashReport(ulong Frame, uint Hash, PlayerId PlayerId);