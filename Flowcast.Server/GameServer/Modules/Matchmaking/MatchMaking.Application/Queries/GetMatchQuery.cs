using Matchmaking.Domain;
using MatchMaking.Application.Shared;
using Shared.Application.Messaging;
using SharedKernel;

namespace MatchMaking.Application.Queries;

public record GetMatchQuery(MatchId MatchId) : IQuery<Match>;

public sealed class GetMatchHandler(IMatchRepository repo) : IQueryHandler<GetMatchQuery, Match>
{
    public async Task<Result<Match>> Handle(GetMatchQuery query, CancellationToken ct)
        => await repo.GetById(query.MatchId, ct);
}