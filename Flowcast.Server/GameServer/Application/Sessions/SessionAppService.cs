using Domain.Entities;
using Domain.Sessions;
using Domain.ValueObjects;
using SharedKernel;
using static Domain.Sessions.SessionId;

namespace Application.Sessions;

public class SessionAppService
{
    private readonly ISessionRepository _sessionRepo;
    private readonly IDateTimeProvider _dateTimeProvider;

    public SessionAppService(ISessionRepository sessionRepo, IDateTimeProvider dateTimeProvider)
    {
        _sessionRepo = sessionRepo;
        _dateTimeProvider = dateTimeProvider;
    }

    public Result<SessionId> CreateSession(string mode, MatchSettings settings)
    {
        var session = new Session(NewId(), mode, settings);
        _sessionRepo.Save(session);
        return Result.Success(session.Id);
    }

    public Result JoinSession(SessionId sessionId, Player player)
    {
        var getResult = _sessionRepo.GetById(sessionId);
        if (getResult.IsFailure)
            return Result.Failure(getResult.Error);

        var session = getResult.Value;
        var result = session.AddPlayer(player);

        if (result.IsSuccess)
            _sessionRepo.Save(session);

        return result;
    }

    public Result LeaveSession(SessionId sessionId, long playerId)
    {
        var getResult = _sessionRepo.GetById(sessionId);
        if (getResult.IsFailure)
            return Result.Failure(getResult.Error);

        var session = getResult.Value;
        var result = session.RemovePlayer(playerId);

        if (result.IsSuccess)
            _sessionRepo.Save(session);

        return result;
    }

    public Result StartSession(SessionId sessionId)
    {
        var getResult = _sessionRepo.GetById(sessionId);
        if (getResult.IsFailure)
            return Result.Failure(getResult.Error);

        var session = getResult.Value;
        var result = session.Start();

        if (result.IsSuccess)
            _sessionRepo.Save(session);

        return result;
    }

    public Result EndSession(SessionId sessionId)
    {
        var getResult = _sessionRepo.GetById(sessionId);
        if (getResult.IsFailure)
            return Result.Failure(getResult.Error);

        var session = getResult.Value;
        session.End(_dateTimeProvider.UtcNow);

        _sessionRepo.Save(session);
        return Result.Success();
    }
}
