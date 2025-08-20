namespace Domain.Sessions;

public enum SessionStatus
{
    Waiting = 0, // Created, participants joining/handshaking; start barrier not yet satisfied
    InProgress = 1, // Simulation ticking
    Aborted = 2, // Did not (or cannot) start; e.g., no-show, allocation failure
    Ended = 3  // Finished after starting (normal or early termination)
}

// Optional, helpful for analytics & clean auditing
public enum SessionCloseReason
{
    Completed = 0,
    NoShow = 1,        // pre-start; someone never connected/loaded
    ServerFailure = 2, // allocator/crash
    AdminTerminate = 3,
    PlayerAbandon = 4  // after start
}

// Existing: SessionStatus { Waiting, InProgress, Ended }
// Add:
public enum ParticipantStatus
{
    Invited = 0,
    Connected = 1,
    Loaded = 2,
    Disconnected = 3
}

// Start barrier profile for the pipeline
public enum StartBarrier
{
    ConnectedOnly = 0,        // Profile B
    ConnectedAndLoaded = 1,   // Profile A (strict)
    Timer = 2                 // Profile C
}
