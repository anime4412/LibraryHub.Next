namespace SOF1022_NHOM1_SD2003_Ver2.Models.Enums
{
    public enum NotificationType
    {
        System,                // Thông báo hệ thống chung
        AdminBroadcast,        // Thông báo được gửi từ quản trị viên
        BorrowCreated,         // Tạo phiếu mượn thành công
        BorrowApproved,        // Phiếu mượn được duyệt
        BorrowRejected,        // Phiếu mượn bị từ chối
        BorrowDelivered,       // Đã nhận sách tại thư viện
        BorrowCancelled,       // Phiếu mượn bị hủy
        ReturnCompleted,       // Trả sách thành công
        ExtensionRequested,    // Đã gửi yêu cầu gia hạn
        ExtensionApproved,     // Yêu cầu gia hạn được duyệt
        ExtensionRejected,     // Yêu cầu gia hạn bị từ chối
        ReservationCreated,    // Đặt trước sách thành công
        ReservationReady,      // Sách đặt trước đã sẵn sàng để nhận
        ReservationCancelled,  // Đặt trước bị hủy/từ chối/hết hạn
        FineIssued,            // Phát sinh khoản phạt mới
        FinePaid                // Đã thanh toán khoản phạt
    }
}
