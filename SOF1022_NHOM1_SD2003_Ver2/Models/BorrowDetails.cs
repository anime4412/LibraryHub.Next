using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SOF1022_NHOM1_SD2003_Ver2.Models
{
    public class BorrowDetails
    {
        public int Id { get; set; }

        public int BorrowSlipId { get; set; }

        public int BookId { get; set; }

        [Range(1, 100)]
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BorrowPrice { get; set; }

        public bool IsReturned { get; set; } = false;

        public DateTime? ReturnedDate { get; set; }

        public string? ConditionBefore { get; set; }

        public string? ConditionAfter { get; set; }

        public string? Note { get; set; }

        public bool IsLost { get; set; }

        public bool IsDamaged { get; set; }

        public int? DamagePercent { get; set; }

        public int ExtensionCount { get; set; } = 0;

        public DateTime? DueDate { get; set; }

        public int? RequestedExtensionDays { get; set; }

        public string? ExtensionReason { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CompensationAmount { get; set; }

        [ForeignKey(nameof(BorrowSlipId))]
        public BorrowSlips? BorrowSlips { get; set; }

        [ForeignKey(nameof(BookId))]
        public Books? Books { get; set; }
    }
}
