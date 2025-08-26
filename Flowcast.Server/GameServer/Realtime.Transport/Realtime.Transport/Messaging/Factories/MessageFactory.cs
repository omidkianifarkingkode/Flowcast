using Realtime.Transport.UserConnection;

namespace Realtime.Transport.Messaging.Factories;

public interface IMessageFactory
{
    MessageHeader CreateHeader(
        ushort type,
        string? userId = null,
        HeaderFlags extraFlags = HeaderFlags.None,
        TelemetrySegment? overrideTelemetry = null);

    RealtimeMessage Create(
        ushort type,
        string? userIdForTelemetry = null,
        HeaderFlags extraFlags = HeaderFlags.None,
        TelemetrySegment? overrideTelemetry = null);

    RealtimeMessage<TPayload> Create<TPayload>(
        ushort type,
        TPayload payload,
        string? userId = null,
        HeaderFlags extraFlags = HeaderFlags.None,
        TelemetrySegment? overrideTelemetry = null)
        where TPayload : IPayload;
}

public sealed class MessageFactory(IUserConnectionRegistry connections) : IMessageFactory
{
    public MessageHeader CreateHeader(ushort type, string? userId = null,
                                HeaderFlags extraFlags = HeaderFlags.None, TelemetrySegment? overrideTelemetry = null)
    {
        return BuildHeader(type, userId, extraFlags, overrideTelemetry);
    }

    public RealtimeMessage Create(
        ushort type,
        string? userId = null,
        HeaderFlags extraFlags = HeaderFlags.None,
        TelemetrySegment? overrideTelemetry = null)
    {
        var header = BuildHeader(type, userId, extraFlags, overrideTelemetry);
        return new RealtimeMessage(header);
    }

    public RealtimeMessage<TPayload> Create<TPayload>(
        ushort type,
        TPayload payload,
        string? userId = null,
        HeaderFlags extraFlags = HeaderFlags.None,
        TelemetrySegment? overrideTelemetry = null)
        where TPayload : IPayload
    {
        var header = BuildHeader(type, userId, extraFlags, overrideTelemetry);
        return new RealtimeMessage<TPayload>(header, payload);
    }

    private MessageHeader BuildHeader(
        ushort type,
        string? userId,
        HeaderFlags extraFlags,
        TelemetrySegment? overrideTelemetry)
    {
        var id = IdGenerator.NewId();
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // Telemetry: skip for Ping/Pong; else attach if provided or available.
        TelemetrySegment? telemetry = null;
        var flags = extraFlags;

        if (type is not RealtimeMessageType.Ping and not RealtimeMessageType.Pong)
        {
            if (overrideTelemetry.HasValue)
            {
                telemetry = overrideTelemetry.Value;
                flags |= HeaderFlags.HasTelemetry;
            }
            else if (userId is not null && connections.TryGetTelemetry(userId, out var t))
            {
                telemetry = t;
                flags |= HeaderFlags.HasTelemetry;
            }
        }

        return new MessageHeader(type, id, now, flags, telemetry);
    }
}
