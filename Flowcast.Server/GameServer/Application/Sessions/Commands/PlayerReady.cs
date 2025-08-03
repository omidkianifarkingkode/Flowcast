using Application.Abstractions.Messaging;
using Domain.Sessions;
using SharedKernel;

namespace Application.Sessions.Commands;

public record PlayerReadyCommand(SessionId SessionId, long PlayerId) : ICommand;

public sealed class PlayerReadyHandler(ISessionRepository sessionRepository) : ICommandHandler<PlayerReadyCommand>
{
    public Task<Result> Handle(PlayerReadyCommand request, CancellationToken cancellationToken)
    {
        var getResult = sessionRepository.GetById(request.SessionId);

        if (getResult.IsFailure)
            return Task.FromResult(Result.Failure(getResult.Error));

        var session = getResult.Value;

        var result = session.MarkPlayerReady(request.PlayerId);

        if (result.IsSuccess)
        {
            sessionRepository.Save(session);
        }

        return Task.FromResult(result);
    }
}


