using System.ComponentModel.DataAnnotations.Schema;

namespace SOF1022_NHOM1_SD2003_Ver2.Models
{
    public class SystemSettings : BaseEntity
    {
        public int MaxExtensionDays { get; set; } = 7;  // Default: 7 days
        public int MaxExtensionCount { get; set; } = 1; // Default: 1 extension per book
        [Column(TypeName = "decimal(18,2)")]
        public decimal LateFeePerDay { get; set; } = 5000; // Default: 5,000 VND per day
        public int MaxBorrowBooks { get; set; } = 10; // Default: 10 books per reader
        public int BorrowPeriodDays { get; set; } = 14; // Default: 14 days borrowing period
    }
}
