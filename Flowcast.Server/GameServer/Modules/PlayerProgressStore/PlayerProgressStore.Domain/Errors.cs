using SharedKernel;

namespace PlayerProgressStore.Domain;

/// <summary>
/// Centralized domain errors for PlayerNamespace aggregate.
/// </summary>
public static class PlayerNamespaceErrors
{
    public static readonly Error NamespaceRequired =
        Error.Validation("namespace.required", "Namespace cannot be null or whitespace.");

    public static readonly Error ClientBehind =
        Error.Conflict("progress.client_behind", "Client progress is behind the server progress.");

    public static readonly Error InvalidProgress =
        Error.Validation("progress.invalid", "Progress value must be non-negative.");

    public static readonly Error UnknownDecision =
        Error.Failure("merge.unknown_decision", "Unknown merge decision type.");

    public static readonly Error EmptyPlayerId =
        Error.Validation("player_id.empty", "PlayerId cannot be empty.");
}