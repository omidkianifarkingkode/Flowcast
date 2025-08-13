using Application.Abstractions.Realtime.Messaging;
using Application.Abstractions.Realtime.Services;

namespace Infrastructure.Realtime.Services;

public sealed class RealtimeContextAccessor : IRealtimeContextAccessor
{
    public RealtimeContext? Current { get; set; }
}
