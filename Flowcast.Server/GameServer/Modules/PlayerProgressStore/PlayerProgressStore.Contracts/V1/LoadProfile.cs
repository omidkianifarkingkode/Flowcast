using PlayerProgressStore.Contracts.V1.Shared;

namespace PlayerProgressStore.Contracts.V1;

/// <summary>
/// Load player progress profile (selected namespaces or all).
/// </summary>
public static class LoadProfile
{
    public const string Method = "GET";
    public const string Route = "player-progress/profile";
    public const string Summary = "Load player progress data";
    public const string Description =
        "Returns the namespaces stored for a player profile. Optionally filter by namespace name.";

    /// <summary>
    /// If <see cref="Namespaces"/> is null or empty, the server may return all existing namespaces for the player.
    /// </summary>
    /// <param name="Namespaces">Subset of namespaces to load (e.g., [\"playerStats\",\"inventory\"]).</param>
    public readonly record struct Request(
        IReadOnlyCollection<string>? Namespaces = null
    );

    /// <summary>
    /// Authoritative response with one or more namespaces.
    /// </summary>
    /// <param name="PlayerId">The player identifier.</param>
    /// <param name="Namespaces">Returned namespace documents.</param>
    public readonly record struct Response(
        Guid PlayerId,
        NamespaceDocument[] Namespaces
    );
}