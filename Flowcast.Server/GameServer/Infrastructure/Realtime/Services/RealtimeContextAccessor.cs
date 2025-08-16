using Application.Realtime.Commons;
using Application.Realtime.Services;

namespace Infrastructure.Realtime.Services;

public sealed class RealtimeContextAccessor : IRealtimeContextAccessor
{
    public RealtimeContext? Current { get; set; }
}
