using System.Security.Claims;
using LibraryHub.Application;
using LibraryHub.Domain;
using LibraryHub.Infrastructure;
using LibraryHub.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryHub.Web.Controllers;

[Authorize(Policy = "InternalOnly")]
public sealed class AdminController(LibraryDbContext db, IPermissionService permissions) : Controller
{
    [HttpGet("/admin/books")]
    public async Task<IActionResult> Books(string? q, string? state, int page = 1)
    {
        if (!await HasAny(
                PermissionCodes.BookCreate,
                PermissionCodes.BookEdit,
                PermissionCodes.BookSuspend,
                PermissionCodes.BookRestore,
                PermissionCodes.BookCompleteMaintenance))
            return Forbid();
        page = Math.Max(1, page);
        const int pageSize = 20;
        var books = db.Books.AsNoTracking().Include(x => x.Category).Include(x => x.Author)
            .Include(x => x.Publisher).Include(x => x.Shelf).Where(x => !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(q))
            books = books.Where(x => x.Title.Contains(q) || x.Isbn.Contains(q));
        books = state?.Trim().ToLowerInvariant() switch
        {
            "active" => books.Where(x => x.Status == BookStatus.Available || x.Status == BookStatus.OutOfStock),
            "maintenance" => books.Where(x => x.Status == BookStatus.Maintenance),
            "discontinued" => books.Where(x => x.Status == BookStatus.Discontinued),
            "emergency" => books.Where(x => x.Status == BookStatus.Discontinued
                && x.SuspensionType == BookSuspensionType.EmergencyRecall),
            _ => books
        };
        var total = await books.CountAsync();
        var pageBooks = await books.OrderBy(x => x.Title)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        var pageBookIds = pageBooks.Select(x => x.Id).ToList();
        var protectedBookIds = pageBookIds.Count == 0
            ? new HashSet<int>()
            : (await db.Books.AsNoTracking()
                .Where(b => pageBookIds.Contains(b.Id) && (
                    db.BorrowDetails.Any(x => x.BookId == b.Id)
                    || db.Reservations.Any(x => x.BookId == b.Id)
                    || db.FineDetails.Any(x => x.BookId == b.Id)
                    || db.BookRatings.Any(x => x.BookId == b.Id)
                    || db.BookComments.Any(x => x.BookId == b.Id)
                    || db.BookFavorites.Any(x => x.BookId == b.Id)
                    || db.BookViewHistories.Any(x => x.BookId == b.Id)
                    || db.ReadingHistories.Any(x => x.BookId == b.Id)
                    || db.ReaderFeaturedBooks.Any(x => x.BookId == b.Id)))
                .Select(b => b.Id)
                .ToListAsync()).ToHashSet();
        var canEmergencyRecall = User.IsInRole("Administrator")
            || await HasAny(PermissionCodes.BookEmergencyRecall);
        return View(new AdminBooksViewModel(
            pageBooks,
            await db.Categories.AsNoTracking().Where(x => !x.IsDeleted).OrderBy(x => x.Name).ToListAsync(),
            await db.Authors.AsNoTracking().Where(x => !x.IsDeleted).OrderBy(x => x.Name).ToListAsync(),
            await db.Publishers.AsNoTracking().Where(x => !x.IsDeleted).OrderBy(x => x.Name).ToListAsync(),
            await db.Shelves.AsNoTracking().Where(x => !x.IsDeleted).OrderBy(x => x.Code).ToListAsync(),
            q, state, page, pageSize, total, protectedBookIds,
            canEmergencyRecall, User.IsInRole("Administrator")));
    }

    [HttpGet("/admin/catalog")]
    public async Task<IActionResult> Catalog()
    {
        if (!await HasAny(PermissionCodes.CatalogView, PermissionCodes.CatalogManage)) return Forbid();
        return View(new AdminCatalogViewModel(
            await db.Categories.AsNoTracking().Where(x => !x.IsDeleted).OrderBy(x => x.Name).ToListAsync(),
            await db.Authors.AsNoTracking().Where(x => !x.IsDeleted).OrderBy(x => x.Name).ToListAsync(),
            await db.Publishers.AsNoTracking().Where(x => !x.IsDeleted).OrderBy(x => x.Name).ToListAsync(),
            await db.Shelves.AsNoTracking().Where(x => !x.IsDeleted).OrderBy(x => x.Code).ToListAsync()));
    }

