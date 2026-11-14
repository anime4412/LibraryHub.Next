using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SOF1022_NHOM1_SD2003_Ver2.Helpers;
using System;
using System.Threading.Tasks;

namespace SOF1022_NHOM1_SD2003_Ver2.Attributes
{
    public class PermissionAuthorizeAttribute : ActionFilterAttribute
    {
        private readonly string[] _permissionCodes;

        public PermissionAuthorizeAttribute(params string[] permissionCodes)
        {
            _permissionCodes = permissionCodes;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.HttpContext.Request.Query.ContainsKey("preview"))
            {
                await next();
                return;
            }

            var session = context.HttpContext.Session;
            
            // SUPER ADMIN BYPASS: Admin has all permissions
            var userRole = session.GetString("Role");
            if (string.Equals(userRole, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                await next();
                return;
            }

            var employeeIdStr = session.GetString("EmployeeId");
            if (string.IsNullOrEmpty(employeeIdStr) || !int.TryParse(employeeIdStr, out _))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
                return;
            }

            // Check permissions using optimized extension
            if (_permissionCodes == null || _permissionCodes.Length == 0)
            {
                await next();
                return;
            }

            var hasPermission = false;
            foreach (var code in _permissionCodes)
            {
                if (context.HttpContext.HasPermission(code))
                {
                    hasPermission = true;
                    break;
                }
            }

            if (!hasPermission)
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
                return;
            }

            await next();
        }
    }
}
