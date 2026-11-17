using LibraryHub.Application;
using LibraryHub.Domain;
using LibraryHub.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace LibraryHub.UnitTests;

public sealed class BookLifecycleTests
{
    [Fact]
    public async Task Unused_book_can_be_suspended_normally_without_changing_physical_quantity()
    {
        await using var db = Db();
        var book = Book(total: 8, available: 8);
        db.Add(book);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await Service(db).SuspendAsync(
            Suspend(book, BookSuspensionType.Normal), TestContext.Current.CancellationToken);

        Assert.Equal(BookStatus.Discontinued, result.Status);
        Assert.Equal(BookSuspensionType.Normal, book.SuspensionType);
        Assert.Equal(8, book.TotalQuantity);
        Assert.Equal(8, book.AvailableQuantity);
        Assert.NotNull(book.SuspendedAt);
        Assert.False(string.IsNullOrWhiteSpace(book.SuspensionReason));
    }

    [Fact]
    public async Task Pending_borrow_is_cancelled_without_stock_or_fine_change()
    {
        await using var db = Db();
        var (book, reader) = await SeedBookAndReader(db, available: 4);
        var slip = Slip(reader, book, BorrowStatus.PendingApproval, 2);
        db.Add(slip);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await Service(db).SuspendAsync(
            Suspend(book, BookSuspensionType.Normal), TestContext.Current.CancellationToken);

        Assert.Equal(1, result.CancelledBorrowRequests);
        Assert.Equal(BorrowStatus.Cancelled, slip.Status);
        Assert.Equal(4, book.AvailableQuantity);
        Assert.Empty(db.Fines);
        Assert.Contains("ngừng phục vụ", slip.Note, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Approved_but_not_delivered_borrow_is_cancelled()
    {
        await using var db = Db();
        var (book, reader) = await SeedBookAndReader(db, available: 3);
        var slip = Slip(reader, book, BorrowStatus.ApprovedAwaitingPickup, 1);
        slip.PickupExpiresAt = FixedClock.Now.AddDays(3);
        db.Add(slip);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        await Service(db).SuspendAsync(
            Suspend(book, BookSuspensionType.Normal), TestContext.Current.CancellationToken);

        Assert.Equal(BorrowStatus.Cancelled, slip.Status);
        Assert.Null(slip.PickupExpiresAt);
        Assert.Equal(3, book.AvailableQuantity);
    }

    [Fact]
    public async Task Normal_suspension_is_blocked_while_a_copy_is_borrowed()
    {
        await using var db = Db();
        var (book, reader) = await SeedBookAndReader(db, available: 1);
        db.Add(Slip(reader, book, BorrowStatus.Borrowing, 1));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var error = await Assert.ThrowsAsync<DomainRuleException>(() => Service(db).SuspendAsync(
            Suspend(book, BookSuspensionType.Normal), TestContext.Current.CancellationToken));

        Assert.Equal("book.suspension.active_borrow", error.Code);
        Assert.Equal(BookStatus.Available, book.Status);
    }

    [Fact]
    public async Task Emergency_recall_succeeds_while_a_copy_is_borrowed()
    {
        await using var db = Db();
        var (book, reader) = await SeedBookAndReader(db, available: 1);
        db.Add(Slip(reader, book, BorrowStatus.Borrowing, 1));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        await Service(db).SuspendAsync(
            Suspend(book, BookSuspensionType.EmergencyRecall, FixedClock.Now.AddDays(1)),
            TestContext.Current.CancellationToken);

        Assert.Equal(BookStatus.Discontinued, book.Status);
        Assert.Equal(BookSuspensionType.EmergencyRecall, book.SuspensionType);
        Assert.Equal(FixedClock.Now.AddDays(1), book.RecallDeadline);
        Assert.Equal(FixedClock.Now, book.RecallRequestedAt);
    }

    [Fact]
    public async Task Emergency_recall_preserves_active_borrow_slip()
    {
        await using var db = Db();
        var (book, reader) = await SeedBookAndReader(db, available: 0);
        var slip = Slip(reader, book, BorrowStatus.Overdue, 1);
        db.Add(slip);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        await Service(db).SuspendAsync(
            Suspend(book, BookSuspensionType.EmergencyRecall), TestContext.Current.CancellationToken);

        Assert.Equal(BorrowStatus.Overdue, slip.Status);
        Assert.Equal(0, slip.Details[0].ReturnedQuantity);
    }

    [Fact]
    public async Task Emergency_recall_notifies_current_borrower_once_on_retry()
    {
        await using var db = Db();
        var (book, reader) = await SeedBookAndReader(db, available: 0);
        db.Add(Slip(reader, book, BorrowStatus.Borrowing, 1));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        var messages = new RecordingNotifications();
        var service = Service(db, messages);
        var command = Suspend(book, BookSuspensionType.EmergencyRecall);

        var first = await service.SuspendAsync(command, TestContext.Current.CancellationToken);
        var replay = await service.SuspendAsync(command, TestContext.Current.CancellationToken);

        Assert.False(first.Replayed);
        Assert.True(replay.Replayed);
        var notification = Assert.Single(messages.Items, x => x.Type == NotificationType.BookRecallRequested);
        Assert.Contains("Phiếu mượn hiện tại của bạn vẫn được ghi nhận", notification.Message);
        Assert.Contains("không bị tính phạt do việc thu hồi", notification.Message);
    }

    [Fact]
    public async Task Returning_recalled_book_does_not_make_it_available_again()
    {
        await using var db = Db();
        var (book, reader) = await SeedBookAndReader(db, available: 0);
        book.Status = BookStatus.Discontinued;
        book.SuspensionType = BookSuspensionType.EmergencyRecall;
        var slip = Slip(reader, book, BorrowStatus.Borrowing, 1);
        slip.DueAt = FixedClock.Now.AddDays(2);
        slip.Details[0].DueAt = slip.DueAt;
        db.AddRange(new SystemSettings(), slip);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        var borrowService = new BorrowService(db, new FineCalculator(), new NullNotifications(), new FixedClock());

        await borrowService.ReturnAsync(new(
            slip.Id,
            Guid.NewGuid(),
            [new ReturnLine(slip.Details[0].Id, 1, ItemCondition.Intact, 0, null)],
            "return-recalled"), TestContext.Current.CancellationToken);

        Assert.Equal(0, book.AvailableQuantity);
        Assert.Equal(BookStatus.Discontinued, book.Status);
        Assert.Equal(BorrowStatus.Returned, slip.Status);
    }

    [Fact]
    public async Task Active_reservation_is_cancelled_and_reader_is_notified()
    {
        await using var db = Db();
        var (book, reader) = await SeedBookAndReader(db, available: 2);
        var reservation = new Reservation
        {
            ReaderId = reader.Id,
            BookId = book.Id,
            Status = ReservationStatus.ReadyToBorrow,
            ReservedAt = FixedClock.Now,
            ExpiresAt = FixedClock.Now.AddDays(3)
        };
        db.Add(reservation);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        var messages = new RecordingNotifications();

        var result = await Service(db, messages).SuspendAsync(
            Suspend(book, BookSuspensionType.Normal), TestContext.Current.CancellationToken);

        Assert.Equal(1, result.CancelledReservations);
        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
        Assert.Null(reservation.ExpiresAt);
        Assert.Equal(2, book.AvailableQuantity);
        Assert.Contains(messages.Items, x => x.Type == NotificationType.ReservationCancelled);
    }

    [Fact]
    public async Task Restore_moves_discontinued_book_to_maintenance()
    {
        await using var db = Db();
        var book = Book();
        book.Status = BookStatus.Discontinued;
        book.SuspensionType = BookSuspensionType.Normal;
        db.Add(book);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        await Service(db).RestoreAsync(new(
            book.Id, Guid.NewGuid(), "Đã có lại quyền cung cấp", "restore-1"),
            TestContext.Current.CancellationToken);

        Assert.Equal(BookStatus.Maintenance, book.Status);
        Assert.Equal("Đã có lại quyền cung cấp", book.RestoreReason);
        Assert.NotNull(book.RestoredAt);
    }

    [Fact]
    public async Task Maintenance_book_cannot_be_added_to_new_borrow_request()
    {
        await using var db = Db();
        var (book, reader) = await SeedBookAndReader(db, available: 2);
        book.Status = BookStatus.Maintenance;
        db.Add(new SystemSettings());
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        var service = new BorrowService(db, new FineCalculator(), new NullNotifications(), new FixedClock());

        var error = await Assert.ThrowsAsync<DomainRuleException>(() => service.CreateAsync(new(
            reader.Id,
            reader.UserAccountId,
            [new BorrowLine(book.Id, 1)],
            null,
            "borrow-maintenance"), TestContext.Current.CancellationToken));

        Assert.Equal("book.unavailable", error.Code);
    }

    [Fact]
    public async Task Completing_maintenance_uses_inspected_quantity_and_returns_to_service()
    {
        await using var db = Db();
        var book = Book(total: 10, available: 7);
        book.Status = BookStatus.Maintenance;
        db.Add(book);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        await Service(db).CompleteMaintenanceAsync(new(
            book.Id, Guid.NewGuid(), 9, 6, "maintenance-1"), TestContext.Current.CancellationToken);

        Assert.Equal(9, book.TotalQuantity);
        Assert.Equal(6, book.AvailableQuantity);
        Assert.Equal(BookStatus.Available, book.Status);
    }

    [Fact]
    public async Task Unused_discontinued_book_can_be_permanently_deleted()
    {
        await using var db = Db();
        var book = Book();
        book.Status = BookStatus.Discontinued;
        db.Add(book);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        await Service(db).PermanentlyDeleteAsync(book.Id, TestContext.Current.CancellationToken);

        Assert.False(await db.Books.AnyAsync(x => x.Id == book.Id, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Borrow_history_blocks_permanent_delete_and_history_is_preserved()
    {
        await using var db = Db();
        var (book, reader) = await SeedBookAndReader(db, available: 1);
        book.Status = BookStatus.Discontinued;
        var slip = Slip(reader, book, BorrowStatus.Returned, 1);
        slip.Details[0].ReturnedQuantity = 1;
        db.Add(slip);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var error = await Assert.ThrowsAsync<DomainRuleException>(() =>
            Service(db).PermanentlyDeleteAsync(book.Id, TestContext.Current.CancellationToken));

        Assert.Equal("book.permanent_delete.history", error.Code);
        Assert.True(await db.Books.AnyAsync(x => x.Id == book.Id, TestContext.Current.CancellationToken));
        Assert.True(await db.BorrowSlips.AnyAsync(x => x.Id == slip.Id, TestContext.Current.CancellationToken));
        Assert.True(await db.BorrowDetails.AnyAsync(x => x.BookId == book.Id, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Review_or_comment_history_also_blocks_permanent_delete()
    {
        await using var db = Db();
        var (book, reader) = await SeedBookAndReader(db);
        book.Status = BookStatus.Discontinued;
        db.AddRange(
            new BookRating { BookId = book.Id, ReaderId = reader.Id, Stars = 5 },
            new BookComment { BookId = book.Id, ReaderId = reader.Id, Content = "Tốt" });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<DomainRuleException>(() =>
            Service(db).PermanentlyDeleteAsync(book.Id, TestContext.Current.CancellationToken));

        Assert.Single(db.BookRatings);
        Assert.Single(db.BookComments);
    }

    [Fact]
    public async Task Administrator_security_role_receives_every_book_lifecycle_permission()
    {
        await using var db = Db();
        var role = new Role { Code = "ADMINISTRATOR", Name = "Quản trị viên", IsSystem = true };
        db.Add(role);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        await new PermissionSeeder(db).SeedAsync(TestContext.Current.CancellationToken);

        var assignedCodes = await (
            from assignment in db.RolePermissions
            join permission in db.Permissions on assignment.PermissionId equals permission.Id
            where assignment.RoleId == role.Id && assignment.Effect == PermissionEffect.Allow
            select permission.Code).ToListAsync(TestContext.Current.CancellationToken);
        Assert.Contains(PermissionCodes.BookSuspend, assignedCodes);
        Assert.Contains(PermissionCodes.BookEmergencyRecall, assignedCodes);
        Assert.Contains(PermissionCodes.BookRestore, assignedCodes);
        Assert.Contains(PermissionCodes.BookCompleteMaintenance, assignedCodes);
        Assert.Contains(PermissionCodes.BookPermanentDelete, assignedCodes);
    }

    [Fact]
    public async Task Recall_deadline_before_current_time_is_rejected()
    {
        await using var db = Db();
        var book = Book();
        db.Add(book);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var error = await Assert.ThrowsAsync<DomainRuleException>(() => Service(db).SuspendAsync(
            Suspend(book, BookSuspensionType.EmergencyRecall, FixedClock.Now.AddMinutes(-1)),
            TestContext.Current.CancellationToken));

        Assert.Equal("book.recall.deadline", error.Code);
        Assert.Equal(BookStatus.Available, book.Status);
    }

    private static LibraryDbContext Db() => new(
        new DbContextOptionsBuilder<LibraryDbContext>()
            .UseInMemoryDatabase($"book-lifecycle-{Guid.NewGuid():N}").Options);

    private static BookLifecycleService Service(
        LibraryDbContext db,
        INotificationPublisher? notifications = null) =>
        new(db, notifications ?? new NullNotifications(), new FixedClock(), NullLogger<BookLifecycleService>.Instance);

    private static SuspendBook Suspend(
        Book book,
        BookSuspensionType type,
        DateTimeOffset? deadline = null) =>
        new(book.Id, Guid.NewGuid(), type, "Lý do kiểm thử hợp lệ", deadline, $"suspend-{Guid.NewGuid():N}");

    private static Book Book(int total = 3, int available = 3) => new()
    {
        Isbn = $"978{Random.Shared.NextInt64(1_000_000_0000, 9_999_999_9999)}",
        Title = "Sách kiểm thử vòng đời",
        Language = "vi",
        TotalQuantity = total,
        AvailableQuantity = available,
        Price = 100_000,
        Status = available > 0 ? BookStatus.Available : BookStatus.OutOfStock
    };

    private static async Task<(Book Book, Reader Reader)> SeedBookAndReader(
        LibraryDbContext db,
        int available = 3)
    {
        var reader = new Reader
        {
            UserAccountId = Guid.NewGuid(),
            ReaderCode = $"DG-{Guid.NewGuid():N}"[..20],
            FullName = "Độc giả kiểm thử",
            Email = $"reader-{Guid.NewGuid():N}@example.test",
            RegisteredOn = new DateOnly(2026, 1, 1),
            ExpiresOn = new DateOnly(2027, 1, 1)
        };
        var book = Book(3, available);
        db.AddRange(book, reader);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        return (book, reader);
    }

    private static BorrowSlip Slip(Reader reader, Book book, BorrowStatus status, int quantity) => new()
    {
        Code = $"BR-{Guid.NewGuid():N}"[..20],
        ReaderId = reader.Id,
        Reader = reader,
        TotalBooks = quantity,
        RequestedAt = FixedClock.Now,
        Status = status,
        CreatedByAccountId = reader.UserAccountId,
        IdempotencyKey = $"borrow-{Guid.NewGuid():N}",
        Details =
        [
            new BorrowDetail
            {
                BookId = book.Id,
                Book = book,
                Quantity = quantity,
                BorrowPrice = book.Price
            }
        ]
    };

    private sealed class FixedClock : IClock
    {
        public static DateTimeOffset Now => new(2026, 8, 1, 5, 0, 0, TimeSpan.Zero);
        public DateTimeOffset UtcNow => Now;
    }

    private sealed class NullNotifications : INotificationPublisher
    {
        public Task PublishAsync(
            Guid accountId,
            NotificationType type,
            string title,
            string message,
            string? link,
            string? correlationId,
            CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class RecordingNotifications : INotificationPublisher
    {
        public List<(NotificationType Type, string Message)> Items { get; } = [];

        public Task PublishAsync(
            Guid accountId,
            NotificationType type,
            string title,
            string message,
            string? link,
            string? correlationId,
            CancellationToken cancellationToken)
        {
            Items.Add((type, message));
            return Task.CompletedTask;
        }
    }
}
