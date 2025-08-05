namespace Contracts.V1.Sessions;

public static class Create
{
    public const string Method = "POST";
    public const string Route = "sessions/create";

    public record Request(string Mode, List<Request.Player> Players, Request.MatchSettings? GameSettings)
    {
        public record Player(Guid Id, string DisplayName);
        public record MatchSettings
        {
            public int TickRate { get; init; } = 60;
            public int InputDelayFrames { get; init; } = 2;
        }
    }

    public record Response(Guid SessionId);
}
