using Microsoft.AspNetCore.Builder;
using Shared.Presentation.Endpoints;
using Shop.Application;
using Shop.Infrastructure;

namespace Shop.Presentation
{
    public static class DependencyInjection
    {
        public static WebApplicationBuilder AddShop(this WebApplicationBuilder builder)
        {
            builder.Services.AddEndpoints(typeof(DependencyInjection).Assembly);
            builder.AddApplication();
            builder.AddInfrastructure();

            return builder;
        }
        

    }
}
