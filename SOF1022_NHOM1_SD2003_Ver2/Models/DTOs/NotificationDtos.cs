using SOF1022_NHOM1_SD2003_Ver2.Models.Enums;

namespace SOF1022_NHOM1_SD2003_Ver2.Models.DTOs
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Link { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public string RecipientType { get; set; } = "Reader";

        public static NotificationDto FromEntity(Notification n) => new NotificationDto
        {
            Id = n.Id,
            Type = n.Type.ToString(),
            Title = n.Title,
            Message = n.Message,
            Link = n.Link,
            IsRead = n.IsRead,
            CreatedDate = n.CreatedDate,
            CreatedBy = n.CreatedBy,
            RecipientType = n.RecipientType
        };
    }

    public class BroadcastRequestDto
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Link { get; set; }

        // "All" = tất cả, "Reader" = chỉ độc giả, "Admin"/"Librarian" = nhân viên
        public string TargetRole { get; set; } = "Reader";
    }

    public class BroadcastSummaryDto
    {
        public Guid BroadcastGroupId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Link { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int RecipientCount { get; set; }
        public int ReadCount { get; set; }
        public string TargetRole { get; set; } = string.Empty;
    }
}
