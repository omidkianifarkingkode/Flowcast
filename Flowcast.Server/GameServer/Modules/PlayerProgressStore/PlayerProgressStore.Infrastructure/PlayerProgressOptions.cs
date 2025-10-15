using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;


namespace PlayerProgressStore.Infrastructure;

public sealed class PlayerProgressOptions
{
    public const string SectionName = "PlayerProgress";

    /// <summary>If true, uses InMemory provider for this module.</summary>
    public bool UseInMemoryDatabase { get; init; } = false;

    /// <summary>EF Core SQL Server connection string (required when not using InMemory).</summary>
    [MaxLength(4096)]
    public string? ConnectionString { get; init; }

    /// <summary>Optional database schema for tables (default 'dbo').</summary>
    [Required, MaxLength(128)]
    public string Schema { get; init; } = "dbo";

    /// <summary>Table name for PlayerNamespaces (default 'PlayerNamespaces').</summary>
    [Required, MaxLength(128)]
    public string PlayerNamespacesTable { get; init; } = "PlayerNamespaces";
}

public sealed class PlayerProgressOptionsValidator : IValidateOptions<PlayerProgressOptions>
{
    public ValidateOptionsResult Validate(string? name, PlayerProgressOptions options)
    {
        if (options is null)
            return ValidateOptionsResult.Fail("PlayerProgress options missing.");

        if (!options.UseInMemoryDatabase && string.IsNullOrWhiteSpace(options.ConnectionString))
            return ValidateOptionsResult.Fail("PlayerProgress: ConnectionString is required when UseInMemoryDatabase is false.");

        if (string.IsNullOrWhiteSpace(options.Schema))
            return ValidateOptionsResult.Fail("PlayerProgress: Schema is required.");

        if (string.IsNullOrWhiteSpace(options.PlayerNamespacesTable))
            return ValidateOptionsResult.Fail("PlayerProgress: PlayerNamespacesTable is required.");

        return ValidateOptionsResult.Success;
    }
}
