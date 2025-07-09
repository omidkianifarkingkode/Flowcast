using System;
using System.Collections.Generic;

namespace Flowcast.Commands
{
    public class CommandValidatorFactoryOptions
    {
        public Dictionary<Type, Type> TypeMappings { get; }
        public Dictionary<Type, Func<ICommandValidator>> FactoryMappings { get; }
        public Func<Type, ICommandValidator> Creator { get; }

        public CommandValidatorFactoryOptions(
            Dictionary<Type, Type> typeMappings,
            Dictionary<Type, Func<ICommandValidator>> factoryMappings,
            Func<Type, ICommandValidator> creator)
        {
            TypeMappings = typeMappings ?? throw new ArgumentNullException(nameof(typeMappings));
            FactoryMappings = factoryMappings ?? throw new ArgumentNullException(nameof(factoryMappings));
            Creator = creator ?? throw new ArgumentNullException(nameof(creator));
        }
    }
}

