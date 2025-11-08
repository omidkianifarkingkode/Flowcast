using System;
using System.Collections.Generic;

namespace Flowcast.Commands
{
    public interface ICommandProcessorFactory
    {
        ICommandProcessor GetProcessor(Type commandType);
        ICommandProcessor<TCommand> GetProcessor<TCommand>() where TCommand : ICommand;
    }

    public class CommandProcessorFactory : ICommandProcessorFactory
    {
        private readonly Dictionary<Type, Type> _typeMappings;
        private readonly Dictionary<Type, Func<ICommandProcessor>> _factoryMappings;
        private readonly Func<Type, ICommandProcessor> _creator;

        public CommandProcessorFactory(Action<CommandProcessorFactoryOptionsBuilder> configure)
        {
            var builder = new CommandProcessorFactoryOptionsBuilder();
            configure?.Invoke(builder);
            var setup = builder.Build();

            _typeMappings = setup.TypeMappings;
            _factoryMappings = setup.FactoryMappings;
            _creator = setup.Creator;
        }

        public CommandProcessorFactory(CommandProcessorFactoryOptions setup)
        {
            _typeMappings = setup.TypeMappings;
            _factoryMappings = setup.FactoryMappings;
            _creator = setup.Creator;
        }

        public ICommandProcessor GetProcessor(Type commandType)
        {
            if (_factoryMappings.TryGetValue(commandType, out var factoryFunc))
            {
                var instance = factoryFunc();
                return instance ?? throw new InvalidOperationException($"Factory function returned null for {commandType.Name}");
            }

            if (!_typeMappings.TryGetValue(commandType, out var processorType))
                return default;

            var processor = _creator(processorType);
            return processor ?? throw new InvalidOperationException($"Processor creation failed for '{processorType.Name}'.");
        }

        public ICommandProcessor<TCommand> GetProcessor<TCommand>() where TCommand : ICommand
        {
            var processor = GetProcessor(typeof(TCommand));

            if (processor is not ICommandProcessor<TCommand> typed)
                throw new InvalidCastException($"Processor is not of expected type ICommandProcessor<{typeof(TCommand).Name}>.");

            return typed;
        }
    }
}

