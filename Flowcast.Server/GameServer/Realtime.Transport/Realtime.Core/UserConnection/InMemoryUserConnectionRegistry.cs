using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace Realtime.Transport.UserConnection;

public class InMemoryUserConnectionRegistry : IUserConnectionRegistry
{
    private readonly ConcurrentDictionary<string, UserConnectionInfo> _connections = new();      // connectionId -> info
    private readonly ConcurrentDictionary<string, string> _userConnections = new();              // user -> connection
    private readonly ConcurrentDictionary<string, object> _userLocks = new();                    // userId -> lock object

    private object GetUserLock(string userId) => _userLocks.GetOrAdd(userId, static _ => new object());

    public event Action<string> UserConnected = delegate { };
    public event Action<string> UserDisconnected = delegate { };

    public void Register(string connectionId, string userId, WebSocket socket, long unixMillis)
    {
        var userLock = GetUserLock(userId);
        lock (userLock)
        {
            if (_userConnections.TryGetValue(userId, out var oldCid) &&
                _connections.TryGetValue(oldCid, out var old) &&
                (old.Socket.State == WebSocketState.Open || old.Socket.State == WebSocketState.CloseReceived))
            {
                _ = old.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure,
                    "New connection established", CancellationToken.None);

                Unregister(oldCid, unixMillis);
            }

            var info = new UserConnectionInfo(connectionId, userId, socket, unixMillis);
            _connections[connectionId] = info;
            _userConnections[userId] = connectionId;

            UserConnected(userId);
        }
    }

    public void Unregister(string connectionId, long unixMillis)
    {
        if (_connections.TryRemove(connectionId, out var conn))
        {
            conn.MarkDisconnected(unixMillis);
            var userId = conn.UserId;

            var userLock = GetUserLock(userId);
            lock (userLock)
            {
                if (_userConnections.TryGetValue(userId, out var cid) && cid == connectionId)
                {
                    _userConnections.TryRemove(userId, out _);
                }
            }
            UserDisconnected(userId);
        }
    }

    public bool TryGetUserId(string connectionId, out string userId)
    {
        if (_connections.TryGetValue(connectionId, out var info))
        {
            userId = info.UserId;
            return true;
        }

        userId = default!;
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
        if (_userConnections.TryGetValue(userId, out var connectionId) &&
                _connections.TryGetValue(connectionId, out var info))
        {
            socket = info.Socket;
            return true;
        }

        socket = default!;
        return false;
    }

    public bool TryGetUserConnectionInfo(string userId, out UserConnectionInfo userConnectionInfo)
    {
        if (_userConnections.TryGetValue(userId, out var connectionId) &&
                _connections.TryGetValue(connectionId, out var info))
        {
            userConnectionInfo = info;
            return true;
        }

        userConnectionInfo = default!;
        return false;
    }

    public IReadOnlyList<UserConnectionInfo> GetAllConnections()
    {
        // Return a snapshot list of all current connections
        return _connections.Values.ToList();
    }

    public bool IsUserConnected(string userId)
    {
        return _userConnections.ContainsKey(userId);
    }

    public void MarkClientActivity(string userId, long unixMillis)
    {
        if (_userConnections.TryGetValue(userId, out var cid) &&
                _connections.TryGetValue(cid, out var info))
        {
            info.MarkClientActivity(unixMillis);
        }
    }
}
