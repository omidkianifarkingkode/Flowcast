using System.ComponentModel.DataAnnotations;


namespace PlayerProgressStore.Infrastructure.Options;

public sealed class PlayerProgressOptions
{
    public const string SectionName = "PlayerProgress";

    /// <summary>If true, uses InMemory provider for this module.</summary>
    public bool UseInMemoryDatabase { get; init; } = false;

    /// <summary>EF Core SQL Server connection string (required when not using InMemory).</summary>
    public string? ConnectionString { get; init; }
}
