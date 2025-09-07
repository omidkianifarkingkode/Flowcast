using Application.Abstractions.Realtime;
using Application.Abstractions.Realtime.Routing;

namespace Application.MatchMakings.Shared;

public static class MatchmakingV1
{
    // Inbound (client → server)
    public const ushort FindMatch       = MessageDomain.Matchmaking | MessageBits.V1 | MessageBits.REQ | 1;
    public const ushort Ready           = MessageDomain.Matchmaking | MessageBits.V1 | MessageBits.REQ | 2;
    public const ushort Cancel          = MessageDomain.Matchmaking | MessageBits.V1 | MessageBits.REQ | 3;

    // Outbound (server → client)
    public const ushort Queued          = MessageDomain.Matchmaking | MessageBits.V1 | MessageBits.PSH | 0;
    public const ushort Found           = MessageDomain.Matchmaking | MessageBits.V1 | MessageBits.PSH | 1;
    public const ushort FoundFail       = MessageDomain.Matchmaking | MessageBits.V1 | MessageBits.PSH | 2;
    public const ushort Aborted         = MessageDomain.Matchmaking | MessageBits.V1 | MessageBits.PSH | 3;
    public const ushort Confirmed       = MessageDomain.Matchmaking | MessageBits.V2 | MessageBits.PSH | 4;
    public const ushort CancelFail      = MessageDomain.Matchmaking | MessageBits.V2 | MessageBits.PSH | 5;
    public const ushort Cancelled       = MessageDomain.Matchmaking | MessageBits.V2 | MessageBits.PSH | 6;
    public const ushort ReadyAck        = MessageDomain.Matchmaking | MessageBits.V2 | MessageBits.PSH | 7;
    public const ushort ReadyAckFail    = MessageDomain.Matchmaking | MessageBits.V2 | MessageBits.PSH | 8;
}
