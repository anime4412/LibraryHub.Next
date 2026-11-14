using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SOF1022_NHOM1_SD2003_Ver2.Attributes;
using SOF1022_NHOM1_SD2003_Ver2.Datas;
using SOF1022_NHOM1_SD2003_Ver2.Models;

namespace SOF1022_NHOM1_SD2003_Ver2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [SessionAuthorize]
    public class ReadingController : ControllerBase
    {
        private readonly LibraryDbContext _context;

        public ReadingController(LibraryDbContext context)
        {
            _context = context;
        }

        // POST: api/Reading/start
        [HttpPost("start")]
        [SessionAuthorize]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> StartReading([FromBody] StartReadingRequest request)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                return BadRequest(new { success = false, message = "Vui lòng đăng nhập" });
            }

            int readerId = int.Parse(userIdString);

            var book = await _context.Books.FindAsync(request.BookId);
            if (book == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy sách" });
            }

            var existingHistory = await _context.ReadingHistories
                .FirstOrDefaultAsync(r => r.BookId == request.BookId && r.ReaderId == readerId);

            if (existingHistory != null)
            {
                existingHistory.LastReadAt = DateTime.Now;
                existingHistory.CurrentPage = request.CurrentPage ?? 1;
                _context.ReadingHistories.Update(existingHistory);
            }
            else
            {
                var readingHistory = new ReadingHistory
                {
                    BookId = request.BookId,
                    ReaderId = readerId,
                    CurrentPage = request.CurrentPage ?? 1,
                    TotalPages = book.PageCount,
                    StartedAt = DateTime.Now,
                    LastReadAt = DateTime.Now
                };
                _context.ReadingHistories.Add(readingHistory);
            }

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Đã bắt đầu đọc" });
        }

        // POST: api/Reading/progress
        [HttpPost("progress")]
        [SessionAuthorize]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> UpdateProgress([FromBody] UpdateProgressRequest request)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                return BadRequest(new { success = false, message = "Vui lòng đăng nhập" });
            }

            int readerId = int.Parse(userIdString);

            var readingHistory = await _context.ReadingHistories
                .FirstOrDefaultAsync(r => r.BookId == request.BookId && r.ReaderId == readerId);

            if (readingHistory == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy lịch sử đọc" });
            }

            readingHistory.CurrentPage = request.CurrentPage;
            readingHistory.LastReadAt = DateTime.Now;

            _context.ReadingHistories.Update(readingHistory);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Đã cập nhật tiến độ" });
        }

        // GET: api/Reading/history
        [HttpGet("history")]
        [SessionAuthorize]
        public async Task<IActionResult> GetReadingHistory(int bookId)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                return Ok(new { currentPage = 1, totalPages = 0, lastReadAt = (DateTime?)null, startedAt = (DateTime?)null });
            }

            int readerId = int.Parse(userIdString);

            var history = await _context.ReadingHistories
                .Where(r => r.BookId == bookId && r.ReaderId == readerId)
                .Select(r => new
                {
                    currentPage = r.CurrentPage,
                    totalPages = r.TotalPages,
                    lastReadAt = (DateTime?)r.LastReadAt,
                    startedAt = (DateTime?)r.StartedAt
                })
                .FirstOrDefaultAsync();

            return Ok(history ?? new { currentPage = 1, totalPages = 0, lastReadAt = (DateTime?)null, startedAt = (DateTime?)null });
        }

        // GET: api/Reading/list
        [HttpGet("list")]
        [SessionAuthorize]
        public async Task<IActionResult> GetReadingList(int page = 1, int pageSize = 12)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                return BadRequest(new { success = false, message = "Vui lòng đăng nhập" });
            }

            int readerId = int.Parse(userIdString);

            var readingList = await _context.ReadingHistories
                .Include(r => r.Book)
                    .ThenInclude(b => b.Author)
                .Where(r => r.ReaderId == readerId)
                .OrderByDescending(r => r.LastReadAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new
                {
                    id = r.Book.Id,
                    title = r.Book.Title,
                    author = r.Book.Author.Name,
                    coverImage = r.Book.CoverImage,
                    currentPage = r.CurrentPage,
                    totalPages = r.TotalPages,
                    lastReadAt = r.LastReadAt.ToString("dd/MM/yyyy HH:mm"),
                    progress = r.TotalPages > 0 ? (r.CurrentPage * 100 / r.TotalPages) : 0
                })
                .ToListAsync();

            var totalCount = await _context.ReadingHistories
                .CountAsync(r => r.ReaderId == readerId);

            return Ok(new { success = true, readingList, totalCount, page, pageSize });
        }
    }

    public class StartReadingRequest
    {
        public int BookId { get; set; }
        public int? CurrentPage { get; set; }
    }

    public class UpdateProgressRequest
    {
        public int BookId { get; set; }
        public int CurrentPage { get; set; }
    }
}
