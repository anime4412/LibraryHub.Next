namespace SOF1022_NHOM1_SD2003_Ver2.Models.Enums
{
    public enum SupportTicketStatus
    {
        Open,           // Vừa tạo, chưa ai nhận xử lý (nằm trong hàng chờ chung)
        InProgress,     // Đã có nhân viên nhận xử lý
        Closed          // Đã đóng
    }
}
