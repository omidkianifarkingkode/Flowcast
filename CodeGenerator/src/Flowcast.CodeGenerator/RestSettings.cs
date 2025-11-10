using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Flowcast.CodeGenerator;

/// <summary>
/// Represents the Flowcast REST generator configuration.
/// </summary>
public sealed class RestSettings
{
    /// <summary>
    /// Gets the collection of OpenAPI document paths that should be used for generation.
    /// Paths are resolved relative to the settings file itself.
    /// </summary>
    [JsonPropertyName("documents")]
    public IList<string> Documents { get; } = new List<string>();

    /// <summary>
    /// Gets or sets the optional namespace that should be used for generated types.
    /// </summary>
    [JsonPropertyName("namespace")]
    public string? Namespace { get; set; }

    /// <summary>
    /// Gets or sets the optional output directory relative to the settings file.
    /// </summary>
    [JsonPropertyName("outputDirectory")]
    public string? OutputDirectory { get; set; }
}
