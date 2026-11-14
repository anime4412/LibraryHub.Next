using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SOF1022_NHOM1_SD2003_Ver2.Attributes;
using SOF1022_NHOM1_SD2003_Ver2.Datas;
using SOF1022_NHOM1_SD2003_Ver2.Helpers;
using SOF1022_NHOM1_SD2003_Ver2.Models;
using SOF1022_NHOM1_SD2003_Ver2.Models.Enums;
using SOF1022_NHOM1_SD2003_Ver2.Services;

[SessionAuthorize]
public class ReservationsController : Controller
{
    private readonly LibraryDbContext _context;
    private readonly INotificationService _notificationService;

    public ReservationsController(LibraryDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    // GET: Reservations
    [PermissionAuthorize("reservation.view")]
    public async Task<IActionResult> Index()
    {
        await AutoExpireAsync();

        var data = await _context.Reservations
            .Include(r => r.Reader)
            .Include(r => r.Book)
            .ToListAsync();

        return View(data);
    }

    // GET: Create
    public async Task<IActionResult> Create(int? bookId)
    {
        var userRole = HttpContext.Session.GetString("Role") ?? "Reader";
        ViewData["UserRole"] = userRole;

        if (userRole == "Admin" || userRole == "Librarian")
        {
            ViewData["Readers"] = await _context.Readers
                .Where(r => r.IsActive == true)
                .OrderBy(r => r.FullName)
                .ToListAsync();
        }

        // If bookId is provided, lock to that specific book
        if (bookId.HasValue)
        {
            var book = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.Id == bookId.Value);

            if (book == null)
            {
                TempData["Error"] = "Không tìm thấy sách.";
                return RedirectToAction("Index", "Books");
            }

            // Check if book is available (should not allow reservation for available books)
            var realAvailableCount = await BookAvailabilityHelper.GetRealAvailableCountAsync(_context, book.Id, book.Quantity);
            if (realAvailableCount > 0)
            {
                TempData["Error"] = "Sách này vẫn còn hàng, vui lòng mượn trực tiếp.";
                userRole = HttpContext.Session.GetString("Role") ?? "Reader";
                if (userRole == "Admin" || userRole == "Librarian")
                {
                    return RedirectToAction("Create", "BorrowSlips");
                }
                else
                {
                    return RedirectToAction("Create", "BorrowSlips");
                }
            }

            // Pass the locked book to view
            ViewData["LockedBook"] = book;
            ViewData["Books"] = new List<Books> { book };
        }
        else
        {
            // Only show books that are out of stock (using real available count)
            var allBooks = await _context.Books
                .Where(b => b.BooksStatus == BooksStatus.Available)
                .Include(b => b.Author)
                .Include(b => b.Category)
                .OrderBy(b => b.Title)
                .ToListAsync();
            
            // Filter to only show books with real available count of 0
            var outOfStockBooks = new List<Books>();
            foreach (var book in allBooks)
            {
                var realCount = await BookAvailabilityHelper.GetRealAvailableCountAsync(_context, book.Id, book.Quantity);
                if (realCount == 0)
                {
                    outOfStockBooks.Add(book);
                }
            }
            ViewData["Books"] = outOfStockBooks;
        }

        return View();
    }

    // POST: Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Reservations model, int? readerId)
    {
        var userRole = HttpContext.Session.GetString("Role") ?? "Reader";
        
        // For admin mode, use the selected readerId
        // For reader mode, use the logged-in user's ID
        int targetReaderId;
        
        if (userRole == "Admin" || userRole == "Librarian")
        {
            if (!readerId.HasValue)
            {
                TempData["Error"] = "Vui lòng chọn độc giả để đặt trước sách.";
                return RedirectToAction("Create");
            }
            targetReaderId = readerId.Value;
        }
        else
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (!int.TryParse(userIdString, out targetReaderId))
            {
                TempData["Error"] = "Bạn cần đăng nhập để đặt trước sách.";
                return RedirectToAction("Login", "Account");
            }
        }

