using Application.Sessions.Shared;
using Domain.Sessions;
using MediatR;
using SharedKernel;

namespace Application.Sessions.Queries;

public record GetSessionsForPlayerQuery(long PlayerId) : IRequest<Result<List<SessionSummaryDto>>>;

public sealed class GetSessionsForPlayerHandler(ISessionRepository repo)
    : IRequestHandler<GetSessionsForPlayerQuery, Result<List<SessionSummaryDto>>>
{
    public Task<Result<List<SessionSummaryDto>>> Handle(GetSessionsForPlayerQuery request, CancellationToken ct)
    {
        var all = repo.GetAll(); // You’d need to expose this
        var sessions = all
            .Where(s => s.Players.Any(p => p.PlayerId == request.PlayerId))
            .Select(s => new SessionSummaryDto(
                s.Id.Value,
                s.Mode,
                s.Status,
                s.Players.Count,
                s.CreatedAtUtc
            ))
            .ToList();

        return Task.FromResult(Result.Success(sessions));
    }
}
