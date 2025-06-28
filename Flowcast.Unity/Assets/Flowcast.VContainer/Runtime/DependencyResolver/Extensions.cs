using Flowcast.Inputs;
using Flowcast.Lockstep;
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

            RegisterInputValidators(builder, assemblies);


            RegisterLockstepSettings(builder);
        }


        /// <summary>
        /// Registers all IInputValidator<T> implementations and the InputValidatorFactory with VContainer.
        /// </summary>
        public static void RegisterInputValidators(this IContainerBuilder builder, params Assembly[] assemblies)
        {
            // Step 1: Scan once and store all IInputValidator<TInput> mappings
            var validatorMappings = assemblies
                .SelectMany(asm => asm.GetTypes())
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .SelectMany(t => t.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IInputValidator<>))
                    .Select(i => new { InputType = i.GetGenericArguments()[0], ValidatorType = t }))
                .Distinct()
                .ToList();

            // Step 2: Register each validator in DI
            foreach (var mapping in validatorMappings)
            {
                var serviceType = typeof(IInputValidator<>).MakeGenericType(mapping.InputType);
                builder.Register(mapping.ValidatorType, Lifetime.Transient).As(serviceType).As(mapping.ValidatorType);
            }

            // Step 3: Register the factory using the pre-scanned mappings
            // Use a Holder Object + Deferred Factory Resolution (Recommended for Complex Setup)
            // 3.1 Register a placeholder holder as a container singleton.
            var holder = new InputValidatorFactoryHolder();
            builder.RegisterInstance(holder);

            // 3.2 After the container is built register a build callback to create the actual factory
            builder.RegisterBuildCallback(container =>
            {
                holder.Factory = new InputValidatorFactory(setup =>
                {
                    setup.UseCreator(type => (IInputValidator)container.Resolve(type));
                    setup.Map(validatorMappings.Select(m => (m.InputType, m.ValidatorType)));
                });
            });

            // 3.3 Register IInputValidatorFactory as resolved from the holder
            builder.Register(c => c.Resolve<InputValidatorFactoryHolder>().Factory, Lifetime.Singleton);
        }

        private static void RegisterLockstepSettings(IContainerBuilder builder)
        {
            var settings = Resources.Load<LockstepSettingsAsset>(LockstepSettingsAsset.ResourceLoadPath);
            if (settings == null)
            {
                Debug.LogError($"LockstepSettingsAsset could not be found at Resources/{LockstepSettingsAsset.ResourceLoadPath}");
            }
            else
            {
                builder.RegisterInstance<ILockstepSettings>(settings).AsImplementedInterfaces().AsSelf();
            }


        }
    }

    internal class InputValidatorFactoryHolder
    {
        public IInputValidatorFactory Factory { get; set; }
    }

}
