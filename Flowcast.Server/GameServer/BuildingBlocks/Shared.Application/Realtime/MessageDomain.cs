using Shared.Application.Realtime.Routing;

namespace Shared.Application.Realtime;

// Format: domain ID << SHIFT_DOMAIN
// ID must be unique across all modules (0..63)
public static class MessageDomain
{
    public const int Reserved       = 0 << MessageBits.SHIFT_DOMAIN; // system pings, errors, auth, etc.
    public const int Chat           = 1 << MessageBits.SHIFT_DOMAIN;
    public const int Matchmaking    = 2 << MessageBits.SHIFT_DOMAIN;
    public const int Sessions       = 3 << MessageBits.SHIFT_DOMAIN;
    public const int GameLogic      = 4 << MessageBits.SHIFT_DOMAIN;
    public const int Inventory      = 5 << MessageBits.SHIFT_DOMAIN;
}
