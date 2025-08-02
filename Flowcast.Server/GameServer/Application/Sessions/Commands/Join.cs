using Domain.Entities;
using Domain.Sessions;
using MediatR;
using SharedKernel;

namespace Application.Sessions.Commands;

public record JoinSessionCommand(SessionId SessionId, long PlayerId, string DisplayName) 
    : IRequest<Result>;

public sealed class JoinSessionHandler(SessionAppService service) : IRequestHandler<JoinSessionCommand, Result>
{
    public Task<Result> Handle(JoinSessionCommand request, CancellationToken cancellationToken)
    {
        var player = new Player(request.PlayerId, request.DisplayName);

        service.JoinSession(request.SessionId, player);

        return Task.FromResult(Result.Success());
    }
}



