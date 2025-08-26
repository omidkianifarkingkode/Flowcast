using Domain.Matchmaking;
using SharedKernel;
using System.Collections.Concurrent;

namespace Infrastructure.Persistence.Matchmaking.Services;

public sealed class InMemoryMatchRepository : IMatchRepository
{
    private readonly ConcurrentDictionary<MatchId, Match> _store = new();

    public Task<Result<Match>> GetById(MatchId id, CancellationToken ct = default)
    {
        if (_store.TryGetValue(id, out var match))
            return Task.FromResult(Result.Success(match));

        return Task.FromResult(Result.Failure<Match>(MatchErrors.NotFound));
    }

    public Task Save(Match match, CancellationToken ct = default)
    {
        _store[match.Id] = match;
        return Task.CompletedTask;
    }

    public Task Delete(MatchId id, CancellationToken ct = default)
    {
        _store.TryRemove(id, out _);
        return Task.CompletedTask;
    }
}
