using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using ZP.Core.Options;

namespace ZP.Core.Persisntence;

public sealed class ApplicationDbContextFactory
    : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();
        for (var i = 0; i < 4; i++)
        {
            if (File.Exists(Path.Combine(basePath, "appsettings.json"))) break;
            basePath = Path.Combine(basePath, "..");
        }

        var cfg = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
        var zarinpalOptions = cfg
            .GetSection(ZarinpalOptions.SectionName)
            .Get<ZarinpalOptions>();

        if(string.IsNullOrWhiteSpace(zarinpalOptions?.ConnectionStrings))
            throw new InvalidOperationException("Zarinpal Connection string not found");

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(zarinpalOptions.ConnectionStrings, sql =>
            {
                sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
            })
            .Options;
        return new ApplicationDbContext(options);
    }
}
