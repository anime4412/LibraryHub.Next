using Microsoft.AspNetCore.Mvc;
using SOF1022_NHOM1_SD2003_Ver2.Attributes;
using SOF1022_NHOM1_SD2003_Ver2.Models.DTOs;
using SOF1022_NHOM1_SD2003_Ver2.Models.Enums;
using SOF1022_NHOM1_SD2003_Ver2.Services;

namespace SOF1022_NHOM1_SD2003_Ver2.Controllers
{
    [SessionAuthorize]
    public class NotificationsController : Controller
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        private bool TryGetUserId(out int userId)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            return int.TryParse(userIdString, out userId);
        }

        // GET: Notifications  -> Hộp thư thông báo của người dùng hiện tại
        public async Task<IActionResult> Index()
        {
            if (!TryGetUserId(out int userId))
                return RedirectToAction("Login", "Account");

            ViewData["Title"] = "Thông báo của tôi";
            var notifications = await _notificationService.GetForUserAsync(userId, unreadOnly: false, take: 100);
            return View(notifications);
        }

        // GET: Notifications/Recent -> dùng cho chuông thông báo (dropdown), gọi định kỳ bằng AJAX
        [HttpGet]
        public async Task<IActionResult> Recent()
        {
            if (!TryGetUserId(out int userId))
                return Json(new { success = false, message = "Vui lòng đăng nhập", unreadCount = 0, notifications = Array.Empty<NotificationDto>() });

            var unreadCount = await _notificationService.GetUnreadCountAsync(userId);
            var notifications = await _notificationService.GetForUserAsync(userId, unreadOnly: false, take: 8);

            return Json(new { success = true, unreadCount, notifications });
        }

        [HttpGet]
        public async Task<IActionResult> UnreadCount()
        {
            if (!TryGetUserId(out int userId))
                return Json(new { success = false, unreadCount = 0 });

            var unreadCount = await _notificationService.GetUnreadCountAsync(userId);
            return Json(new { success = true, unreadCount });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            if (!TryGetUserId(out int userId))
                return Json(new { success = false, message = "Vui lòng đăng nhập" });

            var ok = await _notificationService.MarkAsReadAsync(id, userId);
            return Json(new { success = ok });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllAsRead()
        {
            if (!TryGetUserId(out int userId))
                return Json(new { success = false, message = "Vui lòng đăng nhập" });

            var count = await _notificationService.MarkAllAsReadAsync(userId);
            return Json(new { success = true, count });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!TryGetUserId(out int userId))
                return Json(new { success = false, message = "Vui lòng đăng nhập" });

            var ok = await _notificationService.DeleteAsync(id, userId);
            return Json(new { success = ok });
        }

        // ================= ADMIN: gửi & quản lý thông báo hàng loạt =================

        [SessionAuthorize(Roles = "Admin,Librarian")]
        [PermissionAuthorize("notification.send")]
        public async Task<IActionResult> AdminIndex()
        {
            ViewData["Title"] = "Quản lý thông báo";
            var sent = await _notificationService.GetSentBroadcastsAsync(50);
            return View(sent);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize(Roles = "Admin,Librarian")]
        [PermissionAuthorize("notification.send")]
        public async Task<IActionResult> SendBroadcast(BroadcastRequestDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Message))
                return Json(new { success = false, message = "Vui lòng nhập đầy đủ tiêu đề và nội dung thông báo." });

            var senderName = HttpContext.Session.GetString("UserName") ?? "Quản trị viên";

            var count = await _notificationService.BroadcastAsync(
                request.TargetRole,
                NotificationType.AdminBroadcast,
                request.Title.Trim(),
                request.Message.Trim(),
                string.IsNullOrWhiteSpace(request.Link) ? null : request.Link.Trim(),
                senderName);

            if (count <= 0)
                return Json(new { success = false, message = "Không có người nhận phù hợp hoặc gửi thất bại." });

            return Json(new { success = true, message = $"Đã gửi thông báo tới {count} người dùng.", count });
        }
    }
}
