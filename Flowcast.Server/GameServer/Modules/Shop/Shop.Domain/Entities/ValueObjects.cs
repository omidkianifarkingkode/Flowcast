namespace Shop.Domain.Entities;

public readonly record struct PurchaseId
{
    public const string Prefix = "PUR_";
    public string Value { get; }

    private PurchaseId(string value)
    {
        Value = value;
    }

    public static PurchaseId New()
    {
        var guid = Guid.CreateVersion7();
        return new PurchaseId($"{Prefix}{guid:N}");
    }

    public static PurchaseId FromString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("PurchaseId cannot be empty.");

        if (!value.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
            throw new FormatException($"PurchaseId must start with '{Prefix}'.");

        var guidPart = value[Prefix.Length..];
        if (!Guid.TryParseExact(guidPart, "N", out _))
            throw new FormatException("Invalid GUID v7 format.");

        return new PurchaseId(value);
    }

    public override string ToString() => Value;
    public static implicit operator string(PurchaseId id) => id.Value;
}



public readonly record struct OrderId
{
    public string Value { get; }
    private OrderId(string value) => Value = value;

    public static OrderId Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("OrderId cannot be empty");
        if (value.Length is > 2024)
            throw new ArgumentException("OrderId length must less than 2025.");
        return new OrderId(value.Trim());
    }
    public override string ToString() => Value;

}

public readonly record struct PurchaseToken
{
    public string Value { get; }
    private PurchaseToken(string value)
    {
        Value = value;
    }
    public static PurchaseToken Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Purchase Token cannot be emtpy");
        if (value.Length is > 4000)
            throw new ArgumentException("Purchase Token must be between 3 to 100.");
        return new PurchaseToken(value.Trim());
    }
    public override string ToString() => Value;
}

public readonly record struct PurchaseSignature
{
    public string Value { get; }
    private PurchaseSignature(string value)
    {
        Value = value;
    }
    public static PurchaseSignature Create(string value)
    {
        return new PurchaseSignature(value ?? "");
    }
    public override string ToString() => Value;
}
