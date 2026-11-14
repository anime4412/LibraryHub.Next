namespace SOF1022_NHOM1_SD2003_Ver2.Models.DTOs
{
    using SOF1022_NHOM1_SD2003_Ver2.Models.Enums;

    public class BorrowItemDto
    {
        public int BorrowDetailId { get; set; }
        public int BorrowSlipId { get; set; }
        public string BorrowCode { get; set; } = string.Empty;
        public string BookTitle { get; set; } = string.Empty;
        public string BookCoverImage { get; set; } = string.Empty;
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnedDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public BorrowStatus BorrowStatus { get; set; }
        public bool IsReturned { get; set; }
        public decimal FineAmount { get; set; }
        public bool IsLost { get; set; }
        public bool IsDamaged { get; set; }
        public bool IsLate { get; set; }
        public DateTime? PickupDeadline { get; set; }
        public int ExtensionCount { get; set; }
        public int? FineId { get; set; }
        public string? FineStatus { get; set; }
        public string? TransactionCode { get; set; }
    }
}