        if (!ModelState.IsValid)
        {
            if (userRole == "Admin" || userRole == "Librarian")
            {
                ViewData["Readers"] = await _context.Readers
                    .Where(r => r.IsActive == true)
                    .OrderBy(r => r.FullName)
                    .ToListAsync();
            }

            ViewData["Books"] = await _context.Books
                .Where(b => b.BooksStatus == BooksStatus.Available)
                .Include(b => b.Author)
                .Include(b => b.Category)
                .OrderBy(b => b.Title)
                .ToListAsync();

            return View(model);
        }

        // Age validation: Check if reader meets minimum age requirement for the book
        var reader = await _context.Readers.FindAsync(targetReaderId);
        if (reader == null || !reader.DateOfBirth.HasValue)
        {
            TempData["Error"] = "Không tìm thấy thông tin độc giả hoặc thiếu ngày sinh.";
            return RedirectToAction("Create");
        }

        var book = await _context.Books.FindAsync(model.BookId);
        if (book == null)
        {
            TempData["Error"] = "Không tìm thấy sách.";
            return RedirectToAction("Create");
        }

        // Validate book availability - only allow reservation for out-of-stock books
        var realAvailableCount = await BookAvailabilityHelper.GetRealAvailableCountAsync(_context, book.Id, book.Quantity);
        if (realAvailableCount > 0)
        {
            TempData["Error"] = "Sách này vẫn còn hàng, không thể đặt trước. Vui lòng mượn trực tiếp.";
            return RedirectToAction("Create");
        }

        if (book.MinimumAge > 0 && !AgeHelper.MeetsAgeRequirement(reader.DateOfBirth.Value, book.MinimumAge))
        {
            TempData["Error"] = $"Bạn chưa đủ tuổi để đặt trước sách này. Sách yêu cầu độ tuổi tối thiểu: {book.MinimumAge}+";
            return RedirectToAction("Create");
        }

        model.ReaderId = targetReaderId;
        model.ReservationDate = DateTime.Now;
        model.ExpirationDate = DateTime.Now.AddDays(3);
        model.BooksStatus = ReservationsStatus.Waiting;
        model.Status = ReservationsStatus.Waiting.ToString();

        _context.Reservations.Add(model);
        await _context.SaveChangesAsync();

        await _notificationService.NotifyUserAsync(
            model.ReaderId,
            "Reader",
            NotificationType.ReservationCreated,
            "Đặt trước sách thành công",
            $"Bạn đã đặt trước sách \"{book.Title}\". Chúng tôi sẽ thông báo khi sách có sẵn.",
            link: "/Reservations/MyReservations",
            createdBy: userRole == "Reader" ? "Người dùng" : (HttpContext.Session.GetString("UserName") ?? "Thủ thư"));

        TempData["Success"] = "Đặt trước sách thành công!";

