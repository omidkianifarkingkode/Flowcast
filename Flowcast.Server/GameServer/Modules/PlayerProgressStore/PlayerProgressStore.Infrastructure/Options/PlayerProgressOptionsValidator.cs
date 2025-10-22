using Microsoft.Extensions.Options;


namespace PlayerProgressStore.Infrastructure.Options;

public sealed class PlayerProgressOptionsValidator : IValidateOptions<PlayerProgressOptions>
{
    public ValidateOptionsResult Validate(string? name, PlayerProgressOptions options)
    {
        if (options is null)
            return ValidateOptionsResult.Fail("PlayerProgress options missing.");

        return ValidateOptionsResult.Success;
    }
}
