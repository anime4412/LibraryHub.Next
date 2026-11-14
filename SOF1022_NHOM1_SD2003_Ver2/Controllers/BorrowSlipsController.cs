using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SOF1022_NHOM1_SD2003_Ver2.Attributes;
using SOF1022_NHOM1_SD2003_Ver2.Datas;
using SOF1022_NHOM1_SD2003_Ver2.Helpers;
using SOF1022_NHOM1_SD2003_Ver2.Models;
using SOF1022_NHOM1_SD2003_Ver2.Models.DTOs;
using SOF1022_NHOM1_SD2003_Ver2.Models.Enums;
using SOF1022_NHOM1_SD2003_Ver2.Models.ViewModels;
using SOF1022_NHOM1_SD2003_Ver2.Services;
using ClosedXML.Excel;

namespace SOF1022_NHOM1_SD2003_Ver2.Controllers
{
    [SessionAuthorize]
    public class BorrowSlipsController : Controller
    {
        private readonly IBorrowService _borrowService;
        private readonly LibraryDbContext _context;
        private readonly IFineCalculationService _fineCalculationService;
        private readonly INotificationService _notificationService;

        public BorrowSlipsController(LibraryDbContext context, IBorrowService borrowService, IFineCalculationService fineCalculationService, INotificationService notificationService)
        {
            _context = context;
            _borrowService = borrowService;
            _fineCalculationService = fineCalculationService;
            _notificationService = notificationService;
        }

        // GET: BorrowSlips/GetReturnBookModal/{id}
        [SessionAuthorize(Roles = "Admin,Librarian")]
        public async Task<IActionResult> GetReturnBookModal(int id)
        {
            var slip = await _context.BorrowSlips
                .Include(x => x.BorrowDetails!)
                .ThenInclude(x => x.Books)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (slip == null)
                return NotFound();

            return PartialView("_ReturnBookModalPartial", slip);
        }

        // GET: BorrowSlips
        [SessionAuthorize(Roles = "Admin,Librarian")]
        [PermissionAuthorize("borrow.view")]
        public async Task<IActionResult> Index(string? statusFilter, string? searchString)
        {
            // Calculate statistics from full database (before filtering)
            var allBorrowSlips = await _context.BorrowSlips.ToListAsync();
            ViewBag.TotalCount = allBorrowSlips.Count;
            ViewBag.PendingApprovalCount = allBorrowSlips.Count(x => x.BorrowStatus == BorrowStatus.PendingApproval);
            ViewBag.BorrowingCount = allBorrowSlips.Count(x => x.BorrowStatus == BorrowStatus.Borrowing);
            ViewBag.OverdueCount = allBorrowSlips.Count(x => x.BorrowStatus == BorrowStatus.Borrowing && x.DueDate < DateTime.Now);

            var borrowSlips = _context.BorrowSlips
                .Include(x => x.Readers)
                .Include(x => x.BorrowDetails)
                .AsQueryable();

            // Apply status filter
            switch (statusFilter)
            {
                case "All":
                    // Return all records
                    break;
                case "PendingApproval":
                    borrowSlips = borrowSlips.Where(x => x.BorrowStatus == BorrowStatus.PendingApproval);
                    break;
                case "PendingPickup":
                    borrowSlips = borrowSlips.Where(x => x.BorrowStatus == BorrowStatus.ApprovedAwaitingPickup);
                    break;
                case "PendingExtension":
                    borrowSlips = borrowSlips.Where(x => x.BorrowStatus == BorrowStatus.PendingExtension);
                    break;
                case "Borrowing":
                    borrowSlips = borrowSlips.Where(x => x.BorrowStatus == BorrowStatus.Borrowing);
                    break;
                case "Overdue":
                    borrowSlips = borrowSlips.Where(x => x.DueDate < DateTime.Now 
                        && x.BorrowStatus != BorrowStatus.Returned 
                        && x.BorrowStatus != BorrowStatus.Cancelled);
                    break;
                case "Incident":
                    borrowSlips = borrowSlips.Where(x => x.BorrowStatus == BorrowStatus.LostOrDamaged);
                    break;
                case "Completed":
                    borrowSlips = borrowSlips.Where(x => x.BorrowStatus == BorrowStatus.Returned);
                    break;
                default:
                    // Default: exclude returned and cancelled
                    borrowSlips = borrowSlips.Where(x => x.BorrowStatus != BorrowStatus.Returned && x.BorrowStatus != BorrowStatus.Cancelled);
                    break;
            }

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                borrowSlips = borrowSlips.Where(x =>
                    x.BorrowCode!.Contains(searchString) ||
                    x.Readers!.FullName!.Contains(searchString));
            }

            ViewBag.StatusFilter = statusFilter;
            ViewBag.SearchString = searchString;

            return View(await borrowSlips
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync());
        }

        // GET: BorrowSlips/History
        [SessionAuthorize(Roles = "Admin,Librarian")]
        [PermissionAuthorize("borrow.view")]
        public async Task<IActionResult> History(string? search)
        {
            var borrowSlips = _context.BorrowSlips
                .Include(x => x.Readers)
                .Include(x => x.BorrowDetails)
                .Where(x => x.BorrowStatus == BorrowStatus.Returned 
                    || x.BorrowStatus == BorrowStatus.Cancelled 
                    || x.BorrowStatus == BorrowStatus.CanceledDueToTimeout)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                borrowSlips = borrowSlips.Where(x =>
                    x.BorrowCode!.Contains(search) ||
                    x.Readers!.FullName!.Contains(search));
            }

