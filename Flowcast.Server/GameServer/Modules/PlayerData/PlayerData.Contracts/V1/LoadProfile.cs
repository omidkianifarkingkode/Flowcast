using System;
using System.Collections.Generic;
using PlayerData.Contracts.V1.Shared;

namespace PlayerData.Contracts.V1;

public static class LoadProfile
{
    public const string Method = "GET";
    public const string Route = "player-data/profile";

    public const string Summary = "Load player profile data";
    public const string Description = "Returns the namespaces stored for a player profile. Optionally filter by namespace name.";

    public record Request(
        Guid PlayerId,
        IReadOnlyCollection<string>? Namespaces = null
    );

    public record Response(
        Guid PlayerId,
        NamespaceDocument[] Namespaces
    );
}
