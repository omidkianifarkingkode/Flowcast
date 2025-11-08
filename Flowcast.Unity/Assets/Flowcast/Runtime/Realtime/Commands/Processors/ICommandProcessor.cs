using System;

namespace Flowcast.Commands
{
    public interface ICommandProcessor
    {
        void Process(ICommand command);
    }

    public interface ICommandProcessor<TCommand> : ICommandProcessor where TCommand : Commands.ICommand
    {
        void Process(TCommand command);

        void ICommandProcessor.Process(ICommand command)
        {
            if (command is TCommand typed)
            {
                Process(typed);
            }
            else
            {
                throw new InvalidCastException($"Command type mismatch: expected {typeof(TCommand).Name}");
            }
        }
    }
}

