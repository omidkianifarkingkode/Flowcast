using Application.Abstractions.Messaging;
using SharedKernel;

namespace Application.Abstractions.Messaging.Defaults;

public class DefaultCommand : ICommand;

public class DefaultCommandHandle : ICommandHandler<DefaultCommand>
{
    public Task<Result> Handle(DefaultCommand command, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success());
    }
}

public class DefaultCommand1 : ICommand<bool>;

public class DefaultCommand1Handler : ICommandHandler<DefaultCommand1, bool>
{
    public Task<Result<bool>> Handle(DefaultCommand1 command, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success(true));
    }
}

