using Application.Realtime.Commons;
using Application.Realtime.Messaging;
using System.Net.WebSockets;

namespace Application.Realtime.Services;

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

    void MarkPingSent(Guid userId, ulong pingId, long sentAtUnixMillis);
    void MarkPongReceived(Guid userId, long nowUnixMillis);
    bool TryCompletePing(Guid userId, ulong pingId, long nowUnixMillis, out long rttMillis);
    bool TryGetTelemetry(Guid userId, out TelemetrySegment telemetry);
}
