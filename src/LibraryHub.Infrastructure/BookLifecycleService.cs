using LibraryHub.Application;
using LibraryHub.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LibraryHub.Infrastructure;

public sealed class BookLifecycleService(
    LibraryDbContext db,
    INotificationPublisher notifications,
    IClock clock,
    ILogger<BookLifecycleService> logger) : IBookLifecycleService
{
    private static readonly BorrowStatus[] PendingBorrowStatuses =
        [BorrowStatus.PendingApproval, BorrowStatus.ApprovedAwaitingPickup];
    private static readonly BorrowStatus[] ActiveBorrowStatuses =
        [BorrowStatus.Borrowing, BorrowStatus.Overdue, BorrowStatus.PendingExtension];
    private static readonly ReservationStatus[] ActiveReservationStatuses =
        [ReservationStatus.Waiting, ReservationStatus.Approved, ReservationStatus.ReadyToBorrow];

    public async Task<BookLifecycleImpact> GetImpactAsync(int bookId, CancellationToken cancellationToken)
    {
        if (!await db.Books.AsNoTracking().AnyAsync(x => x.Id == bookId && !x.IsDeleted, cancellationToken))
            throw Rule("book.not_found", "Không tìm thấy sách.");

        var activeBorrowQuery = db.BorrowSlips.AsNoTracking()
            .Where(x => ActiveBorrowStatuses.Contains(x.Status)
                && x.Details.Any(y => y.BookId == bookId && y.ReturnedQuantity < y.Quantity));
        var pendingBorrowQuery = db.BorrowSlips.AsNoTracking()
            .Where(x => PendingBorrowStatuses.Contains(x.Status)
                && x.Details.Any(y => y.BookId == bookId));
        var reservationQuery = db.Reservations.AsNoTracking()
            .Where(x => x.BookId == bookId && ActiveReservationStatuses.Contains(x.Status));

        return new BookLifecycleImpact(
            await activeBorrowQuery.Select(x => x.ReaderId).Distinct().CountAsync(cancellationToken),
            await activeBorrowQuery.SelectMany(x => x.Details)
                .Where(x => x.BookId == bookId)
                .SumAsync(x => (int?)(x.Quantity - x.ReturnedQuantity), cancellationToken) ?? 0,
            await pendingBorrowQuery.CountAsync(cancellationToken),
            await pendingBorrowQuery.SelectMany(x => x.Details)
                .Where(x => x.BookId == bookId)
                .SumAsync(x => (int?)x.Quantity, cancellationToken) ?? 0,
            await reservationQuery.CountAsync(cancellationToken),
            await reservationQuery.CountAsync(cancellationToken));
    }

    public Task<BookLifecycleResult> SuspendAsync(
        SuspendBook command,
        CancellationToken cancellationToken) =>
        db.TransactionAsync(async ct =>
        {
            ValidateOperationKey(command.OperationKey);
            var reason = RequiredReason(command.Reason, "Lý do ngừng phục vụ là bắt buộc.");
            var book = await LockBookAsync(command.BookId, ct)
                ?? throw Rule("book.not_found", "Không tìm thấy sách.");
            if (book.LastLifecycleOperationKey == command.OperationKey)
                return new BookLifecycleResult(book.Id, book.Status, book.SuspensionType, 0, 0, true);
            if (book.IsDeleted)
                throw Rule("book.not_found", "Không tìm thấy sách.");
            if (book.Status == BookStatus.Discontinued)
                throw Rule("book.lifecycle.status", "Sách đã ở trạng thái ngừng phục vụ.");
            if (command.Type == BookSuspensionType.Normal && command.RecallDeadline.HasValue)
                throw Rule("book.recall.invalid", "Chỉ thu hồi khẩn cấp mới được đặt hạn trả sớm.");
            if (command.Type == BookSuspensionType.EmergencyRecall
                && command.RecallDeadline.HasValue
                && command.RecallDeadline.Value < clock.UtcNow)
                throw Rule("book.recall.deadline", "Hạn thu hồi không được trước thời điểm hiện tại.");

            var activeSlips = await db.BorrowSlips
                .Include(x => x.Reader)
                .Include(x => x.Details)
                .Where(x => ActiveBorrowStatuses.Contains(x.Status)
                    && x.Details.Any(y => y.BookId == book.Id && y.ReturnedQuantity < y.Quantity))
                .ToListAsync(ct);
            if (command.Type == BookSuspensionType.Normal && activeSlips.Count > 0)
                throw Rule(
                    "book.suspension.active_borrow",
                    "Không thể ngừng phục vụ theo chế độ thông thường vì hiện còn sách đang được độc giả mượn.");

            var pendingSlips = await db.BorrowSlips
                .Include(x => x.Reader)
                .Include(x => x.Details)
                .Where(x => PendingBorrowStatuses.Contains(x.Status)
                    && x.Details.Any(y => y.BookId == book.Id))
                .ToListAsync(ct);
            var reservations = await db.Reservations
                .Where(x => x.BookId == book.Id && ActiveReservationStatuses.Contains(x.Status))
                .ToListAsync(ct);
            var reservationReaders = await db.Readers
                .Where(x => reservations.Select(y => y.ReaderId).Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, ct);

            book.Status = BookStatus.Discontinued;
            book.SuspensionType = command.Type;
            book.SuspensionReason = reason;
            book.SuspendedAt = clock.UtcNow;
            book.SuspendedByAccountId = command.ActorAccountId;
            book.RecallRequestedAt = command.Type == BookSuspensionType.EmergencyRecall
                && command.RecallDeadline.HasValue ? clock.UtcNow : null;
            book.RecallDeadline = command.Type == BookSuspensionType.EmergencyRecall
                ? command.RecallDeadline : null;
            book.LastLifecycleOperationKey = command.OperationKey;

            foreach (var slip in pendingSlips)
            {
                slip.Status = BorrowStatus.Cancelled;
                slip.PickupExpiresAt = null;
                slip.Note = AppendNote(slip.Note, $"Hủy vì sách '{book.Title}' đã ngừng phục vụ: {reason}");
            }
            foreach (var reservation in reservations)
            {
                reservation.Status = ReservationStatus.Cancelled;
                reservation.ExpiresAt = null;
                reservation.Note = AppendNote(reservation.Note, $"Hủy vì sách '{book.Title}' đã ngừng phục vụ: {reason}");
            }
            await db.SaveChangesAsync(ct);

            const string cancelledMessageTemplate =
                "Yêu cầu đối với sách ‘{0}’ đã được hủy vì thư viện tạm ngừng phục vụ đầu sách này. Chúng tôi xin lỗi vì sự bất tiện.";
            foreach (var slip in pendingSlips)
                await notifications.PublishAsync(
                    slip.Reader!.UserAccountId,
                    NotificationType.BorrowCancelled,
                    "Yêu cầu mượn đã được hủy",
                    string.Format(cancelledMessageTemplate, book.Title),
                    $"/me/borrows/{slip.Id}",
                    Correlation(command.OperationKey, "borrow", slip.Id),
                    ct);
            foreach (var reservation in reservations)
                if (reservationReaders.TryGetValue(reservation.ReaderId, out var reader))
                    await notifications.PublishAsync(
                        reader.UserAccountId,
                        NotificationType.ReservationCancelled,
                        "Đặt trước đã được hủy",
                        string.Format(cancelledMessageTemplate, book.Title),
                        $"/me/reservations/{reservation.Id}",
                        Correlation(command.OperationKey, "reservation", reservation.Id),
                        ct);

            if (command.Type == BookSuspensionType.EmergencyRecall)
            {
                foreach (var slip in activeSlips)
                {
                    var recallInstruction = command.RecallDeadline.HasValue
                        ? $" Thư viện đề nghị bạn trả trước {command.RecallDeadline:dd/MM/yyyy HH:mm}."
                        : " Bạn có thể giữ sách đến ngày phải trả hiện tại.";
                    await notifications.PublishAsync(
                        slip.Reader!.UserAccountId,
                        NotificationType.BookRecallRequested,
                        "Thông báo thu hồi sách",
                        $"Thư viện đã ngừng phục vụ sách ‘{book.Title}’. Phiếu mượn hiện tại của bạn vẫn được ghi nhận. Vui lòng trả sách theo hướng dẫn của thư viện. Bạn sẽ không bị tính phạt do việc thu hồi này.{recallInstruction}",
                        $"/me/borrows/{slip.Id}",
                        Correlation(command.OperationKey, "recall", slip.Id),
                        ct);
                }
            }

            logger.LogInformation(
                "Book {BookId} suspended with type {SuspensionType}; pending borrows {BorrowCount}; reservations {ReservationCount}; active slips {ActiveCount}.",
                book.Id,
                command.Type,
                pendingSlips.Count,
                reservations.Count,
                activeSlips.Count);
            return new BookLifecycleResult(
                book.Id,
                book.Status,
                book.SuspensionType,
                pendingSlips.Count,
                reservations.Count,
                false);
        }, cancellationToken);

    public Task<BookLifecycleResult> RestoreAsync(
        RestoreBook command,
        CancellationToken cancellationToken) =>
        db.TransactionAsync(async ct =>
        {
            ValidateOperationKey(command.OperationKey);
            var reason = RequiredReason(command.Reason, "Lý do khôi phục là bắt buộc.");
            var book = await LockBookAsync(command.BookId, ct)
                ?? throw Rule("book.not_found", "Không tìm thấy sách.");
            if (book.LastLifecycleOperationKey == command.OperationKey)
                return new BookLifecycleResult(book.Id, book.Status, book.SuspensionType, 0, 0, true);
            if (book.IsDeleted || book.Status != BookStatus.Discontinued)
                throw Rule("book.lifecycle.status", "Chỉ có thể khôi phục sách đang ngừng phục vụ.");

            book.Status = BookStatus.Maintenance;
            book.RestoredAt = clock.UtcNow;
            book.RestoredByAccountId = command.ActorAccountId;
            book.RestoreReason = reason;
            book.RecallRequestedAt = null;
            book.RecallDeadline = null;
            book.LastLifecycleOperationKey = command.OperationKey;
            await db.SaveChangesAsync(ct);
            await notifications.PublishAsync(
                command.ActorAccountId,
                NotificationType.BookServiceRestored,
                "Sách đã chuyển sang chờ kiểm tra",
                $"Sách ‘{book.Title}’ đã được khôi phục và chuyển sang trạng thái đang bảo trì / chờ kiểm tra.",
                "/admin/books",
                Correlation(command.OperationKey, "restore", book.Id),
                ct);
            return new BookLifecycleResult(book.Id, book.Status, book.SuspensionType, 0, 0, false);
        }, cancellationToken);

    public Task<BookLifecycleResult> CompleteMaintenanceAsync(
        CompleteBookMaintenance command,
        CancellationToken cancellationToken) =>
        db.TransactionAsync(async ct =>
        {
            ValidateOperationKey(command.OperationKey);
            if (command.ActualTotalQuantity < 0
                || command.AvailableQuantityAfterInspection < 0
                || command.AvailableQuantityAfterInspection > command.ActualTotalQuantity)
                throw Rule(
                    "book.maintenance.quantity",
                    "Số lượng sau kiểm tra phải không âm và số bản khả dụng không được vượt tổng số bản thực tế.");

            var book = await LockBookAsync(command.BookId, ct)
                ?? throw Rule("book.not_found", "Không tìm thấy sách.");
            if (book.LastLifecycleOperationKey == command.OperationKey)
                return new BookLifecycleResult(book.Id, book.Status, book.SuspensionType, 0, 0, true);
            if (book.IsDeleted || book.Status != BookStatus.Maintenance)
                throw Rule("book.lifecycle.status", "Chỉ có thể hoàn tất kiểm tra cho sách đang bảo trì.");

            var activeQuantity = await db.BorrowSlips
                .Where(x => ActiveBorrowStatuses.Contains(x.Status))
                .SelectMany(x => x.Details)
                .Where(x => x.BookId == book.Id && x.ReturnedQuantity < x.Quantity)
                .SumAsync(x => (int?)(x.Quantity - x.ReturnedQuantity), ct) ?? 0;
            if (activeQuantity > 0)
                throw Rule(
                    "book.maintenance.active_borrow",
                    "Chưa thể hoàn tất bảo trì vì vẫn còn bản sách chưa được trả.");

            book.TotalQuantity = command.ActualTotalQuantity;
            book.AvailableQuantity = command.AvailableQuantityAfterInspection;
            book.Status = command.AvailableQuantityAfterInspection > 0
                ? BookStatus.Available
                : BookStatus.OutOfStock;
            book.LastLifecycleOperationKey = command.OperationKey;
            await db.SaveChangesAsync(ct);
            await notifications.PublishAsync(
                command.ActorAccountId,
                NotificationType.BookMaintenanceCompleted,
                "Đã hoàn tất bảo trì sách",
                $"Sách ‘{book.Title}’ đã được kiểm tra: {book.AvailableQuantity}/{book.TotalQuantity} bản khả dụng/tổng số.",
                "/admin/books",
                Correlation(command.OperationKey, "maintenance", book.Id),
                ct);
            return new BookLifecycleResult(book.Id, book.Status, book.SuspensionType, 0, 0, false);
        }, cancellationToken);

    public Task PermanentlyDeleteAsync(int bookId, CancellationToken cancellationToken) =>
        db.TransactionAsync(async ct =>
        {
            var book = await LockBookAsync(bookId, ct)
                ?? throw Rule("book.not_found", "Không tìm thấy sách.");
            if (book.IsDeleted || book.Status != BookStatus.Discontinued)
                throw Rule("book.lifecycle.status", "Chỉ có thể xóa vĩnh viễn sách đang ngừng phục vụ.");
            if (await HasBusinessHistoryAsync(book.Id, ct))
                throw Rule(
                    "book.permanent_delete.history",
                    "Không thể xóa vĩnh viễn vì sách đã phát sinh dữ liệu nghiệp vụ. Sách sẽ tiếp tục được lưu ở trạng thái ngừng phục vụ để bảo toàn lịch sử.");

            db.Books.Remove(book);
            await db.SaveChangesAsync(ct);
            logger.LogWarning("Unused book {BookId} was permanently deleted.", bookId);
            return true;
        }, cancellationToken);

    private async Task<Book?> LockBookAsync(int id, CancellationToken ct) =>
        db.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory"
            ? await db.Books.SingleOrDefaultAsync(x => x.Id == id, ct)
            : await db.Books.FromSqlInterpolated(
                    $"SELECT * FROM [Books] WITH (UPDLOCK, ROWLOCK) WHERE [Id] = {id}")
                .SingleOrDefaultAsync(ct);

    private Task<bool> HasBusinessHistoryAsync(int bookId, CancellationToken ct) =>
        db.Books.AsNoTracking()
            .Where(b => b.Id == bookId)
            .Select(b =>
                db.BorrowDetails.Any(x => x.BookId == bookId)
                || db.Reservations.Any(x => x.BookId == bookId)
                || db.FineDetails.Any(x => x.BookId == bookId)
                || db.BookRatings.Any(x => x.BookId == bookId)
                || db.BookComments.Any(x => x.BookId == bookId)
                || db.BookFavorites.Any(x => x.BookId == bookId)
                || db.BookViewHistories.Any(x => x.BookId == bookId)
                || db.ReadingHistories.Any(x => x.BookId == bookId)
                || db.ReaderFeaturedBooks.Any(x => x.BookId == bookId))
            .SingleOrDefaultAsync(ct);

    private static string RequiredReason(string? reason, string message)
    {
        var normalized = reason?.Trim();
        if (string.IsNullOrWhiteSpace(normalized)) throw Rule("book.lifecycle.reason", message);
        if (normalized.Length > 500)
            throw Rule("book.lifecycle.reason", "Lý do không được vượt quá 500 ký tự.");
        return normalized;
    }

    private static void ValidateOperationKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key) || key.Length > 100)
            throw Rule("idempotency.required", "Idempotency-Key là bắt buộc và không được vượt quá 100 ký tự.");
    }

    private static string AppendNote(string? current, string note) =>
        string.Join(Environment.NewLine, new[] { current, note }.Where(x => !string.IsNullOrWhiteSpace(x)));

    private static string Correlation(string operationKey, string kind, int id) =>
        $"book-lifecycle:{operationKey}:{kind}:{id}";

    private static DomainRuleException Rule(string code, string message) => new(code, message);
}
