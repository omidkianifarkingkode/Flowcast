using Application.Abstractions.Messaging;
using Domain.Sessions;
using SharedKernel;

namespace Application.Sessions.Queries;

public record GetSessionQuery(SessionId SessionId) : IQuery<Session>;

public sealed class GetSessionHandler(ISessionRepository repo) : IQueryHandler<GetSessionQuery, Session>
{
    public Task<Result<Session>> Handle(GetSessionQuery request, CancellationToken ct)
    {
        var result = repo.GetById(request.SessionId);

        return result.IsSuccess
            ? Task.FromResult(Result.Success(result.Value))
            : Task.FromResult(Result.Failure<Session>(result.Error));
    }
}
