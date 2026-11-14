using System.ComponentModel.DataAnnotations;
using SOF1022_NHOM1_SD2003_Ver2.Models.Enums;

namespace SOF1022_NHOM1_SD2003_Ver2.Models
{
    public class Notification
    {
        public int Id { get; set; }

        public int RecipientId { get; set; }

        // Phân loại người nhận: Reader, Admin, hoặc Librarian
        [Required]
        [StringLength(50)]
        public string RecipientType { get; set; } = "Reader";

        public NotificationType Type { get; set; } = NotificationType.System;

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Link { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime? ReadDate { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [StringLength(150)]
        public string? CreatedBy { get; set; }

        // Chỉ có giá trị với thông báo broadcast từ admin, dùng để gom nhóm
        public Guid? BroadcastGroupId { get; set; }

        public Readers? Recipient { get; set; }
    }
}
