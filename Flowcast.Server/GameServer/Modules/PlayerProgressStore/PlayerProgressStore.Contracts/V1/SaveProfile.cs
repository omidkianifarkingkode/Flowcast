using PlayerProgressStore.Contracts.V1.Shared;

namespace PlayerProgressStore.Contracts.V1;

/// <summary>
/// Atomic batch save for one or more namespaces.
/// Progress-first policy:
/// - If client.progress > server.progress => overwrite entire doc.
/// - If client.progress < server.progress => reject (conflict).
/// - If equal => merge via server resolver rules.
/// </summary>
public static class SaveProfile
{
    public const string Method = "POST";
    public const string Route = "player-progress/profile";
    public const string Summary = "Save player progress data (atomic)";
    public const string Description =
        "Creates or replaces namespaces for a player profile atomically. " +
        "Server enforces progress-first conflict handling and returns authoritative copies.";

    /// <summary>
    /// Batch of namespace writes to apply atomically.
    /// </summary>
    /// <param name="Namespaces">One or more namespace writes.</param>
    public readonly record struct Request(
        IReadOnlyCollection<NamespaceWrite> Namespaces
    );

    /// <summary>
    /// Authoritative result after the atomic save is committed.
    /// </summary>
    /// <param name="PlayerId">The player identifier.</param>
    /// <param name="Namespaces">Committed namespace documents (server truth).</param>
    public readonly record struct Response(
        Guid PlayerId,
        NamespaceDocument[] Namespaces
    );
}