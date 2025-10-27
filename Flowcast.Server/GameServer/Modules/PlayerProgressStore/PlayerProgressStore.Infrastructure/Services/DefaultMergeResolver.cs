using PlayerProgressStore.Application.Services;
using SharedKernel;

namespace PlayerProgressStore.Infrastructure.Services;

public class DefaultMergeResolver : IMergeResolver
{
    public string Namespace => "default";

    public Result<byte[]> Merge(byte[] serverDocument, byte[] clientDocument)
    {
        // Default: prefer the client's version entirely
        // (This is a "client wins" merge for equal progress)
        return Result.Success(clientDocument);
    }
}
