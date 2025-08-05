using Domain.Sessions.ValueObjects;
using SharedKernel;

namespace Domain.Sessions.Events;

public record SessionCreated(SessionId SessionId, string Mode, DateTime CreatedAtUtc) : IDomainEvent;

public record PlayerJoinedSession(SessionId SessionId, PlayerId PlayerId) : IDomainEvent;

public record PlayerLeftSession(SessionId SessionId, PlayerId PlayerId) : IDomainEvent;

public record SessionEnded(SessionId SessionId, DateTime EndedAtUtc) : IDomainEvent;

public record PlayerMarkedReady(SessionId SessionId, PlayerId PlayerId) : IDomainEvent;

public record CommandReceived() : IDomainEvent;

public record RollbackRequested() : IDomainEvent;


