using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Shared.Infrastructure.Extensions;

public static class WebHostEnvironmentExtensions 
{
    public static bool IsLocal(this IWebHostEnvironment environment)
    {
        return environment.IsEnvironment("Local");
    }

    public static bool IsLocalOrDevelopement(this IWebHostEnvironment environment)
    {
        return environment.IsEnvironment("Local") || environment.IsDevelopment();
    }
}
