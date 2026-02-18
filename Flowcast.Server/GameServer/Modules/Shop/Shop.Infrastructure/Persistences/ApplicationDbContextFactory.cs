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

        if(string.IsNullOrWhiteSpace(shopOptions?.ConnectionStrings))
            throw new InvalidOperationException("Shop Connection string not found");

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(shopOptions.ConnectionStrings, sql =>
            {
                sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
            })
            .Options;

        return new ApplicationDbContext(options);
    }

}
