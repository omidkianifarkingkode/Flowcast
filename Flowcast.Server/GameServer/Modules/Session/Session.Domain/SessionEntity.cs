using SharedKernel;
using SharedKernel.Primitives;

namespace Session.Domain;

public sealed class SessionEntity : Entity<SessionId>, IAggregateRoot
{
    public string Mode { get; private set; }
    public SessionStatus Status { get; private set; }
    public StartBarrier Barrier { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? StartedAtUtc { get; private set; }
    public DateTime? EndedAtUtc { get; private set; }
    public DateTime? JoinDeadlineUtc { get; private set; }
    public MatchSettings? Settings { get; private set; }
    public SessionCloseReason? CloseReason { get; private set; }

    private readonly List<Participant> _participants = [];
    public IReadOnlyList<Participant> Participants => _participants.AsReadOnly();

    private readonly CommandHistory _commandHistory = new();
    private readonly StateHashBuffer _hashBuffer = new();

    public int CurrentTick { get; private set; }

    private SessionEntity() { }

    private SessionEntity(SessionId id, string mode, StartBarrier barrier, DateTime createdAtUtc, DateTime? joinDeadlineUtc, MatchSettings? settings)
        : base(id)
    {
        Id = id;
        Mode = mode;
        Barrier = barrier;
        Settings = settings;
        Status = SessionStatus.Waiting;
        CreatedAtUtc = createdAtUtc;
        JoinDeadlineUtc = joinDeadlineUtc;
        CurrentTick = 0;

        AddDomainEvent(new SessionCreated(id, mode, createdAtUtc));
    }

    public static SessionEntity Create(string mode, StartBarrier barrier, DateTime createdAtUtc, DateTime? joinDeadlineUtc, MatchSettings? settings = null)
        => new(new SessionId(Guid.NewGuid()), mode, barrier, createdAtUtc, joinDeadlineUtc, settings);

    public Result JoinParticipant(Participant p, DateTime utc)
    {
        if (Status != SessionStatus.Waiting)
            return Result.Failure(SessionErrors.AlreadyStarted);

        if (_participants.Any(x => x.Id == p.Id))
            return Result.Failure(SessionErrors.DuplicateJoin);

        _participants.Add(p);

        AddDomainEvent(new ParticipantJoined(Id, p.Id, utc));

        return Result.Success();
    }

    public Result RemoveParticipant(PlayerId playerId, DateTime utc)
    {
        var index = _participants.FindIndex(p => p.Id == playerId);

        if (index < 0) 
            return Result.Failure(SessionErrors.ParticipantMissing);

        var removed = _participants[index];
        _participants.RemoveAt(index);

        AddDomainEvent(new ParticipantLeft(Id, removed.Id, utc));

        return Result.Success();
    }

    public Result MarkParticipantConnected(PlayerId id)
    {
        var participant = _participants.FirstOrDefault(x => x.Id == id);

        if (participant is null) 
            return Result.Failure(SessionErrors.ParticipantMissing);

        participant.MarkConnected();

        return Result.Success();
    }

    public Result MarkParticipantLoaded(PlayerId id, DateTime utc)
    {
        var participant = _participants.FirstOrDefault(x => x.Id == id);

        if (participant is null)
            return Result.Failure(SessionErrors.ParticipantMissing);

        participant.MarkLoaded();

        AddDomainEvent(new ParticipantLoaded(Id, id, utc));

        return Result.Success();
    }

    public bool StartBarrierSatisfied(DateTime nowUtc)
        => Barrier switch
        {
            StartBarrier.ConnectedOnly => 
                _participants.Count > 0 && _participants.All(p => p.Status is ParticipantStatus.Connected or ParticipantStatus.Loaded),
            StartBarrier.ConnectedAndLoaded => 
                _participants.Count > 0 && _participants.All(p => p.Status == ParticipantStatus.Loaded),
            StartBarrier.Timer => 
                JoinDeadlineUtc is { } dl && nowUtc >= dl,
            _ => false
        };

    public Result TryStart(DateTime nowUtc)
    {
        if (Status != SessionStatus.Waiting)
            return Result.Failure(SessionErrors.AlreadyStarted);

        if (!StartBarrierSatisfied(nowUtc))
            return Result.Success(); // no-op if not ready

        Status = SessionStatus.InProgress;
        StartedAtUtc = nowUtc;

        AddDomainEvent(new SessionStarted(Id, Mode, nowUtc));

        return Result.Success();
    }

    public Result End(DateTime endedAtUtc, SessionCloseReason reason = SessionCloseReason.Completed)
    {
        if (Status == SessionStatus.Ended)
            return Result.Success();

        Status = SessionStatus.Ended;
        CloseReason = reason;
        EndedAtUtc = endedAtUtc;

        AddDomainEvent(new SessionEnded(Id, endedAtUtc));

        return Result.Success();
    }

    public Result AbortBeforeStart(DateTime utc, SessionCloseReason reason = SessionCloseReason.NoShow)
    {
        if (Status is SessionStatus.Aborted or SessionStatus.Ended)
            return Result.Success();

        if (Status == SessionStatus.InProgress)
            return Result.Failure(SessionErrors.AlreadyStarted);

        Status = SessionStatus.Aborted;
        CloseReason = reason;
        EndedAtUtc = utc;

        AddDomainEvent(new SessionEnded(Id, utc));

        return Result.Success();
    }

    // Runtime
    public void ReceiveCommand(IGameCommand command) { /* store in CommandHistory */ }
    public void ReceiveStateHash(StateHashReport report) { /* store in HashBuffer */ }
    public IReadOnlyList<IGameCommand> GetCommandsFromFrame(ulong startFrame) => Array.Empty<IGameCommand>();
    public void TickForward() => CurrentTick += 1;
}
