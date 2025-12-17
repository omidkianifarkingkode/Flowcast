using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Presentation.ApiGuard;

public static class ApiGuardExtensions
{
    public static IServiceCollection AddApiGuard(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<ApiGuardOptions>(config.GetSection("ApiGuard"));
        services.AddTransient<ApiGuardMiddleware>();
        return services;
    }

    public static IApplicationBuilder UseApiGuard(this IApplicationBuilder app)
        => app.UseMiddleware<ApiGuardMiddleware>();
}
