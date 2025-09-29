using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Authentication;
using Shared.Application.Realtime;
using Shared.Application.Services;
using Shared.Infrastructure;
using Shared.Infrastructure.Authentication;
using Shared.Infrastructure.Authorization;
using Shared.Infrastructure.DomainEvents;
using Shared.Infrastructure.Realtime.Services;

namespace Shared.Infrastructure;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddInfrastructure(this WebApplicationBuilder builder)
    {
        return builder
            .AddServices()
            .AddHealthChecks()
            .AddAuthorizationInternal();
    }

    private static WebApplicationBuilder AddServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        builder.Services.AddTransient<IDomainEventsDispatcher, DomainEventsDispatcher>();

        builder.Services.AddSingleton<ILivenessProbe, RegistryLivenessProbe>();

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<IUserContext, UserContext>();

        return builder;
    }

    private static WebApplicationBuilder AddHealthChecks(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddHealthChecks();
        //.AddNpgSql(builder.Configuration.GetConnectionString("Database")!);

        return builder;
    }

    private static WebApplicationBuilder AddAuthorizationInternal(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthorization();

        builder.Services.AddScoped<PermissionProvider>();

        builder.Services.AddTransient<IAuthorizationHandler, PermissionAuthorizationHandler>();

        builder.Services.AddTransient<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

        return builder;
    }
}
