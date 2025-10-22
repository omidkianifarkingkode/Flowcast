namespace AppHost.Extensions
{
    public static class ProgramExtensions
    {
        public static WebApplicationBuilder ConfigureAppSettings(this WebApplicationBuilder builder)
        {
            static string configPath(string file) => Path.Combine("Configs", file);

            builder.Configuration.Sources.Clear();

            builder.Configuration
                .AddJsonFile(configPath("appsettings.json"), optional: false, reloadOnChange: true)
                .AddJsonFile(configPath($"appsettings.{builder.Environment.EnvironmentName}.json"), optional: true, reloadOnChange: true)
                .AddJsonFile(configPath("appsettings.Secrets.json"), optional: true, reloadOnChange: true)
                .AddJsonFile(configPath($"appsettings.{builder.Environment.EnvironmentName}.Secrets.json"), optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddCommandLine(builder.Configuration.GetCommandLineArgs() ?? []);

            if (builder.Environment.IsEnvironment("Local") || builder.Environment.IsDevelopment())
            {
                builder.Configuration.AddUserSecrets<Program>(optional: true);
            }

            return builder;
        }

        public static WebApplication LogEnvironmentStartup(this WebApplication app)
        {
            if (app.Environment.IsEnvironment("Local"))
            {
                app.Logger.LogInformation("Local environment detected - run local seeding and patch jobs here.");
            }
            else if (app.Environment.IsDevelopment())
            {
                app.Logger.LogInformation("Development environment detected - run development-specific patch routines here.");
            }
            else if (app.Environment.IsProduction())
            {
                app.Logger.LogInformation("Production environment detected - run production-specific patch routines here.");
            }

            return app;
        }

        private static string[]? GetCommandLineArgs(this IConfiguration configuration)
        {
            try
            {
                return Environment.GetCommandLineArgs();
            }
            catch
            {
                return null;
            }
        }
    }
}
