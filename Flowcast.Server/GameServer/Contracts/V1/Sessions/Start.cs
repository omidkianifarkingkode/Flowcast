namespace Contracts.V1.Sessions;

public static class Start
{
    public const string Method = "POST";
    public const string Route = "sessions/start";

    public record Request(Guid SessionId);

    public record Response(
        Guid SessionId,
        string Status,           // Waiting|InProgress|Aborted|Ended
        DateTime? StartedAtUtc
    );
}
