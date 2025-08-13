using Application.Abstractions.Realtime.Messaging;

namespace Application.Abstractions.Realtime.Services;

public interface IRealtimeContextAccessor
{
    RealtimeContext? Current { get; set; }
}
