using Application.Abstractions.Messaging;
using Domain.Sessions;
using SharedKernel;

namespace Application.Sessions.Commands;

public record AbortSessionBeforeStartCommand(SessionId SessionId, string Reason = "NoShow") : ICommand<Session>;

public sealed class AbortSessionBeforeStartHandler(ISessionRepository repo, IDateTimeProvider clock)
  : ICommandHandler<AbortSessionBeforeStartCommand, Session>
{
    public async Task<Result<Session>> Handle(AbortSessionBeforeStartCommand cmd, CancellationToken ct)
    {
        var res = await repo.GetById(cmd.SessionId, ct);
        if (res.IsFailure) return Result.Failure<Session>(res.Error);
        var s = res.Value;

        var reason = Enum.TryParse<SessionCloseReason>(cmd.Reason, true, out var r0) ? r0 : SessionCloseReason.NoShow;
        var r = s.AbortBeforeStart(clock.UtcNow, reason);
        if (r.IsFailure) return Result.Failure<Session>(r.Error);

        await repo.Save(s, ct);
        return s;
    }
}