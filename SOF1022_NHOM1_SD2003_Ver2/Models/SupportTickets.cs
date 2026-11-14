using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SOF1022_NHOM1_SD2003_Ver2.Models.Enums;

namespace SOF1022_NHOM1_SD2003_Ver2.Models
{
    public class SupportTickets
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string TicketCode { get; set; } = string.Empty;

        [Required]
        public int ReaderId { get; set; }

        [ForeignKey("ReaderId")]
        public Readers? Reader { get; set; }

        [StringLength(500)]
        public string? Subject { get; set; }

        [Required]
        public SupportTicketStatus Status { get; set; } = SupportTicketStatus.Open;

        // Người xử lý ticket = chính tài khoản Admin/Librarian đang đăng nhập, tức
        // một dòng trong bảng Readers (Session["UserId"] luôn là Readers.Id).
        // KHÔNG dùng bảng Employees — đó là hồ sơ nhân sự, tách biệt hoàn toàn với
        // hệ thống tài khoản đăng nhập, không có liên kết nào tới Readers.
        public int? AssignedStaffId { get; set; }

        [ForeignKey("AssignedStaffId")]
        public Readers? AssignedStaff { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? ClosedDate { get; set; }

        [Required]
        public DateTime LastMessageDate { get; set; } = DateTime.Now;

        [Required]
        public bool UnreadByReader { get; set; } = false;

        [Required]
        public bool UnreadByStaff { get; set; } = false;

        public List<SupportMessages> Messages { get; set; } = new List<SupportMessages>();
    }
}
