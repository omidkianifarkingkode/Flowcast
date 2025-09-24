using SharedKernel;

namespace Matchmaking.Domain;

public static class TicketErrors
{
    public static readonly Error NotFound = Error.NotFound("ticket.not_found", "Ticket not found.");
    public static readonly Error AlreadyOpenForPlayer = Error.Conflict("ticket.already_open", "Player already has an open ticket for this mode.");
    public static readonly Error NotSearching = Error.Conflict("ticket.not_searching", "Ticket is not in Searching state.");
    public static readonly Error NotPendingReady = Error.Conflict("ticket.not_pending_ready", "Ticket is not in PendingReady state.");
}

public static class MatchErrors
{
    public static readonly Error NotFound = Error.NotFound("match.not_found", "Match not found.");
    public static readonly Error NotProposed = Error.Conflict("match.not_proposed", "Match is not in Proposed state.");
    public static readonly Error NotInReadyCheck = Error.Conflict("match.not_ready_check", "Match is not in ReadyCheck state.");
    public static readonly Error AllPlayerNotReady = Error.Conflict("match.all_not_ready", "All Player are not Ready.");
    public static readonly Error PlayerNotInMatch = Error.NotFound("match.player_not_in_match", "Player is not part of this match.");
    public static readonly Error ReadyWindowExpired = Error.Conflict("match.ready_window_expired", "Ready window has expired.");
}

public static class MatchmakingErrors
{
    public static readonly Error AlreadyInSession = Error.Conflict("mm.already_in_session", "Player already in an active session.");
    public static readonly Error NotHealthy = Error.Conflict("mm.not_healthy", "Player connection not healthy.");
}
