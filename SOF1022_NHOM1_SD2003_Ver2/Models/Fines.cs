using SOF1022_NHOM1_SD2003_Ver2.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SOF1022_NHOM1_SD2003_Ver2.Models
{
    public class Fines
    {
        public int Id { get; set; }

        public int BorrowSlipId { get; set; }

        public int? BorrowDetailId { get; set; }

        public int? BookId { get; set; }

        public int? ReaderId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public string? Reason { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? PaidDate { get; set; }

        public FinesType FinesType { get; set; } = FinesType.Late;

        public bool IsPaid { get; set; } = false;

        public int? DamagePercent { get; set; }

        public int? LateDays { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? LateFine { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? DamageFine { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? LostFine { get; set; }

        public string Status { get; set; } = "Unpaid";

        public string? TransactionCode { get; set; }

        public BorrowSlips? BorrowSlips { get; set; }

        public BorrowDetails? BorrowDetails { get; set; }

        public Books? Books { get; set; }

        public ICollection<FineDetails>? FineDetails { get; set; }
    }
}
