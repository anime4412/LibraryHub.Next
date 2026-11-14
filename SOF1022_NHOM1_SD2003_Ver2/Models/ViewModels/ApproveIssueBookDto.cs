namespace SOF1022_NHOM1_SD2003_Ver2.Models.ViewModels
{
    public class ApproveIssueBookDto
    {
        public int BorrowSlipId { get; set; }
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public int DurationDays { get; set; }
    }
}
