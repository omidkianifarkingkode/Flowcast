using Application.Abstractions.Realtime;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net.WebSockets;
using System.Text;

namespace Infrastructure.Realtime;

public class HeartbeatBackgroundService(IUserConnectionRegistry connectionRegistry, ILogger<HeartbeatBackgroundService> logger) : BackgroundService
{
    private readonly TimeSpan _pingInterval = TimeSpan.FromSeconds(30);
    private readonly TimeSpan _timeout = TimeSpan.FromSeconds(60);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;

            foreach (var connection in connectionRegistry.GetAllConnections())
            {
                var socket = connection.Socket;
                if (socket.State != WebSocketState.Open)
                    continue;

                if (now - connection.LastPongUtc > _timeout)
                {
                    logger.LogWarning("Closing connection {ConnectionId} for user {UserId} due to heartbeat timeout.", connection.ConnectionId, connection.UserId);
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure,
                        "Heartbeat timeout", stoppingToken);
                    connectionRegistry.Unregister(connection.ConnectionId);
                    continue;
                }

                var pingMessage = Encoding.UTF8.GetBytes("{\"type\":\"ping\"}");
                try
                {
                    await socket.SendAsync(pingMessage, WebSocketMessageType.Text, true, stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error sending ping to connection {ConnectionId} for user {UserId}.", connection.ConnectionId, connection.UserId);
                }
            }

            await Task.Delay(_pingInterval, stoppingToken);
        }
    }
}
