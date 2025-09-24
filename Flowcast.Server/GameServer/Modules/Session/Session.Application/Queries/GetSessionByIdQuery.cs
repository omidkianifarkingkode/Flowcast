using Session.Contracts;
using Session.Domain;
using Shared.Application.Messaging;
using SharedKernel;

namespace Session.Application.Queries;

public record GetSessionByIdQuery(SessionId SessionId) : IQuery<SessionEntity>;

public sealed class GetSessionHandler(ISessionRepository sessionRepository) : IQueryHandler<GetSessionByIdQuery, SessionEntity>
{
    public async Task<Result<SessionEntity>> Handle(GetSessionByIdQuery query, CancellationToken cancellationToken)
    {
        var result = await sessionRepository.GetById(query.SessionId, cancellationToken);

        if (result.IsFailure)
            return Result.Failure<SessionEntity>(result.Error);

        return result;
    }
}
