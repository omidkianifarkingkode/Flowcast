using Flowcast.Commons;

namespace Flowcast.Commands
{
    public interface ICommandValidator
    {
        Result Validate(ICommand command);
    }

    public interface ICommandValidator<TCommand> : ICommandValidator where TCommand : ICommand
    {
        Result Validate(TCommand command);

        Result ICommandValidator.Validate(ICommand command)
        {
            if (command is TCommand typed)
            {
                return Validate(typed);
            }

            return Result.Failure("Invalid command type.");
        }
    }
}

