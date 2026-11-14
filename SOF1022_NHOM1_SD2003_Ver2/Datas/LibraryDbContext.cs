using Microsoft.EntityFrameworkCore;
using SOF1022_NHOM1_SD2003_Ver2.Models;
using System.Reflection.PortableExecutable;
using static System.Net.WebRequestMethods;
namespace SOF1022_NHOM1_SD2003_Ver2.Datas
{
    public class LibraryDbContext : DbContext
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Fines>()
                        .HasOne(f => f.BorrowSlips)
                        .WithMany(b => b.Fines)
                        .HasForeignKey(f => f.BorrowSlipId);

            modelBuilder.Entity<Fines>()
                        .HasOne(f => f.Books)
                        .WithMany()
                        .HasForeignKey(f => f.BookId)
                        .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Fines>()
                        .HasIndex(f => f.BorrowSlipId);

            modelBuilder.Entity<Fines>()
                        .HasIndex(f => f.BorrowDetailId);

            // Configure FineDetails relationships
            modelBuilder.Entity<FineDetails>()
                        .HasOne(fd => fd.Fine)
                        .WithMany(f => f.FineDetails)
                        .HasForeignKey(fd => fd.FineId)
                        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FineDetails>()
                        .HasOne(fd => fd.Book)
                        .WithMany()
                        .HasForeignKey(fd => fd.BookId)
                        .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FineDetails>()
                        .HasOne(fd => fd.BorrowDetail)
                        .WithMany()
                        .HasForeignKey(fd => fd.BorrowDetailId)
                        .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FineDetails>()
                        .HasIndex(fd => fd.FineId);

            modelBuilder.Entity<FineDetails>()
                        .HasIndex(fd => fd.BookId);

            modelBuilder.Entity<Employee>()
                .Property(e => e.Salary)
                .HasPrecision(18, 2);

