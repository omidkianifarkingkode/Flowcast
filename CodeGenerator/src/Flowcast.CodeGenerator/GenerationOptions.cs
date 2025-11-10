using System;

namespace Flowcast.CodeGenerator;

/// <summary>
/// Defines options that influence how code is generated.
/// </summary>
public sealed class GenerationOptions
{
    /// <summary>
    /// Gets or sets the namespace for generated REST clients.
    /// </summary>
    public string Namespace { get; init; } = "Flowcast.Generated";

    /// <summary>
    /// Gets or sets the output directory where generated files will be written.
    /// </summary>
    public string OutputDirectory { get; init; } = string.Empty;

    /// <summary>
    /// Gets the namespace used for generated models.
    /// </summary>
    public string ModelNamespace => string.IsNullOrWhiteSpace(Namespace) ? "Flowcast.Generated.Models" : Namespace + ".Models";

    /// <summary>
    /// Ensures the options contain the required data.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(OutputDirectory))
        {
            throw new InvalidOperationException("An output directory must be supplied before running code generation.");
        }
    }
}
