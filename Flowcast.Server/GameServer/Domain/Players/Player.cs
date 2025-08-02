using Domain.Players;

namespace Domain.Entities;

public class Player(long playerId, string displayName, PlayerStatus status = PlayerStatus.Connected)
{
    public long PlayerId { get; } = playerId;
    public string DisplayName { get; private set; } = displayName;
    public PlayerStatus Status { get; private set; } = PlayerStatus.Connected;
    public int Score { get; private set; } = 0;

    public void SetStatus(PlayerStatus status)
    {
        Status = status;
    }

    public void AddScore(int points)
    {
        Score += points;
    }

    public void MarkReady() => Status = PlayerStatus.Ready;
}
