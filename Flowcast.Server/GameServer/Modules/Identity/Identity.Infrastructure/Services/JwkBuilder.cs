using Identity.Application.Services;
using System.Security.Cryptography;

namespace Identity.Infrastructure.Services;

public static class JwkBuilder
{
    public sealed record Jwk(string kty, string kid, string alg, string use, string? n = null, string? e = null, string? crv = null, string? x = null, string? y = null);

    public static Jwk ToJwk(KeyMaterial km)
    {
        if (km.Algorithm.StartsWith("RS", StringComparison.OrdinalIgnoreCase))
        {
            using var rsa = RSA.Create();
            rsa.ImportFromPem(km.PublicKeyPem);
            var p = rsa.ExportParameters(false);

            return new Jwk(
                kty: "RSA",
                kid: km.Kid,
                alg: km.Algorithm.ToUpperInvariant(),
                use: "sig",
                n: CryptoPem.Base64Url(p.Modulus!),
                e: CryptoPem.Base64Url(p.Exponent!)
            );
        }

        if (km.Algorithm.StartsWith("ES", StringComparison.OrdinalIgnoreCase))
        {
            using var ecdsa = ECDsa.Create();
            ecdsa.ImportFromPem(km.PublicKeyPem);
            var p = ecdsa.ExportParameters(false);

            var crv = km.Algorithm.ToUpperInvariant() switch
            {
                "ES256" => "P-256",
                "ES384" => "P-384",
                "ES512" => "P-521",
                _ => "P-256"
            };

            return new Jwk(
                kty: "EC",
                kid: km.Kid,
                alg: km.Algorithm.ToUpperInvariant(),
                use: "sig",
                crv: crv,
                x: CryptoPem.Base64Url(p.Q.X!),
                y: CryptoPem.Base64Url(p.Q.Y!)
            );
        }

        throw new NotSupportedException($"Algorithm '{km.Algorithm}' not supported for JWKS.");
    }
}
