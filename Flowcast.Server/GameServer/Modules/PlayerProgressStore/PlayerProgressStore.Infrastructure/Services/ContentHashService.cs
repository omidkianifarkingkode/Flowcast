using PlayerProgressStore.Application;
using PlayerProgressStore.Domain;
using System.Security.Cryptography;
using System.Text;

namespace PlayerProgressStore.Infrastructure.Services;

public class ContentHashService : IContentHashService
{
    public DocHash Compute(string canonicalJson)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(canonicalJson));
        var hashString = Convert.ToBase64String(hashBytes);
        return new DocHash(hashString);
    }
}
