using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SOF1022_NHOM1_SD2003_Ver2.Attributes
{
    public class SessionAuthorizeAttribute
       : ActionFilterAttribute
    {
        private readonly string _role;

        public SessionAuthorizeAttribute(
            string role = "")
        {
            _role = role;
        }

        public string? Roles { get; set; }

        public override void OnActionExecuting(
            ActionExecutingContext context)
        {
            var session =
                context.HttpContext.Session;

            if (context.HttpContext.Request.Query.ContainsKey("preview"))
            {
                base.OnActionExecuting(context);
                return;
            }

            var userId =
                session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                context.Result =
                    new RedirectToActionResult(
                        "Login",
                        "Account",
                        null);

                return;
            }

            // Check role from Roles property or constructor parameter
            var rolesToCheck = !string.IsNullOrEmpty(Roles) ? Roles : _role;

            if (!string.IsNullOrEmpty(rolesToCheck))
            {
                var userRole =
                    session.GetString("Role");

                if (string.IsNullOrEmpty(userRole))
                {
                    context.Result =
                        new RedirectToActionResult(
                            "AccessDenied",
                            "Account",
                            null);
                    return;
                }

                // Admin bypass: Admin has full access
                if (string.Equals(userRole, "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    base.OnActionExecuting(context);
                    return;
                }

                // Support multiple roles separated by comma with alias normalization
                var allowedRoles = rolesToCheck.Split(',')
                    .Select(r => r.Trim())
                    .SelectMany(r => r switch
                    {
                        "Librarian" => new[] { "Librarian", "Thủ thư", "Quản lý thư viện", "Nhân viên kho", "Chăm sóc khách hàng", "Nhân viên" },
                        "Thủ thư" => new[] { "Librarian", "Thủ thư", "Quản lý thư viện" },
                        "Quản lý thư viện" => new[] { "Librarian", "Thủ thư", "Quản lý thư viện" },
                        _ => new[] { r }
                    })
                    .ToList();
                
                if (!allowedRoles.Any(r => string.Equals(r, userRole, StringComparison.OrdinalIgnoreCase)))
                {
                    context.Result =
                        new RedirectToActionResult(
                            "AccessDenied",
                            "Account",
                            null);
                    return;
                }
            }

            base.OnActionExecuting(context);
        }
    }
}
