using SharedKernel;

namespace Domain.Sessions;

public sealed record SessionCreated(SessionId SessionId, string Mode, DateTime CreatedAtUtc) : IDomainEvent;
public sealed record SessionStarted(SessionId SessionId, string Mode, DateTime StartedAtUtc) : IDomainEvent;
public sealed record SessionEnded(SessionId SessionId, DateTime EndedAtUtc) : IDomainEvent;

public sealed record ParticipantJoined(SessionId SessionId, PlayerId PlayerId, DateTime Utc) : IDomainEvent;
public sealed record ParticipantLeft(SessionId SessionId, PlayerId PlayerId, DateTime Utc) : IDomainEvent;
public sealed record ParticipantLoaded(SessionId SessionId, PlayerId PlayerId, DateTime Utc) : IDomainEvent;
