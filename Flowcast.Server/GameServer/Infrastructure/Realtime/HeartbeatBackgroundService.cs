using Application.Abstractions.Realtime;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SharedKernel;
using System.Net.WebSockets;
using System.Text;

namespace Infrastructure.Realtime;

public class HeartbeatBackgroundService(IUserConnectionRegistry connectionRegistry,
    IDateTimeProvider dateTimeProvider,
    IRealtimeMessageSender messageSender,
    ILogger<HeartbeatBackgroundService> logger) : BackgroundService
{
    private readonly TimeSpan _pingInterval = TimeSpan.FromSeconds(15);
    private readonly TimeSpan _timeout = TimeSpan.FromSeconds(60);

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
                    logger.LogWarning("Closing connection {ConnectionId} for user {UserId} due to heartbeat timeout.", connection.ConnectionId, connection.UserId);
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure,
                        "Heartbeat timeout", cancellationToken);
                    connectionRegistry.Unregister(connection.ConnectionId);
                    continue;
                }

                try
                {
                    var pingMessage = RealtimeMessage.Create(RealtimeMessageType.Ping, nowUnix, []);
                    var pingBytes = pingMessage.ToBytes();
                    var segment = new ArraySegment<byte>(pingBytes);

                    await messageSender.SendToUserAsync(connection.UserId, pingMessage, cancellationToken);

                    //await socket.SendAsync(segment, WebSocketMessageType.Binary, true, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error sending ping to connection {ConnectionId} for user {UserId}.", connection.ConnectionId, connection.UserId);
                }
            }

            await Task.Delay(_pingInterval, cancellationToken);
        }
    }
}
