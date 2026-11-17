using System.Security.Claims;
using LibraryHub.Web;
using Microsoft.AspNetCore.Identity;
using Xunit;

namespace LibraryHub.UnitTests;

public sealed class ExternalAuthenticationSecurityTests
{
    [Theory]
    [InlineData("Google", true)]
    [InlineData("Facebook", true)]
    [InlineData("google", false)]
    [InlineData("Unknown", false)]
    [InlineData("", false)]
    public void Supported_provider_names_are_allowlisted_exactly(string provider, bool expected) =>
        Assert.Equal(expected, ExternalAuthenticationSecurity.IsSupportedProvider(provider));

    [Fact]
    public void Verified_google_email_claim_is_trusted()
    {
        var info = ExternalInfo("Google", verified: true);

        Assert.True(ExternalAuthenticationSecurity.IsProviderEmailVerified(info));
    }

    [Theory]
    [InlineData("Google", false)]
    [InlineData("Facebook", true)]
    [InlineData("Facebook", false)]
    public void Missing_or_untrusted_verified_claim_is_rejected(string provider, bool verified)
    {
        var info = ExternalInfo(provider, verified);

        Assert.False(ExternalAuthenticationSecurity.IsProviderEmailVerified(info));
    }

    private static ExternalLoginInfo ExternalInfo(string provider, bool verified)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "test-provider-key"),
            new(ClaimTypes.Email, "reader@example.test")
        };
        if (verified)
            claims.Add(new Claim(ExternalAuthenticationSecurity.EmailVerifiedClaimType, bool.TrueString));

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, provider));
        return new ExternalLoginInfo(principal, provider, "test-provider-key", provider);
    }
}
