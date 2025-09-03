namespace Contracts.V1.Sessions;

public static class Loaded
{
    public const string Method = "PUT";
    public const string Route = "sessions/loaded";

    public record Request(Guid SessionId, Guid PlayerId);

    public record Response(Guid SessionId, Response.ParticipantInfo Participant, string SessionStatus, DateTime? StartedAtUtc)
    {
        public record ParticipantInfo(Guid Id, string DisplayName, string Status);
    }
}