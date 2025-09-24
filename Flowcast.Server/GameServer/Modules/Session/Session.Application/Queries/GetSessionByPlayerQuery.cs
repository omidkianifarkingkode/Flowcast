using Session.Contracts;
using Session.Domain;
using Shared.Application.Messaging;
using SharedKernel;
using SharedKernel.Primitives;

namespace Session.Application.Queries;

public record GetSessionByPlayerQuery(PlayerId PlayerId) : IQuery<SessionEntity>;

public sealed class GetSessionsForPlayerHandler(ISessionRepository repo)
    : IQueryHandler<GetSessionByPlayerQuery, SessionEntity>
{
    public async Task<Result<SessionEntity>> Handle(GetSessionByPlayerQuery query, CancellationToken ct)
    {
        // Try optimized path if implemented
        var active = await repo.GetActiveByPlayer(query.PlayerId, ct);

        if (active.IsSuccess && active.Value is { } seassion)
            return seassion;

        if (active.IsFailure && active.Error is not null)
        {
            if (!string.Equals(active.Error.Code, "not_implemented", StringComparison.OrdinalIgnoreCase))
                return Result.Failure<SessionEntity>(active.Error);
        }

        // Fallback scan
        var all = await repo.GetAll(ct);
        if (all.IsFailure) return Result.Failure<SessionEntity>(all.Error);

        var found = all.Value.FirstOrDefault(s =>
            s.Status is SessionStatus.Waiting or SessionStatus.InProgress
            && s.Participants.Any(p => p.Id == query.PlayerId));

        return found is null
            ? Result.Failure<SessionEntity>(SessionErrors.SessionNotFound)
            : Result.Success(found);
    }
}
