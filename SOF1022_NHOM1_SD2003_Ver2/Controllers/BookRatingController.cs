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
    public class BookRatingController : ControllerBase
    {
        private readonly LibraryDbContext _context;

        public BookRatingController(LibraryDbContext context)
        {
            _context = context;
        }

        // POST: api/BookRating/add
        [HttpPost("add")]
        [SessionAuthorize]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> AddRating([FromBody] AddRatingRequest request)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                return BadRequest(new { success = false, message = "Vui lòng đăng nhập" });
            }

            if (request.Rating < 1 || request.Rating > 5)
            {
                return BadRequest(new { success = false, message = "Đánh giá phải từ 1 đến 5 sao" });
            }

            int readerId = int.Parse(userIdString);

            var existingRating = await _context.BookRatings
                .FirstOrDefaultAsync(r => r.BookId == request.BookId && r.ReaderId == readerId);

            if (existingRating != null)
            {
                existingRating.Rating = request.Rating;
                existingRating.Review = request.Review;
                existingRating.CreatedAt = DateTime.Now;
                _context.BookRatings.Update(existingRating);
            }
            else
            {
                var newRating = new BookRating
                {
                    BookId = request.BookId,
                    ReaderId = readerId,
                    Rating = request.Rating,
                    Review = request.Review,
                    CreatedAt = DateTime.Now
                };
                _context.BookRatings.Add(newRating);
            }

            await _context.SaveChangesAsync();

            var stats = await GetRatingStats(request.BookId);

            return Ok(new
            {
                success = true,
                message = "Đánh giá thành công",
                averageRating = ((dynamic)stats).averageRating,
                totalRatings = ((dynamic)stats).totalRatings,
                distribution = ((dynamic)stats).distribution
            });
        }

        // DELETE: api/BookRating/delete
        [HttpDelete("delete")]
        [SessionAuthorize]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> DeleteRating(int bookId)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                return BadRequest(new { success = false, message = "Vui lòng đăng nhập" });
            }

            int readerId = int.Parse(userIdString);

            var rating = await _context.BookRatings
                .FirstOrDefaultAsync(r => r.BookId == bookId && r.ReaderId == readerId);

            if (rating == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy đánh giá" });
            }

            _context.BookRatings.Remove(rating);
            await _context.SaveChangesAsync();

            var stats = await GetRatingStats(bookId);

            return Ok(new
            {
                success = true,
                message = "Đã xóa đánh giá",
                averageRating = ((dynamic)stats).averageRating,
                totalRatings = ((dynamic)stats).totalRatings,
                distribution = ((dynamic)stats).distribution
            });
        }

        // GET: api/BookRating/stats
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats(int bookId)
        {
            var stats = await GetRatingStats(bookId);
            return Ok(stats);
        }

        // GET: api/BookRating/user-rating
        [HttpGet("user-rating")]
        [SessionAuthorize]
        public async Task<IActionResult> GetUserRating(int bookId)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                return Ok(new { rating = 0 });
            }

            int readerId = int.Parse(userIdString);

            var rating = await _context.BookRatings
                .Where(r => r.BookId == bookId && r.ReaderId == readerId)
                .Select(r => new { r.Rating, r.Review })
                .FirstOrDefaultAsync();

            if (rating != null)
            {
                return Ok(new { @rating = rating.Rating, review = rating.Review });
            }
            return Ok(new { @rating = 0, review = "" });
        }

        private async Task<object> GetRatingStats(int bookId)
        {
            var ratings = await _context.BookRatings
                .Where(r => r.BookId == bookId)
                .ToListAsync();

            if (!ratings.Any())
            {
                return new
                {
                    success = true,
                    averageRating = 0,
                    totalRatings = 0,
                    distribution = new { five = 0, four = 0, three = 0, two = 0, one = 0 }
                };
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

            return new
            {
                success = true,
                averageRating = averageRating.ToString("F1"),
                totalRatings = totalRatings,
                distribution
            };
        }
    }

    public class AddRatingRequest
    {
        public int BookId { get; set; }
        public int Rating { get; set; }
        public string? Review { get; set; }
    }
}
