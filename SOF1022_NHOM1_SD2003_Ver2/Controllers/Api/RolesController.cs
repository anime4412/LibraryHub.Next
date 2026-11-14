using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SOF1022_NHOM1_SD2003_Ver2.Datas;
using SOF1022_NHOM1_SD2003_Ver2.Attributes;

namespace SOF1022_NHOM1_SD2003_Ver2.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [SessionAuthorize]
    public class RolesController : ControllerBase
    {
        private readonly LibraryDbContext _context;

        public RolesController(LibraryDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id}/permissions")]
        [PermissionAuthorize("employee.assign_role")]
        public async Task<IActionResult> GetRolePermissions(int id)
        {
            var role = await _context.Roles
                .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (role == null)
            {
                return NotFound(new { message = "Không tìm thấy vai trò" });
            }

            if (string.Equals(role.Name, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                var allPermIds = await _context.Permissions.Select(p => p.Id).ToListAsync();
                return Ok(allPermIds);
            }

            var permissionIds = role.RolePermissions.Select(rp => rp.PermissionId).ToList();
            return Ok(permissionIds);
        }
    }
}
