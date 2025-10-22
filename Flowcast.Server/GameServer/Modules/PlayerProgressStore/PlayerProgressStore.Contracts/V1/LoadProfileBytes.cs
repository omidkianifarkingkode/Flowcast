using Microsoft.AspNetCore.Mvc;
using PlayerProgressStore.Contracts.V1.Shared;

namespace PlayerProgressStore.Contracts.V1;

/// <summary>
/// Load player progress profile (selected namespaces or all) with payloads encoded as byte arrays.
/// </summary>
public static class LoadProfileBytes
{
    public const string Method = "GET";
    public const string Route = "player-progress/profile/bytes";
    public const string Summary = "Load player progress data as byte arrays";
    public const string Description =
        "Returns the namespaces stored for a player profile encoded as byte arrays. Optionally filter by namespace name.";

    /// <summary>
    /// If <see cref="Namespaces"/> is null or empty, the server may return all existing namespaces for the player.
    /// </summary>
    /// <param name="Namespaces">Subset of namespaces to load (e.g., [\"playerStats\",\"inventory\"]).</param>
    public sealed record Request
    {
        [FromQuery(Name = "namespaces")]
        public string[]? Namespaces { get; init; }
    }

    /// <summary>
    /// Authoritative response with one or more namespaces represented as byte arrays.
    /// </summary>
    /// <param name="Namespaces">Returned namespace documents.</param>
    public sealed record Response(
        NamespaceBinaryDocument[] Namespaces
    );
}
