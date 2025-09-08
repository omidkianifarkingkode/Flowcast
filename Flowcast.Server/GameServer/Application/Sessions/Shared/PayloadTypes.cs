using Application.Abstractions.Realtime;
using Application.Abstractions.Realtime.Routing;

namespace Application.Sessions.Shared;

public static class SessionV1
{
    // Requests (client -> server)
    public const ushort Create = MessageDomain.Sessions | MessageBits.V1 | MessageBits.REQ | 1;

    // Pushes (server -> client)
    public const ushort Created = MessageDomain.Sessions | MessageBits.V1 | MessageBits.PSH | 1;
    public const ushort CreateFail = MessageDomain.Sessions | MessageBits.V1 | MessageBits.PSH | 1;
}
