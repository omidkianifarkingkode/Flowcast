using System;

namespace Flowcast.Commands
{
    public interface ICommandOptionsBuilderStart
    {
        ICommandOptionsBuilderStep1 OnCommandReceived(Action<ICommand> callback);
    }

    public interface ICommandOptionsBuilderStep1
    {
        ICommandOptionsBuilderStep2 HandleCommandsOnGameFrame();
        ICommandOptionsBuilderStep2 HandleCommandsOnLockstepTurn();
    }

    public interface ICommandOptionsBuilderStep2
    {
        ICommandOptionsBuilderStep2 SetupValidatorFactory(Action<CommandValidatorFactoryOptionsBuilder> setup);
        ICommandOptionsBuilderStep2 SetupProcessorFactory(Action<CommandProcessorFactoryOptionsBuilder> setup);
    }
}
