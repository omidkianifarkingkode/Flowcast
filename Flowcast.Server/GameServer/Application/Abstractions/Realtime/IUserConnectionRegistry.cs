using System.Net.WebSockets;

namespace Application.Abstractions.Realtime;

public interface IUserConnectionRegistry
{
    event Action<Guid> UserConnected;
    event Action<Guid> UserDisconnected;

    void Register(string connectionId, Guid userId, WebSocket socket);
    void Unregister(string connectionId);
    bool TryGetUserId(string connectionId, out Guid userId);
    bool TryGetWebSocket(string connectionId, out WebSocket socket);
    bool TryGetWebSocketByUserId(Guid userId, out WebSocket socket);

    IReadOnlyList<UserConnectionInfo> GetAllConnections();
    bool IsUserConnected(Guid userId);
    void MarkPongReceived(Guid userId, long unixMillis);
}
