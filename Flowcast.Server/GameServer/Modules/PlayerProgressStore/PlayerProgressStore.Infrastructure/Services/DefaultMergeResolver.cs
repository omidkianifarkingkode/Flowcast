using PlayerProgressStore.Application;
using SharedKernel;

namespace PlayerProgressStore.Infrastructure.Services;

public class DefaultMergeResolver : IMergeResolver
{
    public string Namespace => "default";

    public Result<string> Merge(string serverJson, string clientJson)
    {
        // Default: prefer the client's version entirely
        // (This is a "client wins" merge for equal progress)
        return Result.Success(clientJson);
    }
}
