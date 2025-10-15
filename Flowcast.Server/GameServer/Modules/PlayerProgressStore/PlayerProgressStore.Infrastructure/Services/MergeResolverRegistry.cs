using PlayerProgressStore.Application;

namespace PlayerProgressStore.Infrastructure.Services;

public class MergeResolverRegistry : IMergeResolverRegistry
{
    private readonly Dictionary<string, IMergeResolver> _resolvers = [];

    public IMergeResolver Get(string @namespace)
    {
        if (_resolvers.TryGetValue(@namespace, out var resolver))
        {
            return resolver;
        }

        return new DefaultMergeResolver(); // Default merge behavior
    }

    public void Register(string @namespace, IMergeResolver resolver)
    {
        _resolvers[@namespace] = resolver;
    }
}
