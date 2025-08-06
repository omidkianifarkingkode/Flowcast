using Domain.Sessions.Entities;
using Domain.Sessions.Enums;
using Domain.Sessions.Errors;
using Domain.Sessions.Events;
using Domain.Sessions.ValueObjects;
using SharedKernel;
using System.Windows.Input;

namespace Domain.Sessions;

public class Session : Entity<SessionId>, IAggregateRoot
{
    public string Mode { get; private set; }
    public SessionStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? StartedAtUtc { get; private set; }
    public DateTime? EndedAtUtc { get; private set; }
    public MatchSettings? Settings { get; private set; }

    private readonly List<Player> _players = [];
    public IReadOnlyList<Player> Players => _players.AsReadOnly();

    private readonly CommandHistory _commandHistory = new();
    private readonly StateHashBuffer _hashBuffer = new();

    public int CurrentTick { get; private set; }

    public static Session Create(string mode, DateTime createdAtUtc, MatchSettings? settings = null)
    {
        var id = new SessionId(Guid.NewGuid());
        var session = new Session(id, mode, createdAtUtc, settings);

        session.AddDomainEvent(new SessionCreated(id, mode, createdAtUtc));

        return session;
    }

    private Session() { }

    private Session(SessionId id, string mode, DateTime createdAtUtc, MatchSettings? settings = null) : base(id)
    {
        Id = id;
        Mode = mode;
        Settings = settings;
        Status = SessionStatus.Waiting;
        CreatedAtUtc = createdAtUtc;
        CurrentTick = 0;
    }

    public Result JoinPlayer(Player playerToJoin)
    {
        if (Status != SessionStatus.Waiting)
            return Result.Failure(SessionErrors.SessionAlreadyStarted);

        if (_players.Any(p => p.Id == playerToJoin.Id))
            return Result.Failure(SessionErrors.PlayerAlreadyInSession);

        _players.Add(playerToJoin);

        AddDomainEvent(new PlayerJoinedSession(Id, playerToJoin.Id));

        return Result.Success();
    }

    public Result RemovePlayer(Player playerToRemove)
    {
        var removed = _players.RemoveAll(p => p.Id == playerToRemove.Id);
        if (removed == 0)
            return Result.Failure(SessionErrors.PlayerNotFound);

        AddDomainEvent(new PlayerLeftSession(Id, playerToRemove.Id));

        return Result.Success();
    }

    public Result DisconnectPlayer(Player playerToDiscounnet)
    {
        var player = _players.FirstOrDefault(p => p.Id == playerToDiscounnet.Id);
        if (player is null)
            return Result.Failure(SessionErrors.PlayerNotFound);

        player.SetStatus(PlayerStatus.Disconnected);
        AddDomainEvent(new PlayerLeftSession(Id, player.Id));

        return Result.Success();
    }

    public Result<bool> MarkPlayerReady(Player readyPlayer, DateTime utc)
    {
        var player = _players.FirstOrDefault(p => p.Id == readyPlayer.Id);
        if (player is null)
            return Result.Failure<bool>(SessionErrors.PlayerNotFound);

        player.MarkReady();
        AddDomainEvent(new PlayerMarkedReady(Id, player.Id));

        if (_players.All(p => p.Status == PlayerStatus.Ready))
        {
            Start(utc);
            return Result.Success(true);
        }

        return Result.Success(false);
    }


    public Result Start(DateTime startedAtUtc)
    {
        if (Status != SessionStatus.Waiting)
            return Result.Failure(SessionErrors.SessionAlreadyStarted);

        Status = SessionStatus.InProgress;
        StartedAtUtc = startedAtUtc;

        AddDomainEvent(new SessionCreated(Id, Mode, CreatedAtUtc));

        return Result.Success();
    }

    public Result End(DateTime endedAtUtc)
    {
        Status = SessionStatus.Ended;
        EndedAtUtc = endedAtUtc;

        AddDomainEvent(new SessionEnded(Id, endedAtUtc));

        return Result.Success();
    }

    public void ReceiveCommand(IGameCommand command) { /* store in CommandHistory */ }
    public void ReceiveStateHash(StateHashReport report) { /* store in HashBuffer */ }
    public IReadOnlyList<IGameCommand> GetCommandsFromFrame(ulong startFrame) { return default; }

    public void TickForward()
    {
        CurrentTick += 1;
    }
}
