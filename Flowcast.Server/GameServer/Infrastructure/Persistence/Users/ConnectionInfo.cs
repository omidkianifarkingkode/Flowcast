namespace Infrastructure.Persistence.Users;

public class ConnectionInfo(string connectionId, Guid userId, DateTime connectedAtUtc)
{
    public string ConnectionId { get; } = connectionId;
    public Guid UserId { get; } = userId;
    public DateTime ConnectedAtUtc { get; } = connectedAtUtc;
    public DateTime? DisconnectedAtUtc { get; private set; }
    public bool IsConnected => DisconnectedAtUtc == null;

    public void MarkDisconnected(DateTime dateTime)
    {
        DisconnectedAtUtc = dateTime;
    }
}
