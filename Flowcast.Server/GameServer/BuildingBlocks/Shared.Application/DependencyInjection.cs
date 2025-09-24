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
        builder.Services.Decorate(typeof(ICommandHandler<,>), typeof(ValidationDecorator.CommandHandler<,>));
        builder.Services.Decorate(typeof(ICommandHandler<>), typeof(ValidationDecorator.CommandBaseHandler<>));

        builder.Services.Decorate(typeof(IQueryHandler<,>), typeof(LoggingDecorator.QueryHandler<,>));
        builder.Services.Decorate(typeof(ICommandHandler<,>), typeof(LoggingDecorator.CommandHandler<,>));
        builder.Services.Decorate(typeof(ICommandHandler<>), typeof(LoggingDecorator.CommandBaseHandler<>));

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
        builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        return builder;
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