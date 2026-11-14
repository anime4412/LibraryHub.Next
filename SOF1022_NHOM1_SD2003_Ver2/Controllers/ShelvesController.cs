using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SOF1022_NHOM1_SD2003_Ver2.Attributes;
using SOF1022_NHOM1_SD2003_Ver2.Datas;
using SOF1022_NHOM1_SD2003_Ver2.Models;
using SOF1022_NHOM1_SD2003_Ver2.Models.ViewModels;

[SessionAuthorize]
[PermissionAuthorize("shelf.view")]
public class ShelvesController : Controller
{
    private readonly LibraryDbContext _context;

    public ShelvesController(LibraryDbContext context)
    {
        _context = context;
    }

    // GET: Shelves
    [PermissionAuthorize("shelf.view")]
    public async Task<IActionResult> Index(string? searchString)
    {
        var shelves = _context.Shelves
            .Include(s => s.Books)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            shelves = shelves.Where(s =>
                s.Name!.Contains(searchString) ||
                s.Code!.Contains(searchString));
        }

        ViewBag.SearchString = searchString;

        var shelvesList = await shelves.ToListAsync();
        
        // Calculate total quantity for each shelf (excluding deleted books)
        var shelfOccupancy = new Dictionary<int, int>();
        foreach (var shelf in shelvesList)
        {
            int totalQuantity = shelf.Books?.Where(b => !b.IsDeleted).Sum(b => b.Quantity) ?? 0;
            shelfOccupancy[shelf.Id] = totalQuantity;
        }
        
        ViewBag.ShelfOccupancy = shelfOccupancy;

