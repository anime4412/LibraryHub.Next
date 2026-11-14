using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SOF1022_NHOM1_SD2003_Ver2.Attributes;
using SOF1022_NHOM1_SD2003_Ver2.Datas;
using SOF1022_NHOM1_SD2003_Ver2.Hubs;
using SOF1022_NHOM1_SD2003_Ver2.Models;
using SOF1022_NHOM1_SD2003_Ver2.Models.Enums;

namespace SOF1022_NHOM1_SD2003_Ver2.Controllers
{
    [SessionAuthorize]
    public class SupportTicketsController : Controller
    {
        private readonly LibraryDbContext _context;
        private readonly IHubContext<SupportChatHub> _hubContext;

        public SupportTicketsController(LibraryDbContext context, IHubContext<SupportChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // GET: SupportTickets
        [PermissionAuthorize("support.view")]
        public async Task<IActionResult> Index(SupportTicketStatus? statusFilter = null)
        {
            var userRole = HttpContext.Request.Query.ContainsKey("preview") ? "Admin" : (HttpContext.Session.GetString("Role") ?? "Reader");
            if (string.IsNullOrEmpty(userRole) || userRole == "Reader")
            {
                return RedirectToAction("Index", "Home");
            }

            var query = _context.SupportTickets
                .Include(st => st.Reader)
                .Include(st => st.AssignedStaff)
                .AsQueryable();

            if (statusFilter.HasValue)
            {
                query = query.Where(st => st.Status == statusFilter.Value);
            }

            var tickets = await query
                .OrderByDescending(st => st.Status == SupportTicketStatus.Open)
                .ThenByDescending(st => st.LastMessageDate)
                .ToListAsync();

            ViewData["StatusFilter"] = statusFilter;
            ViewData["OpenCount"] = await _context.SupportTickets.CountAsync(st => st.Status == SupportTicketStatus.Open);

            return View(tickets);
        }

        // GET: SupportTickets/Details/5
        [PermissionAuthorize("support.view")]
        public async Task<IActionResult> Details(int id)
        {
            var userRole = HttpContext.Session.GetString("Role") ?? "Reader";
            if (string.IsNullOrEmpty(userRole) || userRole == "Reader")
            {
                return RedirectToAction("Index", "Home");
            }

            var ticket = await _context.SupportTickets
                .Include(st => st.Reader)
                .Include(st => st.AssignedStaff)
                .Include(st => st.Messages.OrderBy(m => m.SentDate))
                .FirstOrDefaultAsync(st => st.Id == id);

            if (ticket == null)
            {
                return NotFound();
            }

            // Mark as read by staff
            ticket.UnreadByStaff = false;
            await _context.SaveChangesAsync();

            return View(ticket);
        }

        // POST: SupportTickets/Claim
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize("support.reply")]
        public async Task<IActionResult> Claim(int id)
        {
            var userRole = HttpContext.Session.GetString("Role") ?? "Reader";
            if (string.IsNullOrEmpty(userRole) || userRole == "Reader")
            {
                return Json(new { success = false, message = "Bạn không có quyền thực hiện hành động này." });
            }

            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int staffId))
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin nhân viên." });
            }

            var ticket = await _context.SupportTickets.FindAsync(id);
            if (ticket == null)
            {
                return Json(new { success = false, message = "Không tìm thấy yêu cầu hỗ trợ." });
            }

            if (ticket.Status != SupportTicketStatus.Open)
            {
                return Json(new { success = false, message = "Yêu cầu này đã được nhận hoặc đã đóng." });
            }

            // staffId là Readers.Id của tài khoản Admin/Librarian đang đăng nhập
            // (KHÔNG phải Employees.Id — xem ghi chú trong Models/SupportTickets.cs)
            var staffAccount = await _context.Readers.FindAsync(staffId);
            if (staffAccount == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin nhân viên." });
            }

            // Claim the ticket
            ticket.AssignedStaffId = staffId;
            ticket.Status = SupportTicketStatus.InProgress;
            ticket.LastMessageDate = DateTime.Now;

            await _context.SaveChangesAsync();

            // Add system message
            var systemMessage = new SupportMessages
            {
                SupportTicketId = id,
                SenderType = SupportSenderType.System,
                SenderId = null,
                Content = $"Nhân viên {staffAccount.FullName} đã nhận xử lý yêu cầu của bạn.",
                SentDate = DateTime.Now
            };

            _context.SupportMessages.Add(systemMessage);
            await _context.SaveChangesAsync();

            // Broadcast to staff queue and ticket group
            await _hubContext.Clients.Group("staff-queue").SendAsync("TicketClaimed", id, staffId, staffAccount.FullName);
            await _hubContext.Clients.Group($"ticket-{id}").SendAsync("TicketClaimed", id, staffId, staffAccount.FullName);

            return Json(new { success = true, message = "Đã nhận xử lý yêu cầu thành công." });
        }

        // POST: SupportTickets/Reply
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize("support.reply")]
        public async Task<IActionResult> Reply(int id, string content)
        {
            var userRole = HttpContext.Session.GetString("Role") ?? "Reader";
            if (string.IsNullOrEmpty(userRole) || userRole == "Reader")
            {
                return Json(new { success = false, message = "Bạn không có quyền thực hiện hành động này." });
            }

            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int staffId))
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin nhân viên." });
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                return Json(new { success = false, message = "Vui lòng nhập nội dung tin nhắn." });
            }

            var ticket = await _context.SupportTickets.FindAsync(id);
            if (ticket == null)
            {
                return Json(new { success = false, message = "Không tìm thấy yêu cầu hỗ trợ." });
            }

            if (ticket.Status == SupportTicketStatus.Closed)
            {
                return Json(new { success = false, message = "Yêu cầu này đã được đóng." });
            }

            // Add message
            var message = new SupportMessages
            {
                SupportTicketId = id,
                SenderType = SupportSenderType.Employee,
                SenderId = staffId,
                Content = content,
                SentDate = DateTime.Now
            };

            _context.SupportMessages.Add(message);

            // Update ticket
            ticket.LastMessageDate = DateTime.Now;
            ticket.UnreadByReader = true;

            // If ticket is still Open, auto-claim it when staff replies
            if (ticket.Status == SupportTicketStatus.Open)
            {
                ticket.AssignedStaffId = staffId;
                ticket.Status = SupportTicketStatus.InProgress;
            }

            await _context.SaveChangesAsync();

            // Broadcast to ticket group
            var messageData = new
            {
                id = message.Id,
                senderType = "Employee",
                senderId = staffId,
                content = content,
                sentDate = message.SentDate.ToString("yyyy-MM-dd HH:mm:ss")
            };

            await _hubContext.Clients.Group($"ticket-{id}").SendAsync("NewMessage", id, messageData);

            // Trả về messageData thay vì entity "message" — tránh rủi ro object-cycle
            // giống lỗi đã sửa trước đó ở SupportController.SendMessage().
            return Json(new { success = true, message = messageData });
        }

        // POST: SupportTickets/Close
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize("support.reply")]
        public async Task<IActionResult> Close(int id)
        {
            var userRole = HttpContext.Session.GetString("Role") ?? "Reader";
            if (string.IsNullOrEmpty(userRole) || userRole == "Reader")
            {
                return Json(new { success = false, message = "Bạn không có quyền thực hiện hành động này." });
            }

            var ticket = await _context.SupportTickets.FindAsync(id);
            if (ticket == null)
            {
                return Json(new { success = false, message = "Không tìm thấy yêu cầu hỗ trợ." });
            }

            if (ticket.Status == SupportTicketStatus.Closed)
            {
                return Json(new { success = false, message = "Yêu cầu này đã được đóng." });
            }

            // Close the ticket
            ticket.Status = SupportTicketStatus.Closed;
            ticket.ClosedDate = DateTime.Now;
            ticket.LastMessageDate = DateTime.Now;

            await _context.SaveChangesAsync();

            // Add system message
            var systemMessage = new SupportMessages
            {
                SupportTicketId = id,
                SenderType = SupportSenderType.System,
                SenderId = null,
                Content = "Yêu cầu đã được đóng.",
                SentDate = DateTime.Now
            };

            _context.SupportMessages.Add(systemMessage);
            await _context.SaveChangesAsync();

            // Broadcast to ticket group and staff queue
            await _hubContext.Clients.Group($"ticket-{id}").SendAsync("TicketClosed", id);
            await _hubContext.Clients.Group("staff-queue").SendAsync("TicketClosed", id);

            return Json(new { success = true, message = "Đã đóng yêu cầu thành công." });
        }
    }
}
