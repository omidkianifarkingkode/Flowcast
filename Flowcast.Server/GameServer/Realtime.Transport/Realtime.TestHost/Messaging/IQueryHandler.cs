using Realtime.TestHost.Messaging;
using SharedKernel;

namespace Realtime.TestHost.Messaging;

public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken);
}
