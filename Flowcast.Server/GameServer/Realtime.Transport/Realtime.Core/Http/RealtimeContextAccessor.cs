namespace Realtime.Transport.Http;

public interface IRealtimeContextAccessor
{
    RealtimeContext? Current { get; set; }
}

public sealed class RealtimeContextAccessor : IRealtimeContextAccessor
{
    public RealtimeContext? Current { get; set; }
}