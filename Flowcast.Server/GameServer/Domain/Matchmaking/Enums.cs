namespace Domain.Matchmaking;

public enum TicketState
{
    Searching = 0,
    PendingReady = 1,
    Consumed = 2,
    Cancelled = 3,
    Failed = 4
}

public enum MatchStatus
{
    Proposed = 0,
    ReadyCheck = 1,
    Confirmed = 2,
    Aborted = 3
}

public enum AbortReason
{
    None = 0,
    PeerCancel = 1,
    Timeout = 2,
    Liveness = 3,
    System = 4
}
