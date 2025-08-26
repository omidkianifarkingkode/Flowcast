using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Realtime.Transport.Messaging;
using Realtime.Transport.Messaging.Factories;
using Realtime.Transport.Messaging.Sender;
using Realtime.Transport.UserConnection;
using System.Net.WebSockets;

namespace Realtime.Transport.Liveness;

/// <summary>
/// Periodically pings all open WebSocket connections to ensure liveness and measure RTT.
/// </summary>
public class WebSocketLivenessService(
    IUserConnectionRegistry connectionRegistry,
    IMessageFactory headerFactory,
    IRealtimeMessageSender messageSender,
    IOptions<WebSocketLivenessOptions> options,
    ILogger<WebSocketLivenessService> logger) : BackgroundService
{
    private readonly TimeSpan _pingInterval = options.Value.GetPingInterval();
    private readonly TimeSpan _timeout = options.Value.GetTimeout();
    private readonly TimeSpan _pendingPingTtl = options.Value.GetPendingPingTtl();
    private readonly bool _includeTelemetry = options.Value.IncludeTelemetryInPing;

    /// <summary>
    /// Routine:
    /// 1) Every PingInterval:
    ///    a) For each open connection:
    ///       - If (now - LastPong) > Timeout => close & unregister.
    ///       - Else send a correlated Ping (PingId == header.Id) and record sent time.
    ///       - Purge in-flight pings older than PendingPingTtl.
    /// 2) Repeat until cancelled.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var nowUnix = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            await RunTickAsync(nowUnix, cancellationToken);

            try
            {
                await Task.Delay(_pingInterval, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                // normal on shutdown
            }
        }
    }

    private async Task RunTickAsync(long nowUnix, CancellationToken cancellationToken)
    {
        foreach (var connectionInfo in connectionRegistry.GetAllConnections())
        {
            if (cancellationToken.IsCancellationRequested) break;
            if (connectionInfo.Socket.State != WebSocketState.Open) continue;

            if (IsTimedOut(connectionInfo, nowUnix))
            {
                await CloseAndUnregisterAsync(connectionInfo, cancellationToken);
                continue;
            }

            if (ShouldSendPing(connectionInfo, nowUnix))
                await SendPingAsync(connectionInfo, nowUnix, cancellationToken);

            connectionInfo.CleanupStalePings(nowUnix, (long)_pendingPingTtl.TotalMilliseconds);
        }
    }

    private async Task CloseAndUnregisterAsync(UserConnectionInfo connection, CancellationToken cancellationToken)
    {
        logger.LogWarning("[Liveness] Closing {ConnectionId} user {UserId} (heartbeat timeout).",
            connection.ConnectionId, connection.UserId);

        try
        {
            await connection.Socket.CloseAsync(
                WebSocketCloseStatus.NormalClosure,
                "Heartbeat timeout",
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[Liveness] Error closing timed-out socket {ConnectionId} user {UserId}.",
                connection.ConnectionId, connection.UserId);
        }

        connectionRegistry.Unregister(connection.ConnectionId, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
    }

    private async Task SendPingAsync(UserConnectionInfo connection, long nowUnix, CancellationToken cancellationToken)
    {
        var headerId = IdGenerator.NewId();

        HeaderFlags flags = HeaderFlags.None;
        TelemetrySegment? telemetry = null;

        if (_includeTelemetry)
        {
            if (!connectionRegistry.TryGetTelemetry(connection.UserId, out var t))
            {
                t = new TelemetrySegment(0UL, 0, 0); // default if none yet
            }
            telemetry = t;
            flags = HeaderFlags.HasTelemetry;
        }

        var ping = headerFactory.Create(RealtimeMessageType.Ping);

        //var header = new MessageHeader(3, headerId, nowUnix, flags, telemetry);
        //var payload = new PingCommand { PingId = headerId, ServerTimestamp = nowUnix };
        //var pingMsg = new RealtimeMessage<PingCommand>(header, payload);

        connectionRegistry.MarkPingSent(connection.UserId, headerId, nowUnix);
        await messageSender.SendToUserAsync(connection.UserId, ping, cancellationToken);
    }

    private bool IsTimedOut(UserConnectionInfo connectionInfo, long now)
    {
        var last = options.Value.TreatAnyClientMessageAsHeartbeat
            ? Math.Max(connectionInfo.LastClientActivityUnixMillis, connectionInfo.LastPongUnixMillis)
            : connectionInfo.LastPongUnixMillis; // require Pong if false

        return now - last > (long)_timeout.TotalMilliseconds;
    }

    private bool ShouldSendPing(UserConnectionInfo connectionInfo, long now)
    {
        if (!options.Value.AdaptivePing) return true;
        // if client chatty recently, skip ping
        return now - connectionInfo.LastClientActivityUnixMillis >= (long)_pingInterval.TotalMilliseconds;
    }
}
