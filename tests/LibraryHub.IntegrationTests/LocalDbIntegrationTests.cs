using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using LibraryHub.Application;
using LibraryHub.Domain;
using LibraryHub.Infrastructure;
using LibraryHub.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace LibraryHub.IntegrationTests;

public sealed class LocalDbIntegrationTests : IAsyncLifetime
{
    private static readonly string Connection =
        Environment.GetEnvironmentVariable("LIBRARYHUB_TEST_CONNECTION")
        ?? "Server=.;Database=LibraryHubNext_IntegrationTests;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;Encrypt=False;Connect Timeout=60";
    private const string ReaderEmail = "integration.reader@libraryhub.local";
    private const string ReaderPassword = "Reader@Integration2026!";
    private const string StaffEmail = "integration.staff@libraryhub.local";
    private const string StaffPassword = "Staff@Integration2026!";
    private const string AdminEmail = "integration.admin@libraryhub.local";
    private const string AdminPassword = "Admin@Integration2026!";

    private LocalDbWebFactory factory = null!;

    public async ValueTask InitializeAsync()
    {
        await using (var migrationDb = NewDb())
            await migrationDb.Database.MigrateAsync(TestContext.Current.CancellationToken);

        factory = new LocalDbWebFactory(Connection);
        _ = factory.Services;
        await EnsureTestPrincipalsAsync();
    }

    public async ValueTask DisposeAsync() => await factory.DisposeAsync();

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Public_catalog_and_book_detail_render_real_database_content()
    {
        var ct = TestContext.Current.CancellationToken;
        var book = await CreateUniqueBookAsync("Public page");
        using var client = Client();
        var home = await client.GetAsync("/", ct);
        Assert.Equal(HttpStatusCode.OK, home.StatusCode);
        Assert.Contains("đầu sách", await home.Content.ReadAsStringAsync(ct));

        var detail = await client.GetAsync($"/books/{book.Id}", ct);
        var html = await detail.Content.ReadAsStringAsync(ct);
        Assert.Equal(HttpStatusCode.OK, detail.StatusCode);
        Assert.Contains(book.Title, html);
        Assert.Contains(book.Isbn, html);
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        Assert.True(await db.Books.Where(x => x.Id == book.Id).Select(x => x.ViewCount).SingleAsync(ct) > 0);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Authentication_ui_registers_linked_reader_and_protects_private_pages()
    {
        var ct = TestContext.Current.CancellationToken;
        using var anonymous = Client();
        Assert.Equal(HttpStatusCode.OK, (await anonymous.GetAsync("/login", ct)).StatusCode);
        var registerPage = await anonymous.GetAsync("/register", ct);
        Assert.Equal(HttpStatusCode.OK, registerPage.StatusCode);
        var html = await registerPage.Content.ReadAsStringAsync(ct);
        var tokenMatch = Regex.Match(
            html,
            "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"",
            RegexOptions.IgnoreCase);
        Assert.True(tokenMatch.Success);

        var email = $"stage2.{Guid.NewGuid():N}@libraryhub.local";
        var response = await anonymous.PostAsync("/register", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = WebUtility.HtmlDecode(tokenMatch.Groups[1].Value),
            ["FullName"] = "Stage Two Reader",
            ["Email"] = email,
            ["Password"] = "Reader@StageTwo2026!",
            ["ConfirmPassword"] = "Reader@StageTwo2026!",
            ["Phone"] = "0912345678",
            ["Address"] = "Hà Nội",
            ["ReaderType"] = "0"
        }), ct);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/login", response.Headers.Location?.OriginalString);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        var user = await db.Users.SingleAsync(x => x.Email == email, ct);
        Assert.True(await db.Readers.AnyAsync(x => x.UserAccountId == user.Id && x.Email == email, ct));
        Assert.False(user.EmailConfirmed);

        var protectedResponse = await anonymous.GetAsync("/me", ct);
        Assert.Equal(HttpStatusCode.Redirect, protectedResponse.StatusCode);
        Assert.Contains("/login?", protectedResponse.Headers.Location?.OriginalString);

