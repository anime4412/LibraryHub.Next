using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SOF1022_NHOM1_SD2003_Ver2.Datas;
using SOF1022_NHOM1_SD2003_Ver2.Helpers;
using SOF1022_NHOM1_SD2003_Ver2.Models;
using SOF1022_NHOM1_SD2003_Ver2.Models.DTOs;
using SOF1022_NHOM1_SD2003_Ver2.Models.Enums;
using System.Linq;

namespace SOF1022_NHOM1_SD2003_Ver2.Services
{
    public class BorrowService : IBorrowService
    {
        private readonly LibraryDbContext _context;
        private readonly INotificationService _notificationService;

        // ==== Tham số nghiệp vụ thư viện ====
        private const int LoanPeriodDays = 7;          // Thời hạn mượn mặc định
        private const decimal LateFeePerDay = 5000m;    // Phí phạt trễ hạn / ngày
        private const decimal LostFeeMultiplier = 2.0m; // Phạt mất sách = giá * 2
        private const decimal DamagedFeeMultiplier = 0.5m; // Phạt hư hỏng = giá * 0.5

        public BorrowService(LibraryDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        private string GenerateUniqueTransactionCode()
        {
            // Generate a unique 8-character alphanumeric code
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            string code;
            bool isUnique;
            
            do
            {
                code = new string(Enumerable.Repeat(chars, 8)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
                code = "PT" + code; // PT = Phat ThuVien (Library Fine)
                
                // Check if this code already exists in the database
                isUnique = !_context.Fines.Any(f => f.TransactionCode == code);
            } while (!isUnique);
            
            return code;
        }

        private async Task AutoExpirePendingSlipsAsync()
        {
            var expiredSlips = await _context.BorrowSlips
                .Include(bs => bs.BorrowDetails)
                .Where(bs => (bs.BorrowStatus == BorrowStatus.PendingApproval 
                           || bs.BorrowStatus == BorrowStatus.ApprovedAwaitingPickup)
                           && bs.PickupDeadline.HasValue 
                           && bs.PickupDeadline < DateTime.Now)
                .ToListAsync();

            foreach (var slip in expiredSlips)
            {
                // DO NOT restore inventory - it was never deducted for PendingApproval/ApprovedAwaitingPickup

                // Permanently delete expired slips instead of just canceling
                _context.BorrowSlips.Remove(slip);
            }

            if (expiredSlips.Any())
            {
                await _context.SaveChangesAsync();
            }
        }

        public async Task<BorrowResultDto> BorrowAsync(int readerId, List<int> bookIds, DateTime? dueDate = null, string? note = null, string? createdBy = null, DateTime? borrowDate = null)
        {
            var result = new BorrowResultDto();
            
            if (readerId <= 0 || bookIds == null || !bookIds.Any())
            {
                result.Success = false;
                result.Message = "Thiếu dữ liệu";
                return result;
            }

            // Validate borrowDate if provided (for admin backdating)
            if (borrowDate.HasValue && borrowDate.Value > DateTime.Now)
            {
                result.Success = false;
                result.Message = "Ngày mượn sách không thể vượt quá thời gian hiện tại!";
                return result;
            }

            // Use provided borrowDate or default to current time
            var actualBorrowDate = borrowDate ?? DateTime.Now;

            if (dueDate.HasValue && dueDate.Value.Date <= actualBorrowDate.Date)
            {
                result.Success = false;
                result.Message = "Hạn trả phải sau ngày mượn";
                return result;
            }

            bookIds = bookIds.Distinct().ToList();

            // Auto-expire pending slips that have exceeded pickup deadline
            await AutoExpirePendingSlipsAsync();

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Kiểm tra độc giả
                var reader = await _context.Readers.FirstOrDefaultAsync(r => r.Id == readerId);
                if (reader == null)
                {
                    result.Success = false;
                    result.Message = "Độc giả không tồn tại";
                    return result;
                }
                if (!reader.IsActive)
                {
                    result.Success = false;
                    result.Message = "Tài khoản đang bị khóa";
                    return result;
                }
                if (reader.ExpirationDate < DateTime.Now)
                {
                    result.Success = false;
                    result.Message = "Thẻ độc giả đã hết hạn, vui lòng gia hạn thẻ trước khi mượn sách";
                    return result;
                }

                // 2. Không cho mượn nếu còn khoản phạt chưa thanh toán
                var unpaidFines = await _context.Fines
                    .Where(f => f.BorrowSlips!.ReaderId == readerId && !f.IsPaid)
                    .ToListAsync();
                
                if (unpaidFines.Any())
                {
                    var totalFine = unpaidFines.Sum(f => f.Amount);
                    result.Success = false;
                    result.UnpaidFineAmount = totalFine;
                    result.Message = $"Bạn đang có khoản phạt chưa thanh toán ({totalFine:N0} VNĐ). Vui lòng thanh toán tiền phạt để tiếp tục mượn.";
                    return result;
                }

                // 3. Kiểm tra xem độc giả có sách quá hạn chưa trả không
                var hasOverdueBooks = await _context.BorrowDetails
                    .AnyAsync(d => d.BorrowSlips!.ReaderId == readerId 
                                  && !d.IsReturned 
                                  && d.BorrowSlips!.DueDate < DateTime.Now);
                
                if (hasOverdueBooks)
                {
                    result.Success = false;
                    result.Message = "Bạn có sách quá hạn chưa trả, vui lòng trả trước khi mượn sách mới";
                    return result;
                }

                // 4. Đếm số sách đang mượn thực tế (chỉ tính các trạng thái đang hoạt động)
                var currentBorrowCount = await _context.BorrowDetails
                    .CountAsync(d => d.BorrowSlips!.ReaderId == readerId 
                        && !d.IsReturned
                        && (d.BorrowSlips.BorrowStatus == BorrowStatus.PendingApproval
                            || d.BorrowSlips.BorrowStatus == BorrowStatus.ApprovedAwaitingPickup
                            || d.BorrowSlips.BorrowStatus == BorrowStatus.Borrowing));
                
                result.CurrentBorrowCount = currentBorrowCount;
                result.MaxBorrowBooks = reader.MaxBorrowBooks;

                if (currentBorrowCount >= reader.MaxBorrowBooks)
                {
                    result.Success = false;
                    result.Message = $"Bạn đã đạt giới hạn mượn tối đa ({reader.MaxBorrowBooks} cuốn). Vui lòng trả bớt sách hoặc hủy phiếu chờ trước khi mượn mới.";
                    return result;
                }

                // 5. Khóa các dòng sách (UPDLOCK/ROWLOCK) để tránh race condition
                var parameters = bookIds
                    .Select((id, idx) => new SqlParameter($"@p{idx}", id))
                    .ToArray();
                var placeholders = string.Join(",", parameters.Select(p => p.ParameterName));

#pragma warning disable EF1002
                var books = await _context.Books
                    .FromSqlRaw($"SELECT * FROM Books WITH (UPDLOCK, ROWLOCK) WHERE Id IN ({placeholders})", parameters)
                    .ToListAsync();
#pragma warning restore EF1002

                if (books.Count != bookIds.Count)
                {
                    result.Success = false;
                    result.Message = "Một số sách không tồn tại";
                    return result;
                }

                // 5.5. Check real-time availability.
                // "Cam kết" (committed) = số sách đang ở trạng thái PendingApproval, ApprovedAwaitingPickup
                // hoặc Borrowing (chưa trả) - tức là đã được giữ chỗ cho một độc giả, kể cả khi hệ thống
                // tự động duyệt và chưa giao sách thực tế. Chỉ tính riêng "Borrowing" là sai vì sẽ cho phép
                // nhiều độc giả cùng được tự động duyệt cho MỘT bản sách duy nhất (vượt tồn kho thực tế).
                foreach (var book in books)
                {
                    var committedCount = await BookAvailabilityHelper.GetCommittedCountAsync(_context, book.Id);
                    var realAvailableQuantity = book.Quantity - committedCount;

                    // Hết sách khả dụng (đã cam kết hết cho người khác) -> không tự động duyệt thêm,
                    // độc giả cần chuyển sang đặt trước.
                    if (realAvailableQuantity <= 0)
                    {
                        result.Success = false;
                        result.Message = $"Sách \"{book.Title}\" hiện đã hết, vui lòng đặt trước để nhận thông báo khi có sách.";
                        return result;
                    }
                }

                // 6. Validate each book individually for partial success support
                var successfulBooks = new List<Books>();
                var failedBooks = new List<BorrowError>();

                foreach (var book in books)
                {
                    var error = ValidateBookForBorrow(book, readerId, currentBorrowCount, reader.MaxBorrowBooks);
                    if (error != null)
                    {
                        failedBooks.Add(error);
                    }
                    else
                    {
                        successfulBooks.Add(book);
                    }
                }

                // If no books can be borrowed, return all errors
                if (successfulBooks.Count == 0)
                {
                    result.Success = false;
                    result.PartialSuccess = false;
                    result.FailedBooks = failedBooks;
                    result.Message = "Không thể mượn bất kỳ cuốn sách nào. Vui lòng xem chi tiết lỗi từng sách.";
                    return result;
                }

                // 7. Check if we can borrow all successful books within limit
                if (currentBorrowCount + successfulBooks.Count > reader.MaxBorrowBooks)
                {
                    // Remove excess books from successful list
                    var canBorrow = reader.MaxBorrowBooks - currentBorrowCount;
                    var excessBooks = successfulBooks.Skip(canBorrow).ToList();
                    
                    foreach (var excessBook in excessBooks)
                    {
                        failedBooks.Add(new BorrowError
                        {
                            BookId = excessBook.Id,
                            BookTitle = excessBook.Title,
                            Reason = "Vượt quá giới hạn mượn tối đa",
                            Solution = $"Bạn chỉ có thể mượn thêm {canBorrow} cuốn. Vui lòng bỏ bớt sách hoặc trả sách đang mượn."
                        });
                    }
                    
                    successfulBooks = successfulBooks.Take(canBorrow).ToList();
                }

                // 8. Check if successful books are reserved for others
                var successfulBookIds = successfulBooks.Select(b => b.Id).ToList();
                var reservedForOthers = await _context.Reservations
                    .Where(r => successfulBookIds.Contains(r.BookId)
                                && r.ReaderId != readerId
                                && r.BooksStatus == ReservationsStatus.ReadyToBorrow
                                && (r.ExpirationDate == null || r.ExpirationDate >= DateTime.Now))
                    .Select(r => new { r.BookId, r.Book!.Title })
                    .ToListAsync();

                foreach (var reserved in reservedForOthers)
                {
                    var book = successfulBooks.FirstOrDefault(b => b.Id == reserved.BookId);
                    if (book != null)
                    {
                        successfulBooks.Remove(book);
                        failedBooks.Add(new BorrowError
                        {
                            BookId = reserved.BookId,
                            BookTitle = reserved.Title,
                            Reason = "Sách đang được đặt trước bởi độc giả khác",
                            Solution = "Sách này đang được giữ cho độc giả khác đã đặt trước. Vui lòng chọn sách khác."
                        });
                    }
                }

                // If no books can be borrowed after all validations
                if (successfulBooks.Count == 0)
                {
                    result.Success = false;
                    result.PartialSuccess = false;
                    result.FailedBooks = failedBooks;
                    result.Message = "Không thể mượn bất kỳ cuốn sách nào sau khi kiểm tra các điều kiện.";
                    return result;
                }

                // 9. Check if user has existing pending slip to merge with
                var existingPendingSlip = await _context.BorrowSlips
                    .Where(bs => bs.ReaderId == readerId 
                                && bs.BorrowStatus == BorrowStatus.PendingApproval
                                && bs.CreatedDate.Date == actualBorrowDate.Date)
                    .OrderByDescending(bs => bs.CreatedDate)
                    .FirstOrDefaultAsync();

                BorrowSlips slip;
                bool isMerging = false;

                if (existingPendingSlip != null)
                {
                    // Merge with existing pending slip
                    slip = existingPendingSlip;
                    slip.TotalBooks += successfulBooks.Count;
                    isMerging = true;
                }
                else
                {
                    // Create new borrow slip with automatic approval
                    slip = new BorrowSlips
                    {
                        BorrowCode = "BR" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(),
                        ReaderId = readerId,
                        BorrowDate = actualBorrowDate,
                        DueDate = dueDate ?? actualBorrowDate.AddDays(LoanPeriodDays),
                        PickupDeadline = actualBorrowDate.AddDays(3),
                        Note = note,
                        BorrowStatus = BorrowStatus.ApprovedAwaitingPickup, // Auto-approve
                        TotalBooks = successfulBooks.Count,
                        CreatedDate = DateTime.Now,
                        CreatedBy = createdBy ?? "Hệ thống tự động"
                    };
                    _context.BorrowSlips.Add(slip);
                }

                await _context.SaveChangesAsync();

                result.SlipId = slip.Id;
                result.SlipCode = slip.BorrowCode;

                // 10. Create borrow details for successful books and update inventory
                foreach (var book in successfulBooks)
                {
                    _context.BorrowDetails.Add(new BorrowDetails
                    {
                        BorrowSlipId = slip.Id,
                        BookId = book.Id,
                        Quantity = 1,
                        BorrowPrice = book.Price,
                        IsReturned = false
                    });

                    book.LastBorrowDate = DateTime.Now;
                    
                    // DO NOT subtract from inventory yet - will be deducted on handover

                    // Mark user's own reservation as completed
                    var myReadyReservation = await _context.Reservations
                        .Where(r => r.ReaderId == readerId
                                    && r.BookId == book.Id
                                    && r.BooksStatus == ReservationsStatus.ReadyToBorrow)
                        .FirstOrDefaultAsync();

                    if (myReadyReservation != null)
                    {
                        myReadyReservation.BooksStatus = ReservationsStatus.Completed;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                result.SuccessfulBookIds = successfulBooks.Select(b => b.Id).ToList();
                result.FailedBooks = failedBooks;
                result.PartialSuccess = failedBooks.Count > 0;
                result.Success = true;
                
                if (result.PartialSuccess)
                {
                    var actionText = isMerging ? "thêm vào phiếu mượn hiện có" : "tạo phiếu mượn mới";
                    result.Message = $"Đã {actionText} thành công cho {successfulBooks.Count} cuốn sách. {failedBooks.Count} cuốn sách không thể mượn (xem chi tiết bên dưới).";
                }
                else
                {
                    var actionText = isMerging ? "thêm vào phiếu mượn hiện có" : "tạo phiếu mượn mới";
                    result.Message = $"Đã {actionText} thành công cho {successfulBooks.Count} cuốn sách.";
                }

                // Send notification to reader
                var borrowedTitles = string.Join(", ", successfulBooks.Select(b => b.Title));
                var pickupInfo = slip.PickupDeadline.HasValue
                    ? $" Vui lòng đến nhận sách trước {slip.PickupDeadline.Value:dd/MM/yyyy HH:mm}."
                    : "";
                await _notificationService.NotifyUserAsync(
                    readerId,
                    "Reader",
                    NotificationType.BorrowCreated,
                    "Tạo phiếu mượn thành công",
                    $"Phiếu mượn {slip.BorrowCode} đã ghi nhận {successfulBooks.Count} cuốn sách: {borrowedTitles}.{pickupInfo}",
                    link: "/BorrowSlips/MyBorrows",
                    createdBy: "Hệ thống");

                // Send notification to all admins and librarians about new borrow request
                var adminLibrarianIds = await _context.Readers
                    .Where(r => (r.Role == "Admin" || r.Role == "Librarian") && r.IsActive && !r.IsDeleted)
                    .Select(r => r.Id)
                    .ToListAsync();

                if (adminLibrarianIds.Any())
                {
                    await _notificationService.NotifyManyAsync(
                        adminLibrarianIds,
                        "Admin",
                        NotificationType.BorrowCreated,
                        "Phiếu mượn mới cần xử lý",
                        $"Độc giả {reader.FullName} đã tạo phiếu mượn {slip.BorrowCode} với {successfulBooks.Count} cuốn sách.",
                        link: $"/BorrowSlips/Details/{slip.Id}",
                        createdBy: "Hệ thống");
                }

                return result;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                result.Success = false;
                result.Message = $"Lỗi hệ thống: {ex.Message}";
                return result;
            }
        }

        private BorrowError? ValidateBookForBorrow(Books book, int readerId, int currentBorrowCount, int maxBorrowBooks)
        {
            // Check if already borrowing this book (only check active statuses)
            var alreadyBorrowing = _context.BorrowDetails
                .Any(d => d.BorrowSlips!.ReaderId == readerId 
                    && !d.IsReturned 
                    && d.BookId == book.Id
                    && (d.BorrowSlips.BorrowStatus == BorrowStatus.PendingApproval
                        || d.BorrowSlips.BorrowStatus == BorrowStatus.ApprovedAwaitingPickup
                        || d.BorrowSlips.BorrowStatus == BorrowStatus.Borrowing));
            
            if (alreadyBorrowing)
            {
                return new BorrowError
                {
                    BookId = book.Id,
                    BookTitle = book.Title,
                    Reason = "Bạn đang mượn cuốn sách này và chưa trả",
                    Solution = "Vui lòng trả cuốn sách này trước khi mượn lại."
                };
            }

            // Check book status
            if (book.BooksStatus == BooksStatus.Hidden || book.BooksStatus == BooksStatus.Maintenance)
            {
                return new BorrowError
                {
                    BookId = book.Id,
                    BookTitle = book.Title,
                    Reason = $"Sách hiện không cho mượn (trạng thái: {book.BooksStatus})",
                    Solution = "Sách này đang trong trạng thái bảo trì hoặc ẩn. Vui lòng chọn sách khác."
                };
            }

            // Check availability dựa trên số đã "cam kết" (PendingApproval + ApprovedAwaitingPickup + Borrowing),
            // KHÔNG dùng book.AvailableQuantity vì trường này chỉ phản ánh kho vật lý (chỉ trừ khi giao sách).
            // Việc này đã được lọc ở bước 5.5 (real-time availability) phía trên trước khi vào đây,
            // nhưng vẫn kiểm tra lại ở đây để an toàn khi hàm được gọi độc lập.
            var committedCount = _context.BorrowDetails
                .Count(bd => bd.BookId == book.Id
                    && !bd.IsReturned
                    && bd.BorrowSlips != null
                    && (bd.BorrowSlips.BorrowStatus == BorrowStatus.PendingApproval
                        || bd.BorrowSlips.BorrowStatus == BorrowStatus.ApprovedAwaitingPickup
                        || bd.BorrowSlips.BorrowStatus == BorrowStatus.Borrowing));

            if (book.Quantity - committedCount <= 0)
            {
                return new BorrowError
                {
                    BookId = book.Id,
                    BookTitle = book.Title,
                    Reason = "Sách hiện đã hết trong kho",
                    Solution = "Tất cả các bản in của cuốn sách này đã được mượn hoặc đang chờ giao. Vui lòng đặt trước hoặc chọn sách khác."
                };
            }

            return null; // Book is valid
        }

        public async Task<(bool Success, string Message)> ReturnAsync(int borrowDetailId, bool isLost = false, bool isDamaged = false, string? damageDetails = null)
        {
            return await ReturnMultipleAsync(0, new List<(int BorrowDetailId, string Condition, int? DamagePercent, string? DamageReason, string? DamageDetails)>
            {
                (borrowDetailId, isLost ? "Lost" : (isDamaged ? "Damaged" : "Returned"), null, damageDetails, null)
            });
        }

        public async Task<(bool Success, string Message)> ReturnMultipleAsync(int borrowSlipId, List<(int BorrowDetailId, string Condition, int? DamagePercent, string? DamageReason, string? DamageDetails)> returnItems)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var now = DateTime.Now;
                var borrowSlip = await _context.BorrowSlips
                    .Include(bs => bs.Readers)
                    .FirstOrDefaultAsync(bs => bs.Id == borrowSlipId);

                if (borrowSlip == null)
                    return (false, "Không tìm thấy phiếu mượn");

                // Process each book return item
                var fineDetailsList = new List<FineDetails>();
                Fines? consolidatedFine = null;
                decimal totalFine = 0;
                var returnedBookTitles = new List<string>();
                var readyReservationNotifications = new List<(int ReaderId, string BookTitle)>();

                foreach (var (borrowDetailId, condition, damagePercent, damageReason, damageDetails) in returnItems)
                {
                    var detail = await _context.BorrowDetails
                        .Include(x => x.Books)
                        .Include(x => x.BorrowSlips)
                        .FirstOrDefaultAsync(x => x.Id == borrowDetailId);

                    if (detail == null)
                        return (false, $"Không tìm thấy chi tiết phiếu mượn ID {borrowDetailId}");

                    if (detail.IsReturned)
                        return (false, $"Sách ID {borrowDetailId} đã được trả rồi");

                    if (detail.Books == null || detail.BorrowSlips == null)
                        return (false, "Dữ liệu phiếu mượn không hợp lệ");

                    // Track returned book title for notification
                    if (detail.Books != null)
                    {
                        returnedBookTitles.Add(detail.Books.Title);
                    }

                    var isLate = detail.BorrowSlips.DueDate < now;
                    var isLost = condition == "Lost";
                    var isDamaged = condition == "Damaged";

                    // 1. Cập nhật chi tiết phiếu mượn
                    detail.IsReturned = true;
                    detail.ReturnedDate = now;
                    detail.IsLost = isLost;
                    detail.IsDamaged = isDamaged;
                    detail.ConditionAfter = damageDetails ?? damageReason;
                    detail.DamagePercent = damagePercent;

                    // 2. Tính tiền phạt cho cuốn sách này với logic mới
                    decimal fine = 0;
                    var reasons = new List<string>();

                    if (isLost)
                    {
                        fine = detail.Books.Price * LostFeeMultiplier;
                        reasons.Add("Đền sách bị mất");
                    }
                    else
                    {
                        if (isDamaged)
                        {
                            var percent = damagePercent ?? 100;
                            
                            // Fine calculation based on damage percentage tiers
                            if (percent >= 10 && percent <= 40)
                            {
                                fine = 0; // No fine for 10-40% damage
                            }
                            else if (percent == 50)
                            {
                                fine = 10000; // Fixed 10,000 VNĐ for 50% damage
                                reasons.Add($"Phạt sách hư hỏng ({percent}%)");
                            }
                            else if (percent == 60)
                            {
                                fine = 15000; // Fixed 15,000 VNĐ for 60% damage
                                reasons.Add($"Phạt sách hư hỏng ({percent}%)");
                            }
                            else if (percent == 70)
                            {
                                fine = 20000; // Fixed 20,000 VNĐ for 70% damage
                                reasons.Add($"Phạt sách hư hỏng ({percent}%)");
                            }
                            else if (percent >= 80 && percent <= 100)
                            {
                                fine = detail.Books.Price * (percent / 100.0m); // Proportional to book price for 80-100% damage
                                reasons.Add($"Phạt sách hư hỏng ({percent}%)");
                            }
                        }

                        if (isLate)
                        {
                            var lateDays = (now.Date - detail.BorrowSlips.DueDate.Date).Days;
                            fine += lateDays * LateFeePerDay;
                            reasons.Add($"Phạt trả trễ {lateDays} ngày");
                        }
                    }

                    detail.CompensationAmount = fine;
                    totalFine += fine;

                    // 3. Tạo FineDetail nếu có phạt
                    if (fine > 0)
                    {
                        fineDetailsList.Add(new FineDetails
                        {
                            BookId = detail.BookId,
                            BorrowDetailId = detail.Id,
                            Amount = fine,
                            Reason = string.Join("; ", reasons),
                            FinesType = isLost ? FinesType.Lost : (isDamaged ? FinesType.Damaged : FinesType.Late),
                            DamagePercent = damagePercent,
                            BookPrice = detail.Books.Price,
                            CreatedDate = now
                        });
                    }

                    // 4. Cập nhật kho sách
                    if (isLost)
                    {
                        detail.Books.Quantity = Math.Max(0, detail.Books.Quantity - 1);
                    }
                    else
                    {
                        detail.Books.AvailableQuantity += 1;
                    }

                    // 5. Xử lý đặt trước nếu sách không bị mất
                    if (!isLost)
                    {
                        var nextReservation = await _context.Reservations
                            .Where(r => r.BookId == detail.BookId && r.BooksStatus == ReservationsStatus.Waiting)
                            .OrderBy(r => r.ReservationDate)
                            .FirstOrDefaultAsync();

                        if (nextReservation != null)
                        {
                            nextReservation.BooksStatus = ReservationsStatus.ReadyToBorrow;
                            nextReservation.ExpirationDate = now.AddDays(2);
                            readyReservationNotifications.Add((nextReservation.ReaderId, detail.Books.Title));
                        }
                    }
                }

                // 6. Tạo hoặc cập nhật Fine record hợp nhất
                if (fineDetailsList.Any())
                {
                    // Check if there's already a fine record for this borrow slip from today
                    var existingFine = await _context.Fines
                        .Where(f => f.BorrowSlipId == borrowSlipId 
                                    && f.CreatedDate.Date == now.Date
                                    && !f.IsPaid)
                        .FirstOrDefaultAsync();

                    if (existingFine != null)
                    {
                        // Add to existing fine record
                        existingFine.Amount += totalFine;
                        foreach (var fd in fineDetailsList)
                        {
                            fd.FineId = existingFine.Id;
                            _context.FineDetails.Add(fd);
                        }
                    }
                    else
                    {
                        // Create new consolidated fine record
                        consolidatedFine = new Fines
                        {
                            BorrowSlipId = borrowSlipId,
                            ReaderId = borrowSlip.ReaderId,
                            Amount = totalFine,
                            Reason = $"{fineDetailsList.Count} cuốn sách vi phạm",
                            FinesType = fineDetailsList.Any(fd => fd.FinesType == FinesType.Lost) ? FinesType.Lost : FinesType.Damaged,
                            IsPaid = false,
                            CreatedDate = now,
                            TransactionCode = GenerateUniqueTransactionCode()
                        };

                        _context.Fines.Add(consolidatedFine);
                        await _context.SaveChangesAsync(); // Save to get the FineId

                        // Add fine details
                        foreach (var fd in fineDetailsList)
                        {
                            fd.FineId = consolidatedFine.Id;
                            _context.FineDetails.Add(fd);
                        }
                    }
                }

                // 7. Cập nhật trạng thái phiếu mượn
                var allReturned = await _context.BorrowDetails
                    .Where(bd => bd.BorrowSlipId == borrowSlipId)
                    .AllAsync(bd => bd.IsReturned);

                if (allReturned)
                {
                    borrowSlip.BorrowStatus = BorrowStatus.Returned;
                    borrowSlip.ReturnDate = now;
                }
                else
                {
                    // Check if any book is lost or damaged
                    var hasIncident = await _context.BorrowDetails
                        .Where(bd => bd.BorrowSlipId == borrowSlipId && bd.IsReturned)
                        .AnyAsync(bd => bd.IsLost || bd.IsDamaged);

                    if (hasIncident)
                    {
                        borrowSlip.BorrowStatus = BorrowStatus.LostOrDamaged;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Send notifications
                var returnedTitles = string.Join(", ", returnedBookTitles);
                await _notificationService.NotifyUserAsync(
                    borrowSlip.ReaderId,
                    "Reader",
                    NotificationType.ReturnCompleted,
                    "Trả sách thành công",
                    $"Bạn đã trả {returnedBookTitles.Count} cuốn sách: {returnedTitles}.",
                    link: "/BorrowSlips/MyBorrows",
                    createdBy: "Hệ thống");

                if (totalFine > 0)
                {
                    await _notificationService.NotifyUserAsync(
                        borrowSlip.ReaderId,
                        "Reader",
                        NotificationType.FineIssued,
                        "Phát sinh khoản phạt mới",
                        $"Bạn có khoản phạt {totalFine:N0} VNĐ liên quan đến phiếu mượn {borrowSlip.BorrowCode}. Vui lòng vào mục Khoản phạt để thanh toán.",
                        link: "/Fines/MyFines",
                        createdBy: "Hệ thống");
                }

                foreach (var (readerId, bookTitle) in readyReservationNotifications)
                {
                    await _notificationService.NotifyUserAsync(
                        readerId,
                        "Reader",
                        NotificationType.ReservationReady,
                        "Sách đặt trước đã sẵn sàng",
                        $"Cuốn sách \"{bookTitle}\" bạn đặt trước đã có sẵn. Vui lòng đến nhận trong vòng 2 ngày.",
                        link: "/Reservations/MyReservations",
                        createdBy: "Hệ thống");
                }

                return (true, "Đã xác nhận trả sách thành công");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, "Đã xảy ra lỗi: " + ex.Message);
            }
        }

        public async Task<List<BorrowItemDto>> GetCurrentBorrowsAsync(int readerId)
        {
            var borrowDetails = await _context.BorrowDetails
                .Include(x => x.Books)
                .Include(x => x.BorrowSlips)
                .Where(x => x.BorrowSlips!.ReaderId == readerId 
                            && !x.IsReturned 
                            && (x.BorrowSlips!.BorrowStatus == BorrowStatus.PendingApproval 
                                || x.BorrowSlips!.BorrowStatus == BorrowStatus.ApprovedAwaitingPickup
                                || x.BorrowSlips!.BorrowStatus == BorrowStatus.Borrowing
                                || x.BorrowSlips!.BorrowStatus == BorrowStatus.PendingExtension))
                .OrderByDescending(x => x.BorrowSlips!.BorrowDate)
                .ToListAsync();

            return borrowDetails.Select(d => new BorrowItemDto
            {
                BorrowDetailId = d.Id,
                BorrowSlipId = d.BorrowSlipId,
                BorrowCode = d.BorrowSlips?.BorrowCode ?? ("BR" + d.BorrowSlipId.ToString("D6")),
                BookTitle = d.Books?.Title ?? "",
                BookCoverImage = d.Books?.CoverImage ?? "",
                BorrowDate = d.BorrowSlips!.BorrowDate,
                DueDate = d.DueDate ?? d.BorrowSlips.DueDate,
                ReturnedDate = d.ReturnedDate,
                BorrowStatus = d.BorrowSlips.BorrowStatus,
                Status = d.BorrowSlips.BorrowStatus == BorrowStatus.PendingApproval 
                    ? "Chờ duyệt" 
                    : d.BorrowSlips.BorrowStatus == BorrowStatus.ApprovedAwaitingPickup
                        ? "Đã duyệt"
                        : d.BorrowSlips.BorrowStatus == BorrowStatus.PendingExtension
                            ? "Chờ duyệt gia hạn"
                            : ((d.DueDate ?? d.BorrowSlips.DueDate) < DateTime.Now ? "Quá hạn" : "Đang mượn"),
                IsReturned = d.IsReturned,
                FineAmount = d.CompensationAmount,
                IsLost = d.IsLost,
                IsDamaged = d.IsDamaged,
                IsLate = (d.DueDate ?? d.BorrowSlips.DueDate) < DateTime.Now,
                PickupDeadline = d.BorrowSlips.PickupDeadline,
                ExtensionCount = d.ExtensionCount
            }).ToList();
        }

        public async Task<List<BorrowItemDto>> GetBorrowHistoryAsync(int readerId)
        {
            var borrowDetails = await _context.BorrowDetails
                .Include(x => x.Books)
                .Include(x => x.BorrowSlips)
                .Where(x => x.BorrowSlips!.ReaderId == readerId
                    && (x.IsReturned
                        || x.BorrowSlips!.BorrowStatus == BorrowStatus.Cancelled
                        || x.BorrowSlips!.BorrowStatus == BorrowStatus.CanceledDueToTimeout))
                .OrderByDescending(x => x.ReturnedDate ?? x.BorrowSlips!.CreatedDate)
                .ToListAsync();

            var borrowSlipIds = borrowDetails.Select(d => d.BorrowSlipId).Distinct().ToList();
            var fines = await _context.Fines
                .Where(f => borrowSlipIds.Contains(f.BorrowSlipId))
                .ToListAsync();

            return borrowDetails.Select(d => new BorrowItemDto
            {
                BorrowDetailId = d.Id,
                BorrowSlipId = d.BorrowSlipId,
                BorrowCode = d.BorrowSlips?.BorrowCode ?? ("BR" + d.BorrowSlipId.ToString("D6")),
                BookTitle = d.Books?.Title ?? "",
                BookCoverImage = d.Books?.CoverImage ?? "",
                BorrowDate = d.BorrowSlips!.BorrowDate,
                DueDate = d.BorrowSlips.DueDate,
                ReturnedDate = d.ReturnedDate,
                Status = d.BorrowSlips!.BorrowStatus == BorrowStatus.CanceledDueToTimeout
                    ? "Đã hủy - quá hạn nhận sách"
                    : d.BorrowSlips!.BorrowStatus == BorrowStatus.Cancelled
                        ? "Đã hủy"
                        : d.IsLost
                            ? "Mất sách"
                            : (d.IsDamaged
                                ? "Hư hỏng"
                                : (d.ReturnedDate.HasValue && d.ReturnedDate > d.BorrowSlips!.DueDate ? "Trả trễ hạn" : "Đã trả")),
                IsReturned = d.IsReturned,
                FineAmount = d.CompensationAmount,
                IsLost = d.IsLost,
                IsDamaged = d.IsDamaged,
                IsLate = d.ReturnedDate.HasValue && d.ReturnedDate > d.BorrowSlips!.DueDate,
                FineId = fines.FirstOrDefault(f => f.BorrowSlipId == d.BorrowSlipId)?.Id,
                FineStatus = fines.FirstOrDefault(f => f.BorrowSlipId == d.BorrowSlipId)?.Status,
                TransactionCode = fines.FirstOrDefault(f => f.BorrowSlipId == d.BorrowSlipId)?.TransactionCode
            }).ToList();
        }

        public async Task<(bool Success, string Message)> RequestExtensionAsync(int borrowDetailId, int requestedDays, string reason)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var borrowDetail = await _context.BorrowDetails
                    .Include(d => d.Books)
                    .Include(d => d.BorrowSlips)
                    .FirstOrDefaultAsync(d => d.Id == borrowDetailId);

                if (borrowDetail == null)
                    return (false, "Không tìm thấy chi tiết mượn sách");

                if (borrowDetail.IsReturned)
                    return (false, "Sách này đã được trả");

                if (borrowDetail.BorrowSlips!.BorrowStatus != BorrowStatus.Borrowing)
                    return (false, "Chỉ có thể gia hạn khi đang mượn sách");

                // Check max extension limit (2 renewals)
                if (borrowDetail.ExtensionCount >= 2)
                    return (false, "Đã đạt giới hạn gia hạn (tối đa 2 lần)");

                // Check if book is reserved by another reader
                var isReservedByOthers = await _context.Reservations
                    .AnyAsync(r => r.BookId == borrowDetail.BookId 
                                && r.ReaderId != borrowDetail.BorrowSlips.ReaderId
                                && r.BooksStatus == ReservationsStatus.ReadyToBorrow
                                && (r.ExpirationDate == null || r.ExpirationDate >= DateTime.Now));

                if (isReservedByOthers)
                    return (false, "Sách đang được đặt trước bởi độc giả khác, không thể gia hạn");

                // Update extension request
                borrowDetail.RequestedExtensionDays = requestedDays;
                borrowDetail.ExtensionReason = reason;
                borrowDetail.BorrowSlips.BorrowStatus = BorrowStatus.PendingExtension;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Send notification
                await _notificationService.NotifyUserAsync(
                    borrowDetail.BorrowSlips.ReaderId,
                    "Reader",
                    NotificationType.ExtensionRequested,
                    "Đã gửi yêu cầu gia hạn",
                    $"Yêu cầu gia hạn {requestedDays} ngày cho sách \"{borrowDetail.Books?.Title}\" đã được gửi. Vui lòng chờ thủ thư duyệt.",
                    link: "/BorrowSlips/MyBorrows",
                    createdBy: "Hệ thống");

                return (true, "Đã gửi yêu cầu gia hạn thành công. Vui lòng chờ thủ thư duyệt.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Lỗi hệ thống: {ex.Message}");
            }
        }
    }
}
