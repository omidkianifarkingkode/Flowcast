namespace Contracts.V1.Sessions;

public static class End
{
    public const string Method = "POST";
    public const string Route = "sessions/end";

    public record Request(Guid SessionId);

    public record Response(Guid SessionId, DateTime EndedAtUtc);
}
