using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Flowcast.Inputs
{
    public class InputValidatorFactorySetupBuilder
    {
        private readonly Dictionary<Type, Type> _mappings = new();
        private readonly Dictionary<Type, Func<IInputValidator>> _factories = new();
        private Func<Type, IInputValidator> _creator;

        /// <summary>
        /// Sets the default creator function used to instantiate validators when no factory is provided.
        /// </summary>
        public InputValidatorFactorySetupBuilder UseCreator(Func<Type, IInputValidator> creator)
        {
            _creator = creator ?? throw new ArgumentNullException(nameof(creator));
            return this;
        }

        /// <summary>
        /// Registers a mapping from an input type to its validator type.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if a validator is already registered for the input type.</exception>
        public InputValidatorFactorySetupBuilder Map<TInput, TValidator>()
            where TInput : IInput
            where TValidator : IInputValidator<TInput>
        {
            var inputType = typeof(TInput);
            if (_mappings.ContainsKey(inputType))
                throw new InvalidOperationException($"Validator already mapped for input type {inputType.Name}.");

            _mappings[inputType] = typeof(TValidator);
            return this;
        }

        /// <summary>
        /// Registers multiple validator mappings in bulk.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if any input type has already been registered.</exception>
        public InputValidatorFactorySetupBuilder Map(IEnumerable<(Type InputType, Type ValidatorType)> mappings)
        {
            foreach (var (inputType, validatorType) in mappings)
            {
                if (_mappings.ContainsKey(inputType))
                    throw new InvalidOperationException($"Validator already mapped for input type {inputType.Name}.");

                _mappings[inputType] = validatorType;
            }
            return this;
        }

        /// <summary>
        /// Registers a factory method for creating the validator for the given input type.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if a factory is already registered for the input type.</exception>
        public InputValidatorFactorySetupBuilder Map<TInput>(Func<IInputValidator<TInput>> factory)
            where TInput : IInput
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            var inputType = typeof(TInput);

            if (_factories.ContainsKey(inputType))
                throw new InvalidOperationException($"Instance already mapped for input type {inputType.Name}.");

            _factories[inputType] = () => factory();
            return this;
        }

        /// <summary>
        /// Scans the specified assemblies for IInputValidator<T> implementations and registers them automatically.
        /// Skips types that are already registered.
        /// </summary>
        public InputValidatorFactorySetupBuilder AutoMap(params Assembly[] assemblies)
        {
            var allAssemblies = assemblies.Length > 0 ? assemblies : AppDomain.CurrentDomain.GetAssemblies();

            var validatorTypes = allAssemblies
                .SelectMany(asm => asm.GetTypes())
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .SelectMany(t => t.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IInputValidator<>))
                    .Select(i => new { InputType = i.GetGenericArguments()[0], ValidatorType = t }));

            foreach (var map in validatorTypes)
            {
                if (_mappings.ContainsKey(map.InputType)) continue;
                _mappings[map.InputType] = map.ValidatorType;
            }

            return this;
        }

        /// <summary>
        /// Finalizes the configuration and returns the factory setup with all mappings and factories.
        /// </summary>
        public InputValidatorFactorySetup Build()
        {
            _creator ??= type => (IInputValidator)Activator.CreateInstance(type);

            return new InputValidatorFactorySetup(_mappings, _factories, _creator);
        }
    }
}

