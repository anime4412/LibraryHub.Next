using System.ComponentModel.DataAnnotations;

namespace LibraryHub.Domain;

public abstract class Entity
{
    public int Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public byte[] RowVersion { get; set; } = [];
}

public abstract class SoftDeletableEntity : Entity
{
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}

public sealed class DomainRuleException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}

public enum BookStatus
{
    [Display(Name = "Đang phục vụ")] Available,
    [Display(Name = "Đã lưu trữ")] Hidden,
    [Display(Name = "Thất lạc")] Lost,
    [Display(Name = "Đang bảo trì / chờ kiểm tra")] Maintenance,
    [Display(Name = "Đang phục vụ - hết bản khả dụng")] OutOfStock,
    [Display(Name = "Ngừng phục vụ")] Discontinued
}
public enum BookSuspensionType
{
    [Display(Name = "Thông thường")] Normal,
    [Display(Name = "Thu hồi khẩn cấp")] EmergencyRecall
}
public enum ReaderType
{
    [Display(Name = "Sinh viên")] Student,
    [Display(Name = "Giảng viên")] Teacher,
    [Display(Name = "Khách")] Guest
}
public enum Gender
{
    [Display(Name = "Chưa cung cấp")] Unspecified,
    [Display(Name = "Nam")] Male,
    [Display(Name = "Nữ")] Female,
    [Display(Name = "Khác")] Other
}
public enum EmploymentStatus
{
    [Display(Name = "Đang làm việc")] Working,
    [Display(Name = "Tạm đình chỉ")] Suspended,
    [Display(Name = "Đã nghỉ việc")] Resigned
}
public enum AccountKind
{
    [Display(Name = "Độc giả")] Reader,
    [Display(Name = "Nhân viên nội bộ")] Internal,
    [Display(Name = "Quản trị viên")] Administrator
}
public enum CommentLockMode
{
    [Display(Name = "Không khóa")] None,
    [Display(Name = "Khóa tạm thời")] Temporary,
    [Display(Name = "Khóa vĩnh viễn")] Permanent
}
public enum PermissionEffect
{
    [Display(Name = "Từ chối")] Deny,
    [Display(Name = "Cho phép")] Allow
}
public enum BorrowStatus
{
    [Display(Name = "Chờ duyệt")] PendingApproval,
    [Display(Name = "Đã duyệt, chờ nhận sách")] ApprovedAwaitingPickup,
    [Display(Name = "Đang mượn")] Borrowing,
    [Display(Name = "Hoàn tất")] Completed,
    [Display(Name = "Quá hạn")] Overdue,
    [Display(Name = "Đã hủy")] Cancelled,
    [Display(Name = "Đã trả")] Returned,
    [Display(Name = "Chờ duyệt gia hạn")] PendingExtension,
    [Display(Name = "Hủy do quá hạn nhận sách")] CanceledDueToTimeout,
    [Display(Name = "Mất hoặc hư hỏng")] LostOrDamaged
}
public enum ReservationStatus
{
    [Display(Name = "Đang chờ")] Waiting,
    [Display(Name = "Sẵn sàng nhận sách")] ReadyToBorrow,
    [Display(Name = "Hoàn tất")] Completed,
    [Display(Name = "Đã hủy")] Cancelled,
    [Display(Name = "Hết hạn")] Expired,
    [Display(Name = "Đã duyệt")] Approved,
    [Display(Name = "Bị từ chối")] Rejected
}
public enum FineType
{
    [Display(Name = "Trả trễ")] Late,
    [Display(Name = "Mất sách")] Lost,
    [Display(Name = "Hư hỏng")] Damaged,
    [Display(Name = "Trả trễ và hư hỏng")] LateAndDamaged,
    [Display(Name = "Trả trễ và mất sách")] LateAndLost
}
public enum FineStatus
{
    [Display(Name = "Chưa thanh toán")] Unpaid,
    [Display(Name = "Chờ xác nhận")] PendingApproval,
    [Display(Name = "Đã thanh toán một phần")] PartiallyPaid,
    [Display(Name = "Đã thanh toán")] Paid,
    [Display(Name = "Được miễn")] Waived,
    [Display(Name = "Đã hủy")] Cancelled
}
public enum ItemCondition
{
    [Display(Name = "Nguyên vẹn")] Intact,
    [Display(Name = "Hư hỏng")] Damaged,
    [Display(Name = "Bị mất")] Lost
}
public enum SupportTicketStatus
{
    [Display(Name = "Đang mở")] Open,
    [Display(Name = "Đang xử lý")] InProgress,
    [Display(Name = "Đã đóng")] Closed
}
public enum SupportSenderType
{
    [Display(Name = "Độc giả")] Reader,
    [Display(Name = "Nhân viên")] Employee,
    [Display(Name = "Trợ lý tự động")] Bot,
    [Display(Name = "Hệ thống")] System
}
public enum ReportStatus
{
    [Display(Name = "Đang chờ")] Pending,
    [Display(Name = "Đang xem xét")] Reviewing,
    [Display(Name = "Đã xử lý")] Resolved,
    [Display(Name = "Bị từ chối")] Rejected
}
public enum NotificationType
{
    [Display(Name = "Hệ thống")] System,
    [Display(Name = "Thông báo từ quản trị")] AdminBroadcast,
    [Display(Name = "Chào mừng")] Welcome,
    [Display(Name = "Xác thực email")] EmailVerification,
    [Display(Name = "Đổi mật khẩu")] PasswordChanged,
    [Display(Name = "Tạo phiếu mượn")] BorrowCreated,
    [Display(Name = "Phiếu mượn được duyệt")] BorrowApproved,
    [Display(Name = "Phiếu mượn bị từ chối")] BorrowRejected,
    [Display(Name = "Đã giao sách")] BorrowDelivered,
    [Display(Name = "Phiếu mượn bị hủy")] BorrowCancelled,
    [Display(Name = "Nhắc nhận sách")] BorrowPickupReminder,
    [Display(Name = "Quá hạn nhận sách")] BorrowPickupExpired,
    [Display(Name = "Sắp đến hạn trả")] BorrowDueSoon,
    [Display(Name = "Mượn quá hạn")] BorrowOverdue,
    [Display(Name = "Trả sách hoàn tất")] ReturnCompleted,
    [Display(Name = "Trả sách một phần")] ReturnPartiallyCompleted,
    [Display(Name = "Yêu cầu gia hạn")] ExtensionRequested,
    [Display(Name = "Gia hạn được duyệt")] ExtensionApproved,
    [Display(Name = "Gia hạn bị từ chối")] ExtensionRejected,
    [Display(Name = "Tạo đặt trước")] ReservationCreated,
    [Display(Name = "Đặt trước được duyệt")] ReservationApproved,
    [Display(Name = "Đặt trước bị từ chối")] ReservationRejected,
    [Display(Name = "Sách đặt trước đã sẵn sàng")] ReservationReady,
    [Display(Name = "Đặt trước bị hủy")] ReservationCancelled,
    [Display(Name = "Đặt trước hết hạn")] ReservationExpired,
    [Display(Name = "Phát sinh khoản phạt")] FineIssued,
    [Display(Name = "Cập nhật khoản phạt")] FineUpdated,
    [Display(Name = "Khoản phạt đã thanh toán")] FinePaid,
    [Display(Name = "Khoản phạt được miễn")] FineWaived,
    [Display(Name = "Phản hồi bình luận sách")] BookCommentReply,
    [Display(Name = "Bình luận được yêu thích")] BookCommentLiked,
    [Display(Name = "Bình luận hồ sơ mới")] ProfileCommentAdded,
    [Display(Name = "Tạo yêu cầu hỗ trợ")] SupportTicketCreated,
    [Display(Name = "Yêu cầu hỗ trợ được tiếp nhận")] SupportTicketAssigned,
    [Display(Name = "Tin nhắn hỗ trợ mới")] SupportMessageReceived,
    [Display(Name = "Yêu cầu hỗ trợ đã đóng")] SupportTicketClosed,
    [Display(Name = "Sách ngừng phục vụ")] BookServiceSuspended,
    [Display(Name = "Thu hồi sách khẩn cấp")] BookRecallRequested,
    [Display(Name = "Khôi phục sách")] BookServiceRestored,
    [Display(Name = "Hoàn tất bảo trì sách")] BookMaintenanceCompleted
}

