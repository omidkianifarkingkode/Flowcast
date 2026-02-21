using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Identity.Infrastructure.Persistences;

public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // find content root so it works from CLI
        var basePath = Directory.GetCurrentDirectory();

        var builder = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("Configs/appsettings.json", optional: true)
            .AddJsonFile("Configs/appsettings.Development.json", optional: true)
            .AddJsonFile("Configs/appsettings.Local.json", optional: true)
            .AddEnvironmentVariables();

        var config = builder.Build();

        var connStr = config.GetSection("Identity")["ConnectionString"];
        if (string.IsNullOrWhiteSpace(connStr))
            throw new InvalidOperationException("Missing Identity:ConnectionString in configuration.");

        var provider = config["Database:Provider"] ?? "SqlServer";
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        var migrationsAssembly = typeof(ApplicationDbContext).Assembly.FullName;

        if (string.Equals(provider, "Postgres", StringComparison.OrdinalIgnoreCase))
            optionsBuilder.UseNpgsql(connStr, npgsql => npgsql.MigrationsAssembly(migrationsAssembly));
        else
            optionsBuilder.UseSqlServer(connStr, sql => sql.MigrationsAssembly(migrationsAssembly));

        var options = optionsBuilder.Options;

        return new ApplicationDbContext(options);
    }
}
