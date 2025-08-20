namespace Contracts.V1.Sessions;

public static class Get
{
    public const string Method = "GET";
    public const string Route = "sessions/{sessionId}";

    public record Request(Guid SessionId);

    public record Response(
        Guid SessionId,
        string Mode,
        string Status,                  // Waiting|InProgress|Aborted|Ended
        string StartBarrier,            // ConnectedOnly|ConnectedAndLoaded|Timer
        DateTime CreatedAtUtc,
        DateTime? StartedAtUtc,
        DateTime? EndedAtUtc,
        DateTime? JoinDeadlineUtc,
        string? CloseReason,            // Completed|NoShow|ServerFailure|AdminTerminate|PlayerAbandon
        List<Response.ParticipantInfo> Participants
    )
    {
        public record ParticipantInfo(Guid Id, string DisplayName, string Status); // Invited|Connected|Loaded|Disconnected
    }
}
