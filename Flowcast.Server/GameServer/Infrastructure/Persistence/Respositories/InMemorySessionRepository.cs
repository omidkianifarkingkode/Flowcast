using Domain.Sessions;
using SharedKernel;
using System.Collections.Concurrent;

namespace Infrastructure.Persistence.Respositories;

public class InMemorySessionRepository : ISessionRepository
{
    private readonly ConcurrentDictionary<SessionId, Session> _store = new();

    public Result<Session> GetById(SessionId id)
    {
        if (_store.TryGetValue(id, out var session))
            return Result.Success(session);

        return Result.Failure<Session>(SessionErrors.SessionNotFound);
    }

    public IReadOnlyCollection<Session> GetAll()
    {
        return _store.Values.ToList();
    }

    public void Save(Session session)
    {
        _store[session.Id] = session;
    }

    public void Delete(SessionId id)
    {
        _store.TryRemove(id, out _);
    }
}
