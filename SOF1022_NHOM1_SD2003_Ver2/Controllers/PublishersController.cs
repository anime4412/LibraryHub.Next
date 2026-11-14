using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SOF1022_NHOM1_SD2003_Ver2.Attributes;
using SOF1022_NHOM1_SD2003_Ver2.Datas;
using SOF1022_NHOM1_SD2003_Ver2.Models;

[SessionAuthorize]
[PermissionAuthorize("publisher.view")]
public class PublishersController : Controller
{
    private readonly LibraryDbContext _context;

    public PublishersController(LibraryDbContext context)
    {
        _context = context;
    }

    // GET: Publishers
    [PermissionAuthorize("publisher.view")]
    public async Task<IActionResult> Index(string? searchString)
    {
        var publishers = _context.Publishers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            publishers = publishers.Where(p =>
                p.Name!.Contains(searchString));
        }

        ViewBag.SearchString = searchString;

        return View(await publishers.ToListAsync());
    }

    // GET: Publishers/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var publisher = await _context.Publishers
            .Include(p => p.Books)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (publisher == null)
        {
            return NotFound();
        }

        return View(publisher);
    }

    // GET: Publishers/Create
    [PermissionAuthorize("publisher.create")]
    public IActionResult Create()
    {
        return View();
    }

    // POST: Publishers/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [PermissionAuthorize("publisher.create")]
    public async Task<IActionResult> Create(Publishers publisher)
    {
        bool exists = await _context.Publishers
            .AnyAsync(p => p.Name == publisher.Name);
        bool emailExists = await _context.Publishers
            .AnyAsync(p => p.Email == publisher.Email && p.Id != publisher.Id);
        bool phoneExists = await _context.Publishers
            .AnyAsync(p => p.Phone == publisher.Phone && p.Id != publisher.Id);

        if (!string.IsNullOrWhiteSpace(publisher.Email) && emailExists)
        {
            ModelState.AddModelError("Email", "Email đã tồn tại.");
        }

        if (!string.IsNullOrWhiteSpace(publisher.Phone) && phoneExists)
        {
            ModelState.AddModelError("Phone", "Số điện thoại đã tồn tại.");
        }

        if (exists)
        {
            ModelState.AddModelError("Name", "Tên nhà xuất bản đã tồn tại.");
        }

        if (!ModelState.IsValid)
        {
            return View(publisher);
        }

        try
        {
            _context.Publishers.Add(publisher);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Thêm nhà xuất bản thành công.";

            return RedirectToAction(nameof(Index));
        }
        catch
        {
            TempData["Error"] = "Có lỗi xảy ra khi thêm dữ liệu.";

            return View(publisher);
        }
    }

    // GET: Publishers/Edit/5
    [PermissionAuthorize("publisher.edit")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var publisher = await _context.Publishers.FindAsync(id);

        if (publisher == null)
        {
            return NotFound();
        }

        return View(publisher);
    }

    // POST: Publishers/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [PermissionAuthorize("publisher.edit")]
    public async Task<IActionResult> Edit(int id, Publishers publisher)
    {
        if (id != publisher.Id)
        {
            return NotFound();
        }

        bool exists = await _context.Publishers
            .AnyAsync(p => p.Name == publisher.Name && p.Id != publisher.Id);

        if (exists)
        {
            ModelState.AddModelError("Name", "Tên nhà xuất bản đã tồn tại.");
        }

        if (!ModelState.IsValid)
        {
            return View(publisher);
        }

        try
        {
            _context.Update(publisher);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Cập nhật nhà xuất bản thành công.";

            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await PublisherExists(publisher.Id))
            {
                return NotFound();
            }

            throw;
        }
        catch
        {
            TempData["Error"] = "Có lỗi xảy ra khi cập nhật.";

            return View(publisher);
        }
    }

    // GET: Publishers/Delete/5
    [PermissionAuthorize("publisher.delete")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var publisher = await _context.Publishers
            .FirstOrDefaultAsync(p => p.Id == id);

        if (publisher == null)
        {
            return NotFound();
        }

        return View(publisher);
    }

    // POST: Publishers/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [PermissionAuthorize("publisher.delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var publisher = await _context.Publishers
            .Include(p => p.Books)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (publisher == null)
        {
            return NotFound();
        }

        // Check for dependencies
        if (publisher.Books != null && publisher.Books.Any())
        {
            TempData["Error"] = "Không thể xóa Nhà xuất bản này vì vẫn còn sách thuộc nhà xuất bản này trong kho.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            publisher.IsDeleted = true;
            _context.Publishers.Update(publisher);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Xóa nhà xuất bản thành công.";
        }
        catch
        {
            TempData["Error"] = "Có lỗi xảy ra khi xóa dữ liệu.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> PublisherExists(int id)
    {
        return await _context.Publishers.AnyAsync(e => e.Id == id);
    }
}
