using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Shared.Presentation.Endpoints;
using System.Reflection;
using ZP.Core;

namespace ZP.Apphost;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddZarinpal(this WebApplicationBuilder builder)
    {
        builder.AddCore();
        builder.Services.AddEndpoints(typeof(DependencyInjection).Assembly);
        return builder;
    }
}
