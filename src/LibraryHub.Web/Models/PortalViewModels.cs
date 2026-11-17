using LibraryHub.Domain;

namespace LibraryHub.Web.Models;

public sealed record CatalogPage(
    string? Query,
    IReadOnlyList<Book> Books,
    IReadOnlyList<Book> NewBooks,
    IReadOnlyList<CatalogCategoryRow> Categories,
    int BookCount,
    int ReaderCount,
    int BorrowPeriodDays,
    IReadOnlyDictionary<int, decimal> Ratings);
public sealed record CatalogCategoryRow(int Id, string Name, int BookCount, string? ImagePath = null);

public sealed record ReaderSpaceViewModel(
    Reader Reader,
    IReadOnlyList<BorrowSlip> Borrows,
    IReadOnlyList<ReaderReservationRow> Reservations,
    IReadOnlyList<Fine> Fines,
    IReadOnlyList<ReaderHistoryRow> ReadingHistory,
    IReadOnlyList<Notification> Notifications,
    IReadOnlyList<Book> Favorites,
    int UnreadNotifications,
    string CurrentSection,
    IReadOnlyList<int>? FeaturedBookIds = null,
    IReadOnlyList<Book>? AvailableBooksForShowcase = null);

public sealed record ReaderReservationRow(Reservation Reservation, Book Book);
public sealed record ReaderHistoryRow(ReadingHistory History, Book Book);

public sealed record BookPageViewModel(
    Book Book,
    decimal AverageRating,
    int RatingCount,
    IReadOnlyList<BookCommentRow> Comments,
    bool IsFavorite = false,
    int? CurrentRating = null,
    ReadingHistory? ReadingProgress = null);
public sealed record BookCommentRow(BookComment Comment, Reader Reader, int LikeCount);

public sealed record AdminDashboard(
    int Books,
    int Readers,
    int Borrowing,
    decimal OpenFines,
    int PendingApproval,
    int PendingExtension,
    int OpenTickets,
    int ActiveReservations,
    IReadOnlyList<BorrowSlip> Recent,
    IReadOnlyList<BookPopularityRow> TopBooks,
    IReadOnlyList<ReaderActivityRow> TopReaders);
public sealed record BookPopularityRow(int BookId, string Title, int Borrowed);
public sealed record ReaderActivityRow(int ReaderId, string FullName, int BorrowCount);

public sealed record AdminBooksViewModel(
    IReadOnlyList<Book> Books,
    IReadOnlyList<Category> Categories,
    IReadOnlyList<Author> Authors,
    IReadOnlyList<Publisher> Publishers,
    IReadOnlyList<Shelf> Shelves,
    string? Query,
    string? LifecycleState,
    int Page,
    int PageSize,
    int Total,
    IReadOnlySet<int> PermanentlyProtectedBookIds,
    bool CanEmergencyRecall,
    bool CanPermanentlyDelete)
{
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(Total / (double)PageSize));
}

public sealed record AdminCatalogViewModel(
    IReadOnlyList<Category> Categories,
    IReadOnlyList<Author> Authors,
    IReadOnlyList<Publisher> Publishers,
    IReadOnlyList<Shelf> Shelves);

public sealed record AdminCirculationViewModel(
    PagedResult<BorrowSlip> Borrows,
    PagedResult<ReaderReservationRow> Reservations,
    PagedResult<Fine> Fines,
    string? Query,
    BorrowStatus? BorrowStatus,
    ReservationStatus? ReservationStatus,
    FineStatus? FineStatus,
    IReadOnlyDictionary<int, Reader>? ReadersById = null,
    IReadOnlyList<Reader>? WalkInReaders = null,
    IReadOnlyList<Book>? WalkInBooks = null);

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int Total)
{
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(Total / (double)PageSize));
}

public sealed record AdminReadersViewModel(
    PagedResult<Reader> Readers,
    string? Query,
    ReaderType? Type,
    bool? Verified);

public sealed record AdminEmployeeRow(Employee Employee, Position? Position, Role? Role);
public sealed record AdminEmployeesViewModel(
    PagedResult<AdminEmployeeRow> Employees,
    IReadOnlyList<Position> Positions,
    IReadOnlyList<EmployeeStatusOption> StatusOptions,
    IReadOnlyList<Role> Roles,
    string? Query,
    EmploymentStatus? Status);

public sealed record RolePermissionRow(Role Role, IReadOnlySet<int> PermissionIds);
public sealed record EmployeePermissionRow(Employee Employee, IReadOnlyDictionary<int, PermissionEffect> Overrides);
public sealed record AdminRbacViewModel(
    IReadOnlyList<Permission> Permissions,
    IReadOnlyList<RolePermissionRow> Roles,
    IReadOnlyList<EmployeePermissionRow> Employees);

public sealed record NotificationCenterViewModel(
    IReadOnlyList<Notification> Notifications,
    int UnreadCount);

public sealed record SupportTicketRow(SupportTicket Ticket, Reader Reader);
public sealed record SupportCenterViewModel(
    IReadOnlyList<SupportTicketRow> Tickets,
    SupportTicketRow? Selected,
    bool IsStaff,
    string? Query,
    SupportTicketStatus? Status,
    int Page = 1,
    int PageSize = 20,
    int Total = 0)
{
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(Total / (double)PageSize));
}

public sealed record ProfileCommentRow(ProfileComment Comment, Reader Author);
public sealed record ReaderPublicProfileViewModel(
    Reader Reader,
    IReadOnlyList<Book> FeaturedBooks,
    IReadOnlyList<ProfileCommentRow> Comments,
    bool CanPost);

public sealed record ModerationReportRow(
    CommentReport Report,
    Reader Reporter,
    Reader ReportedUser,
    string? TargetContent,
    int? BookId);
public sealed record AdminModerationViewModel(IReadOnlyList<ModerationReportRow> Reports);

public sealed record AdminPrintSlipViewModel(BorrowSlip Slip, IReadOnlyList<Fine> Fines);

public sealed record PagerModel(int Page, int TotalPages, string Query, string PageParameter = "page");
