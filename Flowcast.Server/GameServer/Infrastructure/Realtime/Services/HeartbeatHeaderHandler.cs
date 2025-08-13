using Application.Abstractions.Realtime;
using Application.Abstractions.Realtime.Messaging;
using Application.Abstractions.Realtime.Services;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Infrastructure.Realtime.Services;

/// <summary>
/// Updates liveness when a Pong header-only message is received.
/// </summary>
public sealed class HeartbeatHeaderHandler(
    IUserConnectionRegistry registry,
    IDateTimeProvider clock,
    ILogger<HeartbeatHeaderHandler> logger)
    : IHeaderMessageHandler
{
    public Task<bool> TryHandleAsync(Guid userId, RealtimeMessage message, CancellationToken ct)
    {
        if (message.Header.Type != RealtimeMessageType.Pong)
            return Task.FromResult(false);

        // Use server receive time to avoid clock skew
        var now = clock.UnixTimeMilliseconds;
        registry.MarkPongReceived(userId, now);

        logger.LogDebug("PONG received for user {UserId}. Marked at {Now}. (Header ts: {HeaderTs})",
            userId, now, message.Header.Timestamp);

        return Task.FromResult(true);
    }
}
