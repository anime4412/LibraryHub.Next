using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SOF1022_NHOM1_SD2003_Ver2.Attributes;
using SOF1022_NHOM1_SD2003_Ver2.Datas;
using SOF1022_NHOM1_SD2003_Ver2.Models;
using SOF1022_NHOM1_SD2003_Ver2.Models.Enums;

[SessionAuthorize]
[PermissionAuthorize("reader.view")]
public class ReadersController : Controller
{
    private readonly LibraryDbContext _context;

    public ReadersController(LibraryDbContext context)
    {
        _context = context;
    }

    // GET: Readers
    public async Task<IActionResult> Index(string? searchString, bool? status, string? sortOrder = "new", int page = 1, int pageSize = 10)
    {
        // Real-time status check
        await CheckReaderStatus();

        var query = _context.Readers
            .Where(r => r.Role == "Reader" && !r.IsDeleted)
            .AsQueryable();

        // Calculate statistics across all active readers in system
        ViewBag.TotalCount = await query.CountAsync();
        ViewBag.ActiveCount = await query.CountAsync(r => r.IsActive);
        ViewBag.LockedCount = await query.CountAsync(r => !r.IsActive);
        ViewBag.TodayCount = await query.CountAsync(r => r.RegisterDate.Date == DateTime.Today);

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            searchString = searchString.Trim();
            query = query.Where(r =>
                (r.FullName != null && r.FullName.Contains(searchString)) ||
                (r.ReaderCode != null && r.ReaderCode.Contains(searchString)) ||
                (r.Phone != null && r.Phone.Contains(searchString)) ||
                (r.Email != null && r.Email.Contains(searchString)));
        }

        if (status.HasValue)
        {
            query = query.Where(r => r.IsActive == status.Value);
        }

        if (sortOrder == "name")
        {
            query = query.OrderBy(r => r.FullName);
        }
        else
        {
            query = query.OrderByDescending(r => r.RegisterDate);
        }

