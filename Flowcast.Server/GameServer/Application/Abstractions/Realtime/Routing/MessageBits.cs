namespace Application.Abstractions.Realtime.Routing;

public static class MessageBits
{
    public const int SHIFT_DOMAIN = 11; // DDDDD | R | VV | CCCCCCCC
    public const int SHIFT_DIR = 10;
    public const int SHIFT_VER = 8;

    public const int DIR_REQ = 0; // client -> server
    public const int DIR_PUSH = 1; // server -> client

    public const byte DefaultVersion = 0;
}

