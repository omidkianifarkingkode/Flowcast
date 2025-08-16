using Application.Realtime.Commons;

namespace Application.Realtime.Services;

public interface IRealtimeContextAccessor
{
    RealtimeContext? Current { get; set; }
}
