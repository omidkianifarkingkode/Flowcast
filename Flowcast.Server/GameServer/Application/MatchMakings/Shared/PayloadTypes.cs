using Application.Abstractions.Realtime.Routing;

namespace Application.MatchMakings.Shared;

public static class PayloadTypes
{
    public const byte Domain = 2;

    // v1 Requests (Client -> Server)
    public const ushort FindMatch = Domain << MessageBits.SHIFT_DOMAIN
        | MessageBits.DIR_REQ << MessageBits.SHIFT_DIR
        | MessageBits.DefaultVersion << MessageBits.SHIFT_VER
        | 1;

    public const ushort Ready = Domain << MessageBits.SHIFT_DOMAIN
        | MessageBits.DIR_REQ << MessageBits.SHIFT_DIR
        | MessageBits.DefaultVersion << MessageBits.SHIFT_VER
        | 2;

    public const ushort Cancel = Domain << MessageBits.SHIFT_DOMAIN
        | MessageBits.DIR_REQ << MessageBits.SHIFT_DIR
        | MessageBits.DefaultVersion << MessageBits.SHIFT_VER
        | 3;

    // v1 Pushes (Server -> Client)
    public const ushort Queued = Domain << MessageBits.SHIFT_DOMAIN
        | MessageBits.DIR_PUSH << MessageBits.SHIFT_DIR
        | MessageBits.DefaultVersion << MessageBits.SHIFT_VER
        | 0;

    public const ushort Found = Domain << MessageBits.SHIFT_DOMAIN
        | MessageBits.DIR_PUSH << MessageBits.SHIFT_DIR
        | MessageBits.DefaultVersion << MessageBits.SHIFT_VER
        | 1;

    public const ushort FoundFail = Domain << MessageBits.SHIFT_DOMAIN
        | MessageBits.DIR_PUSH << MessageBits.SHIFT_DIR
        | MessageBits.DefaultVersion << MessageBits.SHIFT_VER
        | 2;

    public const ushort Aborted = Domain << MessageBits.SHIFT_DOMAIN
        | MessageBits.DIR_PUSH << MessageBits.SHIFT_DIR
        | MessageBits.DefaultVersion << MessageBits.SHIFT_VER
        | 3;

    public const ushort Confirmed = Domain << MessageBits.SHIFT_DOMAIN
        | MessageBits.DIR_PUSH << MessageBits.SHIFT_DIR
        | MessageBits.DefaultVersion << MessageBits.SHIFT_VER
        | 4;
}

