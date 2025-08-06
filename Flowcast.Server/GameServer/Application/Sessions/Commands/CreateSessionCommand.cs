using Application.Abstractions.Messaging;
using Domain.Sessions;
using Domain.Sessions.Entities;
using Domain.Sessions.Services;
using Domain.Sessions.ValueObjects;
using SharedKernel;

namespace Application.Sessions.Commands;

public record CreateSessionCommand(List<CreateSessionCommand.PlayerInfo> Players, string Mode, MatchSettings? MatchSettings)
    : ICommand<SessionId>
{
    public record PlayerInfo(Guid Id, string DisplayName);
}

public sealed class CreateSessionHandler(ISessionRepository sessionRepository, IDateTimeProvider dateTimeProvider)
    : ICommandHandler<CreateSessionCommand, SessionId>
{
    public async Task<Result<SessionId>> Handle(CreateSessionCommand command, CancellationToken ct)
    {
        var session = Session.Create(command.Mode, dateTimeProvider.UtcNow, command.MatchSettings);

        foreach (var player in command.Players)
        {
            var newPlayer = new Player(new PlayerId(player.Id), player.DisplayName);
            var result = session.JoinPlayer(newPlayer);

            if (result.IsFailure)
                return Result.Failure<SessionId>(result.Error);
        }

        // No auto-start here — wait for players to signal readiness

        await sessionRepository.Save(session, ct);

        return session.Id;
    }
}


