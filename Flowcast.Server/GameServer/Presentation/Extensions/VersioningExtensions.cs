using Asp.Versioning;

namespace Presentation.Extensions;

public static class VersioningExtensions 
{
    public static void InstallVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1.0);
            options.ReportApiVersions = true;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new QueryStringApiVersionReader("api-version"),
                new HeaderApiVersionReader("X-Api-Version"));
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'V";
            options.SubstituteApiVersionInUrl = true;
        });
    }

    public static RouteGroupBuilder GetVersionedGroupBuilder(this WebApplication app)
    {
        var versionSet = app.NewApiVersionSet()
                   .HasApiVersion(new ApiVersion(1.0))
                   .ReportApiVersions()
                   .Build();

        var versionedGroup = app.MapGroup("api/v{apiVersion:apiVersion}")
            .WithApiVersionSet(versionSet);

        return versionedGroup;
    }
}