        if (userRole == "Admin" || userRole == "Librarian")
        {
            return RedirectToAction("Index");
        }
        else
        {
            return RedirectToAction("MyReservation");
        }
    }

    // POST: Approve reservation with transactional workflow
    [HttpPost]
    [Route("Admin/Reservations/Approve/{id:int}")]
    [ValidateAntiForgeryToken]
    [PermissionAuthorize("reservation.confirm")]
    public async Task<IActionResult> Approve([FromRoute] int id)
    {
        // 1. Retrieve reservation with related Book and Reader
        var reservation = await _context.Reservations
            .Include(r => r.Book)
            .Include(r => r.Reader)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (reservation == null)
        {
            return Json(new { success = false, message = "Không tìm thấy yêu cầu đặt trước này." });
        }

        // 2. STOPS APPROVAL IF STOCK IS 0
        // Dùng số thực còn (Quantity - số đã cam kết cho các phiếu Pending/Approved/Borrowing),
        // KHÔNG dùng AvailableQuantity (kho vật lý) vì trường đó chỉ trừ khi giao sách thực tế -
        // nếu chỉ check AvailableQuantity thì có thể duyệt đặt trước thành phiếu mượn mới trong khi
        // tất cả bản in đã được cam kết cho các phiếu mượn khác, gây vượt tồn kho.
        if (reservation.Book == null)
        {
            return Json(new { success = false, message = "Không tìm thấy sách của yêu cầu đặt trước này." });
        }

        var realAvailableCount = await BookAvailabilityHelper.GetRealAvailableCountAsync(_context, reservation.Book.Id, reservation.Book.Quantity);
        if (realAvailableCount <= 0)
        {
            return Json(new { 
                success = false, 
                message = $"Không thể duyệt! Cuốn sách '{reservation.Book.Title}' hiện đã hết (đang được giữ chỗ cho các phiếu mượn khác)." 
            });
        }

        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                // 3. CREATE A NEW BORROW SLIP AUTOMATICALLY
                var createdBy = User.Identity?.Name ?? HttpContext.Session.GetString("UserName") ?? "Hệ thống tự động";
                var borrowSlip = new BorrowSlips
                {
                    BorrowCode = "BR" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(),
                    ReaderId = reservation.ReaderId,
                    TotalBooks = 1,
                    BorrowDate = DateTime.Now,
                    DueDate = DateTime.Now.AddDays(14), // Standard 2-week loan
                    PickupDeadline = DateTime.Now.AddDays(3), // 3 days to pickup
                    Status = "Borrowing",
                    CreatedDate = DateTime.Now,
                    CreatedBy = createdBy,
                    BorrowStatus = BorrowStatus.ApprovedAwaitingPickup // Changed from Borrowing to ApprovedAwaitingPickup
                };
                _context.BorrowSlips.Add(borrowSlip);
                await _context.SaveChangesAsync(); // Save to get the ID

                // 4. CREATE BORROW DETAIL FOR THE BOOK
                var borrowDetail = new BorrowDetails
                {
                    BorrowSlipId = borrowSlip.Id,
                    BookId = reservation.BookId,
                    Quantity = 1,
                    BorrowPrice = reservation.Book.Price,
                    DueDate = DateTime.Now.AddDays(14),
                    IsReturned = false
                };
                _context.BorrowDetails.Add(borrowDetail);

                // DO NOT decrement inventory - will be deducted on handover

                // 6. UPDATE RESERVATION STATUS TO COMPLETED
                reservation.BooksStatus = ReservationsStatus.Completed;

                // Commit all changes
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var senderName = HttpContext.Session.GetString("UserName") ?? "Thủ thư";
                await _notificationService.NotifyUserAsync(
                    reservation.ReaderId,
                    "Reader",
                    NotificationType.BorrowApproved,
                    "Đặt trước đã được duyệt",
                    $"Yêu cầu đặt trước sách \"{reservation.Book.Title}\" đã được duyệt. Hệ thống đã tạo phiếu mượn {borrowSlip.BorrowCode}. Vui lòng đến nhận sách trước {borrowSlip.PickupDeadline:dd/MM/yyyy HH:mm}.",
                    link: "/BorrowSlips/MyBorrows",
                    createdBy: senderName);

                return Json(new { 
                    success = true, 
                    message = $"Duyệt đặt trước thành công! Hệ thống đã tạo Phiếu mượn ({borrowSlip.BorrowCode}) và trừ kho sách.",
                    redirectUrl = Url.Action("Index", "BorrowSlips")
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Đã xảy ra lỗi hệ thống: " + ex.Message });
            }
        }
    }

    // GET: Mark as Ready For Pickup
    [PermissionAuthorize("reservation.confirm")]
    public async Task<IActionResult> ReadyForPickup(int id)
    {
        var reservation = await _context.Reservations
            .Include(r => r.Book)
            .FirstOrDefaultAsync(r => r.Id == id);
        
        if (reservation == null) return NotFound();

        // Validate book availability - dùng số thực còn (Quantity - cam kết), không dùng AvailableQuantity
        if (reservation.Book == null)
        {
            TempData["Error"] = "Không tìm thấy sách của yêu cầu đặt trước này.";
            return RedirectToAction(nameof(Index));
        }

        var realAvailableCount = await BookAvailabilityHelper.GetRealAvailableCountAsync(_context, reservation.Book.Id, reservation.Book.Quantity);
        if (realAvailableCount <= 0)
        {
            TempData["Error"] = "Sách hiện không có sẵn để chuẩn bị cho mượn (đang được giữ chỗ cho phiếu mượn khác).";
            return RedirectToAction(nameof(Index));
        }

        reservation.BooksStatus = ReservationsStatus.ReadyToBorrow;

        _context.Update(reservation);
        await _context.SaveChangesAsync();

        await _notificationService.NotifyUserAsync(
            reservation.ReaderId,
            "Reader",
            NotificationType.ReservationReady,
            "Sách đặt trước đã sẵn sàng",
            $"Cuốn sách \"{reservation.Book.Title}\" bạn đặt trước đã có sẵn. Vui lòng đến nhận trong vòng 3 ngày.",
            link: "/Reservations/MyReservations",
            createdBy: "Hệ thống");

        TempData["Success"] = "Đã đánh dấu sách sẵn sàng để mượn.";
        return RedirectToAction(nameof(Index));
    }

    [PermissionAuthorize("reservation.confirm")]
    public async Task<IActionResult> Complete(int id)
    {
        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation == null) return NotFound();

        reservation.BooksStatus = ReservationsStatus.Completed;

        _context.Update(reservation);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // POST: Cancel reservation
    [HttpPost]
    [Route("Admin/Reservations/Cancel/{id:int}")]
    [ValidateAntiForgeryToken]
    [PermissionAuthorize("reservation.cancel")]
    public async Task<IActionResult> Cancel([FromRoute] int id)
    {
        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation == null)
            return Json(new { success = false, message = "Không tìm thấy yêu cầu đặt trước này." });

        reservation.BooksStatus = ReservationsStatus.Cancelled;

        _context.Update(reservation);
        await _context.SaveChangesAsync();

        var senderName = HttpContext.Session.GetString("UserName") ?? "Thủ thư";
        await _notificationService.NotifyUserAsync(
            reservation.ReaderId,
            "Reader",
            NotificationType.ReservationCancelled,
            "Đặt trước đã bị hủy",
            "Yêu cầu đặt trước của bạn đã bị hủy bởi thủ thư.",
            link: "/Reservations/MyReservations",
            createdBy: senderName);

        return Json(new { success = true, message = "Đã từ chối yêu cầu đặt trước thành công." });
    }

    public async Task<IActionResult> Expire(int id)
    {
        var reservation = await _context.Reservations.FindAsync(id);

        if (reservation == null)
            return NotFound();

        if (reservation.BooksStatus != ReservationsStatus.Waiting)
            return BadRequest("Invalid state transition");

        reservation.BooksStatus = ReservationsStatus.Expired;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private async Task AutoExpireAsync()
    {
        var now = DateTime.Now;

        var expiredList = _context.Reservations
            .Where(x => x.BooksStatus == ReservationsStatus.Waiting
                     && x.ExpirationDate < now)
            .ToList();

        foreach (var item in expiredList)
        {
            item.BooksStatus = ReservationsStatus.Expired;
        }

        await _context.SaveChangesAsync();
    }

    // GET: Delete
    [PermissionAuthorize("reservation.cancel")]
    public async Task<IActionResult> Delete(int id)
    {
        var reservation = await _context.Reservations
            .Include(r => r.Reader)
            .Include(r => r.Book)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (reservation == null) return NotFound();

        return View(reservation);
    }

    // POST: Delete
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [PermissionAuthorize("reservation.cancel")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation != null)
        {
            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: Details
    [PermissionAuthorize("reservation.view")]
    public async Task<IActionResult> Details(int id)
    {
        var reservation = await _context.Reservations
            .Include(r => r.Reader)
            .Include(r => r.Book)
            .ThenInclude(b => b.Author)
            .Include(r => r.Book)
            .ThenInclude(b => b.Category)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (reservation == null) return NotFound();

        return View(reservation);
    }

    // ==========================
    // Sách đặt trước
    // ==========================
    public async Task<IActionResult> MyReservation()
    {
        var userIdString = HttpContext.Session.GetString("UserId");

        if (string.IsNullOrEmpty(userIdString))
        {
            return RedirectToAction("Login", "Account");
        }

        int readerId = int.Parse(userIdString);

        var model = await _context.Reservations
            .Include(x => x.Book)
            .Where(x => x.ReaderId == readerId)
            .OrderByDescending(x => x.ReservationDate)
            .ToListAsync();

        return View(model);
    }

    [PermissionAuthorize("reservation.view")]
    public async Task<IActionResult> Pending()
    {
        var reservations = await _context.Reservations
            .Include(x => x.Reader)
            .Include(x => x.Book)
            .Where(x => x.BooksStatus == ReservationsStatus.Waiting)
            .OrderBy(x => x.ReservationDate)
            .ToListAsync();

        return View(reservations);
    }

    // GET: Reservations/History
    [PermissionAuthorize("reservation.view")]
    public async Task<IActionResult> History()
    {
        var reservations = await _context.Reservations
            .Include(x => x.Reader)
            .Include(x => x.Book)
            .Where(x => x.BooksStatus == ReservationsStatus.Completed || 
                        x.BooksStatus == ReservationsStatus.Cancelled ||
                        x.BooksStatus == ReservationsStatus.Expired)
            .OrderByDescending(x => x.ReservationDate)
            .ToListAsync();

        return View(reservations);
    }

    // POST: Reservations/BatchApprove
    [HttpPost]
    [ValidateAntiForgeryToken]
    [PermissionAuthorize("reservation.confirm")]
    public async Task<IActionResult> BatchApprove([FromBody] List<int> ids)
    {
        if (ids == null || !ids.Any())
        {
            return Json(new { success = false, message = "Không có phiếu nào được chọn" });
        }

        var approvedCount = 0;
        var skippedCount = 0;
        var skippedMessages = new List<string>();

        foreach (var id in ids)
        {
            var reservation = await _context.Reservations
                .Include(x => x.Book)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (reservation == null)
            {
                skippedMessages.Add($"Phiếu #{id} không tồn tại");
                skippedCount++;
                continue;
            }

            if (reservation.BooksStatus != ReservationsStatus.Waiting)
            {
                skippedMessages.Add($"Phiếu #{id} không ở trạng thái chờ duyệt");
                skippedCount++;
                continue;
            }

            // Validate book availability - dùng số thực còn (Quantity - cam kết), không dùng AvailableQuantity
            if (reservation.Book == null)
            {
                skippedMessages.Add($"Phiếu #{id}: Không tìm thấy sách");
                skippedCount++;
                continue;
            }

            var realAvailableCount = await BookAvailabilityHelper.GetRealAvailableCountAsync(_context, reservation.Book.Id, reservation.Book.Quantity);
            if (realAvailableCount <= 0)
            {
                skippedMessages.Add($"Phiếu #{id}: Sách '{reservation.Book.Title}' đã hết (đang được giữ chỗ cho phiếu mượn khác)");
                skippedCount++;
                continue;
            }

            // Approve the reservation
            reservation.BooksStatus = ReservationsStatus.Approved;
            approvedCount++;
        }

        await _context.SaveChangesAsync();

        var message = $"Đã duyệt thành công {approvedCount} phiếu";
        if (skippedCount > 0)
        {
            message += $", bỏ qua {skippedCount} phiếu:\n" + string.Join("\n", skippedMessages);
        }

        return Json(new { success = true, message = message });
    }
}
