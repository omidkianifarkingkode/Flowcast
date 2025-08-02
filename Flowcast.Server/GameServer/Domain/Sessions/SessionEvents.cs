using SharedKernel;

namespace Domain.Sessions;

public record SessionCreated(SessionId SessionId, string Mode, DateTime CreatedAtUtc) : IDomainEvent;

public record PlayerJoinedSession(SessionId SessionId, long PlayerId) : IDomainEvent;

public record PlayerLeftSession(SessionId SessionId, long PlayerId) : IDomainEvent;

public record SessionEnded(SessionId SessionId, DateTime EndedAtUtc) : IDomainEvent;

public record PlayerMarkedReady(SessionId SessionId, long PlayerId) : IDomainEvent;


