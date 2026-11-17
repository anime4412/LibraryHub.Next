using System.Security.Claims;
using LibraryHub.Domain;
using LibraryHub.Application;
using LibraryHub.Infrastructure;
using LibraryHub.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryHub.Web.Controllers;

public sealed class HomeController(LibraryDbContext db, IPermissionService permissions) : Controller
{
    public async Task<IActionResult> Index(string? q)
    {
        var query = db.Books.AsNoTracking().Include(x => x.Author).Include(x => x.Category)
            .Where(x => !x.IsDeleted
                && (x.Status == BookStatus.Available || x.Status == BookStatus.OutOfStock));
        List<Book> books;
        if (!string.IsNullOrWhiteSpace(q))
        {
            var candidates = await db.Books.AsNoTracking()
                .Where(x => !x.IsDeleted
                    && (x.Status == BookStatus.Available || x.Status == BookStatus.OutOfStock))
                .Select(x => new
                {
                    x.Id,
                    x.Title,
                    x.Isbn,
                    x.Keywords,
                    x.Description,
                    x.ViewCount,
                    AuthorName = x.Author != null ? x.Author.Name : null,
                    CategoryName = x.Category != null ? x.Category.Name : null
                })
                .ToListAsync();
            var ranked = candidates
                .Select(b => new
                {
                    b.Id,
                    b.ViewCount,
                    Score = SmartSearchHelper.ScoreBook(new Book
                    {
                        Id = b.Id,
                        Title = b.Title,
                        Isbn = b.Isbn,
                        Keywords = b.Keywords,
                        Description = b.Description,
                        Language = "vi",
                        Author = b.AuthorName is null ? null : new Author { Name = b.AuthorName },
                        Category = b.CategoryName is null ? null : new Category { Name = b.CategoryName }
                    }, q)
                })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.ViewCount)
                .Take(8)
                .ToList();
            var topIds = ranked.Select(x => x.Id).ToList();
            if (topIds.Count == 0)
            {
                books = [];
            }
            else
            {
                var orderMap = ranked.Select((x, idx) => (x.Id, idx)).ToDictionary(x => x.Id, x => x.idx);
                books = (await query.Where(x => topIds.Contains(x.Id)).ToListAsync())
                    .OrderBy(x => orderMap[x.Id])
                    .ToList();
            }
        }
        else
        {
            books = await query.OrderByDescending(x => x.ViewCount).Take(8).ToListAsync();
        }
        var newBooks = await db.Books.AsNoTracking().Include(x => x.Author).Include(x => x.Category)
            .Where(x => !x.IsDeleted
                && (x.Status == BookStatus.Available || x.Status == BookStatus.OutOfStock))
            .OrderByDescending(x => x.CreatedAt).Take(4).ToListAsync();
        var ids = books.Concat(newBooks).Select(x => x.Id).Distinct().ToList();
        var ratings = ids.Count == 0
            ? new Dictionary<int, decimal>()
            : await db.BookRatings.AsNoTracking().Where(x => ids.Contains(x.BookId))
                .GroupBy(x => x.BookId)
                .ToDictionaryAsync(x => x.Key, x => x.Average(y => (decimal)y.Stars));
        var categories = await (
            from category in db.Categories.AsNoTracking()
            where !category.IsDeleted
            let bookCount = db.Books.Count(book => !book.IsDeleted
                && (book.Status == BookStatus.Available || book.Status == BookStatus.OutOfStock)
                && book.CategoryId == category.Id)
            orderby bookCount descending, category.Name
            select new CatalogCategoryRow(category.Id, category.Name, bookCount, category.ImagePath))
            .Take(12).ToListAsync();
        var settings = await db.SystemSettings.AsNoTracking().SingleOrDefaultAsync() ?? new SystemSettings();
        return View(new CatalogPage(
            q,
            books,
            newBooks,
            categories,
            await db.Books.CountAsync(x => !x.IsDeleted
                && (x.Status == BookStatus.Available || x.Status == BookStatus.OutOfStock)),
            await db.Readers.CountAsync(x => !x.IsDeleted),
            settings.BorrowPeriodDays,
            ratings));
    }

    [HttpGet("/books/{id:int}")]
    public async Task<IActionResult> Book(int id)
    {
        await db.Books.Where(x => x.Id == id && !x.IsDeleted)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.ViewCount, y => y.ViewCount + 1));
        var book = await db.Books.AsNoTracking().Include(x => x.Author).Include(x => x.Category)
            .Include(x => x.Publisher).Include(x => x.Shelf).SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (book is null) return NotFound();
        var ratings = db.BookRatings.AsNoTracking().Where(x => x.BookId == id);
        var comments = await (
            from comment in db.BookComments.AsNoTracking()
            join reader in db.Readers.AsNoTracking() on comment.ReaderId equals reader.Id
            where comment.BookId == id && !comment.IsDeleted
            orderby comment.CreatedAt descending
            select new BookCommentRow(
                comment,
                reader,
                db.BookCommentLikes.Count(x => x.BookCommentId == comment.Id)))
            .Take(30).ToListAsync();
        var readerId = User.Identity?.IsAuthenticated == true
            && Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var accountId)
            ? await db.Readers.AsNoTracking().Where(x => x.UserAccountId == accountId && !x.IsDeleted)
                .Select(x => (int?)x.Id).SingleOrDefaultAsync()
            : null;
        return View(new BookPageViewModel(
            book,
            await ratings.AverageAsync(x => (decimal?)x.Stars) ?? 0,
            await ratings.CountAsync(),
            comments,
            readerId.HasValue && await db.BookFavorites.AnyAsync(x => x.BookId == id && x.ReaderId == readerId.Value),
            readerId.HasValue
                ? await db.BookRatings.Where(x => x.BookId == id && x.ReaderId == readerId.Value)
                    .Select(x => (int?)x.Stars).SingleOrDefaultAsync()
                : null,
            readerId.HasValue
                ? await db.ReadingHistories.AsNoTracking()
                    .SingleOrDefaultAsync(x => x.BookId == id && x.ReaderId == readerId.Value)
                : null));
    }

    [Authorize]
    [HttpGet("/me/{section?}")]
    public async Task<IActionResult> ReaderSpace(string section = "overview")
    {
        section = section.Trim().ToLowerInvariant();
        var sections = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "overview", "cart", "borrows", "reservations", "fines",
            "history", "favorites", "notifications", "profile"
        };
        if (!sections.Contains(section)) return NotFound();
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var accountId))
            return Challenge();
        var reader = await db.Readers.AsNoTracking()
            .SingleOrDefaultAsync(x => x.UserAccountId == accountId && !x.IsDeleted);
        if (reader is null) return Forbid();
        var borrows = await db.BorrowSlips.AsNoTracking()
            .Include(x => x.Details).ThenInclude(x => x.Book)
            .Where(x => x.ReaderId == reader.Id && (
                x.Status == BorrowStatus.PendingApproval
                || x.Status == BorrowStatus.ApprovedAwaitingPickup
                || x.Status == BorrowStatus.Borrowing
                || x.Status == BorrowStatus.Overdue
                || x.Status == BorrowStatus.PendingExtension))
            .OrderByDescending(x => x.RequestedAt).Take(50).ToListAsync();
        var reservations = await (
            from reservation in db.Reservations.AsNoTracking()
            join book in db.Books.AsNoTracking() on reservation.BookId equals book.Id
            where reservation.ReaderId == reader.Id
                && (reservation.Status == ReservationStatus.Waiting
                    || reservation.Status == ReservationStatus.Approved
                    || reservation.Status == ReservationStatus.ReadyToBorrow)
            orderby reservation.ReservedAt descending
            select new ReaderReservationRow(reservation, book))
            .Take(50).ToListAsync();
        var fines = await db.Fines.AsNoTracking().Include(x => x.Details)
            .Where(x => x.ReaderId == reader.Id)
            .OrderByDescending(x => x.CreatedAt).Take(50).ToListAsync();
        var reading = await (
            from history in db.ReadingHistories.AsNoTracking()
            join book in db.Books.AsNoTracking() on history.BookId equals book.Id
            where history.ReaderId == reader.Id
            orderby history.LastReadAt descending
            select new ReaderHistoryRow(history, book))
            .Take(30).ToListAsync();
        var notifications = await db.Notifications.AsNoTracking()
            .Where(x => x.RecipientAccountId == accountId)
            .OrderByDescending(x => x.CreatedAt).Take(50).ToListAsync();
        var favorites = await (
            from favorite in db.BookFavorites.AsNoTracking()
            join book in db.Books.AsNoTracking() on favorite.BookId equals book.Id
            where favorite.ReaderId == reader.Id && !book.IsDeleted
            orderby favorite.CreatedAt descending
            select book).Take(30).ToListAsync();
        var featuredBookIds = section == "profile"
            ? await db.ReaderFeaturedBooks.AsNoTracking()
                .Where(x => x.ReaderId == reader.Id)
                .OrderBy(x => x.DisplayOrder)
                .Select(x => x.BookId)
                .ToListAsync()
            : null;
        var showcaseBooks = section == "profile"
            ? await db.Books.AsNoTracking()
                .Where(x => !x.IsDeleted && (x.Status == BookStatus.Available || x.Status == BookStatus.OutOfStock))
                .OrderBy(x => x.Title)
                .Take(100)
                .ToListAsync()
            : null;
        return View(new ReaderSpaceViewModel(
            reader, borrows, reservations, fines, reading, notifications, favorites,
            notifications.Count(x => !x.IsRead),
            section, featuredBookIds, showcaseBooks));
    }

    [HttpGet("/readers/{id:int}")]
    public async Task<IActionResult> ReaderProfile(int id)
    {
        var reader = await db.Readers.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (reader is null) return NotFound();
        var featuredIds = await db.ReaderFeaturedBooks.AsNoTracking()
            .Where(x => x.ReaderId == id)
            .OrderBy(x => x.DisplayOrder)
            .Select(x => x.BookId)
            .ToListAsync();
        var featuredMap = featuredIds.Count > 0
            ? await db.Books.AsNoTracking().Include(x => x.Author)
                .Where(x => featuredIds.Contains(x.Id) && !x.IsDeleted)
                .ToDictionaryAsync(x => x.Id)
            : new Dictionary<int, Book>();
        var featuredBooks = featuredIds.Where(featuredMap.ContainsKey).Select(i => featuredMap[i]).ToList();
        var comments = await (
            from comment in db.ProfileComments.AsNoTracking()
            join author in db.Readers.AsNoTracking() on comment.AuthorId equals author.Id
            where comment.ProfileOwnerId == id && !comment.IsDeleted && !author.IsDeleted
            orderby comment.CreatedAt descending
            select new ProfileCommentRow(comment, author))
            .Take(40)
            .ToListAsync();
        var canPost = User.Identity?.IsAuthenticated == true
            && !User.IsInRole("Staff")
            && !User.IsInRole("Administrator");
        return View(new ReaderPublicProfileViewModel(reader, featuredBooks, comments, canPost));
    }

    [Authorize]
    [HttpGet("/me/{section}/{id:int}")]
    public IActionResult ReaderSpaceItem(string section, int id)
    {
        section = section.Trim().ToLowerInvariant();
        var target = section switch
        {
            "borrows" => $"/me/borrows#borrow-{id}",
            "reservations" => $"/me/reservations#reservation-{id}",
            "fines" => $"/me/fines#fine-{id}",
            "notifications" => $"/me/notifications#notification-{id}",
            _ => null
        };
        return target is null ? NotFound() : LocalRedirect(target);
    }

    [Authorize(Policy = "InternalOnly")]
    [HttpGet("/admin")]
    public async Task<IActionResult> Admin()
    {
        if (!User.IsInRole("Administrator")
            && (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var accountId)
                || !await permissions.HasAsync(accountId, PermissionCodes.ReportView, HttpContext.RequestAborted)))
            return Forbid();
        var vm = new AdminDashboard(
            await db.Books.CountAsync(x => !x.IsDeleted),
            await db.Readers.CountAsync(x => !x.IsDeleted),
            await db.BorrowSlips.CountAsync(x => x.Status == BorrowStatus.Borrowing
                || x.Status == BorrowStatus.Overdue
                || x.Status == BorrowStatus.PendingExtension),
            await db.Fines.Where(x => x.Status != FineStatus.Paid
                    && x.Status != FineStatus.Waived
                    && x.Status != FineStatus.Cancelled)
                .SumAsync(x => (decimal?)(x.Amount - x.PaidAmount)) ?? 0,
            await db.BorrowSlips.CountAsync(x => x.Status == BorrowStatus.PendingApproval),
            await db.BorrowSlips.CountAsync(x => x.Status == BorrowStatus.PendingExtension),
            await db.SupportTickets.CountAsync(x => x.Status != SupportTicketStatus.Closed),
            await db.Reservations.CountAsync(x => x.Status == ReservationStatus.Waiting
                || x.Status == ReservationStatus.Approved || x.Status == ReservationStatus.ReadyToBorrow),
            await db.BorrowSlips.AsNoTracking().Include(x => x.Reader)
                .Where(x => x.Status == BorrowStatus.PendingApproval
                    || x.Status == BorrowStatus.ApprovedAwaitingPickup
                    || x.Status == BorrowStatus.Borrowing
                    || x.Status == BorrowStatus.Overdue
                    || x.Status == BorrowStatus.PendingExtension)
                .OrderByDescending(x => x.RequestedAt).Take(8).ToListAsync(),
            await (from detail in db.BorrowDetails
                join book in db.Books on detail.BookId equals book.Id
                group detail by new { book.Id, book.Title } into grouped
                orderby grouped.Sum(x => x.Quantity) descending
                select new BookPopularityRow(grouped.Key.Id, grouped.Key.Title, grouped.Sum(x => x.Quantity)))
                .Take(5).ToListAsync(),
            await (from slip in db.BorrowSlips
                join reader in db.Readers on slip.ReaderId equals reader.Id
                group slip by new { reader.Id, reader.FullName } into grouped
                orderby grouped.Count() descending
                select new ReaderActivityRow(grouped.Key.Id, grouped.Key.FullName, grouped.Count()))
                .Take(5).ToListAsync());
        return View(vm);
    }
}
