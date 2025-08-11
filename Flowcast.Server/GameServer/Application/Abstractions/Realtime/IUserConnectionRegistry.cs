using System.Net.WebSockets;

namespace Application.Abstractions.Realtime;

public interface IUserConnectionRegistry
{
    void Register(string connectionId, Guid userId, WebSocket socket);
    void Unregister(string connectionId);
    bool TryGetUserId(string connectionId, out Guid userId);
    bool TryGetWebSocket(string connectionId, out WebSocket socket);
    bool TryGetWebSocketByUserId(Guid userId, out WebSocket socket);
    IReadOnlyList<UserConnectionInfo> GetAllConnections();

    bool IsUserConnected(Guid userId);

    event Action<Guid> UserConnected;
    event Action<Guid> UserDisconnected;
}
