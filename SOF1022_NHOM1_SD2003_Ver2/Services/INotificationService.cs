using SOF1022_NHOM1_SD2003_Ver2.Models.DTOs;
using SOF1022_NHOM1_SD2003_Ver2.Models.Enums;

namespace SOF1022_NHOM1_SD2003_Ver2.Services
{
    public interface INotificationService
    {
        // KHÔNG BAO GIỜ ném exception ra ngoài - lỗi chỉ log lại, không phá luồng nghiệp vụ chính.
        Task NotifyUserAsync(int recipientId, string recipientType, NotificationType type, string title, string message, string? link = null, string? createdBy = null);

        Task<int> NotifyManyAsync(IEnumerable<int> recipientIds, string recipientType, NotificationType type, string title, string message, string? link, string? createdBy, Guid? broadcastGroupId = null);

        Task<int> BroadcastAsync(string? targetRole, NotificationType type, string title, string message, string? link, string? createdBy);

        Task<List<NotificationDto>> GetForUserAsync(int userId, bool unreadOnly = false, int take = 50);

        Task<int> GetUnreadCountAsync(int userId);

        Task<bool> MarkAsReadAsync(int notificationId, int userId);

        Task<int> MarkAllAsReadAsync(int userId);

        Task<bool> DeleteAsync(int notificationId, int userId);

        Task<List<BroadcastSummaryDto>> GetSentBroadcastsAsync(int take = 50);
    }
}
