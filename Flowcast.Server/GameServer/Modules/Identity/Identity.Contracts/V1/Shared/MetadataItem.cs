namespace Identity.Contracts.V1.Shared;

public sealed record MetadataItem(string Key, string Value)
{
    public static Dictionary<string, string>? ToDictionary(IReadOnlyList<MetadataItem>? metadata)
    {
        if (metadata is null || metadata.Count == 0) return null;
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var m in metadata)
        {
            if (string.IsNullOrWhiteSpace(m.Key)) continue;
            dict[m.Key.Trim()] = m.Value ?? string.Empty;
        }
        return dict.Count > 0 ? dict : null;
    }
}
