using Application.Abstractions.Realtime;
using System.Net.WebSockets;
using System.Text;

namespace Infrastructure.Realtime;

public class UserConnectionSender(IUserConnectionRegistry connectionRegistry) : IUserConnectionSender
{
    public async Task SendToUserAsync(Guid userId, string message, CancellationToken cancellationToken = default)
    {
        if (connectionRegistry.TryGetWebSocketByUserId(userId, out var socket)
             && socket.State == WebSocketState.Open)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            var segment = new ArraySegment<byte>(buffer);

            await socket.SendAsync(segment, WebSocketMessageType.Text, true, cancellationToken);
        }
    }
}

