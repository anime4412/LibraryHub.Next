using LibraryHub.Domain;

namespace LibraryHub.Application;

public interface IClock { DateTimeOffset UtcNow { get; } }

public interface INotificationPublisher
{
    Task PublishAsync(Guid accountId, NotificationType type, string title, string message,
        string? link, string? correlationId, CancellationToken cancellationToken);
}

public interface IRealtimeGateway
{
    Task NotifyAccountAsync(Guid accountId, Notification notification, CancellationToken cancellationToken);
}

public interface IEmailSender
{
    Task<EmailDeliveryResult> SendAsync(
        string recipient,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken);
}

public sealed record EmailDeliveryResult(bool IsConfigured, bool Succeeded);

public interface IChatbotService
{
    ChatbotReply Reply(string question);
}

public interface IQuickHelpService
{
    IReadOnlyList<QuickHelpTopic> GetTopics();
}

public sealed record ChatbotReply(string Answer, IReadOnlyList<string> SuggestedActions);
public sealed record QuickHelpTopic(string Key, string Title, string Summary, string ActionUrl);

public interface ILibraryStore
{
    Task<T> TransactionAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken);
    Task<SystemSettings> GetSettingsAsync(CancellationToken cancellationToken);
    Task<Reader?> FindReaderAsync(int id, CancellationToken cancellationToken);
    Task<Reader?> FindReaderForUpdateAsync(int id, CancellationToken cancellationToken);
    Task<int> CountReaderActiveBorrowBooksAsync(int readerId, CancellationToken cancellationToken);
    Task<int> CountCommittedBookQuantityAsync(int bookId, int? excludeSlipId, CancellationToken cancellationToken);
    Task<IReadOnlyDictionary<int, Reader>> FindReadersByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken);
    Task<IReadOnlyDictionary<int, Book>> FindBooksByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken);
    Task<Book?> FindBookAsync(int id, CancellationToken cancellationToken);
    Task<Book?> FindBookForUpdateAsync(int id, CancellationToken cancellationToken);
    Task<BorrowSlip?> FindBorrowAsync(int id, CancellationToken cancellationToken);
    Task<BorrowSlip?> FindBorrowForUpdateAsync(int id, CancellationToken cancellationToken);
    Task<BorrowSlip?> FindBorrowByDetailAsync(int detailId, CancellationToken cancellationToken);
    Task<BorrowSlip?> FindBorrowByKeyAsync(string key, CancellationToken cancellationToken);
    Task<BorrowReturn?> FindReturnByKeyAsync(string key, CancellationToken cancellationToken);
    Task<bool> HasDamagedOrLostReturnAsync(int borrowSlipId, CancellationToken cancellationToken);
    Task<IReadOnlyList<BorrowSlip>> FindExpiredPickupsAsync(
        DateTimeOffset now,
        DateTimeOffset legacyCutoff,
        CancellationToken cancellationToken);
    Task<IReadOnlyList<BorrowSlip>> FindActiveDueOrOverdueBorrowsAsync(
        DateTimeOffset dueSoonCutoff,
        CancellationToken cancellationToken);
    Task<Reservation?> FindReservationAsync(int id, CancellationToken cancellationToken);
    Task<Reservation?> FindReservationForUpdateAsync(int id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Reservation>> FindExpiredReservationsAsync(
        DateTimeOffset now,
        DateTimeOffset legacyCutoff,
        CancellationToken cancellationToken);
    Task<int?> FindEmployeeIdByAccountAsync(Guid accountId, CancellationToken cancellationToken);
    Task<PermissionEffect?> FindUserPermissionAsync(int employeeId, string code, CancellationToken cancellationToken);
    Task<IReadOnlyList<PermissionEffect>> FindRolePermissionsAsync(int employeeId, string code, CancellationToken cancellationToken);
    void Add<TEntity>(TEntity entity) where TEntity : Entity;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

public sealed record BorrowLine(int BookId, int Quantity);
public sealed record CreateBorrow(
    int ReaderId, Guid RequestedBy, IReadOnlyList<BorrowLine> Lines, string? Note, string IdempotencyKey);
public sealed record CreateWalkInBorrow(
    int ReaderId, Guid RequestedBy, IReadOnlyList<BorrowLine> Lines, string? Note, bool DeliverImmediately, string IdempotencyKey);
public sealed record BorrowResult(int Id, string Code, BorrowStatus Status, bool Replayed);
public sealed record ReturnLine(
    int BorrowDetailId, int Quantity, ItemCondition Condition, decimal DamagePercent, string? Note);
public sealed record ReturnBooks(
    int BorrowSlipId, Guid ProcessedBy, IReadOnlyList<ReturnLine> Lines, string IdempotencyKey);
public sealed record ReturnResult(int SlipId, BorrowStatus Status, int? FineId, decimal FineAmount, bool Replayed);
public sealed record FineInput(
    DateTimeOffset DueAt, DateTimeOffset ReturnedAt, ItemCondition Condition,
    decimal DamagePercent, decimal BookPrice, int Quantity, decimal LateFeePerDay);
public sealed record FineQuote(FineType? Type, int LateDays, decimal Late, decimal Damage, decimal Lost)
{
    public decimal Total => Late + Damage + Lost;
}

public interface IFineCalculator { FineQuote Calculate(FineInput input); }
public interface IPermissionService
{
    Task<bool> HasAsync(Guid accountId, string permissionCode, CancellationToken cancellationToken);
}
public interface IPermissionSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}
public interface IBorrowService
{
    Task<BorrowResult> CreateAsync(CreateBorrow command, CancellationToken cancellationToken);
    Task<BorrowResult> CreateWalkInAsync(CreateWalkInBorrow command, CancellationToken cancellationToken);
    Task ApproveAsync(int id, CancellationToken cancellationToken);
    Task RejectAsync(int id, string reason, CancellationToken cancellationToken);
    Task CancelByReaderAsync(int id, int readerId, CancellationToken cancellationToken);
    Task ConfirmPickupAsync(int id, CancellationToken cancellationToken);
    Task RequestExtensionAsync(int detailId, int days, string reason, CancellationToken cancellationToken);
    Task DecideExtensionAsync(int detailId, bool approve, CancellationToken cancellationToken);
    Task<ReturnResult> ReturnAsync(ReturnBooks command, CancellationToken cancellationToken);
    Task<int> CancelExpiredPickupsAsync(CancellationToken cancellationToken);
    Task<int> ProcessOverdueAndDueSoonAsync(CancellationToken cancellationToken);
}

