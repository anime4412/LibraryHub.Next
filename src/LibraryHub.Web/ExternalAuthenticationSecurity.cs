using Microsoft.AspNetCore.Identity;

namespace LibraryHub.Web;

public static class ExternalAuthenticationSecurity
{
    public const string EmailVerifiedClaimType = "urn:libraryhub:external:email-verified";

    public static bool IsSupportedProvider(string? provider) =>
        string.Equals(provider, "Google", StringComparison.Ordinal)
        || string.Equals(provider, "Facebook", StringComparison.Ordinal);

    public static bool IsProviderEmailVerified(ExternalLoginInfo info) =>
        string.Equals(info.LoginProvider, "Google", StringComparison.Ordinal)
        && string.Equals(
            info.Principal.FindFirst(EmailVerifiedClaimType)?.Value,
            bool.TrueString,
            StringComparison.OrdinalIgnoreCase);
}
