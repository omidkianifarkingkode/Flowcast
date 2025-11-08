using System;
using System.Collections.Generic;

namespace Flowcast.Commands
{
    public class CommandProcessorFactoryOptions
    {
        public Dictionary<Type, Type> TypeMappings { get; }
        public Dictionary<Type, Func<ICommandProcessor>> FactoryMappings { get; }
        public Func<Type, ICommandProcessor> Creator { get; }

        public CommandProcessorFactoryOptions(
            Dictionary<Type, Type> typeMappings,
            Dictionary<Type, Func<ICommandProcessor>> factoryMappings,
            Func<Type, ICommandProcessor> creator)
        {
            TypeMappings = typeMappings ?? throw new ArgumentNullException(nameof(typeMappings));
            FactoryMappings = factoryMappings ?? throw new ArgumentNullException(nameof(factoryMappings));
            Creator = creator ?? throw new ArgumentNullException(nameof(creator));
        }
    }
}

