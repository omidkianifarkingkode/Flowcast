using Domain.Matchmaking;
using Domain.Sessions;
using SharedKernel;
using System.Collections.Concurrent;

namespace Infrastructure.Persistence.Matchmaking.Services;

public sealed class InMemoryTicketRepository : ITicketRepository
{
    private readonly ConcurrentDictionary<TicketId, Ticket> _store = new();
    private readonly ConcurrentDictionary<(PlayerId, string), TicketId> _openByPlayerMode = new(); // (player,mode) -> open ticket id
    private readonly object _gate = new();

    public Task<Result<Ticket>> GetById(TicketId id, CancellationToken ct = default)
    {
        if (_store.TryGetValue(id, out var ticket))
            return Task.FromResult(Result.Success(ticket));

        return Task.FromResult(Result.Failure<Ticket>(TicketErrors.NotFound));
    }

    public Task<Result<Ticket>> GetOpenByPlayer(PlayerId playerId, string mode, CancellationToken ct = default)
    {
        if (_openByPlayerMode.TryGetValue((playerId, mode), out var id) &&
            _store.TryGetValue(id, out var ticket) &&
            IsOpen(ticket))
        {
            return Task.FromResult(Result.Success(ticket));
        }

        return Task.FromResult(Result.Failure<Ticket>(TicketErrors.NotFound));
    }

    public Task<Result<IReadOnlyList<Ticket>>> GetSearchingByMode(string mode, CancellationToken ct = default)
    {
        // Snapshot + filter for Searching; order by enqueue time (FIFO)
        var list = _store.Values
            .Where(t => t.Mode == mode && t.State == TicketState.Searching)
            .OrderBy(t => t.EnqueuedAtUtc)
            .ToArray();

        return Task.FromResult(Result.Success((IReadOnlyList<Ticket>)list));
    }

    public Task Save(Ticket ticket, CancellationToken ct = default)
    {
        lock (_gate)
        {
            _store[ticket.Id] = ticket;

            var key = (ticket.PlayerId, ticket.Mode);
            // Update index according to open/closed
            if (IsOpen(ticket))
                _openByPlayerMode[key] = ticket.Id;
            else
                _openByPlayerMode.TryRemove(key, out _);
        }

        return Task.CompletedTask;
    }

    public Task Delete(TicketId id, CancellationToken ct = default)
    {
        lock (_gate)
        {
            if (_store.TryRemove(id, out var removed))
            {
                var key = (removed.PlayerId, removed.Mode);
                if (_openByPlayerMode.TryGetValue(key, out var mapped) && mapped.Equals(id))
                    _openByPlayerMode.TryRemove(key, out _);
            }
        }

        return Task.CompletedTask;
    }

    private static bool IsOpen(Ticket t) => t.State is TicketState.Searching or TicketState.PendingReady;
}
