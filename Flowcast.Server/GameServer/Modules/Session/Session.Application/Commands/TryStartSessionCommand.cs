using Session.Contracts;
using Session.Domain;
using Shared.Application.Messaging;
using Shared.Application.Services;
using SharedKernel;

namespace Session.Application.Commands;

public record TryStartSessionCommand(SessionId SessionId) : ICommand<SessionEntity>;

public sealed class TryStartSessionHandler(ISessionRepository repo, IDateTimeProvider clock)
  : ICommandHandler<TryStartSessionCommand, SessionEntity>
{
    public async Task<Result<SessionEntity>> Handle(TryStartSessionCommand cmd, CancellationToken ct)
    {
        var res = await repo.GetById(cmd.SessionId, ct);
        if (res.IsFailure) return Result.Failure<SessionEntity>(res.Error);
        var s = res.Value;

        var r = s.TryStart(clock.UtcNow);
        if (r.IsFailure) return Result.Failure<SessionEntity>(r.Error);

        if (s.Status == SessionStatus.InProgress) // only write if changed
            await repo.Save(s, ct);

        return s;
    }
}