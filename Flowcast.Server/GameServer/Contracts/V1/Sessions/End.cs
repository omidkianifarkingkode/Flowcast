namespace Contracts.V1.Sessions;

public static class End
{
    public const string Method = "DELETE";
    public const string Route = "sessions/{sessionId}/end";

    public record Request(string SessionId);

    public record Response(string SessionId, DateTime EndedAtUtc);
}
