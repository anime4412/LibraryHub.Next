using LibraryHub.Application;
using LibraryHub.Domain;
using LibraryHub.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LibraryHub.UnitTests;

public sealed class CoreBusinessTests
{
    [Fact]
    public void Fine_combines_late_and_damage_from_dynamic_rate()
    {
        var calculator = new FineCalculator();
        var quote = calculator.Calculate(new FineInput(
            new DateTimeOffset(2026, 7, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 7, 4, 1, 0, 0, TimeSpan.Zero),
            ItemCondition.Damaged, 25, 100_000, 2, 7_000));
        Assert.Equal(FineType.LateAndDamaged, quote.Type);
        Assert.Equal(4, quote.LateDays);
        Assert.Equal(56_000, quote.Late);
        Assert.Equal(50_000, quote.Damage);
        Assert.Equal(106_000, quote.Total);
    }

    [Fact]
    public void Employee_salary_comes_from_position_data_not_name()
    {
        var position = new Position { Code = "LIB", Name = "Tên có thể đổi", SalaryAmount = 8_500_000 };
        var employee = ValidEmployee();
        employee.Salary = 8_000_000;
        var error = Assert.Throws<DomainRuleException>(() =>
            EmployeePolicy.Validate(employee, position, new DateOnly(2026, 7, 30)));
        Assert.Equal("employee.salary.mismatch", error.Code);
    }

    [Fact]
    public void Reservation_transition_only_moves_forward_or_to_terminal_state()
    {
        Assert.True(ReservationPolicy.CanTransition(ReservationStatus.Waiting, ReservationStatus.Approved));
        Assert.True(ReservationPolicy.CanTransition(ReservationStatus.Approved, ReservationStatus.ReadyToBorrow));
        Assert.False(ReservationPolicy.CanTransition(ReservationStatus.ReadyToBorrow, ReservationStatus.Completed));
        Assert.True(ReservationPolicy.CanTransition(ReservationStatus.Waiting, ReservationStatus.Cancelled));
        Assert.False(ReservationPolicy.CanTransition(ReservationStatus.Approved, ReservationStatus.Waiting));
        Assert.False(ReservationPolicy.CanTransition(ReservationStatus.Completed, ReservationStatus.Waiting));
        Assert.False(ReservationPolicy.CanTransition(ReservationStatus.Waiting, ReservationStatus.Waiting));
    }

