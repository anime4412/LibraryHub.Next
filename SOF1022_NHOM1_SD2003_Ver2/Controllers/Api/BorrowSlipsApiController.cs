using Microsoft.AspNetCore.Mvc;
using SOF1022_NHOM1_SD2003_Ver2.Attributes;
using SOF1022_NHOM1_SD2003_Ver2.Services;

using Microsoft.EntityFrameworkCore;
using SOF1022_NHOM1_SD2003_Ver2.Datas;
using SOF1022_NHOM1_SD2003_Ver2.Helpers;

namespace SOF1022_NHOM1_SD2003_Ver2.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class BorrowSlipsApiController : ControllerBase
    {
        private readonly IBorrowService _borrowService;
        private readonly LibraryDbContext _context;

        public BorrowSlipsApiController(IBorrowService borrowService, LibraryDbContext context)
        {
            _borrowService = borrowService;
            _context = context;
        }

        [HttpPost("/api/borrows")]
        [SessionAuthorize]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> CreateBorrowSlip([FromBody] CreateBorrowSlipRequest request)
        {
            if (request == null || request.ReaderId <= 0 || request.BookIds == null || !request.BookIds.Any())
            {
                return BadRequest("Vui lòng chọn độc giả và ít nhất một cuốn sách");
            }

            DateTime? dueDate = null;
            if (!string.IsNullOrWhiteSpace(request.DueDate) && DateTime.TryParse(request.DueDate, out var parsedDueDate))
            {
                dueDate = parsedDueDate;
            }

            var result = await _borrowService.BorrowAsync(request.ReaderId, request.BookIds, dueDate, request.Note);

            if (!result.Success)
            {
                return BadRequest(new { success = false, message = result.Message });
            }

            return Ok(new { success = true, message = result.Message });
        }

        [HttpPost("quick-borrow")]
        [SessionAuthorize]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> QuickBorrow([FromBody] QuickBorrowRequest request)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int readerId))
            {
                return BadRequest(new { success = false, message = "Vui lòng đăng nhập" });
            }

            if (request == null || request.BookId <= 0)
            {
                return BadRequest(new { success = false, message = "Thiếu thông tin sách cần mượn" });
            }

            // Dùng chung IBorrowService để đảm bảo mọi luồng mượn sách (trang web, API,
            // mượn nhanh từ trang chi tiết sách) đều tuân theo đúng một bộ quy tắc nghiệp vụ.
            var result = await _borrowService.BorrowAsync(readerId, new List<int> { request.BookId });

            if (!result.Success)
            {
                return BadRequest(new { success = false, message = result.Message });
            }

            return Ok(new { success = true, message = result.Message });
        }

        [HttpGet("scan-lookup")]
        [SessionAuthorize]
        public async Task<IActionResult> ScanLookup([FromQuery] string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return BadRequest(new { success = false, message = "Mã quét không được để trống" });

            code = code.Trim();

            bool isNumber = int.TryParse(code, out int parsedId);

            // 1. Tìm theo sách (ISBN hoặc Id)
            var book = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.ISBN == code || (isNumber && b.Id == parsedId));

            if (book != null)
            {
                var committed = await BookAvailabilityHelper.GetCommittedCountAsync(_context, book.Id);
                var realAvailable = Math.Max(0, book.Quantity - committed);
                return Ok(new
                {
                    success = true,
                    type = "book",
                    id = book.Id,
                    title = book.Title,
                    isbn = book.ISBN,
                    author = book.Author?.Name ?? "N/A",
                    category = book.Category?.Name ?? "N/A",
                    available = realAvailable,
                    total = book.Quantity,
                    cover = book.CoverImage,
                    price = book.Price
                });
            }

            // 2. Tìm theo độc giả (ReaderCode hoặc CCCD hoặc Phone)
            var reader = await _context.Readers
                .FirstOrDefaultAsync(r => r.ReaderCode == code || r.IdentityNumber == code || r.Phone == code);

            if (reader != null)
            {
                return Ok(new
                {
                    success = true,
                    type = "reader",
                    id = reader.Id,
                    code = reader.ReaderCode,
                    name = reader.FullName,
                    phone = reader.Phone,
                    maxBorrow = reader.MaxBorrowBooks,
                    isActive = reader.IsActive
                });
            }

            return NotFound(new { success = false, message = $"Không tìm thấy sách hoặc độc giả với mã: {code}" });
        }
    }

    public class QuickBorrowRequest
    {
        public int BookId { get; set; }
    }

    public class CreateBorrowSlipRequest
    {
        public int ReaderId { get; set; }
        public List<int> BookIds { get; set; } = new();
        public string? DueDate { get; set; }
        public string? Note { get; set; }
    }
}
