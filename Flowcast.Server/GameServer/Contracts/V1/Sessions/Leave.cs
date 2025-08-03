namespace Contracts.V1.Sessions;

public static class Leave
{
    public const string Method = "POST";
    public const string Route = "sessions/{sessionId}/leave";

    public record Request(string SessionId, long PlayerId);

    public record Response(string SessionId, bool WasLastPlayer);
}
