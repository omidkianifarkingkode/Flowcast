namespace Contracts.V1.Sessions;

public static class Ready
{
    public const string Method = "PUT";
    public const string Route = "sessions/ready";

    public record Request(Guid SessionId, Guid PlayerId);

    public record Response(Guid SessionId, Response.PlayerInfo Player, bool AllPlayersReady)
    {
        public record PlayerInfo(Guid Id, string DisplayName);
    }
}
