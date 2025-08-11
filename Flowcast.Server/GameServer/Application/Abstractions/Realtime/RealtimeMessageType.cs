namespace Application.Abstractions.Realtime;

public enum RealtimeMessageType : byte
{
    Unknown = 0,
    Command = 1,
    Event = 2,
    Ping = 3,
    Pong = 4
    // Add more as needed
}
