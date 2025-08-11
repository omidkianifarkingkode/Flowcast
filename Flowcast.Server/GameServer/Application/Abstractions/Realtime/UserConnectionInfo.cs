using System.Net.WebSockets;

namespace Application.Abstractions.Realtime;

public class UserConnectionInfo(string connectionId, Guid userId, WebSocket socket, long connectedAtUnixMillis)
{
    public string ConnectionId { get; } = connectionId;
    public Guid UserId { get; } = userId;
    public WebSocket Socket { get; } = socket;

    // Store timestamps as Unix time milliseconds
    public long ConnectedAtUnixMillis { get; } = connectedAtUnixMillis;
    public long? DisconnectedAtUnixMillis { get; private set; }
    public long LastPongUnixMillis { get; private set; } = connectedAtUnixMillis;

    public bool IsConnected => DisconnectedAtUnixMillis == null;

    public WebSocketState State => Socket.State;

    public void MarkDisconnected(long unixMillis)
    {
        DisconnectedAtUnixMillis = unixMillis;
    }

    public void MarkPongReceived(long unixMillis)
    {
        LastPongUnixMillis = unixMillis;
    }
}
