using SharedKernel;

namespace PlayerProgressStore.Domain;

public readonly record struct VersionToken(string Value)
{
    public static VersionToken None => new("v0");
    public override string ToString() => Value ?? "v0";
    public static Result<VersionToken> Create(string? value)
        => string.IsNullOrWhiteSpace(value)
           ? Result.Success(None) // treat null/empty as v0
           : Result.Success(new VersionToken(value.Trim()));
}

public readonly record struct ProgressScore(long Value) : IComparable<ProgressScore>
{
    public static ProgressScore Zero => new(0);

    public static Result<ProgressScore> From(long value)
        => value < 0
           ? Result.Failure<ProgressScore>(PlayerNamespaceErrors.InvalidProgress)
           : Result.Success(new ProgressScore(value));

    public int CompareTo(ProgressScore other) => Value.CompareTo(other.Value);

    public static implicit operator long(ProgressScore p) => p.Value;
}

public readonly record struct DocHash(string Value)
{
    public static DocHash Empty => new("");
    public override string ToString() => Value ?? "";
    public static Result<DocHash> Create(string? value)
        => Result.Success(new DocHash(value?.Trim() ?? ""));
}
