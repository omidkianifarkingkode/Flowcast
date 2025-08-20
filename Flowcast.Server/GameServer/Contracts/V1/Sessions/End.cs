namespace Contracts.V1.Sessions;

public static class End
{
    public const string Method = "POST";
    public const string Route = "sessions/end";

    public record Request(Guid SessionId, string? Reason = null);

    public record Response(Guid SessionId, string Status, DateTime EndedAtUtc, string CloseReason);
}
