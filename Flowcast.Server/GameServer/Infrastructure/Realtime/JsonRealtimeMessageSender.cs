using Application.Abstractions.Realtime;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Realtime;

public class JsonRealtimeMessageSender(IUserConnectionRegistry connectionRegistry) : IRealtimeMessageSender 
{
    public async Task SendToUserAsync(Guid userId, RealtimeMessage message, CancellationToken cancellationToken = default)
    {
        if (connectionRegistry.TryGetWebSocketByUserId(userId, out var socket)
            && socket.State == WebSocketState.Open)
        {
            var json = JsonSerializer.Serialize(message);
            var buffer = Encoding.UTF8.GetBytes(json);
            var segment = new ArraySegment<byte>(buffer);

            await socket.SendAsync(segment, WebSocketMessageType.Text, true, cancellationToken);
        }
    }
}