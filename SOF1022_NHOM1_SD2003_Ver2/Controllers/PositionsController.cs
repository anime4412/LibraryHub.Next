using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SOF1022_NHOM1_SD2003_Ver2.Attributes;
using SOF1022_NHOM1_SD2003_Ver2.Datas;
using SOF1022_NHOM1_SD2003_Ver2.Models;

[SessionAuthorize(Roles = "Admin")]
[PermissionAuthorize("employee.assign_role")]
public class PositionsController : Controller
{
    private readonly LibraryDbContext _context;

    public PositionsController(LibraryDbContext context)
    {
        _context = context;
    }

    // ========================= GET ALL (AJAX) =========================

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var positions = await _context.Positions
            .OrderBy(p => p.Name)
            .Select(p => new
            {
                id = p.Id,
                name = p.Name
            })
            .ToListAsync();

        return Json(positions);
    }

    // ========================= INDEX =========================

    public async Task<IActionResult> Index()
    {
        var positions = await _context.Positions
            .OrderBy(p => p.Name)
            .ToListAsync();

        return View(positions);
    }

    // ========================= CREATE =========================

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Position position)
    {
        if (!ModelState.IsValid)
        {
            return View(position);
        }

        try
        {
            position.Name = position.Name?.Trim();
            position.Description = position.Description?.Trim();

            // Check for duplicate name
            bool exists = await _context.Positions
                .AnyAsync(p => p.Name == position.Name);

            if (exists)
            {
                ModelState.AddModelError(nameof(position.Name), "Chức vụ này đã tồn tại.");
                return View(position);
            }

            _context.Positions.Add(position);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Thêm chức vụ thành công.";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau.");
            return View(position);
        }
    }

    // ========================= CREATE (AJAX) =========================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAjax(string Name, string Description, 
        bool CanManageBooks, bool CanManageUsers, bool CanManageLoans, 
        bool CanManageReports, bool CanManageSystem)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                return Json(new { success = false, message = "Tên chức vụ không được để trống." });
            }

            Name = Name.Trim();
            Description = Description?.Trim();

            // Check for duplicate name
            bool exists = await _context.Positions
                .AnyAsync(p => p.Name == Name);

            if (exists)
            {
                return Json(new { success = false, message = "Chức vụ này đã tồn tại." });
            }

            var position = new Position
            {
                Name = Name,
                Description = Description,
                CanManageBooks = CanManageBooks,
                CanManageUsers = CanManageUsers,
                CanManageLoans = CanManageLoans,
                CanManageReports = CanManageReports,
                CanManageSystem = CanManageSystem,
                CreatedAt = DateTime.Now
            };

            _context.Positions.Add(position);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Thêm chức vụ thành công!" });
        }
        catch (Exception)
        {
            return Json(new { success = false, message = "Đã xảy ra lỗi hệ thống." });
        }
    }

    // ========================= EDIT =========================

    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var position = await _context.Positions.FindAsync(id);

        if (position == null)
        {
            return NotFound();
        }

        return View(position);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Position position)
    {
        if (id != position.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(position);
        }

        try
        {
            position.Name = position.Name?.Trim();
            position.Description = position.Description?.Trim();
            position.UpdatedAt = DateTime.Now;

            // Check for duplicate name (excluding current record)
            bool exists = await _context.Positions
                .AnyAsync(p => p.Name == position.Name && p.Id != position.Id);

            if (exists)
            {
                ModelState.AddModelError(nameof(position.Name), "Chức vụ này đã tồn tại.");
                return View(position);
            }

            _context.Update(position);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cập nhật chức vụ thành công.";

            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException)
        {
            bool exists = await _context.Positions.AnyAsync(p => p.Id == position.Id);

            if (!exists)
            {
                return NotFound();
            }

            ModelState.AddModelError(string.Empty, "Dữ liệu đã được thay đổi bởi người dùng khác. Vui lòng tải lại trang và thử lại.");
            return View(position);
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau.");
            return View(position);
        }
    }

    // ========================= DELETE =========================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var position = await _context.Positions.FindAsync(id);

            if (position == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Chức vụ không tồn tại."
                });
            }

            // Check if position is being used by any employee
            bool isInUse = await _context.Employees
                .AnyAsync(e => e.PositionId == id);

            if (isInUse)
            {
                return Json(new
                {
                    success = false,
                    message = "Không thể xóa chức vụ này vì đang được sử dụng bởi nhân viên."
                });
            }

            _context.Positions.Remove(position);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = $"Đã xóa chức vụ \"{position.Name}\" thành công."
            });
        }
        catch (Exception)
        {
            return Json(new
            {
                success = false,
                message = "Đã xảy ra lỗi hệ thống."
            });
        }
    }
}
