namespace Contracts.V1.Sessions;

public static class Join
{
    public const string Method = "POST";
    public const string Route = "sessions/join";

    public record Request(Guid SessionId, Guid PlayerId, string JoinToken, string? BuildHash = null, string? DisplayName = null);

    public record Response(Guid SessionId, Response.ParticipantInfo Participant, string SessionStatus)
    {
        public record ParticipantInfo(Guid Id, string DisplayName, string Status);
    }
}