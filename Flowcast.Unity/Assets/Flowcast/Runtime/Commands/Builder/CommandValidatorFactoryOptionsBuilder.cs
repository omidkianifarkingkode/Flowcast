using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Flowcast.Commands
{
    public class CommandValidatorFactoryOptionsBuilder
    {
        private readonly Dictionary<Type, Type> _mappings = new();
        private readonly Dictionary<Type, Func<ICommandValidator>> _factories = new();
        private Func<Type, ICommandValidator> _creator;

        /// <summary>
        /// Sets the default creator function used to instantiate validators when no factory is provided.
        /// </summary>
        public CommandValidatorFactoryOptionsBuilder UseCreator(Func<Type, ICommandValidator> creator)
        {
            _creator = creator ?? throw new ArgumentNullException(nameof(creator));
            return this;
        }

        /// <summary>
        /// Registers a mapping from an command type to its validator type.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if a validator is already registered for the command type.</exception>
        public CommandValidatorFactoryOptionsBuilder MapManual<TCommand, TValidator>()
            where TCommand : ICommand
            where TValidator : ICommandValidator<TCommand>
        {
            var commandType = typeof(TCommand);
            if (_mappings.ContainsKey(commandType))
                throw new InvalidOperationException($"Validator already mapped for command type {commandType.Name}.");

            _mappings[commandType] = typeof(TValidator);
            return this;
        }

        /// <summary>
        /// Registers multiple validator mappings in bulk.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if any command type has already been registered.</exception>
        public CommandValidatorFactoryOptionsBuilder MapGroup(IEnumerable<(Type CommandType, Type ValidatorType)> mappings)
        {
            foreach (var (commandType, validatorType) in mappings)
            {
                if (_mappings.ContainsKey(commandType))
                    throw new InvalidOperationException($"Validator already mapped for command type {commandType.Name}.");

                _mappings[commandType] = validatorType;
            }
            return this;
        }

        /// <summary>
        /// Registers a factory method for creating the validator for the given command type.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if a factory is already registered for the command type.</exception>
        public CommandValidatorFactoryOptionsBuilder MapLazy<TCommand>(Func<ICommandValidator<TCommand>> factory)
            where TCommand : ICommand
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            var commandType = typeof(TCommand);

            if (_factories.ContainsKey(commandType))
                throw new InvalidOperationException($"Instance already mapped for command type {commandType.Name}.");

            _factories[commandType] = () => factory();
            return this;
        }

        /// <summary>
        /// Scans the specified assemblies for ICommandValidator<T> implementations and registers them automatically.
        /// Skips types that are already registered.
        /// </summary>
        public CommandValidatorFactoryOptionsBuilder AutoMap(params Assembly[] assemblies)
        {
            var allAssemblies = assemblies.Length > 0 ? assemblies : AppDomain.CurrentDomain.GetAssemblies();

            var validatorTypes = allAssemblies
                .SelectMany(asm => asm.GetTypes())
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .SelectMany(t => t.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommandValidator<>))
                    .Select(i => new { CommandType = i.GetGenericArguments()[0], ProcessorType = t }));

            foreach (var map in validatorTypes)
            {
                if (_mappings.ContainsKey(map.CommandType)) continue;
                _mappings[map.CommandType] = map.ProcessorType;
            }

            return this;
        }

        /// <summary>
        /// Finalizes the configuration and returns the factory setup with all mappings and factories.
        /// </summary>
        public CommandValidatorFactoryOptions Build()
        {
            _creator ??= type => (ICommandValidator)Activator.CreateInstance(type);

            return new CommandValidatorFactoryOptions(_mappings, _factories, _creator);
        }
    }
}

