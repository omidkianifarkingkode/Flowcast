using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using Realtime.Transport.Liveness;
using Realtime.Transport.Messaging;
using System.Collections.Concurrent;
using System.Net.Sockets;
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

    void MarkPingSent(string userId, ulong pingId, long unixMillis);
    void MarkPongReceived(string userId, long unixMillis);
    void MarkClientActivity(string userId, long unixMillis);

    bool TryCompletePing(string userId, ulong pingId, long unixMillis, out long rttMillis);
    bool TryGetTelemetry(string userId, out TelemetrySegment telemetry);
}

public class InMemoryUserConnectionRegistry(IOptions<WebSocketLivenessOptions> options) : IUserConnectionRegistry
{
    private readonly ConcurrentDictionary<string, UserConnectionInfo> _connections = new();
    private readonly ConcurrentDictionary<string, string> _userConnections = new(); // user -> connection
    private readonly object _lock = new();

    public event Action<string> UserConnected = delegate { };
    public event Action<string> UserDisconnected = delegate { };

    public void Register(string connectionId, string userId, WebSocket socket, long unixMillis)
    {
        lock (_lock)
        {
            // If user already connected, close & unregister old connection
            if (_userConnections.TryGetValue(userId, out var oldConnectionId))
            {
                if (_connections.TryGetValue(oldConnectionId, out var oldConnection))
                {
                    if (oldConnection.Socket.State == WebSocketState.Open || oldConnection.Socket.State == WebSocketState.CloseReceived)
                    {
                        // Close old socket gracefully (fire-and-forget)
                        _ = oldConnection.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure,
                            "New connection established", CancellationToken.None);
                    }
                }

                Unregister(oldConnectionId, unixMillis);
            }

            // Register new connection
            var maxPending = options.Value.GetMaxPendingPings();
            var connection = new UserConnectionInfo(connectionId, userId, socket, unixMillis, maxPending);
            _connections[connectionId] = connection;
            _userConnections[userId] = connectionId;

            UserConnected?.Invoke(userId);
        }
    }

    public void Unregister(string connectionId, long unixMillis)
    {
        lock (_lock)
        {
            if (_connections.TryRemove(connectionId, out var conn))
            {
                conn.MarkDisconnected(unixMillis);
                var userId = conn.UserId;

                // Remove from userConnections if it matches this connectionId
                if (_userConnections.TryGetValue(userId, out var registeredConnectionId) && registeredConnectionId == connectionId)
                {
                    _userConnections.TryRemove(userId, out _);
                    UserDisconnected?.Invoke(userId);
                }
            }
        }
    }

    public bool TryGetUserConnectionInfo(string userId, out UserConnectionInfo userConnectionInfo) 
    {
        lock (_lock)
        {
            if (_userConnections.TryGetValue(userId, out var connectionId) &&
                _connections.TryGetValue(connectionId, out var info))
            {
                userConnectionInfo = info;
                return true;
            }
        }

        userConnectionInfo = default!;
        return false;
    }

    public bool TryGetUserId(string connectionId, out string userId)
    {
        if (_connections.TryGetValue(connectionId, out var info))
        {
            userId = info.UserId;
            return true;
        }

        userId = default;
        return false;
    }

    public bool TryGetWebSocket(string connectionId, out WebSocket socket)
    {
        if (_connections.TryGetValue(connectionId, out var info))
        {
            socket = info.Socket;
            return true;
        }

        socket = default!;
        return false;
    }

    public bool TryGetWebSocketByUserId(string userId, out WebSocket socket)
    {
        lock (_lock)
        {
            if (_userConnections.TryGetValue(userId, out var connectionId) &&
                _connections.TryGetValue(connectionId, out var info))
            {
                socket = info.Socket;
                return true;
            }
        }

        socket = default!;
        return false;
    }

    public IReadOnlyList<UserConnectionInfo> GetAllConnections()
    {
        // Return a snapshot list of all current connections
        return _connections.Values.ToList();
    }

    public bool IsUserConnected(string userId)
    {
        lock (_lock)
        {
            return _userConnections.ContainsKey(userId);
        }
    }

    public void MarkPingSent(string userId, ulong pingId, long sentAt)
    {
        lock (_lock)
        {
            if (_userConnections.TryGetValue(userId, out var cid) &&
                _connections.TryGetValue(cid, out var info))
            {
                info.MarkPingSent(pingId, sentAt);
            }
        }
    }

    public void MarkPongReceived(string userId, long now)
    {
        lock (_lock)
        {
            if (_userConnections.TryGetValue(userId, out var cid) &&
                _connections.TryGetValue(cid, out var info))
            {
                info.MarkPongReceived(now);
            }
        }
    }

    public void MarkClientActivity(string userId, long unixMillis)
    {
        lock (_lock)
        {
            if (_userConnections.TryGetValue(userId, out var cid) &&
                _connections.TryGetValue(cid, out var info))
            {
                info.MarkClientActivity(unixMillis);
            }
        }
    }

    public bool TryCompletePing(string userId, ulong pingId, long now, out long rtt)
    {
        lock (_lock)
        {
            if (_userConnections.TryGetValue(userId, out var cid) &&
                _connections.TryGetValue(cid, out var info))
            {
                return info.TryCompletePing(pingId, now, out rtt);
            }
        }
        rtt = 0; return false;
    }

    public bool TryGetTelemetry(string userId, out TelemetrySegment telemetry)
    {
        lock (_lock)
        {
            if (_userConnections.TryGetValue(userId, out var cid) &&
                _connections.TryGetValue(cid, out var info) &&
                info.TryGetTelemetry(out telemetry))
            {
                return true;
            }
        }
        telemetry = default;
        return false;
    }
}
