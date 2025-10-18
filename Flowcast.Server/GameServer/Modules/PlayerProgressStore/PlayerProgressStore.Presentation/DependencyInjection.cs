using Microsoft.AspNetCore.Builder;
using Shared.API.Endpoints;
using PlayerProgressStore.Application;
using PlayerProgressStore.Infrastructure;

namespace PlayerProgressStore.Presentation
{
    public static class DependencyInjection
    {
        public static WebApplicationBuilder AddPlayerProgress(this WebApplicationBuilder builder)
        {
            builder.Services.AddEndpoints(typeof(DependencyInjection).Assembly);

            builder.AddApplication();
            builder.AddInfrastructure();

            return builder;
        }
    }
}
