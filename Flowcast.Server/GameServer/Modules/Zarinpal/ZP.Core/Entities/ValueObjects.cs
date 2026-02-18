namespace ZP.Core.Entities;

public readonly record struct PaymentRequestId
{
    public const string Prefix = "ZPR_";
    public string Value { get; }

    private PaymentRequestId(string value)
    {
        Value = value;
    }

    public static PaymentRequestId New()
    {
        var guid = Guid.CreateVersion7();
        return new PaymentRequestId($"{Prefix}{guid:N}");
    }

    public static PaymentRequestId FromString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("PaymentRequestId cannot be empty.");
        if (!value.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
            throw new FormatException($"PaymentRequestId must start with '{Prefix}'.");
        var guidPart = value[Prefix.Length..];
        if (!Guid.TryParseExact(guidPart, "N", out _))
            throw new FormatException("Invalid GUID format.");
        return new PaymentRequestId(value);
    }

    public override string ToString() => Value;
    public static implicit operator string(PaymentRequestId id) => id.Value;
}

public readonly record struct OrderId
{
    public string Value { get; }
    private OrderId(string value) => Value = value;

    public static OrderId New() => new OrderId($"ORD_{Guid.CreateVersion7():N}");

    public static OrderId Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("OrderId cannot be empty.");
        if (value.Length > 64)
            throw new ArgumentException("OrderId length must be at most 64.");
        return new OrderId(value.Trim());
    }

    public override string ToString() => Value;
}

public readonly record struct Authority
{
    public string Value { get; }
    private Authority(string value) => Value = value;

    public static Authority Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Authority cannot be empty.");
        if (value.Length > 64)
            throw new ArgumentException("Authority length must be at most 64.");
        return new Authority(value.Trim());
    }

    public override string ToString() => Value;
}

public readonly record struct RefId
{
    public long Value { get; }
    private RefId(long value) => Value = value;

    public static RefId Create(long value)
    {
        if (value <= 0)
            throw new ArgumentException("RefId must be positive.");
        return new RefId(value);
    }

    public override string ToString() => Value.ToString();
}
