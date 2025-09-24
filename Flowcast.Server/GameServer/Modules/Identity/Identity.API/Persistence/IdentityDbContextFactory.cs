using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Identity.API.Persistence;

public sealed class IdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
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

        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseSqlServer(connStr, sql =>
            {
                // keep migrations in the same assembly as the DbContext
                sql.MigrationsAssembly(typeof(IdentityDbContext).Assembly.FullName);
            })
            .Options;

        return new IdentityDbContext(options);
    }
}
