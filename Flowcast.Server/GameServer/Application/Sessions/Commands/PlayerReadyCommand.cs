using Application.Abstractions.Messaging;
using Application.Services;
using Domain.Sessions;
using Domain.Sessions.Entities;
using Domain.Sessions.Errors;
using Domain.Sessions.Services;
using Domain.Sessions.ValueObjects;
using SharedKernel;

namespace Application.Sessions.Commands;

public record PlayerReadyCommand(SessionId SessionId, PlayerId PlayerId) : ICommand<PlayerReadyResult>;

public record PlayerReadyResult(Session Session, Player Player, bool AllPlayerReady);

public sealed class PlayerReadyHandler(ISessionRepository sessionRepository, ConnectedPlayersRegistry playerRegistry, IDateTimeProvider dateTimeProvider) 
    : ICommandHandler<PlayerReadyCommand, PlayerReadyResult>
{
    public async Task<Result<PlayerReadyResult>> Handle(PlayerReadyCommand command, CancellationToken cancellationToken)
    {
        if (!playerRegistry.TryGet(command.PlayerId.Value, out var registerdPlayer))
            return Result.Failure<PlayerReadyResult>(SessionErrors.PlayerNotFound);

        var sessionResult = await sessionRepository.GetById(command.SessionId, cancellationToken);
        if (sessionResult.IsFailure)
            return Result.Failure<PlayerReadyResult>(sessionResult.Error);

        var session = sessionResult.Value;

        var playerToReady = new Player(command.PlayerId, registerdPlayer.DisplayName);

        var readyResult = session.MarkPlayerReady(playerToReady, dateTimeProvider.UtcNow);
        if (readyResult.IsFailure)
            return Result.Failure<PlayerReadyResult>(readyResult.Error);

        await sessionRepository.Save(session, cancellationToken);

        var allPlayerReady = readyResult.Value;

        return Result.Success(new PlayerReadyResult(session, playerToReady, allPlayerReady));
    }
}


