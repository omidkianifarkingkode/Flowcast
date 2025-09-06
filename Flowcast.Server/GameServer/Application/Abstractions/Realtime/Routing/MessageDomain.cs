namespace Application.Abstractions.Realtime.Routing;

public enum MessageDomain : byte
{
    Transport = 0,
    Auth = 1,
    Matchmaking = 2,
    Session = 3,
    Gameplay = 4,
    Social = 5,
    Admin = 6,
}