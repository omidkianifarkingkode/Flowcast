using Matchmaking.Domain;
using MatchMaking.Application.Shared;
using Shared.Application.Services;
using SharedKernel;
using System.Collections.Concurrent;

namespace Matchmaking.Infrastructure;

public sealed class InMemoryMatchRepository(IDomainEventsDispatcher dispatcher) : IMatchRepository
{
    private readonly ConcurrentDictionary<MatchId, Match> _store = new();

    public Task<Result<Match>> GetById(MatchId id, CancellationToken ct = default)
    {
        if (_store.TryGetValue(id, out var match))
            return Task.FromResult(Result.Success(match));

        return Task.FromResult(Result.Failure<Match>(MatchErrors.NotFound));
    }

    public async Task Save(Match match, CancellationToken ct = default)
    {
        _store[match.Id] = match;

        var events = match.DomainEvents.ToArray();
        if (events.Length > 0)
        {
            await dispatcher.DispatchAsync(events, ct);
            match.ClearDomainEvents();
        }
    }

    public Task Delete(MatchId id, CancellationToken ct = default)
    {
        _store.TryRemove(id, out _);
        return Task.CompletedTask;
    }
}
