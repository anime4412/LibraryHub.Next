using SOF1022_NHOM1_SD2003_Ver2.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SOF1022_NHOM1_SD2003_Ver2.Models
{
    public class FineDetails
    {
        public int Id { get; set; }

        public int FineId { get; set; }

        public int BookId { get; set; }

        public int? BorrowDetailId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public string? Reason { get; set; }

        public FinesType FinesType { get; set; }

        public int? DamagePercent { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? BookPrice { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation properties
        public Fines? Fine { get; set; }

        public Books? Book { get; set; }

        public BorrowDetails? BorrowDetail { get; set; }
    }
}