        return View(shelvesList);
    }

    // GET: Shelves/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var shelf = await _context.Shelves
            .Include(s => s.Books)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (shelf == null)
        {
            return NotFound();
        }

        // Calculate total quantity for the shelf (excluding deleted books)
        int totalQuantity = shelf.Books?.Where(b => !b.IsDeleted).Sum(b => b.Quantity) ?? 0;
        ViewBag.TotalQuantity = totalQuantity;

        return View(shelf);
    }

    // GET: Shelves/Create
    [PermissionAuthorize("shelf.create")]
    public IActionResult Create()
    {
        return View();
    }

    // POST: Shelves/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [PermissionAuthorize("shelf.create")]
    public async Task<IActionResult> Create(Shelves shelf)
    {
        bool existsCode = await _context.Shelves
            .AnyAsync(s => s.Code == shelf.Code);

        if (existsCode)
        {
            ModelState.AddModelError("Code", "Mã kệ đã tồn tại.");
        }

        if (!ModelState.IsValid)
        {
            return View(shelf);
        }

        try
        {
            _context.Shelves.Add(shelf);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Thêm kệ sách thành công.";

            return RedirectToAction(nameof(Index));
        }
        catch
        {
            TempData["Error"] = "Có lỗi xảy ra khi thêm dữ liệu.";

            return View(shelf);
        }
    }

    // GET: Shelves/Edit/5
    [PermissionAuthorize("shelf.edit")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var shelf = await _context.Shelves.FindAsync(id);

        if (shelf == null)
        {
            return NotFound();
        }

        return View(shelf);
    }

    // POST: Shelves/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [PermissionAuthorize("shelf.edit")]
    public async Task<IActionResult> Edit(int id, Shelves shelf)
    {
        if (id != shelf.Id)
        {
            return NotFound();
        }

        bool existsCode = await _context.Shelves
            .AnyAsync(s => s.Code == shelf.Code && s.Id != shelf.Id);

        if (existsCode)
        {
            ModelState.AddModelError("Code", "Mã kệ đã tồn tại.");
        }

        int currentBooksOnShelf = await _context.Books
            .Where(b => b.ShelvesId == shelf.Id && !b.IsDeleted)
            .SumAsync(b => (int?)b.Quantity) ?? 0;

        if (shelf.Capacity < currentBooksOnShelf)
        {
            ModelState.AddModelError("Capacity", $"Sức chứa tối đa ({shelf.Capacity} cuốn) không thể nhỏ hơn số lượng sách hiện có trên kệ ({currentBooksOnShelf} cuốn). Vui lòng chuyển bớt sách sang kệ khác trước khi giảm sức chứa.");
        }

        if (!ModelState.IsValid)
        {
            return View(shelf);
        }

        try
        {
            _context.Update(shelf);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Cập nhật kệ sách thành công.";

            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await ShelfExists(shelf.Id))
            {
                return NotFound();
            }

            throw;
        }
        catch
        {
            TempData["Error"] = "Có lỗi xảy ra khi cập nhật.";

            return View(shelf);
        }
    }

    // GET: Shelves/Delete/5
    [PermissionAuthorize("shelf.delete")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var shelf = await _context.Shelves
            .FirstOrDefaultAsync(s => s.Id == id);

        if (shelf == null)
        {
            return NotFound();
        }

        return View(shelf);
    }

    // POST: Shelves/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [PermissionAuthorize("shelf.delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var shelf = await _context.Shelves
            .Include(s => s.Books)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (shelf == null)
        {
            return NotFound();
        }

        // Check for dependencies
        if (shelf.Books != null && shelf.Books.Any())
        {
            TempData["Error"] = "Không thể xóa Kệ sách này vì hiện đang có sách được xếp trên kệ. Vui lòng chuyển sách sang kệ khác trước khi xóa kệ.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            _context.Shelves.Remove(shelf);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Xóa kệ sách thành công.";
        }
        catch
        {
            TempData["Error"] = "Có lỗi xảy ra khi xóa dữ liệu.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> ShelfExists(int id)
    {
        return await _context.Shelves.AnyAsync(s => s.Id == id);
    }

    // GET: Shelves/GetTransferBooks/{id}
    [HttpGet]
    public async Task<IActionResult> GetTransferBooks(int id)
    {
        var shelf = await _context.Shelves
            .FirstOrDefaultAsync(s => s.Id == id);

        if (shelf == null)
            return Json(new { success = false, message = "Không tìm thấy kệ sách." });

        // Get books on this shelf
        var books = await _context.Books
            .Where(b => b.ShelvesId == id)
            .Select(b => new
            {
                b.Id,
                b.Title,
                b.Quantity,
                b.ISBN
            })
            .ToListAsync();

        // Get all shelves with available space
        var availableShelves = await _context.Shelves
            .Select(s => new
            {
                s.Id,
                s.Name,
                s.Code,
                s.Capacity,
                CurrentQuantity = _context.Books.Where(b => b.ShelvesId == s.Id).Sum(b => (int?)b.Quantity) ?? 0,
                AvailableSpace = s.Capacity - (_context.Books.Where(b => b.ShelvesId == s.Id).Sum(b => (int?)b.Quantity) ?? 0)
            })
            .Where(s => s.AvailableSpace > 0 && s.Id != id)
            .OrderByDescending(s => s.AvailableSpace)
            .ToListAsync();

        return Json(new { 
            success = true, 
            currentShelf = new { shelf.Id, shelf.Name, shelf.Code, shelf.Capacity },
            books,
            availableShelves
        });
    }

    // POST: Shelves/TransferBooks
    [HttpPost]
    public async Task<IActionResult> TransferBooks([FromBody] TransferBooksRequest model)
    {
        if (model == null || model.BookId <= 0 || model.TargetShelfId <= 0 || model.Quantity <= 0)
        {
            return BadRequest(new { success = false, message = "Dữ liệu gửi lên không hợp lệ!" });
        }

        var sourceShelf = await _context.Shelves.FindAsync(model.SourceShelfId);
        var targetShelf = await _context.Shelves.FindAsync(model.TargetShelfId);
        var book = await _context.Books.FindAsync(model.BookId);

        if (sourceShelf == null || targetShelf == null || book == null)
        {
            return Json(new { success = false, message = "Không tìm thấy dữ liệu." });
        }

        if (book.ShelvesId != model.SourceShelfId)
        {
            return Json(new { success = false, message = "Sách không thuộc kệ nguồn." });
        }

        if (model.Quantity > book.Quantity)
        {
            return Json(new { success = false, message = $"Số lượng chuyển ({model.Quantity}) vượt quá số lượng hiện có ({book.Quantity})." });
        }

        // Check target shelf capacity
        int targetCurrentQuantity = await _context.Books
            .Where(b => b.ShelvesId == model.TargetShelfId && b.Id != model.BookId && !b.IsDeleted)
            .SumAsync(b => b.Quantity);
        int targetAvailableSpace = targetShelf.Capacity - targetCurrentQuantity;

        if (model.Quantity > targetAvailableSpace)
        {
            return Json(new { success = false, message = $"Kệ đích '{targetShelf.Name}' chỉ còn {targetAvailableSpace} chỗ trống, không thể chuyển {model.Quantity} cuốn." });
        }

        try
        {
            if (model.Quantity == book.Quantity)
            {
                // Move entire book to target shelf
                book.ShelvesId = model.TargetShelfId;
            }
            else
            {
                // Split book: reduce quantity on source, create new record on target
                book.Quantity -= model.Quantity;
                book.AvailableQuantity -= model.Quantity;

                var newBook = new Books
                {
                    ISBN = book.ISBN,
                    Title = book.Title,
                    Description = book.Description,
                    Edition = book.Edition,
                    Keywords = book.Keywords,
                    PublishYear = book.PublishYear,
                    PageCount = book.PageCount,
                    Language = book.Language,
                    Price = book.Price,
                    Quantity = model.Quantity,
                    AvailableQuantity = model.Quantity,
                    MinimumAge = book.MinimumAge,
                    CoverImage = book.CoverImage,
                    CategoriesId = book.CategoriesId,
                    AuthorsId = book.AuthorsId,
                    PublishersId = book.PublishersId,
                    ShelvesId = model.TargetShelfId,
                    BooksStatus = book.BooksStatus
                };

                _context.Books.Add(newBook);
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = $"Đã chuyển {model.Quantity} cuốn sang kệ '{targetShelf.Name}' thành công." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
        }
    }
}
