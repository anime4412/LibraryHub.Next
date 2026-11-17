using System.Security.Claims;
using LibraryHub.Application;
using LibraryHub.Domain;
using LibraryHub.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace LibraryHub.Web;

public static class Api
{
    public static IEndpointRouteBuilder MapLibraryApi(this IEndpointRouteBuilder endpoints)
    {
        var api = endpoints.MapGroup("/api/v1");
        api.AddEndpointFilter(async (context, next) =>
        {
            var request = context.HttpContext.Request;
            if (HttpMethods.IsGet(request.Method)
                || HttpMethods.IsHead(request.Method)
                || HttpMethods.IsOptions(request.Method))
                return await next(context);
            if (!string.Equals(
                    request.Headers["X-LibraryHub-Request"].ToString(),
                    "XMLHttpRequest",
                    StringComparison.Ordinal))
                return Results.Problem(
                    statusCode: StatusCodes.Status403Forbidden,
                    title: "Yêu cầu không hợp lệ",
                    detail: "Không thể xác minh nguồn gửi yêu cầu.");
            return await next(context);
        });
        MapCatalog(api);
        MapCirculation(api);
        MapCommunity(api);
        MapSupport(api);
        MapAdministration(api);
        return endpoints;
    }

    private static void MapCatalog(RouteGroupBuilder api)
    {
        MapSimpleCrud(api, "/categories", db => db.Categories,
            (old, value) =>
            {
                old.Name = value.Name;
                old.Description = value.Description;
                old.ImagePath = NormalizeOptional(value.ImagePath);
            },
            (db, id, ct) => db.Books.AnyAsync(b => !b.IsDeleted && b.CategoryId == id, ct));
        MapSimpleCrud(api, "/authors", db => db.Authors,
            (old, value) => { old.Name = value.Name; old.DateOfBirth = value.DateOfBirth; old.Nationality = value.Nationality; old.Biography = value.Biography; },
            (db, id, ct) => db.Books.AnyAsync(b => !b.IsDeleted && b.AuthorId == id, ct));
        MapSimpleCrud(api, "/publishers", db => db.Publishers,
            (old, value) => { old.Name = value.Name; old.Address = value.Address; old.Phone = value.Phone; old.Email = value.Email; old.Website = value.Website; },
            (db, id, ct) => db.Books.AnyAsync(b => !b.IsDeleted && b.PublisherId == id, ct));
        MapSimpleCrud(api, "/shelves", db => db.Shelves,
            (old, value) => { old.Code = value.Code; old.LocationCode = value.LocationCode; old.Name = value.Name; old.Area = value.Area; old.Description = value.Description; old.Capacity = value.Capacity; },
            (db, id, ct) => db.Books.AnyAsync(b => !b.IsDeleted && b.ShelfId == id, ct));
        MapSimpleCrud(api, "/positions", db => db.Positions,
            (old, value) => { old.Code = value.Code; old.Name = value.Name; old.Description = value.Description; old.SalaryAmount = value.SalaryAmount; old.AllowSalaryOverride = value.AllowSalaryOverride; },
            (db, id, ct) => db.Employees.AnyAsync(e => !e.IsDeleted && e.PositionId == id, ct));
        MapSimpleCrud(api, "/employee-status-options", db => db.EmployeeStatusOptions,
            (old, value) => { old.Code = value.Code; old.Name = value.Name; old.Description = value.Description; old.DisplayOrder = value.DisplayOrder; old.Status = value.Status; },
            (db, id, ct) => db.Employees.AnyAsync(e => !e.IsDeleted && e.StatusOptionId == id, ct));

        var books = api.MapGroup("/books");
        books.MapGet("/", async (LibraryDbContext db, string? q, int page = 1, int pageSize = 24) =>
        {
            page = Math.Max(1, page); pageSize = Math.Clamp(pageSize, 1, 100);
            var query = db.Books.AsNoTracking().Include(x => x.Author).Include(x => x.Category)
                .Where(x => !x.IsDeleted
                    && (x.Status == BookStatus.Available || x.Status == BookStatus.OutOfStock));
            if (!string.IsNullOrWhiteSpace(q)) query = query.Where(x => x.Title.Contains(q) || x.Isbn.Contains(q));
            var total = await query.CountAsync();
            var items = await query.OrderBy(x => x.Title).Skip((page - 1) * pageSize).Take(pageSize)
                .Select(x => new { x.Id, x.Isbn, x.Title, Author = x.Author!.Name, Category = x.Category!.Name,
                    x.CoverImagePath, x.AvailableQuantity, x.Status, x.ViewCount }).ToListAsync();
            return Results.Ok(new { items, page, pageSize, total });
        });
        books.MapGet("/{id:int}", async (int id, LibraryDbContext db) =>
        {
            var book = await db.Books.AsNoTracking().Include(x => x.Author).Include(x => x.Category)
                .Include(x => x.Publisher).Include(x => x.Shelf)
                .SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (book is null) return Results.NotFound();
            if (book.Status is not (BookStatus.Available or BookStatus.OutOfStock))
                book.AvailableQuantity = 0;
            return Results.Ok(book);
        });

        api.MapGet("/search/smart", async (LibraryDbContext db, string? q, CancellationToken ct) =>
        {
            var raw = q?.Trim() ?? "";
            var activeCategories = await db.Categories.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .Select(c => new { c.Id, c.Name })
                .ToListAsync(ct);

            var countsList = await db.Books.AsNoTracking()
                .Where(x => !x.IsDeleted && (x.Status == BookStatus.Available || x.Status == BookStatus.OutOfStock))
                .GroupBy(x => x.CategoryId)
                .Select(g => new { CategoryId = g.Key, Count = g.Count() })
                .ToListAsync(ct);
            var countByCategory = countsList.ToDictionary(x => x.CategoryId, x => x.Count);

            if (string.IsNullOrWhiteSpace(raw))
            {
                var topBooks = await db.Books.AsNoTracking()
                    .Where(x => !x.IsDeleted && (x.Status == BookStatus.Available || x.Status == BookStatus.OutOfStock))
                    .OrderByDescending(x => x.ViewCount)
                    .Take(6)
                    .Select(x => new
                    {
                        id = x.Id,
                        title = x.Title,
                        author = x.Author != null ? x.Author.Name : "LibraryHub",
                        category = x.Category != null ? x.Category.Name : "Tổng hợp",
                        year = x.PublicationYear,
                        coverImagePath = x.CoverImagePath ?? "",
                        availableQuantity = x.AvailableQuantity
                    })
                    .ToListAsync(ct);

                var topCategories = activeCategories
                    .OrderByDescending(c => countByCategory.GetValueOrDefault(c.Id))
                    .Take(5)
                    .ToList();

                return Results.Ok(new
                {
                    query = raw,
                    books = topBooks,
                    categories = topCategories.Select(c => new
                    {
                        id = c.Id,
                        name = c.Name,
                        bookCount = countByCategory.GetValueOrDefault(c.Id)
                    }),
                    suggestions = topCategories.Take(4).Select(c => c.Name).ToList()
                });
            }

            var searchableBooks = await db.Books.AsNoTracking()
                .Where(x => !x.IsDeleted && (x.Status == BookStatus.Available || x.Status == BookStatus.OutOfStock))
                .Select(x => new
                {
                    x.Id,
                    x.Title,
                    x.Isbn,
                    x.Keywords,
                    x.Description,
                    x.PublicationYear,
                    x.CoverImagePath,
                    x.AvailableQuantity,
                    x.ViewCount,
                    AuthorName = x.Author != null ? x.Author.Name : null,
                    CategoryName = x.Category != null ? x.Category.Name : null
                })
                .ToListAsync(ct);

            var matchedBooks = searchableBooks
                .Select(b => new
                {
                    Item = b,
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
                    }, raw)
                })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.Item.ViewCount)
                .Take(8)
                .Select(x => x.Item)
                .ToList();

            var matchedCategories = activeCategories
                .Select(c => new { Category = c, Score = SmartSearchHelper.ScoreCategory(new Category { Id = c.Id, Name = c.Name }, raw) })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .Take(4)
                .Select(x => x.Category)
                .ToList();

            var suggestions = activeCategories
                .OrderByDescending(c => countByCategory.GetValueOrDefault(c.Id))
                .Select(c => c.Name)
                .Take(4)
                .ToList();

