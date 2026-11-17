namespace LibraryHub.Application;

public sealed record PermissionDefinition(string Code, string DisplayName, string GroupName);

public static class PermissionCodes
{
    public const string BookCreate = "book.create";
    public const string BookEdit = "book.edit";
    public const string BookDelete = "book.delete";
    public const string BookSuspend = "book.suspend";
    public const string BookEmergencyRecall = "book.emergency_recall";
    public const string BookRestore = "book.restore";
    public const string BookCompleteMaintenance = "book.complete_maintenance";
    public const string BookPermanentDelete = "book.permanent_delete";
    public const string BorrowApprove = "borrow.approve";
    public const string BorrowConfirmPickup = "borrow.confirm_pickup";
    public const string BorrowApproveExtension = "borrow.approve_extension";
    public const string BorrowConfirmReturn = "borrow.confirm_return";
    public const string ReservationManage = "reservation.manage";
    public const string SupportClaim = "support.claim";
    public const string EmployeeView = "employee.view";
    public const string EmployeeCreate = "employee.create";
    public const string EmployeeEdit = "employee.edit";
    public const string EmployeeDelete = "employee.delete";
    public const string ReaderView = "reader.view";
    public const string ReaderEdit = "reader.edit";
    public const string ReportView = "report.view";
    public const string CatalogView = "catalog.view";
    public const string CatalogManage = "catalog.manage";

    public static IReadOnlyList<PermissionDefinition> All { get; } =
    [
        new(BookCreate, "Thêm sách", "Sách"),
        new(BookEdit, "Sửa sách", "Sách"),
        new(BookDelete, "Quyền xóa sách cũ (tương thích)", "Sách"),
        new(BookSuspend, "Ngừng phục vụ sách", "Sách"),
        new(BookEmergencyRecall, "Thu hồi sách khẩn cấp", "Sách"),
        new(BookRestore, "Khôi phục sách", "Sách"),
        new(BookCompleteMaintenance, "Hoàn tất bảo trì sách", "Sách"),
        new(BookPermanentDelete, "Xóa vĩnh viễn sách chưa sử dụng", "Sách"),
        new(BorrowApprove, "Duyệt hoặc từ chối phiếu mượn", "Mượn trả"),
        new(BorrowConfirmPickup, "Xác nhận giao sách", "Mượn trả"),
        new(BorrowApproveExtension, "Duyệt gia hạn", "Mượn trả"),
        new(BorrowConfirmReturn, "Xác nhận trả sách", "Mượn trả"),
        new(ReservationManage, "Quản lý đặt trước", "Đặt trước"),
        new(SupportClaim, "Tiếp nhận yêu cầu hỗ trợ", "Hỗ trợ"),
        new(EmployeeView, "Xem nhân viên", "Nhân sự"),
        new(EmployeeCreate, "Thêm nhân viên", "Nhân sự"),
        new(EmployeeEdit, "Sửa nhân viên", "Nhân sự"),
        new(EmployeeDelete, "Xóa nhân viên", "Nhân sự"),
        new(ReaderView, "Xem độc giả", "Độc giả"),
        new(ReaderEdit, "Sửa hồ sơ độc giả", "Độc giả"),
        new(ReportView, "Xem báo cáo", "Báo cáo"),
        new(CatalogView, "Xem danh mục", "Danh mục"),
        new(CatalogManage, "Quản lý danh mục", "Danh mục")
    ];

    public static IReadOnlySet<string> StaffDefaults { get; } = new HashSet<string>(
    [
        BookCreate, BookEdit, BookSuspend, BookRestore, BookCompleteMaintenance,
        BorrowApprove, BorrowConfirmPickup, BorrowApproveExtension, BorrowConfirmReturn,
        ReservationManage, SupportClaim,
        EmployeeView, ReaderView, ReaderEdit, ReportView,
        CatalogView, CatalogManage
    ], StringComparer.OrdinalIgnoreCase);

    public static IReadOnlySet<string> AccountantDefaults { get; } = new HashSet<string>(
        [EmployeeView, ReaderView, ReportView, CatalogView],
        StringComparer.OrdinalIgnoreCase);
}
