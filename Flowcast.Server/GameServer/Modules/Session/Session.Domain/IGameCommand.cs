using SharedKernel.Primitives;

namespace Session.Domain;

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
