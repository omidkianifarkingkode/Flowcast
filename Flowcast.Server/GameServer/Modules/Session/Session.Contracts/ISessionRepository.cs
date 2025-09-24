using Session.Domain;
using SharedKernel;
using SharedKernel.Primitives;

namespace Session.Contracts;

public interface ISessionRepository
{
    Task<Result<SessionEntity>> GetById(SessionId id, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<SessionEntity>>> GetAll(CancellationToken cancellationToken = default);

    Task<Result<SessionEntity?>> GetActiveByPlayer(PlayerId playerId, CancellationToken ct);

    Task Save(SessionEntity session, CancellationToken cancellationToken = default);

    Task Delete(SessionId id, CancellationToken cancellationToken = default);
}