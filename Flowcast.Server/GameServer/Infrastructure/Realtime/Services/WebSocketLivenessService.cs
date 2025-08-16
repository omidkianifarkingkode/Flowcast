using Application.Realtime;
using Application.Realtime.Commons;
using Application.Realtime.Commons.PingPong;
using Application.Realtime.Messaging;
using Application.Realtime.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel;
using System.Net.WebSockets;

namespace Infrastructure.Realtime.Services;

/// <summary>
/// Periodically pings all open WebSocket connections to ensure liveness and measure RTT.
/// </summary>
public class WebSocketLivenessService(
    IUserConnectionRegistry connectionRegistry,
    IDateTimeProvider dateTimeProvider,
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
            var nowUnix = dateTimeProvider.UnixTimeMilliseconds;

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

    private async Task RunTickAsync(long nowUnix, CancellationToken ct)
    {
        var snapshot = connectionRegistry.GetAllConnections();
        foreach (var connection in snapshot)
        {
            if (ct.IsCancellationRequested) break;

            try
            {
                await HandleConnectionAsync(connection, nowUnix, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[Liveness] Error handling connection {ConnectionId} user {UserId}.",
                    connection.ConnectionId, connection.UserId);
            }
        }
    }

    private async Task HandleConnectionAsync(UserConnectionInfo connection, long nowUnix, CancellationToken ct)
    {
        var socket = connection.Socket;

        // Only consider open sockets
        if (socket.State != WebSocketState.Open)
            return;

        // Timeout check
        if (HasHeartbeatTimedOut(connection, nowUnix))
        {
            await CloseAndUnregisterAsync(connection, ct);
            return;
        }

        // Send one correlated ping this tick
        await SendPingAsync(connection, nowUnix, ct);

        // Hygiene: purge very old in-flight pings
        connection.CleanupStalePings(nowUnix, (long)_pendingPingTtl.TotalMilliseconds);
    }

    private bool HasHeartbeatTimedOut(UserConnectionInfo connection, long nowUnix)
        => nowUnix - connection.LastPongUnixMillis > (long)_timeout.TotalMilliseconds;

    private async Task CloseAndUnregisterAsync(UserConnectionInfo connection, CancellationToken ct)
    {
        logger.LogWarning("[Liveness] Closing {ConnectionId} user {UserId} (heartbeat timeout).",
            connection.ConnectionId, connection.UserId);

        try
        {
            await connection.Socket.CloseAsync(
                WebSocketCloseStatus.NormalClosure,
                "Heartbeat timeout",
                ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[Liveness] Error closing timed-out socket {ConnectionId} user {UserId}.",
                connection.ConnectionId, connection.UserId);
        }

        connectionRegistry.Unregister(connection.ConnectionId);
    }

    private async Task SendPingAsync(UserConnectionInfo connection, long nowUnix, CancellationToken ct)
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

        var header = new MessageHeader(RealtimeMessageType.Ping, headerId, nowUnix, flags, telemetry);
        var payload = new PingCommand { PingId = headerId, ServerTimestamp = nowUnix };
        var pingMsg = new RealtimeMessage<PingCommand>(header, payload);

        connectionRegistry.MarkPingSent(connection.UserId, headerId, nowUnix);
        await messageSender.SendToUserAsync(connection.UserId, pingMsg, ct);
    }
}
