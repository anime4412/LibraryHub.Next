using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SOF1022_NHOM1_SD2003_Ver2.Services;
using System;
using System.Collections.Generic;

namespace SOF1022_NHOM1_SD2003_Ver2.Helpers
{
    public static class PermissionExtensions
    {
        public const string ItemsKey = "_UserPermissionCodes";

        public static bool HasPermission(this HttpContext? context, string permissionCode)
        {
            if (context == null || string.IsNullOrWhiteSpace(permissionCode)) return false;

            var userRole = context.Session.GetString("Role");
            if (string.Equals(userRole, "Admin", StringComparison.OrdinalIgnoreCase))
                return true;

            // 1. Check cached permissions in HttpContext.Items for current request
            if (context.Items.TryGetValue(ItemsKey, out var obj) && obj is HashSet<string> cachedCodes)
            {
                return cachedCodes.Contains(permissionCode);
            }

            // 2. Fallback: load permissions if not yet cached in current request
            var employeeIdStr = context.Session.GetString("EmployeeId");
            if (!string.IsNullOrEmpty(employeeIdStr) && int.TryParse(employeeIdStr, out int empId))
            {
                var permService = context.RequestServices.GetService<IPermissionService>();
                if (permService != null)
                {
                    var codes = permService.GetPermissionCodesAsync(empId).GetAwaiter().GetResult();
                    var codeSet = new HashSet<string>(codes);
                    context.Items[ItemsKey] = codeSet;
                    return codeSet.Contains(permissionCode);
                }
            }

            return false;
        }

        public static bool HasAnyPermission(this HttpContext? context, params string[] permissionCodes)
        {
            if (context == null || permissionCodes == null || permissionCodes.Length == 0) return false;

            var userRole = context.Session.GetString("Role");
            if (string.Equals(userRole, "Admin", StringComparison.OrdinalIgnoreCase))
                return true;

            foreach (var code in permissionCodes)
            {
                if (context.HasPermission(code))
                    return true;
            }
            return false;
        }
    }
}
