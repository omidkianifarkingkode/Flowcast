using Application.Abstractions.Messaging;
using Application.Services;
using Domain.Sessions;
using Domain.Sessions.ValueObjects;
using SharedKernel;

namespace Application.Sessions.Queries;

public record GetSessionByPlayerQuery(PlayerId PlayerId) : IQuery<Session>;

public sealed class GetSessionsForPlayerHandler(ISessionRepository sessionRepository, ConnectedPlayersRegistry playerRegistry)
    : IQueryHandler<GetSessionByPlayerQuery, Session>
{
    public async Task<Result<Session>> Handle(GetSessionByPlayerQuery query, CancellationToken cancellationToken)
    {
        if (!playerRegistry.TryGet(query.PlayerId.Value, out var registerdPlayer))
            return Result.Failure<Session>(SessionErrors.PlayerNotFound);

        var session = (await sessionRepository.GetAll(cancellationToken))
            .FirstOrDefault(s => s.Players.Any(p => p.Id == query.PlayerId));

        if (session is null)
            return Result.Failure<Session>(SessionErrors.SessionNotFound);

        return session;
    }
}