            return Results.Ok(new
            {
                query = raw,
                books = matchedBooks.Select(x => new
                {
                    id = x.Id,
                    title = x.Title,
                    author = x.AuthorName ?? "LibraryHub",
                    category = x.CategoryName ?? "Tổng hợp",
                    year = x.PublicationYear,
                    coverImagePath = x.CoverImagePath ?? "",
                    availableQuantity = x.AvailableQuantity
                }),
                categories = matchedCategories.Select(c => new
                {
                    id = c.Id,
                    name = c.Name,
                    bookCount = countByCategory.GetValueOrDefault(c.Id)
                }),
                suggestions
            });
        });
        WithPermission(books.MapPost("/", async ([FromBody] Book input, LibraryDbContext db, CancellationToken ct) =>
        {
            if (input.TotalQuantity < 0 || input.AvailableQuantity < 0 || input.AvailableQuantity > input.TotalQuantity)
                throw new DomainRuleException("book.quantity.invalid", "Số lượng sách không hợp lệ.");
            var refError = await ValidateBookReferencesAsync(db, input.CategoryId, input.AuthorId, input.PublisherId, input.ShelfId, ct);
            if (refError is not null) return refError;
            input.Id = 0;
            input.Status = input.AvailableQuantity > 0 ? BookStatus.Available : BookStatus.OutOfStock;
            input.IsDeleted = false;
            input.DeletedAt = null;
            db.Books.Add(input);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/v1/books/{input.Id}", input);
        }).RequireAuthorization("InternalOnly"), PermissionCodes.BookCreate);
        WithPermission(books.MapPut("/{id:int}", async (int id, [FromBody] Book input, LibraryDbContext db, CancellationToken ct) =>
        {
            var item = await db.Books.SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
            if (item is null) return Results.NotFound();
            if (input.TotalQuantity < 0 || input.AvailableQuantity < 0 || input.AvailableQuantity > input.TotalQuantity)
                throw new DomainRuleException("book.quantity.invalid", "Số lượng sách không hợp lệ.");
            var refError = await ValidateBookReferencesAsync(db, input.CategoryId, input.AuthorId, input.PublisherId, input.ShelfId, ct);
            if (refError is not null) return refError;
            if (input.RowVersion is { Length: > 0 })
                db.Entry(item).Property(x => x.RowVersion).OriginalValue = input.RowVersion;
            item.Isbn = input.Isbn; item.Title = input.Title; item.Description = input.Description;
            item.Edition = input.Edition; item.Keywords = input.Keywords; item.PublicationYear = input.PublicationYear;
            item.PageCount = input.PageCount; item.Language = input.Language; item.Price = input.Price;
            item.TotalQuantity = input.TotalQuantity; item.AvailableQuantity = input.AvailableQuantity;
            item.MinimumAge = input.MinimumAge; item.CategoryId = input.CategoryId; item.AuthorId = input.AuthorId;
            item.PublisherId = input.PublisherId; item.ShelfId = input.ShelfId;
            if (item.Status is BookStatus.Available or BookStatus.OutOfStock)
                item.Status = item.AvailableQuantity > 0 ? BookStatus.Available : BookStatus.OutOfStock;
            item.CoverImagePath = input.CoverImagePath;
            await db.SaveChangesAsync(ct); return Results.NoContent();
        }).RequireAuthorization("InternalOnly"), PermissionCodes.BookEdit);

        WithPermission(books.MapGet("/{id:int}/lifecycle-impact", async (
            int id, IBookLifecycleService service, CancellationToken ct) =>
                Results.Ok(await service.GetImpactAsync(id, ct)))
            .RequireAuthorization("InternalOnly"), PermissionCodes.BookSuspend);
        WithPermission(books.MapPost("/{id:int}/suspend", async (
            int id,
            BookSuspensionRequest request,
            HttpContext http,
            IBookLifecycleService service,
            IPermissionService permissionService,
            CancellationToken ct) =>
        {
            var actor = AccountId(http.User);
            if (request.Type == BookSuspensionType.EmergencyRecall
                && !http.User.IsInRole("Administrator")
                && !await permissionService.HasAsync(actor, PermissionCodes.BookEmergencyRecall, ct))
                return Results.Forbid();
            return Results.Ok(await service.SuspendAsync(new(
                id,
                actor,
                request.Type,
                request.Reason,
                request.RecallDeadline,
                RequireIdempotency(http)), ct));
        }).RequireAuthorization("InternalOnly"), PermissionCodes.BookSuspend);
        WithPermission(books.MapPost("/{id:int}/restore", async (
            int id,
            BookRestoreRequest request,
            HttpContext http,
            IBookLifecycleService service,
            CancellationToken ct) =>
                Results.Ok(await service.RestoreAsync(new(
                    id,
                    AccountId(http.User),
                    request.Reason,
                    RequireIdempotency(http)), ct)))
            .RequireAuthorization("InternalOnly"), PermissionCodes.BookRestore);
        WithPermission(books.MapPost("/{id:int}/complete-maintenance", async (
            int id,
            CompleteMaintenanceRequest request,
            HttpContext http,
            IBookLifecycleService service,
            CancellationToken ct) =>
                Results.Ok(await service.CompleteMaintenanceAsync(new(
                    id,
                    AccountId(http.User),
                    request.ActualTotalQuantity,
                    request.AvailableQuantityAfterInspection,
                    RequireIdempotency(http)), ct)))
            .RequireAuthorization("InternalOnly"), PermissionCodes.BookCompleteMaintenance);
        books.MapDelete("/{id:int}/permanent", async (
            int id, IBookLifecycleService service, CancellationToken ct) =>
        {
            await service.PermanentlyDeleteAsync(id, ct);
            return Results.NoContent();
        }).RequireAuthorization("AdministratorOnly");

        WithAnyPermission(api.MapPost("/uploads/book-cover", (
            IFormFile file,
            IWebHostEnvironment environment,
            CancellationToken ct) =>
                SaveUploadedImageAsync(file, environment, "book-covers", "book", "Ảnh bìa", ct))
            .RequireAuthorization("InternalOnly").DisableAntiforgery(),
            PermissionCodes.BookCreate, PermissionCodes.BookEdit);

        WithPermission(api.MapPost("/uploads/category-image", (
            IFormFile file,
            IWebHostEnvironment environment,
            CancellationToken ct) =>
                SaveUploadedImageAsync(file, environment, "categories", "cat", "Ảnh danh mục", ct))
            .RequireAuthorization("InternalOnly").DisableAntiforgery(),
            PermissionCodes.CatalogManage);
    }

    private static void MapCirculation(RouteGroupBuilder api)
    {
        var borrow = api.MapGroup("/borrows").RequireAuthorization();
        borrow.MapPost("/", async (CreateBorrowRequest request, HttpContext http, LibraryDbContext db,
            IBorrowService service, CancellationToken ct) =>
        {
            var readerId = await ReaderIdAsync(http.User, db, ct);
            if (!readerId.HasValue) return Results.Forbid();
            return Results.Ok(await service.CreateAsync(new(readerId.Value, AccountId(http.User), request.Lines,
                request.Note, RequireIdempotency(http)), ct));
        });
        borrow.MapPost("/{id:int}/cancel", async (int id, HttpContext http, LibraryDbContext db,
            IBorrowService service, CancellationToken ct) =>
        {
            var readerId = await ReaderIdAsync(http.User, db, ct);
            if (!readerId.HasValue) return Results.Forbid();
            await service.CancelByReaderAsync(id, readerId.Value, ct);
            return Results.NoContent();
        });
        WithPermission(borrow.MapPost("/walk-in", async (
            WalkInBorrowRequest request,
            HttpContext http,
            LibraryDbContext db,
            IBorrowService service,
            CancellationToken ct) =>
        {
            var reader = await db.Readers.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == request.ReaderId && !x.IsDeleted, ct);
            if (reader is null) return Results.NotFound();
            var result = await service.CreateWalkInAsync(new(
                reader.Id,
                AccountId(http.User),
                request.Lines,
                string.IsNullOrWhiteSpace(request.Note) ? "Lập phiếu tại quầy" : request.Note.Trim(),
                request.DeliverImmediately,
                RequireIdempotency(http)), ct);
            return Results.Ok(new
            {
                id = result.Id,
                code = result.Code,
                status = result.Status
            });
        }).RequireAuthorization("InternalOnly"), PermissionCodes.BorrowConfirmPickup);
        WithPermission(borrow.MapPost("/{id:int}/approve", async (int id, IBorrowService service, CancellationToken ct) =>
            { await service.ApproveAsync(id, ct); return Results.NoContent(); }).RequireAuthorization("InternalOnly"),
            PermissionCodes.BorrowApprove);
        WithPermission(borrow.MapPost("/{id:int}/reject", async (int id, DecisionRequest request, IBorrowService service, CancellationToken ct) =>
            { await service.RejectAsync(id, request.Reason ?? "", ct); return Results.NoContent(); }).RequireAuthorization("InternalOnly"),
            PermissionCodes.BorrowApprove);
        WithPermission(borrow.MapPost("/{id:int}/pickup", async (int id, IBorrowService service, CancellationToken ct) =>
            { await service.ConfirmPickupAsync(id, ct); return Results.NoContent(); }).RequireAuthorization("InternalOnly"),
            PermissionCodes.BorrowConfirmPickup);
        WithPermission(borrow.MapPost("/{id:int}/deliver", async (int id, IBorrowService service, CancellationToken ct) =>
            { await service.ConfirmPickupAsync(id, ct); return Results.NoContent(); }).RequireAuthorization("InternalOnly"),
            PermissionCodes.BorrowConfirmPickup);
        borrow.MapPost("/details/{id:int}/extension", async (int id, ExtensionRequest request, HttpContext http,
            LibraryDbContext db, IBorrowService service, CancellationToken ct) =>
        {
            var readerId = await ReaderIdAsync(http.User, db, ct);
            if (!readerId.HasValue) return Results.Forbid();
            var ownsDetail = await (
                from detail in db.BorrowDetails
                join slip in db.BorrowSlips on detail.BorrowSlipId equals slip.Id
                where detail.Id == id && slip.ReaderId == readerId.Value
                select detail.Id).AnyAsync(ct);
            if (!ownsDetail) return Results.Forbid();
            await service.RequestExtensionAsync(id, request.Days, request.Reason, ct);
            return Results.Accepted();
        });
        WithPermission(borrow.MapPost("/details/{id:int}/extension/decision", async (int id, ExtensionDecision request, IBorrowService service, CancellationToken ct) =>
            { await service.DecideExtensionAsync(id, request.Approve, ct); return Results.NoContent(); }).RequireAuthorization("InternalOnly"),
            PermissionCodes.BorrowApproveExtension);
        WithPermission(borrow.MapPost("/{id:int}/returns", async (int id, ReturnRequest request, HttpContext http, IBorrowService service, CancellationToken ct) =>
            Results.Ok(await service.ReturnAsync(new(id, AccountId(http.User), request.Lines, RequireIdempotency(http)), ct)))
            .RequireAuthorization("InternalOnly"), PermissionCodes.BorrowConfirmReturn);

        var reservations = api.MapGroup("/reservations").RequireAuthorization();
        reservations.MapPost("/", async (ReservationRequest r, HttpContext http, LibraryDbContext db, CancellationToken ct) =>
        {
            var readerId = await ReaderIdAsync(http.User, db, ct);
            if (!readerId.HasValue) return Results.Forbid();
            return await db.TransactionAsync<IResult>(async txCt =>
            {
                var lockedReader = await db.FindReaderForUpdateAsync(readerId.Value, txCt);
                if (lockedReader is null || lockedReader.IsDeleted) return Results.Forbid();
                var exists = await db.Reservations.AnyAsync(x => x.ReaderId == readerId.Value && x.BookId == r.BookId
                    && x.Status != ReservationStatus.Cancelled && x.Status != ReservationStatus.Completed
                    && x.Status != ReservationStatus.Expired && x.Status != ReservationStatus.Rejected, txCt);
                if (exists) throw new DomainRuleException("reservation.active", "Đã có đặt trước đang hoạt động.");
                if (!await db.Books.AnyAsync(x => x.Id == r.BookId && !x.IsDeleted
                    && (x.Status == BookStatus.Available || x.Status == BookStatus.OutOfStock), txCt))
                    throw new DomainRuleException("book.unavailable", "Sách không còn phục vụ đặt trước.");
                var item = new Reservation { ReaderId = readerId.Value, BookId = r.BookId, ReservedAt = DateTimeOffset.UtcNow, Note = r.Note };
                db.Add(item);
                await db.SaveChangesAsync(txCt);
                return Results.Created($"/api/v1/reservations/{item.Id}", item);
            }, ct);
        });
        reservations.MapPost("/{id:int}/cancel", async (int id, HttpContext http, LibraryDbContext db,
            IReservationService service, CancellationToken ct) =>
        {
            var readerId = await ReaderIdAsync(http.User, db, ct);
            if (!readerId.HasValue) return Results.Forbid();
            var reservation = await db.Reservations.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
            if (reservation is null) return Results.NotFound();
            if (reservation.ReaderId != readerId.Value) return Results.Forbid();
            await service.UpdateStatusAsync(id, ReservationStatus.Cancelled, ct);
            return Results.NoContent();
        });
        WithPermission(reservations.MapPatch("/{id:int}/status", async (int id, StatusRequest<ReservationStatus> r,
            IReservationService service, CancellationToken ct) =>
        {
            await service.UpdateStatusAsync(id, r.Status, ct);
            return Results.NoContent();
        }).RequireAuthorization("InternalOnly"), PermissionCodes.ReservationManage);
        WithPermission(reservations.MapPost("/{id:int}/deliver", async (int id, HttpContext http,
            IReservationService service, CancellationToken ct) =>
                Results.Ok(await service.DeliverAsync(id, AccountId(http.User), ct)))
            .RequireAuthorization("InternalOnly"), PermissionCodes.ReservationManage);

        api.MapPost("/fines/{id:int}/pay", async (int id, PaymentRequest r, HttpContext http,
            LibraryDbContext db, INotificationPublisher notifications, CancellationToken ct) =>
            await db.TransactionAsync<IResult>(async transactionCt =>
            {
                var fine = await db.Fines
                    .FromSqlInterpolated($"SELECT * FROM [Fines] WITH (UPDLOCK, ROWLOCK) WHERE [Id] = {id}")
                    .SingleOrDefaultAsync(transactionCt);
                if (fine is null) return Results.NotFound();
                var reader = await db.Readers.AsNoTracking()
                    .SingleOrDefaultAsync(x => x.Id == fine.ReaderId
                        && x.UserAccountId == AccountId(http.User) && !x.IsDeleted, transactionCt);
                if (reader is null) return Results.Forbid();
                if (fine.Status is FineStatus.Paid or FineStatus.Waived or FineStatus.Cancelled)
                    throw new DomainRuleException("fine.status", "Khoản phạt không còn cho phép thanh toán.");
                if (r.Amount <= 0)
                    throw new DomainRuleException("fine.payment.amount", "Số tiền thanh toán phải lớn hơn 0.");
                var remaining = fine.Amount - fine.PaidAmount;
                if (r.Amount > remaining)
                    throw new DomainRuleException("fine.payment.exceeds_balance",
                        $"Số tiền vượt quá dư nợ {remaining:N0}đ.");

                fine.PaidAmount += r.Amount;
                fine.TransactionCode = r.TransactionCode;
                fine.Status = fine.PaidAmount >= fine.Amount ? FineStatus.Paid : FineStatus.PartiallyPaid;
                if (fine.Status == FineStatus.Paid) fine.PaidAt = DateTimeOffset.UtcNow;
                await db.SaveChangesAsync(transactionCt);
                if (fine.Status == FineStatus.Paid)
                {
                    await notifications.PublishAsync(
                        reader.UserAccountId,
                        NotificationType.FinePaid,
                        "Đã thanh toán khoản phạt",
                        $"Khoản phạt {fine.Code} ({fine.Amount:N0}đ) đã được thanh toán hoàn tất.",
                        $"/me/fines/{fine.Id}",
                        $"fine-paid:{fine.Id}",
                        transactionCt);
                }
                return Results.NoContent();
            }, ct)).RequireAuthorization();

        WithPermission(api.MapPost("/fines/{id:int}/collect", async (
            int id,
            StaffFineCollectRequest? request,
            LibraryDbContext db,
            INotificationPublisher notifications,
            CancellationToken ct) =>
            await db.TransactionAsync<IResult>(async transactionCt =>
            {
                var fine = await db.Fines
                    .FromSqlInterpolated($"SELECT * FROM [Fines] WITH (UPDLOCK, ROWLOCK) WHERE [Id] = {id}")
                    .SingleOrDefaultAsync(transactionCt);
                if (fine is null) return Results.NotFound();
                if (fine.Status is FineStatus.Paid or FineStatus.Waived or FineStatus.Cancelled)
                    throw new DomainRuleException("fine.status", "Khoản phạt không còn cho phép thu tiền.");
                var remaining = fine.Amount - fine.PaidAmount;
                var amount = request?.Amount is > 0 ? request.Amount.Value : remaining;
                if (amount <= 0)
                    throw new DomainRuleException("fine.payment.amount", "Số tiền thanh toán phải lớn hơn 0.");
                if (amount > remaining)
                    throw new DomainRuleException("fine.payment.exceeds_balance",
                        $"Số tiền vượt quá dư nợ {remaining:N0}đ.");

                fine.PaidAmount += amount;
                fine.TransactionCode = string.IsNullOrWhiteSpace(request?.TransactionCode)
                    ? $"QUAY-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}"
                    : request.TransactionCode.Trim();
                fine.Status = fine.PaidAmount >= fine.Amount ? FineStatus.Paid : FineStatus.PartiallyPaid;
                if (fine.Status == FineStatus.Paid) fine.PaidAt = DateTimeOffset.UtcNow;
                await db.SaveChangesAsync(transactionCt);

                var readerAccountId = await db.Readers.AsNoTracking()
                    .Where(x => x.Id == fine.ReaderId && !x.IsDeleted)
                    .Select(x => (Guid?)x.UserAccountId)
                    .SingleOrDefaultAsync(transactionCt);
                if (readerAccountId.HasValue && fine.Status == FineStatus.Paid)
                {
                    await notifications.PublishAsync(
                        readerAccountId.Value,
                        NotificationType.FinePaid,
                        "Thủ thư đã xác nhận thu phạt",
                        $"Khoản phạt {fine.Code} ({fine.Amount:N0}đ) đã được thanh toán tại quầy.",
                        $"/me/fines/{fine.Id}",
                        $"fine-paid:{fine.Id}",
                        transactionCt);
                }
                return Results.Ok(new
                {
                    id = fine.Id,
                    status = fine.Status,
                    paidAmount = fine.PaidAmount,
                    remaining = fine.Amount - fine.PaidAmount
                });
            }, ct)).RequireAuthorization("InternalOnly"), PermissionCodes.BorrowConfirmReturn);

        WithPermission(api.MapPost("/fines/{id:int}/waive", async (
            int id,
            DecisionRequest? request,
            HttpContext http,
            LibraryDbContext db,
            INotificationPublisher notifications,
            CancellationToken ct) =>
            await db.TransactionAsync<IResult>(async transactionCt =>
            {
                var fine = await db.Fines
                    .FromSqlInterpolated($"SELECT * FROM [Fines] WITH (UPDLOCK, ROWLOCK) WHERE [Id] = {id}")
                    .SingleOrDefaultAsync(transactionCt);
                if (fine is null) return Results.NotFound();
                if (fine.Status is FineStatus.Paid or FineStatus.Waived or FineStatus.Cancelled)
                    throw new DomainRuleException("fine.status", "Khoản phạt này không thể miễn giảm.");

                fine.Status = FineStatus.Waived;
                var actor = http.User.Identity?.Name ?? AccountId(http.User).ToString();
                var reasonText = string.IsNullOrWhiteSpace(request?.Reason) ? null : request.Reason.Trim();
                var waiveNote = reasonText is null
                    ? $"Miễn phạt bởi {actor}"
                    : $"Miễn phạt ({actor}): {reasonText}";
                fine.Reason = string.IsNullOrWhiteSpace(fine.Reason)
                    ? waiveNote
                    : $"{fine.Reason} · {waiveNote}";
                await db.SaveChangesAsync(transactionCt);

                var readerAccountId = await db.Readers.AsNoTracking()
                    .Where(x => x.Id == fine.ReaderId && !x.IsDeleted)
                    .Select(x => (Guid?)x.UserAccountId)
                    .SingleOrDefaultAsync(transactionCt);
                if (readerAccountId.HasValue)
                {
                    await notifications.PublishAsync(
                        readerAccountId.Value,
                        NotificationType.FineWaived,
                        "Khoản phạt đã được miễn",
                        $"Khoản phạt {fine.Code} đã được thư viện miễn giảm.",
                        $"/me/fines/{fine.Id}",
                        $"fine-waived:{fine.Id}",
                        transactionCt);
                }
                return Results.Ok(new { id = fine.Id, status = fine.Status });
            }, ct)).RequireAuthorization("InternalOnly"), PermissionCodes.BorrowConfirmReturn);
    }

    private static void MapCommunity(RouteGroupBuilder api)
    {
        api.MapPut("/me/profile", async (
            ReaderProfileUpdateRequest request,
            HttpContext http,
            LibraryDbContext db,
            UserManager<LibraryUser> users,
            CancellationToken ct) =>
        {
            var readerId = await ReaderIdAsync(http.User, db, ct);
            if (!readerId.HasValue) return Results.Forbid();
            if (!VietnameseValidation.IsFullName(request.FullName))
                throw new DomainRuleException("reader.name.invalid", "Họ tên độc giả không hợp lệ.");
            if (!string.IsNullOrWhiteSpace(request.Phone) && !VietnameseValidation.IsPhone(request.Phone))
                throw new DomainRuleException("reader.phone.invalid", "Số điện thoại Việt Nam không hợp lệ.");
            if (request.Bio?.Trim().Length > 500)
                return ValidationProblem(new Dictionary<string, string[]>
                {
                    ["bio"] = ["Giới thiệu không được vượt quá 500 ký tự."]
                });

            var reader = await db.Readers.FindAsync([readerId.Value], ct);
            if (reader is null || reader.IsDeleted) return Results.NotFound();
            reader.FullName = VietnameseValidation.Normalize(request.FullName);
            reader.Phone = string.IsNullOrWhiteSpace(request.Phone)
                ? null
                : request.Phone.Replace(" ", "");
            reader.Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim();
            reader.Bio = string.IsNullOrWhiteSpace(request.Bio) ? null : request.Bio.Trim();
            var account = await users.FindByIdAsync(reader.UserAccountId.ToString());
            if (account is not null)
            {
                account.DisplayName = reader.FullName;
                var identityResult = await users.UpdateAsync(account);
                if (!identityResult.Succeeded)
                    return ValidationProblem(new Dictionary<string, string[]>
                    {
                        ["profile"] = ["Không thể cập nhật hồ sơ lúc này. Vui lòng thử lại."]
                    });
            }
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        }).RequireAuthorization();

        api.MapPost("/me/password", async (
            ChangePasswordRequest request,
            HttpContext http,
            UserManager<LibraryUser> users,
            SignInManager<LibraryUser> signIn) =>
        {
            if (string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
                return ValidationProblem(new Dictionary<string, string[]> { ["newPassword"] = ["Vui lòng nhập đầy đủ mật khẩu hiện tại và mật khẩu mới."] });
            if (request.NewPassword != request.ConfirmPassword)
                return ValidationProblem(new Dictionary<string, string[]> { ["confirmPassword"] = ["Xác nhận mật khẩu mới không khớp."] });
            var user = await users.GetUserAsync(http.User);
            if (user is null) return Results.Unauthorized();
            var change = await users.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (!change.Succeeded)
            {
                return ValidationProblem(new Dictionary<string, string[]>
                {
                    ["currentPassword"] = change.Errors.Select(e => e.Description).ToArray()
                });
            }
            user.MustChangePassword = false;
            await users.UpdateAsync(user);
            await signIn.RefreshSignInAsync(user);
            return Results.NoContent();
        }).RequireAuthorization();

        api.MapPut("/me/featured-books", async (
            FeaturedBooksRequest request,
            HttpContext http,
            LibraryDbContext db,
            CancellationToken ct) =>
        {
            var readerId = await ReaderIdAsync(http.User, db, ct);
            if (!readerId.HasValue) return Results.Forbid();
            if (request.BookIds.Count > 4 || request.BookIds.Distinct().Count() != request.BookIds.Count)
                return ValidationProblem(new Dictionary<string, string[]> { ["bookIds"] = ["Chọn tối đa 4 sách khác nhau."] });
            return await db.TransactionAsync<IResult>(async txCt =>
            {
                var validCount = await db.Books.CountAsync(x => request.BookIds.Contains(x.Id) && !x.IsDeleted, txCt);
                if (validCount != request.BookIds.Count)
                    return ValidationProblem(new Dictionary<string, string[]> { ["bookIds"] = ["Có đầu sách không còn tồn tại."] });
                await db.ReaderFeaturedBooks.Where(x => x.ReaderId == readerId.Value).ExecuteDeleteAsync(txCt);
                for (var i = 0; i < request.BookIds.Count; i++)
                    db.Add(new ReaderFeaturedBook { ReaderId = readerId.Value, BookId = request.BookIds[i], DisplayOrder = i + 1 });
                await db.SaveChangesAsync(txCt);
                return Results.NoContent();
            }, ct);
        }).RequireAuthorization();

        api.MapPost("/books/{bookId:int}/favorite", async (int bookId, HttpContext http,
            LibraryDbContext db, CancellationToken ct) =>
        {
            var readerId = await ReaderIdAsync(http.User, db, ct);
            if (!readerId.HasValue) return Results.Forbid();
            var old = await db.BookFavorites.SingleOrDefaultAsync(
                x => x.BookId == bookId && x.ReaderId == readerId.Value, ct);
            if (old is null) db.Add(new BookFavorite { BookId = bookId, ReaderId = readerId.Value }); else db.Remove(old);
            await db.SaveChangesAsync(ct); return Results.Ok(new { favorite = old is null });
        }).RequireAuthorization();
        api.MapPost("/books/{bookId:int}/ratings", async (int bookId, RatingRequest r, HttpContext http,
            LibraryDbContext db, CancellationToken ct) =>
        {
            var readerId = await ReaderIdAsync(http.User, db, ct);
            if (!readerId.HasValue) return Results.Forbid();
            if (r.Stars is < 1 or > 5) return ValidationProblem(new Dictionary<string, string[]> { ["stars"] = ["Số sao từ 1 đến 5."] });
            var item = await db.BookRatings.SingleOrDefaultAsync(
                x => x.BookId == bookId && x.ReaderId == readerId.Value, ct);
            if (item is null) db.Add(new BookRating { BookId = bookId, ReaderId = readerId.Value, Stars = r.Stars, Review = r.Review });
            else { item.Stars = r.Stars; item.Review = r.Review; }
            await db.SaveChangesAsync(ct); return Results.NoContent();
        }).RequireAuthorization();
        api.MapPost("/books/{bookId:int}/comments", async (int bookId, CommentRequest r, HttpContext http,
            LibraryDbContext db, CancellationToken ct) =>
        {
            var readerId = await ReaderIdAsync(http.User, db, ct);
            if (!readerId.HasValue) return Results.Forbid();
            var reader = await db.Readers.FindAsync([readerId.Value], ct);
            if (reader is null || reader.CommentLockMode == CommentLockMode.Permanent
                || reader.CommentBlockedUntil > DateTimeOffset.UtcNow) return Results.Forbid();
            if (string.IsNullOrWhiteSpace(r.Content) || r.Content.Trim().Length > 2_000)
                return ValidationProblem(new Dictionary<string, string[]>
                {
                    ["content"] = ["Bình luận phải có từ 1 đến 2.000 ký tự."]
                });
            if (!await db.Books.AnyAsync(x => x.Id == bookId && !x.IsDeleted, ct))
                return Results.NotFound();
            if (r.ParentId.HasValue && !await db.BookComments.AnyAsync(
                    x => x.Id == r.ParentId.Value && x.BookId == bookId && !x.IsDeleted, ct))
                return ValidationProblem(new Dictionary<string, string[]>
                {
                    ["parentId"] = ["Bình luận cha không thuộc sách này."]
                });
            var item = new BookComment { BookId = bookId, ReaderId = readerId.Value, ParentCommentId = r.ParentId, Content = r.Content.Trim() };
            db.Add(item); await db.SaveChangesAsync(ct); return Results.Created($"/api/v1/comments/{item.Id}", item);
        }).RequireAuthorization();
        api.MapPost("/reading/{bookId:int}", async (int bookId, ReadingRequest r, HttpContext http,
            LibraryDbContext db, CancellationToken ct) =>
        {
            var readerId = await ReaderIdAsync(http.User, db, ct);
            if (!readerId.HasValue) return Results.Forbid();
            var pageCount = await db.Books.Where(x => x.Id == bookId && !x.IsDeleted)
                .Select(x => (int?)x.PageCount).SingleOrDefaultAsync(ct);
            if (!pageCount.HasValue) return Results.NotFound();
            var totalPages = r.TotalPages > 0 ? r.TotalPages : pageCount.Value;
            if (totalPages <= 0 || r.CurrentPage < 1 || r.CurrentPage > totalPages)
                return ValidationProblem(new Dictionary<string, string[]>
                {
                    ["currentPage"] = ["Trang hiện tại phải nằm trong tổng số trang."]
                });
            var item = await db.ReadingHistories.SingleOrDefaultAsync(
                x => x.BookId == bookId && x.ReaderId == readerId.Value, ct);
            item ??= new ReadingHistory { BookId = bookId, ReaderId = readerId.Value, StartedAt = DateTimeOffset.UtcNow };
            item.CurrentPage = r.CurrentPage; item.TotalPages = totalPages; item.LastReadAt = DateTimeOffset.UtcNow;
            if (item.Id == 0) db.Add(item); await db.SaveChangesAsync(ct); return Results.NoContent();
        }).RequireAuthorization();

        api.MapPost("/comments/{commentId:int}/like", async (int commentId, HttpContext http,
            LibraryDbContext db, CancellationToken ct) =>
        {
            var readerId = await ReaderIdAsync(http.User, db, ct);
            if (!readerId.HasValue) return Results.Forbid();
            if (!await db.BookComments.AnyAsync(x => x.Id == commentId && !x.IsDeleted, ct))
                return Results.NotFound();
            var existing = await db.BookCommentLikes.SingleOrDefaultAsync(
                x => x.BookCommentId == commentId && x.ReaderId == readerId.Value, ct);
            if (existing is null) db.Add(new BookCommentLike { BookCommentId = commentId, ReaderId = readerId.Value });
            else db.Remove(existing);
            await db.SaveChangesAsync(ct);
            var likes = await db.BookCommentLikes.CountAsync(x => x.BookCommentId == commentId, ct);
            return Results.Ok(new { liked = existing is null, likes });
        }).RequireAuthorization();

        api.MapPost("/readers/{profileOwnerId:int}/profile-comments", async (
            int profileOwnerId,
            CommentRequest r,
            HttpContext http,
            LibraryDbContext db,
            CancellationToken ct) =>
        {
            var authorId = await ReaderIdAsync(http.User, db, ct);
            if (!authorId.HasValue) return Results.Forbid();
            var author = await db.Readers.FindAsync([authorId.Value], ct);
            if (author is null || author.CommentLockMode == CommentLockMode.Permanent
                || author.CommentBlockedUntil > DateTimeOffset.UtcNow)
                return Results.Forbid();
            if (!await db.Readers.AnyAsync(x => x.Id == profileOwnerId && !x.IsDeleted, ct))
                return Results.NotFound();
            if (string.IsNullOrWhiteSpace(r.Content) || r.Content.Trim().Length > 1_000)
                return ValidationProblem(new Dictionary<string, string[]> { ["content"] = ["Lời nhắn phải từ 1 đến 1.000 ký tự."] });
            var item = new ProfileComment { ProfileOwnerId = profileOwnerId, AuthorId = authorId.Value, Content = r.Content.Trim() };
            db.Add(item);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/readers/{profileOwnerId}", item);
        }).RequireAuthorization();

        api.MapPost("/comments/report", async (
            CommentReportRequest request,
            HttpContext http,
            LibraryDbContext db,
            CancellationToken ct) =>
        {
            var reporterId = await ReaderIdAsync(http.User, db, ct);
            if (!reporterId.HasValue) return Results.Forbid();
            if ((request.BookCommentId.HasValue && request.ProfileCommentId.HasValue)
                || (!request.BookCommentId.HasValue && !request.ProfileCommentId.HasValue))
                return ValidationProblem(new Dictionary<string, string[]> { ["target"] = ["Cần chọn một bình luận hợp lệ để báo cáo."] });
            if (string.IsNullOrWhiteSpace(request.Reason) || request.Reason.Trim().Length > 500)
                return ValidationProblem(new Dictionary<string, string[]> { ["reason"] = ["Vui lòng nhập lý do báo cáo (tối đa 500 ký tự)."] });

            int reportedUserId;
            if (request.BookCommentId.HasValue)
            {
                var comment = await db.BookComments.AsNoTracking()
                    .SingleOrDefaultAsync(x => x.Id == request.BookCommentId.Value && !x.IsDeleted, ct);
                if (comment is null) return Results.NotFound();
                reportedUserId = comment.ReaderId;
            }
            else
            {
                var comment = await db.ProfileComments.AsNoTracking()
                    .SingleOrDefaultAsync(x => x.Id == request.ProfileCommentId!.Value && !x.IsDeleted, ct);
                if (comment is null) return Results.NotFound();
                reportedUserId = comment.AuthorId;
            }
            if (reportedUserId == reporterId.Value)
                throw new DomainRuleException("report.self", "Không thể tự báo cáo bình luận của chính bạn.");

            var duplicate = await db.CommentReports.AnyAsync(x =>
                x.ReporterId == reporterId.Value
                && x.Status == ReportStatus.Pending
                && x.BookCommentId == request.BookCommentId
                && x.ProfileCommentId == request.ProfileCommentId, ct);
            if (duplicate)
                throw new DomainRuleException("report.duplicate", "Bạn đã gửi báo cáo cho bình luận này.");

            var report = new CommentReport
            {
                ReporterId = reporterId.Value,
                ReportedUserId = reportedUserId,
                BookCommentId = request.BookCommentId,
                ProfileCommentId = request.ProfileCommentId,
                Reason = request.Reason.Trim(),
                Status = ReportStatus.Pending
            };
            db.Add(report);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/v1/moderation/reports/{report.Id}", new { id = report.Id });
        }).RequireAuthorization();

        var moderation = api.MapGroup("/moderation").RequireAuthorization("InternalOnly");
        WithAnyPermission(moderation.MapPost("/reports/{id:int}/resolve", async (
            int id,
            ModerationActionRequest request,
            HttpContext http,
            LibraryDbContext db,
            CancellationToken ct) =>
        {
            var report = await db.CommentReports.SingleOrDefaultAsync(x => x.Id == id, ct);
            if (report is null) return Results.NotFound();
            if (request.HideComment)
            {
                if (report.BookCommentId.HasValue)
                {
                    var bc = await db.BookComments.SingleOrDefaultAsync(x => x.Id == report.BookCommentId.Value, ct);
                    if (bc is not null) { bc.IsDeleted = true; bc.DeletedAt = DateTimeOffset.UtcNow; }
                }
                if (report.ProfileCommentId.HasValue)
                {
                    var pc = await db.ProfileComments.SingleOrDefaultAsync(x => x.Id == report.ProfileCommentId.Value, ct);
                    if (pc is not null) { pc.IsDeleted = true; pc.DeletedAt = DateTimeOffset.UtcNow; }
                }
            }
            if (request.LockReaderDays is > 0)
            {
                var reader = await db.Readers.SingleOrDefaultAsync(x => x.Id == report.ReportedUserId, ct);
                if (reader is not null)
                {
                    reader.CommentLockMode = CommentLockMode.Temporary;
                    reader.CommentBlockedUntil = DateTimeOffset.UtcNow.AddDays(request.LockReaderDays.Value);
                }
            }
            report.Status = ReportStatus.Resolved;
            report.ReviewedByAccountId = AccountId(http.User);
            report.Resolution = string.IsNullOrWhiteSpace(request.Resolution)
                ? (request.HideComment ? "Đã ẩn bình luận vi phạm" : "Đã xử lý báo cáo")
                : request.Resolution.Trim();
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        }), PermissionCodes.ReaderEdit, PermissionCodes.SupportClaim);
        WithAnyPermission(moderation.MapPost("/reports/{id:int}/reject", async (
            int id,
            DecisionRequest? request,
            HttpContext http,
            LibraryDbContext db,
            CancellationToken ct) =>
        {
            var report = await db.CommentReports.SingleOrDefaultAsync(x => x.Id == id, ct);
            if (report is null) return Results.NotFound();
            report.Status = ReportStatus.Rejected;
            report.ReviewedByAccountId = AccountId(http.User);
            report.Resolution = string.IsNullOrWhiteSpace(request?.Reason) ? "Không vi phạm nội quy" : request.Reason.Trim();
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        }), PermissionCodes.ReaderEdit, PermissionCodes.SupportClaim);
    }

    private static void MapSupport(RouteGroupBuilder api)
    {
        api.MapGet("/help/topics", (IQuickHelpService help) => Results.Ok(help.GetTopics()));
        api.MapPost("/help/chat", async (
            ChatRequest request,
            HttpContext http,
            LibraryDbContext db,
            GeminiChatbotService chatbot,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.Question) || request.Question.Trim().Length > 1000)
                return ValidationProblem(new Dictionary<string, string[]> { ["question"] = ["Câu hỏi phải từ 1 đến 1.000 ký tự."] });

            int? readerId = null;
            if (http.User.Identity?.IsAuthenticated == true && !IsInternal(http.User))
                readerId = await ReaderIdAsync(http.User, db, ct);

            return Results.Ok(await chatbot.ReplyAsync(request.Question, request.History, readerId, ct));
        });
        api.MapGet("/share", (string url) =>
        {
            var encoded = Uri.EscapeDataString(url);
            return Results.Ok(new
            {
                facebook = $"https://www.facebook.com/sharer/sharer.php?u={encoded}",
                x = $"https://twitter.com/intent/tweet?url={encoded}",
                email = $"mailto:?body={encoded}"
            });
        });
        var support = api.MapGroup("/support").RequireAuthorization();
        support.MapGet("/tickets", async (HttpContext http, LibraryDbContext db,
            IPermissionService permissionService, CancellationToken ct) =>
        {
            var query = db.SupportTickets.AsNoTracking().Include(x => x.Messages).AsQueryable();
            if (IsInternal(http.User))
            {
                if (!http.User.IsInRole("Administrator")
                    && !await permissionService.HasAsync(AccountId(http.User), PermissionCodes.SupportClaim, ct))
                    return Results.Forbid();
            }
            else
            {
                var readerId = await ReaderIdAsync(http.User, db, ct);
                if (!readerId.HasValue) return Results.Forbid();
                query = query.Where(x => x.ReaderId == readerId.Value);
            }
            return Results.Ok(await query.OrderByDescending(x => x.LastMessageAt).Take(200).ToListAsync(ct));
        });
        support.MapGet("/tickets/{id:int}", async (int id, HttpContext http, LibraryDbContext db,
            IPermissionService permissionService, CancellationToken ct) =>
        {
            var ticket = await db.SupportTickets.AsNoTracking().Include(x => x.Messages)
                .SingleOrDefaultAsync(x => x.Id == id, ct);
            if (ticket is null) return Results.NotFound();
            if (IsInternal(http.User))
            {
                if (!http.User.IsInRole("Administrator")
                    && !await permissionService.HasAsync(AccountId(http.User), PermissionCodes.SupportClaim, ct))
                    return Results.Forbid();
            }
            else
            {
                var readerId = await ReaderIdAsync(http.User, db, ct);
                if (!readerId.HasValue || ticket.ReaderId != readerId.Value) return Results.Forbid();
            }
            return Results.Ok(ticket);
        });
        support.MapPost("/tickets", async (TicketRequest r, HttpContext http, LibraryDbContext db, CancellationToken ct) =>
        {
            var readerId = await ReaderIdAsync(http.User, db, ct);
            if (!readerId.HasValue) return Results.Forbid();
            if (string.IsNullOrWhiteSpace(r.Subject) || string.IsNullOrWhiteSpace(r.Message)
                || r.Subject.Trim().Length > 200 || r.Message.Trim().Length > 4_000)
                return ValidationProblem(new Dictionary<string, string[]>
                {
                    ["ticket"] = ["Chủ đề và nội dung là bắt buộc; nội dung tối đa 4.000 ký tự."]
                });
            var item = new SupportTicket { Code = $"TK-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..21].ToUpperInvariant(),
                ReaderId = readerId.Value, Subject = r.Subject.Trim(), LastMessageAt = DateTimeOffset.UtcNow, UnreadByStaff = true };
            item.Messages.Add(new SupportMessage { SenderAccountId = AccountId(http.User),
                SenderType = SupportSenderType.Reader, Content = r.Message.Trim() });
            db.Add(item); await db.SaveChangesAsync(ct); return Results.Created($"/api/v1/support/tickets/{item.Id}", item);
        });
        support.MapPost("/tickets/{id:int}/messages", async (int id, MessageRequest r, HttpContext http,
            LibraryDbContext db, IHubContext<SupportHub> hub, IPermissionService permissionService,
            CancellationToken ct) =>
        {
            var ticket = await db.SupportTickets.FindAsync([id], ct); if (ticket is null) return Results.NotFound();
            var sender = AccountId(http.User);
            var internalAccount = IsInternal(http.User);
            if (internalAccount && !http.User.IsInRole("Administrator")
                && !await permissionService.HasAsync(sender, PermissionCodes.SupportClaim, ct))
                return Results.Forbid();
            if (!internalAccount)
            {
                var readerId = await ReaderIdAsync(http.User, db, ct);
                if (!readerId.HasValue || ticket.ReaderId != readerId.Value) return Results.Forbid();
            }
            if (string.IsNullOrWhiteSpace(r.Content))
                return ValidationProblem(new Dictionary<string, string[]> { ["content"] = ["Nội dung không được để trống."] });
            var senderType = internalAccount ? SupportSenderType.Employee : SupportSenderType.Reader;
            var item = new SupportMessage
            {
                SupportTicketId = id,
                SenderAccountId = sender,
                SenderType = senderType,
                Content = r.Content.Trim()
            };
            ticket.LastMessageAt = DateTimeOffset.UtcNow;
            ticket.UnreadByReader = senderType != SupportSenderType.Reader;
            ticket.UnreadByStaff = senderType == SupportSenderType.Reader;
            db.Add(item); await db.SaveChangesAsync(ct);
            await hub.Clients.Group($"ticket:{id}").SendAsync("message", item, ct); return Results.Ok(item);
        });
        WithPermission(support.MapPost("/tickets/{id:int}/claim", async (int id, HttpContext http, LibraryDbContext db) =>
        {
            var item = await db.SupportTickets.FindAsync(id); if (item is null) return Results.NotFound();
            item.AssignedStaffAccountId = AccountId(http.User); item.Status = SupportTicketStatus.InProgress;
            await db.SaveChangesAsync(); return Results.NoContent();
        }).RequireAuthorization("InternalOnly"), PermissionCodes.SupportClaim);
        WithPermission(support.MapPost("/tickets/{id:int}/close", async (
            int id, LibraryDbContext db, CancellationToken ct) =>
        {
            var item = await db.SupportTickets.FindAsync([id], ct);
            if (item is null) return Results.NotFound();
            if (item.Status == SupportTicketStatus.Closed) return Results.NoContent();
            item.Status = SupportTicketStatus.Closed;
            item.ClosedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        }).RequireAuthorization("InternalOnly"), PermissionCodes.SupportClaim);
    }

    private static void MapAdministration(RouteGroupBuilder api)
    {
        var employees = api.MapGroup("/employees").RequireAuthorization("InternalOnly");
        WithPermission(employees.MapGet("/", async (LibraryDbContext db) => Results.Ok(await db.Employees.AsNoTracking()
            .Where(x => !x.IsDeleted).Include(x => x.Position).OrderBy(x => x.FullName).ToListAsync())),
            PermissionCodes.EmployeeView);
        WithPermission(employees.MapPost("/", async (Employee input, LibraryDbContext db, CancellationToken ct) =>
        {
            var position = await db.Positions.SingleOrDefaultAsync(x => x.Id == input.PositionId && !x.IsDeleted, ct);
            if (position is null) return ValidationProblem(new Dictionary<string, string[]> { ["positionId"] = ["Chức vụ không tồn tại."] });
            if (input.RoleId.HasValue && !await db.SecurityRoles.AnyAsync(x => x.Id == input.RoleId.Value && !x.IsDeleted, ct))
                return ValidationProblem(new Dictionary<string, string[]> { ["roleId"] = ["Vai trò bảo mật không tồn tại."] });
            EmployeePolicy.Validate(input, position, DateOnly.FromDateTime(DateTime.UtcNow));
            input.Id = 0; db.Add(input); await db.SaveChangesAsync(ct);
            return Results.Created($"/api/v1/employees/{input.Id}", input);
        }), PermissionCodes.EmployeeCreate);
        WithPermission(employees.MapPut("/{id:int}", async (int id, Employee value, LibraryDbContext db, CancellationToken ct) =>
        {
            var item = await db.Employees.SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
            if (item is null) return Results.NotFound();
            var position = await db.Positions.SingleOrDefaultAsync(x => x.Id == value.PositionId && !x.IsDeleted, ct);
            if (position is null) return ValidationProblem(new Dictionary<string, string[]> { ["positionId"] = ["Chức vụ không tồn tại."] });
            if (value.RoleId.HasValue && !await db.SecurityRoles.AnyAsync(x => x.Id == value.RoleId.Value && !x.IsDeleted, ct))
                return ValidationProblem(new Dictionary<string, string[]> { ["roleId"] = ["Vai trò bảo mật không tồn tại."] });
            EmployeePolicy.Validate(value, position, DateOnly.FromDateTime(DateTime.UtcNow));
            if (value.RowVersion is { Length: > 0 })
                db.Entry(item).Property(x => x.RowVersion).OriginalValue = value.RowVersion;
            item.FullName = value.FullName; item.Email = value.Email; item.Phone = value.Phone; item.Address = value.Address;
            item.NationalId = value.NationalId; item.Gender = value.Gender; item.DateOfBirth = value.DateOfBirth;
            item.HiredOn = value.HiredOn; item.PositionId = value.PositionId; item.Salary = value.Salary;
            item.Status = value.Status; item.StatusOptionId = value.StatusOptionId; item.LeaveReason = value.LeaveReason;
            item.LeaveStartedOn = value.LeaveStartedOn; item.UnexcusedDays = value.UnexcusedDays; item.RoleId = value.RoleId;
            await db.SaveChangesAsync(ct); return Results.NoContent();
        }), PermissionCodes.EmployeeEdit);
        WithPermission(employees.MapDelete("/{id:int}", async (
            int id,
            LibraryDbContext db,
            UserManager<LibraryUser> userManager,
            CancellationToken ct) =>
        {
            var item = await db.Employees.SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
            if (item is null) return Results.NotFound();
            var linkedAccountIds = await db.StaffAccounts
                .Where(x => x.EmployeeId == id)
                .Select(x => x.UserAccountId)
                .ToListAsync(ct);
            item.IsDeleted = true;
            item.DeletedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(ct);
            foreach (var accountId in linkedAccountIds)
            {
                var account = await userManager.FindByIdAsync(accountId.ToString());
                if (account is not null)
                {
                    account.IsActive = false;
                    await userManager.UpdateAsync(account);
                    await userManager.UpdateSecurityStampAsync(account);
                }
            }
            return Results.NoContent();
        }), PermissionCodes.EmployeeDelete);

        var readers = api.MapGroup("/readers").RequireAuthorization("InternalOnly");
        WithPermission(readers.MapGet("/", async (LibraryDbContext db) => Results.Ok(await db.Readers.AsNoTracking()
            .Where(x => !x.IsDeleted).OrderBy(x => x.FullName).Take(500).ToListAsync())),
            PermissionCodes.ReaderView);
        WithPermission(readers.MapPut("/{id:int}", async (
            int id, ReaderAdminUpdateRequest request, LibraryDbContext db, CancellationToken ct) =>
        {
            var item = await db.Readers.FindAsync([id], ct);
            if (item is null || item.IsDeleted) return Results.NotFound();
            if (!VietnameseValidation.IsFullName(request.FullName))
                throw new DomainRuleException("reader.name.invalid", "Họ tên độc giả không hợp lệ.");
            if (!string.IsNullOrWhiteSpace(request.Phone) && !VietnameseValidation.IsPhone(request.Phone))
                throw new DomainRuleException("reader.phone.invalid", "Số điện thoại Việt Nam không hợp lệ.");
            if (!string.IsNullOrWhiteSpace(request.IdentityNumber)
                && !VietnameseValidation.IsIdentityNumber(request.IdentityNumber))
                throw new DomainRuleException("reader.identity.invalid", "CCCD/CMND phải có 9 hoặc 12 chữ số.");
            if (request.MaxBorrowBooks is < 1 or > 100)
                throw new DomainRuleException("reader.borrow_limit.invalid", "Giới hạn mượn phải từ 1 đến 100 sách.");
            if (request.ExpiresOn < item.RegisteredOn)
                throw new DomainRuleException("reader.expiry.invalid", "Ngày hết hạn không thể trước ngày đăng ký.");
            if (request.CommentLockMode == CommentLockMode.Temporary
                && (!request.CommentBlockedUntil.HasValue || request.CommentBlockedUntil <= DateTimeOffset.UtcNow))
                throw new DomainRuleException("reader.comment_lock.invalid", "Khóa tạm thời cần thời hạn trong tương lai.");
            var profileErrors = ValidateReaderProfile(
                request.Bio,
                request.ActiveBadge,
                request.AvatarPath,
                request.CoverImagePath,
                request.BackgroundPath);
            if (profileErrors.Count > 0) return ValidationProblem(profileErrors);

            item.FullName = VietnameseValidation.Normalize(request.FullName);
            item.DateOfBirth = request.DateOfBirth;
            item.Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Replace(" ", "");
            item.Address = request.Address?.Trim();
            item.IdentityNumber = request.IdentityNumber?.Trim();
            item.ExpiresOn = request.ExpiresOn;
            item.MaxBorrowBooks = request.MaxBorrowBooks;
            item.Type = request.Type;
            item.Gender = request.Gender;
            item.IsVerified = request.IsVerified;
            item.CommentLockMode = request.CommentLockMode;
            item.CommentBlockedUntil = request.CommentLockMode == CommentLockMode.Temporary
                ? request.CommentBlockedUntil
                : null;
            ApplyReaderProfile(
                item,
                request.Bio,
                request.ActiveBadge,
                request.AvatarPath,
                request.CoverImagePath,
                request.BackgroundPath);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        }), PermissionCodes.ReaderEdit);
        WithPermission(readers.MapPatch("/{id:int}/profile", async (int id, ProfileRequest request, LibraryDbContext db) =>
        {
            var item = await db.Readers.FindAsync(id); if (item is null || item.IsDeleted) return Results.NotFound();
            var errors = ValidateReaderProfile(
                request.Bio,
                request.ActiveBadge,
                request.AvatarPath,
                request.CoverImagePath,
                request.BackgroundPath);
            if (errors.Count > 0) return ValidationProblem(errors);
            ApplyReaderProfile(
                item,
                request.Bio,
                request.ActiveBadge,
                request.AvatarPath,
                request.CoverImagePath,
                request.BackgroundPath);
            await db.SaveChangesAsync(); return Results.NoContent();
        }), PermissionCodes.ReaderEdit);
        WithPermission(readers.MapPut("/{id:int}/featured-books", async (int id, FeaturedBooksRequest request, LibraryDbContext db, CancellationToken ct) =>
        {
            if (request.BookIds.Count > 4 || request.BookIds.Distinct().Count() != request.BookIds.Count)
                return ValidationProblem(new Dictionary<string, string[]> { ["bookIds"] = ["Chọn tối đa 4 sách, không trùng."] });
            return await db.TransactionAsync<IResult>(async txCt =>
            {
                if (!await db.Readers.AnyAsync(x => x.Id == id && !x.IsDeleted, txCt))
                    return Results.NotFound();
                var validCount = await db.Books.CountAsync(x => request.BookIds.Contains(x.Id) && !x.IsDeleted, txCt);
                if (validCount != request.BookIds.Count)
                    return ValidationProblem(new Dictionary<string, string[]> { ["bookIds"] = ["Có đầu sách không còn tồn tại."] });
                await db.ReaderFeaturedBooks.Where(x => x.ReaderId == id).ExecuteDeleteAsync(txCt);
                db.AddRange(request.BookIds.Select((bookId, index) => new ReaderFeaturedBook { ReaderId = id, BookId = bookId, DisplayOrder = index + 1 }));
                await db.SaveChangesAsync(txCt);
                return Results.NoContent();
            }, ct);
        }), PermissionCodes.ReaderEdit);

        api.MapGet("/roles", async (LibraryDbContext db) => Results.Ok(await db.SecurityRoles.AsNoTracking()
            .Where(x => !x.IsDeleted).OrderBy(x => x.Name).ToListAsync())).RequireAuthorization("AdministratorOnly");
        api.MapGet("/permissions", async (LibraryDbContext db) => Results.Ok(await db.Permissions.AsNoTracking()
            .OrderBy(x => x.GroupName).ThenBy(x => x.Code).ToListAsync())).RequireAuthorization("AdministratorOnly");
        api.MapPut("/roles/{roleId:int}/permissions", async (int roleId, PermissionAssignment request, LibraryDbContext db) =>
        {
            await db.RolePermissions.Where(x => x.RoleId == roleId).ExecuteDeleteAsync();
            db.AddRange(request.PermissionIds.Select(x => new RolePermission { RoleId = roleId, PermissionId = x, Effect = PermissionEffect.Allow }));
            await db.SaveChangesAsync(); return Results.NoContent();
        }).RequireAuthorization("AdministratorOnly");
        api.MapPut("/employees/{employeeId:int}/permission/{permissionId:int}", async (
            int employeeId, int permissionId, PermissionOverride request, LibraryDbContext db) =>
        {
            var item = await db.UserPermissions.SingleOrDefaultAsync(x => x.EmployeeId == employeeId && x.PermissionId == permissionId);
            if (item is null) db.Add(new UserPermission { EmployeeId = employeeId, PermissionId = permissionId, Effect = request.Effect });
            else item.Effect = request.Effect;
            await db.SaveChangesAsync(); return Results.NoContent();
        }).RequireAuthorization("AdministratorOnly");
        api.MapDelete("/employees/{employeeId:int}/permission/{permissionId:int}", async (
            int employeeId, int permissionId, LibraryDbContext db) =>
        {
            await db.UserPermissions
                .Where(x => x.EmployeeId == employeeId && x.PermissionId == permissionId)
                .ExecuteDeleteAsync();
            return Results.NoContent();
        }).RequireAuthorization("AdministratorOnly");

        api.MapPost("/notifications/broadcast", async (BroadcastRequest request, LibraryDbContext db, IRealtimeGateway realtime, CancellationToken ct) =>
        {
            var accountIds = await db.Readers.AsNoTracking().Where(x => !x.IsDeleted).Select(x => x.UserAccountId).ToListAsync(ct);
            var group = Guid.NewGuid();
            var items = accountIds.Select(x => new Notification { RecipientAccountId = x, Type = NotificationType.AdminBroadcast,
                Title = request.Title, Message = request.Message, Link = request.Link, BroadcastGroupId = group }).ToList();
            db.AddRange(items); await db.SaveChangesAsync(ct);
            foreach (var chunk in items.Chunk(100))
                await Task.WhenAll(chunk.Select(x => realtime.NotifyAccountAsync(x.RecipientAccountId, x, ct)));
            return Results.Accepted(value: new { group, recipients = items.Count });
        }).RequireAuthorization("AdministratorOnly");

        WithPermission(api.MapGet("/reports/overview", async (LibraryDbContext db) => Results.Ok(new
        {
            books = await db.Books.CountAsync(x => !x.IsDeleted),
            readers = await db.Readers.CountAsync(x => !x.IsDeleted),
            borrowing = await db.BorrowSlips.CountAsync(x => x.Status == BorrowStatus.Borrowing
                || x.Status == BorrowStatus.Overdue
                || x.Status == BorrowStatus.PendingExtension),
            overdue = await db.BorrowSlips.CountAsync(x => x.DueAt < DateTimeOffset.UtcNow
                && (x.Status == BorrowStatus.Borrowing || x.Status == BorrowStatus.Overdue)),
            topBooks = await db.BorrowDetails.GroupBy(x => x.BookId).Select(x => new { bookId = x.Key, count = x.Sum(y => y.Quantity) })
                .OrderByDescending(x => x.count).Take(10).ToListAsync(),
            topReaders = await db.BorrowSlips.GroupBy(x => x.ReaderId).Select(x => new { readerId = x.Key, count = x.Count() })
                .OrderByDescending(x => x.count).Take(10).ToListAsync()
        })).RequireAuthorization("InternalOnly"), PermissionCodes.ReportView);

        api.MapGet("/notifications", async (HttpContext http, LibraryDbContext db) =>
            Results.Ok(await db.Notifications.AsNoTracking().Where(x => x.RecipientAccountId == AccountId(http.User))
                .OrderByDescending(x => x.CreatedAt).Take(50).ToListAsync())).RequireAuthorization();
        api.MapPost("/notifications/read-all", async (HttpContext http, LibraryDbContext db) =>
        {
            var id = AccountId(http.User);
            await db.Notifications.Where(x => x.RecipientAccountId == id && !x.IsRead)
                .ExecuteUpdateAsync(x => x.SetProperty(y => y.IsRead, true).SetProperty(y => y.ReadAt, DateTimeOffset.UtcNow));
            return Results.NoContent();
        }).RequireAuthorization();
        api.MapPost("/notifications/{id:int}/read", async (int id, HttpContext http, LibraryDbContext db) =>
        {
            var accountId = AccountId(http.User);
            var count = await db.Notifications
                .Where(x => x.Id == id && x.RecipientAccountId == accountId && !x.IsRead)
                .ExecuteUpdateAsync(x => x.SetProperty(y => y.IsRead, true)
                    .SetProperty(y => y.ReadAt, DateTimeOffset.UtcNow));
            return count == 0 && !await db.Notifications.AnyAsync(
                x => x.Id == id && x.RecipientAccountId == accountId)
                ? Results.NotFound()
                : Results.NoContent();
        }).RequireAuthorization();
        api.MapPut("/settings", async (SystemSettings input, LibraryDbContext db) =>
        {
            var errors = ValidateSettings(input);
            if (errors.Count > 0) return ValidationProblem(errors);
            var item = await db.SystemSettings.SingleOrDefaultAsync();
            if (item is null) { input.Id = 0; db.Add(input); } else
            {
                item.MaxExtensionDays = input.MaxExtensionDays; item.MaxExtensionCount = input.MaxExtensionCount;
                item.LateFeePerDay = input.LateFeePerDay; item.MaxBorrowBooks = input.MaxBorrowBooks;
                item.BorrowPeriodDays = input.BorrowPeriodDays; item.PickupWindowDays = input.PickupWindowDays;
            }
            await db.SaveChangesAsync(); return Results.NoContent();
        }).RequireAuthorization("AdministratorOnly");
    }

    private static Guid AccountId(ClaimsPrincipal user) =>
        Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id : throw new UnauthorizedAccessException("Tài khoản không hợp lệ.");
    private static bool IsInternal(ClaimsPrincipal user) =>
        user.IsInRole("Staff") || user.IsInRole("Administrator");
    private static async Task<int?> ReaderIdAsync(
        ClaimsPrincipal user,
        LibraryDbContext db,
        CancellationToken ct) =>
        await db.Readers.AsNoTracking()
            .Where(x => x.UserAccountId == AccountId(user) && !x.IsDeleted)
            .Select(x => (int?)x.Id)
            .SingleOrDefaultAsync(ct);
    private static RouteHandlerBuilder WithPermission(RouteHandlerBuilder builder, string permissionCode)
    {
        builder.AddEndpointFilter(async (context, next) =>
        {
            var user = context.HttpContext.User;
            if (user.IsInRole("Administrator")) return await next(context);
            var permissions = context.HttpContext.RequestServices.GetRequiredService<IPermissionService>();
            return await permissions.HasAsync(
                AccountId(user),
                permissionCode,
                context.HttpContext.RequestAborted)
                ? await next(context)
                : Results.Forbid();
        });
        return builder;
    }
    private static RouteHandlerBuilder WithAnyPermission(
        RouteHandlerBuilder builder,
        params string[] permissionCodes)
    {
        builder.AddEndpointFilter(async (context, next) =>
        {
            var user = context.HttpContext.User;
            if (user.IsInRole("Administrator")) return await next(context);
            var service = context.HttpContext.RequestServices.GetRequiredService<IPermissionService>();
            foreach (var code in permissionCodes)
                if (await service.HasAsync(AccountId(user), code, context.HttpContext.RequestAborted))
                    return await next(context);
            return Results.Forbid();
        });
        return builder;
    }
    private static string RequireIdempotency(HttpContext http) =>
        http.Request.Headers.TryGetValue("Idempotency-Key", out var key) && !string.IsNullOrWhiteSpace(key)
            ? key.ToString() : throw new DomainRuleException("idempotency.required", "Thiếu Idempotency-Key.");

    private static async Task<IResult> SaveUploadedImageAsync(
        IFormFile file,
        IWebHostEnvironment environment,
        string subFolder,
        string filePrefix,
        string fieldLabel,
        CancellationToken ct)
    {
        const long maxBytes = 5 * 1024 * 1024;
        if (file.Length is <= 0 or > maxBytes)
            return ValidationProblem(new Dictionary<string, string[]>
            {
                ["file"] = [$"{fieldLabel} phải có dung lượng từ 1 byte đến 5 MB."]
            });
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension is not (".jpg" or ".jpeg" or ".png" or ".webp"))
            return ValidationProblem(new Dictionary<string, string[]>
            {
                ["file"] = ["Chỉ chấp nhận ảnh JPEG, PNG hoặc WebP."]
            });

        await using var input = file.OpenReadStream();
        var header = new byte[Math.Min(12, (int)file.Length)];
        var read = await input.ReadAsync(header, ct);
        var valid = extension switch
        {
            ".jpg" or ".jpeg" => read >= 3 && header[0] == 0xff && header[1] == 0xd8 && header[2] == 0xff,
            ".png" => read >= 8 && header.AsSpan(0, 8).SequenceEqual(
                new byte[] { 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a }),
            ".webp" => read >= 12
                && header.AsSpan(0, 4).SequenceEqual("RIFF"u8)
                && header.AsSpan(8, 4).SequenceEqual("WEBP"u8),
            _ => false
        };
        if (!valid)
            return ValidationProblem(new Dictionary<string, string[]>
            {
                ["file"] = ["Nội dung tệp không khớp định dạng ảnh đã chọn."]
            });

        var root = string.IsNullOrWhiteSpace(environment.WebRootPath)
            ? Path.Combine(environment.ContentRootPath, "wwwroot")
            : environment.WebRootPath;
        var folder = Path.Combine(root, "uploads", subFolder);
        Directory.CreateDirectory(folder);
        var safeName = $"{filePrefix}-{Guid.NewGuid():N}{extension}";
        var destination = Path.Combine(folder, safeName);
        input.Position = 0;
        await using (var output = new FileStream(
            destination, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true))
            await input.CopyToAsync(output, ct);
        return Results.Ok(new { path = $"/uploads/{subFolder}/{safeName}" });
    }

    private static IResult ValidationProblem(IDictionary<string, string[]> errors) =>
        Results.ValidationProblem(
            errors,
            title: "Dữ liệu không hợp lệ",
            type: "https://libraryhub.local/problems/validation");

    private static Dictionary<string, string[]> ValidateSettings(SystemSettings input)
    {
        var errors = new Dictionary<string, string[]>();
        if (input.MaxExtensionDays is < 1 or > 60) errors["maxExtensionDays"] = ["Số ngày gia hạn phải từ 1 đến 60."];
        if (input.MaxExtensionCount is < 0 or > 10) errors["maxExtensionCount"] = ["Số lần gia hạn phải từ 0 đến 10."];
        if (input.LateFeePerDay is < 0 or > 10_000_000) errors["lateFeePerDay"] = ["Tiền phạt/ngày không hợp lệ."];
        if (input.MaxBorrowBooks is < 1 or > 100) errors["maxBorrowBooks"] = ["Số sách tối đa phải từ 1 đến 100."];
        if (input.BorrowPeriodDays is < 1 or > 365) errors["borrowPeriodDays"] = ["Thời hạn mượn phải từ 1 đến 365 ngày."];
        if (input.PickupWindowDays is < 1 or > 30) errors["pickupWindowDays"] = ["Thời gian đến lấy phải từ 1 đến 30 ngày."];
        return errors;
    }

    private static Dictionary<string, string[]> ValidateReaderProfile(
        string? bio,
        string? activeBadge,
        string? avatarPath,
        string? coverImagePath,
        string? backgroundPath)
    {
        var errors = new Dictionary<string, string[]>();
        if (bio?.Trim().Length > 1_000)
            errors["bio"] = ["Giới thiệu không được vượt quá 1.000 ký tự."];
        if (activeBadge?.Trim().Length > 100)
            errors["activeBadge"] = ["Badge hiển thị không được vượt quá 100 ký tự."];
        if (new[] { avatarPath, coverImagePath, backgroundPath }
            .Any(path => path?.Trim().Length > 2_048))
            errors["images"] = ["Đường dẫn hình ảnh không được vượt quá 2.048 ký tự."];
        return errors;
    }

    private static void ApplyReaderProfile(
        Reader reader,
        string? bio,
        string? activeBadge,
        string? avatarPath,
        string? coverImagePath,
        string? backgroundPath)
    {
        reader.Bio = NormalizeOptional(bio);
        reader.ActiveBadge = NormalizeOptional(activeBadge);
        reader.AvatarPath = NormalizeOptional(avatarPath);
        reader.CoverImagePath = NormalizeOptional(coverImagePath);
        reader.FullPageBackgroundPath = NormalizeOptional(backgroundPath);
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static async Task<IResult?> ValidateBookReferencesAsync(
        LibraryDbContext db,
        int categoryId,
        int authorId,
        int publisherId,
        int shelfId,
        CancellationToken ct)
    {
        var errors = new Dictionary<string, string[]>();
        if (!await db.Categories.AnyAsync(x => x.Id == categoryId && !x.IsDeleted, ct))
            errors["categoryId"] = ["Danh mục không tồn tại."];
        if (!await db.Authors.AnyAsync(x => x.Id == authorId && !x.IsDeleted, ct))
            errors["authorId"] = ["Tác giả không tồn tại."];
        if (!await db.Publishers.AnyAsync(x => x.Id == publisherId && !x.IsDeleted, ct))
            errors["publisherId"] = ["Nhà xuất bản không tồn tại."];
        if (!await db.Shelves.AnyAsync(x => x.Id == shelfId && !x.IsDeleted, ct))
            errors["shelfId"] = ["Kệ sách không tồn tại."];
        return errors.Count > 0 ? ValidationProblem(errors) : null;
    }

    private static void MapSimpleCrud<T>(
        RouteGroupBuilder api,
        string path,
        Func<LibraryDbContext, DbSet<T>> set,
        Action<T, T> update,
        Func<LibraryDbContext, int, CancellationToken, Task<bool>>? hasActiveReferences = null) where T : SoftDeletableEntity
    {
        var group = api.MapGroup(path).RequireAuthorization("InternalOnly");
        WithPermission(group.MapGet("/", async (LibraryDbContext db, CancellationToken ct) =>
            Results.Ok(await set(db).AsNoTracking().Where(x => !x.IsDeleted).OrderBy(x => x.Id).ToListAsync(ct))),
            PermissionCodes.CatalogView);
        WithPermission(group.MapPost("/", async ([FromBody] T input, LibraryDbContext db, CancellationToken ct) =>
        {
            input.Id = 0; set(db).Add(input); await db.SaveChangesAsync(ct);
            return Results.Created($"{path}/{input.Id}", input);
        }), PermissionCodes.CatalogManage);
        WithPermission(group.MapPut("/{id:int}", async (int id, [FromBody] T input, LibraryDbContext db, CancellationToken ct) =>
        {
            var old = await set(db).SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
            if (old is null) return Results.NotFound();
            update(old, input); await db.SaveChangesAsync(ct); return Results.NoContent();
        }), PermissionCodes.CatalogManage);
        WithPermission(group.MapDelete("/{id:int}", async (int id, LibraryDbContext db, CancellationToken ct) =>
        {
            var old = await set(db).SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
            if (old is null) return Results.NotFound();
            if (hasActiveReferences is not null && await hasActiveReferences(db, id, ct))
                throw new DomainRuleException("catalog.in_use", "Không thể xóa mục này vì đang được sách hoặc nhân viên sử dụng.");
            old.IsDeleted = true; old.DeletedAt = DateTimeOffset.UtcNow; await db.SaveChangesAsync(ct); return Results.NoContent();
        }), PermissionCodes.CatalogManage);
    }
}

