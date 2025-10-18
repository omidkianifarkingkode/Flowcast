using Newtonsoft.Json;
using PlayerProgressStore.Application.Services;
using SharedKernel;

namespace PlayerProgressStore.Infrastructure.Services;

public class CanonicalJsonService : ICanonicalJsonService
{
    public Result<string> Canonicalize(string json)
    {
        try
        {
            var parsed = JsonConvert.DeserializeObject<object>(json);
            var canonicalJson = JsonConvert.SerializeObject(parsed, Formatting.None);
            return Result.Success(canonicalJson);
        }
        catch (Exception ex)
        {
            return Result.Failure<string>(Error.Failure("json.invalid", ex.Message));
        }
    }
}
