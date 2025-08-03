namespace Contracts.V1.Sessions;

public static class Create
{
    public const string Method = "POST";
    public const string Route = "sessions";

    public record Request(string Mode, List<PlayerRequest> Players, MatchSettings? GameSettings);

    public record Response(string SessionId);

    public record PlayerRequest(long Id, string DisplayName);

    public record MatchSettings
    {
        public int TickRate { get; init; } = 60;
        public int InputDelayFrames { get; init; } = 2;
    }
}
