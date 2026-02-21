using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace PlayerProgressStore.Infrastructure.Persistences;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Locate your solution/appsettings (adjust path if needed)
        var basePath = Directory.GetCurrentDirectory();
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("Configs/appsettings.json", optional: true)
            .AddJsonFile("Configs/appsettings.Development.json", optional: true)
            .AddJsonFile("Configs/appsettings.Local.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString =
            configuration.GetSection("PlayerProgress")["ConnectionString"]
            ?? "Server=.;Database=PlayerProgressDb;Trusted_Connection=True;TrustServerCertificate=True;";

        var provider = configuration["Database:Provider"] ?? "SqlServer";
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        var migrationsAssembly = typeof(ApplicationDbContext).Assembly.FullName;

        if (string.Equals(provider, "Postgres", StringComparison.OrdinalIgnoreCase))
            optionsBuilder.UseNpgsql(connectionString, npgsql => npgsql.MigrationsAssembly(migrationsAssembly));
        else
            optionsBuilder.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly));

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
