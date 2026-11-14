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
    public class BookCommentController : ControllerBase
    {
        private readonly LibraryDbContext _context;

        // Danh sách từ ngữ không phù hợp cần lọc
        private static readonly HashSet<string> BannedWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // Từ ngữ thô tục tiếng Việt
            "đm", "dmm", "địt", "địt mẹ", "địt mẹ mày", "địt mẹ nó", "địt mẹ chúng mày",
            "cặc", "cặc con", "cặc chó", "buồi", "buồi trâu", "lồn", "lồn chó", "lồn mẹ",
            "đéo", "đéo gì", "đéo có", "đéo được", "đm", "dcm", "dcmm", "vl", "vkl", "vlmm",
            "phết", "phết cái", "phết con", "mẹ mày", "mẹ nó", "mẹ chúng mày", "cha mày", "cha nó",
            "đĩ", "đĩ cái", "đĩ già", "đĩ trẻ", "khốn", "khốn nạn", "khốn kiếp", "khốn khiếp",
            "chó", "chó chết", "chó má", "chó đẻ", "chó cái", "chó đẻ", "súc vật", "súc sinh",
            "ngu", "ngu ngốc", "ngu xuẩn", "ngu ngốc", "đần", "đần độn", "khùng", "khùng điên",
            "thằng", "thằng chó", "thằng ngu", "con chó", "con đĩ", "con hoang", "con điếm",
            "fuck", "shit", "bitch", "bastard", "asshole", "dick", "pussy", "cock",
            "ngu", "ngu vl", "ngu vcl", "ngu đần", "ngu xuẩn", "ngu ngốc", "ngu si",
            "óc chó", "óc dế", "óc cặc", "mất dạy", "hư hỏng", "đê tiện", "hèn hạ"
        };

        public BookCommentController(LibraryDbContext context)
        {
            _context = context;
        }

        // Hàm kiểm tra nội dung có chứa từ ngữ cấm
        private bool ContainsBannedWords(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return false;

            string normalizedContent = content.ToLower();
            
            foreach (var bannedWord in BannedWords)
            {
                if (normalizedContent.Contains(bannedWord.ToLower()))
                    return true;
            }
            
            return false;
        }

        // POST: api/BookComment/add
        [HttpPost("add")]
        [SessionAuthorize]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> AddComment([FromBody] AddCommentRequest request)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                return BadRequest(new { success = false, message = "Vui lòng đăng nhập" });
            }

            if (string.IsNullOrWhiteSpace(request.Content))
            {
                return BadRequest(new { success = false, message = "Nội dung bình luận không được để trống" });
            }

            // Kiểm tra từ ngữ không phù hợp
            if (ContainsBannedWords(request.Content))
            {
                return BadRequest(new { success = false, message = "Bình luận chứa từ ngữ không phù hợp. Vui lòng sử dụng ngôn ngữ lịch sự." });
            }

            int readerId = int.Parse(userIdString);

            // Check if user is blocked from commenting
            var reader = await _context.Readers.FindAsync(readerId);
            if (reader != null && reader.IsCommentBlocked)
            {
                // Check if block has expired
                if (reader.CommentBlockedUntil.HasValue && reader.CommentBlockedUntil.Value > DateTime.Now)
                {
                    return BadRequest(new { 
                        success = false, 
                        message = $"Tài khoản của bạn đã bị chặn bình luận đến ngày {reader.CommentBlockedUntil.Value:dd/MM/yyyy} do vi phạm quy định." 
                    });
                }
                else
                {
                    // Unblock if expired
                    reader.IsCommentBlocked = false;
                    reader.CommentBlockedUntil = null;
                    await _context.SaveChangesAsync();
                }
            }

            var comment = new BookComment
            {
                BookId = request.BookId,
                ReaderId = readerId,
                Content = request.Content,
                ParentCommentId = request.ParentCommentId,
                CreatedAt = DateTime.Now,
                Likes = 0
            };

            _context.BookComments.Add(comment);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Bình luận thành công",
                comment = new
                {
                    id = comment.Id,
                    content = comment.Content,
                    author = reader?.FullName ?? "Người dùng",
                    avatar = reader?.FullName?.Substring(0, 1).ToUpper() ?? "U",
                    date = comment.CreatedAt.ToString("o"), // ISO 8601 format for JS parsing
                    likes = 0
                }
            });
        }

        // GET: api/BookComment/list
        [HttpGet("list")]
        public async Task<IActionResult> GetComments(int bookId, int page = 1, int pageSize = 10)
        {
            var comments = await _context.BookComments
                .Include(c => c.Reader)
                .Include(c => c.Replies)
                .Where(c => c.BookId == bookId && c.ParentCommentId == null)
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new
                {
                    id = c.Id,
                    content = c.Content,
                    author = c.Reader.FullName,
                    avatar = c.Reader.Avatar, // Use actual avatar URL from Reader table
                    readerCode = c.Reader.ReaderCode, // Include ReaderCode for profile navigation
                    readerId = c.ReaderId, // Include ReaderId for self-report check
                    date = c.CreatedAt.ToString("o"), // ISO 8601 format for JS parsing
                    likes = c.Likes,
                    replyCount = c.Replies != null ? c.Replies.Count : 0
                })
                .ToListAsync();

            var totalCount = await _context.BookComments
                .CountAsync(c => c.BookId == bookId && c.ParentCommentId == null);

            return Ok(new { success = true, comments, totalCount, page, pageSize });
        }

        // DELETE: api/BookComment/delete
        [HttpDelete("delete")]
        [SessionAuthorize]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                return BadRequest(new { success = false, message = "Vui lòng đăng nhập" });
            }

            int readerId = int.Parse(userIdString);

            var comment = await _context.BookComments
                .FirstOrDefaultAsync(c => c.Id == commentId && c.ReaderId == readerId);

            if (comment == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy bình luận" });
            }

            _context.BookComments.Remove(comment);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Đã xóa bình luận" });
        }

        // POST: api/BookComment/like
        [HttpPost("like")]
        [SessionAuthorize]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> LikeComment([FromBody] LikeCommentRequest request)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                return BadRequest(new { success = false, message = "Vui lòng đăng nhập" });
            }

            int readerId = int.Parse(userIdString);

            var comment = await _context.BookComments.FindAsync(request.CommentId);
            if (comment == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy bình luận" });
            }

            var existingLike = await _context.BookCommentLikes
                .FirstOrDefaultAsync(l => l.BookCommentId == request.CommentId && l.ReaderId == readerId);

            if (existingLike != null)
            {
                _context.BookCommentLikes.Remove(existingLike);
                comment.Likes--;
            }
            else
            {
                var like = new BookCommentLike
                {
                    BookCommentId = request.CommentId,
                    ReaderId = readerId,
                    CreatedAt = DateTime.Now
                };
                _context.BookCommentLikes.Add(like);
                comment.Likes++;
            }

            await _context.SaveChangesAsync();

            return Ok(new { success = true, likes = comment.Likes });
        }

        // GET: api/BookComment/replies
        [HttpGet("replies")]
        public async Task<IActionResult> GetReplies(int parentCommentId)
        {
            var replies = await _context.BookComments
                .Include(c => c.Reader)
                .Where(c => c.ParentCommentId == parentCommentId)
                .OrderBy(c => c.CreatedAt)
                .Select(c => new
                {
                    id = c.Id,
                    content = c.Content,
                    author = c.Reader.FullName,
                    avatar = c.Reader.Avatar, // Use actual avatar URL from Reader table
                    readerCode = c.Reader.ReaderCode, // Include ReaderCode for profile navigation
                    readerId = c.ReaderId, // Include ReaderId for self-report check
                    date = c.CreatedAt.ToString("o"), // ISO 8601 format for JS parsing
                    likes = c.Likes
                })
                .ToListAsync();

            return Ok(new { success = true, replies });
        }

        // POST: api/BookComment/report
        [HttpPost("report")]
        [SessionAuthorize]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> ReportComment([FromBody] ReportCommentRequest request)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                return BadRequest(new { success = false, message = "Vui lòng đăng nhập" });
            }

            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                return BadRequest(new { success = false, message = "Vui lòng cung cấp lý do báo cáo" });
            }

            int reporterId = int.Parse(userIdString);

            // Check if comment exists
            var comment = await _context.BookComments
                .Include(c => c.Reader)
                .FirstOrDefaultAsync(c => c.Id == request.CommentId);

            if (comment == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy bình luận" });
            }

            // Prevent self-reporting
            if (comment.ReaderId == reporterId)
            {
                return BadRequest(new { success = false, message = "Không thể báo cáo bình luận của chính mình" });
            }

            // Check if already reported by this user
            var existingReport = await _context.CommentReports
                .FirstOrDefaultAsync(r => r.CommentId == request.CommentId && r.ReporterId == reporterId);

            if (existingReport != null)
            {
                return BadRequest(new { success = false, message = "Bạn đã báo cáo bình luận này rồi" });
            }

            // Create report
            var report = new CommentReport
            {
                CommentId = request.CommentId,
                ReporterId = reporterId,
                ReportedUserId = comment.ReaderId,
                Reason = request.Reason,
                CreatedAt = DateTime.Now
            };

            _context.CommentReports.Add(report);

            // Count total reports for this user
            int totalReports = await _context.CommentReports
                .CountAsync(r => r.ReportedUserId == comment.ReaderId);

            string message = "Đã gửi báo cáo thành công";

            // Apply penalties based on report count
            if (totalReports == 1)
            {
                // First report: warning
                message = "Đã gửi báo cáo. Đây là lần báo cáo đầu tiên đối với người dùng này.";
            }
            else if (totalReports >= 2)
            {
                // Second or more reports: block commenting
                comment.Reader.IsCommentBlocked = true;
                comment.Reader.CommentBlockedUntil = DateTime.Now.AddDays(7); // Block for 7 days
                message = "Đã gửi báo cáo. Người dùng này đã bị chặn bình luận trong 7 ngày do vi phạm nhiều lần.";
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = message,
                reportCount = totalReports,
                isBlocked = comment.Reader.IsCommentBlocked
            });
        }
    }

    public class AddCommentRequest
    {
        public int BookId { get; set; }
        public string Content { get; set; }
        public int? ParentCommentId { get; set; }
    }

    public class LikeCommentRequest
    {
        public int CommentId { get; set; }
    }

    public class ReportCommentRequest
    {
        public int CommentId { get; set; }
        public string Reason { get; set; }
    }
}
