namespace SOF1022_NHOM1_SD2003_Ver2.Models.DTOs
{
    public class BorrowError
    {
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Solution { get; set; } = string.Empty;
    }

    public class BorrowResultDto
    {
        public bool Success { get; set; }
        public bool PartialSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? SlipId { get; set; }
        public string? SlipCode { get; set; }
        public List<int> SuccessfulBookIds { get; set; } = new();
        public List<BorrowError> FailedBooks { get; set; } = new();
        public decimal? UnpaidFineAmount { get; set; }
        public int? CurrentBorrowCount { get; set; }
        public int? MaxBorrowBooks { get; set; }
    }
}
