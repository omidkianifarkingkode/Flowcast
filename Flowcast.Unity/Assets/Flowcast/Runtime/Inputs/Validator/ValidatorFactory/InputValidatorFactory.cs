using System;
using System.Collections.Generic;

namespace Flowcast.Inputs
{
    public class InputValidatorFactory : IInputValidatorFactory
    {
        private readonly Dictionary<Type, Type> _typeMappings;
        private readonly Dictionary<Type, Func<IInputValidator>> _factoryMappings;
        private readonly Func<Type, IInputValidator> _creator;

        public InputValidatorFactory(Action<InputValidatorFactorySetupBuilder> configure)
        {
            var builder = new InputValidatorFactorySetupBuilder();
            configure?.Invoke(builder);
            var setup = builder.Build();

            _typeMappings = setup.TypeMappings;
            _factoryMappings = setup.FactoryMappings;
            _creator = setup.Creator;
        }

        public IInputValidator GetValidator(Type inputType)
        {
            if (_factoryMappings.TryGetValue(inputType, out var factoryFunc))
            {
                var validatorInstance = factoryFunc();
                return validatorInstance ?? throw new InvalidOperationException($"Factory function returned null for {inputType.Name}");
            }

            if (!_typeMappings.TryGetValue(inputType, out var validatorType))
                throw new KeyNotFoundException($"No validator mapped for input type '{inputType.Name}'.");

            var validator = _creator(validatorType);
            return validator ?? throw new InvalidOperationException($"Validator creation failed for '{validatorType.Name}'.");
        }

        public IInputValidator<TInput> GetValidator<TInput>() where TInput : IInput
        {
            var validator = GetValidator(typeof(TInput));

            if (validator is not IInputValidator<TInput> typed)
                throw new InvalidCastException($"Validator is not of expected type IInputValidator<{typeof(TInput).Name}>.");

            return typed;
        }
    }
}

