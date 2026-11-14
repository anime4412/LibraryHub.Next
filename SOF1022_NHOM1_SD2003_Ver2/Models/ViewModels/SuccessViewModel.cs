namespace SOF1022_NHOM1_SD2003_Ver2.Models.ViewModels
{
    public class SuccessViewModel
    {
        public string Title { get; set; } = "Thành công!";
        public string Message { get; set; } = "Thao tác của bạn đã được thực hiện thành công.";
        public string? TransactionId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public decimal? Amount { get; set; }
        public string? RedirectUrl { get; set; }
        public string ButtonText { get; set; } = "Tiếp tục";
        public bool ShowHistoryButton { get; set; } = false;
    }
}
