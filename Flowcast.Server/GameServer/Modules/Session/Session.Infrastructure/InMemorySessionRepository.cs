using Session.Contracts;
using Session.Domain;
using SharedKernel;
using SharedKernel.Primitives;
using System.Collections.Concurrent;

namespace Session.Infrastructure;

public sealed class InMemorySessionRepository : ISessionRepository
{
    private readonly ConcurrentDictionary<SessionId, SessionEntity> _store = new();
    // Index: a player can be in at most one *active* session (Waiting/InProgress)
    private readonly ConcurrentDictionary<PlayerId, SessionId> _activeByPlayer = new();
    private readonly object _gate = new(); // to make multi-dict updates atomic

    public Task<Result<SessionEntity>> GetById(SessionId id, CancellationToken ct = default)
    {
        if (_store.TryGetValue(id, out var session))
            return Task.FromResult(Result.Success(session));

        return Task.FromResult(Result.Failure<SessionEntity>(SessionErrors.SessionNotFound));
    }

    public Task<Result<IReadOnlyList<SessionEntity>>> GetAll(CancellationToken ct = default)
    {
        // Snapshot to avoid enumeration over a changing dictionary
        var list = _store.Values.ToArray();
        return Task.FromResult(Result.Success((IReadOnlyList<SessionEntity>)list));
    }

    public Task<Result<SessionEntity?>> GetActiveByPlayer(PlayerId playerId, CancellationToken ct = default)
    {
        if (_activeByPlayer.TryGetValue(playerId, out var sid) &&
            _store.TryGetValue(sid, out var session) &&
            IsActive(session))
        {
            return Task.FromResult(Result.Success<SessionEntity?>(session));
        }
        return Task.FromResult(Result.Success<SessionEntity?>(null));
    }

    public Task Save(SessionEntity session, CancellationToken ct = default)
    {
        // Reindex atomically against the previous snapshot if it existed
        lock (_gate)
        {
            // Grab previous (if any) to diff participants
            _store.TryGetValue(session.Id, out var previous);
            _store[session.Id] = session;

            // Remove previous index entries for this session
            if (previous is not null)
            {
                foreach (var p in previous.Participants)
                {
                    // Only remove if it pointed to this session (avoid clobbering if player was moved elsewhere)
                    if (_activeByPlayer.TryGetValue(p.Id, out var mapped) && mapped.Equals(previous.Id))
                        _activeByPlayer.TryRemove(p.Id, out _);
                }
            }

            // Add new index entries if the session is active
            if (IsActive(session))
            {
                foreach (var p in session.Participants)
                {
                    _activeByPlayer[p.Id] = session.Id;
                }
            }
        }

        return Task.CompletedTask;
    }

    public Task Delete(SessionId id, CancellationToken ct = default)
    {
        lock (_gate)
        {
            if (_store.TryRemove(id, out var removed))
            {
                // Clean any player→session mappings that pointed to this session
                foreach (var p in removed.Participants)
                {
                    if (_activeByPlayer.TryGetValue(p.Id, out var mapped) && mapped.Equals(id))
                        _activeByPlayer.TryRemove(p.Id, out _);
                }
            }
        }
        return Task.CompletedTask;
    }

    private static bool IsActive(SessionEntity s)
        => s.Status is SessionStatus.Waiting or SessionStatus.InProgress;
}
