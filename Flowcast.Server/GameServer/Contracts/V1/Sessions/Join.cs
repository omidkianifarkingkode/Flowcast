namespace Contracts.V1.Sessions;

public static class Join
{
    public const string Method = "POST";
    public const string Route = "sessions/join";

    public record Request(Guid SessionId, Guid PlayerId);

    public record Response(Guid SessionId, Response.PlayerInfo Player)
    {
        public record PlayerInfo(Guid Id, string DisplayName);
    }
}
