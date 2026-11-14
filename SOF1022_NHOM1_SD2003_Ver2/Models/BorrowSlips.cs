using SOF1022_NHOM1_SD2003_Ver2.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.Reflection.PortableExecutable;
using static System.Net.WebRequestMethods;

namespace SOF1022_NHOM1_SD2003_Ver2.Models
{
    public class BorrowSlips
    {
        public int Id { get; set; }

        [Required]
        [StringLength(30)]
        public string? BorrowCode { get; set; }

        public int ReaderId { get; set; }

        public int TotalBooks { get; set; }

        public DateTime BorrowDate { get; set; }

        public DateTime DueDate { get; set; }

        public DateTime? ReturnDate { get; set; }

        public DateTime? PickupDeadline { get; set; }

        public string? Note { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public string? CreatedBy { get; set; }

        public string? Status { get; set; }

        public Readers? Readers { get; set; }

        public BorrowStatus BorrowStatus { get; set; } = BorrowStatus.Borrowing;

        public ICollection<BorrowDetails>? BorrowDetails { get; set; }

        public ICollection<Fines>? Fines { get; set; }
    }
}