        var filteredCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)filteredCount / pageSize);
        if (totalPages < 1) totalPages = 1;
        if (page > totalPages) page = totalPages;

        ViewBag.SearchString = searchString;
        ViewBag.Status = status;
        ViewBag.SortOrder = sortOrder;
        ViewBag.CurrentPage = page;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalPages = totalPages;
        ViewBag.FilteredCount = filteredCount;

        var readers = await query
            .AsNoTracking()
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return PartialView("_ReaderTablePartial", readers);
        }

        return View(readers);
    }

    // ========================= REAL-TIME STATUS CHECK =========================
    private async Task CheckReaderStatus()
    {
        var readers = await _context.Readers
            .Where(r => r.Role == "Reader" && !r.IsDeleted && r.IsActive)
            .ToListAsync();

        foreach (var reader in readers)
        {
            // Check card expiration
            if (reader.ExpirationDate < DateTime.Now)
            {
                reader.IsActive = false;
            }

            // Check for overdue books (>3)
            int overdueCount = await _context.BorrowSlips
                .CountAsync(l => l.ReaderId == reader.Id && 
                               l.ReturnDate == null && 
                               l.DueDate < DateTime.Now);

            if (overdueCount > 3)
            {
                reader.IsActive = false;
            }
        }

        await _context.SaveChangesAsync();
    }

    // GET: Readers/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var reader = await _context.Readers
            .Include(r => r.BorrowSlips)
            .Include(r => r.Reservations)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

        if (reader == null)
        {
            return NotFound();
        }

        return View(reader);
    }

    // GET: Readers/Create
    [PermissionAuthorize("reader.create")]
    public IActionResult Create()
    {
        return View();
    }

    // POST: Readers/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [PermissionAuthorize("reader.create")]
    public async Task<IActionResult> Create(Readers reader)
    {
        bool codeExists = await _context.Readers
            .AnyAsync(r => r.ReaderCode == reader.ReaderCode && !r.IsDeleted);

        if (codeExists)
        {
            ModelState.AddModelError("ReaderCode", "Mã độc giả đã tồn tại.");
        }

        bool emailExists = await _context.Readers
            .AnyAsync(r => r.Email == reader.Email && reader.Email != null && !r.IsDeleted);

        if (emailExists)
        {
            ModelState.AddModelError("Email", "Email đã tồn tại.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                reader.RegisterDate = DateTime.Now;

                _context.Readers.Add(reader);

                await _context.SaveChangesAsync();

                TempData["Success"] = "Thêm độc giả thành công.";

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                TempData["Error"] = "Có lỗi xảy ra.";
            }
        }

        return View(reader);
    }

    // GET: READERSS/Edit/5
    // [PermissionAuthorize("reader.edit")] // TEMP-DISABLED: allow admin to edit readers
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var reader = await _context.Readers
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

        if (reader == null)
        {
            return NotFound();
        }

        return View(reader);
    }

    // POST: Readers/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    // [PermissionAuthorize("reader.edit")] // TEMP-DISABLED: allow admin to edit readers
    public async Task<IActionResult> Edit(int id, Readers reader)
    {
        if (id != reader.Id)
        {
            return NotFound();
        }

        var existingReader = await _context.Readers
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

        if (existingReader == null)
        {
            return NotFound();
        }

        bool codeExists = await _context.Readers
            .AnyAsync(r => r.ReaderCode == reader.ReaderCode && r.Id != reader.Id && !r.IsDeleted);

        if (codeExists)
        {
            ModelState.AddModelError("ReaderCode", "Mã độc giả đã tồn tại.");
        }

        bool emailExists = await _context.Readers
            .AnyAsync(r => r.Email == reader.Email && r.Id != reader.Id && !r.IsDeleted);

        if (!string.IsNullOrWhiteSpace(reader.Email) && emailExists)
        {
            ModelState.AddModelError("Email", "Email đã tồn tại.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                existingReader.FullName = reader.FullName;
                existingReader.DateOfBirth = reader.DateOfBirth;
                existingReader.Genders = reader.Genders;
                existingReader.Phone = reader.Phone;
                existingReader.Email = reader.Email;
                existingReader.IdentityNumber = reader.IdentityNumber;
                existingReader.Address = reader.Address;
                existingReader.ReadersType = reader.ReadersType;
                existingReader.IsActive = reader.IsActive;

                if (reader.ExpirationDate != default)
                {
                    existingReader.ExpirationDate = reader.ExpirationDate;
                }
                if (reader.MaxBorrowBooks > 0)
                {
                    existingReader.MaxBorrowBooks = reader.MaxBorrowBooks;
                }

                _context.Update(existingReader);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Cập nhật thành công.";

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật.";
            }
        }

        return View(reader);
    }

    // GET: Readers/Delete/5
    [PermissionAuthorize("reader.delete")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var reader = await _context.Readers
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

        if (reader == null)
        {
            return NotFound();
        }

        return View(reader);
    }

    // POST: Readers/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [PermissionAuthorize("reader.delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var reader = await _context.Readers
            .Include(r => r.BorrowSlips)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

        if (reader == null)
        {
            return NotFound();
        }

        // Check for active unreturned loans
        bool hasActiveLoans = await _context.BorrowSlips
            .AnyAsync(l => l.ReaderId == id && l.ReturnDate == null && l.BorrowStatus != BorrowStatus.Cancelled && l.BorrowStatus != BorrowStatus.Returned);

        if (hasActiveLoans)
        {
            TempData["Error"] = "Không thể xóa độc giả này vì họ đang mượn sách chưa trả!";
            return RedirectToAction(nameof(Index));
        }

        // Soft delete: Mark deleted and deactivate
        reader.IsDeleted = true;
        reader.IsActive = false;

        _context.Update(reader);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Xóa độc giả thành công.";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Lock(int id)
    {
        var reader = await _context.Readers
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

        if (reader == null)
        {
            return NotFound();
        }

        reader.IsActive = false;

        _context.Update(reader);

        await _context.SaveChangesAsync();

        TempData["Success"] = "Đã khóa tài khoản.";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Unlock(int id)
    {
        var reader = await _context.Readers
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

        if (reader == null)
        {
            return NotFound();
        }

        reader.IsActive = true;

        _context.Update(reader);

        await _context.SaveChangesAsync();

        TempData["Success"] = "Đã mở khóa tài khoản.";

        return RedirectToAction(nameof(Index));
    }

    // POST: Readers/RenewCard/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [SessionAuthorize("Admin")]
    public async Task<IActionResult> RenewCard(int id)
    {
        var reader = await _context.Readers
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

        if (reader == null)
        {
            return Json(new { success = false, message = "Không tìm thấy độc giả." });
        }

        // Renew card for 1 year
        reader.ExpirationDate = DateTime.Now.AddYears(1);
        reader.IsActive = true;

        _context.Update(reader);
        await _context.SaveChangesAsync();

        return Json(new { 
            success = true, 
            message = "Gia hạn thẻ thành công!", 
            newExpiry = reader.ExpirationDate.ToString("dd/MM/yyyy") 
        });
    }

    // POST: Readers/ResetPassword/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [SessionAuthorize("Admin")]
    public async Task<IActionResult> ResetPassword(int id)
    {
        var reader = await _context.Readers
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

        if (reader == null)
        {
            return NotFound();
        }

        // Generate secure random temporary password
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
        var random = new Random();
        string tempPassword = new string(Enumerable.Repeat(chars, 12)
            .Select(s => s[random.Next(s.Length)]).ToArray());

        // Hash the new password
        var hasher = new PasswordHasher<Readers>();
        reader.Password = hasher.HashPassword(reader, tempPassword);
        reader.ForcePasswordChange = true;

        _context.Update(reader);
        await _context.SaveChangesAsync();

         TempData["Success"] = $"Đã reset mật khẩu. Mật khẩu tạm thời: {tempPassword}";

        return RedirectToAction(nameof(Edit), new { id = id });
    }
}
