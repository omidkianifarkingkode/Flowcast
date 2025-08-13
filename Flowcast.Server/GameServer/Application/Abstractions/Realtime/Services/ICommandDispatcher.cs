using Application.Abstractions.Realtime.Messaging;
using SharedKernel;

namespace Application.Abstractions.Realtime.Services;

public interface ICommandDispatcher
{
    Task<Result> DispatchAsync(Guid userId, IRealtimeMessage message, CancellationToken ct);
}
