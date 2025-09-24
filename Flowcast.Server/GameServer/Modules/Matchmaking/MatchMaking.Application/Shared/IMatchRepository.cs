using Matchmaking.Domain;
using SharedKernel;

namespace MatchMaking.Application.Shared;

public interface IMatchRepository
{
    Task<Result<Match>> GetById(MatchId id, CancellationToken ct = default);
    Task Save(Match match, CancellationToken ct = default);
    Task Delete(MatchId id, CancellationToken ct = default);
}