        using var uiClient = Client();
        var loginHtml = await (await uiClient.GetAsync("/login", ct)).Content.ReadAsStringAsync(ct);
        var loginToken = AntiForgeryToken(loginHtml);
        var login = await uiClient.PostAsync("/login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = loginToken,
            ["Email"] = ReaderEmail,
            ["Password"] = ReaderPassword,
            ["RememberMe"] = "false"
        }), ct);
        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);
        Assert.Equal("/me", login.Headers.Location?.OriginalString);
        var mePage = await uiClient.GetAsync("/me", ct);
        Assert.Equal(HttpStatusCode.OK, mePage.StatusCode);
        var logoutToken = AntiForgeryToken(await mePage.Content.ReadAsStringAsync(ct));
        var logout = await uiClient.PostAsync("/logout", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = logoutToken
        }), ct);
        Assert.Equal(HttpStatusCode.Redirect, logout.StatusCode);
        Assert.Equal("/", logout.Headers.Location?.OriginalString);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Authentication_ui_is_vietnamese_and_unconfirmed_account_can_resend_email()
    {
        var ct = TestContext.Current.CancellationToken;
        using var client = Client();
        var registerHtml = await (await client.GetAsync("/register", ct)).Content.ReadAsStringAsync(ct);
        Assert.Contains("Dùng ít nhất 8 ký tự", registerHtml);
        Assert.Contains(">Sinh viên<", registerHtml);
        Assert.DoesNotContain(">Student<", registerHtml);
        Assert.DoesNotContain("Passwords must", registerHtml);
        Assert.DoesNotContain("The field Password", registerHtml);

        var invalidEmail = $"integration.password.{Guid.NewGuid():N}@libraryhub.local";
        var invalid = await client.PostAsync("/register", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = AntiForgeryToken(registerHtml),
            ["FullName"] = "Độc giả kiểm tra mật khẩu",
            ["Email"] = invalidEmail,
            ["Password"] = "12345678",
            ["ConfirmPassword"] = "12345678",
            ["ReaderType"] = "0"
        }), ct);
        var invalidHtml = WebUtility.HtmlDecode(await invalid.Content.ReadAsStringAsync(ct));
        Assert.Equal(HttpStatusCode.OK, invalid.StatusCode);
        Assert.True(
            invalidHtml.Contains("Mật khẩu phải có ít nhất 8 ký tự, gồm tối thiểu một chữ cái và một chữ số."),
            invalidHtml);
        Assert.DoesNotContain("Passwords must", invalidHtml);

        var email = $"integration.unconfirmed.{Guid.NewGuid():N}@libraryhub.local";
        const string password = "DocGia2026";
        Guid userId;
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            var users = scope.ServiceProvider.GetRequiredService<UserManager<LibraryUser>>();
            var user = new LibraryUser
            {
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email,
                EmailConfirmed = false,
                DisplayName = "Độc giả chưa xác thực",
                AccountKind = AccountKind.Reader
            };
            Assert.True((await users.CreateAsync(user, password)).Succeeded);
            Assert.True((await users.AddToRoleAsync(user, "Reader")).Succeeded);
            db.Readers.Add(new Reader
            {
                UserAccountId = user.Id,
                ReaderCode = $"DG-XT-{Guid.NewGuid():N}"[..21].ToUpperInvariant(),
                FullName = user.DisplayName,
                Email = email,
                RegisteredOn = DateOnly.FromDateTime(DateTime.UtcNow),
                ExpiresOn = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)),
                EmailConfirmed = false,
                MaxBorrowBooks = 10
            });
            await db.SaveChangesAsync(ct);
            userId = user.Id;
        }

        try
        {
            var loginHtml = await (await client.GetAsync("/login", ct)).Content.ReadAsStringAsync(ct);
            var blocked = await client.PostAsync("/login", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = AntiForgeryToken(loginHtml),
                ["Email"] = email,
                ["Password"] = password,
                ["RememberMe"] = "false"
            }), ct);
            var blockedRawHtml = await blocked.Content.ReadAsStringAsync(ct);
            var blockedHtml = WebUtility.HtmlDecode(blockedRawHtml);
            Assert.Equal(HttpStatusCode.OK, blocked.StatusCode);
            Assert.Contains("Gửi lại email xác thực", blockedHtml);
            Assert.Contains("Email của bạn chưa được xác thực", blockedHtml);

            var resend = await client.PostAsync("/resend-confirmation", new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["__RequestVerificationToken"] = AntiForgeryToken(blockedRawHtml),
                    ["Email"] = email
                }), ct);
            Assert.Equal(HttpStatusCode.Redirect, resend.StatusCode);
            var resendPage = await client.GetAsync(resend.Headers.Location!, ct);
            var resendHtml = WebUtility.HtmlDecode(await resendPage.Content.ReadAsStringAsync(ct));
            Assert.Contains("Chưa gửi được email xác thực", resendHtml);
            Assert.DoesNotContain("đã được gửi lại", resendHtml, StringComparison.OrdinalIgnoreCase);

            var forgotHtml = await (await client.GetAsync("/forgot-password", ct)).Content.ReadAsStringAsync(ct);
            Assert.Contains("Gửi liên kết đặt lại mật khẩu", forgotHtml);
        }
        finally
        {
            await using var scope = factory.Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            await db.Readers.Where(x => x.UserAccountId == userId).ExecuteDeleteAsync(ct);
            var users = scope.ServiceProvider.GetRequiredService<UserManager<LibraryUser>>();
            var user = await users.FindByIdAsync(userId.ToString());
            if (user is not null) Assert.True((await users.DeleteAsync(user)).Succeeded);
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Reader_and_staff_portals_render_owned_and_real_database_data()
    {
        var ct = TestContext.Current.CancellationToken;
        using var reader = await LoginAsync(ReaderEmail, ReaderPassword);
        var me = await reader.GetAsync("/me", ct);
        var meHtml = await me.Content.ReadAsStringAsync(ct);
        Assert.Equal(HttpStatusCode.OK, me.StatusCode);
        Assert.Contains("Integration Reader", meHtml);
        Assert.Contains("Tổng quan hoạt động", meHtml);
        Assert.DoesNotContain(StaffEmail, meHtml);
        Assert.Contains("Phiếu mượn của tôi",
            await (await reader.GetAsync("/me/borrows", ct)).Content.ReadAsStringAsync(ct));
        Assert.Contains("Khoản phạt",
            await (await reader.GetAsync("/me/fines", ct)).Content.ReadAsStringAsync(ct));

        using var staff = await LoginAsync(StaffEmail, StaffPassword);
        foreach (var path in new[] { "/admin", "/admin/books", "/admin/catalog", "/admin/circulation" })
        {
            var page = await staff.GetAsync(path, ct);
            Assert.Equal(HttpStatusCode.OK, page.StatusCode);
            Assert.Contains("LibraryHub", await page.Content.ReadAsStringAsync(ct));
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Staff_catalog_ui_backend_creates_and_updates_real_record()
    {
        var ct = TestContext.Current.CancellationToken;
        using var staff = await LoginAsync(StaffEmail, StaffPassword);
        var name = $"Stage2 Category {Guid.NewGuid():N}";
        var created = await staff.PostAsJsonAsync("/api/v1/categories", new
        {
            name,
            description = "Created by Stage 2 integration test"
        }, ct);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var payload = await created.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        var id = payload.GetProperty("id").GetInt32();

        var updated = await staff.PutAsJsonAsync($"/api/v1/categories/{id}", new
        {
            name,
            description = "Updated through RBAC API"
        }, ct);
        Assert.Equal(HttpStatusCode.NoContent, updated.StatusCode);
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        Assert.Equal("Updated through RBAC API",
            (await db.Categories.AsNoTracking().SingleAsync(x => x.Id == id, ct)).Description);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Main_http_flow_borrows_returns_pays_and_reserves_through_ui_backends()
    {
        var ct = TestContext.Current.CancellationToken;
        var borrowBook = await CreateUniqueBookAsync("Borrow flow");
        using var reader = await LoginAsync(ReaderEmail, ReaderPassword);
        using var staff = await LoginAsync(StaffEmail, StaffPassword);

        using var borrowRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/borrows")
        {
            Content = JsonContent.Create(new
            {
                lines = new[] { new { bookId = borrowBook.Id, quantity = 1 } },
                note = "Stage 2 UI flow"
            })
        };
        borrowRequest.Headers.Add("Idempotency-Key", $"stage2-borrow-{Guid.NewGuid():N}");
        var created = await reader.SendAsync(borrowRequest, ct);
        Assert.Equal(HttpStatusCode.OK, created.StatusCode);
        var createdPayload = await created.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        var slipId = createdPayload.GetProperty("id").GetInt32();
        var slipCode = createdPayload.GetProperty("code").GetString();

        await using (var createdScope = factory.Services.CreateAsyncScope())
        {
            var createdDb = createdScope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            Assert.Equal(borrowBook.AvailableQuantity,
                await createdDb.Books.Where(x => x.Id == borrowBook.Id).Select(x => x.AvailableQuantity).SingleAsync(ct));
        }

        Assert.Equal(HttpStatusCode.NoContent,
            (await staff.PostAsync($"/api/v1/borrows/{slipId}/approve", null, ct)).StatusCode);
        await using (var approvedScope = factory.Services.CreateAsyncScope())
        {
            var approvedDb = approvedScope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            Assert.Equal(borrowBook.AvailableQuantity,
                await approvedDb.Books.Where(x => x.Id == borrowBook.Id).Select(x => x.AvailableQuantity).SingleAsync(ct));
        }
        Assert.Equal(HttpStatusCode.NoContent,
            (await staff.PostAsync($"/api/v1/borrows/{slipId}/deliver", null, ct)).StatusCode);

        int detailId;
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            Assert.Equal(borrowBook.AvailableQuantity - 1,
                await db.Books.Where(x => x.Id == borrowBook.Id).Select(x => x.AvailableQuantity).SingleAsync(ct));
            detailId = await db.BorrowDetails.Where(x => x.BorrowSlipId == slipId)
                .Select(x => x.Id).SingleAsync(ct);
        }

        using var returnRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/borrows/{slipId}/returns")
        {
            Content = JsonContent.Create(new
            {
                lines = new[]
                {
                    new
                    {
                        borrowDetailId = detailId,
                        quantity = 1,
                        condition = (int)ItemCondition.Damaged,
                        damagePercent = 5,
                        note = "Stage 2 damage"
                    }
                }
            })
        };
        returnRequest.Headers.Add("Idempotency-Key", $"stage2-return-{Guid.NewGuid():N}");
        var returned = await staff.SendAsync(returnRequest, ct);
        Assert.Equal(HttpStatusCode.OK, returned.StatusCode);
        var returnPayload = await returned.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        var fineId = returnPayload.GetProperty("fineId").GetInt32();
        var fineAmount = returnPayload.GetProperty("fineAmount").GetDecimal();
        Assert.True(fineAmount > 0);

        var payment = await reader.PostAsJsonAsync($"/api/v1/fines/{fineId}/pay", new
        {
            amount = fineAmount,
            transactionCode = $"STAGE2-{Guid.NewGuid():N}"
        }, ct);
        Assert.Equal(HttpStatusCode.NoContent, payment.StatusCode);

        var reservationBook = await CreateUniqueBookAsync("Reservation flow");
        var reservation = await reader.PostAsJsonAsync("/api/v1/reservations", new
        {
            bookId = reservationBook.Id,
            note = "Stage 2 reservation"
        }, ct);
        Assert.Equal(HttpStatusCode.Created, reservation.StatusCode);
        var reservationPayload = await reservation.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        var reservationId = reservationPayload.GetProperty("id").GetInt32();
        Assert.Equal(HttpStatusCode.NoContent,
            (await staff.PatchAsJsonAsync($"/api/v1/reservations/{reservationId}/status",
                new { status = (int)ReservationStatus.Approved }, ct)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent,
            (await staff.PatchAsJsonAsync($"/api/v1/reservations/{reservationId}/status",
                new { status = (int)ReservationStatus.ReadyToBorrow }, ct)).StatusCode);

        await using (var readyScope = factory.Services.CreateAsyncScope())
        {
            var readyDb = readyScope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            Assert.Equal(reservationBook.AvailableQuantity,
                await readyDb.Books.Where(x => x.Id == reservationBook.Id).Select(x => x.AvailableQuantity).SingleAsync(ct));
            Assert.Equal(ReservationStatus.ReadyToBorrow,
                await readyDb.Reservations.Where(x => x.Id == reservationId).Select(x => x.Status).SingleAsync(ct));
            Assert.NotNull(await readyDb.Reservations.Where(x => x.Id == reservationId)
                .Select(x => x.ExpiresAt).SingleAsync(ct));
            Assert.True(await readyDb.Notifications.AnyAsync(x =>
                x.Type == NotificationType.ReservationReady && x.Message.Contains(reservationBook.Title), ct));
        }

        var circulationHtml = await (await staff.GetAsync("/admin/circulation", ct)).Content.ReadAsStringAsync(ct);
        Assert.Contains($"/api/v1/reservations/{reservationId}/deliver", circulationHtml);
        Assert.Contains("Giao sách", circulationHtml);
        var reservationDelivery = await staff.PostAsync($"/api/v1/reservations/{reservationId}/deliver", null, ct);
        Assert.Equal(HttpStatusCode.OK, reservationDelivery.StatusCode);

        var borrowListHtml = await (await staff.GetAsync("/admin/circulation", ct)).Content.ReadAsStringAsync(ct);
        Assert.DoesNotContain(slipCode!, borrowListHtml);
        Assert.DoesNotContain($"/api/v1/reservations/{reservationId}/deliver", borrowListHtml);
        await using var finalScope = factory.Services.CreateAsyncScope();
        var finalDb = finalScope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        Assert.Equal(FineStatus.Paid,
            await finalDb.Fines.Where(x => x.Id == fineId).Select(x => x.Status).SingleAsync(ct));
        Assert.Equal(BorrowStatus.LostOrDamaged,
            await finalDb.BorrowSlips.Where(x => x.Id == slipId).Select(x => x.Status).SingleAsync(ct));
        Assert.Equal(ReservationStatus.Completed,
            await finalDb.Reservations.Where(x => x.Id == reservationId).Select(x => x.Status).SingleAsync(ct));
        Assert.Equal(reservationBook.AvailableQuantity - 1,
            await finalDb.Books.Where(x => x.Id == reservationBook.Id).Select(x => x.AvailableQuantity).SingleAsync(ct));
        Assert.True(await finalDb.BorrowSlips.AnyAsync(x =>
            x.IdempotencyKey == $"reservation-delivery-{reservationId}" && x.Status == BorrowStatus.Borrowing, ct));
        Assert.True(await finalDb.Notifications.AnyAsync(x =>
            x.Type == NotificationType.BorrowDelivered && x.Message.Contains(reservationBook.Title), ct));
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Permission_seed_is_code_based_and_repeatable()
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        var seeder = scope.ServiceProvider.GetRequiredService<IPermissionSeeder>();

        await seeder.SeedAsync(TestContext.Current.CancellationToken);
        var permissionCount = await db.Permissions.CountAsync(TestContext.Current.CancellationToken);
        var assignmentCount = await db.RolePermissions.CountAsync(TestContext.Current.CancellationToken);
        await seeder.SeedAsync(TestContext.Current.CancellationToken);
        db.ChangeTracker.Clear();

        Assert.Equal(permissionCount, await db.Permissions.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(assignmentCount, await db.RolePermissions.CountAsync(TestContext.Current.CancellationToken));
        Assert.Empty(await db.Permissions.GroupBy(x => x.Code).Where(x => x.Count() > 1)
            .Select(x => x.Key).ToListAsync(TestContext.Current.CancellationToken));

        var expected = PermissionCodes.All.Select(x => x.Code).OrderBy(x => x).ToArray();
        var actual = await db.Permissions.Select(x => x.Code).OrderBy(x => x)
            .ToArrayAsync(TestContext.Current.CancellationToken);
        Assert.All(expected, code => Assert.Contains(code, actual));

        var staffCodes = await (
            from role in db.SecurityRoles
            join rolePermission in db.RolePermissions on role.Id equals rolePermission.RoleId
            join permission in db.Permissions on rolePermission.PermissionId equals permission.Id
            where role.Code == "INTEGRATION_STAFF" && rolePermission.Effect == PermissionEffect.Allow
            select permission.Code).ToListAsync(TestContext.Current.CancellationToken);
        Assert.Contains(PermissionCodes.EmployeeView, staffCodes);
        Assert.Contains(PermissionCodes.BorrowConfirmReturn, staffCodes);
        Assert.Contains(PermissionCodes.ReservationManage, staffCodes);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Staff_permission_allows_view_and_explicit_deny_blocks_create()
    {
        using var client = await LoginAsync(StaffEmail, StaffPassword);
        var allowed = await client.GetAsync("/api/v1/employees/", TestContext.Current.CancellationToken);
        var denied = await client.PostAsJsonAsync("/api/v1/employees/", new
        {
            employeeCode = "SHOULD-NOT-BE-CREATED",
            fullName = "Integration Denied",
            email = "denied@example.test",
            phone = "0912345678",
            address = "Hà Nội",
            nationalId = "001234567890",
            dateOfBirth = "1990-01-01",
            hiredOn = "2020-01-01",
            positionId = 1,
            salary = 1
        }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, allowed.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, denied.StatusCode);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Employee_permission_override_can_return_to_role_inheritance()
    {
        var ct = TestContext.Current.CancellationToken;
        int employeeId;
        int permissionId;
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            employeeId = await db.Employees.Where(x => x.EmployeeCode == "NV-INTEGRATION")
                .Select(x => x.Id).SingleAsync(ct);
            permissionId = await db.Permissions.Where(x => x.Code == PermissionCodes.CatalogView)
                .Select(x => x.Id).SingleAsync(ct);
        }

        using var admin = await LoginAsync(AdminEmail, AdminPassword);
        using var staff = await LoginAsync(StaffEmail, StaffPassword);
        var endpoint = $"/api/v1/employees/{employeeId}/permission/{permissionId}";

        await admin.DeleteAsync(endpoint, ct);
        Assert.Equal(HttpStatusCode.OK, (await staff.GetAsync("/api/v1/categories/", ct)).StatusCode);
        try
        {
            var denied = await admin.PutAsJsonAsync(endpoint, new { effect = (int)PermissionEffect.Deny }, ct);
            Assert.Equal(HttpStatusCode.NoContent, denied.StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden,
                (await staff.GetAsync("/api/v1/categories/", ct)).StatusCode);
        }
        finally
        {
            var inherited = await admin.DeleteAsync(endpoint, ct);
            Assert.Equal(HttpStatusCode.NoContent, inherited.StatusCode);
        }
        Assert.Equal(HttpStatusCode.OK, (await staff.GetAsync("/api/v1/categories/", ct)).StatusCode);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Return_replay_preserves_original_fine_snapshot_without_duplicates()
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        var reader = await db.Readers.SingleAsync(x => x.Email == ReaderEmail, TestContext.Current.CancellationToken);
        var book = await EnsureBookAsync(db);
        var accountId = reader.UserAccountId;
        var key = $"integration-return-{Guid.NewGuid():N}";
        var slip = new BorrowSlip
        {
            Code = $"BR-I-{Guid.NewGuid():N}"[..21].ToUpperInvariant(),
            ReaderId = reader.Id,
            TotalBooks = 1,
            RequestedAt = DateTimeOffset.UtcNow.AddDays(-2),
            BorrowedAt = DateTimeOffset.UtcNow.AddDays(-1),
            DueAt = DateTimeOffset.UtcNow.AddDays(1),
            Status = BorrowStatus.Borrowing,
            CreatedByAccountId = accountId,
            IdempotencyKey = $"integration-borrow-{Guid.NewGuid():N}",
            Details =
            [
                new BorrowDetail
                {
                    BookId = book.Id,
                    Quantity = 1,
                    BorrowPrice = book.Price,
                    DueAt = DateTimeOffset.UtcNow.AddDays(1)
                }
            ]
        };
        db.Add(slip);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = new BorrowService(
            db,
            new FineCalculator(),
            new NullNotifications(),
            new LibraryHub.Infrastructure.SystemClock());
        var command = new ReturnBooks(
            slip.Id,
            accountId,
            [new ReturnLine(slip.Details[0].Id, 1, ItemCondition.Damaged, 10, "integration")],
            key);
        var first = await service.ReturnAsync(command, TestContext.Current.CancellationToken);
        var replay = await service.ReturnAsync(command, TestContext.Current.CancellationToken);

        Assert.False(first.Replayed);
        Assert.True(replay.Replayed);
        Assert.Equal(first.SlipId, replay.SlipId);
        Assert.Equal(first.Status, replay.Status);
        Assert.Equal(first.FineId, replay.FineId);
        Assert.Equal(first.FineAmount, replay.FineAmount);
        Assert.NotNull(first.FineId);
        Assert.True(first.FineAmount > 0);
        Assert.Equal(1, await db.Fines.CountAsync(x => x.BorrowSlipId == slip.Id, TestContext.Current.CancellationToken));
        Assert.Equal(1, await db.BorrowReturns.CountAsync(x => x.IdempotencyKey == key, TestContext.Current.CancellationToken));
        Assert.Equal(1, slip.Details[0].ReturnedQuantity);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Domain_rule_exception_maps_to_problem_details_400_and_409()
    {
        using var readerClient = await LoginAsync(ReaderEmail, ReaderPassword);
        using var emptyBorrow = new HttpRequestMessage(HttpMethod.Post, "/api/v1/borrows/")
        {
            Content = JsonContent.Create(new { lines = Array.Empty<object>(), note = (string?)null })
        };
        emptyBorrow.Headers.Add("Idempotency-Key", $"integration-empty-{Guid.NewGuid():N}");
        var badRequest = await readerClient.SendAsync(emptyBorrow, TestContext.Current.CancellationToken);
        var badProblem = await badRequest.Content.ReadFromJsonAsync<JsonElement>(
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, badRequest.StatusCode);
        Assert.Equal("application/problem+json", badRequest.Content.Headers.ContentType?.MediaType);
        Assert.Equal("borrow.empty", badProblem.GetProperty("code").GetString());
        Assert.Equal(400, badProblem.GetProperty("status").GetInt32());

        var invalidSlipId = await CreateBorrowingSlipAsync();
        using var staffClient = await LoginAsync(StaffEmail, StaffPassword);
        var conflict = await staffClient.PostAsync(
            $"/api/v1/borrows/{invalidSlipId}/approve",
            null,
            TestContext.Current.CancellationToken);
        var conflictProblem = await conflict.Content.ReadFromJsonAsync<JsonElement>(
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, conflict.StatusCode);
        Assert.Equal("application/problem+json", conflict.Content.Headers.ContentType?.MediaType);
        Assert.Equal("borrow.status", conflictProblem.GetProperty("code").GetString());
        Assert.Equal(409, conflictProblem.GetProperty("status").GetInt32());
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Concurrent_fine_payments_are_serialized_and_cannot_overpay()
    {
        var fineId = await CreateFineAsync(100_000m);
        using var firstClient = await LoginAsync(ReaderEmail, ReaderPassword);
        using var secondClient = await LoginAsync(ReaderEmail, ReaderPassword);

        var firstTask = firstClient.PostAsJsonAsync(
            $"/api/v1/fines/{fineId}/pay",
            new { amount = 70_000m, transactionCode = $"TX-A-{Guid.NewGuid():N}" },
            TestContext.Current.CancellationToken);
        var secondTask = secondClient.PostAsJsonAsync(
            $"/api/v1/fines/{fineId}/pay",
            new { amount = 70_000m, transactionCode = $"TX-B-{Guid.NewGuid():N}" },
            TestContext.Current.CancellationToken);
        var responses = await Task.WhenAll(firstTask, secondTask);

        Assert.Contains(responses, x => x.StatusCode == HttpStatusCode.NoContent);
        Assert.Contains(responses, x => x.StatusCode == HttpStatusCode.Conflict);
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        var fine = await db.Fines.AsNoTracking().SingleAsync(x => x.Id == fineId, TestContext.Current.CancellationToken);
        Assert.Equal(70_000m, fine.PaidAmount);
        Assert.True(fine.PaidAmount <= fine.Amount);
        Assert.Equal(FineStatus.PartiallyPaid, fine.Status);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task SignalR_hub_negotiation_requires_authentication()
    {
        using var anonymous = Client();
        var unauthorized = await anonymous.PostAsync(
            "/hubs/notifications/negotiate?negotiateVersion=1",
            null,
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorized.StatusCode);

        using var reader = await LoginAsync(ReaderEmail, ReaderPassword);
        var authorized = await reader.PostAsync(
            "/hubs/notifications/negotiate?negotiateVersion=1",
            null,
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, authorized.StatusCode);
        var negotiation = await authorized.Content.ReadFromJsonAsync<JsonElement>(
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.True(negotiation.TryGetProperty("connectionId", out _));
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Stage3_admin_pages_use_rbac_and_render_real_data()
    {
        var ct = TestContext.Current.CancellationToken;
        using var staff = await LoginAsync(StaffEmail, StaffPassword);
        foreach (var path in new[]
        {
            "/admin/readers?q=Integration",
            "/admin/employees?q=Integration",
            "/admin/support",
            "/admin/circulation?borrowPage=2&reservationPage=2&finePage=2"
        })
        {
            var response = await staff.GetAsync(path, ct);
            var html = await response.Content.ReadAsStringAsync(ct);
            Assert.True(response.StatusCode == HttpStatusCode.OK,
                $"{path} returned {(int)response.StatusCode}: {html}");
            Assert.Contains("LibraryHub", html);
        }
        var deniedRbac = await staff.GetAsync("/admin/rbac", ct);
        var deniedSettings = await staff.GetAsync("/admin/settings", ct);
        Assert.Equal(HttpStatusCode.Redirect, deniedRbac.StatusCode);
        Assert.Contains("/access-denied", deniedRbac.Headers.Location?.OriginalString);
        Assert.Equal(HttpStatusCode.Redirect, deniedSettings.StatusCode);
        Assert.Contains("/access-denied", deniedSettings.Headers.Location?.OriginalString);

        Reader currentReader;
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            currentReader = await db.Readers.AsNoTracking().SingleAsync(x => x.Email == ReaderEmail, ct);
        }
        var readerUpdate = await staff.PutAsJsonAsync($"/api/v1/readers/{currentReader.Id}", new
        {
            currentReader.FullName,
            currentReader.DateOfBirth,
            currentReader.Phone,
            currentReader.Address,
            currentReader.IdentityNumber,
            currentReader.ExpiresOn,
            currentReader.MaxBorrowBooks,
            type = (int)currentReader.Type,
            gender = (int)currentReader.Gender,
            currentReader.IsVerified,
            commentLockMode = (int)currentReader.CommentLockMode,
            currentReader.CommentBlockedUntil,
            bio = "Hồ sơ tích hợp được cập nhật nguyên tử",
            activeBadge = "Kiểm thử tích hợp",
            avatarPath = "/images/integration-avatar.png",
            coverImagePath = "/images/integration-cover.png",
            backgroundPath = "/images/integration-background.png"
        }, ct);
        Assert.Equal(HttpStatusCode.NoContent, readerUpdate.StatusCode);
        await using (var verifyReaderScope = factory.Services.CreateAsyncScope())
        {
            var verifyReaderDb = verifyReaderScope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            var updatedReader = await verifyReaderDb.Readers.AsNoTracking()
                .SingleAsync(x => x.Id == currentReader.Id, ct);
            Assert.Equal("Hồ sơ tích hợp được cập nhật nguyên tử", updatedReader.Bio);
            Assert.Equal("Kiểm thử tích hợp", updatedReader.ActiveBadge);
            Assert.Equal("/images/integration-avatar.png", updatedReader.AvatarPath);
        }

        using var admin = await LoginAsync(AdminEmail, AdminPassword);
        Assert.Equal(HttpStatusCode.OK, (await admin.GetAsync("/admin/rbac", ct)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await admin.GetAsync("/admin/settings", ct)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await admin.GetAsync("/api/v1/permissions", ct)).StatusCode);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Notification_center_marks_only_owned_notifications()
    {
        var ct = TestContext.Current.CancellationToken;
        int ownedId;
        int foreignId;
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            var readerAccount = await db.Users.Where(x => x.Email == ReaderEmail).Select(x => x.Id).SingleAsync(ct);
            var staffAccount = await db.Users.Where(x => x.Email == StaffEmail).Select(x => x.Id).SingleAsync(ct);
            var owned = new Notification
            {
                RecipientAccountId = readerAccount,
                Type = NotificationType.System,
                Title = "Stage 3 owned notification",
                Message = "Notification integration test"
            };
            var foreign = new Notification
            {
                RecipientAccountId = staffAccount,
                Type = NotificationType.System,
                Title = "Stage 3 foreign notification",
                Message = "Must remain private"
            };
            db.AddRange(owned, foreign);
            await db.SaveChangesAsync(ct);
            ownedId = owned.Id;
            foreignId = foreign.Id;
        }

        using var reader = await LoginAsync(ReaderEmail, ReaderPassword);
        var page = await reader.GetAsync("/notifications", ct);
        Assert.Equal(HttpStatusCode.OK, page.StatusCode);
        Assert.Contains("Stage 3 owned notification", await page.Content.ReadAsStringAsync(ct));
        Assert.Equal(HttpStatusCode.NoContent,
            (await reader.PostAsync($"/api/v1/notifications/{ownedId}/read", null, ct)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await reader.PostAsync($"/api/v1/notifications/{foreignId}/read", null, ct)).StatusCode);

        await using var finalScope = factory.Services.CreateAsyncScope();
        var finalDb = finalScope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        Assert.True(await finalDb.Notifications.Where(x => x.Id == ownedId).Select(x => x.IsRead).SingleAsync(ct));
        Assert.False(await finalDb.Notifications.Where(x => x.Id == foreignId).Select(x => x.IsRead).SingleAsync(ct));
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Support_ticket_is_scoped_and_staff_can_claim_reply_and_close()
    {
        var ct = TestContext.Current.CancellationToken;
        using var reader = await LoginAsync(ReaderEmail, ReaderPassword);
        var created = await reader.PostAsJsonAsync("/api/v1/support/tickets", new
        {
            subject = "Stage 3 support",
            message = "Reader needs help"
        }, ct);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var ticketId = (await created.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct))
            .GetProperty("id").GetInt32();
        Assert.Equal(HttpStatusCode.OK, (await reader.GetAsync($"/api/v1/support/tickets/{ticketId}", ct)).StatusCode);

        using var staff = await LoginAsync(StaffEmail, StaffPassword);
        Assert.Equal(HttpStatusCode.NoContent,
            (await staff.PostAsync($"/api/v1/support/tickets/{ticketId}/claim", null, ct)).StatusCode);
        Assert.Equal(HttpStatusCode.OK,
            (await staff.PostAsJsonAsync($"/api/v1/support/tickets/{ticketId}/messages",
                new { content = "Staff answered" }, ct)).StatusCode);
        var supportPage = await reader.GetAsync($"/support?ticketId={ticketId}", ct);
        var supportHtml = await supportPage.Content.ReadAsStringAsync(ct);
        Assert.True(supportPage.StatusCode == HttpStatusCode.OK,
            $"/support returned {(int)supportPage.StatusCode}: {supportHtml}");
        Assert.Contains("Staff answered", supportHtml);
        Assert.Equal(HttpStatusCode.NoContent,
            (await staff.PostAsync($"/api/v1/support/tickets/{ticketId}/close", null, ct)).StatusCode);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        Assert.Equal(SupportTicketStatus.Closed,
            await db.SupportTickets.Where(x => x.Id == ticketId).Select(x => x.Status).SingleAsync(ct));
        Assert.Equal(2, await db.SupportMessages.CountAsync(x => x.SupportTicketId == ticketId, ct));
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Community_actions_persist_real_reader_data()
    {
        var ct = TestContext.Current.CancellationToken;
        var book = await CreateUniqueBookAsync("Stage 3 community");
        using var reader = await LoginAsync(ReaderEmail, ReaderPassword);
        Assert.Equal(HttpStatusCode.NoContent,
            (await reader.PostAsJsonAsync($"/api/v1/books/{book.Id}/ratings",
                new { stars = 5, review = "Great" }, ct)).StatusCode);
        Assert.Equal(HttpStatusCode.Created,
            (await reader.PostAsJsonAsync($"/api/v1/books/{book.Id}/comments",
                new { parentId = (int?)null, content = "Stage 3 real comment" }, ct)).StatusCode);
        Assert.Equal(HttpStatusCode.OK,
            (await reader.PostAsync($"/api/v1/books/{book.Id}/favorite", null, ct)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent,
            (await reader.PostAsJsonAsync($"/api/v1/reading/{book.Id}",
                new { currentPage = 1, totalPages = 1 }, ct)).StatusCode);
        var page = await reader.GetAsync($"/books/{book.Id}", ct);
        Assert.Contains("Stage 3 real comment", await page.Content.ReadAsStringAsync(ct));

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        var readerId = await db.Readers.Where(x => x.Email == ReaderEmail).Select(x => x.Id).SingleAsync(ct);
        Assert.True(await db.BookRatings.AnyAsync(x => x.BookId == book.Id && x.ReaderId == readerId && x.Stars == 5, ct));
        Assert.True(await db.BookFavorites.AnyAsync(x => x.BookId == book.Id && x.ReaderId == readerId, ct));
        Assert.True(await db.ReadingHistories.AnyAsync(x => x.BookId == book.Id && x.ReaderId == readerId, ct));
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Safe_cover_upload_rejects_spoofed_file_and_accepts_signature()
    {
        var ct = TestContext.Current.CancellationToken;
        using var staff = await LoginAsync(StaffEmail, StaffPassword);
        using var badContent = new MultipartFormDataContent();
        badContent.Add(new ByteArrayContent("not an image"u8.ToArray()), "file", "../../cover.png");
        var bad = await staff.PostAsync("/api/v1/uploads/book-cover", badContent, ct);
        Assert.Equal(HttpStatusCode.BadRequest, bad.StatusCode);

        using var validContent = new MultipartFormDataContent();
        validContent.Add(new ByteArrayContent(
            new byte[] { 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a, 0, 0, 0, 0 }),
            "file", "cover.png");
        var valid = await staff.PostAsync("/api/v1/uploads/book-cover", validContent, ct);
        Assert.Equal(HttpStatusCode.OK, valid.StatusCode);
        var path = (await valid.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct))
            .GetProperty("path").GetString();
        Assert.Matches("^/uploads/book-covers/book-[a-f0-9]{32}\\.png$", path!);
        Assert.DoesNotContain("cover.png", path);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Api_mutations_require_same_origin_request_marker()
    {
        using var client = Client();
        client.DefaultRequestHeaders.Remove("X-LibraryHub-Request");
        var rejected = await client.PostAsJsonAsync(
            "/api/v1/help/chat",
            new { question = "Kiểm tra bảo vệ yêu cầu" },
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Forbidden, rejected.StatusCode);

        client.DefaultRequestHeaders.Add("X-LibraryHub-Request", "XMLHttpRequest");
        var accepted = await client.PostAsJsonAsync(
            "/api/v1/help/chat",
            new { question = "Hướng dẫn mượn sách" },
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, accepted.StatusCode);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Concurrency_conflicts_return_safe_vietnamese_problem_details()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/v1/readers/1";
        context.Response.Body = new MemoryStream();
        var handled = await new DomainRuleExceptionHandler().TryHandleAsync(
            context,
            new DbUpdateConcurrencyException("database detail must not be returned"),
            TestContext.Current.CancellationToken);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status409Conflict, context.Response.StatusCode);
        context.Response.Body.Position = 0;
        var problem = await JsonSerializer.DeserializeAsync<JsonElement>(
            context.Response.Body,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal("concurrency.conflict", problem.GetProperty("code").GetString());
        Assert.DoesNotContain("database detail", problem.GetProperty("detail").GetString());
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Settings_validation_is_admin_only_and_does_not_accept_invalid_values()
    {
        var ct = TestContext.Current.CancellationToken;
        using var reader = await LoginAsync(ReaderEmail, ReaderPassword);
        Assert.Equal(HttpStatusCode.Forbidden,
            (await reader.PutAsJsonAsync("/api/v1/settings", new SystemSettings(), ct)).StatusCode);

        using var admin = await LoginAsync(AdminEmail, AdminPassword);
        var invalid = await admin.PutAsJsonAsync("/api/v1/settings", new
        {
            maxExtensionDays = 0,
            maxExtensionCount = 1,
            lateFeePerDay = 5000,
            maxBorrowBooks = 10,
            borrowPeriodDays = 14,
            pickupWindowDays = 3
        }, ct);
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        Assert.Equal("application/problem+json", invalid.Content.Headers.ContentType?.MediaType);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        var current = await db.SystemSettings.AsNoTracking().SingleOrDefaultAsync(ct) ?? new SystemSettings();
        Assert.Equal(HttpStatusCode.NoContent,
            (await admin.PutAsJsonAsync("/api/v1/settings", current, ct)).StatusCode);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task External_login_without_secrets_is_hidden_and_rejects_unknown_provider_safely()
    {
        var ct = TestContext.Current.CancellationToken;
        using var client = Client();
        var login = await client.GetAsync("/login?returnUrl=https%3A%2F%2Fevil.example", ct);
        var html = await login.Content.ReadAsStringAsync(ct);

        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        Assert.Contains("Google và Facebook cần được quản trị viên cấu hình", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("name=\"Provider\"", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Tiếp tục với Google", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Tiếp tục với Facebook", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("value=\"Google\"", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("value=\"Facebook\"", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("evil.example", html, StringComparison.OrdinalIgnoreCase);

        var rejected = await client.PostAsync("/external-login", new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = AntiForgeryToken(html),
                ["Provider"] = "UnknownProvider",
                ["ReturnUrl"] = "https://evil.example/steal"
            }), ct);

        Assert.Equal(HttpStatusCode.Redirect, rejected.StatusCode);
        Assert.DoesNotContain("evil.example", rejected.Headers.Location?.OriginalString ?? "",
            StringComparison.OrdinalIgnoreCase);
        Assert.StartsWith("/login", rejected.Headers.Location?.OriginalString ?? "", StringComparison.Ordinal);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task External_login_remote_error_is_localized_and_blocks_open_redirect()
    {
        var ct = TestContext.Current.CancellationToken;
        using var client = Client();
        var response = await client.GetAsync(
            "/external-login/callback?remoteError=Google&returnUrl=https%3A%2F%2Fevil.example", ct);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.DoesNotContain("evil.example", response.Headers.Location?.OriginalString ?? "",
            StringComparison.OrdinalIgnoreCase);

        var login = await client.GetAsync(response.Headers.Location, ct);
        var html = WebUtility.HtmlDecode(await login.Content.ReadAsStringAsync(ct));
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        Assert.Contains("Đăng nhập bằng nhà cung cấp ngoài đã bị hủy hoặc không thể hoàn tất.", html);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task OAuth_schemes_are_registered_only_with_complete_configuration_and_use_expected_callbacks()
    {
        using var configuredFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Authentication:Google:ClientId", "test-google-client-id");
            builder.UseSetting("Authentication:Google:ClientSecret", "test-google-client-secret");
            builder.UseSetting("Authentication:Facebook:AppId", "test-facebook-app-id");
            builder.UseSetting("Authentication:Facebook:AppSecret", "test-facebook-app-secret");
        });
        await using var scope = configuredFactory.Services.CreateAsyncScope();
        var schemes = await scope.ServiceProvider
            .GetRequiredService<IAuthenticationSchemeProvider>()
            .GetAllSchemesAsync();
        var google = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<GoogleOptions>>().Get("Google");
        var facebook = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<FacebookOptions>>().Get("Facebook");

        Assert.Contains(schemes, x => x.Name == "Google");
        Assert.Contains(schemes, x => x.Name == "Facebook");
        Assert.Equal("/signin-google", google.CallbackPath);
        Assert.Equal("/signin-facebook", facebook.CallbackPath);
        Assert.False(google.SaveTokens);
        Assert.False(facebook.SaveTokens);
        Assert.Equal(SameSiteMode.None, google.CorrelationCookie.SameSite);
        Assert.Equal(SameSiteMode.None, facebook.CorrelationCookie.SameSite);
        Assert.Equal(CookieSecurePolicy.Always, google.CorrelationCookie.SecurePolicy);
        Assert.Equal(CookieSecurePolicy.Always, facebook.CorrelationCookie.SecurePolicy);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Verified_google_login_creates_reader_and_persists_provider_login_without_elevating_roles()
    {
        var ct = TestContext.Current.CancellationToken;
        const string provider = "Google";
        var normalizedProvider = provider.ToLowerInvariant();
        var email = $"integration.oauth.{normalizedProvider}@libraryhub.local";

        await DeleteExternalTestAccountAsync(email, ct);
        using var oauthFactory = factory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services =>
            {
                services.AddAuthentication()
                    .AddScheme<AuthenticationSchemeOptions, TestExternalAuthenticationHandler>(
                        "Google",
                        "Google",
                        options => options.ClaimsIssuer = "verified-email")
                    .AddScheme<AuthenticationSchemeOptions, TestExternalAuthenticationHandler>(
                        "Facebook",
                        "Facebook",
                        options => options.ClaimsIssuer = "unverified-email");
            }));

        try
        {
            using (var client = oauthFactory.CreateClient(new WebApplicationFactoryClientOptions
                   {
                       BaseAddress = new Uri("https://localhost"),
                       AllowAutoRedirect = false,
                       HandleCookies = true
                   }))
            {
                var loginHtml = await (await client.GetAsync("/login", ct)).Content.ReadAsStringAsync(ct);
                Assert.Contains($"value=\"{provider}\"", loginHtml, StringComparison.OrdinalIgnoreCase);

                var challenge = await client.PostAsync("/external-login", new FormUrlEncodedContent(
                    new Dictionary<string, string>
                    {
                        ["__RequestVerificationToken"] = AntiForgeryToken(loginHtml),
                        ["Provider"] = provider,
                        ["ReturnUrl"] = "/me"
                    }), ct);
                Assert.Equal(HttpStatusCode.Redirect, challenge.StatusCode);

                var confirmation = await client.GetAsync(challenge.Headers.Location, ct);
                var confirmationHtml = await confirmation.Content.ReadAsStringAsync(ct);
                Assert.Equal(HttpStatusCode.OK, confirmation.StatusCode);
                Assert.Contains($"Email từ {provider}", confirmationHtml);

                var confirmed = await client.PostAsync("/external-login/confirm", new FormUrlEncodedContent(
                    new Dictionary<string, string>
                    {
                        ["__RequestVerificationToken"] = AntiForgeryToken(confirmationHtml),
                        ["DisplayName"] = $"Độc giả {provider}",
                        ["ReaderType"] = "0",
                        ["ReturnUrl"] = "/me"
                    }), ct);
                Assert.Equal(HttpStatusCode.Redirect, confirmed.StatusCode);
                Assert.Equal("/me", confirmed.Headers.Location?.OriginalString);
                Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/me", ct)).StatusCode);
            }

            DateTimeOffset firstLoginAt;
            await using (var scope = oauthFactory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
                var user = await db.Users.SingleAsync(x => x.Email == email, ct);
                var reader = await db.Readers.SingleAsync(x => x.UserAccountId == user.Id, ct);
                var externalLogin = await db.UserLogins.SingleAsync(
                    x => x.UserId == user.Id && x.LoginProvider == provider,
                    ct);

                Assert.Equal(AccountKind.Reader, user.AccountKind);
                Assert.True(user.EmailConfirmed);
                Assert.Null(user.PasswordHash);
                Assert.True(await db.UserRoles.AnyAsync(x => x.UserId == user.Id, ct));
                var roleNames = await (
                    from userRole in db.UserRoles
                    join role in db.Roles on userRole.RoleId equals role.Id
                    where userRole.UserId == user.Id
                    select role.Name).ToListAsync(ct);
                Assert.Single(roleNames);
                Assert.Equal("Reader", roleNames[0]);
                Assert.Equal(email, reader.Email);
                Assert.True(reader.EmailConfirmed);
                Assert.Equal($"test-provider-key-{normalizedProvider}", externalLogin.ProviderKey);
                Assert.NotNull(user.LastLoginAt);
                Assert.NotNull(reader.LastLoginAt);
                firstLoginAt = user.LastLoginAt!.Value;
            }

            await Task.Delay(20, ct);
            using (var returningClient = oauthFactory.CreateClient(new WebApplicationFactoryClientOptions
                   {
                       BaseAddress = new Uri("https://localhost"),
                       AllowAutoRedirect = false,
                       HandleCookies = true
                   }))
            {
                var loginHtml = await (await returningClient.GetAsync("/login", ct)).Content.ReadAsStringAsync(ct);
                var challenge = await returningClient.PostAsync("/external-login", new FormUrlEncodedContent(
                    new Dictionary<string, string>
                    {
                        ["__RequestVerificationToken"] = AntiForgeryToken(loginHtml),
                        ["Provider"] = provider
                    }), ct);
                var callback = await returningClient.GetAsync(challenge.Headers.Location, ct);
                Assert.Equal(HttpStatusCode.Redirect, callback.StatusCode);
                Assert.Equal("/me", callback.Headers.Location?.OriginalString);
            }

            await using (var scope = oauthFactory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
                var user = await db.Users.SingleAsync(x => x.Email == email, ct);
                Assert.True(user.LastLoginAt >= firstLoginAt);
                Assert.Equal(1, await db.UserLogins.CountAsync(x => x.UserId == user.Id, ct));
            }
        }
        finally
        {
            await DeleteExternalTestAccountAsync(email, CancellationToken.None);
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Facebook_email_is_not_auto_confirmed_and_linked_login_works_after_local_confirmation()
    {
        var ct = TestContext.Current.CancellationToken;
        const string provider = "Facebook";
        const string email = "integration.oauth.facebook@libraryhub.local";
        await DeleteExternalTestAccountAsync(email, ct);
        using var oauthFactory = ExternalTestFactory("verified-email", "unverified-email");

        try
        {
            Guid userId;
            using (var client = OAuthClient(oauthFactory))
            {
                var loginHtml = await (await client.GetAsync("/login", ct)).Content.ReadAsStringAsync(ct);
                var challenge = await StartExternalLoginAsync(client, loginHtml, provider, "/me", ct);
                var confirmation = await client.GetAsync(challenge.Headers.Location, ct);
                var confirmationHtml = await confirmation.Content.ReadAsStringAsync(ct);
                Assert.Equal(HttpStatusCode.OK, confirmation.StatusCode);

                var created = await client.PostAsync("/external-login/confirm", new FormUrlEncodedContent(
                    new Dictionary<string, string>
                    {
                        ["__RequestVerificationToken"] = AntiForgeryToken(confirmationHtml),
                        ["DisplayName"] = "Độc giả Facebook",
                        ["ReaderType"] = "0",
                        ["ReturnUrl"] = "/me"
                    }), ct);
                Assert.Equal(HttpStatusCode.Redirect, created.StatusCode);
                Assert.StartsWith("/login", created.Headers.Location?.OriginalString ?? "", StringComparison.Ordinal);
                Assert.Equal(HttpStatusCode.Redirect, (await client.GetAsync("/me", ct)).StatusCode);
            }

            await using (var scope = oauthFactory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
                var user = await db.Users.SingleAsync(x => x.Email == email, ct);
                var reader = await db.Readers.SingleAsync(x => x.UserAccountId == user.Id, ct);
                userId = user.Id;
                Assert.False(user.EmailConfirmed);
                Assert.False(reader.EmailConfirmed);
                Assert.Equal(1, await db.UserLogins.CountAsync(x => x.UserId == user.Id, ct));
            }

            string confirmationToken;
            await using (var scope = oauthFactory.Services.CreateAsyncScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<LibraryUser>>();
                var user = await userManager.FindByIdAsync(userId.ToString());
                Assert.NotNull(user);
                confirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
            }

            using (var confirmationClient = OAuthClient(oauthFactory))
            {
                var confirmed = await confirmationClient.GetAsync(
                    $"/confirm-email?userId={userId:D}&token={Uri.EscapeDataString(confirmationToken)}", ct);
                Assert.Equal(HttpStatusCode.OK, confirmed.StatusCode);
            }

            using (var returningClient = OAuthClient(oauthFactory))
            {
                var loginHtml = await (await returningClient.GetAsync("/login", ct)).Content.ReadAsStringAsync(ct);
                var challenge = await StartExternalLoginAsync(returningClient, loginHtml, provider, "/me", ct);
                var callback = await returningClient.GetAsync(challenge.Headers.Location, ct);
                Assert.Equal(HttpStatusCode.Redirect, callback.StatusCode);
                Assert.Equal("/me", callback.Headers.Location?.OriginalString);
                Assert.Equal(HttpStatusCode.OK, (await returningClient.GetAsync("/me", ct)).StatusCode);
            }
        }
        finally
        {
            await DeleteExternalTestAccountAsync(email, CancellationToken.None);
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task External_provider_without_email_is_rejected_without_creating_an_account()
    {
        var ct = TestContext.Current.CancellationToken;
        const string email = "integration.oauth.google@libraryhub.local";
        await DeleteExternalTestAccountAsync(email, ct);
        using var oauthFactory = ExternalTestFactory("missing-email", "unverified-email");
        using var client = OAuthClient(oauthFactory);
        var loginHtml = await (await client.GetAsync("/login", ct)).Content.ReadAsStringAsync(ct);
        var challenge = await StartExternalLoginAsync(client, loginHtml, "Google", null, ct);
        var callback = await client.GetAsync(challenge.Headers.Location, ct);
        var html = WebUtility.HtmlDecode(await callback.Content.ReadAsStringAsync(ct));

        Assert.Equal(HttpStatusCode.OK, callback.StatusCode);
        Assert.Contains("không trả địa chỉ email", html, StringComparison.OrdinalIgnoreCase);
        await using var scope = oauthFactory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        Assert.False(await db.Users.AnyAsync(x => x.Email == email, ct));
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Help_center_changes_content_and_links_by_role()
    {
        var ct = TestContext.Current.CancellationToken;
        using var anonymous = Client();
        Assert.Equal(HttpStatusCode.Redirect, (await anonymous.GetAsync("/help", ct)).StatusCode);

        using var reader = await LoginAsync(ReaderEmail, ReaderPassword);
        var readerHtml = WebUtility.HtmlDecode(
            await (await reader.GetAsync("/help", ct)).Content.ReadAsStringAsync(ct));
        Assert.Contains("Trung tâm trợ giúp · Độc giả", readerHtml);
        Assert.Contains("href=\"/me/cart\"", readerHtml);
        Assert.DoesNotContain("href=\"/admin/rbac\"", readerHtml);

        using var staff = await LoginAsync(StaffEmail, StaffPassword);
        var staffHtml = WebUtility.HtmlDecode(
            await (await staff.GetAsync("/help", ct)).Content.ReadAsStringAsync(ct));
        Assert.Contains("Trung tâm trợ giúp · Nhân viên", staffHtml);
        Assert.Contains("Trạng thái nghiệp vụ", staffHtml);
        Assert.DoesNotContain("href=\"/admin/rbac\"", staffHtml);

        using var admin = await LoginAsync(AdminEmail, AdminPassword);
        var adminHtml = WebUtility.HtmlDecode(
            await (await admin.GetAsync("/help", ct)).Content.ReadAsStringAsync(ct));
        Assert.Contains("Trung tâm trợ giúp · Quản trị viên", adminHtml);
        Assert.Contains("href=\"/admin/rbac\"", adminHtml);
        Assert.Contains("Cảnh báo an toàn", adminHtml);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Pickup_expiration_worker_stops_cleanly_when_host_is_cancelled()
    {
        using var worker = new PickupExpirationWorker(
            factory.Services.GetRequiredService<IServiceScopeFactory>(),
            factory.Services.GetRequiredService<ILogger<PickupExpirationWorker>>());

        await worker.StartAsync(CancellationToken.None);
        await Task.Delay(TimeSpan.FromMilliseconds(500), CancellationToken.None);
        await worker.StopAsync(CancellationToken.None);

        Assert.NotNull(worker.ExecuteTask);
        await worker.ExecuteTask.WaitAsync(TimeSpan.FromSeconds(5), CancellationToken.None);
        Assert.True(
            worker.ExecuteTask.IsCompletedSuccessfully,
            $"Worker kết thúc với trạng thái {worker.ExecuteTask.Status}.");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Suspended_book_is_removed_from_public_borrowable_catalog()
    {
        var ct = TestContext.Current.CancellationToken;
        var book = await CreateUniqueBookAsync("Suspended public catalog");
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            var adminId = await db.Users.Where(x => x.Email == AdminEmail).Select(x => x.Id).SingleAsync(ct);
            await scope.ServiceProvider.GetRequiredService<IBookLifecycleService>().SuspendAsync(new(
                book.Id,
                adminId,
                BookSuspensionType.Normal,
                "Ngừng phục vụ để kiểm thử danh mục công khai",
                null,
                $"integration-public-{Guid.NewGuid():N}"), ct);
        }

        using var client = Client();
        var api = await client.GetStringAsync($"/api/v1/books?q={Uri.EscapeDataString(book.Title)}", ct);
        var home = await client.GetStringAsync($"/?q={Uri.EscapeDataString(book.Title)}", ct);

        Assert.DoesNotContain(book.Title, api);
        Assert.DoesNotContain($"href=\"/books/{book.Id}\"", home);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Reader_cannot_reserve_a_suspended_book()
    {
        var ct = TestContext.Current.CancellationToken;
        var book = await CreateUniqueBookAsync("Suspended reservation");
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            book = await db.Books.SingleAsync(x => x.Id == book.Id, ct);
            book.Status = BookStatus.Discontinued;
            book.SuspensionType = BookSuspensionType.Normal;
            book.SuspensionReason = "Kiểm thử chặn đặt trước";
            book.SuspendedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(ct);
        }

        using var reader = await LoginAsync(ReaderEmail, ReaderPassword);
        var response = await reader.PostAsJsonAsync(
            "/api/v1/reservations",
            new { bookId = book.Id, note = "Phải bị chặn" },
            ct);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        Assert.Equal("book.unavailable", problem.GetProperty("code").GetString());
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Staff_without_emergency_permission_is_denied_by_direct_endpoint_call()
    {
        var ct = TestContext.Current.CancellationToken;
        var book = await CreateUniqueBookAsync("Emergency authorization");
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            var reader = await db.Readers.SingleAsync(x => x.Email == ReaderEmail, ct);
            db.Add(new BorrowSlip
            {
                Code = $"BR-I-{Guid.NewGuid():N}"[..21].ToUpperInvariant(),
                ReaderId = reader.Id,
                TotalBooks = 1,
                RequestedAt = DateTimeOffset.UtcNow,
                BorrowedAt = DateTimeOffset.UtcNow,
                DueAt = DateTimeOffset.UtcNow.AddDays(7),
                Status = BorrowStatus.Borrowing,
                CreatedByAccountId = reader.UserAccountId,
                IdempotencyKey = $"emergency-auth-{Guid.NewGuid():N}",
                Details = [new BorrowDetail { BookId = book.Id, Quantity = 1, BorrowPrice = book.Price }]
            });
            await db.SaveChangesAsync(ct);
        }

        using var staff = await LoginAsync(StaffEmail, StaffPassword);
        using var request = LifecycleRequest(
            $"/api/v1/books/{book.Id}/suspend",
            new { type = (int)BookSuspensionType.EmergencyRecall, reason = "Kiểm thử phân quyền", recallDeadline = (DateTimeOffset?)null });
        var response = await staff.SendAsync(request, ct);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        await using var verifyScope = factory.Services.CreateAsyncScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        Assert.NotEqual(
            BookStatus.Discontinued,
            await verifyDb.Books.Where(x => x.Id == book.Id).Select(x => x.Status).SingleAsync(ct));
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Concurrent_suspension_requests_do_not_corrupt_stock_or_duplicate_transition()
    {
        var ct = TestContext.Current.CancellationToken;
        var book = await CreateUniqueBookAsync("Concurrent lifecycle");
        using var firstAdmin = await LoginAsync(AdminEmail, AdminPassword);
        using var secondAdmin = await LoginAsync(AdminEmail, AdminPassword);
        using var firstRequest = LifecycleRequest(
            $"/api/v1/books/{book.Id}/suspend",
            new { type = (int)BookSuspensionType.Normal, reason = "Yêu cầu đồng thời thứ nhất", recallDeadline = (DateTimeOffset?)null });
        using var secondRequest = LifecycleRequest(
            $"/api/v1/books/{book.Id}/suspend",
            new { type = (int)BookSuspensionType.Normal, reason = "Yêu cầu đồng thời thứ hai", recallDeadline = (DateTimeOffset?)null });

        var responses = await Task.WhenAll(
            firstAdmin.SendAsync(firstRequest, ct),
            secondAdmin.SendAsync(secondRequest, ct));

        Assert.Single(responses, x => x.StatusCode == HttpStatusCode.OK);
        Assert.Single(responses, x => x.StatusCode == HttpStatusCode.Conflict);
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        var stored = await db.Books.AsNoTracking().SingleAsync(x => x.Id == book.Id, ct);
        Assert.Equal(BookStatus.Discontinued, stored.Status);
        Assert.Equal(5, stored.TotalQuantity);
        Assert.Equal(5, stored.AvailableQuantity);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Borrow_cart_requires_login_and_password_login_consumes_pending_action_once()
    {
        var ct = TestContext.Current.CancellationToken;
        var book = await CreateUniqueBookAsync("Pending borrow cart");
        using var client = Client();
        var detailHtml = await (await client.GetAsync($"/books/{book.Id}", ct))
            .Content.ReadAsStringAsync(ct);

        var anonymousAdd = await client.PostAsync(
            "/borrow-cart/add",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = AntiForgeryToken(detailHtml),
                ["BookId"] = book.Id.ToString(),
                ["ReturnUrl"] = $"/books/{book.Id}"
            }),
            ct);

        Assert.Equal(HttpStatusCode.Unauthorized, anonymousAdd.StatusCode);
        var authentication = await anonymousAdd.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        Assert.True(authentication.GetProperty("requiresAuthentication").GetBoolean());
        var loginUrl = authentication.GetProperty("loginUrl").GetString();
        Assert.NotNull(loginUrl);
        Assert.Contains("%2Fborrow-cart%2Fcontinue", loginUrl, StringComparison.OrdinalIgnoreCase);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            Assert.False(await db.BorrowSlips.AnyAsync(x => x.Details.Any(y => y.BookId == book.Id), ct));
        }

        var loginHtml = await (await client.GetAsync(loginUrl, ct)).Content.ReadAsStringAsync(ct);
        var failedLogin = await client.PostAsync("/login", new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = AntiForgeryToken(loginHtml),
                ["Email"] = ReaderEmail,
                ["Password"] = "SaiMatKhau2026",
                ["RememberMe"] = "false",
                ["ReturnUrl"] = "/borrow-cart/continue"
            }), ct);
        Assert.Equal(HttpStatusCode.OK, failedLogin.StatusCode);
        Assert.Contains("Email hoặc mật khẩu không đúng", WebUtility.HtmlDecode(
            await failedLogin.Content.ReadAsStringAsync(ct)));

        var failedHtml = await failedLogin.Content.ReadAsStringAsync(ct);
        var successfulLogin = await client.PostAsync("/login", new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = AntiForgeryToken(failedHtml),
                ["Email"] = ReaderEmail,
                ["Password"] = ReaderPassword,
                ["RememberMe"] = "false",
                ["ReturnUrl"] = "/borrow-cart/continue"
            }), ct);
        Assert.Equal(HttpStatusCode.Redirect, successfulLogin.StatusCode);
        Assert.Equal("/borrow-cart/continue", successfulLogin.Headers.Location?.OriginalString);

        var continuation = await client.GetAsync(successfulLogin.Headers.Location, ct);
        var continuationHtml = await continuation.Content.ReadAsStringAsync(ct);
        Assert.Equal(HttpStatusCode.OK, continuation.StatusCode);
        Assert.Contains($"data-book-id=\"{book.Id}\"", continuationHtml);
        Assert.Contains("data-can-add=\"true\"", continuationHtml);
        Assert.Contains(book.Title, continuationHtml);

        var replay = await client.GetAsync("/borrow-cart/continue", ct);
        Assert.Equal(HttpStatusCode.Redirect, replay.StatusCode);
        Assert.Equal("/", replay.Headers.Location?.OriginalString);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Borrow_cart_endpoint_enforces_antiforgery_authentication_lockout_and_stock()
    {
        var ct = TestContext.Current.CancellationToken;
        var book = await CreateUniqueBookAsync("Validated borrow cart");

        using (var directClient = Client())
        {
            var direct = await directClient.PostAsync(
                "/borrow-cart/add",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["BookId"] = book.Id.ToString(),
                    ["ReturnUrl"] = $"/books/{book.Id}"
                }), ct);
            Assert.Equal(HttpStatusCode.BadRequest, direct.StatusCode);
        }

        using var reader = await LoginAsync(ReaderEmail, ReaderPassword);
        var detailHtml = await (await reader.GetAsync($"/books/{book.Id}", ct))
            .Content.ReadAsStringAsync(ct);
        var token = AntiForgeryToken(detailHtml);
        var available = await reader.PostAsync("/borrow-cart/add", new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["BookId"] = book.Id.ToString(),
                ["ReturnUrl"] = $"/books/{book.Id}"
            }), ct);
        Assert.Equal(HttpStatusCode.OK, available.StatusCode);
        var item = await available.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        Assert.Equal(book.Id, item.GetProperty("bookId").GetInt32());

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<LibraryUser>>();
            var account = await users.FindByEmailAsync(ReaderEmail);
            Assert.NotNull(account);
            Assert.True((await users.SetLockoutEndDateAsync(
                account,
                DateTimeOffset.UtcNow.AddMinutes(15))).Succeeded);
        }
        try
        {
            var locked = await reader.PostAsync("/borrow-cart/add", new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["__RequestVerificationToken"] = token,
                    ["BookId"] = book.Id.ToString(),
                    ["ReturnUrl"] = $"/books/{book.Id}"
                }), ct);
            Assert.Equal(HttpStatusCode.BadRequest, locked.StatusCode);
            Assert.Contains("tạm khóa", WebUtility.HtmlDecode(await locked.Content.ReadAsStringAsync(ct)));
        }
        finally
        {
            await using var scope = factory.Services.CreateAsyncScope();
            var users = scope.ServiceProvider.GetRequiredService<UserManager<LibraryUser>>();
            var account = await users.FindByEmailAsync(ReaderEmail);
            Assert.NotNull(account);
            Assert.True((await users.SetLockoutEndDateAsync(account, null)).Succeeded);
        }

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            await db.Books.Where(x => x.Id == book.Id).ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(x => x.AvailableQuantity, 0)
                    .SetProperty(x => x.Status, BookStatus.OutOfStock), ct);
        }
        var outOfStock = await reader.PostAsync("/borrow-cart/add", new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["BookId"] = book.Id.ToString(),
                ["ReturnUrl"] = $"/books/{book.Id}"
            }), ct);
        Assert.Equal(HttpStatusCode.Conflict, outOfStock.StatusCode);
        Assert.Contains("không còn bản khả dụng", WebUtility.HtmlDecode(
            await outOfStock.Content.ReadAsStringAsync(ct)));
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Reader_registration_preserves_pending_borrow_cart_until_confirmed_login()
    {
        var ct = TestContext.Current.CancellationToken;
        var book = await CreateUniqueBookAsync("Registered borrow cart");
        var email = $"borrow.cart.{Guid.NewGuid():N}@libraryhub.local";
        const string password = "DocGiaCart2026";
        using var client = Client();
        var detailHtml = await (await client.GetAsync($"/books/{book.Id}", ct))
            .Content.ReadAsStringAsync(ct);
        var pending = await client.PostAsync("/borrow-cart/add", new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = AntiForgeryToken(detailHtml),
                ["BookId"] = book.Id.ToString(),
                ["ReturnUrl"] = $"/books/{book.Id}"
            }), ct);
        Assert.Equal(HttpStatusCode.Unauthorized, pending.StatusCode);

        var registerHtml = await (await client.GetAsync(
            "/register?returnUrl=%2Fborrow-cart%2Fcontinue", ct)).Content.ReadAsStringAsync(ct);
        var registered = await client.PostAsync("/register", new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = AntiForgeryToken(registerHtml),
                ["ReturnUrl"] = "/borrow-cart/continue",
                ["FullName"] = "Độc giả giỏ mượn",
                ["Email"] = email,
                ["Password"] = password,
                ["ConfirmPassword"] = password,
                ["ReaderType"] = "0"
            }), ct);
        Assert.Equal(HttpStatusCode.Redirect, registered.StatusCode);
        Assert.Contains("returnUrl=%2Fborrow-cart%2Fcontinue",
            registered.Headers.Location?.OriginalString ?? "", StringComparison.OrdinalIgnoreCase);

        Guid userId;
        string confirmationToken;
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<LibraryUser>>();
            var account = await users.FindByEmailAsync(email);
            Assert.NotNull(account);
            userId = account.Id;
            confirmationToken = await users.GenerateEmailConfirmationTokenAsync(account);
        }

        try
        {
            var confirmed = await client.GetAsync(
                $"/confirm-email?userId={userId:D}&token={Uri.EscapeDataString(confirmationToken)}", ct);
            Assert.Equal(HttpStatusCode.OK, confirmed.StatusCode);

            var loginHtml = await (await client.GetAsync("/login", ct)).Content.ReadAsStringAsync(ct);
            Assert.Contains("value=\"/borrow-cart/continue\"", loginHtml);
            var login = await client.PostAsync("/login", new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["__RequestVerificationToken"] = AntiForgeryToken(loginHtml),
                    ["Email"] = email,
                    ["Password"] = password,
                    ["RememberMe"] = "false",
                    ["ReturnUrl"] = "/borrow-cart/continue"
                }), ct);
            Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);
            Assert.Equal("/borrow-cart/continue", login.Headers.Location?.OriginalString);

            var continuation = await client.GetAsync(login.Headers.Location, ct);
            Assert.Equal(HttpStatusCode.OK, continuation.StatusCode);
            Assert.Contains($"data-book-id=\"{book.Id}\"",
                await continuation.Content.ReadAsStringAsync(ct));
        }
        finally
        {
            await using var scope = factory.Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            await db.Readers.Where(x => x.UserAccountId == userId).ExecuteDeleteAsync(ct);
            var users = scope.ServiceProvider.GetRequiredService<UserManager<LibraryUser>>();
            var account = await users.FindByIdAsync(userId.ToString());
            if (account is not null) Assert.True((await users.DeleteAsync(account)).Succeeded);
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Book_service_lifecycle_migration_is_applied_without_recreating_database()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var db = NewDb();
        var migrations = await db.Database.GetAppliedMigrationsAsync(ct);

        Assert.Contains(migrations, x => x.EndsWith("_BookServiceLifecycle", StringComparison.Ordinal));
        Assert.True(await db.Users.AnyAsync(x => x.Email == ReaderEmail, ct));
        Assert.True(await db.Books.AnyAsync(ct));
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Security_concurrency_and_catalog_integrity_guards_hold_end_to_end()
    {
        var ct = TestContext.Current.CancellationToken;
        var admin = await LoginAsync(AdminEmail, AdminPassword);
        var readerClient = await LoginAsync(ReaderEmail, ReaderPassword);

        int? bookId = null;
        int? categoryId = null;
        int? empId = null;
        Guid? empUserId = null;
        int? reservationReaderId = null;

        try
        {
            await using (var scope = factory.Services.CreateAsyncScope())
            {
                var sp = scope.ServiceProvider;
                var db = sp.GetRequiredService<LibraryDbContext>();
                var users = sp.GetRequiredService<UserManager<LibraryUser>>();

                var category = new Category { Name = $"=SUM(1,1) TestCat {Guid.NewGuid():N}"[..36] };
                var author = await db.Authors.FirstAsync(ct);
                var publisher = await db.Publishers.FirstAsync(ct);
                var shelf = await db.Shelves.FirstAsync(ct);
                db.Add(category);
                await db.SaveChangesAsync(ct);
                categoryId = category.Id;

                var book = new Book
                {
                    Isbn = $"978{Random.Shared.Next(100000000, 999999999)}",
                    Title = "=HYPERLINK(\"http://evil\",\"FormulaBook\")",
                    Language = "vi",
                    Price = 100_000,
                    TotalQuantity = 2,
                    AvailableQuantity = 2,
                    CategoryId = category.Id,
                    AuthorId = author.Id,
                    PublisherId = publisher.Id,
                    ShelfId = shelf.Id,
                    Status = BookStatus.Available
                };
                db.Add(book);
                await db.SaveChangesAsync(ct);
                bookId = book.Id;

                var position = await db.Positions.FirstAsync(ct);
                var role = await db.SecurityRoles.FirstAsync(ct);
                var empEmail = $"temp.staff.{Guid.NewGuid():N}@libraryhub.local";
                var empUser = new LibraryUser
                {
                    UserName = empEmail,
                    Email = empEmail,
                    EmailConfirmed = true,
                    IsActive = true,
                    DisplayName = "Temp Staff"
                };
                Assert.True((await users.CreateAsync(empUser, StaffPassword)).Succeeded);
                Assert.True((await users.AddToRoleAsync(empUser, "Staff")).Succeeded);
                empUserId = empUser.Id;

                var emp = new Employee
                {
                    EmployeeCode = $"NV-T-{Guid.NewGuid():N}"[..16].ToUpperInvariant(),
                    FullName = "Nhân Viên Kiểm Thử",
                    Email = empEmail,
                    Phone = "0912345678",
                    Address = "Hà Nội",
                    NationalId = Random.Shared.NextInt64(100000000000, 999999999999).ToString(),
                    DateOfBirth = new(1995, 2, 2),
                    HiredOn = new(2024, 1, 1),
                    PositionId = position.Id,
                    RoleId = role.Id,
                    Salary = 9_000_000,
                    Status = EmploymentStatus.Working
                };
                db.Add(emp);
                await db.SaveChangesAsync(ct);
                empId = emp.Id;
                db.Add(new StaffAccount { EmployeeId = emp.Id, UserAccountId = empUser.Id });
                await db.SaveChangesAsync(ct);

                reservationReaderId = await db.Readers.Where(x => x.Email == ReaderEmail).Select(x => x.Id).SingleAsync(ct);
            }

            // 1. In-use category deletion returns 409 Conflict
            var delCat = await admin.DeleteAsync($"/api/v1/categories/{categoryId}", ct);
            Assert.Equal(HttpStatusCode.Conflict, delCat.StatusCode);

            // 2. Concurrent reservations serialize into 201 + 409 without duplicates
            var r1Task = readerClient.PostAsJsonAsync("/api/v1/reservations", new { bookId, note = "Concurrent 1" }, ct);
            var r2Task = readerClient.PostAsJsonAsync("/api/v1/reservations", new { bookId, note = "Concurrent 2" }, ct);
            var responses = await Task.WhenAll(r1Task, r2Task);
            var statuses = responses.Select(r => r.StatusCode).OrderBy(s => s).ToArray();
            Assert.Contains(HttpStatusCode.Created, statuses);
            Assert.Contains(HttpStatusCode.Conflict, statuses);

            // 3. Soft-deleting employee disables account & denies subsequent login
            var delEmp = await admin.DeleteAsync($"/api/v1/employees/{empId}", ct);
            Assert.Equal(HttpStatusCode.NoContent, delEmp.StatusCode);
            var blockedLoginClient = Client();
            var loginPageHtml = await (await blockedLoginClient.GetAsync("/login", ct)).Content.ReadAsStringAsync(ct);
            await using (var checkScope = factory.Services.CreateAsyncScope())
            {
                var u = await checkScope.ServiceProvider.GetRequiredService<UserManager<LibraryUser>>()
                    .FindByIdAsync(empUserId!.Value.ToString());
                Assert.NotNull(u);
                Assert.False(u.IsActive);
            }
            var empEmailForLogin = (await NewDb().Employees.SingleAsync(x => x.Id == empId, ct)).Email;
            var loginResp = await blockedLoginClient.PostAsync("/login", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = AntiForgeryToken(loginPageHtml),
                ["Email"] = empEmailForLogin,
                ["Password"] = StaffPassword,
                ["RememberMe"] = "false"
            }), ct);
            Assert.Equal(HttpStatusCode.OK, loginResp.StatusCode);

            // 4. CSV formula neutralization
            var csvResp = await admin.GetAsync("/admin/exports/books.csv", ct);
            Assert.Equal(HttpStatusCode.OK, csvResp.StatusCode);
            var csvText = await csvResp.Content.ReadAsStringAsync(ct);
            Assert.Contains("'=HYPERLINK(", csvText);
            Assert.Contains("'=SUM(1,1)", csvText);
        }
        finally
        {
            await using var scope = factory.Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            if (bookId.HasValue && reservationReaderId.HasValue)
                await db.Reservations.Where(x => x.BookId == bookId.Value && x.ReaderId == reservationReaderId.Value).ExecuteDeleteAsync(ct);
            if (bookId.HasValue)
                await db.Books.Where(x => x.Id == bookId.Value).ExecuteDeleteAsync(ct);
            if (categoryId.HasValue)
                await db.Categories.Where(x => x.Id == categoryId.Value).ExecuteDeleteAsync(ct);
            if (empId.HasValue)
            {
                await db.StaffAccounts.Where(x => x.EmployeeId == empId.Value).ExecuteDeleteAsync(ct);
                await db.Employees.Where(x => x.Id == empId.Value).ExecuteDeleteAsync(ct);
            }
            if (empUserId.HasValue)
            {
                var um = scope.ServiceProvider.GetRequiredService<UserManager<LibraryUser>>();
                var u = await um.FindByIdAsync(empUserId.Value.ToString());
                if (u is not null) await um.DeleteAsync(u);
            }
        }
    }

    private async Task EnsureTestPrincipalsAsync()
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var services = scope.ServiceProvider;
        var db = services.GetRequiredService<LibraryDbContext>();
        var users = services.GetRequiredService<UserManager<LibraryUser>>();
        var identityRoles = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        await EnsureIdentityRoleAsync(identityRoles, "Reader");
        await EnsureIdentityRoleAsync(identityRoles, "Staff");
        await EnsureIdentityRoleAsync(identityRoles, "Administrator");

        var readerUser = await EnsureUserAsync(users, ReaderEmail, ReaderPassword, AccountKind.Reader, "Reader");
        var reader = await db.Readers.SingleOrDefaultAsync(x => x.UserAccountId == readerUser.Id);
        if (reader is null)
        {
            reader = new Reader
            {
                UserAccountId = readerUser.Id,
                ReaderCode = $"DG-I-{Guid.NewGuid():N}"[..20].ToUpperInvariant(),
                FullName = "Integration Reader",
                Email = ReaderEmail,
                RegisteredOn = DateOnly.FromDateTime(DateTime.UtcNow),
                ExpiresOn = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)),
                EmailConfirmed = true,
                MaxBorrowBooks = 10
            };
            db.Add(reader);
            await db.SaveChangesAsync();
        }
        else
        {
            await db.BorrowSlips
                .Where(x => x.ReaderId == reader.Id
                    && (x.Status == BorrowStatus.PendingApproval
                        || x.Status == BorrowStatus.ApprovedAwaitingPickup
                        || x.Status == BorrowStatus.Borrowing
                        || x.Status == BorrowStatus.Overdue
                        || x.Status == BorrowStatus.PendingExtension))
                .ExecuteUpdateAsync(x => x.SetProperty(y => y.Status, BorrowStatus.Cancelled));
        }

        var staffUser = await EnsureUserAsync(users, StaffEmail, StaffPassword, AccountKind.Internal, "Staff");
        _ = await EnsureUserAsync(users, AdminEmail, AdminPassword, AccountKind.Internal, "Administrator");
        var position = await db.Positions.SingleOrDefaultAsync(x => x.Code == "INTEGRATION");
        if (position is null)
        {
            position = new Position
            {
                Code = "INTEGRATION",
                Name = "Integration",
                SalaryAmount = 10_000_000m,
                AllowSalaryOverride = true
            };
            db.Add(position);
        }
        var securityRole = await db.SecurityRoles.SingleOrDefaultAsync(x => x.Code == "INTEGRATION_STAFF");
        if (securityRole is null)
        {
            securityRole = new Role
            {
                Code = "INTEGRATION_STAFF",
                Name = "Integration Staff",
                IsSystem = true
            };
            db.Add(securityRole);
        }
        await db.SaveChangesAsync();

        var employee = await db.Employees.SingleOrDefaultAsync(x => x.EmployeeCode == "NV-INTEGRATION");
        if (employee is null)
        {
            employee = new Employee
            {
                EmployeeCode = "NV-INTEGRATION",
                FullName = "Integration Staff",
                Email = StaffEmail,
                Phone = "0912345678",
                Address = "Hà Nội",
                NationalId = "001234567890",
                DateOfBirth = new DateOnly(1990, 1, 1),
                HiredOn = new DateOnly(2020, 1, 1),
                PositionId = position.Id,
                Salary = position.SalaryAmount,
                RoleId = securityRole.Id
            };
            db.Add(employee);
            await db.SaveChangesAsync();
        }
        else if (employee.RoleId != securityRole.Id)
        {
            employee.RoleId = securityRole.Id;
            await db.SaveChangesAsync();
        }

        if (!await db.StaffAccounts.AnyAsync(x => x.UserAccountId == staffUser.Id))
        {
            db.Add(new StaffAccount { UserAccountId = staffUser.Id, EmployeeId = employee.Id });
            await db.SaveChangesAsync();
        }

        await services.GetRequiredService<IPermissionSeeder>().SeedAsync();
        var createPermission = await db.Permissions.SingleAsync(x => x.Code == PermissionCodes.EmployeeCreate);
        var deny = await db.UserPermissions.SingleOrDefaultAsync(
            x => x.EmployeeId == employee.Id && x.PermissionId == createPermission.Id);
        if (deny is null)
            db.Add(new UserPermission
            {
                EmployeeId = employee.Id,
                PermissionId = createPermission.Id,
                Effect = PermissionEffect.Deny
            });
        else
            deny.Effect = PermissionEffect.Deny;
        await db.SaveChangesAsync();
    }

    private static async Task EnsureIdentityRoleAsync(RoleManager<IdentityRole<Guid>> roles, string role)
    {
        if (!await roles.RoleExistsAsync(role))
            Assert.True((await roles.CreateAsync(new IdentityRole<Guid>(role))).Succeeded);
    }

    private static async Task<LibraryUser> EnsureUserAsync(
        UserManager<LibraryUser> users,
        string email,
        string password,
        AccountKind kind,
        string role)
    {
        var user = await users.FindByEmailAsync(email);
        if (user is null)
        {
            user = new LibraryUser
            {
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                DisplayName = email,
                AccountKind = kind
            };
            Assert.True((await users.CreateAsync(user, password)).Succeeded);
        }
        else
        {
            user.EmailConfirmed = true;
            user.AccountKind = kind;
            await users.UpdateAsync(user);
            if (await users.HasPasswordAsync(user))
            {
                var token = await users.GeneratePasswordResetTokenAsync(user);
                Assert.True((await users.ResetPasswordAsync(user, token, password)).Succeeded);
            }
        }
        if (!await users.IsInRoleAsync(user, role))
            Assert.True((await users.AddToRoleAsync(user, role)).Succeeded);
        return user;
    }

    private async Task<Book> EnsureBookAsync(LibraryDbContext db)
    {
        var book = await db.Books.SingleOrDefaultAsync(x => x.Isbn == "9780000999991");
        if (book is not null) return book;

        var category = new Category { Name = $"Integration-{Guid.NewGuid():N}" };
        var author = new Author { Name = "Integration Author" };
        var publisher = new Publisher { Name = "Integration Publisher" };
        var shelf = new Shelf { Code = $"I-{Guid.NewGuid():N}"[..16], Name = "Integration Shelf" };
        book = new Book
        {
            Isbn = "9780000999991",
            Title = "Integration Book",
            Price = 100_000m,
            TotalQuantity = 100,
            AvailableQuantity = 50,
            Status = BookStatus.Available,
            Category = category,
            Author = author,
            Publisher = publisher,
            Shelf = shelf
        };
        db.Add(book);
        await db.SaveChangesAsync();
        return book;
    }

    private async Task<Book> CreateUniqueBookAsync(string title)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        var suffix = Guid.NewGuid().ToString("N");
        var book = new Book
        {
            Isbn = $"97{Random.Shared.NextInt64(10_000_000_000, 99_999_999_999)}",
            Title = $"{title} {suffix[..6]}",
            Price = 100_000m,
            TotalQuantity = 5,
            AvailableQuantity = 5,
            Status = BookStatus.Available,
            Category = new Category { Name = $"Category {suffix}" },
            Author = new Author { Name = $"Author {suffix}" },
            Publisher = new Publisher { Name = $"Publisher {suffix}" },
            Shelf = new Shelf { Code = $"S-{suffix[..10]}", Name = $"Shelf {suffix}" }
        };
        db.Add(book);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        return book;
    }

    private static string AntiForgeryToken(string html)
    {
        var match = Regex.Match(
            html,
            "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"",
            RegexOptions.IgnoreCase);
        Assert.True(match.Success);
        return WebUtility.HtmlDecode(match.Groups[1].Value);
    }

    private async Task DeleteExternalTestAccountAsync(string email, CancellationToken cancellationToken)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<LibraryUser>>();
        var user = await users.FindByEmailAsync(email);
        if (user is null) return;

        await db.Readers.Where(x => x.UserAccountId == user.Id).ExecuteDeleteAsync(cancellationToken);
        Assert.True((await users.DeleteAsync(user)).Succeeded);
    }

    private async Task<int> CreateBorrowingSlipAsync()
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        var reader = await db.Readers.SingleAsync(x => x.Email == ReaderEmail);
        var book = await EnsureBookAsync(db);
        var slip = new BorrowSlip
        {
            Code = $"BR-I-{Guid.NewGuid():N}"[..21].ToUpperInvariant(),
            ReaderId = reader.Id,
            TotalBooks = 1,
            RequestedAt = DateTimeOffset.UtcNow,
            Status = BorrowStatus.Borrowing,
            DueAt = DateTimeOffset.UtcNow.AddDays(7),
            CreatedByAccountId = reader.UserAccountId,
            IdempotencyKey = $"integration-invalid-{Guid.NewGuid():N}",
            Details = [new BorrowDetail { BookId = book.Id, Quantity = 1, BorrowPrice = book.Price }]
        };
        db.Add(slip);
        await db.SaveChangesAsync();
        return slip.Id;
    }

    private async Task<int> CreateFineAsync(decimal amount)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        var reader = await db.Readers.SingleAsync(x => x.Email == ReaderEmail);
        var slip = new BorrowSlip
        {
            Code = $"BR-I-{Guid.NewGuid():N}"[..21].ToUpperInvariant(),
            ReaderId = reader.Id,
            TotalBooks = 0,
            RequestedAt = DateTimeOffset.UtcNow,
            Status = BorrowStatus.Returned,
            CreatedByAccountId = reader.UserAccountId,
            IdempotencyKey = $"integration-fine-borrow-{Guid.NewGuid():N}"
        };
        db.Add(slip);
        await db.SaveChangesAsync();
        var fine = new Fine
        {
            Code = $"FN-I-{Guid.NewGuid():N}"[..21].ToUpperInvariant(),
            BorrowSlipId = slip.Id,
            ReaderId = reader.Id,
            Type = FineType.Damaged,
            Amount = amount,
            Status = FineStatus.Unpaid,
            Reason = "Integration concurrency test"
        };
        db.Add(fine);
        await db.SaveChangesAsync();
        return fine.Id;
    }

    private async Task<HttpClient> LoginAsync(string email, string password)
    {
        var client = Client();
        var ct = TestContext.Current.CancellationToken;
        var html = await (await client.GetAsync("/login", ct)).Content.ReadAsStringAsync(ct);
        var response = await client.PostAsync("/login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = AntiForgeryToken(html),
            ["Email"] = email,
            ["Password"] = password,
            ["RememberMe"] = "false"
        }), ct);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        return client;
    }

    private WebApplicationFactory<Program> ExternalTestFactory(
        string googleScenario,
        string facebookScenario) =>
        factory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services =>
            {
                services.AddAuthentication()
                    .AddScheme<AuthenticationSchemeOptions, TestExternalAuthenticationHandler>(
                        "Google",
                        "Google",
                        options => options.ClaimsIssuer = googleScenario)
                    .AddScheme<AuthenticationSchemeOptions, TestExternalAuthenticationHandler>(
                        "Facebook",
                        "Facebook",
                        options => options.ClaimsIssuer = facebookScenario);
            }));

    private static HttpClient OAuthClient(WebApplicationFactory<Program> oauthFactory) =>
        oauthFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
            HandleCookies = true
        });

    private static async Task<HttpResponseMessage> StartExternalLoginAsync(
        HttpClient client,
        string loginHtml,
        string provider,
        string? returnUrl,
        CancellationToken cancellationToken)
    {
        var form = new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = AntiForgeryToken(loginHtml),
            ["Provider"] = provider
        };
        if (returnUrl is not null) form["ReturnUrl"] = returnUrl;
        var response = await client.PostAsync("/external-login", new FormUrlEncodedContent(form), cancellationToken);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        return response;
    }

    private static HttpRequestMessage LifecycleRequest(string url, object body)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(body)
        };
        request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString("N"));
        return request;
    }

    private HttpClient Client()
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
            HandleCookies = true
        });
        client.DefaultRequestHeaders.Add("X-LibraryHub-Request", "XMLHttpRequest");
        return client;
    }

    private static LibraryDbContext NewDb() =>
        new(new DbContextOptionsBuilder<LibraryDbContext>()
            .UseSqlServer(Connection, sql => sql.EnableRetryOnFailure())
            .Options);

    private sealed class NullNotifications : INotificationPublisher
    {
        public Task PublishAsync(Guid accountId, NotificationType type, string title, string message,
            string? link, string? correlationId, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class TestExternalAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync() =>
            Task.FromResult(AuthenticateResult.NoResult());

        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            var normalizedProvider = Scheme.Name.ToLowerInvariant();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, $"test-provider-key-{normalizedProvider}"),
                new Claim(ClaimTypes.Name, $"Độc giả {Scheme.Name}"),
                new Claim(ClaimTypes.Role, "Staff"),
                new Claim(ClaimTypes.Role, "Administrator")
            };
            if (!string.Equals(Options.ClaimsIssuer, "missing-email", StringComparison.Ordinal))
                claims.Add(new Claim(
                    ClaimTypes.Email,
                    $"integration.oauth.{normalizedProvider}@libraryhub.local"));
            if (string.Equals(Options.ClaimsIssuer, "verified-email", StringComparison.Ordinal))
                claims.Add(new Claim(ExternalAuthenticationSecurity.EmailVerifiedClaimType, bool.TrueString));
            var principal = new ClaimsPrincipal(new ClaimsIdentity(
                claims,
                Scheme.Name,
                ClaimTypes.Name,
                ClaimTypes.Role));

            await Context.SignInAsync(IdentityConstants.ExternalScheme, principal, properties);
            Response.Redirect(properties.RedirectUri ?? "/external-login/callback");
        }
    }

    private sealed class LocalDbWebFactory(string connection) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<LibraryDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<LibraryDbContext>>();
                services.RemoveAll<LibraryDbContext>();
                services.AddDbContext<LibraryDbContext>(options =>
                    options.UseSqlServer(connection, sql => sql.EnableRetryOnFailure()));
            });
        }
    }
}
