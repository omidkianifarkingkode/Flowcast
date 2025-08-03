using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Presentation.SwaggerUtilities
{
    public class ConfigureSwaggerGenOptions : IConfigureNamedOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider;

        public ConfigureSwaggerGenOptions(IApiVersionDescriptionProvider provider)
        {
            _provider = provider;
        }

        public void Configure(string? name, SwaggerGenOptions options)
        {
            foreach (var description in _provider.ApiVersionDescriptions)
            {
                var openApiInfo = new OpenApiInfo 
                {
                    Title = $"GameServer.Api v{description.ApiVersion}",
                    Version = description.ApiVersion.ToString()
                };

                if (description.IsDeprecated)
                {
                    openApiInfo.Description += " deprecated API.";
                }


                options.SwaggerDoc(description.GroupName, openApiInfo);
            }
        }

        public void Configure(SwaggerGenOptions options)
        {
            Configure(options);
        }
    }
}
