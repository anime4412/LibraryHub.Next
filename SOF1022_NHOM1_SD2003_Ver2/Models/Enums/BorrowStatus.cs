namespace SOF1022_NHOM1_SD2003_Ver2.Models.Enums
{
    public enum BorrowStatus
    {
        PendingApproval,        // Chờ duyệt - Reader requested
        ApprovedAwaitingPickup, // Đã duyệt, chờ đến lấy - Admin approved, 3-day countdown starts
        Borrowing,              // Đang mượn thực tế - Reader physically picked up the books, deadline removed
        Completed,              // Đã trả
        Overdue,                // Quá hạn
        Cancelled,              // Đã hủy
        Returned,               // Đã trả
        PendingExtension,       // Chờ duyệt gia hạn
        CanceledDueToTimeout,   // Hủy do quá hạn nhận
        LostOrDamaged           // Mất hoặc hư hại - Incident status
    }
}
