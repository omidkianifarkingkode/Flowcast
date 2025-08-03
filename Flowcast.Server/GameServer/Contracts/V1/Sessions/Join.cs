namespace Contracts.V1.Sessions;

public static class Join
{
    public const string Method = "POST";
    public const string Route = "sessions/{sessionId}/join";

    public record Request(long PlayerId, string DisplayName);

    public record Response(string SessionId);
}
