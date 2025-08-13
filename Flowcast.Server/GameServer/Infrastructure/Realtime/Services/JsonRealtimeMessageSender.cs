using Application.Abstractions.Realtime;
using Application.Abstractions.Realtime.Messaging;
using Application.Abstractions.Realtime.Services;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Infrastructure.Realtime.Services;

public class JsonRealtimeMessageSender(IUserConnectionRegistry connectionRegistry) : IRealtimeMessageSender 
{
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public async Task SendToUserAsync(Guid userId, RealtimeMessage message, CancellationToken cancellationToken = default)
    {
        if (connectionRegistry.TryGetWebSocketByUserId(userId, out var socket)
            && socket.State == WebSocketState.Open)
        {
            var json = JsonSerializer.Serialize(message, _options);
            var buffer = Encoding.UTF8.GetBytes(json);
            var segment = new ArraySegment<byte>(buffer);

            await socket.SendAsync(segment, WebSocketMessageType.Text, true, cancellationToken);
        }
    }
}