            // Configure cascade delete behavior to avoid multiple cascade paths
            modelBuilder.Entity<BookCommentLike>()
                .HasOne(bcl => bcl.Reader)
                .WithMany()
                .HasForeignKey(bcl => bcl.ReaderId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ReadingHistory>()
                .HasOne(rh => rh.Reader)
                .WithMany()
                .HasForeignKey(rh => rh.ReaderId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<BookViewHistory>()
                .HasOne(bvh => bvh.Reader)
                .WithMany()
                .HasForeignKey(bvh => bvh.ReaderId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<BookFavorite>()
                .HasOne(bf => bf.Reader)
                .WithMany()
                .HasForeignKey(bf => bf.ReaderId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<BookRating>()
                .HasOne(br => br.Reader)
                .WithMany()
                .HasForeignKey(br => br.ReaderId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<BookComment>()
                .HasOne(bc => bc.Reader)
                .WithMany()
                .HasForeignKey(bc => bc.ReaderId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure CommentReport relationships
            modelBuilder.Entity<CommentReport>()
                .HasOne(cr => cr.Comment)
                .WithMany()
                .HasForeignKey(cr => cr.CommentId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<CommentReport>()
                .HasOne(cr => cr.Reporter)
                .WithMany()
                .HasForeignKey(cr => cr.ReporterId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<CommentReport>()
                .HasOne(cr => cr.ReportedUser)
                .WithMany()
                .HasForeignKey(cr => cr.ReportedUserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure ProfileComment relationships
            modelBuilder.Entity<ProfileComment>()
                .HasOne(pc => pc.ProfileOwner)
                .WithMany()
                .HasForeignKey(pc => pc.ProfileOwnerId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ProfileComment>()
                .HasOne(pc => pc.Author)
                .WithMany()
                .HasForeignKey(pc => pc.AuthorId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure ProfileTagPin relationships
            modelBuilder.Entity<ProfileTagPin>()
                .HasOne(ptp => ptp.ProfileOwner)
                .WithMany()
                .HasForeignKey(ptp => ptp.ProfileOwnerId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ProfileTagPin>()
                .HasOne(ptp => ptp.Author)
                .WithMany()
                .HasForeignKey(ptp => ptp.AuthorId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure BorrowSlips relationship with Readers
            modelBuilder.Entity<BorrowSlips>()
                .HasOne(bs => bs.Readers)
                .WithMany()
                .HasForeignKey(bs => bs.ReaderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Employee relationships
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Position)
                .WithMany(p => p.Employees)
                .HasForeignKey(e => e.PositionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.EmployeeStatusOption)
                .WithMany(s => s.Employees)
                .HasForeignKey(e => e.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            // Global query filters for soft-delete
            modelBuilder.Entity<Books>()
                .HasQueryFilter(b => !b.IsDeleted);

            modelBuilder.Entity<Authors>()
                .HasQueryFilter(a => !a.IsDeleted);

            modelBuilder.Entity<Publishers>()
                .HasQueryFilter(p => !p.IsDeleted);

            // Configure Reservations relationships
            modelBuilder.Entity<Reservations>()
                .HasOne(r => r.Reader)
                .WithMany()
                .HasForeignKey(r => r.ReaderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reservations>()
                .HasOne(r => r.Book)
                .WithMany()
                .HasForeignKey(r => r.BookId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure SupportTickets relationships
            modelBuilder.Entity<SupportTickets>()
                .HasOne(st => st.Reader)
                .WithMany()
                .HasForeignKey(st => st.ReaderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SupportTickets>()
                .HasOne(st => st.AssignedStaff)
                .WithMany()
                .HasForeignKey(st => st.AssignedStaffId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SupportTickets>()
                .HasIndex(st => st.TicketCode)
                .IsUnique();

            // Configure SupportMessages relationships
            modelBuilder.Entity<SupportMessages>()
                .HasOne(sm => sm.SupportTicket)
                .WithMany(st => st.Messages)
                .HasForeignKey(sm => sm.SupportTicketId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Notification relationships
            // Dùng NoAction (giống các bảng con khác của Readers ở trên) để tránh lỗi
            // "may cause cycles or multiple cascade paths" của SQL Server.
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Recipient)
                .WithMany()
                .HasForeignKey(n => n.RecipientId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Notification>()
                .HasIndex(n => new { n.RecipientId, n.IsRead });

            modelBuilder.Entity<Notification>()
                .HasIndex(n => n.BroadcastGroupId);

            // Configure Role relationships
            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RolePermission>()
                .HasIndex(rp => new { rp.RoleId, rp.PermissionId })
                .IsUnique();

            // Configure UserPermission relationships
            modelBuilder.Entity<UserPermission>()
                .HasOne(up => up.Employee)
                .WithMany(e => e.UserPermissions)
                .HasForeignKey(up => up.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserPermission>()
                .HasOne(up => up.Permission)
                .WithMany(p => p.UserPermissions)
                .HasForeignKey(up => up.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserPermission>()
                .HasIndex(up => new { up.EmployeeId, up.PermissionId })
                .IsUnique();

            // Configure Employee-Role relationship
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Role)
                .WithMany(r => r.Employees)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        }



        public DbSet<Books> Books { get; set; }

        public DbSet<Categories> Categories { get; set; }

        public DbSet<Authors> Authors { get; set; }

        public DbSet<Publishers> Publishers { get; set; }

        public DbSet<Shelves> Shelves { get; set; }

        public DbSet<Readers> Readers { get; set; }

        public DbSet<BorrowSlips> BorrowSlips { get; set; }

        public DbSet<BorrowDetails> BorrowDetails { get; set; }

        public DbSet<Reservations> Reservations { get; set; }

        public DbSet<Fines> Fines { get; set; }

        public DbSet<FineDetails> FineDetails { get; set; }

        public DbSet<Employee> Employees { get; set; }

        public DbSet<BookFavorite> BookFavorites { get; set; }

        public DbSet<BookComment> BookComments { get; set; }

        public DbSet<BookRating> BookRatings { get; set; }

        public DbSet<BookViewHistory> BookViewHistories { get; set; }

        public DbSet<BookCommentLike> BookCommentLikes { get; set; }

        public DbSet<CommentReport> CommentReports { get; set; }

        public DbSet<ReadingHistory> ReadingHistories { get; set; }

        public DbSet<ProfileComment> ProfileComments { get; set; }

        public DbSet<ProfileTagPin> ProfileTagPins { get; set; }

        public DbSet<Position> Positions { get; set; }

        public DbSet<EmployeeStatusOption> EmployeeStatusOptions { get; set; }

        public DbSet<SystemSettings> SystemSettings { get; set; }

        public DbSet<SupportTickets> SupportTickets { get; set; }

        public DbSet<SupportMessages> SupportMessages { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        public DbSet<Role> Roles { get; set; }

        public DbSet<Permission> Permissions { get; set; }

        public DbSet<RolePermission> RolePermissions { get; set; }

        public DbSet<UserPermission> UserPermissions { get; set; }
    }
}
