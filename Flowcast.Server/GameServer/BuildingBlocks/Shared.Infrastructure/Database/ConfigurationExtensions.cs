using Microsoft.Extensions.Configuration;

namespace Shared.Infrastructure.Database;

public static class ConfigurationExtensions
{
    public static string GetModuleConnectionString(this IConfiguration config, string moduleSection)
    {
        var moduleConnectionString = config[$"{moduleSection}:ConnectionString"];
        if (!string.IsNullOrWhiteSpace(moduleConnectionString))
            return moduleConnectionString;

        return config.GetConnectionString("DefaultConnection")
               ?? throw new InvalidOperationException("DefaultConnection not configured.");
    }
}
