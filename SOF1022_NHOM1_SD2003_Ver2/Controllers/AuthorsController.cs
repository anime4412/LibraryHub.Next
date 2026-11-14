using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SOF1022_NHOM1_SD2003_Ver2.Attributes;
using SOF1022_NHOM1_SD2003_Ver2.Datas;
using SOF1022_NHOM1_SD2003_Ver2.Models;

[SessionAuthorize]
[PermissionAuthorize("author.view")]
public class AuthorsController : Controller
{
    private readonly LibraryDbContext _context;

    public AuthorsController(LibraryDbContext context)
    {
        _context = context;
    }

    // GET: Authors
    [PermissionAuthorize("author.view")]
    public async Task<IActionResult> Index(string? searchString)
    {
        var authors = _context.Authors.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            authors = authors.Where(a =>
                a.Name!.Contains(searchString));
        }

        ViewBag.SearchString = searchString;

        return View(await authors.ToListAsync());
    }

    // GET: Authors/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var author = await _context.Authors
            .Include(a => a.Books)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (author == null)
        {
            return NotFound();
        }

        return View(author);
    }

    // GET: Authors/Create
    [PermissionAuthorize("author.create")]
    public IActionResult Create()
    {
        return View();
    }

    // POST: Authors/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [PermissionAuthorize("author.create")]
    public async Task<IActionResult> Create(Authors author)
    {
        bool exists = await _context.Authors
            .AnyAsync(a => a.Name == author.Name);

        if (exists)
        {
            ModelState.AddModelError("Name", "Tên tác giả đã tồn tại.");
        }

        if (!ModelState.IsValid)
        {
            return View(author);
        }

        try
        {
            _context.Authors.Add(author);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Thêm tác giả thành công.";

            return RedirectToAction(nameof(Index));
        }
        catch
        {
            TempData["Error"] = "Có lỗi xảy ra khi thêm dữ liệu.";

            return View(author);
        }
    }

    // GET: Authors/Edit/5
    [PermissionAuthorize("author.edit")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var author = await _context.Authors.FindAsync(id);

        if (author == null)
        {
            return NotFound();
        }

        return View(author);
    }

    // POST: Authors/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [PermissionAuthorize("author.edit")]
    public async Task<IActionResult> Edit(int id, Authors author)
    {
        if (id != author.Id)
        {
            return NotFound();
        }

        bool exists = await _context.Authors
            .AnyAsync(a => a.Name == author.Name && a.Id != author.Id);

        if (exists)
        {
            ModelState.AddModelError("Name", "Tên tác giả đã tồn tại.");
        }

        if (!ModelState.IsValid)
        {
            return View(author);
        }

        try
        {
            _context.Update(author);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Cập nhật tác giả thành công.";

            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await AuthorExists(author.Id))
            {
                return NotFound();
            }

            throw;
        }
        catch
        {
            TempData["Error"] = "Có lỗi xảy ra khi cập nhật.";

            return View(author);
        }
    }

    // GET: Authors/Delete/5
    [PermissionAuthorize("author.delete")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var author = await _context.Authors
            .FirstOrDefaultAsync(a => a.Id == id);

        if (author == null)
        {
            return NotFound();
        }

        return View(author);
    }

    // POST: Authors/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [PermissionAuthorize("author.delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var author = await _context.Authors
            .Include(a => a.Books)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (author == null)
        {
            return NotFound();
        }

        // Check for dependencies
        if (author.Books != null && author.Books.Any())
        {
            TempData["Error"] = "Không thể xóa Tác giả này vì hệ thống vẫn còn sách của tác giả này. Vui lòng cập nhật lại thông tin tác giả của các đầu sách liên quan trước.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            author.IsDeleted = true;
            _context.Authors.Update(author);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Xóa tác giả thành công.";
        }
        catch
        {
            TempData["Error"] = "Có lỗi xảy ra khi xóa dữ liệu.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> AuthorExists(int id)
    {
        return await _context.Authors.AnyAsync(a => a.Id == id);
    }
}
