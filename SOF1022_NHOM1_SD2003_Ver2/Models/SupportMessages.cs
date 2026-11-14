using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SOF1022_NHOM1_SD2003_Ver2.Models.Enums;

namespace SOF1022_NHOM1_SD2003_Ver2.Models
{
    public class SupportMessages
    {
        public int Id { get; set; }

        [Required]
        public int SupportTicketId { get; set; }

        [ForeignKey("SupportTicketId")]
        public SupportTickets? SupportTicket { get; set; }

        [Required]
        public SupportSenderType SenderType { get; set; }

        public int? SenderId { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        public DateTime SentDate { get; set; } = DateTime.Now;
    }
}
