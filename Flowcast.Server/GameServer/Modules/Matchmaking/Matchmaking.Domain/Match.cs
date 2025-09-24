using SharedKernel;
using SharedKernel.Primitives;

namespace Matchmaking.Domain;

public sealed class Match : Entity<MatchId>, IAggregateRoot
{
    public string Mode { get; private set; } = string.Empty;
    public MatchStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ReadyDeadlineUtc { get; private set; }
    public AbortReason AbortReason { get; private set; } = AbortReason.None;

    private readonly List<PlayerId> _players = new(2);
    public IReadOnlyList<PlayerId> Players => _players;

    private readonly HashSet<PlayerId> _ready = new();
    public IReadOnlySet<PlayerId> ReadyPlayers => _ready;

    private Match() { }

    private Match(MatchId id, string mode, IEnumerable<PlayerId> players, DateTime utc)
        : base(id)
    {
        Id = id;
        Mode = mode;
        _players.AddRange(players);
        Status = MatchStatus.Proposed;
        CreatedAtUtc = utc;
        AddDomainEvent(new MatchProposed(Id, Mode, _players.AsReadOnly(), utc));
    }

    public static Match Create(string mode, PlayerId a, PlayerId b, DateTime utc)
        => new(MatchId.New(), mode, new[] { a, b }, utc);

    public Result BeginReadyCheck(ReadyWindow window, DateTime nowUtc)
    {
        if (Status != MatchStatus.Proposed)
            return Result.Failure(MatchErrors.NotProposed);

        Status = MatchStatus.ReadyCheck;
        ReadyDeadlineUtc = nowUtc + window.Duration;
        return Result.Success();
    }

    public Result AcknowledgeReady(PlayerId playerId, DateTime utc)
    {
        if (Status != MatchStatus.ReadyCheck)
            return Result.Failure(MatchErrors.NotInReadyCheck);

        if (!_players.Contains(playerId))
            return Result.Failure(MatchErrors.PlayerNotInMatch);

        if (ReadyDeadlineUtc is { } dl && utc > dl)
            return Result.Failure(MatchErrors.ReadyWindowExpired);

        if (_ready.Add(playerId))
            AddDomainEvent(new MatchReadyAcknowledged(Id, playerId, utc));

        return Result.Success();
    }

    public bool IsAllReady() => _ready.Count == _players.Count;

    public Result ConfirmIfAllReady(DateTime utc, out bool confirmed)
    {
        confirmed = false;
        if (Status != MatchStatus.ReadyCheck)
            return Result.Failure(MatchErrors.NotInReadyCheck);

        if (!IsAllReady())
            return Result.Failure(MatchErrors.AllPlayerNotReady);

        Status = MatchStatus.Confirmed;
        AddDomainEvent(new MatchConfirmed(Id, Mode, _players.AsReadOnly(), utc));
        confirmed = true;
        return Result.Success();
    }

    public Result Abort(AbortReason reason, DateTime utc)
    {
        if (Status is MatchStatus.Confirmed or MatchStatus.Aborted)
            return Result.Success();

        Status = MatchStatus.Aborted;
        AbortReason = reason;
        AddDomainEvent(new MatchReadyFailed(Id, reason, utc));
        return Result.Success();
    }

    public bool IsReadyWindowExpired(DateTime nowUtc)
        => Status == MatchStatus.ReadyCheck && ReadyDeadlineUtc is { } dl && nowUtc >= dl;
}
