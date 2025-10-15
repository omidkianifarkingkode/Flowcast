using PlayerProgressStore.Application;
using SharedKernel;

namespace PlayerProgressStore.Infrastructure.Services;

public class NamespaceValidationPolicy : INamespaceValidationPolicy
{
    public Result Validate(string @namespace)
    {
        if (string.IsNullOrWhiteSpace(@namespace))
            return Result.Failure(Error.Validation("namespace.required", "Namespace is required."));

        if (@namespace.Length > 128)
            return Result.Failure(Error.Validation("namespace.too_long", "Namespace is too long."));

        // add more rules if needed (allowed chars, etc.)
        return Result.Success();
    }
}
