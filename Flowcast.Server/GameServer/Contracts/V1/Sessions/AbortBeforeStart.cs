namespace Contracts.V1.Sessions;

public static class AbortBeforeStart
{
    public const string Method = "POST";
    public const string Route = "sessions/abort";

    public record Request(Guid SessionId, string Reason = "NoShow");

    public record Response(
        Guid SessionId,
        string Status,            // Aborted (or Ended, depending on your domain)
        DateTime EndedAtUtc,
        string CloseReason        // Completed|NoShow|ServerFailure|AdminTerminate|PlayerAbandon
    );
}