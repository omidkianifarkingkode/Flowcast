using System;

namespace Flowcast.Commands
{
    public class CommandOptionsBuilder : ICommandOptionsBuilderStart, ICommandOptionsBuilderStep1, ICommandOptionsBuilderStep2
    {
        public CommandOptions Options { get; private set; } = new();

        public ICommandOptionsBuilderStep1 OnCommandReceived(Action<ICommand> callback)
        {
            Options.OnCommandReceived = callback;
            return this;
        }

        public ICommandOptionsBuilderStep2 HandleCommandsOnGameFrame()
        {
            Options.HandleOnGameFrame = true;
            return this;
        }

        public ICommandOptionsBuilderStep2 HandleCommandsOnLockstepTurn()
        {
            Options.HandleOnLockstepTurn = true;
            return this;
        }

        public ICommandOptionsBuilderStep2 SetupValidatorFactory(Action<CommandValidatorFactoryOptionsBuilder> configure)
        {
            var builder = new CommandValidatorFactoryOptionsBuilder();
            configure(builder);
            Options.ValidatorFactoryOptions = builder.Build();
            return this;
        }

        public ICommandOptionsBuilderStep2 SetupProcessorFactory(Action<CommandProcessorFactoryOptionsBuilder> configure)
        {
            var builder = new CommandProcessorFactoryOptionsBuilder();
            configure(builder);
            Options.CommandFactoryOptions = builder.Build();
            return this;
        }

        internal CommandOptions Build()
        {
            if (Options.ValidatorFactoryOptions == null)
                SetupValidatorFactory(config => config.AutoMap());

            if (Options.CommandFactoryOptions == null)
                SetupProcessorFactory(config => config.AutoMap());

            if (Options.HandleOnGameFrame == false && Options.HandleOnLockstepTurn == false)
                Options.HandleOnGameFrame = true;

            return Options;
        }
    }

}