public interface IReservationService
{
    Task UpdateStatusAsync(int id, ReservationStatus status, CancellationToken cancellationToken);
    Task<BorrowResult> DeliverAsync(int id, Guid processedBy, CancellationToken cancellationToken);
    Task<int> CancelExpiredAsync(CancellationToken cancellationToken);
}

public sealed record BookLifecycleImpact(
    int ActiveBorrowerCount,
    int ActiveBorrowedQuantity,
    int PendingBorrowRequestCount,
    int PendingBorrowQuantity,
    int ActiveReservationCount,
    int HeldReservationCount);

public sealed record SuspendBook(
    int BookId,
    Guid ActorAccountId,
    BookSuspensionType Type,
    string Reason,
    DateTimeOffset? RecallDeadline,
    string OperationKey);

public sealed record RestoreBook(
    int BookId,
    Guid ActorAccountId,
    string Reason,
    string OperationKey);

public sealed record CompleteBookMaintenance(
    int BookId,
    Guid ActorAccountId,
    int ActualTotalQuantity,
    int AvailableQuantityAfterInspection,
    string OperationKey);

public sealed record BookLifecycleResult(
    int BookId,
    BookStatus Status,
    BookSuspensionType? SuspensionType,
    int CancelledBorrowRequests,
    int CancelledReservations,
    bool Replayed);

public interface IBookLifecycleService
{
    Task<BookLifecycleImpact> GetImpactAsync(int bookId, CancellationToken cancellationToken);
    Task<BookLifecycleResult> SuspendAsync(SuspendBook command, CancellationToken cancellationToken);
    Task<BookLifecycleResult> RestoreAsync(RestoreBook command, CancellationToken cancellationToken);
    Task<BookLifecycleResult> CompleteMaintenanceAsync(
        CompleteBookMaintenance command,
        CancellationToken cancellationToken);
    Task PermanentlyDeleteAsync(int bookId, CancellationToken cancellationToken);
}
