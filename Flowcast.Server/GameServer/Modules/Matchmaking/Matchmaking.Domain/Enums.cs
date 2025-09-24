namespace Matchmaking.Domain;

public enum TicketState
{
    /// <summary>
    /// Candidate for pairing in the given mode.
    /// Entered by: FindMatch (new) or policy requeue.
    /// Exits to: PendingReady, Cancelled, Failed.
    /// </summary>
    Searching = 0,

    /// <summary>
    /// Paired into a Match; waiting for Ready(matchId).
    /// Entered by: pair success.
    /// Exits to: Consumed (after match confirmed), Cancelled, Failed (abort/timeout).
    /// </summary>
    PendingReady = 1,

    /// <summary>
    /// Match confirmed; ticket no longer active.
    /// Entered by: MatchStatus.Confirmed.
    /// Exits to: terminal.
    /// </summary>
    Consumed = 2,

    /// <summary>
    /// User cancelled before confirmation.
    /// Entered by: CancelMatch.
    /// Exits to: terminal.
    /// </summary>
    Cancelled = 3,

    /// <summary>
    /// System abort (timeout/liveness/policy).
    /// Entered by: AbortReason != None.
    /// Exits to: terminal (or requeue by policy).
    /// </summary>
    Failed = 4
}

public enum MatchStatus
{
    /// <summary>
    /// Internal creation intent; not yet visible to players
    /// </summary>
    Proposed = 0,

    /// <summary>
    /// All players notified; waiting for Ready(matchId) before deadline.
    /// </summary>
    ReadyCheck = 1,

    /// <summary>
    /// All required Ready ACKs received in time; go allocate session.
    /// </summary>
    Confirmed = 2,

    /// <summary>
    /// ReadyCheck failed or explicit abort; notify clients and close related tickets.
    /// </summary>
    Aborted = 3
}

public enum AbortReason
{
    /// <summary>
    /// Default / Placeholder.
    /// </summary>
    None = 0,

    /// <summary>
    /// One side cancelled while the other waited.
    /// </summary>
    PeerCancel = 1,

    /// <summary>
    /// Ready deadline passed; not all ACKed in time.
    /// </summary>
    Timeout = 2,

    /// <summary>
    /// Detected dead/disconnected during ready phase.
    /// </summary>
    Liveness = 3,

    /// <summary>
    /// Server/allocator/policy error.
    /// </summary>
    System = 4
}
