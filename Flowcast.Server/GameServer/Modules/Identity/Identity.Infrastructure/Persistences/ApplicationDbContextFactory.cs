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
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables();

        var config = builder.Build();

        // Expect "Identity:ConnectionString" to exist in config
        var connStr = config.GetSection("Identity")["ConnectionString"];
        if (string.IsNullOrWhiteSpace(connStr))
            throw new InvalidOperationException("Missing Identity:ConnectionString in configuration.");

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connStr, sql =>
            {
                // keep migrations in the same assembly as the DbContext
                sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
            })
            .Options;

        return new ApplicationDbContext(options);
    }
}
