using SharedKernel;

namespace PlayerProgressStore.Application.Services;

/// <summary>
/// Central place to validate namespace naming/length/character rules at the app boundary.
/// </summary>
public interface INamespaceValidationPolicy
{
    Result Validate(string @namespace);
}