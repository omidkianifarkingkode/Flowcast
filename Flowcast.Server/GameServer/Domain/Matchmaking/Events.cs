using Domain.Sessions;
using SharedKernel;

namespace Domain.Matchmaking;

public sealed record TicketEnqueued(TicketId TicketId, PlayerId PlayerId, string Mode, DateTime EnqueuedAtUtc) : IDomainEvent;
public sealed record TicketCancelled(TicketId TicketId, PlayerId PlayerId, DateTime Utc) : IDomainEvent;
public sealed record TicketMovedToPendingReady(TicketId TicketId, MatchId MatchId, DateTime Utc) : IDomainEvent;

public sealed record MatchProposed(MatchId MatchId, string Mode, IReadOnlyList<PlayerId> Players, DateTime Utc) : IDomainEvent;
public sealed record MatchReadyAcknowledged(MatchId MatchId, PlayerId PlayerId, DateTime Utc) : IDomainEvent;
public sealed record MatchConfirmed(MatchId MatchId, string Mode, IReadOnlyList<PlayerId> Players, DateTime Utc) : IDomainEvent;
public sealed record MatchReadyFailed(MatchId MatchId, AbortReason Reason, DateTime Utc) : IDomainEvent;
