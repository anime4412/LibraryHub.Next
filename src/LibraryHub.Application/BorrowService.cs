using LibraryHub.Domain;

namespace LibraryHub.Application;

public sealed class BorrowService(
    ILibraryStore store,
    IFineCalculator fineCalculator,
    INotificationPublisher notifications,
    IClock clock) : IBorrowService
{
    public async Task<BorrowResult> CreateAsync(CreateBorrow command, CancellationToken cancellationToken)
    {
        RequireKey(command.IdempotencyKey);
        var replay = await store.FindBorrowByKeyAsync(command.IdempotencyKey, cancellationToken);
        if (replay is not null) return new(replay.Id, replay.Code, replay.Status, true);
        if (command.Lines.Count == 0) throw Rule("borrow.empty", "Phiếu mượn phải có sách.");

        return await store.TransactionAsync(async ct =>
        {
            replay = await store.FindBorrowByKeyAsync(command.IdempotencyKey, ct);
            if (replay is not null) return new BorrowResult(replay.Id, replay.Code, replay.Status, true);
            var reader = await store.FindReaderForUpdateAsync(command.ReaderId, ct) ?? throw Rule("reader.not_found", "Không tìm thấy độc giả.");
            EnsureReaderEligible(reader);
            var settings = await store.GetSettingsAsync(ct);
            var lines = command.Lines
                .GroupBy(x => x.BookId)
                .Select(x => new BorrowLine(x.Key, x.Sum(y => y.Quantity)))
                .OrderBy(x => x.BookId)
                .ToList();
            var total = lines.Sum(x => x.Quantity);
            if (lines.Any(x => x.Quantity <= 0)) throw Rule("borrow.quantity.invalid", "Số lượng phải lớn hơn 0.");
            var maxAllowed = Math.Min(reader.MaxBorrowBooks, settings.MaxBorrowBooks);
            var activeBorrowed = await store.CountReaderActiveBorrowBooksAsync(reader.Id, ct);
            if (activeBorrowed + total > maxAllowed)
                throw Rule("borrow.limit", "Vượt giới hạn số sách được mượn.");
            var slip = new BorrowSlip
            {
                Code = NewCode("BR"),
                ReaderId = reader.Id,
                TotalBooks = total,
                RequestedAt = clock.UtcNow,
                Note = command.Note,
                CreatedByAccountId = command.RequestedBy,
                IdempotencyKey = command.IdempotencyKey
            };
            foreach (var line in lines)
            {
                var book = await store.FindBookAsync(line.BookId, ct) ?? throw Rule("book.not_found", "Không tìm thấy sách.");
                if (book.IsDeleted || book.Status is not (BookStatus.Available or BookStatus.OutOfStock))
                    throw Rule("book.unavailable", $"Sách '{book.Title}' không khả dụng.");
                slip.Details.Add(new BorrowDetail { BookId = book.Id, Quantity = line.Quantity, BorrowPrice = book.Price });
            }
            store.Add(slip);
            await store.SaveChangesAsync(ct);
            await Notify(reader, slip, NotificationType.BorrowCreated, "Đã gửi yêu cầu mượn",
                $"Phiếu {slip.Code} đang chờ duyệt.", ct);
            return new BorrowResult(slip.Id, slip.Code, slip.Status, false);
        }, cancellationToken);
    }

    public async Task<BorrowResult> CreateWalkInAsync(CreateWalkInBorrow command, CancellationToken cancellationToken)
    {
        RequireKey(command.IdempotencyKey);
        var replay = await store.FindBorrowByKeyAsync(command.IdempotencyKey, cancellationToken);
        if (replay is not null) return new(replay.Id, replay.Code, replay.Status, true);
        if (command.Lines.Count == 0) throw Rule("borrow.empty", "Phiếu mượn phải có sách.");

        return await store.TransactionAsync(async ct =>
        {
            replay = await store.FindBorrowByKeyAsync(command.IdempotencyKey, ct);
            if (replay is not null) return new BorrowResult(replay.Id, replay.Code, replay.Status, true);
            var reader = await store.FindReaderForUpdateAsync(command.ReaderId, ct) ?? throw Rule("reader.not_found", "Không tìm thấy độc giả.");
            EnsureReaderEligible(reader);
            var settings = await store.GetSettingsAsync(ct);
            var lines = command.Lines
                .GroupBy(x => x.BookId)
                .Select(x => new BorrowLine(x.Key, x.Sum(y => y.Quantity)))
                .OrderBy(x => x.BookId)
                .ToList();
            var total = lines.Sum(x => x.Quantity);
            if (lines.Any(x => x.Quantity <= 0)) throw Rule("borrow.quantity.invalid", "Số lượng phải lớn hơn 0.");
            var maxAllowed = Math.Min(reader.MaxBorrowBooks, settings.MaxBorrowBooks);
            var activeBorrowed = await store.CountReaderActiveBorrowBooksAsync(reader.Id, ct);
            if (activeBorrowed + total > maxAllowed)
                throw Rule("borrow.limit", "Vượt giới hạn số sách được mượn.");

            var now = clock.UtcNow;
            var dueAt = command.DeliverImmediately ? now.AddDays(settings.BorrowPeriodDays) : (DateTimeOffset?)null;
            var slip = new BorrowSlip
            {
                Code = NewCode("BR"),
                ReaderId = reader.Id,
                TotalBooks = total,
                RequestedAt = now,
                BorrowedAt = command.DeliverImmediately ? now : null,
                DueAt = dueAt,
                PickupExpiresAt = command.DeliverImmediately ? null : now.AddDays(settings.PickupWindowDays),
                Status = command.DeliverImmediately ? BorrowStatus.Borrowing : BorrowStatus.ApprovedAwaitingPickup,
                Note = command.Note,
                CreatedByAccountId = command.RequestedBy,
                IdempotencyKey = command.IdempotencyKey
            };

            foreach (var line in lines)
            {
                var book = await store.FindBookForUpdateAsync(line.BookId, ct) ?? throw Rule("book.not_found", "Không tìm thấy sách.");
                if (book.IsDeleted || book.Status is not BookStatus.Available)
                    throw Rule("book.stock.insufficient", $"Sách '{book.Title}' không đủ số lượng khả dụng.");
                var committed = await store.CountCommittedBookQuantityAsync(book.Id, null, ct);
                if (book.AvailableQuantity - committed < line.Quantity)
                    throw Rule("book.stock.insufficient", $"Sách '{book.Title}' đã được giữ chỗ hoặc hết bản khả dụng.");
                if (command.DeliverImmediately)
                {
                    book.Reserve(line.Quantity);
                    book.LastBorrowedAt = now;
                }
                slip.Details.Add(new BorrowDetail
                {
                    BookId = book.Id,
                    Quantity = line.Quantity,
                    BorrowPrice = book.Price,
                    DueAt = dueAt
                });
            }

            store.Add(slip);
            await store.SaveChangesAsync(ct);
            await Notify(reader, slip,
                command.DeliverImmediately ? NotificationType.BorrowDelivered : NotificationType.BorrowApproved,
                command.DeliverImmediately ? "Đã giao sách tại quầy" : "Phiếu đã được duyệt",
                command.DeliverImmediately
                    ? $"Phiếu {slip.Code} đã giao. Hạn trả {slip.DueAt:dd/MM/yyyy}."
                    : $"Đến nhận trước {slip.PickupExpiresAt:dd/MM/yyyy HH:mm}.",
                ct);
            return new BorrowResult(slip.Id, slip.Code, slip.Status, false);
        }, cancellationToken);
    }

    public Task ApproveAsync(int id, CancellationToken cancellationToken) =>
        store.TransactionAsync(async ct =>
        {
            var slip = await RequireSlip(id, ct);
            Ensure(slip, BorrowStatus.PendingApproval);
            var settings = await store.GetSettingsAsync(ct);
            foreach (var detail in slip.Details.OrderBy(x => x.BookId))
            {
                var book = await store.FindBookForUpdateAsync(detail.BookId, ct)
                    ?? throw Rule("book.not_found", "Không tìm thấy sách.");
                var committed = await store.CountCommittedBookQuantityAsync(book.Id, slip.Id, ct);
                if (book.IsDeleted || book.Status is not BookStatus.Available
                    || book.AvailableQuantity - committed < detail.Quantity)
                    throw Rule("book.stock.insufficient", $"Sách '{book.Title}' không đủ số lượng để duyệt phiếu.");
            }
            slip.Status = BorrowStatus.ApprovedAwaitingPickup;
            slip.PickupExpiresAt = clock.UtcNow.AddDays(settings.PickupWindowDays);
            await store.SaveChangesAsync(ct);
            await Notify(slip, NotificationType.BorrowApproved, "Phiếu đã được duyệt",
                $"Đến nhận trước {slip.PickupExpiresAt:dd/MM/yyyy HH:mm}.", ct);
            return true;
        }, cancellationToken);

    public Task RejectAsync(int id, string reason, CancellationToken cancellationToken) =>
        store.TransactionAsync(async ct =>
        {
            var slip = await RequireSlip(id, ct);
            Ensure(slip, BorrowStatus.PendingApproval);
            slip.Status = BorrowStatus.Cancelled;
            slip.Note = string.Join(Environment.NewLine, new[] { slip.Note, $"Từ chối: {reason}" }.Where(x => !string.IsNullOrWhiteSpace(x)));
            await store.SaveChangesAsync(ct);
            await Notify(slip, NotificationType.BorrowRejected, "Phiếu bị từ chối", reason, ct);
            return true;
        }, cancellationToken);

    public Task CancelByReaderAsync(int id, int readerId, CancellationToken cancellationToken) =>
        store.TransactionAsync(async ct =>
        {
            var slip = await RequireSlip(id, ct);
            if (slip.ReaderId != readerId)
                throw Rule("borrow.forbidden", "Bạn không có quyền hủy phiếu mượn này.");
            Ensure(slip, BorrowStatus.PendingApproval);
            slip.Status = BorrowStatus.Cancelled;
            slip.Note = string.Join(Environment.NewLine,
                new[] { slip.Note, "Độc giả tự hủy phiếu trước khi duyệt." }.Where(x => !string.IsNullOrWhiteSpace(x)));
            await store.SaveChangesAsync(ct);
            await Notify(slip, NotificationType.BorrowCancelled, "Phiếu mượn đã hủy",
                $"Phiếu {slip.Code} đã được hủy theo yêu cầu của bạn.", ct);
            return true;
        }, cancellationToken);

    public async Task<int> ProcessOverdueAndDueSoonAsync(CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        var dueSoonCutoff = now.AddDays(2);
        var candidates = await store.FindActiveDueOrOverdueBorrowsAsync(dueSoonCutoff, cancellationToken);
        if (candidates.Count == 0) return 0;

        var markedOverdue = new List<BorrowSlip>();
        var dueSoon = new List<BorrowSlip>();
        foreach (var candidate in candidates)
        {
            var outcome = await store.TransactionAsync<(bool Overdue, bool DueSoon, BorrowSlip? Slip)>(async ct =>
            {
                var slip = await store.FindBorrowForUpdateAsync(candidate.Id, ct);
                if (slip is null || slip.Status is not (BorrowStatus.Borrowing or BorrowStatus.PendingExtension))
                    return (false, false, null);

                var unreturnedDueDates = slip.Details
                    .Where(d => d.ReturnedQuantity < d.Quantity)
                    .Select(d => d.DueAt ?? slip.DueAt)
                    .Where(d => d.HasValue)
                    .Select(d => d!.Value)
                    .ToList();
                if (unreturnedDueDates.Count == 0)
                    return (false, false, null);

                if (unreturnedDueDates.Any(d => d < now))
                {
                    if (slip.Status != BorrowStatus.Overdue)
                    {
                        slip.Status = BorrowStatus.Overdue;
                        await store.SaveChangesAsync(ct);
                        return (true, false, slip);
                    }
                    return (false, false, null);
                }

                if (unreturnedDueDates.Any(d => d <= dueSoonCutoff))
                    return (false, true, slip);

                return (false, false, null);
            }, cancellationToken);

            if (outcome.Overdue && outcome.Slip is not null) markedOverdue.Add(outcome.Slip);
            else if (outcome.DueSoon && outcome.Slip is not null) dueSoon.Add(outcome.Slip);
        }

        foreach (var slip in markedOverdue)
        {
            if (slip.Reader is null) continue;
            await notifications.PublishAsync(
                slip.Reader.UserAccountId,
                NotificationType.BorrowOverdue,
                "Phiếu mượn đã quá hạn",
                $"Phiếu {slip.Code} đã quá hạn trả sách. Vui lòng hoàn trả sớm để tránh phát sinh thêm phí phạt.",
                $"/me/borrows/{slip.Id}",
                $"borrow-overdue:{slip.Id}",
                cancellationToken);
        }
        foreach (var slip in dueSoon)
        {
            if (slip.Reader is null) continue;
            await notifications.PublishAsync(
                slip.Reader.UserAccountId,
                NotificationType.BorrowDueSoon,
                "Sách sắp đến hạn trả",
                $"Phiếu {slip.Code} sắp đến hạn trả trong 48 giờ tới.",
                $"/me/borrows/{slip.Id}",
                $"borrow-duesoon:{slip.Id}",
                cancellationToken);
        }
        return markedOverdue.Count;
    }

    public async Task ConfirmPickupAsync(int id, CancellationToken cancellationToken)
    {
        var expired = await store.TransactionAsync(async ct =>
        {
            var slip = await RequireSlip(id, ct);
            Ensure(slip, BorrowStatus.ApprovedAwaitingPickup);
            var settings = await store.GetSettingsAsync(ct);
            var effectivePickupExpiry = slip.PickupExpiresAt
                ?? slip.RequestedAt.AddDays(settings.PickupWindowDays);
            if (effectivePickupExpiry <= clock.UtcNow)
            {
                slip.Status = BorrowStatus.CanceledDueToTimeout;
                slip.PickupExpiresAt = null;
                await store.SaveChangesAsync(ct);
                await Notify(slip, NotificationType.BorrowPickupExpired, "Phiếu đã hết hạn",
                    "Phiếu bị hủy do quá hạn đến lấy.", ct);
                return true;
            }
            slip.Status = BorrowStatus.Borrowing;
            slip.BorrowedAt = clock.UtcNow;
            slip.DueAt = clock.UtcNow.AddDays(settings.BorrowPeriodDays);
            slip.PickupExpiresAt = null;
            foreach (var detail in slip.Details.OrderBy(x => x.BookId))
            {
                var book = await store.FindBookForUpdateAsync(detail.BookId, ct)
                    ?? throw Rule("book.not_found", "Không tìm thấy sách.");
                book.Reserve(detail.Quantity);
                book.LastBorrowedAt = clock.UtcNow;
                detail.DueAt = slip.DueAt;
            }
            await store.SaveChangesAsync(ct);
            await Notify(slip, NotificationType.BorrowDelivered, "Đã nhận sách", $"Hạn trả {slip.DueAt:dd/MM/yyyy}.", ct);
            return false;
        }, cancellationToken);
        if (expired) throw Rule("borrow.pickup.expired", "Phiếu đã quá hạn nhận sách và không còn hiệu lực.");
    }

    public Task RequestExtensionAsync(int detailId, int days, string reason, CancellationToken cancellationToken) =>
        store.TransactionAsync(async ct =>
        {
            var slipRef = await FindSlipByDetail(detailId, ct);
            var slip = await RequireSlip(slipRef.Id, ct);
            Ensure(slip, BorrowStatus.Borrowing);
            var detail = slip.Details.Single(x => x.Id == detailId);
            if (detail.ReturnedQuantity >= detail.Quantity)
                throw Rule("extension.returned", "Sách này đã được trả, không thể gia hạn.");
            var settings = await store.GetSettingsAsync(ct);
            if (days is <= 0 || days > settings.MaxExtensionDays) throw Rule("extension.days", "Số ngày gia hạn không hợp lệ.");
            if (detail.ExtensionCount >= settings.MaxExtensionCount) throw Rule("extension.count", "Đã đạt số lần gia hạn tối đa.");
            if (detail.DueAt <= clock.UtcNow) throw Rule("extension.overdue", "Không thể gia hạn sách đã quá hạn.");
            detail.RequestedExtensionDays = days;
            detail.ExtensionReason = reason;
            slip.Status = BorrowStatus.PendingExtension;
            await store.SaveChangesAsync(ct);
            await Notify(slip, NotificationType.ExtensionRequested, "Đã gửi yêu cầu gia hạn", reason, ct);
            return true;
        }, cancellationToken);

    public Task DecideExtensionAsync(int detailId, bool approve, CancellationToken cancellationToken) =>
        store.TransactionAsync(async ct =>
        {
            var slipRef = await FindSlipByDetail(detailId, ct);
            var slip = await RequireSlip(slipRef.Id, ct);
            Ensure(slip, BorrowStatus.PendingExtension);
            var detail = slip.Details.Single(x => x.Id == detailId);
            if (approve)
            {
                var settings = await store.GetSettingsAsync(ct);
                if (detail.ExtensionCount >= settings.MaxExtensionCount || detail.RequestedExtensionDays is null)
                    throw Rule("extension.invalid", "Yêu cầu gia hạn không còn hợp lệ.");
                detail.DueAt = detail.DueAt?.AddDays(detail.RequestedExtensionDays.Value);
                detail.ExtensionCount++;
                var latestDue = slip.Details
                    .Where(d => d.ReturnedQuantity < d.Quantity && d.DueAt.HasValue)
                    .Select(d => d.DueAt!.Value)
                    .DefaultIfEmpty()
                    .Max();
                if (latestDue != default) slip.DueAt = latestDue;
            }
            detail.RequestedExtensionDays = null;
            detail.ExtensionReason = null;
            var stillOverdue = !approve && slip.Details.Any(d =>
                d.ReturnedQuantity < d.Quantity && (d.DueAt ?? slip.DueAt) < clock.UtcNow);
            slip.Status = stillOverdue ? BorrowStatus.Overdue : BorrowStatus.Borrowing;
            await store.SaveChangesAsync(ct);
            await Notify(slip, approve ? NotificationType.ExtensionApproved : NotificationType.ExtensionRejected,
                approve ? "Gia hạn được duyệt" : "Gia hạn bị từ chối",
                approve ? $"Hạn mới {detail.DueAt:dd/MM/yyyy}." : "Vui lòng trả theo hạn hiện tại.", ct);
            return true;
        }, cancellationToken);

    public async Task<ReturnResult> ReturnAsync(ReturnBooks command, CancellationToken cancellationToken)
    {
        RequireKey(command.IdempotencyKey);
        var replay = await store.FindReturnByKeyAsync(command.IdempotencyKey, cancellationToken);
        if (replay is not null)
            return new(replay.ResultSlipId, replay.ResultStatus, replay.FineId, replay.FineAmount, true);
        if (command.Lines.Count == 0) throw Rule("return.empty", "Chưa chọn sách trả.");
        if (command.Lines.GroupBy(x => x.BorrowDetailId).Any(g => g.Count() > 1))
            throw Rule("return.duplicate_line", "Không được trùng dòng sách trong một lần trả.");
        return await store.TransactionAsync(async ct =>
        {
            replay = await store.FindReturnByKeyAsync(command.IdempotencyKey, ct);
            if (replay is not null)
                return new ReturnResult(replay.ResultSlipId, replay.ResultStatus, replay.FineId, replay.FineAmount, true);
            var slip = await RequireSlip(command.BorrowSlipId, ct);
            Ensure(slip, BorrowStatus.Borrowing, BorrowStatus.Overdue);
            var settings = await store.GetSettingsAsync(ct);
            var lineDetails = command.Lines
                .Select(line => (
                    Line: line,
                    Detail: slip.Details.SingleOrDefault(x => x.Id == line.BorrowDetailId)
                        ?? throw Rule("borrow.detail.not_found", "Không tìm thấy dòng mượn.")))
                .ToList();
            var lockedBooks = new Dictionary<int, Book>();
            foreach (var bookId in lineDetails.Select(x => x.Detail.BookId).Distinct().OrderBy(x => x))
            {
                lockedBooks[bookId] = await store.FindBookForUpdateAsync(bookId, ct)
                    ?? throw Rule("book.not_found", "Không tìm thấy sách.");
            }
            Fine? fine = null;
            var returnEntries = new List<BorrowReturn>();
            foreach (var (line, detail) in lineDetails)
            {
                var outstanding = detail.Quantity - detail.ReturnedQuantity;
                if (line.Quantity <= 0 || line.Quantity > outstanding) throw Rule("return.quantity", "Số lượng trả không hợp lệ.");
                if (line.DamagePercent is < 0 or > 100 || (line.Condition != ItemCondition.Damaged && line.DamagePercent != 0))
                    throw Rule("return.damage", "Phần trăm hư hỏng không hợp lệ.");
                var book = lockedBooks[detail.BookId];
                var due = detail.DueAt ?? slip.DueAt ?? throw Rule("borrow.due.missing", "Thiếu hạn trả.");
                var quote = fineCalculator.Calculate(new(due, clock.UtcNow, line.Condition, line.DamagePercent,
                    book.Price, line.Quantity, settings.LateFeePerDay));
                if (line.Condition == ItemCondition.Intact
                    && book.Status is BookStatus.Available or BookStatus.OutOfStock)
                    book.Release(line.Quantity);
                if (line.Condition == ItemCondition.Lost)
                {
                    book.TotalQuantity = Math.Max(0, book.TotalQuantity - line.Quantity);
                    if (book.AvailableQuantity > book.TotalQuantity)
                        book.AvailableQuantity = book.TotalQuantity;
                    if (book.AvailableQuantity == 0 && book.Status == BookStatus.Available)
                        book.Status = BookStatus.OutOfStock;
                }
                detail.ReturnedQuantity += line.Quantity;
                var returnEntry = new BorrowReturn
                {
                    BorrowDetailId = detail.Id,
                    Quantity = line.Quantity,
                    Condition = line.Condition,
                    DamagePercent = line.DamagePercent,
                    ConditionNote = line.Note,
                    ProcessedByAccountId = command.ProcessedBy,
                    IdempotencyKey = command.IdempotencyKey
                };
                returnEntries.Add(returnEntry);
                store.Add(returnEntry);
                if (quote.Total <= 0 || !quote.Type.HasValue) continue;
                fine ??= new Fine { Code = NewCode("FN"), BorrowSlipId = slip.Id, ReaderId = slip.ReaderId, Type = quote.Type.Value };
                fine.Details.Add(new FineDetail
                {
                    FineId = fine.Id, BookId = book.Id, BorrowDetailId = detail.Id, Type = quote.Type.Value,
                    Amount = quote.Total, DamagePercent = line.DamagePercent, BookPrice = book.Price,
                    LateDays = quote.LateDays, Reason = FineReason(book.Title, quote)
                });
            }
            if (fine is not null)
            {
                fine.Type = SummarizeFineType(fine.Details);
                fine.Amount = fine.Details.Sum(x => x.Amount);
                fine.Reason = string.Join("; ", fine.Details.Select(x => x.Reason));
                store.Add(fine);
            }
            var complete = slip.Details.All(x => x.ReturnedQuantity == x.Quantity);
            var hasDamageOrLoss = (fine?.Details.Any(y => y.Type != FineType.Late) == true)
                || returnEntries.Any(r => r.Condition != ItemCondition.Intact)
                || await store.HasDamagedOrLostReturnAsync(slip.Id, ct);
            var anyRemainingOverdue = !complete && slip.Details.Any(d =>
                d.ReturnedQuantity < d.Quantity && (d.DueAt ?? slip.DueAt) < clock.UtcNow);
            slip.Status = complete
                ? (hasDamageOrLoss ? BorrowStatus.LostOrDamaged : BorrowStatus.Returned)
                : (anyRemainingOverdue ? BorrowStatus.Overdue : BorrowStatus.Borrowing);
            if (complete) slip.ReturnedAt = clock.UtcNow;
            await store.SaveChangesAsync(ct);
            foreach (var returnEntry in returnEntries)
            {
                returnEntry.ResultSlipId = slip.Id;
                returnEntry.ResultStatus = slip.Status;
                returnEntry.FineId = fine?.Id;
                returnEntry.FineAmount = fine?.Amount ?? 0;
            }
            await store.SaveChangesAsync(ct);
            await Notify(slip, complete ? NotificationType.ReturnCompleted : NotificationType.ReturnPartiallyCompleted,
                complete ? "Đã hoàn tất trả sách" : "Đã trả một phần",
                fine is null ? "Không phát sinh phạt." : $"Phát sinh {fine.Amount:N0}đ tiền phạt.", ct);
            return new ReturnResult(slip.Id, slip.Status, fine?.Id, fine?.Amount ?? 0, false);
        }, cancellationToken);
    }

    public async Task<int> CancelExpiredPickupsAsync(CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        var settings = await store.GetSettingsAsync(cancellationToken);
        var legacyCutoff = now.AddDays(-settings.PickupWindowDays);
        var candidates = await store.FindExpiredPickupsAsync(
            now,
            legacyCutoff,
            cancellationToken);
        var expiredSlips = new List<BorrowSlip>();
        foreach (var candidate in candidates)
        {
            var updated = await store.TransactionAsync(async ct =>
            {
                var slip = await store.FindBorrowForUpdateAsync(candidate.Id, ct);
                if (slip is null || slip.Status != BorrowStatus.ApprovedAwaitingPickup)
                    return null;
                var effectiveExpiry = slip.PickupExpiresAt ?? slip.RequestedAt.AddDays(settings.PickupWindowDays);
                if (effectiveExpiry > now)
                    return null;
                slip.Status = BorrowStatus.CanceledDueToTimeout;
                slip.PickupExpiresAt = null;
                await store.SaveChangesAsync(ct);
                return slip;
            }, cancellationToken);
            if (updated is not null) expiredSlips.Add(updated);
        }
        foreach (var slip in expiredSlips)
        {
            await Notify(slip, NotificationType.BorrowPickupExpired, "Phiếu đã hết hạn",
                "Phiếu bị hủy do quá hạn đến lấy.", cancellationToken);
        }
        return expiredSlips.Count;
    }

    private void EnsureReaderEligible(Reader reader)
    {
        if (reader.IsDeleted)
            throw Rule("reader.not_found", "Không tìm thấy độc giả.");
        var today = DateOnly.FromDateTime(clock.UtcNow.UtcDateTime);
        if (reader.ExpiresOn < today)
            throw Rule("reader.expired", "Thẻ độc giả đã hết hạn, vui lòng gia hạn thẻ trước khi mượn.");
    }

    private async Task<BorrowSlip> RequireSlip(int id, CancellationToken ct) =>
        await store.FindBorrowForUpdateAsync(id, ct) ?? throw Rule("borrow.not_found", "Không tìm thấy phiếu mượn.");
    private async Task<BorrowSlip> FindSlipByDetail(int detailId, CancellationToken ct)
    {
        return await store.FindBorrowByDetailAsync(detailId, ct)
            ?? throw Rule("borrow.detail.not_found", "Không tìm thấy dòng mượn.");
    }
    private static void Ensure(BorrowSlip slip, params BorrowStatus[] allowed)
    {
        if (!allowed.Contains(slip.Status))
            throw Rule("borrow.status", $"Trạng thái “{slip.Status.ToVietnamese()}” không cho phép thao tác.");
    }
    private Task Notify(BorrowSlip slip, NotificationType type, string title, string message, CancellationToken ct) =>
        Notify(slip.Reader ?? throw Rule("reader.not_loaded", "Thiếu dữ liệu độc giả."), slip, type, title, message, ct);
    private Task Notify(Reader reader, BorrowSlip slip, NotificationType type, string title, string message, CancellationToken ct) =>
        notifications.PublishAsync(reader.UserAccountId, type, title, message, $"/me/borrows/{slip.Id}", slip.Id.ToString(), ct);
    private string NewCode(string prefix) => $"{prefix}-{clock.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..21].ToUpperInvariant();
    private static FineType SummarizeFineType(IReadOnlyList<FineDetail> details)
    {
        var hasLate = details.Any(x => x.Type is FineType.Late or FineType.LateAndDamaged or FineType.LateAndLost);
        var hasLost = details.Any(x => x.Type is FineType.Lost or FineType.LateAndLost);
        var hasDamaged = details.Any(x => x.Type is FineType.Damaged or FineType.LateAndDamaged);
        return (hasLate, hasLost, hasDamaged) switch
        {
            (true, true, _) => FineType.LateAndLost,
            (true, false, true) => FineType.LateAndDamaged,
            (false, true, _) => FineType.Lost,
            (false, false, true) => FineType.Damaged,
            _ => FineType.Late
        };
    }

    private static string FineReason(string title, FineQuote quote) =>
        string.Join(", ", new[] { quote.LateDays > 0 ? $"'{title}' trễ {quote.LateDays} ngày" : null,
            quote.Damage > 0 ? $"hư hỏng {quote.Damage:N0}đ" : null, quote.Lost > 0 ? $"mất sách {quote.Lost:N0}đ" : null }
            .Where(x => x is not null));
    private static void RequireKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) throw Rule("idempotency.required", "Thiếu Idempotency-Key.");
    }
    private static DomainRuleException Rule(string code, string message) => new(code, message);
}
