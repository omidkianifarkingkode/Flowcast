namespace Contracts.V1.Sessions;

public static class Get
{
    public const string Method = "GET";
    public const string Route = "sessions/{sessionId}";

    public record Response(string SessionId, string Mode, string Status, DateTime CreatedAtUtc, List<PlayerResponse> Players);

    public record PlayerResponse(long Id, string DisplayName, string Status);
}
