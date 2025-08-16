using Application.Realtime.Messaging;
using System.Net.WebSockets;

namespace Application.Realtime.Commons;

public class UserConnectionInfo
{
    public string ConnectionId { get; }
    public Guid UserId { get; }
    public WebSocket Socket { get; }

    // Store timestamps as Unix time milliseconds
    public long ConnectedAtUnixMillis { get; }
    public long? DisconnectedAtUnixMillis { get; private set; }
    public long LastPongUnixMillis { get; private set; }
    public long LastClientActivityUnixMillis { get; private set; }

    public bool IsConnected => DisconnectedAtUnixMillis == null;

    public WebSocketState State => Socket.State;

    // RTT tracking
    private readonly int _maxPendingPings;
    private readonly Dictionary<ulong, long> _pendingPings = []; // kvp<PingId,sentAt>

    public ulong? LastCompletedPingId { get; private set; }
    public long? LastRttMillis { get; private set; }

    public UserConnectionInfo(string connectionId, Guid userId, WebSocket socket, long connectedAtUnixMillis, int maxPendingPings = 16)
    {
        ConnectionId = connectionId;
        UserId = userId;
        Socket = socket;
        ConnectedAtUnixMillis = connectedAtUnixMillis;
        LastPongUnixMillis = connectedAtUnixMillis;
        LastClientActivityUnixMillis = connectedAtUnixMillis;
        _maxPendingPings = Math.Max(1, maxPendingPings);
    }


    public void MarkDisconnected(long unixMillis)
    {
        DisconnectedAtUnixMillis = unixMillis;
    }

    public void MarkPingSent(ulong pingId, long sentAtUnixMillis)
    {
        if (_pendingPings.Count >= _maxPendingPings)
        {
            // Drop oldest to stay within cap
            var oldest = _pendingPings.OrderBy(kv => kv.Value).First();
            _pendingPings.Remove(oldest.Key);
        }
        _pendingPings[pingId] = sentAtUnixMillis;
    }

    public void MarkPongReceived(long unixMillis)
    {
        LastPongUnixMillis = unixMillis;
    }

    public void MarkClientActivity(long unixMillis)
    {
        LastClientActivityUnixMillis = unixMillis;
    }

    public bool TryCompletePing(ulong pingId, long nowUnixMillis, out long rttMillis)
    {
        if (_pendingPings.Remove(pingId, out var sentAt))
        {
            rttMillis = Math.Max(0, nowUnixMillis - sentAt);
            LastRttMillis = rttMillis;
            LastCompletedPingId = pingId;
            return true;
        }
        rttMillis = 0;
        return false;
    }

    public bool TryGetTelemetry(out TelemetrySegment telemetry)
    {
        if (LastRttMillis is long rtt && LastCompletedPingId is ulong pid)
        {
            telemetry = new TelemetrySegment(pid, (int)Math.Min(int.MaxValue, rtt), 0);
            return true;
        }
        telemetry = default;
        return false;
    }

    public void CleanupStalePings(long nowUnixMillis, long staleAfterMillis)
    {
        var stale = _pendingPings.Where(kv => nowUnixMillis - kv.Value > staleAfterMillis)
                                 .Select(kv => kv.Key).ToList();
        foreach (var k in stale) _pendingPings.Remove(k);
    }
}
