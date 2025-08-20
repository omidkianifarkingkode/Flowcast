using SharedKernel;

namespace Domain.Sessions;

public sealed class Participant : Entity<PlayerId>
{
    public string DisplayName { get; private set; }
    public ParticipantStatus Status { get; private set; }
    public int Score { get; private set; }

    private Participant() : base(default) { }

    public Participant(PlayerId id, string displayName)
        : base(id)
    {
        DisplayName = displayName;
        Status = ParticipantStatus.Invited;
        Score = 0;
    }

    public void MarkConnected() => Status = ParticipantStatus.Connected;
    public void MarkLoaded() => Status = ParticipantStatus.Loaded;
    public void MarkDisconnected() => Status = ParticipantStatus.Disconnected;
    public void AddScore(int points) => Score += points;
}
