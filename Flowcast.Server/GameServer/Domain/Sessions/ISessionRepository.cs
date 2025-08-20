using SharedKernel;

namespace Domain.Sessions;

public interface ISessionRepository
{
    Task<Result<Session>> GetById(SessionId id, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<Session>>> GetAll(CancellationToken cancellationToken = default);

    Task<Result<Session?>> GetActiveByPlayer(PlayerId playerId, CancellationToken ct);

    Task Save(Session session, CancellationToken cancellationToken = default);

    Task Delete(SessionId id, CancellationToken cancellationToken = default);
}