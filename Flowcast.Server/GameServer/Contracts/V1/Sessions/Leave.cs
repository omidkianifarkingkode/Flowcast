namespace Contracts.V1.Sessions;

public static class Leave
{
    public const string Method = "POST";
    public const string Route = "sessions/leave";

    public record Request(Guid SessionId, Guid PlayerId);

    public record Response(Guid SessionId, Response.PlayerInfo Player, bool WasLastPlayer)
    {
        public record PlayerInfo(Guid Id, string DisplayName);
    }
}
