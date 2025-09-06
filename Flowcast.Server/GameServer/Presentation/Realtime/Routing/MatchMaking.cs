namespace Presentation.Realtime.Routing;

public static class MatchMaking
{
    // v1 Requests (Client -> Server)
    public const ushort FindMatch = (ushort)(((int)MessageDomain.Matchmaking << MessageBits.SHIFT_DOMAIN)
        | (MessageBits.DIR_REQ << MessageBits.SHIFT_DIR)
        | (MessageBits.DefaultVersion << MessageBits.SHIFT_VER)
        | 1);

    public const ushort Ready = (ushort)(((int)MessageDomain.Matchmaking << MessageBits.SHIFT_DOMAIN)
        | (MessageBits.DIR_REQ << MessageBits.SHIFT_DIR)
        | (MessageBits.DefaultVersion << MessageBits.SHIFT_VER)
        | 2);

    public const ushort Cancel = (ushort)(((int)MessageDomain.Matchmaking << MessageBits.SHIFT_DOMAIN)
        | (MessageBits.DIR_REQ << MessageBits.SHIFT_DIR)
        | (MessageBits.DefaultVersion << MessageBits.SHIFT_VER)
        | 3);

    // v1 Pushes (Server -> Client)
    public const ushort Queued = (ushort)(((int)MessageDomain.Matchmaking << MessageBits.SHIFT_DOMAIN)
        | (MessageBits.DIR_PUSH << MessageBits.SHIFT_DIR)
        | (MessageBits.DefaultVersion << MessageBits.SHIFT_VER)
        | 0);

    public const ushort Found = (ushort)(((int)MessageDomain.Matchmaking << MessageBits.SHIFT_DOMAIN)
        | (MessageBits.DIR_PUSH << MessageBits.SHIFT_DIR)
        | (MessageBits.DefaultVersion << MessageBits.SHIFT_VER)
        | 1);

    public const ushort FoundFail = (ushort)(((int)MessageDomain.Matchmaking << MessageBits.SHIFT_DOMAIN)
        | (MessageBits.DIR_PUSH << MessageBits.SHIFT_DIR)
        | (MessageBits.DefaultVersion << MessageBits.SHIFT_VER)
        | 2);

    public const ushort Aborted = (ushort)(((int)MessageDomain.Matchmaking << MessageBits.SHIFT_DOMAIN)
        | (MessageBits.DIR_PUSH << MessageBits.SHIFT_DIR)
        | (MessageBits.DefaultVersion << MessageBits.SHIFT_VER)
        | 3);

    public const ushort Confirmed = (ushort)(((int)MessageDomain.Matchmaking << MessageBits.SHIFT_DOMAIN)
        | (MessageBits.DIR_PUSH << MessageBits.SHIFT_DIR)
        | (MessageBits.DefaultVersion << MessageBits.SHIFT_VER)
        | 4);
}

