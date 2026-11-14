using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SOF1022_NHOM1_SD2003_Ver2.Attributes;
using SOF1022_NHOM1_SD2003_Ver2.Datas;
using SOF1022_NHOM1_SD2003_Ver2.Helpers;
using SOF1022_NHOM1_SD2003_Ver2.Models;
using SOF1022_NHOM1_SD2003_Ver2.Models.Enums;
using SOF1022_NHOM1_SD2003_Ver2.Models.ViewModels;
using Microsoft.AspNetCore.Antiforgery;
using ClosedXML.Excel;

public class BooksController : Controller
{
    private readonly LibraryDbContext _context;

    public BooksController(LibraryDbContext context)
    {
        _context = context;
    }

    // GET: Books
    public async Task<IActionResult> Index(
    string? searchString,
    int? categoryId,
    int? authorId,
    int? publisherId,
    string? sortOrder,
    int PageCount = 1)
    {
        var books = _context.Books
            .Where(x => x.BooksStatus != BooksStatus.Hidden)
            .Include(x => x.Category)
            .Include(x => x.Author)
            .Include(x => x.Publisher)
            .Include(x => x.Shelf)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            books = books.Where(x =>

                (x.Title != null &&
                 EF.Functions.Like(x.Title, $"%{searchString}%"))

                ||

                (x.Description != null &&
                 EF.Functions.Like(x.Description, $"%{searchString}%"))

                ||

                (x.Keywords != null &&
                 EF.Functions.Like(x.Keywords, $"%{searchString}%"))

                ||

                (x.ISBN != null &&
                 EF.Functions.Like(x.ISBN, $"%{searchString}%"))

                ||

                (x.Author != null &&
                 x.Author.Name != null &&
                 EF.Functions.Like(x.Author.Name, $"%{searchString}%"))

                ||

                (x.Category != null &&
                 x.Category.Name != null &&
                 EF.Functions.Like(x.Category.Name, $"%{searchString}%"))

                ||

                (x.Publisher != null &&
                 x.Publisher.Name != null &&
                 EF.Functions.Like(x.Publisher.Name, $"%{searchString}%"))
            );
        }

        // Only apply category filter if a valid category is selected (not null, empty, or 0)
        if (categoryId.HasValue && categoryId.Value > 0)
            books = books.Where(x => x.CategoriesId == categoryId);

        // Only apply author filter if a valid author is selected
        if (authorId.HasValue && authorId.Value > 0)
            books = books.Where(x => x.AuthorsId == authorId);

        // Only apply publisher filter if a valid publisher is selected
        if (publisherId.HasValue && publisherId.Value > 0)
            books = books.Where(x => x.PublishersId == publisherId);

        switch (sortOrder)
        {
            case "name":
                books = books.OrderBy(x => x.Title);
                break;

            case "new":
                books = books.OrderByDescending(x => x.Id);
                break;

            case "quantity":
                books = books.OrderByDescending(x => x.AvailableQuantity);
                break;

            default:
                books = books.OrderByDescending(x => x.Id);
                break;
        }

        const int pageSize = 12;

        var totalItems = await books.CountAsync();

