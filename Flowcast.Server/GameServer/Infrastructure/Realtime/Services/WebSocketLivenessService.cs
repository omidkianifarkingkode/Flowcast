using Application.Abstractions.Realtime;
using Application.Abstractions.Realtime.Messaging;
using Application.Abstractions.Realtime.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel;
using System.Net.WebSockets;

namespace Infrastructure.Realtime.Services;

public class WebSocketLivenessService(IUserConnectionRegistry connectionRegistry,
    IDateTimeProvider dateTimeProvider,
    IRealtimeMessageSender messageSender,
    IOptions<WebSocketLivenessOptions> options,
    ILogger<WebSocketLivenessService> logger) : BackgroundService
{
    private readonly TimeSpan _pingInterval = TimeSpan.FromSeconds(options.Value.PingIntervalSeconds);
    private readonly TimeSpan _timeout = TimeSpan.FromSeconds(options.Value.TimeoutSeconds);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var nowUnix = dateTimeProvider.UnixTimeMilliseconds;

            foreach (var connection in connectionRegistry.GetAllConnections())
            {
                var socket = connection.Socket;
                if (socket.State != WebSocketState.Open)
                    continue;

                if (nowUnix - connection.LastPongUnixMillis > (long)_timeout.TotalMilliseconds)
                {
                    logger.LogWarning("[WebSocketLivenessService] Closing connection {ConnectionId} for user {UserId} due to heartbeat timeout.", connection.ConnectionId, connection.UserId);
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure,
                        "Heartbeat timeout", cancellationToken);
                    connectionRegistry.Unregister(connection.ConnectionId);
                    continue;
                }

                try
                {
                    var pingMessage = RealtimeMessage.Create(RealtimeMessageType.Ping, nowUnix);

                    await messageSender.SendToUserAsync(connection.UserId, pingMessage, cancellationToken);

                    //await socket.SendAsync(segment, WebSocketMessageType.Binary, true, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "[WebSocketLivenessService] Error sending ping to connection {ConnectionId} for user {UserId}.", connection.ConnectionId, connection.UserId);
                }
            }

            await Task.Delay(_pingInterval, cancellationToken);
        }
    }
}
