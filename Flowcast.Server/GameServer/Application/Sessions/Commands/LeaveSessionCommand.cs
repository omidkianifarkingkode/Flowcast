using Application.Abstractions.Messaging;
using Domain.Sessions;
using SharedKernel;

namespace Application.Sessions.Commands;

public record LeaveSessionCommand(SessionId SessionId, PlayerId PlayerId) : ICommand<LeaveSessionResult>;

public record LeaveSessionResult(Session Session, Participant Participant, bool WasLastPlayer);

public sealed class LeaveSessionHandler(ISessionRepository sessionRepository, IDateTimeProvider clock)
    : ICommandHandler<LeaveSessionCommand, LeaveSessionResult>
{
    public async Task<Result<LeaveSessionResult>> Handle(LeaveSessionCommand command, CancellationToken cancellationToken)
    {
        var sessionResult = await sessionRepository.GetById(command.SessionId, cancellationToken);
        if (sessionResult.IsFailure)
            return Result.Failure<LeaveSessionResult>(sessionResult.Error);

        var session = sessionResult.Value;

        var snapshot = session.Participants.FirstOrDefault(p => p.Id == command.PlayerId);
        if (snapshot is null)
            return Result.Failure<LeaveSessionResult>(SessionErrors.ParticipantMissing);

        var removeResult = session.RemoveParticipant(command.PlayerId, clock.UtcNow);

        if (removeResult.IsFailure)
            return Result.Failure<LeaveSessionResult>(removeResult.Error);

        await sessionRepository.Save(session, cancellationToken);

        var wasLastPlayer = session.Participants.Count == 0;

        var left = new Participant(snapshot.Id, snapshot.DisplayName);

        return Result.Success(new LeaveSessionResult(session, left, wasLastPlayer));
    }
}
