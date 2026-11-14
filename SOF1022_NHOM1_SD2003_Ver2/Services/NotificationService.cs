using Microsoft.EntityFrameworkCore;
using SOF1022_NHOM1_SD2003_Ver2.Datas;
using SOF1022_NHOM1_SD2003_Ver2.Models;
using SOF1022_NHOM1_SD2003_Ver2.Models.DTOs;
using SOF1022_NHOM1_SD2003_Ver2.Models.Enums;
using Microsoft.AspNetCore.SignalR;
using SOF1022_NHOM1_SD2003_Ver2.Hubs;

namespace SOF1022_NHOM1_SD2003_Ver2.Services
{
    public class NotificationService : INotificationService
    {
        private readonly LibraryDbContext _context;
        private readonly ILogger<NotificationService> _logger;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(LibraryDbContext context, ILogger<NotificationService> logger, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _logger = logger;
            _hubContext = hubContext;
        }

        public async Task NotifyUserAsync(int recipientId, string recipientType, NotificationType type, string title, string message, string? link = null, string? createdBy = null)
        {
            if (recipientId <= 0 || string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(message))
                return;

            try
            {
                var notification = new Notification
                {
                    RecipientId = recipientId,
                    RecipientType = recipientType,
                    Type = type,
                    Title = title,
                    Message = message,
                    Link = link,
                    CreatedBy = createdBy ?? "Hệ thống",
                    CreatedDate = DateTime.Now
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                // Send real-time notification via SignalR
                var notificationDto = NotificationDto.FromEntity(notification);
                await _hubContext.Clients.Group($"user-{recipientId}").SendAsync("ReceiveNotification", notificationDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Không thể tạo thông báo cho người dùng {RecipientId}", recipientId);
            }
        }

        public async Task<int> NotifyManyAsync(IEnumerable<int> recipientIds, string recipientType, NotificationType type, string title, string message, string? link, string? createdBy, Guid? broadcastGroupId = null)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(message))
                return 0;

            var ids = (recipientIds ?? Enumerable.Empty<int>()).Distinct().Where(id => id > 0).ToList();
            if (ids.Count == 0)
                return 0;

            try
            {
                var now = DateTime.Now;
                var groupId = broadcastGroupId ?? Guid.NewGuid();
                var notifications = new List<Notification>();

                foreach (var id in ids)
                {
                    var notification = new Notification
                    {
                        RecipientId = id,
                        RecipientType = recipientType,
                        Type = type,
                        Title = title,
                        Message = message,
                        Link = link,
                        CreatedBy = createdBy ?? "Hệ thống",
                        CreatedDate = now,
                        BroadcastGroupId = groupId
                    };
                    notifications.Add(notification);
                    _context.Notifications.Add(notification);
                }

                await _context.SaveChangesAsync();

                // Send real-time notifications via SignalR
                foreach (var id in ids)
                {
                    var notificationDto = notifications.First(n => n.RecipientId == id);
                    var dto = NotificationDto.FromEntity(notificationDto);
                    await _hubContext.Clients.Group($"user-{id}").SendAsync("ReceiveNotification", dto);
                }

                return ids.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Không thể gửi thông báo hàng loạt");
                return 0;
            }
        }

        public async Task<int> BroadcastAsync(string? targetRole, NotificationType type, string title, string message, string? link, string? createdBy)
        {
            var query = _context.Readers.Where(r => r.IsActive && !r.IsDeleted);

            string recipientType = "Reader";
            if (!string.IsNullOrWhiteSpace(targetRole) && targetRole != "All")
            {
                query = query.Where(r => r.Role == targetRole);
                recipientType = targetRole;
            }

            var recipientIds = await query.Select(r => r.Id).ToListAsync();

            return await NotifyManyAsync(recipientIds, recipientType, type, title, message, link, createdBy);
        }

        public async Task<List<NotificationDto>> GetForUserAsync(int userId, bool unreadOnly = false, int take = 50)
        {
            var query = _context.Notifications.Where(n => n.RecipientId == userId);

            if (unreadOnly)
                query = query.Where(n => !n.IsRead);

            var notifications = await query
                .OrderByDescending(n => n.CreatedDate)
                .Take(take)
                .ToListAsync();

            return notifications.Select(NotificationDto.FromEntity).ToList();
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _context.Notifications.CountAsync(n => n.RecipientId == userId && !n.IsRead);
        }

        public async Task<bool> MarkAsReadAsync(int notificationId, int userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.RecipientId == userId);

            if (notification == null)
                return false;

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                notification.ReadDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<int> MarkAllAsReadAsync(int userId)
        {
            var unread = await _context.Notifications
                .Where(n => n.RecipientId == userId && !n.IsRead)
                .ToListAsync();

            if (unread.Count == 0)
                return 0;

            var now = DateTime.Now;
            foreach (var n in unread)
            {
                n.IsRead = true;
                n.ReadDate = now;
            }

            await _context.SaveChangesAsync();
            return unread.Count;
        }

        public async Task<bool> DeleteAsync(int notificationId, int userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.RecipientId == userId);

            if (notification == null)
                return false;

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<BroadcastSummaryDto>> GetSentBroadcastsAsync(int take = 50)
        {
            var groups = await _context.Notifications
                .Where(n => n.BroadcastGroupId != null && n.Type == NotificationType.AdminBroadcast)
                .GroupBy(n => n.BroadcastGroupId)
                .Select(g => new BroadcastSummaryDto
                {
                    BroadcastGroupId = g.Key!.Value,
                    Title = g.OrderBy(x => x.Id).Select(x => x.Title).First(),
                    Message = g.OrderBy(x => x.Id).Select(x => x.Message).First(),
                    Link = g.OrderBy(x => x.Id).Select(x => x.Link).First(),
                    CreatedBy = g.OrderBy(x => x.Id).Select(x => x.CreatedBy).First(),
                    CreatedDate = g.Min(x => x.CreatedDate),
                    RecipientCount = g.Count(),
                    ReadCount = g.Count(x => x.IsRead)
                })
                .OrderByDescending(g => g.CreatedDate)
                .Take(take)
                .ToListAsync();

            return groups;
        }
    }
}
