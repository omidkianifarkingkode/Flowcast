using Application.Abstractions.Messaging;
using Domain.Sessions;
using SharedKernel;

namespace Application.Sessions.Queries;

public record GetSessionsForPlayerQuery(long PlayerId) : IQuery<List<Session>>;

public sealed class GetSessionsForPlayerHandler(ISessionRepository repo)
    : IQueryHandler<GetSessionsForPlayerQuery, List<Session>>
{
    public Task<Result<List<Session>>> Handle(GetSessionsForPlayerQuery request, CancellationToken ct)
    {
        var sessions = repo.GetAll()
            .Where(s => s.Players.Any(p => p.PlayerId == request.PlayerId))
            .ToList();

        return Task.FromResult(Result.Success(sessions));
    }
}
