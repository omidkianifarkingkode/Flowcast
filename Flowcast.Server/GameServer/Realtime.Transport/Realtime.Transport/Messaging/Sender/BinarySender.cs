using Realtime.Transport.Messaging.Codec;
using Realtime.Transport.UserConnection;
using System.Net.WebSockets;

namespace Realtime.Transport.Messaging.Sender;

public class BinarySender(IUserConnectionRegistry connectionRegistry, ICodec codec) 
    : IRealtimeMessageSender
{
    public async Task SendToUserAsync(string userId, RealtimeMessage message, CancellationToken cancellationToken = default)
    {
        if (connectionRegistry.TryGetWebSocketByUserId(userId, out var socket)
             && socket.State == WebSocketState.Open)
        {
            var final = AttachTelemetryIfAllowed(userId, message);
            var buffer = codec.ToBytes(final);
            await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Binary, true, cancellationToken);
        }
    }

    public async Task SendToUserAsync<TPayload>(string userId, RealtimeMessage<TPayload> message, CancellationToken cancellationToken = default)
        where TPayload : IPayload
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
    private RealtimeMessage AttachTelemetryIfAllowed(string userId, RealtimeMessage msg)
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

    private RealtimeMessage<TPayload> AttachTelemetryIfAllowed<TPayload>(string userId, RealtimeMessage<TPayload> msg)
        where TPayload : IPayload
    {
        if ((msg.Header.Flags & HeaderFlags.HasTelemetry) != 0 && msg.Header.Telemetry.HasValue)
            return msg;

        if (msg.Header.Type == RealtimeMessageType.Ping)
            return msg;

        if (connectionRegistry.TryGetTelemetry(userId, out var telem))
        {
            var hdr = new MessageHeader(msg.Header.Type, msg.Header.Id, msg.Header.Timestamp,
                                        msg.Header.Flags | HeaderFlags.HasTelemetry, telem);
            return new RealtimeMessage<TPayload>(hdr, msg.Payload);
        }
        return msg;
    }
}
