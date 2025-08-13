using Application.Abstractions.Realtime;
using SharedKernel;
using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace Infrastructure.Realtime.Services;

public class InMemoryUserConnectionRegistry(IDateTimeProvider dateTimeProvider) : IUserConnectionRegistry
{
    private readonly ConcurrentDictionary<string, UserConnectionInfo> _connections = new();
    private readonly ConcurrentDictionary<Guid, string> _userConnections = new(); // One connectionId per user
    private readonly object _lock = new();

    public event Action<Guid> UserConnected = delegate { };
    public event Action<Guid> UserDisconnected = delegate { };

    public void Register(string connectionId, Guid userId, WebSocket socket)
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

                Unregister(oldConnectionId);
            }

            // Register new connection
            var connection = new UserConnectionInfo(connectionId, userId, socket, dateTimeProvider.UnixTimeMilliseconds);
            _connections[connectionId] = connection;
            _userConnections[userId] = connectionId;

            UserConnected?.Invoke(userId);
        }
    }

    public void Unregister(string connectionId)
    {
        lock (_lock)
        {
            if (_connections.TryRemove(connectionId, out var conn))
            {
                conn.MarkDisconnected(dateTimeProvider.UnixTimeMilliseconds);
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

    public bool TryGetUserId(string connectionId, out Guid userId)
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

    public bool TryGetWebSocketByUserId(Guid userId, out WebSocket socket)
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

    public bool IsUserConnected(Guid userId)
    {
        lock (_lock)
        {
            return _userConnections.ContainsKey(userId);
        }
    }

    public void MarkPongReceived(Guid userId, long unixMillis)
    {
        lock (_lock)
        {
            if (_userConnections.TryGetValue(userId, out var connectionId) &&
                _connections.TryGetValue(connectionId, out var info))
            {
                info.MarkPongReceived(unixMillis);
            }
        }
    }
}
