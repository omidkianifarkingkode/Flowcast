using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Behaviors;
using Shared.Application.Messaging;
using Shared.Application.Services;
using SharedKernel;
using System.Reflection;

namespace Shared.Application;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddCQRS(this WebApplicationBuilder builder, params Assembly[] assemblies)
    {
        if (assemblies is null || assemblies.Length == 0)
            assemblies = SafeDomainAssemblies();

        // Handlers
        builder.Services.Scan(scan => scan.FromAssemblies(assemblies)
            .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<,>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        // Decorators
        TryDecorate(builder.Services, typeof(ICommandHandler<,>), typeof(ValidationDecorator.CommandHandler<,>));
        TryDecorate(builder.Services, typeof(ICommandHandler<>), typeof(ValidationDecorator.CommandBaseHandler<>));

        TryDecorate(builder.Services, typeof(IQueryHandler<,>), typeof(LoggingDecorator.QueryHandler<,>));
        TryDecorate(builder.Services, typeof(ICommandHandler<,>), typeof(LoggingDecorator.CommandHandler<,>));
        TryDecorate(builder.Services, typeof(ICommandHandler<>), typeof(LoggingDecorator.CommandBaseHandler<>));

        // Domain event handlers
        builder.Services.Scan(scan => scan.FromAssemblies(assemblies)
            .AddClasses(classes => classes.AssignableTo(typeof(IDomainEventHandler<>)), publicOnly: false)
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        // Validators
        builder.Services.AddValidatorsFromAssemblies(assemblies, includeInternalTypes: true);

        return builder;
    }

    public static WebApplicationBuilder AddSharedServices(this WebApplicationBuilder builder) 
    {
        return builder;
    }

    private static void TryDecorate(IServiceCollection services, Type serviceType, Type decoratorType)
    {
        // Only decorate if any registration exists
        if (services.Any(s => s.ServiceType.IsGenericType && s.ServiceType.GetGenericTypeDefinition() == serviceType))
        {
            services.Decorate(serviceType, decoratorType);
        }
    }

    private static Assembly[] SafeDomainAssemblies()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .Where(a =>
                !a.IsDynamic &&
                a.FullName is string n &&
                !n.StartsWith("System.", StringComparison.Ordinal) &&
                !n.StartsWith("Microsoft.", StringComparison.Ordinal) &&
                !n.StartsWith("netstandard", StringComparison.Ordinal) &&
                !n.Contains("TestHost", StringComparison.OrdinalIgnoreCase) &&
                !n.Contains("xunit", StringComparison.OrdinalIgnoreCase))
            .Distinct()
            .ToArray();
    }
}