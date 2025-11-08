namespace PlayerProgressStore.Application.Services;

/// <summary>
/// Looks up a resolver for a given namespace; returns a default pass-through if none registered.
/// </summary>
public interface IMergeResolverRegistry
{
    IMergeResolver Get(string @namespace);
}
