using System;
using System.Text.Json;

namespace PlayerData.Contracts.V1.Shared;

public record NamespaceDocument(
    string Namespace,
    JsonElement Document,
    string? Version = null,
    DateTimeOffset? UpdatedAtUtc = null
);
