using System;
using System.Collections.Generic;

namespace Flowcast.Commands
{
    public interface ICommandValidatorFactory
    {
        ICommandValidator GetValidator(Type commandType);
        ICommandValidator<TCommand> GetValidator<TCommand>() where TCommand : ICommand;
    }

    public class CommandValidatorFactory : ICommandValidatorFactory
    {
        private readonly Dictionary<Type, Type> _typeMappings;
        private readonly Dictionary<Type, Func<ICommandValidator>> _factoryMappings;
        private readonly Func<Type, ICommandValidator> _creator;

        public CommandValidatorFactory(Action<CommandValidatorFactoryOptionsBuilder> configure)
        {
            var builder = new CommandValidatorFactoryOptionsBuilder();
            configure?.Invoke(builder);
            var setup = builder.Build();

            _typeMappings = setup.TypeMappings;
            _factoryMappings = setup.FactoryMappings;
            _creator = setup.Creator;
        }

        public CommandValidatorFactory(CommandValidatorFactoryOptions setup)
        {
            _typeMappings = setup.TypeMappings;
            _factoryMappings = setup.FactoryMappings;
            _creator = setup.Creator;
        }

        public ICommandValidator GetValidator(Type commandType)
        {
            if (_factoryMappings.TryGetValue(commandType, out var factoryFunc))
            {
                var validatorInstance = factoryFunc();
                return validatorInstance ?? throw new InvalidOperationException($"Factory function returned null for {commandType.Name}");
            }

            if (!_typeMappings.TryGetValue(commandType, out var validatorType))
                return default;

            var validator = _creator(validatorType);
            return validator ?? throw new InvalidOperationException($"Validator creation failed for '{validatorType.Name}'.");
        }

        public ICommandValidator<TCommand> GetValidator<TCommand>() where TCommand : ICommand
        {
            var validator = GetValidator(typeof(TCommand));

            if (validator is not ICommandValidator<TCommand> typed)
                throw new InvalidCastException($"Validator is not of expected type ICommandValidator<{typeof(TCommand).Name}>.");

            return typed;
        }
    }
}

