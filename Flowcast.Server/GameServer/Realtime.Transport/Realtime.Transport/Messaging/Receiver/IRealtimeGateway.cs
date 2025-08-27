using Realtime.Transport.Http;

namespace Realtime.Transport.Messaging.Receiver;

public interface IRealtimeGateway
{
    event Action<RealtimeContext, IRealtimeMessage> OnFrame;
    IAsyncEnumerable<(RealtimeContext ctx, IRealtimeMessage frame)> ReadAllAsync(CancellationToken ct);
}

