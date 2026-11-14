using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SOF1022_NHOM1_SD2003_Ver2.Attributes;
using SOF1022_NHOM1_SD2003_Ver2.Datas;
using SOF1022_NHOM1_SD2003_Ver2.Hubs;
using SOF1022_NHOM1_SD2003_Ver2.Models;
using SOF1022_NHOM1_SD2003_Ver2.Models.DTOs;
using SOF1022_NHOM1_SD2003_Ver2.Models.Enums;

namespace SOF1022_NHOM1_SD2003_Ver2.Controllers
{
    [SessionAuthorize]
    public class SupportController : Controller
    {
        private readonly LibraryDbContext _context;
        private readonly IHubContext<SupportChatHub> _hubContext;

        public SupportController(LibraryDbContext context, IHubContext<SupportChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // GET: Support/MyTicket
        [HttpGet]
        public async Task<IActionResult> MyTicket()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!int.TryParse(userIdString, out int readerId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Get the most recent Open or InProgress ticket for this reader
            var ticket = await _context.SupportTickets
                .Include(st => st.Messages)
                .Where(st => st.ReaderId == readerId &&
                           (st.Status == SupportTicketStatus.Open || st.Status == SupportTicketStatus.InProgress))
                .OrderByDescending(st => st.LastMessageDate)
                .FirstOrDefaultAsync();

            if (ticket == null)
            {
                return Json(new { success = true, ticket = (TicketDto?)null });
            }

            // Mark as read by reader
            ticket.UnreadByReader = false;
            await _context.SaveChangesAsync();

            // QUAN TRỌNG: map sang DTO trước khi trả JSON. Không trả thẳng entity
            // "ticket" vì SupportTickets.Messages <-> SupportMessages.SupportTicket
            // tham chiếu vòng, sẽ làm System.Text.Json crash giữa chừng khi serialize.
            var ticketDto = new TicketDto
            {
                Id = ticket.Id,
                TicketCode = ticket.TicketCode,
                Subject = ticket.Subject,
                Status = ticket.Status,
                UnreadByReader = ticket.UnreadByReader,
                Messages = ticket.Messages
                    .OrderBy(m => m.SentDate) // tin cũ trước, tin mới sau (giống khung chat admin)
                    .Select(m => new TicketMessageDto
                    {
                        Id = m.Id,
                        SenderType = m.SenderType,
                        SenderId = m.SenderId,
                        Content = m.Content,
                        SentDate = m.SentDate.ToString("yyyy-MM-dd HH:mm:ss")
                    })
                    .ToList()
            };

            return Json(new { success = true, ticket = ticketDto });
        }

        // POST: Support/CreateTicket
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTicket([FromBody] CreateTicketRequest request)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để gửi yêu cầu hỗ trợ." });
            }

            if (!int.TryParse(userIdString, out int readerId))
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để gửi yêu cầu hỗ trợ." });
            }

            if (string.IsNullOrWhiteSpace(request.FirstMessage))
            {
                return Json(new { success = false, message = "Vui lòng nhập nội dung yêu cầu." });
            }

            // Check if reader already has an Open or InProgress ticket
            var existingTicket = await _context.SupportTickets
                .Where(st => st.ReaderId == readerId &&
                           (st.Status == SupportTicketStatus.Open || st.Status == SupportTicketStatus.InProgress))
                .FirstOrDefaultAsync();

            if (existingTicket != null)
            {
                return Json(new { success = false, message = "Bạn đang có yêu cầu hỗ trợ chưa được giải quyết. Vui lòng chờ nhân viên phản hồi." });
            }

            // Generate ticket code (similar to BorrowCode format)
            var ticketCode = "SP" + DateTime.Now.ToString("yyyyMMddHHmmss");

            var ticket = new SupportTickets
            {
                TicketCode = ticketCode,
                ReaderId = readerId,
                Subject = request.Subject,
                Status = SupportTicketStatus.Open,
                CreatedDate = DateTime.Now,
                LastMessageDate = DateTime.Now,
                UnreadByReader = false,
                UnreadByStaff = true
            };

            _context.SupportTickets.Add(ticket);
            await _context.SaveChangesAsync();

            // Add first message
            var message = new SupportMessages
            {
                SupportTicketId = ticket.Id,
                SenderType = SupportSenderType.Reader,
                SenderId = readerId,
                Content = request.FirstMessage,
                SentDate = DateTime.Now
            };

            _context.SupportMessages.Add(message);
            await _context.SaveChangesAsync();

            // Broadcast to staff queue
            await _hubContext.Clients.Group("staff-queue").SendAsync("NewTicketCreated", ticket.Id);

            return Json(new { success = true, ticketId = ticket.Id, ticketCode = ticket.TicketCode });
        }

        // POST: Support/SendMessage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập." });
            }

            if (!int.TryParse(userIdString, out int readerId))
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập." });
            }

            if (string.IsNullOrWhiteSpace(request.Content))
            {
                return Json(new { success = false, message = "Vui lòng nhập nội dung tin nhắn." });
            }

            var ticket = await _context.SupportTickets.FindAsync(request.TicketId);
            if (ticket == null)
            {
                return Json(new { success = false, message = "Không tìm thấy yêu cầu hỗ trợ." });
            }

            // Validate that this ticket belongs to the current reader
            if (ticket.ReaderId != readerId)
            {
                return Json(new { success = false, message = "Bạn không có quyền gửi tin nhắn vào yêu cầu này." });
            }

            // Check if ticket is closed
            if (ticket.Status == SupportTicketStatus.Closed)
            {
                return Json(new { success = false, message = "Yêu cầu này đã được đóng. Vui lòng tạo yêu cầu mới nếu cần hỗ trợ." });
            }

            // Add message
            var message = new SupportMessages
            {
                SupportTicketId = request.TicketId,
                SenderType = SupportSenderType.Reader,
                SenderId = readerId,
                Content = request.Content,
                SentDate = DateTime.Now
            };

            _context.SupportMessages.Add(message);

            // Update ticket
            ticket.LastMessageDate = DateTime.Now;
            ticket.UnreadByStaff = true;

            await _context.SaveChangesAsync();

            // Broadcast to ticket group
            var messageData = new
            {
                id = message.Id,
                senderType = "Reader",
                senderId = readerId,
                content = request.Content,
                sentDate = message.SentDate.ToString("yyyy-MM-dd HH:mm:ss")
            };

            await _hubContext.Clients.Group($"ticket-{request.TicketId}").SendAsync("NewMessage", request.TicketId, messageData);

            // Trả về messageData (DTO ẩn danh) thay vì entity "message" để tránh
            // cùng rủi ro object-cycle như MyTicket() ở trên.
            return Json(new { success = true, message = messageData });
        }
    }
}
