using Microsoft.AspNetCore.Builder;
using Shared.Application;

namespace Identity.Application;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddApplication(this WebApplicationBuilder builder)
    {
        builder.AddCQRS(typeof(DependencyInjection).Assembly);

        return builder;
    }
}
