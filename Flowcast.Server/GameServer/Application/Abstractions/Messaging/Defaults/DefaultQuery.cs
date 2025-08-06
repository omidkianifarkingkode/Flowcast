using SharedKernel;

namespace Application.Abstractions.Messaging.Defaults;

public class DefaultQuery : IQuery<bool>;

public class DefaultQueryHandler : IQueryHandler<DefaultQuery, bool>
{
    public Task<Result<bool>> Handle(DefaultQuery command, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success(true));
    }
}

