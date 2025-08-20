using Application.Abstractions.Messaging;
using Domain.Sessions;
using SharedKernel;

namespace Application.Sessions.Queries;

public record GetSessionByPlayerQuery(PlayerId PlayerId) : IQuery<Session>;

public sealed class GetSessionsForPlayerHandler(ISessionRepository repo)
    : IQueryHandler<GetSessionByPlayerQuery, Session>
{
    public async Task<Result<Session>> Handle(GetSessionByPlayerQuery query, CancellationToken ct)
    {
        // Try optimized path if implemented
        var active = await repo.GetActiveByPlayer(query.PlayerId, ct);

        if (active.IsSuccess && active.Value is { } seassion)
            return seassion;

        if (active.IsFailure && active.Error is not null)
        {
            if (!string.Equals(active.Error.Code, "not_implemented", StringComparison.OrdinalIgnoreCase))
                return Result.Failure<Session>(active.Error);
        }

        // Fallback scan
        var all = await repo.GetAll(ct);
        if (all.IsFailure) return Result.Failure<Session>(all.Error);

        var found = all.Value.FirstOrDefault(s =>
            s.Status is SessionStatus.Waiting or SessionStatus.InProgress
            && s.Participants.Any(p => p.Id == query.PlayerId));

        return found is null
            ? Result.Failure<Session>(SessionErrors.SessionNotFound)
            : Result.Success(found);
    }
}