public sealed record CreateBorrowRequest(IReadOnlyList<BorrowLine> Lines, string? Note);
public sealed record WalkInBorrowRequest(int ReaderId, IReadOnlyList<BorrowLine> Lines, string? Note, bool DeliverImmediately);
public sealed record DecisionRequest(string? Reason);
public sealed record ExtensionRequest(int Days, string Reason);
public sealed record ExtensionDecision(bool Approve);
public sealed record ReturnRequest(IReadOnlyList<ReturnLine> Lines);
public sealed record ReservationRequest(int BookId, string? Note);
public sealed record StatusRequest<T>(T Status);
public sealed record BookSuspensionRequest(
    BookSuspensionType Type,
    string Reason,
    DateTimeOffset? RecallDeadline);
public sealed record BookRestoreRequest(string Reason);
public sealed record CompleteMaintenanceRequest(
    int ActualTotalQuantity,
    int AvailableQuantityAfterInspection);
public sealed record PaymentRequest(decimal Amount, string TransactionCode);
public sealed record StaffFineCollectRequest(decimal? Amount, string? TransactionCode);
public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword, string ConfirmPassword);
public sealed record CommentReportRequest(int? BookCommentId, int? ProfileCommentId, string Reason);
public sealed record ModerationActionRequest(bool HideComment, int? LockReaderDays, string? Resolution);
public sealed record RatingRequest(int Stars, string? Review);
public sealed record CommentRequest(int? ParentId, string Content);
public sealed record ReadingRequest(int CurrentPage, int TotalPages);
public sealed record ReaderProfileUpdateRequest(string FullName, string? Phone, string? Address, string? Bio);
public sealed record TicketRequest(string Subject, string Message);
public sealed record MessageRequest(string Content);
public sealed record ChatRequest(string Question, IReadOnlyList<ChatTurn>? History = null);
public sealed record ProfileRequest(string? Bio, string? ActiveBadge, string? AvatarPath, string? CoverImagePath, string? BackgroundPath);
public sealed record ReaderAdminUpdateRequest(
    string FullName,
    DateOnly? DateOfBirth,
    string? Phone,
    string? Address,
    string? IdentityNumber,
    DateOnly ExpiresOn,
    int MaxBorrowBooks,
    ReaderType Type,
    Gender Gender,
    bool IsVerified,
    CommentLockMode CommentLockMode,
    DateTimeOffset? CommentBlockedUntil,
    string? Bio = null,
    string? ActiveBadge = null,
    string? AvatarPath = null,
    string? CoverImagePath = null,
    string? BackgroundPath = null);
public sealed record FeaturedBooksRequest(IReadOnlyList<int> BookIds);
public sealed record PermissionAssignment(IReadOnlyList<int> PermissionIds);
public sealed record PermissionOverride(PermissionEffect Effect);
public sealed record BroadcastRequest(string Title, string Message, string? Link);
