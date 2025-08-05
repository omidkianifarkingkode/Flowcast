using Domain.Sessions.ValueObjects;
using SharedKernel;

namespace Domain.Sessions;

public interface ISessionRepository
{
    Task<Result<Session>> GetById(SessionId id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Session>> GetAll(CancellationToken cancellationToken = default);

    Task Save(Session session, CancellationToken cancellationToken = default);

    Task Delete(SessionId id, CancellationToken cancellationToken = default);
}