using Session.Contracts;
using Session.Domain;
using Shared.Application.Messaging;
using Shared.Application.Services;
using SharedKernel;

namespace Session.Application.Commands;

public record AbortSessionBeforeStartCommand(SessionId SessionId, string Reason = "NoShow") : ICommand<SessionEntity>;

public sealed class AbortSessionBeforeStartHandler(ISessionRepository repo, IDateTimeProvider clock)
  : ICommandHandler<AbortSessionBeforeStartCommand, SessionEntity>
{
    public async Task<Result<SessionEntity>> Handle(AbortSessionBeforeStartCommand cmd, CancellationToken ct)
    {
        var res = await repo.GetById(cmd.SessionId, ct);
        if (res.IsFailure) return Result.Failure<SessionEntity>(res.Error);
        var s = res.Value;

        var reason = Enum.TryParse<SessionCloseReason>(cmd.Reason, true, out var r0) ? r0 : SessionCloseReason.NoShow;
        var r = s.AbortBeforeStart(clock.UtcNow, reason);
        if (r.IsFailure) return Result.Failure<SessionEntity>(r.Error);

        await repo.Save(s, ct);
        return s;
    }
}