    [HttpGet("/admin/circulation")]
    public async Task<IActionResult> Circulation(
        string? q,
        BorrowStatus? borrowStatus,
        ReservationStatus? reservationStatus,
        FineStatus? fineStatus,
        int borrowPage = 1,
        int reservationPage = 1,
        int finePage = 1)
    {
        if (!await HasAny(
                PermissionCodes.BorrowApprove,
                PermissionCodes.BorrowConfirmPickup,
                PermissionCodes.BorrowApproveExtension,
                PermissionCodes.BorrowConfirmReturn,
                PermissionCodes.ReservationManage))
            return Forbid();

        const int borrowPageSize = 15;
        const int secondaryPageSize = 10;
        borrowPage = Math.Max(1, borrowPage);
        reservationPage = Math.Max(1, reservationPage);
        finePage = Math.Max(1, finePage);

        BorrowStatus[] activeBorrowStatuses =
        [
            BorrowStatus.PendingApproval,
            BorrowStatus.ApprovedAwaitingPickup,
            BorrowStatus.Borrowing,
            BorrowStatus.Overdue,
            BorrowStatus.PendingExtension
        ];
        ReservationStatus[] activeReservationStatuses =
        [
            ReservationStatus.Waiting,
            ReservationStatus.Approved,
            ReservationStatus.ReadyToBorrow
        ];

        var borrowQuery = db.BorrowSlips.AsNoTracking().Include(x => x.Reader)
            .Include(x => x.Details).ThenInclude(x => x.Book)
            .AsSplitQuery()
            .AsQueryable();
        borrowQuery = borrowStatus.HasValue
            ? borrowQuery.Where(x => x.Status == borrowStatus.Value)
            : borrowQuery.Where(x => activeBorrowStatuses.Contains(x.Status));
        if (!string.IsNullOrWhiteSpace(q))
            borrowQuery = borrowQuery.Where(x => x.Code.Contains(q)
                || x.Reader!.FullName.Contains(q)
                || x.Details.Any(d => d.Book != null && d.Book.Title.Contains(q)));
        var borrowTotal = await borrowQuery.CountAsync();
        borrowPage = Math.Min(borrowPage, Math.Max(1, (int)Math.Ceiling(borrowTotal / (double)borrowPageSize)));
        var borrows = await borrowQuery.OrderByDescending(x => x.RequestedAt)
            .Skip((borrowPage - 1) * borrowPageSize).Take(borrowPageSize).ToListAsync();

        var reservationQuery =
            from reservation in db.Reservations.AsNoTracking()
            join book in db.Books.AsNoTracking() on reservation.BookId equals book.Id
            join reader in db.Readers.AsNoTracking() on reservation.ReaderId equals reader.Id
            select new { Reservation = reservation, Book = book, Reader = reader };
        reservationQuery = reservationStatus.HasValue
            ? reservationQuery.Where(x => x.Reservation.Status == reservationStatus.Value)
            : reservationQuery.Where(x => activeReservationStatuses.Contains(x.Reservation.Status));
        if (!string.IsNullOrWhiteSpace(q))
            reservationQuery = reservationQuery.Where(x => x.Book.Title.Contains(q) || x.Reader.FullName.Contains(q));
        var reservationTotal = await reservationQuery.CountAsync();
        reservationPage = Math.Min(reservationPage,
            Math.Max(1, (int)Math.Ceiling(reservationTotal / (double)secondaryPageSize)));
        var reservationRows = await reservationQuery.OrderByDescending(x => x.Reservation.ReservedAt)
            .Skip((reservationPage - 1) * secondaryPageSize).Take(secondaryPageSize)
            .ToListAsync();
        var reservations = reservationRows
            .Select(x => new ReaderReservationRow(x.Reservation, x.Book))
            .ToList();

        var fineQuery = db.Fines.AsNoTracking().Include(x => x.Details).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            fineQuery = fineQuery.Where(x => x.Code.Contains(q)
                || (x.Reason != null && x.Reason.Contains(q)));
        if (fineStatus.HasValue)
            fineQuery = fineQuery.Where(x => x.Status == fineStatus);
        var fineTotal = await fineQuery.CountAsync();
        finePage = Math.Min(finePage, Math.Max(1, (int)Math.Ceiling(fineTotal / (double)secondaryPageSize)));
        var fines = await fineQuery.OrderByDescending(x => x.CreatedAt)
            .Skip((finePage - 1) * secondaryPageSize).Take(secondaryPageSize).ToListAsync();

        var readerIds = reservationRows.Select(x => x.Reservation.ReaderId)
            .Concat(fines.Select(x => x.ReaderId))
            .Distinct()
            .ToList();
        var readersById = readerIds.Count > 0
            ? await db.Readers.AsNoTracking().Where(x => readerIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id)
            : new Dictionary<int, Reader>();

        var walkInReaders = await db.Readers.AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.FullName)
            .Take(100)
            .ToListAsync();
        var walkInBooks = await db.Books.AsNoTracking()
            .Where(x => !x.IsDeleted && x.Status == BookStatus.Available && x.AvailableQuantity > 0)
            .OrderBy(x => x.Title)
            .Take(150)
            .ToListAsync();

