using System.Net.WebSockets;

namespace Realtime.Transport.UserConnection;

public class UserConnectionInfo(string connectionId, string userId, WebSocket socket, long connectedAt)
{
    public string ConnectionId { get; } = connectionId;
    public string UserId { get; } = userId;
    public WebSocket Socket { get; } = socket;

    // Store timestamps as Unix time milliseconds
    public long ConnectedAt { get; } = connectedAt;
    public long? DisconnectedAt { get; private set; }
    public long LastClientActivity { get; private set; } = connectedAt;

    public bool IsConnected => DisconnectedAt == null;

    public WebSocketState State => Socket.State;

    public void MarkDisconnected(long unixMillis) => DisconnectedAt = unixMillis;

    public void MarkClientActivity(long unixMillis) => LastClientActivity = unixMillis;
}
