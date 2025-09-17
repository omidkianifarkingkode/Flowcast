// Project: Identity.Tests
// Framework: xUnit + FluentAssertions
// Install:
// dotnet add package xunit
// dotnet add package xunit.runner.visualstudio
// dotnet add package FluentAssertions
// dotnet add package Microsoft.Extensions.Options
// dotnet add package Microsoft.IdentityModel.Tokens

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using Identity.API.Infrastructures;
using Identity.API.Options;
using Microsoft.Extensions.Options;
using Xunit;

namespace Identity.Tests;

public class TokenServiceTests
{
    private static IdentityOptions BuildOptions(
        string alg = "RS256",
        int accessMins = 15,
        int refreshDays = 30,
        int skewSeconds = 60)
    {
        return new IdentityOptions
        {
            ConnectionString = "Server=(localdb)\\MSSQLLocalDB;Database=IdentityDb;Trusted_Connection=True;",
            TokenOptions = new TokenOptions
            {
                Issuer = "test-issuer",
                Audience = "test-audience",
                Algorithm = alg,
                KeyId = "test-key",
                PublicKeyPem = TestKeys.RsaPublicPem,
                PrivateKeyPem = TestKeys.RsaPrivatePem,
                AccessTokenExpiryMinutes = accessMins,
                RefreshTokenExpiryDays = refreshDays,
                ClockSkewSeconds = skewSeconds
            }
        };
    }

    private static TokenService CreateService(IdentityOptions opts)
        => new TokenService(Options.Create(opts));

    [Fact]
    public async Task IssueAsync_ReturnsTokens_WithExpectedJwtHeadersAndClaims()
    {
        var svc = CreateService(BuildOptions());
        var accountId = Guid.NewGuid();

        var (access, refresh, exp) = await svc.IssueAsync(accountId, default);

        access.Should().NotBeNullOrEmpty();
        refresh.Should().NotBeNullOrEmpty();

        var handler = new JwtSecurityTokenHandler();
        var accessJwt = handler.ReadJwtToken(access);
        var refreshJwt = handler.ReadJwtToken(refresh);

        accessJwt.Header.Alg.Should().Be("RS256");
        accessJwt.Header["kid"].Should().Be("test-key");

        refreshJwt.Header.Alg.Should().Be("RS256");
        refreshJwt.Header["kid"].Should().Be("test-key");

        accessJwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value.Should().Be(accountId.ToString());
        accessJwt.Claims.First(c => c.Type == "token_use").Value.Should().Be("access");

        refreshJwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value.Should().Be(accountId.ToString());
        refreshJwt.Claims.First(c => c.Type == "token_use").Value.Should().Be("refresh");

        exp.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(15), TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task RefreshAsync_WithValidRefreshToken_IssuesNewTokens_AndKeepsSubject()
    {
        var svc = CreateService(BuildOptions());
        var accountId = Guid.NewGuid();

        var (_, refresh, _) = await svc.IssueAsync(accountId, default);

        var (ok, sub, newAccess, newRefresh, newExp) = await svc.RefreshAsync(refresh, default);

        ok.Should().BeTrue();
        sub.Should().Be(accountId);
        newAccess.Should().NotBeNullOrEmpty();
        newRefresh.Should().NotBeNullOrEmpty();

        var handler = new JwtSecurityTokenHandler();
        var a = handler.ReadJwtToken(newAccess);
        var r = handler.ReadJwtToken(newRefresh);
        a.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value.Should().Be(accountId.ToString());
        a.Claims.First(c => c.Type == "token_use").Value.Should().Be("access");
        r.Claims.First(c => c.Type == "token_use").Value.Should().Be("refresh");
        newExp.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(15), TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task RefreshAsync_WithAccessToken_ShouldFail()
    {
        var svc = CreateService(BuildOptions());
        var accountId = Guid.NewGuid();
        var (access, _, _) = await svc.IssueAsync(accountId, default);

        var (ok, sub, _, _, _) = await svc.RefreshAsync(access, default);
        ok.Should().BeFalse();
        sub.Should().Be(Guid.Empty);
    }

    [Fact]
    public async Task RefreshAsync_WithTamperedToken_ShouldFailValidation()
    {
        var svc = CreateService(BuildOptions());
        var accountId = Guid.NewGuid();
        var (_, refresh, _) = await svc.IssueAsync(accountId, default);

        // Tamper one char (still Base64-like but signature breaks)
        var bad = refresh.TrimEnd('A', 'B', 'C') + "A";

        var (ok, sub, _, _, _) = await svc.RefreshAsync(bad, default);
        ok.Should().BeFalse();
        sub.Should().Be(Guid.Empty);
    }
}

internal static class TestKeys
{
    // *** Test-only RSA 2048 keys ***
    // Generate your own for real tests. These are placeholders.
    public const string RsaPrivatePem = @"-----BEGIN RSA PRIVATE KEY-----
MIIEowIBAAKCAQEAu3w2eA1E0r8kR8Kj7+oQ8l1o3s4vPMjT4i9GQqgT3o1Zf8M7
0v2yZk3Q0nqA0Gv+3e0bY1j3o0s5s8e5yV2NwJw3FfS7iCw2w4bqv6c1rGOkz0cS
2bS7lXy1j71yP5b6C1ZKQ2yS7gq1x3jQyq8g7wG0mTI6VtE6K+Hj1yY9mH5q5p5e
x5x3lC2n7S3v8zXo2wIDAQABAoIBABJ3bJxQe0H1hC8Qzq8x8y1F1f3mYl3F1OeK
b7oN2cYqG3l3x9Y5Qm4n0j3qg1o2Yx6EJp7Yz1m3m3i8j3i2m5q6p7s8t9u0v1w2
h3i4j5k6l7m8n9o0p1q2r3s4t5u6v7w8x9y0zABCD==
-----END RSA PRIVATE KEY-----";

    public const string RsaPublicPem = @"-----BEGIN PUBLIC KEY-----
MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAu3w2eA1E0r8kR8Kj7+oQ
8l1o3s4vPMjT4i9GQqgT3o1Zf8M70v2yZk3Q0nqA0Gv+3e0bY1j3o0s5s8e5yV2Nw
Jw3FfS7iCw2w4bqv6c1rGOkz0cS2bS7lXy1j71yP5b6C1ZKQ2yS7gq1x3jQyq8g7
wG0mTI6VtE6K+Hj1yY9mH5q5p5ex5x3lC2n7S3v8zXo2wIDAQAB
-----END PUBLIC KEY-----";
}
