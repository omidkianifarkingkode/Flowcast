using Domain.Sessions;
using SharedKernel;

namespace Domain.Matchmaking;

public sealed class Ticket : Entity<TicketId>, IAggregateRoot
{
    public PlayerId PlayerId { get; private set; }
    public string Mode { get; private set; } = string.Empty;
    public DateTime EnqueuedAtUtc { get; private set; }
    public TicketState State { get; private set; }
    public MatchId? MatchId { get; private set; } // set when paired

    private Ticket() { }

    private Ticket(TicketId id, PlayerId playerId, string mode, DateTime enqueuedAtUtc)
        : base(id)
    {
        Id = id;
        PlayerId = playerId;
        Mode = mode;
        EnqueuedAtUtc = enqueuedAtUtc;
        State = TicketState.Searching;
        AddDomainEvent(new TicketEnqueued(Id, PlayerId, Mode, EnqueuedAtUtc));
    }

    public static Ticket Create(PlayerId playerId, string mode, DateTime enqueuedAtUtc)
        => new(TicketId.New(), playerId, mode, enqueuedAtUtc);

    public Result MoveToPendingReady(MatchId matchId, DateTime utc)
    {
        if (State != TicketState.Searching)
            return Result.Failure(TicketErrors.NotSearching);
        MatchId = matchId;
        State = TicketState.PendingReady;
        AddDomainEvent(new TicketMovedToPendingReady(Id, matchId, utc));
        return Result.Success();
    }

    public Result Cancel(DateTime utc)
    {
        if (State is TicketState.Cancelled or TicketState.Consumed) return Result.Success();
        if (State is TicketState.Failed) return Result.Success();
        State = TicketState.Cancelled;
        AddDomainEvent(new TicketCancelled(Id, PlayerId, utc));
        return Result.Success();
    }

    public Result Fail(DateTime utc)
    {
        if (State is TicketState.Failed or TicketState.Cancelled or TicketState.Consumed) return Result.Success();
        State = TicketState.Failed;
        return Result.Success();
    }

    public Result Consume()
    {
        if (State == TicketState.Consumed) return Result.Success();
        if (State != TicketState.PendingReady) return Result.Failure(TicketErrors.NotPendingReady);
        State = TicketState.Consumed;
        return Result.Success();
    }
}
