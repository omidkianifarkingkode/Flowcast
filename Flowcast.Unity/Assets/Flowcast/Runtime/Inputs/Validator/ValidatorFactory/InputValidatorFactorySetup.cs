using System;
using System.Collections.Generic;

namespace Flowcast.Inputs
{
    public class InputValidatorFactorySetup
    {
        public Dictionary<Type, Type> TypeMappings { get; }
        public Dictionary<Type, Func<IInputValidator>> FactoryMappings { get; }
        public Func<Type, IInputValidator> Creator { get; }

        public InputValidatorFactorySetup(
            Dictionary<Type, Type> typeMappings,
            Dictionary<Type, Func<IInputValidator>> factoryMappings,
            Func<Type, IInputValidator> creator)
        {
            TypeMappings = typeMappings ?? throw new ArgumentNullException(nameof(typeMappings));
            FactoryMappings = factoryMappings ?? throw new ArgumentNullException(nameof(factoryMappings));
            Creator = creator ?? throw new ArgumentNullException(nameof(creator));
        }
    }
}