        var result = await books
                        .Skip((PageCount - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();

        ViewBag.CurrentPage = PageCount;

        ViewBag.TotalPages =
            (int)Math.Ceiling((double)totalItems / pageSize);
        ViewBag.TotalItems = totalItems;
        ViewBag.SearchString = searchString;
        ViewBag.CategoryId = categoryId;
        ViewBag.AuthorId = authorId;
        ViewBag.PublisherId = publisherId;
        ViewBag.SortOrder = sortOrder;

        // Populate filter dropdowns
        ViewBag.Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
        ViewBag.Authors = await _context.Authors.OrderBy(a => a.Name).ToListAsync();
        ViewBag.Publishers = await _context.Publishers.OrderBy(p => p.Name).ToListAsync();

        // Calculate real-time availability for each book.
        // Phải tính luôn cả ApprovedAwaitingPickup (phiếu được hệ thống tự động duyệt) vì kể từ lúc
        // duyệt, sách đó đã được giữ chỗ cho độc giả - số lượng hiển thị bên ngoài phải giảm ngay,
        // không đợi đến khi thủ thư "Xác nhận giao sách" mới giảm.
        var committedCounts = await BookAvailabilityHelper.GetCommittedCountsAsync(_context, result.Select(b => b.Id));
        var bookAvailability = new Dictionary<int, (int availableCount, bool isAvailable)>();
        foreach (var book in result)
        {
            int committedCount = committedCounts.TryGetValue(book.Id, out var c) ? c : 0;
            int realAvailableCount = Math.Max(0, book.Quantity - committedCount);
            bookAvailability[book.Id] = (realAvailableCount, realAvailableCount > 0);
        }
        ViewBag.BookAvailability = bookAvailability;

        return View(result);
    }

    // GET: Books/AdminIndex
    [PermissionAuthorize("book.view")]
    public async Task<IActionResult> AdminIndex(
        string? searchString,
        int? categoryId,
        int? authorId,
        int? publisherId,
        string? sortOrder,
        int PageCount = 1)
    {

        var books = _context.Books
            .Include(x => x.Category)
            .Include(x => x.Author)
            .Include(x => x.Publisher)
            .Include(x => x.Shelf)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            books = books.Where(x =>
                (x.Title != null && EF.Functions.Like(x.Title, $"%{searchString}%")) ||
                (x.ISBN != null && EF.Functions.Like(x.ISBN, $"%{searchString}%")) ||
                (x.Author != null && x.Author.Name != null && EF.Functions.Like(x.Author.Name, $"%{searchString}%")) ||
                (x.Category != null && x.Category.Name != null && EF.Functions.Like(x.Category.Name, $"%{searchString}%"))
            );
        }

        if (categoryId.HasValue)
            books = books.Where(x => x.CategoriesId == categoryId);

        if (authorId.HasValue)
            books = books.Where(x => x.AuthorsId == authorId);

        if (publisherId.HasValue)
            books = books.Where(x => x.PublishersId == publisherId);

        switch (sortOrder)
        {
            case "name":
                books = books.OrderBy(x => x.Title);
                break;
            case "new":
                books = books.OrderByDescending(x => x.Id);
                break;
            case "quantity":
                books = books.OrderByDescending(x => x.AvailableQuantity);
                break;
            default:
                books = books.OrderByDescending(x => x.Id);
                break;
        }

        const int pageSize = 12;
        var totalItems = await books.CountAsync();
        var result = await books
            .Skip((PageCount - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.CurrentPage = PageCount;
        ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
        ViewBag.Categories = await _context.Categories.AsNoTracking().ToListAsync();
        ViewBag.Authors = await _context.Authors.AsNoTracking().ToListAsync();
        ViewBag.Publishers = await _context.Publishers.AsNoTracking().ToListAsync();

        // Get summary statistics
        ViewBag.TotalBooks = await _context.Books.CountAsync();
        ViewBag.AvailableBooks = await _context.Books.CountAsync(x => x.AvailableQuantity > 0);
        ViewBag.TotalCategories = await _context.Categories.CountAsync();
        ViewBag.TotalAuthors = await _context.Authors.CountAsync();

        return View(result);
    }

    // GET: Books/Inventory
    public async Task<IActionResult> Inventory()
    {
        // Role check - only Admin can access
        var role = HttpContext.Session.GetString("Role");
        if (role != "Admin")
        {
            return RedirectToAction("Index", "Home");
        }

        var books = await _context.Books
            .Include(x => x.Category)
            .Include(x => x.Author)
            .Include(x => x.Shelf)
            .AsNoTracking()
            .OrderBy(x => x.Title)
            .ToListAsync();

        // Get categories and shelves for filter dropdowns
        var categories = await _context.Categories
            .OrderBy(c => c.Name)
            .ToListAsync();
        
        var shelves = await _context.Shelves
            .OrderBy(s => s.Name)
            .ToListAsync();

        ViewBag.Categories = categories;
        ViewBag.Shelves = shelves;

        return View(books);
    }

    // POST: Books/UpdateInventory
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateInventory([FromBody] List<SOF1022_NHOM1_SD2003_Ver2.Models.ViewModels.InventoryUpdateModel> updates)
    {
        // Permission check
        if (!HttpContext.HasPermission("book.edit"))
        {
            return Json(new { success = false, message = "Bạn không có quyền thực hiện thao tác này." });
        }

        if (updates == null || !updates.Any())
        {
            return Json(new { success = false, message = "Không có dữ liệu để cập nhật." });
        }

        try
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            foreach (var update in updates)
            {
                var book = await _context.Books
                    .Include(b => b.Shelf)
                    .FirstOrDefaultAsync(b => b.Id == update.BookId);
                if (book != null)
                {
                    // Validate shelf capacity
                    if (book.Shelf != null)
                    {
                        int currentShelfQuantity = await _context.Books
                            .Where(b => b.ShelvesId == book.ShelvesId && b.Id != book.Id)
                            .SumAsync(b => b.Quantity);
                        int availableSpace = book.Shelf.Capacity - currentShelfQuantity;
                        
                        if (update.NewQuantity > availableSpace)
                        {
                            await transaction.RollbackAsync();
                            return Json(new { 
                                success = false, 
                                message = $"Kệ '{book.Shelf.Name}' đã vượt quá sức chứa tối đa ({book.Shelf.Capacity} cuốn). Hiện đã có {currentShelfQuantity} cuốn (không tính cuốn này), còn trống {availableSpace} chỗ, không thể có {update.NewQuantity} cuốn." 
                            });
                        }
                    }

                    // Validate quantity range (0-1000)
                    if (update.NewQuantity < 0 || update.NewQuantity > 1000)
                    {
                        await transaction.RollbackAsync();
                        return Json(new { 
                            success = false, 
                            message = $"Sách '{book.Title}' có số lượng không hợp lệ. Số lượng phải từ 0 đến 1,000 cuốn." 
                        });
                    }

                    book.AvailableQuantity = update.NewQuantity;
                    book.Quantity = update.NewQuantity;
                    
                    // Auto-update status to OutOfStock when quantity reaches 0
                    if (update.NewQuantity == 0 && book.BooksStatus == BooksStatus.Available)
                    {
                        book.BooksStatus = BooksStatus.OutOfStock;
                    }
                    // Auto-update status to Available when quantity > 0 and status was OutOfStock
                    else if (update.NewQuantity > 0 && book.BooksStatus == BooksStatus.OutOfStock)
                    {
                        book.BooksStatus = BooksStatus.Available;
                    }
                    
                    _context.Update(book);
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Json(new { success = true, message = $"Cập nhật kho sách thành công cho {updates.Count} cuốn sách!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật kho: " + ex.Message });
        }
    }

    // GET: Books/AdminDetails/5
    [SessionAuthorize]
    [PermissionAuthorize("book.view")]
    public async Task<IActionResult> AdminDetails(int? id)
    {
        // Role check - readers cannot access AdminDetails
        var role = HttpContext.Session.GetString("Role");
        if (role == "Reader" || string.IsNullOrEmpty(role))
        {
            return RedirectToAction("Index", "Home");
        }

        if (id == null)
        {
            return NotFound();
        }

        var book = await _context.Books
            .Include(b => b.Category)
            .Include(b => b.Author)
            .Include(b => b.Publisher)
            .Include(b => b.Shelf)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (book == null)
        {
            return NotFound();
        }

        return View(book);
    }

    // GET: Books/PrintLabel/5
    [SessionAuthorize]
    [PermissionAuthorize("book.view")]
    public async Task<IActionResult> PrintLabel(int? id)
    {
        if (id == null) return NotFound();

        var book = await _context.Books
            .Include(x => x.Category)
            .Include(x => x.Author)
            .Include(x => x.Publisher)
            .Include(x => x.Shelf)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (book == null) return NotFound();

        return View(book);
    }

    // GET: Books/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var book = await _context.Books
            .Include(x => x.Category)
            .Include(x => x.Author)
                .ThenInclude(a => a!.Books)
            .Include(x => x.Publisher)
            .Include(x => x.Shelf)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (book == null)
        {
            return NotFound();
        }

        // Track view history
        await TrackBookView(book.Id);

        // Get current user ID if logged in
        var userIdString = HttpContext.Session.GetString("UserId");
        int? readerId = null;
        if (!string.IsNullOrEmpty(userIdString) && int.TryParse(userIdString, out int parsedId))
        {
            readerId = parsedId;
        }

        // Check if user has favorited this book
        bool isFavorited = false;
        if (readerId.HasValue)
        {
            isFavorited = await _context.BookFavorites
                .AnyAsync(f => f.BookId == book.Id && f.ReaderId == readerId.Value);
        }

        // Check if user has borrowed this book (for rating validation)
        bool hasBorrowedBook = false;
        bool isCurrentlyBorrowing = false;
        DateTime? currentBorrowDueDate = null;
        if (readerId.HasValue)
        {
            hasBorrowedBook = await _context.BorrowDetails
                .AnyAsync(bd => bd.BookId == book.Id &&
                    bd.BorrowSlips.ReaderId == readerId.Value);

            // Check if user is currently borrowing this book (active slip with Borrowing status)
            var activeBorrowDetail = await _context.BorrowDetails
                .Include(bd => bd.BorrowSlips)
                .FirstOrDefaultAsync(bd => 
                    bd.BookId == book.Id && 
                    bd.BorrowSlips.ReaderId == readerId.Value &&
                    bd.BorrowSlips.BorrowStatus == BorrowStatus.Borrowing &&
                    bd.IsReturned == false);

            if (activeBorrowDetail != null)
            {
                isCurrentlyBorrowing = true;
                currentBorrowDueDate = activeBorrowDetail.BorrowSlips.DueDate;
            }
        }

        // Get comments
        var comments = await _context.BookComments
            .Include(c => c.Reader)
            .Where(c => c.BookId == book.Id && c.ParentCommentId == null)
            .OrderByDescending(c => c.CreatedAt)
            .Take(10)
            .ToListAsync();

        // Calculate average rating
        var ratings = await _context.BookRatings
            .Where(r => r.BookId == book.Id)
            .ToListAsync();
        decimal averageRating = ratings.Any() ? (decimal)ratings.Average(r => r.Rating) : 0;
        int reviewCount = ratings.Count;

        // Get favorite count
        int favoriteCount = await _context.BookFavorites
            .CountAsync(f => f.BookId == book.Id);

        // Get borrow count from BorrowDetails
        int borrowCount = await _context.BorrowDetails
            .CountAsync(bd => bd.BookId == book.Id);

        // Calculate real-time available stock including pending requests.
        // Tính cả ApprovedAwaitingPickup vì hệ thống tự động duyệt thẳng sang trạng thái này -
        // số lượng còn phải giảm ngay tại thời điểm đó, kho vật lý (AvailableQuantity) chỉ trừ
        // khi thủ thư xác nhận giao sách.
        int realAvailableCount = await BookAvailabilityHelper.GetRealAvailableCountAsync(_context, book.Id, book.Quantity);
        bool isAvailable = realAvailableCount > 0;

        // Get related books with bulletproof fallback
        // 1. Try to get books with same category OR same author OR same publisher
        var relatedBooks = await _context.Books
            .Include(b => b.Author)
            .Where(b => b.Id != book.Id &&
                       (b.CategoriesId == book.CategoriesId ||
                        b.AuthorsId == book.AuthorsId ||
                        b.PublishersId == book.PublishersId))
            .OrderByDescending(b => b.ViewCount)
            .Take(8)
            .ToListAsync();

        // 2. EMERGENCY FALLBACK: If no matches, just take top 8 newest books
        if (relatedBooks == null || !relatedBooks.Any())
        {
            relatedBooks = await _context.Books
                .Include(b => b.Author)
                .Where(b => b.Id != book.Id)
                .OrderByDescending(b => b.Id)
                .Take(8)
                .ToListAsync();
        }

        // Ensure we always return a list (even if empty)
        relatedBooks = relatedBooks ?? new List<Books>();

        var model = new BookDetailViewModel
        {
            Book = book,
            RelatedBooks = relatedBooks,
            Comments = comments,
            IsFavorited = isFavorited,
            AverageRating = averageRating,
            ReviewCount = reviewCount,
            FavoriteCount = favoriteCount,
            BorrowCount = borrowCount,
            HasBorrowedBook = hasBorrowedBook,
            IsCurrentlyBorrowing = isCurrentlyBorrowing,
            CurrentBorrowDueDate = currentBorrowDueDate,
            RealAvailableCount = realAvailableCount,
            IsAvailable = isAvailable
        };

        return View(model);
    }

    // POST: Books/Rate
    [HttpPost]
    public async Task<IActionResult> Rate(int bookId, int rating)
    {
        // Validate rating range
        if (rating < 1 || rating > 5)
        {
            return Json(new { success = false, message = "Đánh giá phải từ 1 đến 5 sao" });
        }

        // Get current user ID
        var userIdString = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int readerId))
        {
            return Json(new { success = false, message = "Vui lòng đăng nhập để đánh giá" });
        }

        // Check if user has borrowed this book
        var hasBorrowed = await _context.BorrowDetails
            .AnyAsync(bd => bd.BookId == bookId && bd.BorrowSlips.ReaderId == readerId);

        if (!hasBorrowed)
        {
            return Json(new { success = false, message = "Chỉ độc giả đã mượn cuốn sách này mới có thể gửi đánh giá" });
        }

        // Check if user already rated this book
        var existingRating = await _context.BookRatings
            .FirstOrDefaultAsync(r => r.BookId == bookId && r.ReaderId == readerId);

        if (existingRating != null)
        {
            // Update existing rating
            existingRating.Rating = rating;
            existingRating.CreatedAt = DateTime.Now;
            _context.BookRatings.Update(existingRating);
        }
        else
        {
            // Add new rating
            var newRating = new BookRating
            {
                BookId = bookId,
                ReaderId = readerId,
                Rating = rating,
                CreatedAt = DateTime.Now
            };
            _context.BookRatings.Add(newRating);
        }

        await _context.SaveChangesAsync();

        // Calculate new average rating
        var ratings = await _context.BookRatings
            .Where(r => r.BookId == bookId)
            .ToListAsync();
        decimal newAverage = ratings.Any() ? (decimal)ratings.Average(r => r.Rating) : 0;
        int newCount = ratings.Count;

        return Json(new
        {
            success = true,
            message = "Đánh giá của bạn đã được lưu",
            averageRating = newAverage.ToString("F1"),
            reviewCount = newCount
        });
    }

