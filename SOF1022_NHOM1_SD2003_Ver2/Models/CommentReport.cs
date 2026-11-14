using System.ComponentModel.DataAnnotations;

namespace SOF1022_NHOM1_SD2003_Ver2.Models
{
    public class CommentReport
    {
        public int Id { get; set; }
        
        [Required]
        public int CommentId { get; set; }
        
        [Required]
        public int ReporterId { get; set; } // Người báo cáo
        
        [Required]
        public int ReportedUserId { get; set; } // Người bị báo cáo
        
        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } // Lý do báo cáo
        
        public DateTime CreatedAt { get; set; }
        
        // Navigation properties
        public BookComment Comment { get; set; }
        public Readers Reporter { get; set; }
        public Readers ReportedUser { get; set; }
    }
}
