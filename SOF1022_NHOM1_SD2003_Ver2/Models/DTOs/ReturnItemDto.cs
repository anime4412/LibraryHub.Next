namespace SOF1022_NHOM1_SD2003_Ver2.Models.DTOs
{
    public class ReturnItemDto
    {
        public int BorrowDetailId { get; set; }
        public string Condition { get; set; } = "good"; // good, damaged, lost
        public int? DamagePercent { get; set; }
        public string? DamageReason { get; set; }
        public decimal CalculatedFine { get; set; }
    }

    public class ReturnBookRequestDto
    {
        public int SlipId { get; set; }
        public string OverallCondition { get; set; } = "good"; // good, damaged, lost
        public List<ReturnItemDto> ReturnItems { get; set; } = new();
    }
}