    private async Task TrackBookView(int bookId)
    {
        try
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            int? readerId = null;
            
            if (!string.IsNullOrEmpty(userIdString) && int.TryParse(userIdString, out int parsedId))
            {
                readerId = parsedId;
            }

            var sessionId = HttpContext.Session.Id;

            // Check if user has already viewed this book (to prevent spam)
            bool shouldIncrementCount = false;
            
            if (readerId.HasValue)
            {
                // For logged-in users: check if they've viewed this book before
                var existingView = await _context.BookViewHistories
                    .AnyAsync(v => v.BookId == bookId && v.ReaderId == readerId.Value);
                
                if (!existingView)
                {
                    shouldIncrementCount = true;
                }
            }
            else
            {
                // For anonymous users: check if this session has viewed this book before
                var existingView = await _context.BookViewHistories
                    .AnyAsync(v => v.BookId == bookId && v.SessionId == sessionId && v.ReaderId == null);
                
                if (!existingView)
                {
                    shouldIncrementCount = true;
                }
            }

            // Always record the view history for analytics
            var viewHistory = new BookViewHistory
            {
                BookId = bookId,
                ReaderId = readerId,
                SessionId = sessionId,
                ViewedAt = DateTime.Now
            };

            _context.BookViewHistories.Add(viewHistory);

            // Only increment book view count if this is the first view by this user/session
            if (shouldIncrementCount)
            {
                var book = await _context.Books.FindAsync(bookId);
                if (book != null)
                {
                    book.ViewCount++;
                    _context.Books.Update(book);
                }
            }

            await _context.SaveChangesAsync();
        }
        catch
        {
            // Silently fail to not break the page
        }
    }

    // GET: Books/Create
    [PermissionAuthorize("book.create")]
    public async Task<IActionResult> Create()
    {
        ViewData["CategoriesId"] = new SelectList(_context.Categories, "Id", "Name");
        ViewData["AuthorsId"] = new SelectList(_context.Authors, "Id", "Name");
        ViewData["PublishersId"] = new SelectList(_context.Publishers, "Id", "Name");
        
        // Order shelves by available capacity (most space first)
        var shelves = await _context.Shelves
            .Include(s => s.Books)
            .ToListAsync();

        var shelfSelectList = shelves.Select(s => new
        {
            s.Id,
            DisplayName = $"{s.Name} - Sức chứa còn lại: {s.Capacity - s.Books.Where(b => !b.IsDeleted).Sum(b => b.Quantity)}/{s.Capacity}"
        }).OrderByDescending(s => s.DisplayName).ToList();

        ViewData["ShelvesId"] = new SelectList(shelfSelectList, "Id", "DisplayName");

        return View();
    }

    // POST: Books/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [PermissionAuthorize("book.create")]
    public async Task<IActionResult> Create(Books book, IFormFile? CoverImageFile)
    {
        bool exists = await _context.Books
            .AnyAsync(x => x.ISBN == book.ISBN);

        if (exists)
        {
            ModelState.AddModelError("ISBN", "ISBN đã tồn tại.");
        }

        // Validate foreign keys
        bool categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == book.CategoriesId);
        if (!categoryExists)
        {
            ModelState.AddModelError("CategoriesId", "Danh mục không tồn tại.");
        }

        bool authorExists = await _context.Authors
            .AnyAsync(a => a.Id == book.AuthorsId);
        if (!authorExists)
        {
            ModelState.AddModelError("AuthorsId", "Tác giả không tồn tại.");
        }

        bool publisherExists = await _context.Publishers
            .AnyAsync(p => p.Id == book.PublishersId);
        if (!publisherExists)
        {
            ModelState.AddModelError("PublishersId", "Nhà xuất bản không tồn tại.");
        }

        // Validate shelf capacity
        if (book.ShelvesId > 0)
        {
            var shelf = await _context.Shelves.FindAsync(book.ShelvesId);
            if (shelf != null)
            {
                int currentQuantity = await _context.Books
                    .Where(b => b.ShelvesId == book.ShelvesId)
                    .SumAsync(b => b.Quantity);
                int availableSpace = shelf.Capacity - currentQuantity;
                
                if (book.Quantity > availableSpace)
                {
                    ModelState.AddModelError("ShelvesId",
                        $"Kệ sách '{shelf.Name}' đã vượt quá sức chứa tối đa ({shelf.Capacity} cuốn). Hiện đã có {currentQuantity} cuốn, còn trống {availableSpace} chỗ, không thể thêm {book.Quantity} cuốn.");
                }
            }
        }

        if (ModelState.IsValid)
        {
            try
            {
                // Handle file upload
                if (CoverImageFile != null && CoverImageFile.Length > 0)
                {
                    // Validate file size (5MB max)
                    if (CoverImageFile.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("CoverImageFile", "Kích thước file không được vượt quá 5MB.");
                        ViewData["CategoriesId"] = new SelectList(_context.Categories, "Id", "Name", book.CategoriesId);
                        ViewData["AuthorsId"] = new SelectList(_context.Authors, "Id", "Name", book.AuthorsId);
                        ViewData["PublishersId"] = new SelectList(_context.Publishers, "Id", "Name", book.PublishersId);
                        
                        var shelvesValidation = await _context.Shelves
                            .Include(s => s.Books)
                            .ToListAsync();

                        var shelfSelectListValidation = shelvesValidation.Select(s => new
                        {
                            s.Id,
                            DisplayName = $"{s.Name} - Sức chứa còn lại: {s.Capacity - s.Books.Where(b => !b.IsDeleted).Sum(b => b.Quantity)}/{s.Capacity}"
                        }).OrderByDescending(s => s.DisplayName).ToList();

                        ViewData["ShelvesId"] = new SelectList(shelfSelectListValidation, "Id", "DisplayName", book.ShelvesId);
                        return View(book);
                    }

                    // Validate file type
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    var fileExtension = Path.GetExtension(CoverImageFile.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("CoverImageFile", "Chỉ chấp nhận các định dạng ảnh: JPG, PNG, GIF, WEBP.");
                        ViewData["CategoriesId"] = new SelectList(_context.Categories, "Id", "Name", book.CategoriesId);
                        ViewData["AuthorsId"] = new SelectList(_context.Authors, "Id", "Name", book.AuthorsId);
                        ViewData["PublishersId"] = new SelectList(_context.Publishers, "Id", "Name", book.PublishersId);
                        
                        var shelvesValidation = await _context.Shelves
                            .Include(s => s.Books)
                            .ToListAsync();

                        var shelfSelectListValidation = shelvesValidation.Select(s => new
                        {
                            s.Id,
                            DisplayName = $"{s.Name} - Sức chứa còn lại: {s.Capacity - s.Books.Where(b => !b.IsDeleted).Sum(b => b.Quantity)}/{s.Capacity}"
                        }).OrderByDescending(s => s.DisplayName).ToList();

                        ViewData["ShelvesId"] = new SelectList(shelfSelectListValidation, "Id", "DisplayName", book.ShelvesId);
                        return View(book);
                    }

                    // Generate unique filename
                    var fileName = $"{Guid.NewGuid()}{fileExtension}";
                    var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "books");
                    
                    // Ensure directory exists
                    if (!Directory.Exists(uploadsPath))
                    {
                        Directory.CreateDirectory(uploadsPath);
                    }

                    // Save file
                    var filePath = Path.Combine(uploadsPath, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await CoverImageFile.CopyToAsync(stream);
                    }

                    // Set cover image path
                    book.CoverImage = $"/images/books/{fileName}";
                }

                book.AvailableQuantity = book.Quantity;

                _context.Books.Add(book);

                await _context.SaveChangesAsync();

                TempData["Success"] = "Thêm sách thành công.";

                return RedirectToAction(nameof(AdminIndex));
            }
            catch
            {
                TempData["Error"] = "Có lỗi xảy ra.";
            }
        }

        ViewData["CategoriesId"] = new SelectList(_context.Categories, "Id", "Name", book.CategoriesId);
        ViewData["AuthorsId"] = new SelectList(_context.Authors, "Id", "Name", book.AuthorsId);
        ViewData["PublishersId"] = new SelectList(_context.Publishers, "Id", "Name", book.PublishersId);
        
        // Order shelves by available capacity (most space first)
        var shelves = await _context.Shelves
            .Include(s => s.Books)
            .ToListAsync();

        var shelfSelectList = shelves.Select(s => new
        {
            s.Id,
            DisplayName = $"{s.Name} - Sức chứa còn lại: {s.Capacity - s.Books.Where(b => !b.IsDeleted).Sum(b => b.Quantity)}/{s.Capacity}"
        }).OrderByDescending(s => s.DisplayName).ToList();

        ViewData["ShelvesId"] = new SelectList(shelfSelectList, "Id", "DisplayName", book.ShelvesId);

        return View(book);
    }

    // GET: Books/Edit/5
    [PermissionAuthorize("book.edit")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var book = await _context.Books.FindAsync(id);

        if (book == null)
        {
            return NotFound();
        }

        ViewData["CategoriesId"] = new SelectList(_context.Categories, "Id", "Name", book.CategoriesId);
        ViewData["AuthorsId"] = new SelectList(_context.Authors, "Id", "Name", book.AuthorsId);
        ViewData["PublishersId"] = new SelectList(_context.Publishers, "Id", "Name", book.PublishersId);
        
        // Order shelves by available capacity (most space first)
        var shelves = await _context.Shelves
            .Include(s => s.Books)
            .ToListAsync();

        var shelfSelectList = shelves.Select(s => new
        {
            s.Id,
            DisplayName = $"{s.Name} - Sức chứa còn lại: {s.Capacity - s.Books.Where(b => !b.IsDeleted).Sum(b => b.Quantity)}/{s.Capacity}"
        }).OrderByDescending(s => s.DisplayName).ToList();

        ViewData["ShelvesId"] = new SelectList(shelfSelectList, "Id", "DisplayName", book.ShelvesId);

        return View(book);
    }

    // POST: Books/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [PermissionAuthorize("book.edit")]
    public async Task<IActionResult> Edit(int id, Books book, IFormFile? CoverImageFile)
    {
        if (id != book.Id)
        {
            return NotFound();
        }

        bool existsISBN = await _context.Books
            .AnyAsync(x => x.ISBN == book.ISBN && x.Id != book.Id);

        if (existsISBN)
        {
            ModelState.AddModelError("ISBN", "ISBN đã tồn tại.");
        }

        // Validate foreign keys
        bool categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == book.CategoriesId);
        if (!categoryExists)
        {
            ModelState.AddModelError("CategoriesId", "Danh mục không tồn tại.");
        }

        bool authorExists = await _context.Authors
            .AnyAsync(a => a.Id == book.AuthorsId);
        if (!authorExists)
        {
            ModelState.AddModelError("AuthorsId", "Tác giả không tồn tại.");
        }

        bool publisherExists = await _context.Publishers
            .AnyAsync(p => p.Id == book.PublishersId);
        if (!publisherExists)
        {
            ModelState.AddModelError("PublishersId", "Nhà xuất bản không tồn tại.");
        }

        // Validate shelf capacity
        if (book.ShelvesId > 0)
        {
            var shelf = await _context.Shelves.FindAsync(book.ShelvesId);
            if (shelf != null)
            {
                int currentQuantity = await _context.Books
                    .Where(b => b.ShelvesId == book.ShelvesId && b.Id != id)
                    .SumAsync(b => b.Quantity);
                int availableSpace = shelf.Capacity - currentQuantity;
                
                if (book.Quantity > availableSpace)
                {
                    ModelState.AddModelError("ShelvesId",
                        $"Kệ sách '{shelf.Name}' đã vượt quá sức chứa tối đa ({shelf.Capacity} cuốn). Hiện đã có {currentQuantity} cuốn (không tính cuốn này), còn trống {availableSpace} chỗ, không thể có {book.Quantity} cuốn.");
                }
            }
        }

        if (ModelState.IsValid)
        {
            try
            {
                var oldBook = await _context.Books
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (oldBook == null)
                {
                    return NotFound();
                }

                // Handle file upload
                if (CoverImageFile != null && CoverImageFile.Length > 0)
                {
                    // Validate file size (5MB max)
                    if (CoverImageFile.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("CoverImageFile", "Kích thước file không được vượt quá 5MB.");
                        ViewData["CategoriesId"] = new SelectList(_context.Categories, "Id", "Name", book.CategoriesId);
                        ViewData["AuthorsId"] = new SelectList(_context.Authors, "Id", "Name", book.AuthorsId);
                        ViewData["PublishersId"] = new SelectList(_context.Publishers, "Id", "Name", book.PublishersId);
                        
                        var shelvesValidation = await _context.Shelves
                            .Include(s => s.Books)
                            .ToListAsync();

                        var shelfSelectListValidation = shelvesValidation.Select(s => new
                        {
                            s.Id,
                            DisplayName = $"{s.Name} - Sức chứa còn lại: {s.Capacity - s.Books.Where(b => !b.IsDeleted).Sum(b => b.Quantity)}/{s.Capacity}"
                        }).OrderByDescending(s => s.DisplayName).ToList();

                        ViewData["ShelvesId"] = new SelectList(shelfSelectListValidation, "Id", "DisplayName", book.ShelvesId);
                        return View(book);
                    }

                    // Validate file type
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    var fileExtension = Path.GetExtension(CoverImageFile.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("CoverImageFile", "Chỉ chấp nhận các định dạng ảnh: JPG, PNG, GIF, WEBP.");
                        ViewData["CategoriesId"] = new SelectList(_context.Categories, "Id", "Name", book.CategoriesId);
                        ViewData["AuthorsId"] = new SelectList(_context.Authors, "Id", "Name", book.AuthorsId);
                        ViewData["PublishersId"] = new SelectList(_context.Publishers, "Id", "Name", book.PublishersId);
                        
                        var shelvesValidation = await _context.Shelves
                            .Include(s => s.Books)
                            .ToListAsync();

                        var shelfSelectListValidation = shelvesValidation.Select(s => new
                        {
                            s.Id,
                            DisplayName = $"{s.Name} - Sức chứa còn lại: {s.Capacity - s.Books.Where(b => !b.IsDeleted).Sum(b => b.Quantity)}/{s.Capacity}"
                        }).OrderByDescending(s => s.DisplayName).ToList();

                        ViewData["ShelvesId"] = new SelectList(shelfSelectListValidation, "Id", "DisplayName", book.ShelvesId);
                        return View(book);
                    }

                    // Delete old image file if exists
                    if (!string.IsNullOrEmpty(oldBook.CoverImage) && oldBook.CoverImage.StartsWith("/images/books/"))
                    {
                        var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", oldBook.CoverImage.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Generate unique filename
                    var fileName = $"{Guid.NewGuid()}{fileExtension}";
                    var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "books");
                    
                    // Ensure directory exists
                    if (!Directory.Exists(uploadsPath))
                    {
                        Directory.CreateDirectory(uploadsPath);
                    }

                    // Save file
                    var filePath = Path.Combine(uploadsPath, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await CoverImageFile.CopyToAsync(stream);
                    }

                    // Set cover image path
                    book.CoverImage = $"/images/books/{fileName}";
                }
                else
                {
                    // Keep old cover image if no new file uploaded
                    book.CoverImage = oldBook.CoverImage;
                }

                int borrowedBooks =
                    oldBook.Quantity - oldBook.AvailableQuantity;

                book.AvailableQuantity =
                    book.Quantity - borrowedBooks;

                if (book.AvailableQuantity < 0)
                {
                    book.AvailableQuantity = 0;
                }

                // Auto-update status based on quantity
                if (book.AvailableQuantity == 0 && book.BooksStatus == BooksStatus.Available)
                {
                    book.BooksStatus = BooksStatus.OutOfStock;
                }
                else if (book.AvailableQuantity > 0 && book.BooksStatus == BooksStatus.OutOfStock)
                {
                    book.BooksStatus = BooksStatus.Available;
                }

                _context.Update(book);

                await _context.SaveChangesAsync();

                TempData["Success"] = "Cập nhật sách thành công.";

                return RedirectToAction(nameof(AdminIndex));
            }
            catch
            {
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật.";
            }
        }

        ViewData["CategoriesId"] = new SelectList(_context.Categories, "Id", "Name", book.CategoriesId);
        ViewData["AuthorsId"] = new SelectList(_context.Authors, "Id", "Name", book.AuthorsId);
        ViewData["PublishersId"] = new SelectList(_context.Publishers, "Id", "Name", book.PublishersId);
        
        // Order shelves by available capacity (most space first)
        var shelves = await _context.Shelves
            .Include(s => s.Books)
            .ToListAsync();

        var shelfSelectList = shelves.Select(s => new
        {
            s.Id,
            DisplayName = $"{s.Name} - Sức chứa còn lại: {s.Capacity - s.Books.Where(b => !b.IsDeleted).Sum(b => b.Quantity)}/{s.Capacity}"
        }).OrderByDescending(s => s.DisplayName).ToList();

        ViewData["ShelvesId"] = new SelectList(shelfSelectList, "Id", "DisplayName", book.ShelvesId);

        return View(book);
    }

    // GET: Books/Delete/5
    [PermissionAuthorize("book.delete")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var book = await _context.Books
            .Include(b => b.Category)
            .Include(b => b.Author)
            .Include(b => b.Publisher)
            .Include(b => b.Shelf)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (book == null)
        {
            return NotFound();
        }

        return View(book);
    }

    // POST: Books/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [PermissionAuthorize("book.delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var book = await _context.Books
            .Include(b => b.BorrowDetails)
            .Include(b => b.Reservations)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book == null)
        {
            return NotFound();
        }

        // Soft delete logic: If book is active, change to Discontinued. If already Discontinued, permanently delete.
        if (book.BooksStatus == BooksStatus.Discontinued)
        {
            // Check for dependencies before permanent delete
            if (book.BorrowDetails != null && book.BorrowDetails.Any())
            {
                TempData["Error"] = "Không thể xóa vĩnh viễn cuốn sách này vì sách đang nằm trong phiếu mượn chưa trả hoặc có lịch sử liên quan. Vui lòng xử lý các phiếu mượn trước!";
                return RedirectToAction(nameof(AdminIndex));
            }

            if (book.Reservations != null && book.Reservations.Any())
            {
                TempData["Error"] = "Không thể xóa vĩnh viễn cuốn sách này vì có độc giả đang đặt trước sách này. Vui lòng hủy các đặt chỗ trước khi xóa!";
                return RedirectToAction(nameof(AdminIndex));
            }

            try
            {
                // Permanent delete
                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa vĩnh viễn sách thành công.";
            }
            catch
            {
                TempData["Error"] = "Có lỗi xảy ra khi xóa dữ liệu.";
            }
        }
        else
        {
            // Soft delete: Change status to Discontinued
            try
            {
                book.BooksStatus = BooksStatus.Discontinued;
                _context.Update(book);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã chuyển trạng thái sách sang 'Ngừng kinh doanh'.";
            }
            catch
            {
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật trạng thái.";
            }
        }

        return RedirectToAction(nameof(AdminIndex));
    }

    private async Task<bool> BooksExists(int id)
    {
        return await _context.Books.AnyAsync(e => e.Id == id);
    }

    // POST: Books/UpdateStatus
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int bookId, BooksStatus newStatus)
    {
        // Permission check
        if (!HttpContext.HasPermission("book.edit"))
        {
            return Json(new { success = false, message = "Bạn không có quyền thực hiện thao tác này." });
        }

        var book = await _context.Books.FindAsync(bookId);
        if (book == null)
        {
            return Json(new { success = false, message = "Không tìm thấy sách." });
        }

        try
        {
            book.BooksStatus = newStatus;
            
            // Auto-update AvailableQuantity based on status
            if (newStatus == BooksStatus.OutOfStock)
            {
                book.AvailableQuantity = 0;
            }
            else if (newStatus == BooksStatus.Available && book.AvailableQuantity == 0 && book.Quantity > 0)
            {
                book.AvailableQuantity = book.Quantity;
            }

            _context.Update(book);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Cập nhật trạng thái thành công." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
        }
    }

    public async Task<IActionResult> Category(int id)
    {
        var books = await _context.Books
            .Include(x => x.Author)
            .Include(x => x.Category)
            .Where(x => x.CategoriesId == id &&
                        x.BooksStatus != BooksStatus.Hidden)
            .ToListAsync();

        return View("Index", books);
    }

    public async Task<IActionResult> Author(int id)
    {
        var books = await _context.Books
            .Include(x => x.Author)
            .Include(x => x.Category)
            .Where(x => x.AuthorsId == id &&
                        x.BooksStatus != BooksStatus.Hidden)
            .ToListAsync();

        return View("Index", books);
    }

    [HttpGet]
    public async Task<IActionResult> Search(string keyword)
    {
        ViewBag.Keyword = keyword;

        if (string.IsNullOrWhiteSpace(keyword))
        {
            return View(new List<Books>());
        }

        var books = await _context.Books
            .Include(x => x.Author)
            .Include(x => x.Category)
            .Include(x => x.Publisher)
            .Where(x =>
                x.BooksStatus != BooksStatus.Hidden
                &&
                (
                    (x.Title != null && EF.Functions.Like(x.Title, $"%{keyword}%")) ||

                    (x.Description != null && EF.Functions.Like(x.Description, $"%{keyword}%")) ||

                    (x.Keywords != null && EF.Functions.Like(x.Keywords, $"%{keyword}%")) ||

                    (x.ISBN != null && EF.Functions.Like(x.ISBN, $"%{keyword}%")) ||

                    (x.Author != null &&
                     x.Author.Name != null &&
                     EF.Functions.Like(x.Author.Name, $"%{keyword}%")) ||

                    (x.Publisher != null &&
                     x.Publisher.Name != null &&
                     EF.Functions.Like(x.Publisher.Name, $"%{keyword}%"))
                )
            )
            .ToListAsync();

        return View(books);
    }

    // ===== API ENDPOINTS FOR BOOK INTERACTIONS =====

    // POST: Books/ToggleFavorite
    [HttpPost]
    [SessionAuthorize]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> ToggleFavorite(int bookId)
    {
        var userIdString = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdString))
        {
            return Json(new { success = false, message = "Vui lòng đăng nhập" });
        }

        int readerId = int.Parse(userIdString);

        var existingFavorite = await _context.BookFavorites
            .FirstOrDefaultAsync(f => f.BookId == bookId && f.ReaderId == readerId);

        if (existingFavorite != null)
        {
            _context.BookFavorites.Remove(existingFavorite);
            await _context.SaveChangesAsync();
            return Json(new { success = true, isFavorite = false, message = "Đã xóa khỏi yêu thích" });
        }
        else
        {
            var favorite = new BookFavorite
            {
                BookId = bookId,
                ReaderId = readerId,
                CreatedAt = DateTime.Now
            };
            _context.BookFavorites.Add(favorite);
            await _context.SaveChangesAsync();
            return Json(new { success = true, isFavorite = true, message = "Đã thêm vào yêu thích" });
        }
    }

    // GET: Books/IsFavorite
    [HttpGet]
    [SessionAuthorize]
    public async Task<IActionResult> IsFavorite(int bookId)
    {
        var userIdString = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdString))
        {
            return Json(new { isFavorite = false });
        }

        int readerId = int.Parse(userIdString);

        var isFavorite = await _context.BookFavorites
            .AnyAsync(f => f.BookId == bookId && f.ReaderId == readerId);

        return Json(new { isFavorite });
    }

    // POST: Books/AddComment
    [HttpPost]
    [SessionAuthorize]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> AddComment(int bookId, string content, int? parentCommentId = null)
    {
        var userIdString = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdString))
        {
            return Json(new { success = false, message = "Vui lòng đăng nhập" });
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            return Json(new { success = false, message = "Nội dung bình luận không được để trống" });
        }

        int readerId = int.Parse(userIdString);

        var comment = new BookComment
        {
            BookId = bookId,
            ReaderId = readerId,
            Content = content,
            ParentCommentId = parentCommentId,
            CreatedAt = DateTime.Now,
            Likes = 0
        };

        _context.BookComments.Add(comment);
        await _context.SaveChangesAsync();

        // Get reader info for response
        var reader = await _context.Readers.FindAsync(readerId);

        return Json(new
        {
            success = true,
            message = "Bình luận thành công",
            comment = new
            {
                id = comment.Id,
                content = comment.Content,
                author = reader?.FullName ?? "Người dùng",
                avatar = reader?.FullName?.Substring(0, 1).ToUpper() ?? "U",
                date = comment.CreatedAt.ToString("dd/MM/yyyy"),
                likes = 0
            }
        });
    }

    // GET: Books/GetComments
    [HttpGet]
    public async Task<IActionResult> GetComments(int bookId)
    {
        var comments = await _context.BookComments
            .Include(c => c.Reader)
            .Include(c => c.Replies)
            .Where(c => c.BookId == bookId && c.ParentCommentId == null)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new
            {
                id = c.Id,
                content = c.Content,
                author = c.Reader.FullName,
                avatar = c.Reader.FullName != null ? c.Reader.FullName.Substring(0, 1).ToUpper() : "U",
                date = c.CreatedAt.ToString("dd/MM/yyyy"),
                likes = c.Likes,
                replyCount = c.Replies != null ? c.Replies.Count : 0
            })
            .ToListAsync();

        return Json(new { success = true, comments });
    }

    // POST: Books/LikeComment
    [HttpPost]
    [SessionAuthorize]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> LikeComment(int commentId)
    {
        var userIdString = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdString))
        {
            return Json(new { success = false, message = "Vui lòng đăng nhập" });
        }

        var comment = await _context.BookComments.FindAsync(commentId);
        if (comment == null)
        {
            return Json(new { success = false, message = "Không tìm thấy bình luận" });
        }

        comment.Likes++;
        await _context.SaveChangesAsync();

        return Json(new { success = true, likes = comment.Likes });
    }

    // POST: Books/AddRating
    [HttpPost]
    [SessionAuthorize]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> AddRating(int bookId, int rating, string? review = null)
    {
        var userIdString = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdString))
        {
            return Json(new { success = false, message = "Vui lòng đăng nhập" });
        }

        if (rating < 1 || rating > 5)
        {
            return Json(new { success = false, message = "Đánh giá phải từ 1 đến 5 sao" });
        }

        int readerId = int.Parse(userIdString);

        // Check if user already rated this book
        var existingRating = await _context.BookRatings
            .FirstOrDefaultAsync(r => r.BookId == bookId && r.ReaderId == readerId);

        if (existingRating != null)
        {
            existingRating.Rating = rating;
            existingRating.Review = review;
            existingRating.CreatedAt = DateTime.Now;
            _context.BookRatings.Update(existingRating);
        }
        else
        {
            var newRating = new BookRating
            {
                BookId = bookId,
                ReaderId = readerId,
                Rating = rating,
                Review = review,
                CreatedAt = DateTime.Now
            };
            _context.BookRatings.Add(newRating);
        }

        await _context.SaveChangesAsync();

        // Calculate new average rating
        var ratings = await _context.BookRatings
            .Where(r => r.BookId == bookId)
            .ToListAsync();

        var averageRating = ratings.Any() ? ratings.Average(r => r.Rating) : 0;
        var totalRatings = ratings.Count;

        return Json(new
        {
            success = true,
            message = "Đánh giá thành công",
            averageRating = averageRating.ToString("F1"),
            totalRatings = totalRatings
        });
    }

    // GET: Books/GetRatingStats
    [HttpGet]
    public async Task<IActionResult> GetRatingStats(int bookId)
    {
        var ratings = await _context.BookRatings
            .Where(r => r.BookId == bookId)
            .ToListAsync();

        if (!ratings.Any())
        {
            return Json(new
            {
                success = true,
                averageRating = 0,
                totalRatings = 0,
                distribution = new { five = 0, four = 0, three = 0, two = 0, one = 0 }
            });
        }

        var averageRating = ratings.Average(r => r.Rating);
        var totalRatings = ratings.Count;

        var distribution = new
        {
            five = ratings.Count(r => r.Rating == 5),
            four = ratings.Count(r => r.Rating == 4),
            three = ratings.Count(r => r.Rating == 3),
            two = ratings.Count(r => r.Rating == 2),
            one = ratings.Count(r => r.Rating == 1)
        };

        return Json(new
        {
            success = true,
            averageRating = averageRating.ToString("F1"),
            totalRatings = totalRatings,
            distribution
        });
    }

    // GET: Books/GetUserRating
    [HttpGet]
    [SessionAuthorize]
    public async Task<IActionResult> GetUserRating(int bookId)
    {
        var userIdString = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdString))
        {
            return Json(new { rating = 0 });
        }

        int readerId = int.Parse(userIdString);

        var rating = await _context.BookRatings
            .Where(r => r.BookId == bookId && r.ReaderId == readerId)
            .Select(r => r.Rating)
            .FirstOrDefaultAsync();

        return Json(new { rating });
    }

    // GET: Books/Random
    public async Task<IActionResult> Random()
    {
        var randomBook = await _context.Books
            .Where(x => x.BooksStatus == BooksStatus.Available && !x.IsDeleted)
            .OrderBy(x => Guid.NewGuid())
            .FirstOrDefaultAsync();

        if (randomBook == null)
        {
            TempData["Error"] = "Không tìm thấy sách nào trong hệ thống.";
            return RedirectToAction("Index");
        }

        // Set success message for the random book
        TempData["Success"] = "Đây là cuốn sách ngẫu nhiên dành riêng cho bạn!";
        
        return RedirectToAction("Details", new { id = randomBook.Id });
    }

    // POST: Books/CreateReservation
    [HttpPost]
    [SessionAuthorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateReservation(int bookId)
    {
        var userIdString = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdString))
        {
            return Json(new { success = false, message = "Vui lòng đăng nhập" });
        }

        int readerId = int.Parse(userIdString);

        // Check if book exists
        var book = await _context.Books.FindAsync(bookId);
        if (book == null)
        {
            return Json(new { success = false, message = "Không tìm thấy sách" });
        }

        // Check if user already has a reservation for this book
        var existingReservation = await _context.Reservations
            .FirstOrDefaultAsync(r => r.BookId == bookId && r.ReaderId == readerId && 
                                     r.BooksStatus == ReservationsStatus.Waiting);
        
        if (existingReservation != null)
        {
            return Json(new { success = false, message = "Bạn đã đặt trước sách này rồi" });
        }

        // Create reservation
        var reservation = new Reservations
        {
            ReaderId = readerId,
            BookId = bookId,
            ReservationDate = DateTime.Now,
            ExpirationDate = DateTime.Now.AddDays(3),
            BooksStatus = ReservationsStatus.Waiting,
            Status = "Waiting"
        };

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        return Json(new { success = true, message = "Đặt trước thành công", reservationId = reservation.Id });
    }

    [HttpGet]
    [SessionAuthorize]
    public async Task<IActionResult> ExportToExcel()
    {
        var books = await _context.Books
            .Include(b => b.Category)
            .Include(b => b.Author)
            .Include(b => b.Publisher)
            .Include(b => b.Shelf)
            .OrderBy(b => b.Id)
            .ToListAsync();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("DanhSachSach");

        worksheet.Cell(1, 1).Value = "DANH MỤC SÁCH THƯ VIỆN ĐIỆN TỬ";
        worksheet.Range(1, 1, 1, 11).Merge().Style
            .Font.SetBold(true)
            .Font.SetFontSize(16)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        string[] headers = new[] {
            "STT", "Mã sách", "Tên sách", "Tác giả", "Thể loại",
            "Nhà xuất bản", "ISBN", "Năm XB", "Số lượng", "Giá tiền (VNĐ)", "Vị trí kệ"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(3, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromArgb(46, 204, 113);
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        int row = 4;
        for (int i = 0; i < books.Count; i++)
        {
            var b = books[i];
            worksheet.Cell(row, 1).Value = i + 1;
            worksheet.Cell(row, 2).Value = b.Id;
            worksheet.Cell(row, 3).Value = b.Title;
            worksheet.Cell(row, 4).Value = b.Author?.Name ?? "";
            worksheet.Cell(row, 5).Value = b.Category?.Name ?? "";
            worksheet.Cell(row, 6).Value = b.Publisher?.Name ?? "";
            worksheet.Cell(row, 7).Value = b.ISBN ?? "";
            worksheet.Cell(row, 8).Value = b.PublishYear;
            worksheet.Cell(row, 9).Value = b.Quantity;
            worksheet.Cell(row, 10).Value = b.Price;
            worksheet.Cell(row, 11).Value = b.Shelf?.Name ?? "";
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"DanhMucSach_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
    }

    [HttpGet]
    [SessionAuthorize]
    public IActionResult DownloadImportTemplate()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("MauNhapSach");

        string[] headers = new[] {
            "Tên sách (*)", "Tác giả (*)", "Thể loại (*)",
            "Nhà xuất bản", "ISBN", "Năm xuất bản", "Số lượng (*)", "Đơn giá (VNĐ)", "Mô tả"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromArgb(52, 152, 219);
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        // Sample rows
        worksheet.Cell(2, 1).Value = "Lập trình C# cơ bản và nâng cao";
        worksheet.Cell(2, 2).Value = "Phạm Văn Đồng";
        worksheet.Cell(2, 3).Value = "Công nghệ thông tin";
        worksheet.Cell(2, 4).Value = "NXB Giáo Dục";
        worksheet.Cell(2, 5).Value = "978-604-0-12345-6";
        worksheet.Cell(2, 6).Value = 2024;
        worksheet.Cell(2, 7).Value = 20;
        worksheet.Cell(2, 8).Value = 125000;
        worksheet.Cell(2, 9).Value = "Giáo trình nhập môn C#";

        worksheet.Cell(3, 1).Value = "Đắc nhân tâm";
        worksheet.Cell(3, 2).Value = "Dale Carnegie";
        worksheet.Cell(3, 3).Value = "Kỹ năng sống";
        worksheet.Cell(3, 4).Value = "NXB Trẻ";
        worksheet.Cell(3, 5).Value = "978-604-1-67890-1";
        worksheet.Cell(3, 6).Value = 2023;
        worksheet.Cell(3, 7).Value = 15;
        worksheet.Cell(3, 8).Value = 95000;
        worksheet.Cell(3, 9).Value = "Sách nghệ thuật ứng xử";

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "MauNhapSach_Template.xlsx");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [SessionAuthorize]
    public async Task<IActionResult> ImportFromExcel(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["ErrorMessage"] = "Vui lòng chọn file Excel để nhập dữ liệu.";
            return RedirectToAction(nameof(Index));
        }

        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = "Chỉ chấp nhận định dạng file .xlsx.";
            return RedirectToAction(nameof(Index));
        }

        int successCount = 0;
        int errorCount = 0;

        try
        {
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);
            var rows = worksheet.RangeUsed()?.RowsUsed()?.Skip(1);

            if (rows == null)
            {
                TempData["ErrorMessage"] = "File Excel không có dữ liệu.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var row in rows)
            {
                try
                {
                    var title = row.Cell(1).GetString()?.Trim();
                    var authorName = row.Cell(2).GetString()?.Trim();
                    var categoryName = row.Cell(3).GetString()?.Trim();
                    var publisherName = row.Cell(4).GetString()?.Trim();
                    var isbn = row.Cell(5).GetString()?.Trim();
                    int year = row.Cell(6).TryGetValue(out int y) ? y : DateTime.Now.Year;
                    int quantity = row.Cell(7).TryGetValue(out int q) ? q : 1;
                    decimal price = row.Cell(8).TryGetValue(out decimal p) ? p : 50000m;
                    var description = row.Cell(9).GetString()?.Trim();

                    if (string.IsNullOrWhiteSpace(title))
                    {
                        errorCount++;
                        continue;
                    }

                    Authors? author = null;
                    if (!string.IsNullOrWhiteSpace(authorName))
                    {
                        author = await _context.Authors.FirstOrDefaultAsync(a => a.Name.ToLower() == authorName.ToLower());
                        if (author == null)
                        {
                            author = new Authors { Name = authorName, Biography = "Nhập tự động từ Excel" };
                            _context.Authors.Add(author);
                            await _context.SaveChangesAsync();
                        }
                    }

                    Categories? category = null;
                    if (!string.IsNullOrWhiteSpace(categoryName))
                    {
                        category = await _context.Categories.FirstOrDefaultAsync(c => c.Name.ToLower() == categoryName.ToLower());
                        if (category == null)
                        {
                            category = new Categories { Name = categoryName, Description = "Nhập tự động từ Excel" };
                            _context.Categories.Add(category);
                            await _context.SaveChangesAsync();
                        }
                    }

                    Publishers? publisher = null;
                    if (!string.IsNullOrWhiteSpace(publisherName))
                    {
                        publisher = await _context.Publishers.FirstOrDefaultAsync(p => p.Name.ToLower() == publisherName.ToLower());
                        if (publisher == null)
                        {
                            publisher = new Publishers { Name = publisherName, Address = "Chưa cập nhật" };
                            _context.Publishers.Add(publisher);
                            await _context.SaveChangesAsync();
                        }
                    }

                    var newBook = new Books
                    {
                        Title = title,
                        AuthorsId = author?.Id ?? 1,
                        CategoriesId = category?.Id ?? 1,
                        PublishersId = publisher?.Id ?? 1,
                        ShelvesId = 1,
                        ISBN = string.IsNullOrWhiteSpace(isbn) ? $"ISBN-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}" : isbn,
                        PublishYear = year > 0 ? year : DateTime.Now.Year,
                        Quantity = quantity > 0 ? quantity : 1,
                        AvailableQuantity = quantity > 0 ? quantity : 1,
                        Price = price > 0 ? price : 50000m,
                        Description = description ?? "Sách mới nhập kho",
                        CoverImage = "/images/no-image.png",
                        BooksStatus = BooksStatus.Available
                    };

                    _context.Books.Add(newBook);
                    await _context.SaveChangesAsync();
                    successCount++;
                }
                catch
                {
                    errorCount++;
                }
            }

            TempData["SuccessMessage"] = $"Đã nhập thành công {successCount} cuốn sách vào kho dữ liệu." + (errorCount > 0 ? $" ({errorCount} dòng bỏ qua do lỗi)" : "");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Lỗi khi xử lý file Excel: " + ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
