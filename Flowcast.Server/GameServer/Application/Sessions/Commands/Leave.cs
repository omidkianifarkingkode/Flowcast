using Domain.Sessions;
using MediatR;
using SharedKernel;

namespace Application.Sessions.Commands;

public record LeaveSessionCommand(SessionId SessionId, long PlayerId) : IRequest<Result>;

public sealed class LeaveSessionHandler(ISessionRepository repo)
    : IRequestHandler<LeaveSessionCommand, Result>
{
    public Task<Result> Handle(LeaveSessionCommand request, CancellationToken ct)
    {
        var result = repo.GetById(request.SessionId);

        if (result.IsFailure)
            return Task.FromResult(Result.Failure(result.Error));

        var session = result.Value;

        var removeResult = session.RemovePlayer(request.PlayerId);

        if (removeResult.IsSuccess)
            repo.Save(session);

        return Task.FromResult(removeResult);
    }
}
