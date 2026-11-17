using LibraryHub.Application;
using LibraryHub.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LibraryHub.Infrastructure;

public sealed class LibraryDbContext(DbContextOptions<LibraryDbContext> options)
    : IdentityDbContext<LibraryUser, IdentityRole<Guid>, Guid>(options), ILibraryStore
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Publisher> Publishers => Set<Publisher>();
    public DbSet<Shelf> Shelves => Set<Shelf>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Reader> Readers => Set<Reader>();
    public DbSet<ReaderFeaturedBook> ReaderFeaturedBooks => Set<ReaderFeaturedBook>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<EmployeeStatusOption> EmployeeStatusOptions => Set<EmployeeStatusOption>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<StaffAccount> StaffAccounts => Set<StaffAccount>();
    public DbSet<Role> SecurityRoles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserPermission> UserPermissions => Set<UserPermission>();
    public DbSet<BorrowSlip> BorrowSlips => Set<BorrowSlip>();
    public DbSet<BorrowDetail> BorrowDetails => Set<BorrowDetail>();
    public DbSet<BorrowReturn> BorrowReturns => Set<BorrowReturn>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<Fine> Fines => Set<Fine>();
    public DbSet<FineDetail> FineDetails => Set<FineDetail>();
    public DbSet<SystemSettings> SystemSettings => Set<SystemSettings>();
    public DbSet<BookFavorite> BookFavorites => Set<BookFavorite>();
    public DbSet<BookComment> BookComments => Set<BookComment>();
    public DbSet<BookCommentLike> BookCommentLikes => Set<BookCommentLike>();
    public DbSet<BookRating> BookRatings => Set<BookRating>();
    public DbSet<BookViewHistory> BookViewHistories => Set<BookViewHistory>();
    public DbSet<ReadingHistory> ReadingHistories => Set<ReadingHistory>();
    public DbSet<ProfileComment> ProfileComments => Set<ProfileComment>();
    public DbSet<ProfileTagPin> ProfileTagPins => Set<ProfileTagPin>();
    public DbSet<CommentReport> CommentReports => Set<CommentReport>();
    public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();
    public DbSet<SupportMessage> SupportMessages => Set<SupportMessage>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.Entity<Category>(x => x.Property(y => y.ImagePath).HasMaxLength(2048));
        b.Entity<Book>(x =>
        {
            x.HasIndex(y => y.Isbn).IsUnique();
            x.Property(y => y.Price).HasPrecision(18, 2);
            x.Property(y => y.SuspensionReason).HasMaxLength(500);
            x.Property(y => y.RestoreReason).HasMaxLength(500);
            x.Property(y => y.LastLifecycleOperationKey).HasMaxLength(100);
            x.HasOne(y => y.Category).WithMany().HasForeignKey(y => y.CategoryId).OnDelete(DeleteBehavior.Restrict);
            x.HasOne(y => y.Author).WithMany().HasForeignKey(y => y.AuthorId).OnDelete(DeleteBehavior.Restrict);
            x.HasOne(y => y.Publisher).WithMany().HasForeignKey(y => y.PublisherId).OnDelete(DeleteBehavior.Restrict);
            x.HasOne(y => y.Shelf).WithMany().HasForeignKey(y => y.ShelfId).OnDelete(DeleteBehavior.Restrict);
            x.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Book_Quantity", "[TotalQuantity] >= 0 AND [AvailableQuantity] >= 0 AND [AvailableQuantity] <= [TotalQuantity]");
                t.HasCheckConstraint("CK_Book_Price", "[Price] >= 0");
            });
        });
        b.Entity<Reader>(x =>
        {
            x.HasIndex(y => y.UserAccountId).IsUnique();
            x.HasIndex(y => y.ReaderCode).IsUnique();
            x.HasIndex(y => y.Email).IsUnique();
        });
        b.Entity<ReaderFeaturedBook>(x =>
        {
            x.HasIndex(y => new { y.ReaderId, y.BookId }).IsUnique();
            x.HasIndex(y => new { y.ReaderId, y.DisplayOrder }).IsUnique();
            x.HasOne<Reader>().WithMany().HasForeignKey(y => y.ReaderId).OnDelete(DeleteBehavior.Cascade);
            x.HasOne<Book>().WithMany().HasForeignKey(y => y.BookId).OnDelete(DeleteBehavior.Restrict);
            x.ToTable(t => t.HasCheckConstraint("CK_FeaturedBook_Order", "[DisplayOrder] BETWEEN 1 AND 4"));
        });
        b.Entity<Position>(x =>
        {
            x.HasIndex(y => y.Code).IsUnique();
            x.Property(y => y.SalaryAmount).HasPrecision(18, 2);
        });
        b.Entity<Employee>(x =>
        {
            x.HasIndex(y => y.EmployeeCode).IsUnique();
            x.HasIndex(y => y.Email).IsUnique();
            x.HasIndex(y => y.NationalId).IsUnique();
            x.Property(y => y.Salary).HasPrecision(18, 2);
            x.HasOne(y => y.Position).WithMany().HasForeignKey(y => y.PositionId).OnDelete(DeleteBehavior.Restrict);
            x.HasOne<EmployeeStatusOption>().WithMany().HasForeignKey(y => y.StatusOptionId).OnDelete(DeleteBehavior.Restrict);
            x.HasOne<Role>().WithMany().HasForeignKey(y => y.RoleId).OnDelete(DeleteBehavior.Restrict);
        });
        b.Entity<StaffAccount>(x =>
        {
            x.HasIndex(y => y.UserAccountId).IsUnique();
            x.HasIndex(y => y.EmployeeId).IsUnique();
            x.HasOne<Employee>().WithOne().HasForeignKey<StaffAccount>(y => y.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            x.HasOne<LibraryUser>().WithOne().HasForeignKey<StaffAccount>(y => y.UserAccountId).OnDelete(DeleteBehavior.Cascade);
        });
        b.Entity<Role>(x => x.HasIndex(y => y.Code).IsUnique());
        b.Entity<Permission>(x => x.HasIndex(y => y.Code).IsUnique());
        b.Entity<RolePermission>(x =>
        {
            x.HasIndex(y => new { y.RoleId, y.PermissionId }).IsUnique();
            x.HasOne<Role>().WithMany().HasForeignKey(y => y.RoleId).OnDelete(DeleteBehavior.Cascade);
            x.HasOne<Permission>().WithMany().HasForeignKey(y => y.PermissionId).OnDelete(DeleteBehavior.Cascade);
        });
        b.Entity<UserPermission>(x =>
        {
            x.HasIndex(y => new { y.EmployeeId, y.PermissionId }).IsUnique();
            x.HasOne<Employee>().WithMany().HasForeignKey(y => y.EmployeeId).OnDelete(DeleteBehavior.Cascade);
            x.HasOne<Permission>().WithMany().HasForeignKey(y => y.PermissionId).OnDelete(DeleteBehavior.Cascade);
        });
        b.Entity<BorrowSlip>(x =>
        {
            x.HasIndex(y => y.Code).IsUnique();
            x.HasIndex(y => y.IdempotencyKey).IsUnique();
            x.HasOne(y => y.Reader).WithMany().HasForeignKey(y => y.ReaderId).OnDelete(DeleteBehavior.Restrict);
            x.HasMany(y => y.Details).WithOne().HasForeignKey(y => y.BorrowSlipId).OnDelete(DeleteBehavior.Cascade);
        });
        b.Entity<BorrowDetail>(x =>
        {
            x.HasIndex(y => new { y.BorrowSlipId, y.BookId }).IsUnique();
            x.Property(y => y.BorrowPrice).HasPrecision(18, 2);
            x.HasOne(y => y.Book).WithMany().HasForeignKey(y => y.BookId).OnDelete(DeleteBehavior.Restrict);
            x.ToTable(t => t.HasCheckConstraint("CK_BorrowDetail_Quantity", "[Quantity] > 0 AND [ReturnedQuantity] >= 0 AND [ReturnedQuantity] <= [Quantity]"));
        });
        b.Entity<BorrowReturn>(x =>
        {
            x.HasIndex(y => new { y.IdempotencyKey, y.BorrowDetailId }).IsUnique();
            x.Property(y => y.DamagePercent).HasPrecision(5, 2);
            x.Property(y => y.FineAmount).HasPrecision(18, 2);
            x.HasOne<BorrowDetail>().WithMany().HasForeignKey(y => y.BorrowDetailId).OnDelete(DeleteBehavior.Cascade);
        });
        b.Entity<Reservation>(x =>
        {
            x.HasIndex(y => new { y.ReaderId, y.BookId, y.Status });
            x.HasOne<Reader>().WithMany().HasForeignKey(y => y.ReaderId).OnDelete(DeleteBehavior.Restrict);
            x.HasOne<Book>().WithMany().HasForeignKey(y => y.BookId).OnDelete(DeleteBehavior.Restrict);
        });
        b.Entity<Fine>(x =>
        {
            x.HasIndex(y => y.Code).IsUnique();
            x.Property(y => y.Amount).HasPrecision(18, 2);
            x.Property(y => y.PaidAmount).HasPrecision(18, 2);
            x.HasMany(y => y.Details).WithOne().HasForeignKey(y => y.FineId).OnDelete(DeleteBehavior.Cascade);
            x.HasOne<BorrowSlip>().WithMany().HasForeignKey(y => y.BorrowSlipId).OnDelete(DeleteBehavior.Restrict);
            x.HasOne<Reader>().WithMany().HasForeignKey(y => y.ReaderId).OnDelete(DeleteBehavior.Restrict);
        });
        b.Entity<FineDetail>(x =>
        {
            x.Property(y => y.Amount).HasPrecision(18, 2);
            x.Property(y => y.BookPrice).HasPrecision(18, 2);
            x.Property(y => y.DamagePercent).HasPrecision(5, 2);
            x.HasOne<Book>().WithMany().HasForeignKey(y => y.BookId).OnDelete(DeleteBehavior.Restrict);
            x.HasOne<BorrowDetail>().WithMany().HasForeignKey(y => y.BorrowDetailId).OnDelete(DeleteBehavior.Restrict);
        });
        b.Entity<SystemSettings>(x => x.Property(y => y.LateFeePerDay).HasPrecision(18, 2));
        b.Entity<BookFavorite>(x =>
        {
            x.HasIndex(y => new { y.BookId, y.ReaderId }).IsUnique();
            x.HasOne<Book>().WithMany().HasForeignKey(y => y.BookId).OnDelete(DeleteBehavior.Restrict);
            x.HasOne<Reader>().WithMany().HasForeignKey(y => y.ReaderId).OnDelete(DeleteBehavior.NoAction);
        });
        b.Entity<BookComment>(x =>
        {
            x.HasOne<Book>().WithMany().HasForeignKey(y => y.BookId).OnDelete(DeleteBehavior.Restrict);
            x.HasOne<Reader>().WithMany().HasForeignKey(y => y.ReaderId).OnDelete(DeleteBehavior.NoAction);
            x.HasOne<BookComment>().WithMany().HasForeignKey(y => y.ParentCommentId).OnDelete(DeleteBehavior.NoAction);
        });
        b.Entity<BookCommentLike>(x =>
        {
            x.HasIndex(y => new { y.BookCommentId, y.ReaderId }).IsUnique();
            x.HasOne<BookComment>().WithMany().HasForeignKey(y => y.BookCommentId).OnDelete(DeleteBehavior.Cascade);
            x.HasOne<Reader>().WithMany().HasForeignKey(y => y.ReaderId).OnDelete(DeleteBehavior.NoAction);
        });
        b.Entity<BookRating>(x =>
        {
            x.HasIndex(y => new { y.BookId, y.ReaderId }).IsUnique();
            x.HasOne<Book>().WithMany().HasForeignKey(y => y.BookId).OnDelete(DeleteBehavior.Restrict);
            x.HasOne<Reader>().WithMany().HasForeignKey(y => y.ReaderId).OnDelete(DeleteBehavior.NoAction);
            x.ToTable(t => t.HasCheckConstraint("CK_BookRating_Stars", "[Stars] BETWEEN 1 AND 5"));
        });
        b.Entity<BookViewHistory>(x =>
        {
            x.HasOne<Book>().WithMany().HasForeignKey(y => y.BookId).OnDelete(DeleteBehavior.Restrict);
            x.HasOne<Reader>().WithMany().HasForeignKey(y => y.ReaderId).OnDelete(DeleteBehavior.NoAction);
        });
        b.Entity<ReadingHistory>(x =>
        {
            x.HasIndex(y => new { y.BookId, y.ReaderId }).IsUnique();
            x.HasOne<Book>().WithMany().HasForeignKey(y => y.BookId).OnDelete(DeleteBehavior.Restrict);
            x.HasOne<Reader>().WithMany().HasForeignKey(y => y.ReaderId).OnDelete(DeleteBehavior.NoAction);
        });
        b.Entity<ProfileComment>(x =>
        {
            x.HasOne<Reader>().WithMany().HasForeignKey(y => y.ProfileOwnerId).OnDelete(DeleteBehavior.NoAction);
            x.HasOne<Reader>().WithMany().HasForeignKey(y => y.AuthorId).OnDelete(DeleteBehavior.NoAction);
        });
        b.Entity<ProfileTagPin>(x =>
        {
            x.HasOne<Reader>().WithMany().HasForeignKey(y => y.ProfileOwnerId).OnDelete(DeleteBehavior.NoAction);
            x.HasOne<Reader>().WithMany().HasForeignKey(y => y.AuthorId).OnDelete(DeleteBehavior.NoAction);
        });
        b.Entity<CommentReport>(x =>
        {
            x.HasOne<Reader>().WithMany().HasForeignKey(y => y.ReporterId).OnDelete(DeleteBehavior.NoAction);
            x.HasOne<Reader>().WithMany().HasForeignKey(y => y.ReportedUserId).OnDelete(DeleteBehavior.NoAction);
            x.HasOne<BookComment>().WithMany().HasForeignKey(y => y.BookCommentId).OnDelete(DeleteBehavior.NoAction);
            x.HasOne<ProfileComment>().WithMany().HasForeignKey(y => y.ProfileCommentId).OnDelete(DeleteBehavior.NoAction);
            x.ToTable(t => t.HasCheckConstraint("CK_CommentReport_Target", "([BookCommentId] IS NOT NULL AND [ProfileCommentId] IS NULL) OR ([BookCommentId] IS NULL AND [ProfileCommentId] IS NOT NULL)"));
        });
        b.Entity<SupportTicket>(x =>
        {
            x.HasIndex(y => y.Code).IsUnique();
            x.HasMany(y => y.Messages).WithOne().HasForeignKey(y => y.SupportTicketId).OnDelete(DeleteBehavior.Cascade);
            x.HasOne<Reader>().WithMany().HasForeignKey(y => y.ReaderId).OnDelete(DeleteBehavior.Restrict);
            x.HasOne<LibraryUser>().WithMany().HasForeignKey(y => y.AssignedStaffAccountId).OnDelete(DeleteBehavior.NoAction);
        });
        b.Entity<SupportMessage>(x =>
            x.HasOne<LibraryUser>().WithMany().HasForeignKey(y => y.SenderAccountId).OnDelete(DeleteBehavior.NoAction));
        b.Entity<Notification>(x =>
        {
            x.HasIndex(y => new { y.RecipientAccountId, y.IsRead, y.CreatedAt });
            x.Property(y => y.CorrelationKey).HasMaxLength(200);
            x.HasIndex(y => new { y.RecipientAccountId, y.Type, y.CorrelationKey })
                .IsUnique()
                .HasFilter("[CorrelationKey] IS NOT NULL");
            x.HasOne<LibraryUser>().WithMany().HasForeignKey(y => y.RecipientAccountId).OnDelete(DeleteBehavior.Cascade);
            x.HasOne<LibraryUser>().WithMany().HasForeignKey(y => y.CreatedByAccountId).OnDelete(DeleteBehavior.NoAction);
        });
        ConfigureRows(b);
    }

    public async Task<T> TransactionAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken ct)
    {
        if (Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory") return await operation(ct);
        var strategy = Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await Database.BeginTransactionAsync(ct);
            try
            {
                var result = await operation(ct);
                await tx.CommitAsync(ct);
                return result;
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        });
    }

    public async Task<SystemSettings> GetSettingsAsync(CancellationToken ct) =>
        await SystemSettings.SingleOrDefaultAsync(ct) ?? new SystemSettings();
    public Task<Reader?> FindReaderAsync(int id, CancellationToken ct) => Readers.SingleOrDefaultAsync(x => x.Id == id, ct);
    public Task<Reader?> FindReaderForUpdateAsync(int id, CancellationToken ct) =>
        Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory"
            ? FindReaderAsync(id, ct)
            : Readers.FromSqlInterpolated($"SELECT * FROM [Readers] WITH (UPDLOCK, ROWLOCK) WHERE [Id] = {id}")
                .SingleOrDefaultAsync(ct);
    public Task<int> CountReaderActiveBorrowBooksAsync(int readerId, CancellationToken ct) =>
        (from d in BorrowDetails
         join s in BorrowSlips on d.BorrowSlipId equals s.Id
         where s.ReaderId == readerId
             && (s.Status == BorrowStatus.PendingApproval
                 || s.Status == BorrowStatus.ApprovedAwaitingPickup
                 || s.Status == BorrowStatus.Borrowing
                 || s.Status == BorrowStatus.Overdue
                 || s.Status == BorrowStatus.PendingExtension)
         select d.Quantity - d.ReturnedQuantity).SumAsync(ct);
    public async Task<int> CountCommittedBookQuantityAsync(int bookId, int? excludeSlipId, CancellationToken ct)
    {
        var approvedPickups = await (from d in BorrowDetails
                                     join s in BorrowSlips on d.BorrowSlipId equals s.Id
                                     where d.BookId == bookId
                                         && s.Id != excludeSlipId
                                         && s.Status == BorrowStatus.ApprovedAwaitingPickup
                                     select d.Quantity).SumAsync(ct);
        var heldReservations = await Reservations
            .CountAsync(r => r.BookId == bookId && r.Status == ReservationStatus.ReadyToBorrow, ct);
        return approvedPickups + heldReservations;
    }
    public async Task<IReadOnlyDictionary<int, Reader>> FindReadersByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken ct) =>
        ids.Count == 0
            ? new Dictionary<int, Reader>()
            : await Readers.Where(x => ids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);
    public async Task<IReadOnlyDictionary<int, Book>> FindBooksByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken ct) =>
        ids.Count == 0
            ? new Dictionary<int, Book>()
            : await Books.Where(x => ids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);
    public Task<Book?> FindBookAsync(int id, CancellationToken ct) => Books.SingleOrDefaultAsync(x => x.Id == id, ct);
    public Task<Book?> FindBookForUpdateAsync(int id, CancellationToken ct) =>
        Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory"
            ? FindBookAsync(id, ct)
            : Books.FromSqlInterpolated($"SELECT * FROM [Books] WITH (UPDLOCK, ROWLOCK) WHERE [Id] = {id}")
                .SingleOrDefaultAsync(ct);
    public Task<BorrowSlip?> FindBorrowAsync(int id, CancellationToken ct) => BorrowQuery().SingleOrDefaultAsync(x => x.Id == id, ct);
    public Task<BorrowSlip?> FindBorrowForUpdateAsync(int id, CancellationToken ct) =>
        Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory"
            ? FindBorrowAsync(id, ct)
            : BorrowSlips.FromSqlInterpolated(
                    $"SELECT * FROM [BorrowSlips] WITH (UPDLOCK, ROWLOCK) WHERE [Id] = {id}")
                .Include(x => x.Reader).Include(x => x.Details).ThenInclude(x => x.Book)
                .SingleOrDefaultAsync(ct);
    public Task<BorrowSlip?> FindBorrowByDetailAsync(int detailId, CancellationToken ct) =>
        BorrowQuery().SingleOrDefaultAsync(x => x.Details.Any(y => y.Id == detailId), ct);
    public Task<BorrowSlip?> FindBorrowByKeyAsync(string key, CancellationToken ct) =>
        BorrowQuery().SingleOrDefaultAsync(x => x.IdempotencyKey == key, ct);
    public Task<BorrowReturn?> FindReturnByKeyAsync(string key, CancellationToken ct) =>
        BorrowReturns.FirstOrDefaultAsync(x => x.IdempotencyKey == key, ct);
    public Task<bool> HasDamagedOrLostReturnAsync(int borrowSlipId, CancellationToken ct) =>
        (from ret in BorrowReturns
         join detail in BorrowDetails on ret.BorrowDetailId equals detail.Id
         where detail.BorrowSlipId == borrowSlipId && ret.Condition != ItemCondition.Intact
         select ret.Id).AnyAsync(ct);
    public async Task<IReadOnlyList<BorrowSlip>> FindExpiredPickupsAsync(
        DateTimeOffset now,
        DateTimeOffset legacyCutoff,
        CancellationToken ct) =>
        await BorrowQuery().Where(x => x.Status == BorrowStatus.ApprovedAwaitingPickup
            && (x.PickupExpiresAt <= now || (x.PickupExpiresAt == null && x.RequestedAt <= legacyCutoff)))
            .ToListAsync(ct);
    public async Task<IReadOnlyList<BorrowSlip>> FindActiveDueOrOverdueBorrowsAsync(
        DateTimeOffset dueSoonCutoff,
        CancellationToken ct) =>
        await BorrowQuery()
            .Where(x => x.Status == BorrowStatus.Borrowing || x.Status == BorrowStatus.PendingExtension)
            .Where(x => x.Details.Any(d => d.ReturnedQuantity < d.Quantity
                && (d.DueAt ?? x.DueAt) != null
                && (d.DueAt ?? x.DueAt) <= dueSoonCutoff))
            .ToListAsync(ct);
    public Task<Reservation?> FindReservationAsync(int id, CancellationToken ct) =>
        Reservations.SingleOrDefaultAsync(x => x.Id == id, ct);
    public Task<Reservation?> FindReservationForUpdateAsync(int id, CancellationToken ct) =>
        Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory"
            ? FindReservationAsync(id, ct)
            : Reservations.FromSqlInterpolated(
                    $"SELECT * FROM [Reservations] WITH (UPDLOCK, ROWLOCK) WHERE [Id] = {id}")
                .SingleOrDefaultAsync(ct);
    public async Task<IReadOnlyList<Reservation>> FindExpiredReservationsAsync(
        DateTimeOffset now,
        DateTimeOffset legacyCutoff,
        CancellationToken ct) =>
        await Reservations.Where(x => x.Status == ReservationStatus.ReadyToBorrow
                && (x.ExpiresAt <= now || (x.ExpiresAt == null && x.ReservedAt <= legacyCutoff)))
            .ToListAsync(ct);
    public Task<int?> FindEmployeeIdByAccountAsync(Guid accountId, CancellationToken ct) =>
        (from sa in StaffAccounts
         join e in Employees on sa.EmployeeId equals e.Id
         where sa.UserAccountId == accountId
             && !e.IsDeleted
             && e.Status == EmploymentStatus.Working
         select (int?)sa.EmployeeId).SingleOrDefaultAsync(ct);
    public async Task<PermissionEffect?> FindUserPermissionAsync(int employeeId, string code, CancellationToken ct) =>
        await (from up in UserPermissions
               join e in Employees on up.EmployeeId equals e.Id
               join p in Permissions on up.PermissionId equals p.Id
               where up.EmployeeId == employeeId
                   && !e.IsDeleted
                   && e.Status == EmploymentStatus.Working
                   && p.Code == code
               select (PermissionEffect?)up.Effect).SingleOrDefaultAsync(ct);
    public async Task<IReadOnlyList<PermissionEffect>> FindRolePermissionsAsync(int employeeId, string code, CancellationToken ct) =>
        await (from e in Employees
               join rp in RolePermissions on e.RoleId equals rp.RoleId
               join p in Permissions on rp.PermissionId equals p.Id
               where e.Id == employeeId
                   && !e.IsDeleted
                   && e.Status == EmploymentStatus.Working
                   && p.Code == code
               select rp.Effect).ToListAsync(ct);
    void ILibraryStore.Add<TEntity>(TEntity entity) => Add(entity);

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var e in ChangeTracker.Entries<Entity>())
        {
            if (e.State == EntityState.Added) e.Entity.CreatedAt = now;
            if (e.State is EntityState.Added or EntityState.Modified) e.Entity.UpdatedAt = now;
        }
        return await base.SaveChangesAsync(ct);
    }

    private IQueryable<BorrowSlip> BorrowQuery() =>
        BorrowSlips.Include(x => x.Reader).Include(x => x.Details).ThenInclude(x => x.Book);
    private static void ConfigureRows(ModelBuilder b)
    {
        foreach (var t in b.Model.GetEntityTypes().Where(x => typeof(Entity).IsAssignableFrom(x.ClrType)))
            b.Entity(t.ClrType).Property(nameof(Entity.RowVersion)).IsRowVersion();
    }
}
