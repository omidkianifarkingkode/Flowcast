using System.Net.WebSockets;

namespace Realtime.Transport.UserConnection;

public interface IUserConnectionRegistry
{
    event Action<string> UserConnected;
    event Action<string> UserDisconnected;

    void Register(string connectionId, string userId, WebSocket socket, long unixMillis);
    void Unregister(string connectionId, long unixMillis);

    bool TryGetUserId(string connectionId, out string userId);
    bool TryGetWebSocket(string connectionId, out WebSocket socket);
    bool TryGetWebSocketByUserId(string userId, out WebSocket socket);
    bool TryGetUserConnectionInfo(string userId, out UserConnectionInfo userConnectionInfo);

    IReadOnlyList<UserConnectionInfo> GetAllConnections();
    bool IsUserConnected(string userId);

    void MarkClientActivity(string userId, long unixMillis);
}