using Microsoft.EntityFrameworkCore;
using SOF1022_NHOM1_SD2003_Ver2.Datas;
using SOF1022_NHOM1_SD2003_Ver2.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SOF1022_NHOM1_SD2003_Ver2.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly LibraryDbContext _context;

        public PermissionService(LibraryDbContext context)
        {
            _context = context;
        }

        public async Task<bool> HasPermissionAsync(int employeeId, string permissionCode)
        {
            var employee = await _context.Employees
                .Include(e => e.Role)
                .Include(e => e.UserPermissions)
                .ThenInclude(up => up.Permission)
                .FirstOrDefaultAsync(e => e.Id == employeeId);

            if (employee == null)
                return false;

            // SUPER ADMIN BYPASS: Admin role always has all permissions
            // Note: PermissionAuthorizeAttribute already bypasses Admin before calling this
            // This is an additional safety check
            if (employee.Role?.Name == "Admin")
                return true;

            // Check for user-specific override
            var userPermission = employee.UserPermissions
                .FirstOrDefault(up => up.Permission.Code == permissionCode);

            if (userPermission != null)
                return userPermission.IsGranted;

            // Check role permissions
            if (employee.RoleId.HasValue)
            {
                var hasRolePermission = await _context.RolePermissions
                    .AnyAsync(rp => rp.RoleId == employee.RoleId.Value && rp.Permission.Code == permissionCode);
                return hasRolePermission;
            }

            return false;
        }

        public async Task<List<string>> GetPermissionCodesAsync(int employeeId)
        {
            var employee = await _context.Employees
                .Include(e => e.Role)
                .Include(e => e.UserPermissions)
                .ThenInclude(up => up.Permission)
                .FirstOrDefaultAsync(e => e.Id == employeeId);

            if (employee == null)
                return new List<string>();

            // Admin role has all permissions
            if (employee.Role?.Name == "Admin")
            {
                return await _context.Permissions
                    .Select(p => p.Code)
                    .ToListAsync();
            }

            var permissionCodes = new List<string>();

            // Get role permissions
            if (employee.RoleId.HasValue)
            {
                var rolePermissions = await _context.RolePermissions
                    .Where(rp => rp.RoleId == employee.RoleId.Value)
                    .Include(rp => rp.Permission)
                    .Select(rp => rp.Permission.Code)
                    .ToListAsync();
                permissionCodes.AddRange(rolePermissions);
            }

            // Apply user-specific overrides
            foreach (var userPermission in employee.UserPermissions)
            {
                if (userPermission.IsGranted)
                {
                    if (!permissionCodes.Contains(userPermission.Permission.Code))
                        permissionCodes.Add(userPermission.Permission.Code);
                }
                else
                {
                    permissionCodes.Remove(userPermission.Permission.Code);
                }
            }

            return permissionCodes;
        }
    }
}
