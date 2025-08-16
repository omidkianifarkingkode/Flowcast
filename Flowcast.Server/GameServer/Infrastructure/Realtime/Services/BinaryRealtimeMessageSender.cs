using Application.Realtime.Messaging;
using Application.Realtime.Services;
using System.Net.WebSockets;

namespace Infrastructure.Realtime.Services;

public class BinaryRealtimeMessageSender(
    IUserConnectionRegistry connectionRegistry,
    IRealtimeMessageCodec codec) : IRealtimeMessageSender
{
    public async Task SendToUserAsync(Guid userId, RealtimeMessage message, CancellationToken cancellationToken = default)
    {
        if (connectionRegistry.TryGetWebSocketByUserId(userId, out var socket)
             && socket.State == WebSocketState.Open)
        {
            var final = AttachTelemetryIfAllowed(userId, message);
            var buffer = codec.ToBytes(final);
            await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Binary, true, cancellationToken);
        }
    }

    public async Task SendToUserAsync<T>(Guid userId, RealtimeMessage<T> message, CancellationToken cancellationToken = default)
        where T : IRealtimeCommand
    {
        if (connectionRegistry.TryGetWebSocketByUserId(userId, out var socket)
             && socket.State == WebSocketState.Open)
        {
            var final = AttachTelemetryIfAllowed(userId, message);
            var buffer = codec.ToBytes(final);
            await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Binary, true, cancellationToken);
        }
    }

    // --- helpers ---
    private RealtimeMessage AttachTelemetryIfAllowed(Guid userId, RealtimeMessage msg)
    {
        // Respect caller: if telemetry already present, keep it.
        if ((msg.Header.Flags & HeaderFlags.HasTelemetry) != 0 && msg.Header.Telemetry.HasValue)
            return msg;

        // Do NOT auto-attach for Ping; leave it to liveness service (option-controlled).
        if (msg.Header.Type == RealtimeMessageType.Ping)
            return msg;

        if (connectionRegistry.TryGetTelemetry(userId, out var telem))
        {
            var hdr = new MessageHeader(msg.Header.Type, msg.Header.Id, msg.Header.Timestamp,
                                        msg.Header.Flags | HeaderFlags.HasTelemetry, telem);
            return new RealtimeMessage(hdr);
        }
        return msg;
    }

    private RealtimeMessage<T> AttachTelemetryIfAllowed<T>(Guid userId, RealtimeMessage<T> msg) where T : IRealtimeCommand
    {
        if ((msg.Header.Flags & HeaderFlags.HasTelemetry) != 0 && msg.Header.Telemetry.HasValue)
            return msg;

        if (msg.Header.Type == RealtimeMessageType.Ping)
            return msg;

        if (connectionRegistry.TryGetTelemetry(userId, out var telem))
        {
            var hdr = new MessageHeader(msg.Header.Type, msg.Header.Id, msg.Header.Timestamp,
                                        msg.Header.Flags | HeaderFlags.HasTelemetry, telem);
            return new RealtimeMessage<T>(hdr, msg.Payload);
        }
        return msg;
    }
}
