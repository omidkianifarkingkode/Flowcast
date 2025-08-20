namespace Contracts.V1.Matchmaking;

// 2) CancelMatch — remove ticket or abort pending match
public static class CancelMatch
{
    public const string Method = "POST";
    public const string Route = "matchmaking/cancel";

    public record Request(Guid PlayerId, string Mode);

    public record Response(Guid TicketId, string TicketState);
}