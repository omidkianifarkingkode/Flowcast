using Application.Abstractions.Messaging;
using Domain.Sessions;
using Domain.Sessions.Services;
using Domain.Sessions.ValueObjects;
using SharedKernel;

namespace Application.Sessions.Commands;

public record EndSessionCommand(SessionId SessionId) : ICommand<Session>;

public sealed class EndSessionHandler(ISessionRepository sessionRepository, IDateTimeProvider dateTimeProvider) 
    : ICommandHandler<EndSessionCommand, Session>
{
    public async Task<Result<Session>> Handle(EndSessionCommand command, CancellationToken cancellationToken)
    {
        var getResult = await sessionRepository.GetById(command.SessionId, cancellationToken);
        if (getResult.IsFailure)
            return Result.Failure<Session>(getResult.Error);

        var session = getResult.Value;
        session.End(dateTimeProvider.UtcNow);

        await sessionRepository.Save(session, cancellationToken);

        return session;
    }
}


