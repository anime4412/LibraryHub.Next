using LibraryHub.Domain;
using Microsoft.AspNetCore.Identity;

namespace LibraryHub.Infrastructure;

public sealed class LibraryUser : IdentityUser<Guid>
{
    public AccountKind AccountKind { get; set; } = AccountKind.Reader;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool MustChangePassword { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }
}