public sealed class Category : SoftDeletableEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? ImagePath { get; set; }
}

public sealed class Author : SoftDeletableEntity
{
    public required string Name { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Nationality { get; set; }
    public string? Biography { get; set; }
}

public sealed class Publisher : SoftDeletableEntity
{
    public required string Name { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
}

public sealed class Shelf : SoftDeletableEntity
{
    public required string Code { get; set; }
    public string? LocationCode { get; set; }
    public required string Name { get; set; }
    public string? Area { get; set; }
    public string? Description { get; set; }
    public int Capacity { get; set; }
}

public sealed class Book : SoftDeletableEntity
{
    public required string Isbn { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public string? Edition { get; set; }
    public string? Keywords { get; set; }
    public int PublicationYear { get; set; }
    public string? CreatedBy { get; set; }
    public int PageCount { get; set; }
    public string? Language { get; set; }
    public DateTimeOffset? LastBorrowedAt { get; set; }
    public decimal Price { get; set; }
    public int TotalQuantity { get; set; }
    public int AvailableQuantity { get; set; }
    public int MinimumAge { get; set; }
    public string? CoverImagePath { get; set; }
    public BookStatus Status { get; set; } = BookStatus.Available;
    public BookSuspensionType? SuspensionType { get; set; }
    public string? SuspensionReason { get; set; }
    public DateTimeOffset? SuspendedAt { get; set; }
    public Guid? SuspendedByAccountId { get; set; }
    public DateTimeOffset? RestoredAt { get; set; }
    public Guid? RestoredByAccountId { get; set; }
    public string? RestoreReason { get; set; }
    public DateTimeOffset? RecallRequestedAt { get; set; }
    public DateTimeOffset? RecallDeadline { get; set; }
    public string? LastLifecycleOperationKey { get; set; }
    public long ViewCount { get; set; }
    public int CategoryId { get; set; }
    public int AuthorId { get; set; }
    public int PublisherId { get; set; }
    public int ShelfId { get; set; }
    public Category? Category { get; set; }
    public Author? Author { get; set; }
    public Publisher? Publisher { get; set; }
    public Shelf? Shelf { get; set; }

    public void Reserve(int quantity)
    {
        if (quantity <= 0) throw new DomainRuleException("book.quantity.invalid", "Số lượng phải lớn hơn 0.");
        if (Status is not BookStatus.Available || AvailableQuantity < quantity)
            throw new DomainRuleException("book.stock.insufficient", $"Sách '{Title}' không đủ số lượng.");
        AvailableQuantity -= quantity;
        if (AvailableQuantity == 0) Status = BookStatus.OutOfStock;
    }

    public void Release(int quantity)
    {
        if (quantity <= 0) throw new DomainRuleException("book.quantity.invalid", "Số lượng phải lớn hơn 0.");
        AvailableQuantity = Math.Min(TotalQuantity, AvailableQuantity + quantity);
        if (Status == BookStatus.OutOfStock && AvailableQuantity > 0) Status = BookStatus.Available;
    }
}

public sealed class Reader : SoftDeletableEntity
{
    public Guid UserAccountId { get; set; }
    public required string ReaderCode { get; set; }
    public required string FullName { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Phone { get; set; }
    public required string Email { get; set; }
    public string? Address { get; set; }
    public string? IdentityNumber { get; set; }
    public DateOnly RegisteredOn { get; set; }
    public DateOnly ExpiresOn { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }
    public int ConsecutiveLoginDays { get; set; } = 1;
    public DateOnly? LastStreakOn { get; set; }
    public string? AvatarPath { get; set; }
    public string? CoverImagePath { get; set; }
    public string? Bio { get; set; }
    public string? ActiveBadge { get; set; }
    public string? FullPageBackgroundPath { get; set; }
    public CommentLockMode CommentLockMode { get; set; }
    public DateTimeOffset? CommentBlockedUntil { get; set; }
    public int MaxBorrowBooks { get; set; } = 10;
    public ReaderType Type { get; set; }
    public Gender Gender { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool ForcePasswordChange { get; set; }
    public bool IsNameSet { get; set; }
    public bool IsVerified { get; set; }
}

public sealed class ReaderFeaturedBook : Entity
{
    public int ReaderId { get; set; }
    public int BookId { get; set; }
    public int DisplayOrder { get; set; }
}

public sealed class Position : SoftDeletableEntity
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public decimal SalaryAmount { get; set; }
    public bool AllowSalaryOverride { get; set; }
}

public sealed class EmployeeStatusOption : SoftDeletableEntity
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public EmploymentStatus Status { get; set; }
}

public sealed class Employee : SoftDeletableEntity
{
    public required string EmployeeCode { get; set; }
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string Phone { get; set; }
    public required string Address { get; set; }
    public required string NationalId { get; set; }
    public Gender Gender { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public DateOnly HiredOn { get; set; }
    public int PositionId { get; set; }
    public Position? Position { get; set; }
    public decimal Salary { get; set; }
    public EmploymentStatus Status { get; set; } = EmploymentStatus.Working;
    public int? StatusOptionId { get; set; }
    public string? LeaveReason { get; set; }
    public DateOnly? LeaveStartedOn { get; set; }
    public int UnexcusedDays { get; set; }
    public DateOnly? LastUnexcusedOn { get; set; }
    public int? RoleId { get; set; }
}

public sealed class StaffAccount : Entity
{
    public Guid UserAccountId { get; set; }
    public int EmployeeId { get; set; }
}

public sealed class Role : SoftDeletableEntity
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public bool IsSystem { get; set; }
}

public sealed class Permission : Entity
{
    public required string Code { get; set; }
    public required string DisplayName { get; set; }
    public required string GroupName { get; set; }
}

public sealed class RolePermission : Entity
{
    public int RoleId { get; set; }
    public int PermissionId { get; set; }
    public PermissionEffect Effect { get; set; } = PermissionEffect.Allow;
}

public sealed class UserPermission : Entity
{
    public int EmployeeId { get; set; }
    public int PermissionId { get; set; }
    public PermissionEffect Effect { get; set; }
}

public sealed class BorrowSlip : Entity
{
    public required string Code { get; set; }
    public int ReaderId { get; set; }
    public Reader? Reader { get; set; }
    public int TotalBooks { get; set; }
    public DateTimeOffset RequestedAt { get; set; }
    public DateTimeOffset? BorrowedAt { get; set; }
    public DateTimeOffset? DueAt { get; set; }
    public DateTimeOffset? ReturnedAt { get; set; }
    public DateTimeOffset? PickupExpiresAt { get; set; }
    public string? Note { get; set; }
    public Guid CreatedByAccountId { get; set; }
    public BorrowStatus Status { get; set; } = BorrowStatus.PendingApproval;
    public required string IdempotencyKey { get; set; }
    public List<BorrowDetail> Details { get; set; } = [];
}

public sealed class BorrowDetail : Entity
{
    public int BorrowSlipId { get; set; }
    public int BookId { get; set; }
    public Book? Book { get; set; }
    public int Quantity { get; set; }
    public int ReturnedQuantity { get; set; }
    public decimal BorrowPrice { get; set; }
    public string? ConditionBefore { get; set; }
    public string? Note { get; set; }
    public int ExtensionCount { get; set; }
    public DateTimeOffset? DueAt { get; set; }
    public int? RequestedExtensionDays { get; set; }
    public string? ExtensionReason { get; set; }
}

public sealed class BorrowReturn : Entity
{
    public int BorrowDetailId { get; set; }
    public int Quantity { get; set; }
    public ItemCondition Condition { get; set; }
    public decimal DamagePercent { get; set; }
    public string? ConditionNote { get; set; }
    public Guid ProcessedByAccountId { get; set; }
    public required string IdempotencyKey { get; set; }
    public int ResultSlipId { get; set; }
    public BorrowStatus ResultStatus { get; set; }
    public int? FineId { get; set; }
    public decimal FineAmount { get; set; }
}

public sealed class Reservation : Entity
{
    public int ReaderId { get; set; }
    public int BookId { get; set; }
    public DateTimeOffset ReservedAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.Waiting;
    public string? Note { get; set; }
}

public sealed class Fine : Entity
{
    public required string Code { get; set; }
    public int BorrowSlipId { get; set; }
    public int ReaderId { get; set; }
    public FineType Type { get; set; }
    public decimal Amount { get; set; }
    public decimal PaidAmount { get; set; }
    public string? Reason { get; set; }
    public FineStatus Status { get; set; } = FineStatus.Unpaid;
    public string? TransactionCode { get; set; }
    public DateTimeOffset? PaidAt { get; set; }
    public List<FineDetail> Details { get; set; } = [];
}

public sealed class FineDetail : Entity
{
    public int FineId { get; set; }
    public int BookId { get; set; }
    public int? BorrowDetailId { get; set; }
    public FineType Type { get; set; }
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
    public decimal? DamagePercent { get; set; }
    public decimal BookPrice { get; set; }
    public int LateDays { get; set; }
}

public sealed class SystemSettings : Entity
{
    public int MaxExtensionDays { get; set; } = 7;
    public int MaxExtensionCount { get; set; } = 1;
    public decimal LateFeePerDay { get; set; } = 5_000m;
    public int MaxBorrowBooks { get; set; } = 10;
    public int BorrowPeriodDays { get; set; } = 14;
    public int PickupWindowDays { get; set; } = 3;
}

public sealed class BookFavorite : Entity { public int BookId { get; set; } public int ReaderId { get; set; } }
public sealed class BookComment : SoftDeletableEntity
{
    public int BookId { get; set; }
    public int ReaderId { get; set; }
    public int? ParentCommentId { get; set; }
    public required string Content { get; set; }
}
public sealed class BookCommentLike : Entity { public int BookCommentId { get; set; } public int ReaderId { get; set; } }
public sealed class BookRating : Entity
{
    public int BookId { get; set; }
    public int ReaderId { get; set; }
    public int Stars { get; set; }
    public string? Review { get; set; }
}
public sealed class BookViewHistory : Entity
{
    public int BookId { get; set; }
    public int? ReaderId { get; set; }
    public string? SessionId { get; set; }
    public DateTimeOffset ViewedAt { get; set; }
}
public sealed class ReadingHistory : Entity
{
    public int BookId { get; set; }
    public int ReaderId { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public DateTimeOffset LastReadAt { get; set; }
    public DateTimeOffset StartedAt { get; set; }
}
public sealed class ProfileComment : SoftDeletableEntity
{
    public int ProfileOwnerId { get; set; }
    public int AuthorId { get; set; }
    public required string Content { get; set; }
}
public sealed class ProfileTagPin : Entity
{
    public int ProfileOwnerId { get; set; }
    public int AuthorId { get; set; }
    public required string TagKeysJson { get; set; }
    public string? PresetMessageKey { get; set; }
}
public sealed class CommentReport : Entity
{
    public int ReporterId { get; set; }
    public int ReportedUserId { get; set; }
    public int? BookCommentId { get; set; }
    public int? ProfileCommentId { get; set; }
    public required string Reason { get; set; }
    public ReportStatus Status { get; set; } = ReportStatus.Pending;
    public Guid? ReviewedByAccountId { get; set; }
    public string? Resolution { get; set; }
}

public sealed class SupportTicket : Entity
{
    public required string Code { get; set; }
    public int ReaderId { get; set; }
    public Guid? AssignedStaffAccountId { get; set; }
    public required string Subject { get; set; }
    public SupportTicketStatus Status { get; set; } = SupportTicketStatus.Open;
    public DateTimeOffset LastMessageAt { get; set; }
    public bool UnreadByReader { get; set; }
    public bool UnreadByStaff { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public List<SupportMessage> Messages { get; set; } = [];
}
public sealed class SupportMessage : Entity
{
    public int SupportTicketId { get; set; }
    public SupportSenderType SenderType { get; set; }
    public Guid? SenderAccountId { get; set; }
    public required string Content { get; set; }
}
public sealed class Notification : Entity
{
    public Guid RecipientAccountId { get; set; }
    public NotificationType Type { get; set; }
    public required string Title { get; set; }
    public required string Message { get; set; }
    public string? Link { get; set; }
    public bool IsRead { get; set; }
    public DateTimeOffset? ReadAt { get; set; }
    public Guid? CreatedByAccountId { get; set; }
    public Guid? BroadcastGroupId { get; set; }
    public string? CorrelationKey { get; set; }
}
