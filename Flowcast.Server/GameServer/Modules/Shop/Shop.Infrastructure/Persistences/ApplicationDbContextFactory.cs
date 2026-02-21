using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Shop.Infrastructure.Options;

namespace Shop.Infrastructure.Persistences;

public sealed class ApplicationDbContextFactory
    : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();

        var config = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("Configs/appsettings.json", optional: true)
            .AddJsonFile("Configs/appsettings.Development.json", optional: true)
            .AddJsonFile("Configs/appsettings.Local.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var shopOptions = config
            .GetSection(ShopOptions.SectionName)
            .Get<ShopOptions>();

        if(string.IsNullOrWhiteSpace(shopOptions?.ConnectionString))
            throw new InvalidOperationException("Shop Connection string not found");

        var provider = config["Database:Provider"] ?? "SqlServer";
        var migrationsAssembly = typeof(ApplicationDbContext).Assembly.FullName;
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        if (string.Equals(provider, "Postgres", StringComparison.OrdinalIgnoreCase))
            optionsBuilder.UseNpgsql(shopOptions.ConnectionString, npgsql => npgsql.MigrationsAssembly(migrationsAssembly));
        else
            optionsBuilder.UseSqlServer(shopOptions.ConnectionString, sql => sql.MigrationsAssembly(migrationsAssembly));

        var options = optionsBuilder.Options;

        return new ApplicationDbContext(options);
    }

}
