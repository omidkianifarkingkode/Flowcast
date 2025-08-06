using Application.Abstractions.Messaging;
using Domain.Sessions;
using Domain.Sessions.Services;
using Domain.Sessions.ValueObjects;
using SharedKernel;

namespace Application.Sessions.Queries;

public record GetSessionByIdQuery(SessionId SessionId) : IQuery<Session>;

public sealed class GetSessionHandler(ISessionRepository sessionRepository) : IQueryHandler<GetSessionByIdQuery, Session>
{
    public async Task<Result<Session>> Handle(GetSessionByIdQuery query, CancellationToken cancellationToken)
    {
        var result = await sessionRepository.GetById(query.SessionId, cancellationToken);

        if (result.IsFailure)
            return Result.Failure<Session>(result.Error);

        return result;
    }
}
