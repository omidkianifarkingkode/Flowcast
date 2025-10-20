using Microsoft.Extensions.Options;


namespace PlayerProgressStore.Infrastructure.Options;

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
