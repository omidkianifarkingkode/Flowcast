using Session.Contracts;
using Session.Domain;
using Shared.Application.Messaging;
using Shared.Application.Services;
using SharedKernel;
using SharedKernel.Primitives;

namespace Session.Application.Commands;

public record MarkParticipantLoadedCommand(SessionId SessionId, PlayerId PlayerId) : ICommand<SessionEntity>;

public sealed class MarkParticipantLoadedHandler(ISessionRepository sessionRepository, IDateTimeProvider clock)
  : ICommandHandler<MarkParticipantLoadedCommand, SessionEntity>
{
    public async Task<Result<SessionEntity>> Handle(MarkParticipantLoadedCommand command, CancellationToken cancellationToken)
    {
        var res = await sessionRepository.GetById(command.SessionId, cancellationToken);
        if (res.IsFailure) return Result.Failure<SessionEntity>(res.Error);
        var s = res.Value;

        var r = s.MarkParticipantLoaded(command.PlayerId, clock.UtcNow);
        if (r.IsFailure) return Result.Failure<SessionEntity>(r.Error);

        await sessionRepository.Save(s, cancellationToken);
        return s;
    }
}