        return View(new AdminCirculationViewModel(
            new PagedResult<BorrowSlip>(borrows, borrowPage, borrowPageSize, borrowTotal),
            new PagedResult<ReaderReservationRow>(
                reservations, reservationPage, secondaryPageSize, reservationTotal),
            new PagedResult<Fine>(fines, finePage, secondaryPageSize, fineTotal),
            q, borrowStatus, reservationStatus, fineStatus, readersById, walkInReaders, walkInBooks));
    }

    [HttpGet("/admin/circulation/print/{id:int}")]
    public async Task<IActionResult> PrintSlip(int id)
    {
        if (!await HasAny(PermissionCodes.BorrowApprove, PermissionCodes.BorrowConfirmPickup, PermissionCodes.BorrowConfirmReturn))
            return Forbid();
        var slip = await db.BorrowSlips.AsNoTracking()
            .Include(x => x.Reader)
            .Include(x => x.Details).ThenInclude(x => x.Book)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (slip is null) return NotFound();
        var fines = await db.Fines.AsNoTracking()
            .Where(x => x.BorrowSlipId == id)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
        return View(new AdminPrintSlipViewModel(slip, fines));
    }

    [HttpGet("/admin/moderation")]
    public async Task<IActionResult> Moderation()
    {
        if (!await HasAny(PermissionCodes.ReaderEdit, PermissionCodes.SupportClaim))
            return Forbid();
        var reports = await (
            from report in db.CommentReports.AsNoTracking()
            join reporter in db.Readers.AsNoTracking() on report.ReporterId equals reporter.Id
            join reported in db.Readers.AsNoTracking() on report.ReportedUserId equals reported.Id
            orderby report.Status == ReportStatus.Pending descending, report.CreatedAt descending
            select new { report, reporter, reported })
            .Take(100)
            .ToListAsync();

        var bcIds = reports.Where(x => x.report.BookCommentId.HasValue).Select(x => x.report.BookCommentId!.Value).Distinct().ToList();
        var pcIds = reports.Where(x => x.report.ProfileCommentId.HasValue).Select(x => x.report.ProfileCommentId!.Value).Distinct().ToList();
        var bookComments = bcIds.Count > 0
            ? await db.BookComments.AsNoTracking().Where(x => bcIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id)
            : new Dictionary<int, BookComment>();
        var profileComments = pcIds.Count > 0
            ? await db.ProfileComments.AsNoTracking().Where(x => pcIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id)
            : new Dictionary<int, ProfileComment>();

        var rows = reports.Select(x =>
        {
            string? content = null;
            int? bookId = null;
            if (x.report.BookCommentId.HasValue && bookComments.TryGetValue(x.report.BookCommentId.Value, out var bc))
            {
                content = bc.Content;
                bookId = bc.BookId;
            }
            else if (x.report.ProfileCommentId.HasValue && profileComments.TryGetValue(x.report.ProfileCommentId.Value, out var pc))
            {
                content = pc.Content;
            }
            return new ModerationReportRow(x.report, x.reporter, x.reported, content, bookId);
        }).ToList();

        return View(new AdminModerationViewModel(rows));
    }

    [HttpGet("/admin/exports/{kind}.csv")]
    public async Task<IActionResult> ExportCsv(string kind)
    {
        if (!await HasAny(PermissionCodes.ReportView, PermissionCodes.BookCreate, PermissionCodes.BorrowApprove, PermissionCodes.ReaderView))
            return Forbid();

        var sb = new System.Text.StringBuilder();
        static string Csv(object? val)
        {
            var s = Convert.ToString(val, System.Globalization.CultureInfo.InvariantCulture) ?? "";
            var trimmed = s.TrimStart();
            if (trimmed.Length > 0 && (trimmed[0] is '=' or '+' or '-' or '@' or '\t' or '\r'))
                s = "'" + s;
            if (s.Contains('"') || s.Contains(',') || s.Contains('\n') || s.Contains('\r'))
                s = "\"" + s.Replace("\"", "\"\"") + "\"";
            return s;
        }

        switch (kind.Trim().ToLowerInvariant())
        {
            case "books":
                sb.AppendLine("ISBN,Ten sach,Tac gia,Danh muc,Gia,Kha dung,Tong so,Trang thai");
                foreach (var b in await db.Books.AsNoTracking().Include(x => x.Author).Include(x => x.Category).Where(x => !x.IsDeleted).OrderBy(x => x.Title).ToListAsync())
                    sb.AppendLine(string.Join(",", Csv(b.Isbn), Csv(b.Title), Csv(b.Author?.Name), Csv(b.Category?.Name), Csv(b.Price), Csv(b.AvailableQuantity), Csv(b.TotalQuantity), Csv(b.Status.ToVietnamese())));
                break;
            case "readers":
                sb.AppendLine("Ma doc gia,Ho ten,Email,So dien thoai,Loai,Han the,Gioi han muon,Xac minh");
                foreach (var r in await db.Readers.AsNoTracking().Where(x => !x.IsDeleted).OrderBy(x => x.FullName).ToListAsync())
                    sb.AppendLine(string.Join(",", Csv(r.ReaderCode), Csv(r.FullName), Csv(r.Email), Csv(r.Phone), Csv(r.Type.ToVietnamese()), Csv(r.ExpiresOn.ToString("yyyy-MM-dd")), Csv(r.MaxBorrowBooks), Csv(r.IsVerified ? "Da xac minh" : "Chua xac minh")));
                break;
            case "borrows":
                sb.AppendLine("Ma phieu,Doc gia,So sach,Ngay tao,Han tra,Ngay tra,Trang thai");
                foreach (var s in await db.BorrowSlips.AsNoTracking().Include(x => x.Reader).OrderByDescending(x => x.RequestedAt).Take(1000).ToListAsync())
                    sb.AppendLine(string.Join(",", Csv(s.Code), Csv(s.Reader?.FullName), Csv(s.TotalBooks), Csv(s.RequestedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm")), Csv(s.DueAt?.ToLocalTime().ToString("yyyy-MM-dd")), Csv(s.ReturnedAt?.ToLocalTime().ToString("yyyy-MM-dd")), Csv(s.Status.ToVietnamese())));
                break;
            case "fines":
                sb.AppendLine("Ma phat,Loai,Tong tien,Da thu,Con no,Trang thai,Ly do");
                foreach (var f in await db.Fines.AsNoTracking().OrderByDescending(x => x.CreatedAt).Take(1000).ToListAsync())
                    sb.AppendLine(string.Join(",", Csv(f.Code), Csv(f.Type.ToVietnamese()), Csv(f.Amount), Csv(f.PaidAmount), Csv(Math.Max(0, f.Amount - f.PaidAmount)), Csv(f.Status.ToVietnamese()), Csv(f.Reason)));
                break;
            default:
                return NotFound();
        }

        var bom = System.Text.Encoding.UTF8.GetPreamble();
        var data = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
        var payload = new byte[bom.Length + data.Length];
        Buffer.BlockCopy(bom, 0, payload, 0, bom.Length);
        Buffer.BlockCopy(data, 0, payload, bom.Length, data.Length);
        return File(payload, "text/csv; charset=utf-8", $"libraryhub-{kind.ToLowerInvariant()}-{DateTime.Now:yyyyMMdd}.csv");
    }

    private async Task<bool> HasAny(params string[] codes)
    {
        if (User.IsInRole("Administrator")) return true;
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var accountId)) return false;
        foreach (var code in codes)
            if (await permissions.HasAsync(accountId, code, HttpContext.RequestAborted)) return true;
        return false;
    }
}
