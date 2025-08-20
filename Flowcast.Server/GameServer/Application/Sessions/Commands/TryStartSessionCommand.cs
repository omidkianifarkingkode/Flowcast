using Application.Abstractions.Messaging;
using Domain.Sessions;
using SharedKernel;

namespace Application.Sessions.Commands;

public record TryStartSessionCommand(SessionId SessionId) : ICommand<Session>;

public sealed class TryStartSessionHandler(ISessionRepository repo, IDateTimeProvider clock)
  : ICommandHandler<TryStartSessionCommand, Session>
{
    public async Task<Result<Session>> Handle(TryStartSessionCommand cmd, CancellationToken ct)
    {
        var res = await repo.GetById(cmd.SessionId, ct);
        if (res.IsFailure) return Result.Failure<Session>(res.Error);
        var s = res.Value;

        var r = s.TryStart(clock.UtcNow);
        if (r.IsFailure) return Result.Failure<Session>(r.Error);

        if (s.Status == SessionStatus.InProgress) // only write if changed
            await repo.Save(s, ct);

        return s;
    }
}