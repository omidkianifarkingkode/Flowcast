using System.Net.WebSockets;

namespace Application.Abstractions.Realtime;

public class UserConnectionInfo(string connectionId, Guid userId, WebSocket socket, DateTime connectedAtUtc)
{
    public string ConnectionId { get; } = connectionId;
    public Guid UserId { get; } = userId;
    public WebSocket Socket { get; } = socket;
    public DateTime ConnectedAtUtc { get; } = connectedAtUtc;
    public DateTime? DisconnectedAtUtc { get; private set; }
    public DateTime LastPongUtc { get; private set; }
    public bool IsConnected => DisconnectedAtUtc == null;

    public WebSocketState State => Socket.State;

    public void MarkDisconnected(DateTime dateTime)
    {
        DisconnectedAtUtc = dateTime;
    }

    public void MarkPongReceived(DateTime utcNow)
    {
        LastPongUtc = utcNow;
    }
}