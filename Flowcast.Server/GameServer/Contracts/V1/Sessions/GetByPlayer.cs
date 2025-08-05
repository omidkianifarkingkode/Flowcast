namespace Contracts.V1.Sessions;

public static class GetByPlayer
{
    public const string Method = "GET";
    public const string Route = "sessions/get-by-player/{playerId}";

    public record Request(Guid PlayerId);

    public record Response(Guid SessionId, string Mode, string Status, int PlayerCount, DateTime CreatedAtUtc);
}
