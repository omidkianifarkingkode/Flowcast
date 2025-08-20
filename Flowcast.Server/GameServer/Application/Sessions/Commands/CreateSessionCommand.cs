using Application.Abstractions.Messaging;
using Domain.Sessions;
using SharedKernel;

namespace Application.Sessions.Commands;

public record CreateSessionCommand(
    List<CreateSessionCommand.PlayerInfo> Players,
    string Mode,
    MatchSettings? MatchSettings,
    string StartBarrier = "ConnectedAndLoaded",
    int? JoinDeadlineSeconds = 15)
    : ICommand<SessionId>
{
    public record PlayerInfo(Guid Id, string DisplayName);
}

public sealed class CreateSessionHandler(ISessionRepository sessionRepository, IDateTimeProvider clock)
    : ICommandHandler<CreateSessionCommand, SessionId>
{
    public async Task<Result<SessionId>> Handle(CreateSessionCommand command, CancellationToken cancellationToken)
    {
        var barrier = Enum.Parse<StartBarrier>(command.StartBarrier, ignoreCase: true);
        DateTime? joinDeadline = barrier == StartBarrier.Timer || barrier == StartBarrier.ConnectedOnly || barrier == StartBarrier.ConnectedAndLoaded
            ? (command.JoinDeadlineSeconds is int s ? clock.UtcNow.AddSeconds(s) : clock.UtcNow.AddSeconds(15)) 
            : null;

        var session = Session.Create(command.Mode, barrier, clock.UtcNow, joinDeadline, command.MatchSettings);

        foreach (var player in command.Players)
        {
            var participant = new Participant(new PlayerId(player.Id), player.DisplayName);
            var result = session.JoinParticipant(participant, clock.UtcNow);

            if (result.IsFailure)
                return Result.Failure<SessionId>(result.Error);
        }

        await sessionRepository.Save(session, cancellationToken);

        return session.Id;
    }
}


