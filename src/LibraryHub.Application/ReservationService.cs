using LibraryHub.Domain;

namespace LibraryHub.Application;

public sealed class ReservationService(
    ILibraryStore store,
    INotificationPublisher notifications,
    IClock clock) : IReservationService
{
    public Task UpdateStatusAsync(
        int id,
        ReservationStatus status,
        CancellationToken cancellationToken) =>
        store.TransactionAsync(async ct =>
        {
            var reservation = await RequireReservationAsync(id, ct);
            if (!ReservationPolicy.CanTransition(reservation.Status, status))
                throw Rule(
                    "reservation.status",
                    $"Không thể chuyển đặt trước từ “{reservation.Status.ToVietnamese()}” sang “{status.ToVietnamese()}”.");

            var reader = await store.FindReaderAsync(reservation.ReaderId, ct)
                ?? throw Rule("reader.not_found", "Không tìm thấy độc giả.");
            var book = await store.FindBookForUpdateAsync(reservation.BookId, ct)
                ?? throw Rule("book.not_found", "Không tìm thấy sách.");

            if (status == ReservationStatus.ReadyToBorrow)
            {
                if (book.IsDeleted || book.Status is not BookStatus.Available || book.AvailableQuantity < 1)
                    throw Rule("book.stock.insufficient", $"Sách '{book.Title}' chưa có bản sẵn sàng để giao.");
                var settings = await store.GetSettingsAsync(ct);
                reservation.ExpiresAt = clock.UtcNow.AddDays(settings.PickupWindowDays);
            }

            reservation.Status = status;
            await store.SaveChangesAsync(ct);
            await NotifyStatusAsync(reader, reservation, book, status, ct);
            return true;
        }, cancellationToken);

    public async Task<BorrowResult> DeliverAsync(
        int id,
        Guid processedBy,
        CancellationToken cancellationToken)
    {
        var key = DeliveryKey(id);
        var replay = await store.FindBorrowByKeyAsync(key, cancellationToken);
        if (replay is not null) return new(replay.Id, replay.Code, replay.Status, true);

        var outcome = await store.TransactionAsync(async ct =>
        {
            replay = await store.FindBorrowByKeyAsync(key, ct);
            if (replay is not null)
                return new DeliveryOutcome(new BorrowResult(replay.Id, replay.Code, replay.Status, true), false);

            var reservation = await RequireReservationAsync(id, ct);
            if (reservation.Status != ReservationStatus.ReadyToBorrow)
                throw Rule(
                    "reservation.status",
                    $"Đặt trước đang ở trạng thái “{reservation.Status.ToVietnamese()}”, chưa thể giao sách.");

            var reader = await store.FindReaderAsync(reservation.ReaderId, ct)
                ?? throw Rule("reader.not_found", "Không tìm thấy độc giả.");
            var book = await store.FindBookForUpdateAsync(reservation.BookId, ct)
                ?? throw Rule("book.not_found", "Không tìm thấy sách.");

            var settings = await store.GetSettingsAsync(ct);
            var effectivePickupExpiry = reservation.ExpiresAt
                ?? reservation.ReservedAt.AddDays(settings.PickupWindowDays);
            if (effectivePickupExpiry <= clock.UtcNow)
            {
                reservation.Status = ReservationStatus.Expired;
                await store.SaveChangesAsync(ct);
                await NotifyAsync(
                    reader,
                    NotificationType.ReservationExpired,
                    "Đặt trước đã hết hạn",
                    $"Thời hạn nhận sách '{book.Title}' đã kết thúc.",
                    $"/me/reservations/{reservation.Id}",
                    reservation.Id.ToString(),
                    ct);
                return new DeliveryOutcome(null, true);
            }

            book.Reserve(1);
            book.LastBorrowedAt = clock.UtcNow;
            var dueAt = clock.UtcNow.AddDays(settings.BorrowPeriodDays);
            var slip = new BorrowSlip
            {
                Code = NewCode("BR"),
                ReaderId = reader.Id,
                Reader = reader,
                TotalBooks = 1,
                RequestedAt = reservation.ReservedAt,
                BorrowedAt = clock.UtcNow,
                DueAt = dueAt,
                Note = $"Giao từ đặt trước #{reservation.Id}",
                CreatedByAccountId = processedBy,
                Status = BorrowStatus.Borrowing,
                IdempotencyKey = key,
                Details =
                [
                    new BorrowDetail
                    {
                        BookId = book.Id,
                        Book = book,
                        Quantity = 1,
                        BorrowPrice = book.Price,
                        DueAt = dueAt
                    }
                ]
            };

            reservation.Status = ReservationStatus.Completed;
            store.Add(slip);
            await store.SaveChangesAsync(ct);
            await NotifyAsync(
                reader,
                NotificationType.BorrowDelivered,
                "Đã giao sách đặt trước",
                $"Bạn đã nhận '{book.Title}'. Hạn trả {dueAt:dd/MM/yyyy HH:mm}.",
                $"/me/borrows/{slip.Id}",
                reservation.Id.ToString(),
                ct);
            return new DeliveryOutcome(new BorrowResult(slip.Id, slip.Code, slip.Status, false), false);
        }, cancellationToken);

        if (outcome.Expired)
            throw Rule("reservation.pickup.expired", "Đặt trước đã quá hạn nhận sách và không còn hiệu lực.");
        return outcome.Result!;
    }

    public async Task<int> CancelExpiredAsync(CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        var settings = await store.GetSettingsAsync(cancellationToken);
        var legacyCutoff = now.AddDays(-settings.PickupWindowDays);
        var candidates = await store.FindExpiredReservationsAsync(
            now,
            legacyCutoff,
            cancellationToken);
        if (candidates.Count == 0) return 0;

        var readers = await store.FindReadersByIdsAsync(
            candidates.Select(x => x.ReaderId).Distinct().ToList(),
            cancellationToken);
        var books = await store.FindBooksByIdsAsync(
            candidates.Select(x => x.BookId).Distinct().ToList(),
            cancellationToken);

        var expiredList = new List<Reservation>();
        foreach (var candidate in candidates)
        {
            var updated = await store.TransactionAsync(async ct =>
            {
                var reservation = await store.FindReservationForUpdateAsync(candidate.Id, ct);
                if (reservation is null || reservation.Status != ReservationStatus.ReadyToBorrow)
                    return null;
                var effectiveExpiry = reservation.ExpiresAt ?? reservation.ReservedAt.AddDays(settings.PickupWindowDays);
                if (effectiveExpiry > now)
                    return null;
                reservation.Status = ReservationStatus.Expired;
                await store.SaveChangesAsync(ct);
                return reservation;
            }, cancellationToken);
            if (updated is not null) expiredList.Add(updated);
        }

        foreach (var reservation in expiredList)
        {
            if (readers.TryGetValue(reservation.ReaderId, out var reader)
                && books.TryGetValue(reservation.BookId, out var book))
            {
                await NotifyAsync(
                    reader,
                    NotificationType.ReservationExpired,
                    "Đặt trước đã hết hạn",
                    $"Bạn không đến nhận '{book.Title}' trong thời hạn nhận sách nên đặt trước đã hết hiệu lực.",
                    $"/me/reservations/{reservation.Id}",
                    reservation.Id.ToString(),
                    cancellationToken);
            }
        }
        return expiredList.Count;
    }

    private async Task<Reservation> RequireReservationAsync(int id, CancellationToken cancellationToken) =>
        await store.FindReservationForUpdateAsync(id, cancellationToken)
        ?? throw Rule("reservation.not_found", "Không tìm thấy đặt trước.");

    private Task NotifyStatusAsync(
        Reader reader,
        Reservation reservation,
        Book book,
        ReservationStatus status,
        CancellationToken cancellationToken)
    {
        var content = status switch
        {
            ReservationStatus.Approved => (
                NotificationType.ReservationApproved,
                "Đặt trước đã được duyệt",
                $"Thư viện đang chuẩn bị sách '{book.Title}'."),
            ReservationStatus.ReadyToBorrow => (
                NotificationType.ReservationReady,
                "Sách đã về thư viện",
                $"'{book.Title}' đã sẵn sàng. Bạn có thể tới lấy trước {reservation.ExpiresAt:dd/MM/yyyy HH:mm}."),
            ReservationStatus.Rejected => (
                NotificationType.ReservationRejected,
                "Đặt trước bị từ chối",
                $"Yêu cầu đặt trước '{book.Title}' không được duyệt."),
            ReservationStatus.Cancelled => (
                NotificationType.ReservationCancelled,
                "Đặt trước đã bị hủy",
                $"Yêu cầu đặt trước '{book.Title}' đã bị hủy."),
            ReservationStatus.Expired => (
                NotificationType.ReservationExpired,
                "Đặt trước đã hết hạn",
                $"Yêu cầu đặt trước '{book.Title}' không còn hiệu lực."),
            _ => throw Rule("reservation.status", "Trạng thái đặt trước không hỗ trợ thông báo.")
        };
        return NotifyAsync(
            reader,
            content.Item1,
            content.Item2,
            content.Item3,
            $"/me/reservations/{reservation.Id}",
            reservation.Id.ToString(),
            cancellationToken);
    }

    private Task NotifyAsync(
        Reader reader,
        NotificationType type,
        string title,
        string message,
        string link,
        string correlationId,
        CancellationToken cancellationToken) =>
        notifications.PublishAsync(
            reader.UserAccountId,
            type,
            title,
            message,
            link,
            correlationId,
            cancellationToken);

    private string NewCode(string prefix) =>
        $"{prefix}-{clock.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..21].ToUpperInvariant();

    private static string DeliveryKey(int reservationId) => $"reservation-delivery-{reservationId}";
    private static DomainRuleException Rule(string code, string message) => new(code, message);
    private sealed record DeliveryOutcome(BorrowResult? Result, bool Expired);
}
