using Microsoft.AspNetCore.Builder;

namespace Shared.Presentation.ApiGuard;

public static class ApiGuardEndpointExtensions
{
    public static TBuilder AllowOnlyLaunchModes<TBuilder>(this TBuilder builder, params string[] modes)
        where TBuilder : IEndpointConventionBuilder
    {
        builder.Add(e => e.Metadata.Add(new AllowOnlyLaunchModes(modes)));
        return builder;
    }

    public static TBuilder BlockLaunchModes<TBuilder>(this TBuilder builder, params string[] modes)
        where TBuilder : IEndpointConventionBuilder
    {
        builder.Add(e => e.Metadata.Add(new BlockLaunchModes(modes)));
        return builder;
    }

    // Convenience helpers (ASP.NET Core default env names)
    public static TBuilder AllowOnlyProduction<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
        => builder.AllowOnlyLaunchModes("Production");

    public static TBuilder AllowOnlyDevelopment<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
        => builder.AllowOnlyLaunchModes("Development");

    public static TBuilder AllowOnlyStaging<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
        => builder.AllowOnlyLaunchModes("Staging");

    public static TBuilder BlockInDevelopment<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
        => builder.BlockLaunchModes("Development");

    public static TBuilder BlockInLocal<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
        => builder.BlockLaunchModes("Local");

    public static TBuilder BlockInLocalAndDev<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
        => builder.BlockLaunchModes("Local", "Development");
}
