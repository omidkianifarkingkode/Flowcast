namespace Contracts.V1.Sessions;

public static class Get
{
    public const string Method = "GET";
    public const string Route = "sessions/{sessionId}";

    public record Request(Guid SessionId);

    public record Response(Guid SessionId, string Mode, string Status, DateTime CreatedAtUtc, List<Response.PlayerInfo> Players)
    {
        public record PlayerInfo(Guid Id, string DisplayName, string Status);
    }
}
