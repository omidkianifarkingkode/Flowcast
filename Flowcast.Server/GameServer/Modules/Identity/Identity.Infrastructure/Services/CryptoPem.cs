using System.Security.Cryptography;

namespace Identity.Infrastructure.Services;

public static class CryptoPem
{
    public static (string publicPem, string privatePem) Generate(string algorithm)
    {
        if (algorithm.StartsWith("RS", StringComparison.OrdinalIgnoreCase))
        {
            // default 2048
            using var rsa = RSA.Create(2048);
            var publicPem = rsa.ExportSubjectPublicKeyInfoPem();
            var privatePem = rsa.ExportRSAPrivateKeyPem();
            return (publicPem, privatePem);
        }
        if (algorithm.Equals("ES256", StringComparison.OrdinalIgnoreCase))
            return GenerateEcdsa(ECCurve.NamedCurves.nistP256);
        if (algorithm.Equals("ES384", StringComparison.OrdinalIgnoreCase))
            return GenerateEcdsa(ECCurve.NamedCurves.nistP384);
        if (algorithm.Equals("ES512", StringComparison.OrdinalIgnoreCase))
            return GenerateEcdsa(ECCurve.NamedCurves.nistP521);

        throw new NotSupportedException($"Algorithm '{algorithm}' not supported.");
    }

    private static (string publicPem, string privatePem) GenerateEcdsa(ECCurve curve)
    {
        using var ecdsa = ECDsa.Create(curve);
        var publicPem = ecdsa.ExportSubjectPublicKeyInfoPem();
        var privatePem = ecdsa.ExportECPrivateKeyPem();
        return (publicPem, privatePem);
    }

    // Simple kid: base64url(SHA-256(SPKI DER)) (shortened)
    public static string ComputeKidFromPublicPem(string publicPem)
    {
        var der = PemToDer(publicPem);
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(der);
        return Base64Url(hash[..16]); // 16 bytes is plenty for uniqueness
    }

    private static byte[] PemToDer(string pem)
    {
        // take content between header/footer
        var lines = pem.Split('\n').Select(l => l.Trim()).Where(l => !l.StartsWith("-----")).ToArray();
        return Convert.FromBase64String(string.Concat(lines));
    }

    public static string Base64Url(byte[] bytes)
        => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
