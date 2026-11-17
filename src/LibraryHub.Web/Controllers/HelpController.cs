using System.Security.Claims;
using LibraryHub.Application;
using LibraryHub.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryHub.Web.Controllers;

[Authorize]
public sealed class HelpController(LibraryDbContext db, IPermissionService permissions) : Controller
{
    [HttpGet("/help")]
    public async Task<IActionResult> Index()
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var accountId))
            return Challenge();

        if (User.IsInRole("Administrator"))
            return View(new HelpCenterViewModel("Administrator", PermissionCodes.All.Select(x => x.Code).ToHashSet()));

        if (User.IsInRole("Staff"))
        {
            var granted = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var permission in PermissionCodes.All)
                if (await permissions.HasAsync(accountId, permission.Code, HttpContext.RequestAborted))
                    granted.Add(permission.Code);
            return View(new HelpCenterViewModel("Staff", granted));
        }

        var hasReaderProfile = await db.Readers.AsNoTracking().AnyAsync(
            x => x.UserAccountId == accountId && !x.IsDeleted,
            HttpContext.RequestAborted);
        return hasReaderProfile
            ? View(new HelpCenterViewModel("Reader", new HashSet<string>()))
            : Forbid();
    }
}

public sealed record HelpCenterViewModel(string Audience, IReadOnlySet<string> Permissions)
{
    public bool IsAdministrator => Audience == "Administrator";
    public bool IsStaff => Audience == "Staff";
    public bool IsInternal => IsAdministrator || IsStaff;
    public bool Can(string permission) => IsAdministrator || Permissions.Contains(permission);
}
