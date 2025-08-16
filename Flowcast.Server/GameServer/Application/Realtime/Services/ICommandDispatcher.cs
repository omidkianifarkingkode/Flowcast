using Application.Realtime.Messaging;
using SharedKernel;

namespace Application.Realtime.Services;

public interface ICommandDispatcher
{
    Task<Result> DispatchAsync(Guid userId, IRealtimeMessage message, CancellationToken ct);
}
