using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Realtime.Transport.Liveness.Policies;
using Realtime.Transport.UserConnection;
using System.Net.WebSockets;

namespace Realtime.Transport.Liveness;

/// <summary>
/// Periodically check all open WebSocket connections to ensure liveness and measure RTT.
/// </summary>
public class WebSocketLivenessService(
    IUserConnectionRegistry connectionRegistry,
    ILivenessPolicy policy,
    IOptions<WebSocketLivenessOptions> options,
    ILogger<WebSocketLivenessService> logger) : BackgroundService
{
    /// <summary>
    /// Routine:
    /// 1) Every Interval:
    ///    a) For each open connection:
    ///       - If (now - LastPong) > Timeout => close & unregister.
    /// 2) Repeat until cancelled.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var checkInterval = TimeSpan.FromSeconds(Math.Max(1, options.Value.TimeoutSeconds / 3)); // cheap heuristic

        while (!cancellationToken.IsCancellationRequested)
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            await RunTickAsync(now, cancellationToken);

            try
            {
                await Task.Delay(checkInterval, cancellationToken);
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
            if (cancellationToken.IsCancellationRequested)
                break;

            if (!policy.IsCandidateForTimeoutCheck(connectionInfo))
                continue;

            if (policy.IsTimedOut(connectionInfo, nowUnix, out var status, out var reason))
            {
                try
                {
                    logger.LogInformation("[Liveness] Closing {UserId}/{ConnectionId} due to timeout ({Reason})",
                        connectionInfo.UserId, connectionInfo.ConnectionId, reason);

                    if (connectionInfo.Socket.State is WebSocketState.Open or WebSocketState.CloseReceived)
                    {
                        await connectionInfo.Socket.CloseAsync(status, reason, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "[Liveness] Error closing socket for {UserId}/{ConnectionId}", connectionInfo.UserId, connectionInfo.ConnectionId);
                }

                connectionRegistry.Unregister(connectionInfo.ConnectionId, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            }
        }
    }
}
