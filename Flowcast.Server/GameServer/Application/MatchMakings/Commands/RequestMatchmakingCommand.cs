using Application.Abstractions.Messaging;
using Domain.Sessions.ValueObjects;
using SharedKernel;

namespace Application.MatchMakings.Commands;

public record RequestMatchmakingCommand(PlayerId PlayerId) : ICommand;

public sealed class RequestMatchmakingCommandHandler : ICommandHandler<RequestMatchmakingCommand>
{
    public Task<Result> Handle(RequestMatchmakingCommand command, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
