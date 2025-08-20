using SharedKernel;

namespace Domain.Sessions;

public static class SessionErrors
{
    public static readonly Error AlreadyStarted = Error.Conflict("session.already_started", "Session already started.");
    public static readonly Error AlreadyEnded = Error.Conflict("session.already_ended", "Session already ended.");
    public static readonly Error ParticipantMissing = Error.NotFound("session.participant_missing", "Participant not found.");
    public static readonly Error DuplicateJoin = Error.Conflict("session.duplicate_join", "Participant already joined.");
    public static readonly Error SessionNotFound = Error.NotFound("session.not_found", "Session was not found.");
}
