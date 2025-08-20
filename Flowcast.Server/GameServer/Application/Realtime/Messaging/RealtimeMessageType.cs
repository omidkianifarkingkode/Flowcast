namespace Application.Realtime.Messaging;

public enum RealtimeMessageType : ushort
{
    Unknown = 0,
    Command = 1,
    Spawn = 2,
    MatchMaking = 3,
    Pong = 4
    // Add more as needed
}
