using PlayerProgressStore.Application.Services;
using PlayerProgressStore.Domain;
using System;
using System.Security.Cryptography;

namespace PlayerProgressStore.Infrastructure.Services;

public class ContentHashService : IContentHashService
{
    public DocHash Compute(ReadOnlySpan<byte> document)
    {
        var hashBytes = SHA256.HashData(document);
        var hashString = Convert.ToBase64String(hashBytes);
        return new DocHash(hashString);
    }
}
