using SharedKernel;

namespace Domain.Sessions;

public static class SessionErrors
{
    public static readonly Error SessionAlreadyStarted = Error.Conflict(
        code: "Session.AlreadyStarted",
        description: "Cannot join a session that has already started.");

    public static readonly Error PlayerAlreadyInSession = Error.Conflict(
        code: "Session.PlayerAlreadyIn",
        description: "Player is already in the session.");

    public static readonly Error SessionAlreadyEnded = Error.Conflict(
        code: "Session.AlreadyEnded",
        description: "Session is already ended.");

    public static readonly Error SessionNotFound =
        Error.NotFound("Session.NotFound", "Session not found.");

    public static readonly Error PlayerNotFound = Error.NotFound(
        code: "Session.PlayerNotFound",
        description: "Player not found in the session.");

    public static readonly Error SessionFull = Error.Conflict(
        code: "Session.Full",
        description: "Session already has two players.");

}
