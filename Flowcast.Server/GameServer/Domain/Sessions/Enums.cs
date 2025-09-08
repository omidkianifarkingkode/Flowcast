namespace Domain.Sessions;

public enum SessionStatus
{
    /// <summary>
    /// Session record created; participants invited and handshaking. Start barrier not yet satisfied.
    /// Entered by: SessionCreated.
    /// Exits to: InProgress (barrier met).
    /// Exits to: Aborted (pre‑start failure).
    /// </summary>
    Waiting = 0,

    /// <summary>
    /// Simulation/authoritative gameplay has started.
    /// Entered by: barrier satisfied before joinDeadline → TryStart.
    /// Exits to: Ended (normal finish).
    /// Exits to: Aborted (server failure/admin terminate).
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Session could not start or was killed before completing (e.g., no‑show, allocator error).
    /// Entered by: join/load timeout, server failure, admin action.
    /// Exits to: terminal.
    /// </summary>
    Aborted = 2,

    /// <summary>
    /// Graceful/normal finish after start (or early termination counted as ended for analytics).
    /// Entered by: match rules complete / game over.
    /// Exits to: terminal.
    /// </summary>
    Ended = 3
}

public enum SessionCloseReason
{
    /// <summary>
    /// Normal end of game/session after InProgress.
    /// </summary>
    Completed = 0,

    /// <summary>
    /// Pre‑start failure: at least one participant never Connected/Loaded before deadline.
    /// </summary>
    NoShow = 1,

    /// <summary>
    /// Infra/allocator/crash or dependency issue.
    /// </summary>
    ServerFailure = 2,

    /// <summary>
    /// Manual operator stop.
    /// </summary>
    AdminTerminate = 3,

    /// <summary>
    /// After start, a participant leaves (policy decides whether this is “Ended” vs “Aborted”).
    /// </summary>
    PlayerAbandon = 4
}

public enum ParticipantStatus
{
    /// <summary>
    /// Listed as a session participant (from SessionCreated) but not attached.
    /// </summary>
    Invited = 0,

    /// <summary>
    /// Transport and auth bound to this session/seat (after JoinSession).
    /// </summary>
    Connected = 1,

    /// <summary>
    /// Client reports content readiness (after ParticipantLoaded), eligible for barrier.
    /// </summary>
    Loaded = 2,

    /// <summary>
    /// Was attached, then transport died or left; may reattach if policy allows.
    /// </summary>
    Disconnected = 3
}

/// <summary>
/// Start barrier profile for the pipeline
/// </summary>
public enum StartBarrier
{
    /// <summary>
    /// Start as soon as all participants are Connected. Fast, tolerant of long loads.
    /// </summary>
    ConnectedOnly = 0,

    /// <summary>
    /// Strict start: all participants must be Connected & Loaded. Fair start.
    /// </summary>
    ConnectedAndLoaded = 1,

    /// <summary>
    /// Start at scheduled time (T‑minus) regardless of late joiners; optional minimum quorum check.
    /// </summary>
    Timer = 2
}
