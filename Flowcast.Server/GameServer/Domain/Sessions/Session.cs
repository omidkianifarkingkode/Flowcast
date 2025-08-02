using Domain.Entities;
using Domain.Players;
using Domain.ValueObjects;
using SharedKernel;

namespace Domain.Sessions;

public class Session : Entity
{
    public SessionId Id { get; private set; }
    public SessionStatus Status { get; private set; }

    private readonly List<Player> _players = new();
    public IReadOnlyList<Player> Players => _players.AsReadOnly();

    public string Mode { get; private set; }
    public MatchSettings? Settings { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? StartedAtUtc { get; private set; }

    public int CurrentTick { get; private set; }

    private Session() { }

    public Session(SessionId id, string mode, MatchSettings? settings = null)
    {
        Id = id;
        Mode = mode;
        Settings = settings;
        Status = SessionStatus.Waiting;
        CreatedAtUtc = DateTime.UtcNow;
        CurrentTick = 0;
    }

    public Result AddPlayer(Player player)
    {
        if (Status != SessionStatus.Waiting)
            return Result.Failure(SessionErrors.SessionAlreadyStarted);

        if (_players.Any(p => p.PlayerId == player.PlayerId))
            return Result.Failure(SessionErrors.PlayerAlreadyInSession);

        _players.Add(player);

        Raise(new PlayerJoinedSession(Id, player.PlayerId));

        return Result.Success();
    }

    public Result RemovePlayer(long playerId)
    {
        var removed = _players.RemoveAll(p => p.PlayerId == playerId);
        if (removed == 0)
            return Result.Failure(SessionErrors.PlayerNotFound);

        Raise(new PlayerLeftSession(Id, playerId));

        return Result.Success();
    }

    public Result DisconnectPlayer(long playerId)
    {
        var player = _players.FirstOrDefault(p => p.PlayerId == playerId);
        if (player is null)
            return Result.Failure(SessionErrors.PlayerNotFound);

        player.SetStatus(PlayerStatus.Disconnected);
        Raise(new PlayerLeftSession(Id, playerId)); // or define a separate event if needed

        return Result.Success();
    }

    public Result MarkPlayerReady(long playerId)
    {
        var player = _players.FirstOrDefault(p => p.PlayerId == playerId);
        if (player is null)
            return Result.Failure(SessionErrors.PlayerNotFound);

        player.MarkReady();
        Raise(new PlayerMarkedReady(Id, playerId));

        if (_players.All(p => p.Status == PlayerStatus.Ready))
        {
            Start();
        }

        return Result.Success();
    }


    public Result Start()
    {
        if (Status != SessionStatus.Waiting)
            return Result.Failure(SessionErrors.SessionAlreadyStarted);

        Status = SessionStatus.InProgress;
        StartedAtUtc = DateTime.UtcNow;

        Raise(new SessionCreated(Id, Mode, CreatedAtUtc));

        return Result.Success();
    }

    public Result End(DateTime endedAtUtc)
    {
        Status = SessionStatus.Ended;

        Raise(new SessionEnded(Id, endedAtUtc));

        return Result.Success();
    }

    public void TickForward()
    {
        CurrentTick += 1;
    }
}