    [Fact]
    public async Task User_permission_denial_overrides_role_allow()
    {
        await using var db = Db(nameof(User_permission_denial_overrides_role_allow));
        var role = new Role { Code = "STAFF", Name = "Nhân viên" };
        var permission = new Permission { Code = "book.edit", DisplayName = "Sửa sách", GroupName = "Sách" };
        var employee = ValidEmployee(); employee.RoleId = 1;
        var accountId = Guid.NewGuid();
        db.AddRange(role, permission, employee);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        employee.RoleId = role.Id;
        db.Add(new StaffAccount { UserAccountId = accountId, EmployeeId = employee.Id });
        db.Add(new RolePermission { RoleId = role.Id, PermissionId = permission.Id, Effect = PermissionEffect.Allow });
        db.Add(new UserPermission { EmployeeId = employee.Id, PermissionId = permission.Id, Effect = PermissionEffect.Deny });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        var service = new PermissionService(db);
        Assert.False(await service.HasAsync(accountId, "book.edit", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Borrow_approval_keeps_stock_and_delivery_decrements_it_once()
    {
        await using var db = Db(nameof(Borrow_approval_keeps_stock_and_delivery_decrements_it_once));
        var accountId = Guid.NewGuid();
        var reader = new Reader
        {
            UserAccountId = accountId, ReaderCode = "DG001", FullName = "Nguyễn Văn An",
            Email = "an@example.test", RegisteredOn = new(2026, 1, 1), ExpiresOn = new(2027, 1, 1),
            MaxBorrowBooks = 10
        };
        var book = new Book
        {
            Isbn = "9780000000001", Title = "Kiểm thử", Language = "vi",
            TotalQuantity = 3, AvailableQuantity = 3, Price = 100_000, Status = BookStatus.Available
        };
        db.AddRange(reader, book, new SystemSettings());
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        var service = new BorrowService(db, new FineCalculator(), new NullNotifications(), new FixedClock());
        var command = new CreateBorrow(reader.Id, accountId, [new BorrowLine(book.Id, 2)], null, "borrow-001");
        var first = await service.CreateAsync(command, TestContext.Current.CancellationToken);
        var replay = await service.CreateAsync(command, TestContext.Current.CancellationToken);
        Assert.False(first.Replayed);
        Assert.True(replay.Replayed);
        Assert.Equal(first.Id, replay.Id);
        await service.ApproveAsync(first.Id, TestContext.Current.CancellationToken);
        var stored = await db.BorrowSlips.SingleAsync(x => x.Id == first.Id, TestContext.Current.CancellationToken);
        Assert.Equal(3, book.AvailableQuantity);
        Assert.Equal(BorrowStatus.ApprovedAwaitingPickup, stored.Status);
        await service.ConfirmPickupAsync(first.Id, TestContext.Current.CancellationToken);
        Assert.Equal(1, book.AvailableQuantity);
        Assert.Equal(BorrowStatus.Borrowing, stored.Status);
    }

    [Fact]
    public async Task Reservation_ready_notifies_and_delivery_creates_borrow_then_decrements_stock_once()
    {
        await using var db = Db(nameof(Reservation_ready_notifies_and_delivery_creates_borrow_then_decrements_stock_once));
        var reader = new Reader
        {
            UserAccountId = Guid.NewGuid(), ReaderCode = "DG003", FullName = "Lê An",
            Email = "le.an@example.test", RegisteredOn = new(2026, 1, 1), ExpiresOn = new(2027, 1, 1)
        };
        var book = new Book
        {
            Isbn = "9780000000003", Title = "Sách đặt trước", Language = "vi",
            TotalQuantity = 3, AvailableQuantity = 3, Price = 120_000, Status = BookStatus.Available
        };
        db.AddRange(reader, book, new SystemSettings { PickupWindowDays = 3, BorrowPeriodDays = 14 });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        var reservation = new Reservation
        {
            ReaderId = reader.Id, BookId = book.Id, ReservedAt = FixedClock.Now,
            Status = ReservationStatus.Waiting
        };
        db.Add(reservation);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        var messages = new RecordingNotifications();
        var service = new ReservationService(db, messages, new FixedClock());

        await service.UpdateStatusAsync(reservation.Id, ReservationStatus.Approved, TestContext.Current.CancellationToken);
        await service.UpdateStatusAsync(reservation.Id, ReservationStatus.ReadyToBorrow, TestContext.Current.CancellationToken);

        Assert.Equal(3, book.AvailableQuantity);
        Assert.Equal(FixedClock.Now.AddDays(3), reservation.ExpiresAt);
        Assert.Contains(messages.Items, x => x.Type == NotificationType.ReservationReady);

        var delivery = await service.DeliverAsync(reservation.Id, Guid.NewGuid(), TestContext.Current.CancellationToken);
        var replay = await service.DeliverAsync(reservation.Id, Guid.NewGuid(), TestContext.Current.CancellationToken);

        Assert.Equal(BorrowStatus.Borrowing, delivery.Status);
        Assert.True(replay.Replayed);
        Assert.Equal(delivery.Id, replay.Id);
        Assert.Equal(2, book.AvailableQuantity);
        Assert.Equal(ReservationStatus.Completed, reservation.Status);
        Assert.Single(db.BorrowSlips);
        Assert.Contains(messages.Items, x => x.Type == NotificationType.BorrowDelivered);
    }

    [Fact]
    public async Task Expired_pickup_cancels_without_releasing_stock_that_was_never_decremented()
    {
        await using var db = Db(nameof(Expired_pickup_cancels_without_releasing_stock_that_was_never_decremented));
        var reader = new Reader
        {
            UserAccountId = Guid.NewGuid(), ReaderCode = "DG004", FullName = "Ngô An",
            Email = "ngo.an@example.test", RegisteredOn = new(2026, 1, 1), ExpiresOn = new(2027, 1, 1)
        };
        var book = new Book
        {
            Isbn = "9780000000004", Title = "Sách quá hạn nhận", Language = "vi",
            TotalQuantity = 3, AvailableQuantity = 3, Price = 90_000, Status = BookStatus.Available
        };
        var slip = new BorrowSlip
        {
            Code = "BR-EXPIRED", Reader = reader, ReaderId = reader.Id, TotalBooks = 2,
            RequestedAt = FixedClock.Now.AddDays(-4),
            PickupExpiresAt = null, Status = BorrowStatus.ApprovedAwaitingPickup,
            CreatedByAccountId = reader.UserAccountId, IdempotencyKey = "borrow-expired",
            Details = [new BorrowDetail { Book = book, BookId = book.Id, Quantity = 2, BorrowPrice = book.Price }]
        };
        var recentSlip = new BorrowSlip
        {
            Code = "BR-RECENT", Reader = reader, ReaderId = reader.Id, TotalBooks = 1,
            RequestedAt = FixedClock.Now.AddDays(-2), PickupExpiresAt = null,
            Status = BorrowStatus.ApprovedAwaitingPickup,
            CreatedByAccountId = reader.UserAccountId, IdempotencyKey = "borrow-recent",
            Details = [new BorrowDetail { Book = book, BookId = book.Id, Quantity = 1, BorrowPrice = book.Price }]
        };
        db.AddRange(reader, book, new SystemSettings(), slip, recentSlip);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        var service = new BorrowService(db, new FineCalculator(), new NullNotifications(), new FixedClock());

        Assert.Equal(1, await service.CancelExpiredPickupsAsync(TestContext.Current.CancellationToken));
        Assert.Equal(BorrowStatus.CanceledDueToTimeout, slip.Status);
        Assert.Equal(BorrowStatus.ApprovedAwaitingPickup, recentSlip.Status);
        Assert.Equal(3, book.AvailableQuantity);
    }

    [Fact]
    public async Task Batch_return_creates_one_combined_fine_and_is_idempotent()
    {
        await using var db = Db(nameof(Batch_return_creates_one_combined_fine_and_is_idempotent));
        var accountId = Guid.NewGuid();
        var reader = new Reader
        {
            UserAccountId = accountId, ReaderCode = "DG002", FullName = "Trần Minh Anh",
            Email = "minh@example.test", RegisteredOn = new(2026, 1, 1), ExpiresOn = new(2027, 1, 1)
        };
        var book = new Book
        {
            Isbn = "9780000000002", Title = "Trả sách", Language = "vi",
            TotalQuantity = 2, AvailableQuantity = 0, Price = 200_000, Status = BookStatus.OutOfStock
        };
        var slip = new BorrowSlip
        {
            Code = "BR-TEST", Reader = reader, ReaderId = reader.Id, TotalBooks = 2,
            RequestedAt = new(2026, 7, 1, 0, 0, 0, TimeSpan.Zero),
            DueAt = new(2026, 7, 20, 0, 0, 0, TimeSpan.Zero), Status = BorrowStatus.Borrowing,
            CreatedByAccountId = accountId, IdempotencyKey = "borrow-test",
            Details = [new BorrowDetail { Book = book, BookId = book.Id, Quantity = 2, BorrowPrice = book.Price,
                DueAt = new(2026, 7, 20, 0, 0, 0, TimeSpan.Zero) }]
        };
        db.AddRange(reader, book, new SystemSettings { LateFeePerDay = 5_000 }, slip);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        var service = new BorrowService(db, new FineCalculator(), new NullNotifications(), new FixedClock());
        var command = new ReturnBooks(slip.Id, accountId,
            [new ReturnLine(slip.Details[0].Id, 2, ItemCondition.Damaged, 10, "rách bìa")], "return-001");
        var result = await service.ReturnAsync(command, TestContext.Current.CancellationToken);
        var replay = await service.ReturnAsync(command, TestContext.Current.CancellationToken);
        Assert.NotNull(result.FineId);
        Assert.Equal(70_000, result.FineAmount);
        Assert.True(replay.Replayed);
        Assert.Equal(result.SlipId, replay.SlipId);
        Assert.Equal(result.Status, replay.Status);
        Assert.Equal(result.FineId, replay.FineId);
        Assert.Equal(result.FineAmount, replay.FineAmount);
        Assert.Single(db.Fines);
        Assert.Single(db.BorrowReturns);
    }

    [Fact]
    public async Task Partial_damaged_return_followed_by_intact_return_preserves_lost_or_damaged_status()
    {
        await using var db = Db(nameof(Partial_damaged_return_followed_by_intact_return_preserves_lost_or_damaged_status));
        var accountId = Guid.NewGuid();
        var reader = new Reader
        {
            UserAccountId = accountId, ReaderCode = "DG009", FullName = "Nguyễn Hoàng",
            Email = "hoang@example.test", RegisteredOn = new(2026, 1, 1), ExpiresOn = new(2027, 1, 1)
        };
        var book = new Book
        {
            Isbn = "9780000000009", Title = "Partial Return", Language = "vi",
            TotalQuantity = 2, AvailableQuantity = 0, Price = 100_000, Status = BookStatus.OutOfStock
        };
        var slip = new BorrowSlip
        {
            Code = "BR-PART", Reader = reader, ReaderId = reader.Id, TotalBooks = 2,
            RequestedAt = new(2026, 7, 1, 0, 0, 0, TimeSpan.Zero),
            DueAt = new(2026, 7, 30, 0, 0, 0, TimeSpan.Zero), Status = BorrowStatus.Borrowing,
            CreatedByAccountId = accountId, IdempotencyKey = "borrow-partial",
            Details =
            [
                new BorrowDetail
                {
                    Book = book, BookId = book.Id, Quantity = 2, BorrowPrice = book.Price,
                    DueAt = new(2026, 7, 30, 0, 0, 0, TimeSpan.Zero)
                }
            ]
        };
        db.AddRange(reader, book, new SystemSettings(), slip);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = new BorrowService(db, new FineCalculator(), new NullNotifications(), new FixedClock());
        var part1 = await service.ReturnAsync(
            new ReturnBooks(slip.Id, accountId,
                [new ReturnLine(slip.Details[0].Id, 1, ItemCondition.Damaged, 20, "sước bìa")],
                "ret-part-1"),
            TestContext.Current.CancellationToken);
        Assert.Equal(BorrowStatus.Borrowing, part1.Status);
        Assert.NotNull(part1.FineId);
        Assert.Equal(20_000, part1.FineAmount);

        var part2 = await service.ReturnAsync(
            new ReturnBooks(slip.Id, accountId,
                [new ReturnLine(slip.Details[0].Id, 1, ItemCondition.Intact, 0, null)],
                "ret-part-2"),
            TestContext.Current.CancellationToken);
        Assert.Equal(BorrowStatus.LostOrDamaged, part2.Status);
        Assert.Equal(1, book.AvailableQuantity);
        Assert.Equal(BookStatus.Available, book.Status);
    }

    [Fact]
    public async Task Cannot_request_extension_on_already_returned_detail_and_approved_extension_updates_slip_due_date()
    {
        await using var db = Db(nameof(Cannot_request_extension_on_already_returned_detail_and_approved_extension_updates_slip_due_date));
        var accountId = Guid.NewGuid();
        var reader = new Reader
        {
            UserAccountId = accountId, ReaderCode = "DG010", FullName = "Lê Minh",
            Email = "leminh@example.test", RegisteredOn = new(2026, 1, 1), ExpiresOn = new(2027, 1, 1)
        };
        var b1 = new Book { Isbn = "9780000000010", Title = "Sách 1", Language = "vi", TotalQuantity = 1, AvailableQuantity = 0, Price = 80_000, Status = BookStatus.OutOfStock };
        var b2 = new Book { Isbn = "9780000000011", Title = "Sách 2", Language = "vi", TotalQuantity = 1, AvailableQuantity = 0, Price = 90_000, Status = BookStatus.OutOfStock };
        var due = FixedClock.Now.AddDays(5);
        var slip = new BorrowSlip
        {
            Code = "BR-EXT", Reader = reader, ReaderId = reader.Id, TotalBooks = 2,
            RequestedAt = FixedClock.Now.AddDays(-5),
            BorrowedAt = FixedClock.Now.AddDays(-4),
            DueAt = due, Status = BorrowStatus.Borrowing,
            CreatedByAccountId = accountId, IdempotencyKey = "borrow-ext",
            Details =
            [
                new BorrowDetail { Book = b1, BookId = b1.Id, Quantity = 1, ReturnedQuantity = 1, BorrowPrice = b1.Price, DueAt = due },
                new BorrowDetail { Book = b2, BookId = b2.Id, Quantity = 1, ReturnedQuantity = 0, BorrowPrice = b2.Price, DueAt = due }
            ]
        };
        db.AddRange(reader, b1, b2, new SystemSettings { MaxExtensionDays = 7, MaxExtensionCount = 1 }, slip);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = new BorrowService(db, new FineCalculator(), new NullNotifications(), new FixedClock());
        await Assert.ThrowsAsync<DomainRuleException>(() =>
            service.RequestExtensionAsync(slip.Details[0].Id, 3, "đã trả", TestContext.Current.CancellationToken));

        await service.RequestExtensionAsync(slip.Details[1].Id, 4, "đọc tiếp", TestContext.Current.CancellationToken);
        await service.DecideExtensionAsync(slip.Details[1].Id, true, TestContext.Current.CancellationToken);
        Assert.Equal(due.AddDays(4), slip.Details[1].DueAt);
        Assert.Equal(due.AddDays(4), slip.DueAt);
    }

    [Fact]
    public async Task Reader_can_cancel_own_pending_borrow_and_overdue_worker_marks_overdue_and_notifies()
    {
        await using var db = Db(nameof(Reader_can_cancel_own_pending_borrow_and_overdue_worker_marks_overdue_and_notifies));
        var reader = new Reader
        {
            UserAccountId = Guid.NewGuid(), ReaderCode = "DG020", FullName = "Trần An",
            Email = "tran.an@example.test", RegisteredOn = new(2026, 1, 1), ExpiresOn = new(2027, 1, 1)
        };
        var book = new Book { Isbn = "9780000000020", Title = "Worker Test", Language = "vi", TotalQuantity = 3, AvailableQuantity = 2, Price = 50_000, Status = BookStatus.Available };
        var pendingSlip = new BorrowSlip
        {
            Code = "BR-PENDING", Reader = reader, ReaderId = reader.Id, TotalBooks = 1,
            RequestedAt = FixedClock.Now, Status = BorrowStatus.PendingApproval,
            CreatedByAccountId = reader.UserAccountId, IdempotencyKey = "pend-1",
            Details = [new BorrowDetail { Book = book, BookId = book.Id, Quantity = 1, BorrowPrice = book.Price }]
        };
        var overdueSlip = new BorrowSlip
        {
            Code = "BR-LATE", Reader = reader, ReaderId = reader.Id, TotalBooks = 1,
            RequestedAt = FixedClock.Now.AddDays(-16), BorrowedAt = FixedClock.Now.AddDays(-15),
            DueAt = FixedClock.Now.AddDays(-1), Status = BorrowStatus.Borrowing,
            CreatedByAccountId = reader.UserAccountId, IdempotencyKey = "late-1",
            Details = [new BorrowDetail { Book = book, BookId = book.Id, Quantity = 1, BorrowPrice = book.Price, DueAt = FixedClock.Now.AddDays(-1) }]
        };
        db.AddRange(reader, book, new SystemSettings(), pendingSlip, overdueSlip);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var notifications = new RecordingNotifications();
        var service = new BorrowService(db, new FineCalculator(), notifications, new FixedClock());

        await Assert.ThrowsAsync<DomainRuleException>(() =>
            service.CancelByReaderAsync(pendingSlip.Id, reader.Id + 99, TestContext.Current.CancellationToken));
        await service.CancelByReaderAsync(pendingSlip.Id, reader.Id, TestContext.Current.CancellationToken);
        Assert.Equal(BorrowStatus.Cancelled, pendingSlip.Status);

        var transitioned = await service.ProcessOverdueAndDueSoonAsync(TestContext.Current.CancellationToken);
        Assert.Equal(1, transitioned);
        Assert.Equal(BorrowStatus.Overdue, overdueSlip.Status);
        Assert.Contains(notifications.Items, x => x.Type == NotificationType.BorrowOverdue);
    }

    [Fact]
    public async Task Borrow_create_enforces_cumulative_limit_and_card_expiry()
    {
        await using var db = Db(nameof(Borrow_create_enforces_cumulative_limit_and_card_expiry));
        var ct = TestContext.Current.CancellationToken;
        var reader = new Reader
        {
            UserAccountId = Guid.NewGuid(),
            ReaderCode = "DG-LIM",
            FullName = "Phạm Văn Hùng",
            Email = "hung@example.test",
            RegisteredOn = new(2026, 1, 1),
            ExpiresOn = new(2027, 1, 1),
            MaxBorrowBooks = 3
        };
        var expiredReader = new Reader
        {
            UserAccountId = Guid.NewGuid(),
            ReaderCode = "DG-EXP",
            FullName = "Nguyễn Thị Lan",
            Email = "lan@example.test",
            RegisteredOn = new(2025, 1, 1),
            ExpiresOn = new(2026, 7, 1),
            MaxBorrowBooks = 5
        };
        var book = new Book
        {
            Isbn = "9780000000031",
            Title = "Aggregate Limit",
            Language = "vi",
            TotalQuantity = 5,
            AvailableQuantity = 5,
            Price = 90_000,
            Status = BookStatus.Available
        };
        db.AddRange(reader, expiredReader, book, new SystemSettings { MaxBorrowBooks = 10 });
        await db.SaveChangesAsync(ct);

        var service = new BorrowService(db, new FineCalculator(), new NullNotifications(), new FixedClock());
        await service.CreateAsync(new(reader.Id, reader.UserAccountId, [new(book.Id, 2)], null, "cum-1"), ct);

        var limitEx = await Assert.ThrowsAsync<DomainRuleException>(() =>
            service.CreateAsync(new(reader.Id, reader.UserAccountId, [new(book.Id, 2)], null, "cum-2"), ct));
        Assert.Equal("borrow.limit", limitEx.Code);

        var expEx = await Assert.ThrowsAsync<DomainRuleException>(() =>
            service.CreateAsync(new(expiredReader.Id, expiredReader.UserAccountId, [new(book.Id, 1)], null, "exp-1"), ct));
        Assert.Equal("reader.expired", expEx.Code);
    }

    [Fact]
    public async Task Approve_blocks_overcommitting_stock_across_slips_while_keeping_available_until_pickup()
    {
        await using var db = Db(nameof(Approve_blocks_overcommitting_stock_across_slips_while_keeping_available_until_pickup));
        var ct = TestContext.Current.CancellationToken;
        var reader1 = new Reader
        {
            UserAccountId = Guid.NewGuid(),
            ReaderCode = "DG-C1",
            FullName = "Độc giả Một",
            Email = "dg1@example.test",
            RegisteredOn = new(2026, 1, 1),
            ExpiresOn = new(2027, 1, 1)
        };
        var reader2 = new Reader
        {
            UserAccountId = Guid.NewGuid(),
            ReaderCode = "DG-C2",
            FullName = "Độc giả Hai",
            Email = "dg2@example.test",
            RegisteredOn = new(2026, 1, 1),
            ExpiresOn = new(2027, 1, 1)
        };
        var book = new Book
        {
            Isbn = "9780000000032",
            Title = "Single Copy Book",
            Language = "vi",
            TotalQuantity = 1,
            AvailableQuantity = 1,
            Price = 120_000,
            Status = BookStatus.Available
        };
        db.AddRange(reader1, reader2, book, new SystemSettings());
        await db.SaveChangesAsync(ct);

        var service = new BorrowService(db, new FineCalculator(), new NullNotifications(), new FixedClock());
        var s1 = await service.CreateAsync(new(reader1.Id, reader1.UserAccountId, [new(book.Id, 1)], null, "oc-1"), ct);
        var s2 = await service.CreateAsync(new(reader2.Id, reader2.UserAccountId, [new(book.Id, 1)], null, "oc-2"), ct);

        await service.ApproveAsync(s1.Id, ct);
        Assert.Equal(1, book.AvailableQuantity);

        var ex = await Assert.ThrowsAsync<DomainRuleException>(() => service.ApproveAsync(s2.Id, ct));
        Assert.Equal("book.stock.insufficient", ex.Code);
    }

    [Fact]
    public async Task Walk_in_immediate_delivery_creates_approves_and_decrements_stock_atomically()
    {
        await using var db = Db(nameof(Walk_in_immediate_delivery_creates_approves_and_decrements_stock_atomically));
        var ct = TestContext.Current.CancellationToken;
        var reader = new Reader
        {
            UserAccountId = Guid.NewGuid(),
            ReaderCode = "DG-WI",
            FullName = "Trần Minh",
            Email = "tm@example.test",
            RegisteredOn = new(2026, 1, 1),
            ExpiresOn = new(2027, 1, 1)
        };
        var book = new Book
        {
            Isbn = "9780000000033",
            Title = "Walk-in Book",
            Language = "vi",
            TotalQuantity = 3,
            AvailableQuantity = 3,
            Price = 75_000,
            Status = BookStatus.Available
        };
        db.AddRange(reader, book, new SystemSettings { BorrowPeriodDays = 14 });
        await db.SaveChangesAsync(ct);

        var service = new BorrowService(db, new FineCalculator(), new NullNotifications(), new FixedClock());
        var result = await service.CreateWalkInAsync(
            new(reader.Id, Guid.NewGuid(), [new(book.Id, 2)], "Tại quầy", true, "wi-1"), ct);

        Assert.Equal(BorrowStatus.Borrowing, result.Status);
        Assert.Equal(1, book.AvailableQuantity);
        var slip = await db.BorrowSlips.Include(x => x.Details).SingleAsync(x => x.Id == result.Id, ct);
        Assert.Equal(FixedClock.Now.AddDays(14), slip.DueAt);
        Assert.All(slip.Details, d => Assert.Equal(slip.DueAt, d.DueAt));
    }

    [Fact]
    public async Task Return_rejects_duplicate_borrow_detail_lines()
    {
        await using var db = Db(nameof(Return_rejects_duplicate_borrow_detail_lines));
        var ct = TestContext.Current.CancellationToken;
        var reader = new Reader
        {
            UserAccountId = Guid.NewGuid(),
            ReaderCode = "DG-DUP",
            FullName = "Đỗ Hải",
            Email = "hai@example.test",
            RegisteredOn = new(2026, 1, 1),
            ExpiresOn = new(2027, 1, 1)
        };
        var book = new Book
        {
            Isbn = "9780000000034",
            Title = "Dup Detail Book",
            Language = "vi",
            TotalQuantity = 2,
            AvailableQuantity = 0,
            Price = 60_000,
            Status = BookStatus.OutOfStock
        };
        var slip = new BorrowSlip
        {
            Code = "BR-DUP",
            Reader = reader,
            TotalBooks = 2,
            RequestedAt = FixedClock.Now.AddDays(-5),
            BorrowedAt = FixedClock.Now.AddDays(-4),
            DueAt = FixedClock.Now.AddDays(5),
            Status = BorrowStatus.Borrowing,
            CreatedByAccountId = reader.UserAccountId,
            IdempotencyKey = "dup-slip",
            Details = [new BorrowDetail { Book = book, Quantity = 2, BorrowPrice = book.Price, DueAt = FixedClock.Now.AddDays(5) }]
        };
        db.AddRange(reader, book, new SystemSettings(), slip);
        await db.SaveChangesAsync(ct);

        var service = new BorrowService(db, new FineCalculator(), new NullNotifications(), new FixedClock());
        var detailId = slip.Details.Single().Id;
        var ex = await Assert.ThrowsAsync<DomainRuleException>(() => service.ReturnAsync(new(
            slip.Id,
            Guid.NewGuid(),
            [new(detailId, 1, ItemCondition.Intact, 0, null), new(detailId, 1, ItemCondition.Intact, 0, null)],
            "ret-dup"), ct));
        Assert.Equal("return.duplicate_line", ex.Code);
    }

    [Fact]
    public async Task Deleted_or_resigned_employee_loses_all_permissions()
    {
        await using var db = Db(nameof(Deleted_or_resigned_employee_loses_all_permissions));
        var ct = TestContext.Current.CancellationToken;
        var role = new Role { Code = "LIBRARIAN", Name = "Thủ thư" };
        var perm = new Permission { Code = PermissionCodes.BookEdit, DisplayName = "Sửa sách", GroupName = "Books" };
        db.AddRange(role, perm);
        await db.SaveChangesAsync(ct);

        var emp = new Employee
        {
            EmployeeCode = "NV-DEL",
            FullName = "Nguyễn Bình",
            Email = "binh@example.test",
            Phone = "0912345678",
            Address = "Hà Nội",
            NationalId = "001234567891",
            DateOfBirth = new(1994, 5, 1),
            HiredOn = new(2022, 1, 1),
            PositionId = 1,
            Salary = 8_000_000,
            RoleId = role.Id,
            Status = EmploymentStatus.Working
        };
        var accountId = Guid.NewGuid();
        db.Add(emp);
        await db.SaveChangesAsync(ct);
        db.Add(new RolePermission { RoleId = role.Id, PermissionId = perm.Id, Effect = PermissionEffect.Allow });
        db.Add(new StaffAccount { EmployeeId = emp.Id, UserAccountId = accountId });
        await db.SaveChangesAsync(ct);

        var perms = new PermissionService(db);
        Assert.True(await perms.HasAsync(accountId, PermissionCodes.BookEdit, ct));

        emp.Status = EmploymentStatus.Resigned;
        await db.SaveChangesAsync(ct);
        Assert.False(await perms.HasAsync(accountId, PermissionCodes.BookEdit, ct));

        emp.Status = EmploymentStatus.Working;
        emp.IsDeleted = true;
        await db.SaveChangesAsync(ct);
        Assert.False(await perms.HasAsync(accountId, PermissionCodes.BookEdit, ct));
    }

    private static LibraryDbContext Db(string name) =>
        new(new DbContextOptionsBuilder<LibraryDbContext>().UseInMemoryDatabase(name).Options);
    private static Employee ValidEmployee() => new()
    {
        EmployeeCode = "NV001", FullName = "Nguyễn Văn An", Email = "nv@example.test",
        Phone = "0912345678", Address = "Hà Nội", NationalId = "001234567890",
        DateOfBirth = new(1995, 1, 1), HiredOn = new(2020, 1, 1), PositionId = 1, Salary = 8_500_000
    };
    private sealed class FixedClock : IClock
    {
        public static DateTimeOffset Now => new(2026, 7, 23, 0, 0, 0, TimeSpan.Zero);
        public DateTimeOffset UtcNow => Now;
    }
    private sealed class NullNotifications : INotificationPublisher
    {
        public Task PublishAsync(Guid accountId, NotificationType type, string title, string message,
            string? link, string? correlationId, CancellationToken cancellationToken) => Task.CompletedTask;
    }
    private sealed class RecordingNotifications : INotificationPublisher
    {
        public List<(NotificationType Type, string Title, string Message)> Items { get; } = [];
        public Task PublishAsync(Guid accountId, NotificationType type, string title, string message,
            string? link, string? correlationId, CancellationToken cancellationToken)
        {
            Items.Add((type, title, message));
            return Task.CompletedTask;
        }
    }
}
