using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SOF1022_NHOM1_SD2003_Ver2.Attributes;
using SOF1022_NHOM1_SD2003_Ver2.Datas;
using SOF1022_NHOM1_SD2003_Ver2.Models;

[SessionAuthorize(Roles = "Admin")]
[PermissionAuthorize("employee.edit")]
public class EmployeeStatusOptionsController : Controller
{
    private readonly LibraryDbContext _context;

    public EmployeeStatusOptionsController(LibraryDbContext context)
    {
        _context = context;
    }

    // ========================= GET ALL (AJAX) =========================

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var statusOptions = await _context.EmployeeStatusOptions
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.Name)
            .Select(s => new
            {
                id = s.Id,
                name = s.Name
            })
            .ToListAsync();

        return Json(statusOptions);
    }

    // ========================= INDEX =========================

    public async Task<IActionResult> Index()
    {
        var statusOptions = await _context.EmployeeStatusOptions
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.Name)
            .ToListAsync();

        return View(statusOptions);
    }

    // ========================= CREATE =========================

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EmployeeStatusOption statusOption)
    {
        if (!ModelState.IsValid)
        {
            return View(statusOption);
        }

        try
        {
            statusOption.Name = statusOption.Name?.Trim();
            statusOption.Description = statusOption.Description?.Trim();

            // Check for duplicate name
            bool exists = await _context.EmployeeStatusOptions
                .AnyAsync(s => s.Name == statusOption.Name);

            if (exists)
            {
                ModelState.AddModelError(nameof(statusOption.Name), "Trạng thái này đã tồn tại.");
                return View(statusOption);
            }

            _context.EmployeeStatusOptions.Add(statusOption);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Thêm trạng thái thành công.";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau.");
            return View(statusOption);
        }
    }

    // ========================= CREATE (AJAX) =========================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAjax(string Name, string Description, int DisplayOrder)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                return Json(new { success = false, message = "Tên trạng thái không được để trống." });
            }

            Name = Name.Trim();
            Description = Description?.Trim();

            // Check for duplicate name
            bool exists = await _context.EmployeeStatusOptions
                .AnyAsync(s => s.Name == Name);

            if (exists)
            {
                return Json(new { success = false, message = "Trạng thái này đã tồn tại." });
            }

            var statusOption = new EmployeeStatusOption
            {
                Name = Name,
                Description = Description,
                DisplayOrder = DisplayOrder,
                CreatedAt = DateTime.Now
            };

            _context.EmployeeStatusOptions.Add(statusOption);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Thêm trạng thái thành công!" });
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

        var statusOption = await _context.EmployeeStatusOptions.FindAsync(id);

        if (statusOption == null)
        {
            return NotFound();
        }

        return View(statusOption);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EmployeeStatusOption statusOption)
    {
        if (id != statusOption.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(statusOption);
        }

        try
        {
            statusOption.Name = statusOption.Name?.Trim();
            statusOption.Description = statusOption.Description?.Trim();
            statusOption.UpdatedAt = DateTime.Now;

            // Check for duplicate name (excluding current record)
            bool exists = await _context.EmployeeStatusOptions
                .AnyAsync(s => s.Name == statusOption.Name && s.Id != statusOption.Id);

            if (exists)
            {
                ModelState.AddModelError(nameof(statusOption.Name), "Trạng thái này đã tồn tại.");
                return View(statusOption);
            }

            _context.Update(statusOption);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cập nhật trạng thái thành công.";

            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException)
        {
            bool exists = await _context.EmployeeStatusOptions.AnyAsync(s => s.Id == statusOption.Id);

            if (!exists)
            {
                return NotFound();
            }

            ModelState.AddModelError(string.Empty, "Dữ liệu đã được thay đổi bởi người dùng khác. Vui lòng tải lại trang và thử lại.");
            return View(statusOption);
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau.");
            return View(statusOption);
        }
    }

    // ========================= DELETE =========================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var statusOption = await _context.EmployeeStatusOptions.FindAsync(id);

            if (statusOption == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Trạng thái không tồn tại."
                });
            }

            // Check if status is being used by any employee
            bool isInUse = await _context.Employees
                .AnyAsync(e => e.StatusId == id);

            if (isInUse)
            {
                return Json(new
                {
                    success = false,
                    message = "Không thể xóa trạng thái này vì đang được sử dụng bởi nhân viên."
                });
            }

            _context.EmployeeStatusOptions.Remove(statusOption);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = $"Đã xóa trạng thái \"{statusOption.Name}\" thành công."
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
