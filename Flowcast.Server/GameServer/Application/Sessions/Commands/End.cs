using Domain.Sessions;
using MediatR;
using SharedKernel;

namespace Application.Sessions.Commands;

public record EndSessionCommand(SessionId SessionId) : IRequest<Result>;

public sealed class EndSessionHandler(SessionAppService service) : IRequestHandler<EndSessionCommand, Result>
{
    public Task<Result> Handle(EndSessionCommand request, CancellationToken cancellationToken)
    {
        service.EndSession(request.SessionId);

        return Task.FromResult(Result.Success());
    }
}


