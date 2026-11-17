using LibraryHub.Application;
using LibraryHub.Domain;
using Microsoft.EntityFrameworkCore;

namespace LibraryHub.Infrastructure;

public sealed class PermissionSeeder(LibraryDbContext db) : IPermissionSeeder
{
    public Task SeedAsync(CancellationToken cancellationToken = default) =>
        db.TransactionAsync(async ct =>
        {
            var permissions = await db.Permissions.ToListAsync(ct);
            var byCode = permissions.ToDictionary(x => x.Code, StringComparer.OrdinalIgnoreCase);

            foreach (var definition in PermissionCodes.All)
            {
                if (byCode.TryGetValue(definition.Code, out var existing))
                {
                    existing.DisplayName = definition.DisplayName;
                    existing.GroupName = definition.GroupName;
                    continue;
                }

                var permission = new Permission
                {
                    Code = definition.Code,
                    DisplayName = definition.DisplayName,
                    GroupName = definition.GroupName
                };
                db.Permissions.Add(permission);
                byCode.Add(permission.Code, permission);
            }

            await db.SaveChangesAsync(ct);

            var roles = await db.SecurityRoles.Where(x => !x.IsDeleted).ToListAsync(ct);
            var existingAssignments = (await db.RolePermissions
                    .Select(x => new { x.RoleId, x.PermissionId })
                    .ToListAsync(ct))
                .Select(x => (x.RoleId, x.PermissionId))
                .ToHashSet();

            foreach (var role in roles)
            {
                var defaults = DefaultsFor(role.Code);
                foreach (var permissionCode in defaults)
                {
                    var permission = byCode[permissionCode];
                    if (!existingAssignments.Add((role.Id, permission.Id))) continue;
                    db.RolePermissions.Add(new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = permission.Id,
                        Effect = PermissionEffect.Allow
                    });
                }
            }

            await db.SaveChangesAsync(ct);
            return true;
        }, cancellationToken);

    private static IReadOnlySet<string> DefaultsFor(string roleCode)
    {
        var normalized = roleCode.Trim().ToUpperInvariant();
        if (normalized.Contains("ADMIN", StringComparison.Ordinal))
            return PermissionCodes.All.Select(x => x.Code)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (normalized.Contains("ACCOUNT", StringComparison.Ordinal)
            || normalized.Contains("KE_TOAN", StringComparison.Ordinal)
            || normalized.Contains("KETOAN", StringComparison.Ordinal))
            return PermissionCodes.AccountantDefaults;
        return PermissionCodes.StaffDefaults;
    }
}
