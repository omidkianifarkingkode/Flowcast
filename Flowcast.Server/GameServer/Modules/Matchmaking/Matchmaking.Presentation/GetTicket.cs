namespace Contracts.V1.Matchmaking;

// 4) GetTicket — polling or diagnostics
public static class GetTicket
{
    public const string Method = "GET";
    public const string Route = "matchmaking/tickets/{ticketId}";

    public record Request(Guid TicketId);

    public record Response(
        Guid TicketId,
        string TicketState,
        string Mode,
        DateTime EnqueuedAtUtc,
        Guid? MatchId
    );
}