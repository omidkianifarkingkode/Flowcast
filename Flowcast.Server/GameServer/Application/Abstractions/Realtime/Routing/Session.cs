namespace Application.Abstractions.Realtime.Routing;

public static class Session
{
    public const ushort Join = (int)MessageDomain.Session << MessageBits.SHIFT_DOMAIN
        | MessageBits.DIR_REQ << MessageBits.SHIFT_DIR
        | MessageBits.DefaultVersion << MessageBits.SHIFT_VER
        | 1;

    public const ushort Allocated = (int)MessageDomain.Session << MessageBits.SHIFT_DOMAIN
        | MessageBits.DIR_PUSH << MessageBits.SHIFT_DIR
        | MessageBits.DefaultVersion << MessageBits.SHIFT_VER
        | 0;

    // ...add others similarly
}

