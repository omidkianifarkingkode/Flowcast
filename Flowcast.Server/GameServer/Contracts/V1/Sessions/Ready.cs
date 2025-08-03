namespace Contracts.V1.Sessions;

public static class Ready
{
    public const string Method = "POST";
    public const string Route = "sessions/{sessionId}/ready";

    public record Request(string SessionId, long PlayerId);

    public record Response(string SessionId, bool AllPlayersReady);
}
