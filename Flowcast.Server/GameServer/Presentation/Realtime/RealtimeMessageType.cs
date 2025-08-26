namespace Presentation.Realtime;

public enum RealtimeMessageType : ushort
{
    Unknown = 0,
    Command = 1,
    Spawn = 2,
    MatchMaking = 3,
    Ping = 4,
    Pong = 5,

    // --- Matchmaking (outbound & inbound) ---
    MatchFound = 10, // server -> client
    MatchAborted = 11, // server -> client
    MatchConfirmed = 12, // server -> client
    ReadyRequest = 13, // server -> client (optional prompt)
    ReadyAck = 14, // client -> server (player presses Ready)

    // --- Session allocation/start lifecycle ---
    SessionAllocated = 20, // server -> client (sessionId, joinTicket, deadline, barrier)
    JoinAccepted = 21, // server -> client (optional)
    ClientLoaded = 22, // client -> server
    SessionStarted = 23, // server -> client
    SessionEnded = 24  // server -> client
}
