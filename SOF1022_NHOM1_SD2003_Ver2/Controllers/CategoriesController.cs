using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SOF1022_NHOM1_SD2003_Ver2.Attributes;
using SOF1022_NHOM1_SD2003_Ver2.Datas;
using SOF1022_NHOM1_SD2003_Ver2.Models;

[SessionAuthorize]
[PermissionAuthorize("category.view")]
public class CategoriesController : Controller
{
    private readonly LibraryDbContext _context;

    public CategoriesController(LibraryDbContext context)
    {
        _context = context;
    }

    // GET: Categories
    [PermissionAuthorize("category.view")]
    public async Task<IActionResult> Index(string? searchString)
    {
        var categories = _context.Categories.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            categories = categories.Where(c =>
                c.Name!.Contains(searchString));
        }

        ViewBag.SearchString = searchString;

        return View(await categories.ToListAsync());
    }

    // GET: Categories/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var category = await _context.Categories
            .Include(c => c.Books)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
        {
            return NotFound();
        }

        return View(category);
    }

    // GET: Categories/Create
    [PermissionAuthorize("category.create")]
    public IActionResult Create()
    {
        return View();
    }

    // POST: Categories/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [PermissionAuthorize("category.create")]
    public async Task<IActionResult> Create(Categories category)
    {
        bool exists = await _context.Categories
            .AnyAsync(c => c.Name == category.Name);

        if (exists)
        {
            ModelState.AddModelError("Name", "Tên thể loại đã tồn tại.");
        }

        if (!ModelState.IsValid)
        {
            return View(category);
        }

        try
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thêm thể loại thành công.";

            return RedirectToAction(nameof(Index));
        }
        catch
        {
            TempData["Error"] = "Có lỗi xảy ra khi thêm dữ liệu.";

            return View(category);
        }
    }

    // GET: Categories/Edit/5
    [PermissionAuthorize("category.edit")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var category = await _context.Categories.FindAsync(id);

        if (category == null)
        {
            return NotFound();
        }

        return View(category);
    }

    // POST: Categories/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [PermissionAuthorize("category.edit")]
    public async Task<IActionResult> Edit(int id, Categories category)
    {
        if (id != category.Id)
        {
            return NotFound();
        }

        bool exists = await _context.Categories
            .AnyAsync(c => c.Name == category.Name && c.Id != category.Id);

        if (exists)
        {
            ModelState.AddModelError("Name", "Tên thể loại đã tồn tại.");
        }

        if (!ModelState.IsValid)
        {
            return View(category);
        }

        try
        {
            _context.Update(category);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Cập nhật thành công.";

            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await CategoryExists(category.Id))
            {
                return NotFound();
            }

            throw;
        }
        catch
        {
            TempData["Error"] = "Có lỗi xảy ra khi cập nhật.";

            return View(category);
        }
    }

    // GET: Categories/Delete/5
    [PermissionAuthorize("category.delete")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
        {
            return NotFound();
        }

        return View(category);
    }

    // POST: Categories/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [PermissionAuthorize("category.delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var category = await _context.Categories
            .Include(c => c.Books)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
        {
            return NotFound();
        }

        // Check for dependencies
        if (category.Books != null && category.Books.Any())
        {
            TempData["Error"] = "Không thể xóa Danh mục này vì hiện đang có sách thuộc danh mục này. Bạn cần chuyển các cuốn sách đó sang danh mục khác trước.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            _context.Categories.Remove(category);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Xóa thể loại thành công.";
        }
        catch
        {
            TempData["Error"] = "Có lỗi xảy ra khi xóa dữ liệu.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> CategoryExists(int id)
    {
        return await _context.Categories.AnyAsync(e => e.Id == id);
    }
}
