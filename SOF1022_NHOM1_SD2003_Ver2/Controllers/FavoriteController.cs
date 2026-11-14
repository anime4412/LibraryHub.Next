using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SOF1022_NHOM1_SD2003_Ver2.Attributes;
using SOF1022_NHOM1_SD2003_Ver2.Datas;
using SOF1022_NHOM1_SD2003_Ver2.Helpers;
using SOF1022_NHOM1_SD2003_Ver2.Models;

namespace SOF1022_NHOM1_SD2003_Ver2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [SessionAuthorize]
    public class FavoriteController : ControllerBase
    {
        private readonly LibraryDbContext _context;

        public FavoriteController(LibraryDbContext context)
        {
            _context = context;
        }

        // POST: api/Favorite/toggle
        [HttpPost("toggle")]
        [SessionAuthorize]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> ToggleFavorite([FromBody] ToggleFavoriteRequest request)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                return BadRequest(new { success = false, message = "Vui lòng đăng nhập" });
            }

            int readerId = int.Parse(userIdString);

            var existingFavorite = await _context.BookFavorites
                .FirstOrDefaultAsync(f => f.BookId == request.BookId && f.ReaderId == readerId);

            if (existingFavorite != null)
            {
                _context.BookFavorites.Remove(existingFavorite);
                await _context.SaveChangesAsync();
                return Ok(new { success = true, isFavorite = false, message = "Đã xóa khỏi yêu thích" });
            }
            else
            {
                var favorite = new BookFavorite
                {
                    BookId = request.BookId,
                    ReaderId = readerId,
                    CreatedAt = DateTime.Now
                };
                _context.BookFavorites.Add(favorite);
                await _context.SaveChangesAsync();
                return Ok(new { success = true, isFavorite = true, message = "Đã thêm vào yêu thích" });
            }
        }

        // GET: api/Favorite/check
        [HttpGet("check")]
        [SessionAuthorize]
        public async Task<IActionResult> IsFavorite(int bookId)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                return Ok(new { isFavorite = false });
            }

            int readerId = int.Parse(userIdString);

            var isFavorite = await _context.BookFavorites
                .AnyAsync(f => f.BookId == bookId && f.ReaderId == readerId);

            return Ok(new { isFavorite });
        }

        // GET: api/Favorite/list
        [HttpGet("list")]
        [SessionAuthorize]
        public async Task<IActionResult> GetFavorites(int page = 1, int pageSize = 12)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                return BadRequest(new { success = false, message = "Vui lòng đăng nhập" });
            }

            int readerId = int.Parse(userIdString);

            var favorites = await _context.BookFavorites
                .Include(f => f.Book)
                    .ThenInclude(b => b.Author)
                .Include(f => f.Book)
                    .ThenInclude(b => b.Category)
                .Where(f => f.ReaderId == readerId)
                .OrderByDescending(f => f.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Calculate real available counts for all books
            var bookIds = favorites.Select(f => f.Book.Id).ToList();
            var committedCounts = await BookAvailabilityHelper.GetCommittedCountsAsync(_context, bookIds);

            var result = favorites.Select(f => new
            {
                id = f.Book.Id,
                title = f.Book.Title,
                author = f.Book.Author.Name,
                category = f.Book.Category.Name,
                coverImage = f.Book.CoverImage,
                price = f.Book.Price,
                availableQuantity = Math.Max(0, f.Book.Quantity - (committedCounts.ContainsKey(f.Book.Id) ? committedCounts[f.Book.Id] : 0)),
                addedAt = f.CreatedAt.ToString("dd/MM/yyyy")
            }).ToList();

            var totalCount = await _context.BookFavorites
                .CountAsync(f => f.ReaderId == readerId);

            return Ok(new { success = true, favorites = result, totalCount, page, pageSize });
        }
    }

    public class ToggleFavoriteRequest
    {
        public int BookId { get; set; }
    }
}