            return View(await borrowSlips
                .OrderByDescending(x => x.ReturnDate)
                .ToListAsync());
        }

        // GET: BorrowSlips/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var borrowSlip = await _context.BorrowSlips
                .Include(x => x.Readers)
                .Include(x => x.BorrowDetails!)
                    .ThenInclude(d => d.Books!)
                        .ThenInclude(b => b.Author)
                .Include(x => x.Fines!)
                    .ThenInclude(f => f.FineDetails)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (borrowSlip == null)
            {
                return NotFound();
            }

            return View(borrowSlip);
        }

        // GET: BorrowSlips/GetDetails/{id}
        [HttpGet]
        public async Task<IActionResult> GetDetails(int id)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (!int.TryParse(userIdString, out int userId))
            {
                return Json(new { success = false, message = "Phiên đăng nhập hết hạn" });
            }

            var userRole = HttpContext.Session.GetString("Role") ?? "Reader";

            var slip = await _context.BorrowSlips
                .Include(x => x.BorrowDetails!)
                    .ThenInclude(d => d.Books!)
                        .ThenInclude(b => b.Author)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (slip == null)
            {
                return Json(new { success = false, message = "Không tìm thấy chi tiết phiếu mượn" });
            }

            if (slip.ReaderId != userId && userRole != "Admin" && userRole != "Librarian")
            {
                return Json(new { success = false, message = "Bạn không có quyền xem phiếu mượn này" });
            }

            var statusText = slip.BorrowStatus switch
            {
                BorrowStatus.PendingApproval => "Chờ duyệt",
                BorrowStatus.ApprovedAwaitingPickup => "Chờ lấy sách",
                BorrowStatus.Borrowing => slip.DueDate < DateTime.Now ? "Quá hạn" : "Đang mượn",
                BorrowStatus.PendingExtension => "Chờ duyệt gia hạn",
                BorrowStatus.Returned or BorrowStatus.Completed => "Đã trả",
                BorrowStatus.Cancelled or BorrowStatus.CanceledDueToTimeout => "Đã hủy",
                BorrowStatus.LostOrDamaged => "Sự cố / Bồi thường",
                _ => slip.BorrowStatus.ToString()
            };

            var items = (slip.BorrowDetails ?? new List<BorrowDetails>())
                .Where(d => !d.IsReturned)
                .Select(d => new
                {
                    detailId = d.Id,
                    bookId = d.BookId,
                    bookTitle = d.Books?.Title ?? "Không rõ",
                    bookCover = d.Books?.CoverImage ?? "",
                    authorName = d.Books?.Author?.Name ?? "",
                    extensionCount = d.ExtensionCount,
                    fineAmount = d.CompensationAmount
                })
                .ToList();

            return Json(new
            {
                success = true,
                data = new
                {
                    id = slip.Id,
                    borrowCode = slip.BorrowCode ?? ("BR" + slip.Id.ToString("D6")),
                    borrowStatus = (int)slip.BorrowStatus,
                    statusText,
                    borrowDate = slip.BorrowDate,
                    dueDate = slip.DueDate,
                    pickupDeadline = slip.PickupDeadline,
                    totalBooks = slip.TotalBooks,
                    note = slip.Note,
                    items
                }
            });
        }

        // GET: BorrowSlips/Create
        [HttpGet]
        // [PermissionAuthorize("borrow.create")] // TEMP-DISABLED: allow readers to borrow books
        public async Task<IActionResult> Create(
            int? bookId = null,
            string? searchString = null,
            int? categoryId = null,
            string? sortOrder = null,
            string? selectedBooksData = null)
        {
            var userName = HttpContext.Session.GetString("UserName") ?? "Người dùng";
            var userRole = HttpContext.Session.GetString("Role") ?? "Reader";
            ViewData["CurrentUserName"] = userName;
            ViewData["UserRole"] = userRole;

            // Calculate current borrow count for the logged-in user
            int currentBorrowCount = 0;
            int maxBorrowBooks = 10;
            var userIdString = HttpContext.Session.GetString("UserId");
            if (int.TryParse(userIdString, out int userId))
            {
                var reader = await _context.Readers.FindAsync(userId);
                if (reader != null)
                {
                    maxBorrowBooks = reader.MaxBorrowBooks;
                    currentBorrowCount = await _context.BorrowDetails
                        .CountAsync(d => d.BorrowSlips!.ReaderId == userId 
                            && !d.IsReturned
                            && (d.BorrowSlips.BorrowStatus == BorrowStatus.PendingApproval
                                || d.BorrowSlips.BorrowStatus == BorrowStatus.ApprovedAwaitingPickup
                                || d.BorrowSlips.BorrowStatus == BorrowStatus.Borrowing));
                }
            }
            ViewBag.CurrentBorrowCount = currentBorrowCount;
            ViewBag.MaxBorrowBooks = maxBorrowBooks;

            // Preserve selected books data from filter submission
            if (!string.IsNullOrEmpty(selectedBooksData))
            {
                ViewBag.SelectedBooksData = selectedBooksData;
            }

            var booksQuery = _context.Books
                .Where(x => x.BooksStatus != BooksStatus.Hidden)
                .Include(x => x.Category)
                .Include(x => x.Author)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                booksQuery = booksQuery.Where(x =>
                    (x.Title != null && EF.Functions.Like(x.Title, $"%{searchString}%")) ||
                    (x.Author != null && x.Author.Name != null && EF.Functions.Like(x.Author.Name, $"%{searchString}%")) ||
                    (x.ISBN != null && EF.Functions.Like(x.ISBN, $"%{searchString}%"))
                );
            }

            // Apply category filter
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                booksQuery = booksQuery.Where(x => x.CategoriesId == categoryId);
            }

            // Apply sorting
            switch (sortOrder)
            {
                case "name_asc":
                    booksQuery = booksQuery.OrderBy(x => x.Title);
                    break;
                case "name_desc":
                    booksQuery = booksQuery.OrderByDescending(x => x.Title);
                    break;
                case "quantity_desc":
                    booksQuery = booksQuery.OrderByDescending(x => x.AvailableQuantity);
                    break;
                case "new":
                    booksQuery = booksQuery.OrderByDescending(x => x.Id);
                    break;
                default:
                    booksQuery = booksQuery.OrderByDescending(x => x.Id);
                    break;
            }

            ViewData["Books"] = await booksQuery.ToListAsync();
            ViewData["Categories"] = await _context.Categories.ToListAsync();

            // Calculate real available counts for all books
            var allBookIds = ((List<Books>)ViewData["Books"]).Select(x => x.Id).ToList();
            var committedCounts = await BookAvailabilityHelper.GetCommittedCountsAsync(_context, allBookIds);
            ViewData["RealAvailableCounts"] = committedCounts;

            // Preserve filter values
            ViewBag.SearchString = searchString;
            ViewBag.CategoryId = categoryId;
            ViewBag.SortOrder = sortOrder;

            // If bookId is provided, pre-select it
            if (bookId.HasValue)
            {
                var book = await _context.Books.FindAsync(bookId.Value);
                if (book != null)
                {
                    ViewData["PreSelectedBookId"] = bookId.Value;
                    ViewData["PreSelectedBookTitle"] = book.Title;
                }
            }

            // Load readers for admin mode
            if (userRole == "Admin" || userRole == "Librarian")
            {
                ViewData["Readers"] = await _context.Readers
                    .Where(r => r.IsActive == true)
                    .OrderBy(r => r.FullName)
                    .ToListAsync();
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // [PermissionAuthorize("borrow.create")] // TEMP-DISABLED: allow readers to borrow books
        public async Task<IActionResult> Create(List<int> bookIds, int? readerId, DateTime? borrowDate, DateTime? dueDate, string? note)
        {
            var userRole = HttpContext.Session.GetString("Role") ?? "Reader";
            
            // For admin mode, use the selected readerId
            // For reader mode, use the logged-in user's ID
            int targetReaderId;
            
            if (userRole == "Admin" || userRole == "Librarian")
            {
                if (!readerId.HasValue)
                {
                    TempData["Error"] = "Vui lòng chọn độc giả để tạo phiếu mượn.";
                    return RedirectToAction("Create");
                }
                targetReaderId = readerId.Value;
            }
            else
            {
                var userIdString = HttpContext.Session.GetString("UserId");
                if (!int.TryParse(userIdString, out targetReaderId))
                {
                    TempData["Error"] = "Bạn cần đăng nhập để mượn sách";
                    return RedirectToAction("Login", "Account");
                }
            }

            // Age validation: Check if reader meets minimum age requirements for all books
            var reader = await _context.Readers.FindAsync(targetReaderId);
            if (reader == null || !reader.DateOfBirth.HasValue)
            {
                TempData["Error"] = "Không tìm thấy thông tin độc giả hoặc thiếu ngày sinh.";
                return RedirectToAction("Create");
            }

            // Check for unpaid fines
            var hasUnpaidFines = await _context.Fines
                .AnyAsync(f => f.ReaderId == targetReaderId && !f.IsPaid);
            
            if (hasUnpaidFines)
            {
                TempData["Error"] = "Bạn có khoản phí phạt chưa thanh toán! Vui lòng tất toán trước khi mượn sách mới.";
                return RedirectToAction("Create");
            }

            // TEMP-DISABLED: verification gate blocking borrow flow
            // Underlying bug: AccountController has no "Edit" action (RedirectToAction dead route),
            // and Reader.IsVerified is only set in UpdateProfile, so users with phone/address set
            // through other paths stay permanently IsVerified=false and get stuck.
            /* TEMP-DISABLED: verification gate blocking borrow flow - see note below
            if (!reader.IsVerified || string.IsNullOrEmpty(reader.Phone) || string.IsNullOrEmpty(reader.Address))
            {
                TempData["Error"] = "Bạn cần cập nhật thông tin cá nhân (SĐT, địa chỉ) trước khi mượn sách.";
                return RedirectToAction("Edit", "Account", new { id = targetReaderId });
            }
            */

            var books = await _context.Books
                .Where(b => bookIds.Contains(b.Id))
                .ToListAsync();

            var restrictedBooks = books
                .Where(b => b.MinimumAge > 0 && !AgeHelper.MeetsAgeRequirement(reader.DateOfBirth.Value, b.MinimumAge))
                .Select(b => new { b.Title, b.MinimumAge })
                .ToList();

            if (restrictedBooks.Any())
            {
                var restrictedBookList = string.Join(", ", restrictedBooks.Select(b => $"{b.Title} ({b.MinimumAge}+)"));
                TempData["Error"] = $"Bạn chưa đủ tuổi để mượn các sách sau: {restrictedBookList}";
                return RedirectToAction("Create");
            }

            // Check for duplicate borrow requests - check each book individually
            var duplicateBooks = new List<string>();
            var validBookIds = new List<int>();
            var unavailableBooks = new List<string>();

            foreach (var bookId in bookIds)
            {
                var book = await _context.Books.FindAsync(bookId);
                var bookTitle = book?.Title ?? $"Unknown Book (ID: {bookId})";

                var hasExistingBorrow = await _context.BorrowDetails
                    .Include(bd => bd.BorrowSlips)
                    .Include(bd => bd.Books)
                    .AnyAsync(bd => bd.BookId == bookId 
                                && bd.BorrowSlips.ReaderId == targetReaderId
                                && !bd.IsReturned
                                && (bd.BorrowSlips.BorrowStatus == BorrowStatus.PendingApproval 
                                    || bd.BorrowSlips.BorrowStatus == BorrowStatus.ApprovedAwaitingPickup
                                    || bd.BorrowSlips.BorrowStatus == BorrowStatus.Borrowing));

                if (hasExistingBorrow)
                {
                    duplicateBooks.Add(bookTitle);
                }
                else
                {
                    // Check real-time availability including committed requests (PendingApproval, ApprovedAwaitingPickup, Borrowing)
                    int realAvailable = book != null 
                        ? await BookAvailabilityHelper.GetRealAvailableCountAsync(_context, bookId, book.Quantity)
                        : 0;
                    
                    if (realAvailable <= 0)
                    {
                        unavailableBooks.Add(bookTitle);
                    }
                    else
                    {
                        validBookIds.Add(bookId);
                    }
                }
            }

            if (unavailableBooks.Any() && bookIds.Count == 1)
            {
                // Single book unavailable - redirect to reservation
                var unavailableBookList = string.Join(", ", unavailableBooks);
                TempData["Info"] = $"Sách này hiện đã có người đăng ký giữ chỗ. Hệ thống đã chuyển yêu cầu của bạn sang danh sách Đặt trước!";
                return RedirectToAction("Create", "Reservations", new { bookId = bookIds.First() });
            }

            if (duplicateBooks.Any())
            {
                if (validBookIds.Any())
                {
                    // Allow borrowing non-duplicate books
                    var duplicateBookList = string.Join(", ", duplicateBooks);
                    TempData["Warning"] = $"Các sách sau đã được đăng ký hoặc đang mượn: {duplicateBookList}. Chỉ các sách chưa mượn sẽ được tạo phiếu.";
                    bookIds = validBookIds;
                }
                else
                {
                    // All books are duplicates
                    var duplicateBookList = string.Join(", ", duplicateBooks);
                    TempData["Error"] = $"Bạn đã đăng ký hoặc đang mượn tất cả các sách sau: {duplicateBookList}. Vui lòng kiểm tra trang sách đang mượn.";
                    return RedirectToAction("MyBorrows");
                }
            }

            var userName = HttpContext.Session.GetString("UserName") ?? "Người dùng";
            var createdBy = (userRole == "Admin" || userRole == "Librarian") ? userName : "Hệ thống tự động";

            var result = await _borrowService.BorrowAsync(targetReaderId, bookIds, dueDate: dueDate, note: note, createdBy: createdBy, borrowDate: borrowDate);

            if (userRole == "Admin" || userRole == "Librarian")
            {
                if (result.Success)
                {
                    if (result.PartialSuccess)
                    {
                        TempData["Warning"] = result.Message;
                        TempData["FailedBooksJson"] = System.Text.Json.JsonSerializer.Serialize(result.FailedBooks);
                    }
                    else
                    {
                        TempData["Success"] = result.Message;
                    }
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["Error"] = result.Message;
                    TempData["FailedBooksJson"] = System.Text.Json.JsonSerializer.Serialize(result.FailedBooks);
                    if (result.UnpaidFineAmount.HasValue)
                    {
                        TempData["UnpaidFineAmount"] = result.UnpaidFineAmount.Value.ToString("N0");
                    }
                    return RedirectToAction("Create");
                }
            }
            else
            {
                if (result.Success)
                {
                    if (result.PartialSuccess)
                    {
                        TempData["Warning"] = result.Message;
                        TempData["FailedBooksJson"] = System.Text.Json.JsonSerializer.Serialize(result.FailedBooks);
                    }
                    return RedirectToAction("MyBorrows");
                }
                else
                {
                    TempData["Error"] = result.Message;
                    TempData["FailedBooksJson"] = System.Text.Json.JsonSerializer.Serialize(result.FailedBooks);
                    if (result.UnpaidFineAmount.HasValue)
                    {
                        TempData["UnpaidFineAmount"] = result.UnpaidFineAmount.Value.ToString("N0");
                    }
                    return RedirectToAction("Create");
                }
            }
        }

        // POST: BorrowSlips/ReturnBook/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReturnBook(int id, string? damageDetails)
        {
            var result = await _borrowService.ReturnAsync(id, damageDetails: damageDetails);

            // Check if there are pending reservations for the returned book
            if (result.Success)
            {
                // Get the borrow detail to find the book ID
                var borrowDetail = await _context.BorrowDetails.FindAsync(id);
                if (borrowDetail != null)
                {
                    // Find the first pending reservation for this book
                    var pendingReservation = await _context.Reservations
                        .Where(r => r.BookId == borrowDetail.BookId && 
                                   r.BooksStatus == ReservationsStatus.Waiting)
                        .OrderBy(r => r.ReservationDate)
                        .FirstOrDefaultAsync();

                    if (pendingReservation != null)
                    {
                        // Update reservation status to Available
                        pendingReservation.BooksStatus = ReservationsStatus.Approved;
                        pendingReservation.Status = "Available";
                        await _context.SaveChangesAsync();
                    }
                }
            }

            return Json(new { success = result.Success, message = result.Message });
        }

        // POST: BorrowSlips/ApproveIssueBook (with custom dates)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize(Roles = "Admin,Librarian")]
        public async Task<IActionResult> ApproveIssueBook([FromBody] ApproveRequestDto dto)
        {
            try
            {
                Console.WriteLine($"ApproveIssueBook called with BorrowSlipId={dto?.BorrowSlipId}, BorrowDate={dto?.BorrowDate}, DurationDays={dto?.DurationDays}");

                if (dto == null)
                {
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ!" });
                }

                if (dto.BorrowSlipId <= 0)
                {
                    return Json(new { success = false, message = "Mã phiếu mượn không hợp lệ!" });
                }

                // Validate BorrowDate is not default
                if (dto.BorrowDate == default)
                {
                    return Json(new { success = false, message = "Ngày mượn không hợp lệ!" });
                }

                // Validate DurationDays
                if (dto.DurationDays <= 0)
                {
                    return Json(new { success = false, message = "Số ngày mượn không hợp lệ!" });
                }

                // Fetch slip without tracking conflicts
                var slip = await _context.BorrowSlips
                    .Include(b => b.BorrowDetails)
                    .ThenInclude(b => b.Books)
                    .FirstOrDefaultAsync(b => b.Id == dto.BorrowSlipId);

                if (slip == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy phiếu mượn trong cơ sở dữ liệu!" });
                }

                if (slip.BorrowStatus != BorrowStatus.PendingApproval)
                {
                    return Json(new { success = false, message = "Phiếu mượn không ở trạng thái chờ duyệt!" });
                }

                // Validate book inventory - dùng số thực còn (Quantity - số đã cam kết cho các phiếu
                // Pending/Approved/Borrowing khác), không dùng AvailableQuantity (kho vật lý, chỉ trừ
                // khi giao sách) để tránh duyệt vượt quá số bản in thực tế còn trống.
                foreach (var detail in slip.BorrowDetails)
                {
                    if (detail.Books == null)
                    {
                        return Json(new { success = false, message = "Không tìm thấy thông tin sách trong phiếu mượn!" });
                    }

                    var committedByOthers = await BookAvailabilityHelper.GetCommittedCountAsync(_context, detail.Books.Id)
                        - 1; // trừ chính phiếu đang xét (đang ở PendingApproval nên đã được tính trong committed)
                    var realAvailableCount = detail.Books.Quantity - committedByOthers;

                    if (realAvailableCount <= 0)
                    {
                        return Json(new { success = false, message = $"Sách '{detail.Books.Title}' đã hết, không thể giao!" });
                    }
                }

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Update fields safely
                    slip.BorrowDate = dto.BorrowDate;
                    
                    // Calculate DueDate from BorrowDate + DurationDays
                    slip.DueDate = dto.BorrowDate.AddDays(dto.DurationDays);
                    
                    // Change status to ApprovedAwaitingPickup (not yet physically handed over)
                    slip.BorrowStatus = BorrowStatus.ApprovedAwaitingPickup;
                    slip.PickupDeadline = DateTime.Now.AddDays(3); // 3-day pickup deadline

                    // DO NOT subtract from book inventory yet - will be deducted on handover
                    // Ensure proper borrow state
                    foreach (var detail in slip.BorrowDetails)
                    {
                        if (detail.Books != null)
                        {
                            detail.Books.LastBorrowDate = DateTime.Now;
                            
                            // CRITICAL: Ensure borrow detail is in correct state
                            detail.IsReturned = false;
                            detail.ReturnedDate = null;
                            detail.IsLost = false;
                            detail.IsDamaged = false;
                            detail.ConditionAfter = null;
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Json(new { success = true, message = "Duyệt và giao sách thành công!" });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"Transaction error: {ex.Message}");
                    return Json(new { success = false, message = $"Lỗi giao dịch: {ex.Message}" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ApproveIssueBook error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: BorrowSlips/Approve
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize(Roles = "Admin,Librarian")]
        public async Task<IActionResult> Approve([FromBody] IdRequestDto request)
        {
            var id = request.Id;
            var slip = await _context.BorrowSlips
                .Include(x => x.BorrowDetails!)
                .ThenInclude(x => x.Books)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (slip == null)
                return Json(new { success = false, message = "Không tìm thấy phiếu mượn" });

            if (slip.BorrowStatus != BorrowStatus.PendingApproval)
                return Json(new { success = false, message = "Chỉ có thể duyệt phiếu đang chờ duyệt" });

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Change status to ApprovedAwaitingPickup
                slip.BorrowStatus = BorrowStatus.ApprovedAwaitingPickup;
                slip.PickupDeadline = DateTime.Now.AddDays(3);

                // DO NOT subtract from book inventory yet - will be deducted on handover

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var senderName = HttpContext.Session.GetString("UserName") ?? "Thủ thư";
                await _notificationService.NotifyUserAsync(
                    slip.ReaderId,
                    "Reader",
                    NotificationType.BorrowApproved,
                    "Phiếu mượn đã được duyệt",
                    $"Phiếu mượn {slip.BorrowCode} đã được duyệt. Vui lòng đến nhận sách trước {slip.PickupDeadline:dd/MM/yyyy HH:mm}.",
                    link: "/BorrowSlips/MyBorrows",
                    createdBy: senderName);

                return Json(new { success = true, message = "Đã duyệt phiếu mượn thành công" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // POST: BorrowSlips/Reject
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize(Roles = "Admin,Librarian")]
        public async Task<IActionResult> Reject([FromBody] IdRequestDto request)
        {
            var id = request.Id;
            var slip = await _context.BorrowSlips
                .Include(x => x.BorrowDetails!)
                .ThenInclude(x => x.Books)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (slip == null)
                return Json(new { success = false, message = "Không tìm thấy phiếu mượn" });

            if (slip.BorrowStatus != BorrowStatus.PendingApproval)
                return Json(new { success = false, message = "Chỉ có thể từ chối phiếu đang chờ duyệt" });

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Change status to Cancelled
                slip.BorrowStatus = BorrowStatus.Cancelled;
                slip.Note = "Từ chối duyệt bởi thủ thư";

                // DO NOT restore inventory - it was never deducted for PendingApproval

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var senderName = HttpContext.Session.GetString("UserName") ?? "Thủ thư";
                await _notificationService.NotifyUserAsync(
                    slip.ReaderId,
                    "Reader",
                    NotificationType.BorrowRejected,
                    "Phiếu mượn bị từ chối",
                    $"Phiếu mượn {slip.BorrowCode} đã bị từ chối bởi thủ thư.",
                    link: "/BorrowSlips/MyBorrows",
                    createdBy: senderName);

                return Json(new { success = true, message = "Đã từ chối phiếu mượn thành công" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // POST: BorrowSlips/Deliver
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize(Roles = "Admin,Librarian")]
        public async Task<IActionResult> Deliver([FromBody] IdRequestDto request)
        {
            var id = request.Id;
            var slip = await _context.BorrowSlips
                .Include(x => x.BorrowDetails!)
                .ThenInclude(x => x.Books)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (slip == null)
                return Json(new { success = false, message = "Không tìm thấy phiếu mượn" });

            if (slip.BorrowStatus != BorrowStatus.ApprovedAwaitingPickup)
                return Json(new { success = false, message = "Chỉ có thể xác nhận giao sách cho phiếu đang chờ nhận" });

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Change status to Borrowing (physically handed over)
                slip.BorrowStatus = BorrowStatus.Borrowing;
                slip.BorrowDate = DateTime.Now;
                slip.DueDate = DateTime.Now.AddDays(14); // Standard 14-day loan period
                slip.PickupDeadline = null; // Clear pickup deadline so background worker ignores it

                // Deduct from book inventory only when physically handed over
                foreach (var detail in slip.BorrowDetails)
                {
                    if (detail.Books != null)
                    {
                        detail.Books.AvailableQuantity = Math.Max(0, detail.Books.AvailableQuantity - 1);
                        
                        // CRITICAL: Ensure borrow detail is in correct state
                        detail.IsReturned = false;
                        detail.ReturnedDate = null;
                        detail.IsLost = false;
                        detail.IsDamaged = false;
                        detail.ConditionAfter = null;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var senderName = HttpContext.Session.GetString("UserName") ?? "Thủ thư";
                await _notificationService.NotifyUserAsync(
                    slip.ReaderId,
                    "Reader",
                    NotificationType.BorrowDelivered,
                    "Đã nhận sách thành công",
                    $"Bạn đã nhận sách theo phiếu mượn {slip.BorrowCode}. Hạn trả là {slip.DueDate:dd/MM/yyyy}.",
                    link: "/BorrowSlips/MyBorrows",
                    createdBy: senderName);

                return Json(new { success = true, message = "Đã xác nhận giao sách thành công" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // POST: BorrowSlips/ConfirmHandover/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize(Roles = "Admin,Librarian")]
        public async Task<IActionResult> ConfirmHandover(int id)
        {
            var slip = await _context.BorrowSlips
                .Include(x => x.BorrowDetails!)
                .ThenInclude(x => x.Books)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (slip == null)
                return Json(new { success = false, message = "Không tìm thấy phiếu mượn" });

            if (slip.BorrowStatus != BorrowStatus.ApprovedAwaitingPickup)
                return Json(new { success = false, message = "Chỉ có thể xác nhận giao sách cho phiếu đang chờ nhận" });

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Change status to Borrowing (physically handed over)
                slip.BorrowStatus = BorrowStatus.Borrowing;
                slip.BorrowDate = DateTime.Now;
                slip.DueDate = DateTime.Now.AddDays(14); // Standard 14-day loan period
                slip.PickupDeadline = null; // Clear pickup deadline so background worker ignores it

                // Deduct from book inventory only when physically handed over
                foreach (var detail in slip.BorrowDetails)
                {
                    if (detail.Books != null)
                    {
                        detail.Books.AvailableQuantity = Math.Max(0, detail.Books.AvailableQuantity - 1);
                        
                        // CRITICAL: Ensure borrow detail is in correct state
                        detail.IsReturned = false;
                        detail.ReturnedDate = null;
                        detail.IsLost = false;
                        detail.IsDamaged = false;
                        detail.ConditionAfter = null;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var senderName = HttpContext.Session.GetString("UserName") ?? "Thủ thư";
                await _notificationService.NotifyUserAsync(
                    slip.ReaderId,
                    "Reader",
                    NotificationType.BorrowDelivered,
                    "Đã nhận sách thành công",
                    $"Bạn đã nhận sách theo phiếu mượn {slip.BorrowCode}. Hạn trả là {slip.DueDate:dd/MM/yyyy}.",
                    link: "/BorrowSlips/MyBorrows",
                    createdBy: senderName);

                return Json(new { success = true, message = "Đã xác nhận giao sách thành công" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // POST: BorrowSlips/ExtendLoan/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize(Roles = "Admin,Librarian")]
        public async Task<IActionResult> ExtendLoan(int id, int extendDays = 7)
        {
            var slip = await _context.BorrowSlips.FindAsync(id);

            if (slip == null)
                return Json(new { success = false, message = "Không tìm thấy phiếu mượn." });

            if (slip.BorrowStatus != BorrowStatus.Borrowing && slip.BorrowStatus != BorrowStatus.Overdue)
                return Json(new { success = false, message = "Chỉ có thể gia hạn cho phiếu đang trong trạng thái mượn hoặc quá hạn." });

            if (extendDays <= 0 || extendDays > 30)
                extendDays = 7;

            // Extend DueDate
            slip.DueDate = (slip.DueDate < DateTime.Now ? DateTime.Now : slip.DueDate).AddDays(extendDays);
            if (slip.BorrowStatus == BorrowStatus.Overdue)
            {
                slip.BorrowStatus = BorrowStatus.Borrowing;
            }

            slip.Note = string.IsNullOrEmpty(slip.Note) 
                ? $"Gia hạn thêm {extendDays} ngày vào {DateTime.Now:dd/MM/yyyy HH:mm} bởi Thủ thư."
                : $"{slip.Note}\n[Gia hạn thêm {extendDays} ngày vào {DateTime.Now:dd/MM/yyyy HH:mm}]";

            await _context.SaveChangesAsync();

            var senderName = HttpContext.Session.GetString("UserName") ?? "Thủ thư";
            if (_notificationService != null)
            {
                await _notificationService.NotifyUserAsync(
                    slip.ReaderId,
                    "Reader",
                    NotificationType.ExtensionApproved,
                    "Gia hạn phiếu mượn thành công",
                    $"Phiếu mượn {slip.BorrowCode} đã được gia hạn thêm {extendDays} ngày. Hạn trả mới: {slip.DueDate:dd/MM/yyyy}.",
                    link: "/BorrowSlips/MyBorrows",
                    createdBy: senderName);
            }

            return Json(new { 
                success = true, 
                message = $"Đã gia hạn thành công thêm {extendDays} ngày. Hạn trả mới: {slip.DueDate:dd/MM/yyyy}.",
                newDueDate = slip.DueDate.ToString("dd/MM/yyyy")
            });
        }

        // POST: BorrowSlips/CancelOrder/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize(Roles = "Admin,Librarian")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var slip = await _context.BorrowSlips.FindAsync(id);

            if (slip == null)
                return Json(new { success = false, message = "Không tìm thấy phiếu mượn" });

            if (slip.BorrowStatus != BorrowStatus.PendingApproval)
                return Json(new { success = false, message = "Chỉ có thể hủy phiếu mượn đang chờ duyệt" });

            try
            {
                slip.BorrowStatus = BorrowStatus.Cancelled;
                await _context.SaveChangesAsync();

                var senderName = HttpContext.Session.GetString("UserName") ?? "Thủ thư";
                await _notificationService.NotifyUserAsync(
                    slip.ReaderId,
                    "Reader",
                    NotificationType.BorrowCancelled,
                    "Phiếu mượn bị hủy",
                    $"Phiếu mượn {slip.BorrowCode} đã bị hủy bởi thủ thư.",
                    link: "/BorrowSlips/MyBorrows",
                    createdBy: senderName);

                return Json(new { success = true, message = "Đã hủy phiếu mượn thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // POST: BorrowSlips/CancelExpiredSlip/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize(Roles = "Admin,Librarian")]
        public async Task<IActionResult> CancelExpiredSlip(int id)
        {
            var slip = await _context.BorrowSlips
                .Include(x => x.BorrowDetails!)
                .ThenInclude(x => x.Books)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (slip == null)
                return Json(new { success = false, message = "Không tìm thấy phiếu mượn" });

            if (slip.BorrowStatus != BorrowStatus.Borrowing)
                return Json(new { success = false, message = "Chỉ có thể hủy phiếu mượn đang ở trạng thái Đang mượn" });

            if (!slip.PickupDeadline.HasValue || slip.PickupDeadline > DateTime.Now)
                return Json(new { success = false, message = "Phiếu mượn chưa quá hạn nhận" });

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Change status to CanceledDueToTimeout
                slip.BorrowStatus = BorrowStatus.CanceledDueToTimeout;
                slip.Note = "Tự động hủy do quá hạn nhận sách";

                // Restore book inventory since books were deducted when handed over (Borrowing status)
                foreach (var detail in slip.BorrowDetails)
                {
                    if (detail.Books != null)
                    {
                        detail.Books.AvailableQuantity += 1;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                await _notificationService.NotifyUserAsync(
                    slip.ReaderId,
                    "Reader",
                    NotificationType.BorrowCancelled,
                    "Phiếu mượn đã bị hủy do quá hạn nhận sách",
                    $"Phiếu mượn {slip.BorrowCode} đã tự động bị hủy vì bạn chưa đến nhận sách đúng hạn.",
                    link: "/BorrowSlips/MyBorrows",
                    createdBy: "Hệ thống");

                return Json(new { success = true, message = "Đã hủy phiếu mượn quá hạn và hoàn lại kho sách" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // POST: BorrowSlips/CancelItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize]
        public async Task<IActionResult> CancelItem([FromBody] CancelItemRequestDto request)
        {
            return await CancelBorrowDetailAsync(request.ResolveDetailId());
        }

        // POST: BorrowSlips/CancelRequest
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize]
        public async Task<IActionResult> CancelRequest([FromBody] CancelItemRequestDto request)
        {
            return await CancelBorrowDetailAsync(request.ResolveDetailId());
        }

        // POST: BorrowSlips/CancelSlip - hủy toàn bộ phiếu (user, trạng thái Pending)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize]
        public async Task<IActionResult> CancelSlip([FromBody] IdRequestDto request)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (!int.TryParse(userIdString, out int userId))
            {
                return Json(new { success = false, message = "Phiên đăng nhập hết hạn" });
            }

            var slip = await _context.BorrowSlips
                .Include(x => x.BorrowDetails!)
                    .ThenInclude(d => d.Books)
                .FirstOrDefaultAsync(x => x.Id == request.Id);

            if (slip == null)
                return Json(new { success = false, message = "Không tìm thấy phiếu mượn" });

            if (slip.ReaderId != userId)
                return Json(new { success = false, message = "Bạn không có quyền hủy phiếu này" });

            if (slip.BorrowStatus != BorrowStatus.PendingApproval && slip.BorrowStatus != BorrowStatus.ApprovedAwaitingPickup)
                return Json(new { success = false, message = "Chỉ có thể hủy phiếu đang chờ duyệt hoặc chờ nhận sách" });

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var detail in slip.BorrowDetails ?? new List<BorrowDetails>())
                {
                    // DO NOT restore inventory - it was never deducted for PendingApproval/ApprovedAwaitingPickup
                    _context.BorrowDetails.Remove(detail);
                }

                slip.BorrowStatus = BorrowStatus.Cancelled;
                slip.TotalBooks = 0;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                await _notificationService.NotifyUserAsync(
                    slip.ReaderId,
                    "Reader",
                    NotificationType.BorrowCancelled,
                    "Đã hủy phiếu mượn",
                    $"Bạn đã tự hủy phiếu mượn {slip.BorrowCode}.",
                    link: "/BorrowSlips/MyBorrows",
                    createdBy: "Người dùng");

                return Json(new { success = true, message = "Đã hủy phiếu mượn thành công." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        private async Task<IActionResult> CancelBorrowDetailAsync(int borrowDetailId)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (!int.TryParse(userIdString, out int userId))
            {
                return Json(new { success = false, message = "Phiên đăng nhập hết hạn" });
            }

            var userRole = HttpContext.Session.GetString("Role") ?? "Reader";

            var borrowDetail = await _context.BorrowDetails
                .Include(d => d.BorrowSlips)
                .ThenInclude(bs => bs!.BorrowDetails)
                .Include(d => d.Books)
                .FirstOrDefaultAsync(d => d.Id == borrowDetailId);

            if (borrowDetail == null)
                return Json(new { success = false, message = "Không tìm thấy chi tiết phiếu mượn" });

            if (borrowDetail.BorrowSlips == null)
                return Json(new { success = false, message = "Phiếu mượn không hợp lệ" });

            var isOwner = borrowDetail.BorrowSlips.ReaderId == userId;
            var isStaff = userRole == "Admin" || userRole == "Librarian";

            if (!isOwner && !isStaff)
                return Json(new { success = false, message = "Bạn không có quyền hủy yêu cầu này" });

            if (borrowDetail.BorrowSlips.BorrowStatus != BorrowStatus.PendingApproval && borrowDetail.BorrowSlips.BorrowStatus != BorrowStatus.ApprovedAwaitingPickup)
                return Json(new { success = false, message = "Không thể hủy phiếu đã được giao sách!" });

            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                _context.BorrowDetails.Remove(borrowDetail);

                // DO NOT restore inventory - it was never deducted for PendingApproval/ApprovedAwaitingPickup

                var remainingBooks = borrowDetail.BorrowSlips.BorrowDetails!.Count - 1;
                if (remainingBooks <= 0)
                {
                    borrowDetail.BorrowSlips.BorrowStatus = BorrowStatus.Cancelled;
                    borrowDetail.BorrowSlips.TotalBooks = 0;
                }
                else
                {
                    borrowDetail.BorrowSlips.TotalBooks = remainingBooks;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                await _notificationService.NotifyUserAsync(
                    borrowDetail.BorrowSlips.ReaderId,
                    "Reader",
                    NotificationType.BorrowCancelled,
                    "Đã hủy cuốn sách khỏi phiếu mượn",
                    remainingBooks <= 0
                        ? $"Đã hủy phiếu mượn {borrowDetail.BorrowSlips.BorrowCode}."
                        : $"Đã hủy sách \"{borrowDetail.Books?.Title}\" khỏi phiếu mượn {borrowDetail.BorrowSlips.BorrowCode}.",
                    link: "/BorrowSlips/MyBorrows",
                    createdBy: isStaff ? (HttpContext.Session.GetString("UserName") ?? "Thủ thư") : "Người dùng");

                return Json(new
                {
                    success = true,
                    message = remainingBooks <= 0
                        ? "Đã hủy phiếu mượn thành công"
                        : "Đã hủy cuốn sách khỏi phiếu mượn"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // Legacy form POST support for CancelRequest
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize]
        public async Task<IActionResult> CancelRequestForm(int borrowDetailId)
        {
            return await CancelBorrowDetailAsync(borrowDetailId);
        }

        // POST: BorrowSlips/ConvertToReservation/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize(Roles = "Admin,Librarian")]
        public async Task<IActionResult> ConvertToReservation(int id)
        {
            var slip = await _context.BorrowSlips
                .Include(x => x.BorrowDetails!)
                .ThenInclude(x => x.Books)
                .Include(x => x.Readers)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (slip == null)
                return Json(new { success = false, message = "Không tìm thấy phiếu mượn" });

            if (slip.BorrowStatus != BorrowStatus.PendingApproval)
                return Json(new { success = false, message = "Chỉ có thể chuyển đổi phiếu mượn đang chờ duyệt" });

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Create reservations for each book in the slip
                foreach (var detail in slip.BorrowDetails)
                {
                    if (detail.Books != null)
                    {
                        var reservation = new Reservations
                        {
                            ReaderId = slip.ReaderId,
                            BookId = detail.BookId,
                            ReservationDate = DateTime.Now,
                            ExpirationDate = DateTime.Now.AddDays(7), // 7 days reservation period
                            Status = "Waiting",
                            BooksStatus = ReservationsStatus.Waiting,
                            Note = $"Tự động chuyển đổi từ phiếu mượn #{slip.BorrowCode}"
                        };
                        _context.Reservations.Add(reservation);
                    }
                }

                // Cancel the original borrow slip
                slip.BorrowStatus = BorrowStatus.Cancelled;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var senderName = HttpContext.Session.GetString("UserName") ?? "Thủ thư";
                await _notificationService.NotifyUserAsync(
                    slip.ReaderId,
                    "Reader",
                    NotificationType.ReservationCreated,
                    "Phiếu mượn đã chuyển thành đặt trước",
                    $"Phiếu mượn {slip.BorrowCode} đã được chuyển thành {slip.BorrowDetails.Count} đặt trước sách.",
                    link: "/Reservations/MyReservations",
                    createdBy: senderName);

                return Json(new { success = true, message = $"Đã chuyển đổi phiếu mượn sang {slip.BorrowDetails.Count} đặt trước" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // POST: BorrowSlips/RejectDueToNoStock/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize(Roles = "Admin,Librarian")]
        public async Task<IActionResult> RejectDueToNoStock(int id)
        {
            var slip = await _context.BorrowSlips
                .Include(x => x.BorrowDetails)
                .ThenInclude(x => x.Books)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (slip == null)
                return Json(new { success = false, message = "Không tìm thấy phiếu mượn" });

            if (slip.BorrowStatus != BorrowStatus.PendingApproval)
                return Json(new { success = false, message = "Chỉ có thể hủy phiếu mượn đang chờ duyệt" });

            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                
                slip.BorrowStatus = BorrowStatus.Cancelled;
                slip.Note = (slip.Note ?? "") + " [Đã từ chối do hết sách]";

                // DO NOT restore inventory - it was never deducted for PendingApproval

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var senderName = HttpContext.Session.GetString("UserName") ?? "Thủ thư";
                await _notificationService.NotifyUserAsync(
                    slip.ReaderId,
                    "Reader",
                    NotificationType.BorrowRejected,
                    "Phiếu mượn bị từ chối do hết sách",
                    $"Phiếu mượn {slip.BorrowCode} đã bị từ chối vì sách hiện đã hết.",
                    link: "/BorrowSlips/MyBorrows",
                    createdBy: senderName);

                return Json(new { success = true, message = "Đã từ chối phiếu mượn do hết sách" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // GET: BorrowSlips/GetBorrowSlipBooks/{id}
        [HttpGet]
        [SessionAuthorize(Roles = "Admin,Librarian")]
        public async Task<IActionResult> GetBorrowSlipBooks(int id)
        {
            var slip = await _context.BorrowSlips
                .Include(x => x.BorrowDetails!)
                .ThenInclude(x => x.Books)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (slip == null)
                return Json(new { success = false, message = "Không tìm thấy phiếu mượn" });

            var books = slip.BorrowDetails!
                .Where(bd => !bd.IsReturned && bd.Books != null)
                .Select(bd => new
                {
                    id = bd.Id,
                    title = bd.Books!.Title,
                    price = bd.Books.Price
                })
                .ToList();

            return Json(new { success = true, books = books });
        }

        // POST: BorrowSlips/ConfirmReturnBook/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize(Roles = "Admin,Librarian")]
        [PermissionAuthorize("borrow.confirm_return")]
        public async Task<IActionResult> ConfirmReturnBook(int id, string condition = "good", string? bookData = null)
        {
            var slip = await _context.BorrowSlips
                .FirstOrDefaultAsync(x => x.Id == id);

            if (slip == null)
                return Json(new { success = false, message = "Không tìm thấy phiếu mượn" });

            if (slip.BorrowStatus != BorrowStatus.Borrowing)
                return Json(new { success = false, message = "Phiếu mượn không ở trạng thái đang mượn" });

            // Parse book data if provided
            List<BookDamageData>? bookDamageList = null;
            if (!string.IsNullOrEmpty(bookData))
            {
                try
                {
                    bookDamageList = System.Text.Json.JsonSerializer.Deserialize<List<BookDamageData>>(bookData);
                }
                catch
                {
                    return Json(new { success = false, message = "Dữ liệu sách không hợp lệ" });
                }
            }

            // Build return request DTO
            var request = new ReturnBookRequestDto
            {
                SlipId = id,
                OverallCondition = condition
            };

            // Add return items if book data is provided
            if (bookDamageList != null)
            {
                foreach (var bookDataItem in bookDamageList)
                {
                    request.ReturnItems.Add(new ReturnItemDto
                    {
                        BorrowDetailId = bookDataItem.BorrowDetailId,
                        Condition = condition,
                        DamagePercent = bookDataItem.DamagePercent,
                        DamageReason = bookDataItem.DamageNote
                    });
                }
            }

            // Use centralized return logic
            var (success, message, totalFine) = await ProcessBookReturnAsync(id, request);

            return Json(new { 
                success = success, 
                message = message,
                status = success ? "Returned" : "Error",
                fineAmount = totalFine
            });
        }

        // Helper class for book damage data
        private class BookDamageData
        {
            public int BorrowDetailId { get; set; }
            public int? DamagePercent { get; set; }
            public string? DamageNote { get; set; }
        }

        // ========================= CENTRALIZED RETURN BOOK LOGIC =========================
        // Single method for processing book returns with fine creation
        
        private async Task<(bool Success, string Message, decimal TotalFine)> ProcessBookReturnAsync(
            int slipId, 
            ReturnBookRequestDto request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var slip = await _context.BorrowSlips
                    .Include(x => x.BorrowDetails!)
                    .ThenInclude(x => x.Books)
                    .FirstOrDefaultAsync(x => x.Id == slipId);

                if (slip == null)
                    return (false, "Không tìm thấy phiếu mượn", 0);

                if (slip.BorrowStatus != BorrowStatus.Borrowing)
                    return (false, "Phiếu mượn không ở trạng thái đang mượn", 0);

                var actualReturnDate = DateTime.Now;
                var isOverdue = actualReturnDate > slip.DueDate;
                int lateDays = isOverdue ? (actualReturnDate - slip.DueDate).Days : 0;
                decimal totalFineAmount = 0;
                List<string> aggregatedNotes = new List<string>();
                int? primaryDetailId = null;

                // Process each book in the slip
                foreach (var detail in slip.BorrowDetails ?? Enumerable.Empty<BorrowDetails>())
                {
                    if (detail.Books == null)
                        continue;

                    // Find the return item for this detail
                    var returnItem = request.ReturnItems.FirstOrDefault(ri => ri.BorrowDetailId == detail.Id);
                    string bookCondition = request.OverallCondition;
                    int? bookDamagePercent = null;
                    string? damageReason = null;

                    // If specific return item data is provided
                    if (returnItem != null)
                    {
                        bookCondition = returnItem.Condition;
                        bookDamagePercent = returnItem.DamagePercent;
                        damageReason = returnItem.DamageReason;
                    }

                    // Calculate fine for this book
                    decimal bookFineAmount = 0;
                    if (bookCondition == "damaged" || bookCondition == "lost" || isOverdue)
                    {
                        var fine = await _fineCalculationService.CalculateAndCreateFineAsync(
                            detail.Id, bookCondition, bookDamagePercent);
                        
                        if (fine != null)
                        {
                            bookFineAmount = fine.Amount;
                            totalFineAmount += bookFineAmount;
                            
                            // Add to aggregated notes
                            if (bookCondition == "lost")
                            {
                                aggregatedNotes.Add($"{detail.Books.Title}: Mất sách ({bookFineAmount:N0} VNĐ)");
                            }
                            else if (bookCondition == "damaged" && bookDamagePercent.HasValue)
                            {
                                var reason = !string.IsNullOrEmpty(damageReason) ? damageReason : "Không có chi tiết";
                                aggregatedNotes.Add($"{detail.Books.Title}: Hư hại {bookDamagePercent}% ({reason}) - {bookFineAmount:N0} VNĐ");
                            }
                            else if (isOverdue)
                            {
                                aggregatedNotes.Add($"{detail.Books.Title}: Trễ hạn ({bookFineAmount:N0} VNĐ)");
                            }
                            
                            if (primaryDetailId == null)
                                primaryDetailId = detail.Id;
                            
                            // Sync fine amount to borrow detail for history display
                            detail.CompensationAmount = bookFineAmount;
                        }
                    }

                    // Update borrow detail
                    detail.IsReturned = true;
                    detail.ReturnedDate = actualReturnDate;
                    detail.IsLost = (bookCondition == "lost");
                    detail.IsDamaged = (bookCondition == "damaged");
                    detail.DamagePercent = bookDamagePercent;
                    
                    if (bookCondition == "damaged" && bookDamagePercent.HasValue)
                    {
                        var reason = !string.IsNullOrEmpty(damageReason) ? damageReason : "Không có chi tiết";
                        detail.ConditionAfter = $"Hư hại {bookDamagePercent}% ({reason})";
                    }
                    else if (bookCondition == "lost")
                    {
                        detail.ConditionAfter = "Mất sách";
                    }
                    else
                    {
                        detail.ConditionAfter = "Nguyên vẹn";
                    }

                    // Update book status and inventory
                    if (bookCondition == "lost")
                    {
                        detail.Books.Quantity = Math.Max(0, detail.Books.Quantity - 1);
                    }
                    else
                    {
                        detail.Books.AvailableQuantity += 1;
                    }
                }

                // Create single master fine record if total fine > 0
                if (totalFineAmount > 0 && primaryDetailId.HasValue)
                {
                    var masterFine = new Fines
                    {
                        BorrowSlipId = slipId,
                        ReaderId = slip.ReaderId,
                        BorrowDetailId = primaryDetailId.Value,
                        Amount = totalFineAmount,
                        Reason = string.Join(" | ", aggregatedNotes),
                        Status = "Unpaid",
                        IsPaid = false,
                        CreatedDate = DateTime.Now,
                        FinesType = isOverdue ? Models.Enums.FinesType.Late : Models.Enums.FinesType.Lost
                    };
                    
                    if (request.OverallCondition == "lost")
                    {
                        masterFine.FinesType = Models.Enums.FinesType.Lost;
                        masterFine.LostFine = totalFineAmount;
                    }
                    else if (request.OverallCondition == "damaged")
                    {
                        masterFine.DamageFine = totalFineAmount;
                    }
                    else if (isOverdue)
                    {
                        masterFine.LateFine = totalFineAmount;
                        masterFine.LateDays = lateDays;
                    }
                    
                    _context.Fines.Add(masterFine);
                }

                // Update slip status
                slip.ReturnDate = actualReturnDate;
                
                if (totalFineAmount > 0)
                {
                    slip.BorrowStatus = BorrowStatus.LostOrDamaged;
                }
                else
                {
                    slip.BorrowStatus = BorrowStatus.Returned;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                string fineMessage;
                if (totalFineAmount > 0)
                {
                    fineMessage = $"Trả sách thành công! Tổng tiền phạt: {totalFineAmount:N0} VNĐ.";
                    if (lateDays > 0)
                    {
                        fineMessage += $" (Trễ {lateDays} ngày)";
                    }
                }
                else
                {
                    fineMessage = "Đã xác nhận trả sách thành công";
                }

                return (true, fineMessage, totalFineAmount);
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return (false, $"Lỗi cơ sở dữ liệu: {innerMessage}", 0);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Lỗi: {ex.Message}", 0);
            }
        }

        // GET: BORROWSLIPSS/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var borrowslips = await _context.BorrowSlips.FindAsync(id);
            if (borrowslips == null)
            {
                return NotFound();
            }
            return View(borrowslips);
        }

        // POST: BORROWSLIPSS/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BorrowSlips borrowslips)
        {
            if (id != borrowslips.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(borrowslips);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BorrowSlipsExists(borrowslips.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(borrowslips);
        }

        // GET: BORROWSLIPSS/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var borrowslips = await _context.BorrowSlips
                .Include(x => x.Readers)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (borrowslips == null)
            {
                return NotFound();
            }

            return View(borrowslips);
        }

        // POST: BORROWSLIPSS/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var borrowslips = await _context.BorrowSlips.FindAsync(id);
            if (borrowslips != null)
            {
                _context.BorrowSlips.Remove(borrowslips);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BorrowSlipsExists(int id)
        {
            return _context.BorrowSlips.Any(e => e.Id == id);
        }
        public async Task<IActionResult> MyBorrows(
            string? searchString,
            string? statusFilter,
            string? sortOrder)
        {
            var userIdString = HttpContext.Session.GetString("UserId");

            if (!int.TryParse(userIdString, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var currentBorrows = await _borrowService.GetCurrentBorrowsAsync(userId);
            var history = await _borrowService.GetBorrowHistoryAsync(userId);

            // Check if user has unpaid fines
            bool hasUnpaidFines = await _context.Fines
                .AnyAsync(f => f.ReaderId == userId && 
                              (f.Status == "Unpaid" || f.Status == "Chưa thanh toán" || f.Status == "Chờ xử lý"));

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var searchLower = searchString.ToLower();
                currentBorrows = currentBorrows
                    .Where(b => b.BookTitle != null && b.BookTitle.ToLower().Contains(searchLower))
                    .ToList();
                history = history
                    .Where(h => h.BookTitle != null && h.BookTitle.ToLower().Contains(searchLower))
                    .ToList();
            }

            // Apply status filter
            if (!string.IsNullOrWhiteSpace(statusFilter))
            {
                if (statusFilter == "overdue")
                {
                    currentBorrows = currentBorrows.Where(b => b.Status == "Quá hạn").ToList();
                    history = history.Where(h => h.IsLate).ToList();
                }
                else if (statusFilter == "active")
                {
                    currentBorrows = currentBorrows.Where(b => b.Status != "Quá hạn" && b.Status != "Đã hủy").ToList();
                }
                else if (statusFilter == "returned")
                {
                    // Only show history
                    currentBorrows = new List<SOF1022_NHOM1_SD2003_Ver2.Models.DTOs.BorrowItemDto>();
                }
            }

            // Apply sorting
            switch (sortOrder)
            {
                case "borrow_date_asc":
                    currentBorrows = currentBorrows.OrderBy(b => b.BorrowDate).ToList();
                    history = history.OrderBy(h => h.BorrowDate).ToList();
                    break;
                case "borrow_date_desc":
                    currentBorrows = currentBorrows.OrderByDescending(b => b.BorrowDate).ToList();
                    history = history.OrderByDescending(h => h.BorrowDate).ToList();
                    break;
                case "due_date_asc":
                    currentBorrows = currentBorrows.OrderBy(b => b.DueDate).ToList();
                    break;
                case "due_date_desc":
                    currentBorrows = currentBorrows.OrderByDescending(b => b.DueDate).ToList();
                    break;
                case "name_asc":
                    currentBorrows = currentBorrows.OrderBy(b => b.BookTitle).ToList();
                    history = history.OrderBy(h => h.BookTitle).ToList();
                    break;
                case "name_desc":
                    currentBorrows = currentBorrows.OrderByDescending(b => b.BookTitle).ToList();
                    history = history.OrderByDescending(h => h.BookTitle).ToList();
                    break;
                default:
                    currentBorrows = currentBorrows.OrderByDescending(b => b.BorrowDate).ToList();
                    history = history.OrderByDescending(h => h.BorrowDate).ToList();
                    break;
            }

            var vm = new MyBorrowViewModel
            {
                CurrentBorrows = currentBorrows,
                History = history,
                HasUnpaidFines = hasUnpaidFines
            };

            // Preserve filter values in ViewBag
            ViewBag.SearchString = searchString;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.SortOrder = sortOrder;

            return View(vm);
        }

        // GET: BorrowSlips/BookingSuccess
        [SessionAuthorize]
        public IActionResult BookingSuccess(string type = "Booking")
        {
            var viewModel = new SuccessViewModel
            {
                Timestamp = DateTime.Now
            };

            switch (type.ToLower())
            {
                case "booking":
                    viewModel.Title = "Đặt trước thành công!";
                    viewModel.Message = "Yêu cầu mượn sách của bạn đã được ghi nhận. Vui lòng chờ thủ thư phê duyệt.";
                    viewModel.RedirectUrl = "/BorrowSlips/MyBorrows";
                    viewModel.ButtonText = "Xem lịch sử mượn";
                    viewModel.ShowHistoryButton = true;
                    break;
                case "payment":
                    viewModel.Title = "Thanh toán thành công!";
                    viewModel.Message = "Yêu cầu thanh toán của bạn đã được ghi nhận. Vui lòng chờ xác nhận từ thủ thư.";
                    viewModel.RedirectUrl = "/Fines/MyFines";
                    viewModel.ButtonText = "Xem khoản phạt";
                    viewModel.ShowHistoryButton = true;
                    break;
                case "cancel":
                    viewModel.Title = "Đã hủy thành công!";
                    viewModel.Message = "Yêu cầu mượn của bạn đã được hủy.";
                    viewModel.RedirectUrl = "/BorrowSlips/MyBorrows";
                    viewModel.ButtonText = "Xem lịch sử mượn";
                    viewModel.ShowHistoryButton = true;
                    break;
                default:
                    viewModel.Title = "Thành công!";
                    viewModel.Message = "Thao tác của bạn đã được thực hiện thành công.";
                    viewModel.RedirectUrl = "/";
                    viewModel.ButtonText = "Về trang chủ";
                    break;
            }

            return View(viewModel);
        }

        // ========================= ADMIN REJECT RESERVATION =========================
        // Reject a pending reservation (Admin side) with reason

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize(Roles = "Admin,Librarian")]
        public async Task<IActionResult> RejectReservation(int id, string reason)
        {
            try
            {
                var slip = await _context.BorrowSlips
                    .Include(s => s.BorrowDetails)
                    .ThenInclude(bd => bd.Books)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (slip == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy phiếu mượn." });
                }

                // Only allow rejection for pending status
                if (slip.BorrowStatus != BorrowStatus.PendingApproval)
                {
                    return Json(new { success = false, message = "Chỉ có thể từ chối phiếu đang chờ duyệt." });
                }

                // Update status to cancelled/rejected
                slip.BorrowStatus = BorrowStatus.Cancelled;
                slip.Note = string.IsNullOrWhiteSpace(reason) ? "Từ chối duyệt bởi thủ thư" : reason;

                // DO NOT restore inventory - it was never deducted for PendingApproval

                await _context.SaveChangesAsync();

                var senderName = HttpContext.Session.GetString("UserName") ?? "Thủ thư";
                await _notificationService.NotifyUserAsync(
                    slip.ReaderId,
                    "Reader",
                    NotificationType.BorrowRejected,
                    "Phiếu mượn bị từ chối",
                    $"Phiếu mượn {slip.BorrowCode} đã bị từ chối bởi thủ thư. Lý do: {slip.Note}.",
                    link: "/BorrowSlips/MyBorrows",
                    createdBy: senderName);

                return Json(new 
                { 
                    success = true, 
                    message = $"Đã từ chối duyệt phiếu mượn. Lý do: {slip.Note}." 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi: " + ex.Message });
            }
        }

        // ========================= READER CANCEL RESERVATION =========================
        // Cancel a pending reservation (Reader side)

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelReservation(int detailId)
        {
            try
            {
                var borrowDetail = await _context.BorrowDetails
                    .Include(bd => bd.BorrowSlips)
                    .Include(bd => bd.Books)
                    .FirstOrDefaultAsync(bd => bd.Id == detailId);

                if (borrowDetail == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin mượn sách." });
                }

                // Only allow cancellation for pending status
                if (borrowDetail.BorrowSlips?.BorrowStatus != BorrowStatus.PendingApproval)
                {
                    return Json(new { success = false, message = "Chỉ có thể hủy yêu cầu đang chờ duyệt." });
                }

                // Update status to cancelled
                borrowDetail.BorrowSlips.BorrowStatus = BorrowStatus.Cancelled;

                // DO NOT restore inventory - it was never deducted for PendingApproval

                await _context.SaveChangesAsync();

                await _notificationService.NotifyUserAsync(
                    borrowDetail.BorrowSlips.ReaderId,
                    "Reader",
                    NotificationType.BorrowCancelled,
                    "Đã hủy yêu cầu đặt sách",
                    $"Bạn đã hủy yêu cầu đặt sách \"{borrowDetail.Books?.Title}\".",
                    link: "/BorrowSlips/MyBorrows",
                    createdBy: "Người dùng");

                return Json(new 
                { 
                    success = true, 
                    message = "Đã hủy yêu cầu đặt sách thành công!" 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi: " + ex.Message });
            }
        }

        // ========================= REQUEST EXTENSION =========================
        // Request extension approval (Reader side)

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestExtension(int detailId, int requestedDays, string reason)
        {
            try
            {
                var userIdString = HttpContext.Session.GetString("UserId");
                if (!int.TryParse(userIdString, out int userId))
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập để thực hiện thao tác này." });
                }

                // Check if user has unpaid fines
                bool hasUnpaidFines = await _context.Fines
                    .AnyAsync(f => f.ReaderId == userId && 
                                  (f.Status == "Unpaid" || f.Status == "Chưa thanh toán" || f.Status == "Chờ xử lý"));
                
                if (hasUnpaidFines)
                {
                    return Json(new { success = false, message = "Bạn không thể gia hạn sách khi đang có khoản phạt chưa thanh toán. Vui lòng thanh toán trước!" });
                }

                // Use service method for validation
                var result = await _borrowService.RequestExtensionAsync(detailId, requestedDays, reason);

                return Json(new 
                { 
                    success = result.Success, 
                    message = result.Message 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi: " + ex.Message });
            }
        }

        // ========================= APPROVE EXTENSION =========================
        // Approve extension request (Admin side)

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize(Roles = "Admin,Librarian")]
        public async Task<IActionResult> ApproveExtension(int detailId, int extensionDays = 7)
        {
            try
            {
                var borrowDetail = await _context.BorrowDetails
                    .Include(bd => bd.Books)
                    .Include(bd => bd.BorrowSlips)
                    .FirstOrDefaultAsync(bd => bd.Id == detailId);

                if (borrowDetail == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin mượn sách." });
                }

                if (borrowDetail.BorrowSlips.BorrowStatus != BorrowStatus.PendingExtension)
                {
                    return Json(new { success = false, message = "Phiếu mượn không ở trạng thái chờ duyệt gia hạn." });
                }

                var dueDate = borrowDetail.DueDate ?? borrowDetail.BorrowSlips.DueDate;
                if (dueDate == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy hạn trả sách." });
                }

                // Validate extension days
                if (extensionDays < 1 || extensionDays > 14)
                {
                    return Json(new { success = false, message = "Số ngày gia hạn phải từ 1 đến 14 ngày." });
                }

                // Rule A: Check if book has active reservations from other readers
                var hasReservation = await _context.Reservations
                    .AnyAsync(r => r.BookId == borrowDetail.BookId 
                                   && r.ReaderId != borrowDetail.BorrowSlips.ReaderId
                                   && (r.Status == "Waiting" || r.Status == "Pending" || r.Status == "Chờ duyệt"));

                if (hasReservation)
                {
                    return Json(new { success = false, message = "Không thể duyệt. Sách này đã có độc giả khác đặt lịch mượn tiếp theo." });
                }

                // Rule B: Check if reader has any overdue books
                var hasOverdueBooks = await _context.BorrowDetails
                    .Include(bd => bd.BorrowSlips)
                    .AnyAsync(bd => bd.BorrowSlips.ReaderId == borrowDetail.BorrowSlips.ReaderId
                                   && bd.BorrowSlips.BorrowStatus == BorrowStatus.Borrowing
                                   && bd.DueDate < DateTime.Now
                                   && bd.Id != borrowDetail.Id);

                if (hasOverdueBooks)
                {
                    return Json(new { success = false, message = "Không thể duyệt. Độc giả này đang có sách quá hạn chưa hoàn trả." });
                }

                // Rule C: Check extension count limit
                var settings = await SystemSettingsController.GetCurrentSettings(_context);
                if (borrowDetail.ExtensionCount >= settings.MaxExtensionCount)
                {
                    return Json(new { success = false, message = $"Không thể duyệt. Độc giả đã đạt giới hạn số lần gia hạn ({settings.MaxExtensionCount} lần)." });
                }

                // Apply extension with admin-specified days
                borrowDetail.DueDate = dueDate.AddDays(extensionDays);
                borrowDetail.ExtensionCount += 1;
                borrowDetail.RequestedExtensionDays = null;
                borrowDetail.ExtensionReason = null;
                borrowDetail.BorrowSlips.BorrowStatus = BorrowStatus.Borrowing;

                await _context.SaveChangesAsync();

                var senderName = HttpContext.Session.GetString("UserName") ?? "Thủ thư";
                await _notificationService.NotifyUserAsync(
                    borrowDetail.BorrowSlips.ReaderId,
                    "Reader",
                    NotificationType.ExtensionApproved,
                    "Yêu cầu gia hạn được duyệt",
                    $"Yêu cầu gia hạn cho sách \"{borrowDetail.Books?.Title}\" đã được duyệt {extensionDays} ngày. Hạn trả mới: {borrowDetail.DueDate.Value:dd/MM/yyyy}.",
                    link: "/BorrowSlips/MyBorrows",
                    createdBy: senderName);

                return Json(new 
                { 
                    success = true, 
                    message = $"Đã duyệt gia hạn {extensionDays} ngày. Hạn trả mới: {borrowDetail.DueDate.Value:dd/MM/yyyy}",
                    newDueDate = borrowDetail.DueDate.Value.ToString("dd/MM/yyyy")
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi: " + ex.Message });
            }
        }

        // ========================= CONFIRM RETURN =========================
        // Confirm book return with condition (normal or incident)

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize(Roles = "Admin,Librarian")]
        [PermissionAuthorize("borrow.confirm_return")]
        public async Task<IActionResult> ConfirmReturn(int id, string condition)
        {
            try
            {
                var borrowSlip = await _context.BorrowSlips
                    .Include(x => x.BorrowDetails)
                        .ThenInclude(bd => bd.Books)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (borrowSlip == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy phiếu mượn." });
                }

                if (borrowSlip.BorrowStatus != BorrowStatus.Borrowing)
                {
                    return Json(new { success = false, message = "Phiếu mượn không ở trạng thái đang mượn." });
                }

                switch (condition.ToLower())
                {
                    case "normal":
                        // Mark all books as returned
                        foreach (var detail in borrowSlip.BorrowDetails)
                        {
                            detail.IsReturned = true;
                            detail.ReturnedDate = DateTime.Now;
                            detail.IsLost = false;
                            detail.IsDamaged = false;
                            
                            // Restore book quantity
                            if (detail.Books != null)
                            {
                                detail.Books.AvailableQuantity += detail.Quantity;
                            }
                        }
                        
                        borrowSlip.BorrowStatus = BorrowStatus.Returned;
                        borrowSlip.ReturnDate = DateTime.Now;
                        break;

                    case "incident":
                        // Mark slip as incident for compensation processing
                        borrowSlip.BorrowStatus = BorrowStatus.LostOrDamaged;
                        
                        // Mark all books as lost/damaged for compensation
                        foreach (var detail in borrowSlip.BorrowDetails)
                        {
                            detail.IsLost = true;
                            detail.CompensationAmount = detail.Books?.Price ?? 0;
                        }
                        break;

                    default:
                        return Json(new { success = false, message = "Tình trạng không hợp lệ." });
                }

                await _context.SaveChangesAsync();

                return Json(new 
                { 
                    success = true, 
                    message = condition.ToLower() == "normal" 
                        ? "Đã xác nhận trả sách thành công. Số lượng sách đã được khôi phục." 
                        : "Đã đánh dấu sự cố. Vui lòng xử lý bồi thường." 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi: " + ex.Message });
            }
        }

        // ========================= PROCESS RETURN (PER-BOOK CONDITIONS) =========================
        // Handle return with per-book condition selection from refactored modal

        public class BookConditionDto
        {
            public int BookId { get; set; }
            public string Condition { get; set; } // good, damaged, lost
            public string? DamageReason { get; set; }
            public int? DamagePercent { get; set; }
            public string? DamageDetails { get; set; }
        }

        public class ProcessReturnRequest
        {
            public int BorrowSlipId { get; set; }
            public List<BookConditionDto> BookConditions { get; set; } = new();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize(Roles = "Admin,Librarian")]
        public async Task<IActionResult> ProcessReturn([FromBody] ProcessReturnRequest request)
        {
            try
            {
                var borrowSlip = await _context.BorrowSlips
                    .Include(bs => bs.BorrowDetails)
                        .ThenInclude(bd => bd.Books)
                    .Include(bs => bs.Readers)
                    .FirstOrDefaultAsync(bs => bs.Id == request.BorrowSlipId);

                if (borrowSlip == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy phiếu mượn." });
                }

                if (borrowSlip.BorrowStatus != BorrowStatus.Borrowing)
                {
                    return Json(new { success = false, message = "Phiếu mượn không ở trạng thái đang mượn." });
                }

                // Process each book according to its condition
                var returnItems = new List<(int BorrowDetailId, string Condition, int? DamagePercent, string? DamageReason, string? DamageDetails)>();
                
                foreach (var bookCondition in request.BookConditions)
                {
                    var borrowDetail = borrowSlip.BorrowDetails
                        .FirstOrDefault(bd => bd.BookId == bookCondition.BookId && !bd.IsReturned);

                    if (borrowDetail == null)
                    {
                        return Json(new { success = false, message = $"Không tìm thấy chi tiết mượn cho sách ID {bookCondition.BookId}." });
                    }

                    string condition = bookCondition.Condition switch
                    {
                        "good" => "Returned",
                        "damaged" => "Damaged",
                        "lost" => "Lost",
                        _ => "Returned"
                    };

                    returnItems.Add((borrowDetail.Id, condition, bookCondition.DamagePercent, bookCondition.DamageReason, bookCondition.DamageDetails));
                }

                // Use BorrowService to process the return with consolidated fine logic
                var (success, message) = await _borrowService.ReturnMultipleAsync(
                    borrowSlipId: request.BorrowSlipId, 
                    returnItems: returnItems
                );

                return Json(new 
                { 
                    success = true, 
                    message = "Đã xác nhận trả sách thành công." 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi: " + ex.Message });
            }
        }

        // ========================= UPDATE BOOK STATUS =========================
        // Update individual book status (Returned, Lost, Damaged) for partial return handling

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize(Roles = "Admin,Librarian")]
        public async Task<IActionResult> UpdateBookStatus(int detailId, string status, int? fineAmount = null, string? fineNote = null, int? damagePercentage = null)
        {
            try
            {
                var borrowDetail = await _context.BorrowDetails
                    .Include(bd => bd.BorrowSlips)
                    .Include(bd => bd.Books)
                    .FirstOrDefaultAsync(bd => bd.Id == detailId);

                if (borrowDetail == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy chi tiết mượn sách." });
                }

                var slipId = borrowDetail.BorrowSlipId;

                // Build return request DTO
                var request = new ReturnBookRequestDto
                {
                    SlipId = slipId,
                    OverallCondition = status
                };

                // Add return item for this specific detail
                request.ReturnItems.Add(new ReturnItemDto
                {
                    BorrowDetailId = detailId,
                    Condition = status,
                    DamagePercent = damagePercentage,
                    DamageReason = fineNote,
                    CalculatedFine = fineAmount ?? 0
                });

                // Use centralized return logic
                var (success, message, totalFine) = await ProcessBookReturnAsync(slipId, request);

                return Json(new 
                { 
                    success = success, 
                    message = message
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi: " + ex.Message });
            }
        }

        // ========================= UPDATE DATES =========================
        // Update borrow date and due date for a borrow slip

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize(Roles = "Admin,Librarian")]
        public async Task<IActionResult> UpdateDates(int borrowSlipId, DateTime borrowDate, DateTime dueDate)
        {
            try
            {
                var borrowSlip = await _context.BorrowSlips
                    .FirstOrDefaultAsync(bs => bs.Id == borrowSlipId);

                if (borrowSlip == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy phiếu mượn." });
                }

                // Validate dates
                if (borrowDate > dueDate)
                {
                    return Json(new { success = false, message = "Ngày mượn không được lớn hơn hạn trả." });
                }

                if (borrowSlip.BorrowStatus != BorrowStatus.Borrowing && borrowSlip.BorrowStatus != BorrowStatus.PendingApproval)
                {
                    return Json(new { success = false, message = "Chỉ có thể chỉnh sửa ngày tháng khi phiếu đang mượn hoặc chờ duyệt." });
                }

                borrowSlip.BorrowDate = borrowDate;
                borrowSlip.DueDate = dueDate;

                await _context.SaveChangesAsync();

                return Json(new 
                { 
                    success = true, 
                    message = "Đã cập nhật ngày tháng thành công." 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi: " + ex.Message });
            }
        }

        // ========================= CHECK EXTENSION VALIDATION STATUS =========================
        // Check validation status for extension request (Admin side)

        [HttpGet]
        [SessionAuthorize(Roles = "Admin,Librarian")]
        public async Task<IActionResult> CheckExtensionValidation(int detailId)
        {
            try
            {
                var borrowDetail = await _context.BorrowDetails
                    .Include(bd => bd.Books)
                    .Include(bd => bd.BorrowSlips)
                    .FirstOrDefaultAsync(bd => bd.Id == detailId);

                if (borrowDetail == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin mượn sách." });
                }

                var validationIssues = new List<string>();
                var canApprove = true;

                // Rule A: Check if book has active reservations from other readers
                var hasReservation = await _context.Reservations
                    .AnyAsync(r => r.BookId == borrowDetail.BookId 
                                   && r.ReaderId != borrowDetail.BorrowSlips.ReaderId
                                   && (r.Status == "Waiting" || r.Status == "Pending" || r.Status == "Chờ duyệt"));

                if (hasReservation)
                {
                    validationIssues.Add("Có người đặt trước");
                    canApprove = false;
                }

                // Rule B: Check if reader has any overdue books
                var hasOverdueBooks = await _context.BorrowDetails
                    .Include(bd => bd.BorrowSlips)
                    .AnyAsync(bd => bd.BorrowSlips.ReaderId == borrowDetail.BorrowSlips.ReaderId
                                   && bd.BorrowSlips.BorrowStatus == BorrowStatus.Borrowing
                                   && bd.DueDate < DateTime.Now
                                   && bd.Id != borrowDetail.Id);

                if (hasOverdueBooks)
                {
                    validationIssues.Add("Độc giả dính lỗi");
                    canApprove = false;
                }

                // Rule C: Check extension count limit
                var settings = await SystemSettingsController.GetCurrentSettings(_context);
                if (borrowDetail.ExtensionCount >= settings.MaxExtensionCount)
                {
                    validationIssues.Add($"Đã đạt giới hạn ({settings.MaxExtensionCount} lần)");
                    canApprove = false;
                }

                return Json(new 
                { 
                    success = true, 
                    canApprove = canApprove,
                    validationIssues = validationIssues,
                    message = canApprove ? "Hợp lệ" : string.Join(", ", validationIssues)
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi: " + ex.Message });
            }
        }

        // ========================= REJECT EXTENSION =========================
        // Reject extension request (Admin side)

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize(Roles = "Admin,Librarian")]
        public async Task<IActionResult> RejectExtension(int detailId, string reason)
        {
            try
            {
                var borrowDetail = await _context.BorrowDetails
                    .Include(bd => bd.BorrowSlips)
                    .FirstOrDefaultAsync(bd => bd.Id == detailId);

                if (borrowDetail == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin mượn sách." });
                }

                if (borrowDetail.BorrowSlips.BorrowStatus != BorrowStatus.PendingExtension)
                {
                    return Json(new { success = false, message = "Phiếu mượn không ở trạng thái chờ duyệt gia hạn." });
                }

                // Reset status to Borrowing without changing due date
                borrowDetail.BorrowSlips.BorrowStatus = BorrowStatus.Borrowing;
                borrowDetail.RequestedExtensionDays = null;
                borrowDetail.ExtensionReason = null;
                
                if (!string.IsNullOrWhiteSpace(reason))
                {
                    borrowDetail.Note = $"Yêu cầu gia hạn bị từ chối: {reason}";
                }

                await _context.SaveChangesAsync();

                var senderName = HttpContext.Session.GetString("UserName") ?? "Thủ thư";
                await _notificationService.NotifyUserAsync(
                    borrowDetail.BorrowSlips.ReaderId,
                    "Reader",
                    NotificationType.ExtensionRejected,
                    "Yêu cầu gia hạn bị từ chối",
                    $"Yêu cầu gia hạn đã bị từ chối bởi thủ thư. Lý do: {reason ?? "Không có lý do cụ thể"}.",
                    link: "/BorrowSlips/MyBorrows",
                    createdBy: senderName);

                return Json(new 
                { 
                    success = true, 
                    message = "Đã từ chối yêu cầu gia hạn." 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi: " + ex.Message });
            }
        }

        // ========================= EXTEND LOAN =========================
        // Extend loan period by 7 days (max 1 extension per book)

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExtendLoan(int detailId)
        {
            try
            {
                var borrowDetail = await _context.BorrowDetails
                    .Include(bd => bd.Books)
                    .Include(bd => bd.BorrowSlips)
                    .FirstOrDefaultAsync(bd => bd.Id == detailId);

                if (borrowDetail == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin mượn sách." });
                }

                // Check if already returned
                if (borrowDetail.IsReturned)
                {
                    return Json(new { success = false, message = "Sách này đã được trả, không thể gia hạn." });
                }

                // Check if already overdue
                var dueDate = borrowDetail.DueDate ?? borrowDetail.BorrowSlips?.DueDate;
                if (dueDate == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy hạn trả sách." });
                }

                if (DateTime.Now > dueDate)
                {
                    return Json(new { success = false, message = "Sách đã quá hạn, không thể gia hạn." });
                }

                // Check if max extensions reached (1 extension allowed)
                if (borrowDetail.ExtensionCount >= 1)
                {
                    return Json(new { success = false, message = "Đã đạt giới hạn gia hạn (1 lần)." });
                }

                // Check if book is reserved by another reader
                var isReserved = await _context.Reservations
                    .AnyAsync(br => br.BookId == borrowDetail.BookId 
                                    && br.ReaderId != borrowDetail.BorrowSlips.ReaderId
                                    && (br.Status == "Waiting" || br.Status == "Pending"));

                if (isReserved)
                {
                    return Json(new { success = false, message = "Sách đang được đặt bởi độc giả khác, không thể gia hạn." });
                }

                // Extend loan by 7 days
                borrowDetail.DueDate = dueDate.Value.AddDays(7);
                borrowDetail.ExtensionCount += 1;

                await _context.SaveChangesAsync();

                return Json(new 
                { 
                    success = true, 
                    message = "Gia hạn thành công!",
                    newDueDate = borrowDetail.DueDate.Value.ToString("dd/MM/yyyy"),
                    extensionCount = borrowDetail.ExtensionCount
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi: " + ex.Message });
            }
        }

        [HttpGet]
        [SessionAuthorize]
        public async Task<IActionResult> PrintSlip(int id)
        {
            var borrowSlip = await _context.BorrowSlips
                .Include(b => b.Readers)
                .Include(b => b.BorrowDetails)
                    .ThenInclude(d => d.Books)
                        .ThenInclude(bk => bk.Author)
                .Include(b => b.BorrowDetails)
                    .ThenInclude(d => d.Books)
                        .ThenInclude(bk => bk.Category)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (borrowSlip == null)
            {
                return NotFound();
            }

            var userRole = HttpContext.Session.GetString("UserRole");
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (userRole == "Reader" && borrowSlip.ReaderId != currentUserId)
            {
                return Forbid();
            }

            return View(borrowSlip);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize]
        public async Task<IActionResult> RenewSlip(int slipId)
        {
            var slip = await _context.BorrowSlips
                .Include(s => s.BorrowDetails)
                    .ThenInclude(d => d.Books)
                .Include(s => s.Readers)
                .FirstOrDefaultAsync(s => s.Id == slipId);

            if (slip == null)
            {
                return Json(new { success = false, message = "Không tìm thấy phiếu mượn." });
            }

            var userRole = HttpContext.Session.GetString("UserRole");
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (userRole == "Reader" && slip.ReaderId != currentUserId)
            {
                return Json(new { success = false, message = "Bạn không có quyền thao tác trên phiếu mượn này." });
            }

            if (slip.BorrowStatus != BorrowStatus.Borrowing)
            {
                return Json(new { success = false, message = "Chỉ có thể gia hạn phiếu đang trong trạng thái mượn." });
            }

            if (DateTime.Now.Date > slip.DueDate.Date)
            {
                return Json(new { success = false, message = "Phiếu mượn đã quá hạn, vui lòng liên hệ thủ thư để xử lý." });
            }

            var hasUnpaidFine = await _context.Fines
                .AnyAsync(f => f.ReaderId == slip.ReaderId && f.Status == "Unpaid");
            if (hasUnpaidFine)
            {
                return Json(new { success = false, message = "Độc giả còn tiền phạt chưa nộp, không thể gia hạn." });
            }

            var activeDetails = slip.BorrowDetails != null 
                ? slip.BorrowDetails.Where(d => !d.IsReturned).ToList() 
                : new List<BorrowDetails>();
            if (!activeDetails.Any())
            {
                return Json(new { success = false, message = "Không có cuốn sách nào đang mượn cần gia hạn." });
            }

            if (activeDetails.Any(d => d.ExtensionCount >= 2))
            {
                return Json(new { success = false, message = "Sách trong phiếu đã đạt giới hạn gia hạn tối đa (2 lần)." });
            }

            var bookIds = activeDetails.Select(d => d.BookId).ToList();
            var isReserved = await _context.Reservations
                .AnyAsync(r => bookIds.Contains(r.BookId) && r.ReaderId != slip.ReaderId && (r.Status == "Waiting" || r.Status == "Pending"));
            if (isReserved)
            {
                return Json(new { success = false, message = "Sách đang được độc giả khác đặt trước, không thể gia hạn lúc này." });
            }

            slip.DueDate = slip.DueDate.AddDays(7);
            foreach (var detail in activeDetails)
            {
                detail.DueDate = (detail.DueDate ?? slip.DueDate).AddDays(7);
                detail.ExtensionCount += 1;
            }

            await _context.SaveChangesAsync();
            return Json(new
            {
                success = true,
                message = "Gia hạn phiếu mượn thành công thêm 7 ngày!",
                newDueDate = slip.DueDate.ToString("dd/MM/yyyy")
            });
        }

        [HttpGet]
        [SessionAuthorize]
        public async Task<IActionResult> ExportBorrowReport()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole == "Reader")
            {
                return Forbid();
            }

            var slips = await _context.BorrowSlips
                .Include(s => s.Readers)
                .Include(s => s.BorrowDetails)
                    .ThenInclude(d => d.Books)
                .OrderByDescending(s => s.BorrowDate)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("BaoCaoPhieuMuon");

            worksheet.Cell(1, 1).Value = "BÁO CÁO DANH SÁCH PHIẾU MƯỢN THƯ VIỆN";
            worksheet.Range(1, 1, 1, 8).Merge().Style
                .Font.SetBold(true)
                .Font.SetFontSize(16)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            string[] headers = new[] { "Mã phiếu", "Độc giả", "Mã thẻ", "Ngày mượn", "Hạn trả", "Số sách", "Trạng thái", "Ghi chú" };
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cell(3, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromArgb(41, 128, 185);
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            int row = 4;
            foreach (var s in slips)
            {
                worksheet.Cell(row, 1).Value = s.BorrowCode ?? $"PM{s.Id:D5}";
                worksheet.Cell(row, 2).Value = s.Readers?.FullName ?? "N/A";
                worksheet.Cell(row, 3).Value = s.Readers?.ReaderCode ?? "N/A";
                worksheet.Cell(row, 4).Value = s.BorrowDate.ToString("dd/MM/yyyy");
                worksheet.Cell(row, 5).Value = s.DueDate.ToString("dd/MM/yyyy");
                worksheet.Cell(row, 6).Value = s.BorrowDetails?.Count ?? 0;
                worksheet.Cell(row, 7).Value = s.BorrowStatus.ToString();
                worksheet.Cell(row, 8).Value = s.Note ?? "";
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new System.IO.MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"BaoCaoPhieuMuon_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }
    }
}
