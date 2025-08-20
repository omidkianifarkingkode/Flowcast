using Application.Abstractions.Messaging;
using Domain.Sessions;
using SharedKernel;

namespace Application.Sessions.Commands;

public record MarkParticipantLoadedCommand(SessionId SessionId, PlayerId PlayerId) : ICommand<Session>;

public sealed class MarkParticipantLoadedHandler(ISessionRepository sessionRepository, IDateTimeProvider clock)
  : ICommandHandler<MarkParticipantLoadedCommand, Session>
{
    public async Task<Result<Session>> Handle(MarkParticipantLoadedCommand command, CancellationToken cancellationToken)
    {
        var res = await sessionRepository.GetById(command.SessionId, cancellationToken);
        if (res.IsFailure) return Result.Failure<Session>(res.Error);
        var s = res.Value;

        var r = s.MarkParticipantLoaded(command.PlayerId, clock.UtcNow);
        if (r.IsFailure) return Result.Failure<Session>(r.Error);

        await sessionRepository.Save(s, cancellationToken);
        return s;
    }
}