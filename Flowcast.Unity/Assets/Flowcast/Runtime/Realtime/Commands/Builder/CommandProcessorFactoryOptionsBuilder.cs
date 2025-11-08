using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Flowcast.Commands
{
    public class CommandProcessorFactoryOptionsBuilder
    {
        private readonly Dictionary<Type, Type> _mappings = new();
        private readonly Dictionary<Type, Func<ICommandProcessor>> _factories = new();
        private Func<Type, ICommandProcessor> _creator;

        /// <summary>
        /// Sets the default creator function used to instantiate command processor when no factory is provided.
        /// </summary>
        public CommandProcessorFactoryOptionsBuilder UseCreator(Func<Type, ICommandProcessor> creator)
        {
            _creator = creator ?? throw new ArgumentNullException(nameof(creator));
            return this;
        }

        /// <summary>
        /// Registers a mapping from an command type to its processor type.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if a processor is already registered for the command type.</exception>
        public CommandProcessorFactoryOptionsBuilder MapManual<TCommand, TProcessor>()
            where TCommand : ICommand
            where TProcessor : ICommandProcessor<TCommand>
        {
            var commandType = typeof(TCommand);
            if (_mappings.ContainsKey(commandType))
                throw new InvalidOperationException($"Processor already mapped for command type {commandType.Name}.");

            _mappings[commandType] = typeof(TProcessor);
            return this;
        }

        /// <summary>
        /// Registers multiple command processor mappings in bulk.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if any command type has already been registered.</exception>
        public CommandProcessorFactoryOptionsBuilder MapGroup(IEnumerable<(Type CommandType, Type ProcessorType)> mappings)
        {
            foreach (var (commandType, processorType) in mappings)
            {
                if (_mappings.ContainsKey(commandType))
                    throw new InvalidOperationException($"Processor already mapped for command type {commandType.Name}.");

                _mappings[commandType] = processorType;
            }
            return this;
        }

        /// <summary>
        /// Registers a factory method for creating the processor for the given command type.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if a factory is already registered for the command type.</exception>
        public CommandProcessorFactoryOptionsBuilder MapLazy<TCommand>(Func<ICommandProcessor<TCommand>> factory)
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
        /// Scans the specified assemblies for ICommandProcessor<T> implementations and registers them automatically.
        /// Skips types that are already registered.
        /// </summary>
        public CommandProcessorFactoryOptionsBuilder AutoMap(params Assembly[] assemblies)
        {
            var allAssemblies = assemblies.Length > 0 ? assemblies : AppDomain.CurrentDomain.GetAssemblies();

            var processorTypes = allAssemblies
                .SelectMany(asm => asm.GetTypes())
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .SelectMany(t => t.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommandProcessor<>))
                    .Select(i => new { CommandType = i.GetGenericArguments()[0], ProcessorType = t }));

            foreach (var map in processorTypes)
            {
                if (_mappings.ContainsKey(map.CommandType)) continue;
                _mappings[map.CommandType] = map.ProcessorType;
            }

            return this;
        }

        /// <summary>
        /// Finalizes the configuration and returns the factory setup with all mappings and factories.
        /// </summary>
        public CommandProcessorFactoryOptions Build()
        {
            _creator ??= type => (ICommandProcessor)Activator.CreateInstance(type);

            return new CommandProcessorFactoryOptions(_mappings, _factories, _creator);
        }
    }
}

