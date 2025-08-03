namespace Contracts.V1.Sessions;

public static class GetForPlayer
{
    public const string Method = "GET";
    public const string Route = "sessions/player/{playerId}";

    public record Response(string SessionId, string Mode, string Status, int PlayerCount, DateTime CreatedAtUtc);
}
