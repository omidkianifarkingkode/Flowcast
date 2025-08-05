using Domain.Sessions.Enums;
using Domain.Sessions.ValueObjects;
using SharedKernel;

namespace Domain.Sessions.Entities;

public class Player(PlayerId id, string displayName = default, PlayerStatus status = PlayerStatus.Connected) : Entity<PlayerId>(id)
{
    public string DisplayName { get; private set; } = displayName;
    public PlayerStatus Status { get; private set; } = PlayerStatus.Connected;
    public int Score { get; private set; } = 0;

    public void SetDisplayName(string name) => DisplayName = name;
    public void SetStatus(PlayerStatus status) => Status = status;
    public void MarkReady() => Status = PlayerStatus.Ready;
    public void Disconnect() => Status = PlayerStatus.Disconnected;
    public void AddScore(int points) => Score += points;
}