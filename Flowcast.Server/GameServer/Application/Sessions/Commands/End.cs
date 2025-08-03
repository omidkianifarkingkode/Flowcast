using Application.Abstractions.Messaging;
using Domain.Sessions;
using SharedKernel;

namespace Application.Sessions.Commands;

public record EndSessionCommand(SessionId SessionId) : ICommand;

public sealed class EndSessionHandler(SessionAppService service) : ICommandHandler<EndSessionCommand>
{
    public Task<Result> Handle(EndSessionCommand request, CancellationToken cancellationToken)
    {
        service.EndSession(request.SessionId);

        return Task.FromResult(Result.Success());
    }
}


