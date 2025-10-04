using System.Text.Json;

namespace PlayerData.Contracts.V1.Shared;

public record NamespaceWrite(
    string Namespace,
    JsonElement Document,
    string? ClientVersion = null
);
