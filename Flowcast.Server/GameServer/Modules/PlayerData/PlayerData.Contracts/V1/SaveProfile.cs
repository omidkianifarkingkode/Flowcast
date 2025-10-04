using System;
using System.Collections.Generic;
using PlayerData.Contracts.V1.Shared;

namespace PlayerData.Contracts.V1;

public static class SaveProfile
{
    public const string Method = "POST";
    public const string Route = "player-data/profile";

    public const string Summary = "Save player profile data";
    public const string Description = "Creates or replaces namespaces for a player profile.";

    public record Request(
        Guid PlayerId,
        IReadOnlyCollection<NamespaceWrite> Namespaces
    );

    public record Response(
        Guid PlayerId,
        NamespaceDocument[] Namespaces
    );
}
