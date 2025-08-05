namespace Domain.Sessions.ValueObjects;

public interface IGameCommand
{
    long Id { get; }
    ulong Frame { get; set; }
    long PlayerId { get; }
    long CreateTime { get; }
}

public class GameCommand : IGameCommand
{
    public long Id { get; init; }
    public ulong Frame { get; set; }
    public long PlayerId { get; init; }
    public long CreateTime { get; init; }
    public string Payload { get; init; } // JSON or binary
}