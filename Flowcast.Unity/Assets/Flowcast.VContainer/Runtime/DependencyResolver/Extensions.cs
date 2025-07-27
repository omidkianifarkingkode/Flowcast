using Flowcast.Commands;
using Flowcast.Lockstep;
using Flowcast.Options;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using VContainer;

namespace Flowcast.VContainer
{
    public static class RegistrationExtensions
    {
        public static void RegisterFlowcast(this IContainerBuilder builder, params Assembly[] assembliesToScan) 
        {
            // Default to all loaded assemblies if none specified
            var assemblies = assembliesToScan?.Length > 0
                ? assembliesToScan
                : AppDomain.CurrentDomain.GetAssemblies();

            RegisterCommandValidators(builder, assemblies);


            RegisterLockstepSettings(builder);
        }


        /// <summary>
        /// Registers all ICommandValidator<T> implementations and the CommandValidatorFactory with VContainer.
        /// </summary>
        public static void RegisterCommandValidators(this IContainerBuilder builder, params Assembly[] assemblies)
        {
            // Step 1: Scan once and store all ICommandValidator<TCommand> mappings
            var validatorMappings = assemblies
                .SelectMany(asm => asm.GetTypes())
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .SelectMany(t => t.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommandValidator<>))
                    .Select(i => new { CommandType = i.GetGenericArguments()[0], ValidatorType = t }))
                .Distinct()
                .ToList();

            // Step 2: Register each validator in DI
            foreach (var mapping in validatorMappings)
            {
                var serviceType = typeof(ICommandValidator<>).MakeGenericType(mapping.CommandType);
                builder.Register(mapping.ValidatorType, Lifetime.Transient).As(serviceType).As(mapping.ValidatorType);
            }

            // Step 3: Register the factory using the pre-scanned mappings
            // Use a Holder Object + Deferred Factory Resolution (Recommended for Complex Setup)
            // 3.1 Register a placeholder holder as a container singleton.
            var holder = new CommandValidatorFactoryHolder();
            builder.RegisterInstance(holder);

            // 3.2 After the container is built register a build callback to create the actual factory
            builder.RegisterBuildCallback(container =>
            {
                holder.Factory = new CommandValidatorFactory(setup =>
                {
                    setup.UseCreator(type => (ICommandValidator)container.Resolve(type));
                    setup.MapGroup(validatorMappings.Select(m => (m.CommandType, m.ValidatorType)));
                });
            });

            // 3.3 Register ICommandValidatorFactory as resolved from the holder
            builder.Register(c => c.Resolve<CommandValidatorFactoryHolder>().Factory, Lifetime.Singleton);
        }

        private static void RegisterLockstepSettings(IContainerBuilder builder)
        {
            var settings = Resources.Load<LockstepEngineOptionsAsset>(LockstepEngineOptionsAsset.ResourceLoadPath);
            if (settings == null)
            {
                Debug.LogError($"LockstepSettingsAsset could not be found at Resources/{LockstepEngineOptionsAsset.ResourceLoadPath}");
            }
            else
            {
                builder.RegisterInstance<ILockstepSettings>(settings).AsImplementedInterfaces().AsSelf();
            }


        }
    }

    internal class CommandValidatorFactoryHolder
    {
        public ICommandValidatorFactory Factory { get; set; }
    }

}
