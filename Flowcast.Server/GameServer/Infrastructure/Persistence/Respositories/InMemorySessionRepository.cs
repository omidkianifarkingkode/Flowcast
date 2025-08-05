using Domain.Sessions;
using Domain.Sessions.ValueObjects;
using SharedKernel;
using System.Collections.Concurrent;

namespace Infrastructure.Persistence.Respositories;

public class InMemorySessionRepository : ISessionRepository
{
    private readonly ConcurrentDictionary<SessionId, Session> _store = new();

    public async Task<Result<Session>> GetById(SessionId id, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        if (_store.TryGetValue(id, out var session))
            return Result.Success(session);

        return Result.Failure<Session>(SessionErrors.SessionNotFound);
    }

    public async Task<IReadOnlyCollection<Session>> GetAll(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        return _store.Values.ToList();
    }

    public async Task Save(Session session, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        _store[session.Id] = session;
    }

    public async Task Delete(SessionId id, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        _store.TryRemove(id, out _);
    }
}
