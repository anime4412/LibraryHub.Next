using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
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

namespace LibraryHub.IntegrationTests;

public sealed class OAuthIntegrationTests
{
    [Fact]
    public async Task Missing_configuration_hides_provider_buttons_and_rejects_unknown_scheme()
    {
        var ct = TestContext.Current.CancellationToken;
        using var factory = new OAuthWebFactory();
        using var client = Client(factory);
        var html = await (await client.GetAsync("/login?returnUrl=https%3A%2F%2Fevil.example", ct))
            .Content.ReadAsStringAsync(ct);

        Assert.DoesNotContain("Tiếp tục với Google", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Tiếp tục với Facebook", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("evil.example", html, StringComparison.OrdinalIgnoreCase);

        var response = await client.PostAsync("/external-login", new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = AntiForgeryToken(html),
                ["Provider"] = "Unknown",
                ["ReturnUrl"] = "https://evil.example/steal"
            }), ct);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith("/login", response.Headers.Location?.OriginalString ?? "", StringComparison.Ordinal);
        Assert.DoesNotContain("evil.example", response.Headers.Location?.OriginalString ?? "", StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Complete_configuration_registers_both_schemes_with_secure_callback_options()
    {
        var ct = TestContext.Current.CancellationToken;
        using var factory = new OAuthWebFactory(registerRealProviders: true);
        await using var scope = factory.Services.CreateAsyncScope();
        var schemes = await scope.ServiceProvider.GetRequiredService<IAuthenticationSchemeProvider>()
            .GetAllSchemesAsync();
        var google = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<GoogleOptions>>().Get("Google");
        var facebook = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<FacebookOptions>>().Get("Facebook");

        Assert.Contains(schemes, x => x.Name == "Google");
        Assert.Contains(schemes, x => x.Name == "Facebook");
        Assert.Equal("/signin-google", google.CallbackPath);
        Assert.Equal("/signin-facebook", facebook.CallbackPath);
        Assert.False(google.SaveTokens);
        Assert.False(facebook.SaveTokens);
        Assert.Contains("email", google.Scope);
        Assert.Contains("profile", google.Scope);
        Assert.Contains("email", facebook.Scope);
        Assert.Contains("name", facebook.Fields);
        Assert.Contains("email", facebook.Fields);
        Assert.Equal(SameSiteMode.None, google.CorrelationCookie.SameSite);
        Assert.Equal(SameSiteMode.None, facebook.CorrelationCookie.SameSite);
        Assert.Equal(CookieSecurePolicy.Always, google.CorrelationCookie.SecurePolicy);
        Assert.Equal(CookieSecurePolicy.Always, facebook.CorrelationCookie.SecurePolicy);

        using var client = Client(factory);
        var invalidCorrelation = await client.GetAsync("/signin-google?code=test&state=invalid", ct);
        Assert.Equal(HttpStatusCode.Redirect, invalidCorrelation.StatusCode);
        Assert.Equal(
            "/login?externalError=remote-failure&provider=Google",
            invalidCorrelation.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Stable_remote_errors_are_localized_without_echoing_provider_details()
    {
        var ct = TestContext.Current.CancellationToken;
        using var factory = new OAuthWebFactory();
        using var client = Client(factory);
        var response = await client.GetAsync(
            "/login?externalError=remote-failure&provider=Google&returnUrl=https%3A%2F%2Fevil.example", ct);
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(ct));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Không thể hoàn tất đăng nhập bằng Google", html);
        Assert.DoesNotContain("evil.example", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("stack trace", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Verified_google_creates_one_reader_ignores_role_claims_and_reuses_linked_login()
    {
        var ct = TestContext.Current.CancellationToken;
        using var factory = new OAuthWebFactory(addFakeProviders: true);
        using (var client = Client(factory))
        {
            await CreateExternalReaderAsync(client, "Google", expectedRedirect: "/me", ct);
            Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/me", ct)).StatusCode);
        }

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            var user = await db.Users.SingleAsync(x => x.Email == GoogleEmail, ct);
            var roles = await (
                from userRole in db.UserRoles
                join role in db.Roles on userRole.RoleId equals role.Id
                where userRole.UserId == user.Id
                select role.Name).ToListAsync(ct);
            Assert.True(user.EmailConfirmed);
            Assert.Equal(AccountKind.Reader, user.AccountKind);
            Assert.Single(await db.Readers.Where(x => x.UserAccountId == user.Id).ToListAsync(ct));
            Assert.Single(await db.UserLogins.Where(x => x.UserId == user.Id).ToListAsync(ct));
            Assert.Single(roles);
            Assert.Equal("Reader", roles[0]);
        }

        using (var returningClient = Client(factory))
        {
            var callback = await StartAndFollowCallbackAsync(returningClient, "Google", "/me", ct);
            Assert.Equal(HttpStatusCode.Redirect, callback.StatusCode);
            Assert.Equal("/me", callback.Headers.Location?.OriginalString);
        }

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            Assert.Equal(1, await db.Users.CountAsync(x => x.Email == GoogleEmail, ct));
            var userId = await db.Users.Where(x => x.Email == GoogleEmail).Select(x => x.Id).SingleAsync(ct);
            Assert.Equal(1, await db.UserLogins.CountAsync(x => x.UserId == userId, ct));
        }
    }

    [Fact]
    public async Task Facebook_account_remains_unconfirmed_until_libraryhub_email_confirmation()
    {
        var ct = TestContext.Current.CancellationToken;
        using var factory = new OAuthWebFactory(addFakeProviders: true);
        Guid userId;
        using (var client = Client(factory))
        {
            var created = await CreateExternalReaderAsync(client, "Facebook", expectedRedirect: "/login", ct);
            Assert.StartsWith("/login", created.Headers.Location?.OriginalString ?? "", StringComparison.Ordinal);
            Assert.Equal(HttpStatusCode.Redirect, (await client.GetAsync("/me", ct)).StatusCode);
        }

        string token;
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            var user = await db.Users.SingleAsync(x => x.Email == FacebookEmail, ct);
            var reader = await db.Readers.SingleAsync(x => x.UserAccountId == user.Id, ct);
            userId = user.Id;
            Assert.False(user.EmailConfirmed);
            Assert.False(reader.EmailConfirmed);
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<LibraryUser>>();
            token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        }

        using (var client = Client(factory))
        {
            var confirmed = await client.GetAsync(
                $"/confirm-email?userId={userId:D}&token={Uri.EscapeDataString(token)}", ct);
            Assert.Equal(HttpStatusCode.OK, confirmed.StatusCode);

            var callback = await StartAndFollowCallbackAsync(client, "Facebook", "/me", ct);
            Assert.Equal(HttpStatusCode.Redirect, callback.StatusCode);
            Assert.Equal("/me", callback.Headers.Location?.OriginalString);
        }
    }

    [Fact]
    public async Task Provider_without_email_is_rejected_without_persisting_identity_data()
    {
        var ct = TestContext.Current.CancellationToken;
        using var factory = new OAuthWebFactory(addFakeProviders: true, googleScenario: "missing-email");
        using var client = Client(factory);
        var callback = await StartAndFollowCallbackAsync(client, "Google", null, ct);
        var html = WebUtility.HtmlDecode(await callback.Content.ReadAsStringAsync(ct));

        Assert.Equal(HttpStatusCode.OK, callback.StatusCode);
        Assert.Contains("không trả địa chỉ email", html, StringComparison.OrdinalIgnoreCase);
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        Assert.Empty(await db.Users.ToListAsync(ct));
        Assert.Empty(await db.Readers.ToListAsync(ct));
        Assert.Empty(await db.UserLogins.ToListAsync(ct));
    }

    [Fact]
    public async Task Existing_email_requires_password_before_linking_external_identity()
    {
        var ct = TestContext.Current.CancellationToken;
        using var factory = new OAuthWebFactory(addFakeProviders: true);
        await CreatePasswordReaderAsync(factory, GoogleEmail, Password, ct);
        using (var passwordClient = Client(factory))
        {
            var passwordLoginHtml = await (await passwordClient.GetAsync("/login", ct))
                .Content.ReadAsStringAsync(ct);
            var passwordLogin = await passwordClient.PostAsync("/login", new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["__RequestVerificationToken"] = AntiForgeryToken(passwordLoginHtml),
                    ["Email"] = GoogleEmail,
                    ["Password"] = Password,
                    ["RememberMe"] = "false",
                    ["ReturnUrl"] = "/me"
                }), ct);
            Assert.Equal(HttpStatusCode.Redirect, passwordLogin.StatusCode);
            Assert.Equal("/me", passwordLogin.Headers.Location?.OriginalString);
        }

        using var client = Client(factory);
        var linkPage = await StartAndFollowCallbackAsync(client, "Google", "/me", ct);
        var html = await linkPage.Content.ReadAsStringAsync(ct);
        Assert.Equal(HttpStatusCode.OK, linkPage.StatusCode);
        Assert.Contains("Mật khẩu LibraryHub", html);

        var rejected = await client.PostAsync("/external-login/link", new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = AntiForgeryToken(html),
                ["Password"] = "wrong-password",
                ["ReturnUrl"] = "/me"
            }), ct);
        Assert.Equal(HttpStatusCode.OK, rejected.StatusCode);
        await AssertExternalLoginCountAsync(factory, 0, ct);

        html = await rejected.Content.ReadAsStringAsync(ct);
        var linked = await client.PostAsync("/external-login/link", new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = AntiForgeryToken(html),
                ["Password"] = Password,
                ["ReturnUrl"] = "/me"
            }), ct);
        Assert.Equal(HttpStatusCode.Redirect, linked.StatusCode);
        Assert.Equal("/me", linked.Headers.Location?.OriginalString);
        await AssertExternalLoginCountAsync(factory, 1, ct);
    }

    [Fact]
    public async Task Locked_and_inactive_linked_accounts_cannot_bypass_identity_rules()
    {
        var ct = TestContext.Current.CancellationToken;
        using var factory = new OAuthWebFactory(addFakeProviders: true);
        using (var client = Client(factory))
            await CreateExternalReaderAsync(client, "Google", expectedRedirect: "/me", ct);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<LibraryUser>>();
            var user = await users.FindByEmailAsync(GoogleEmail);
            Assert.NotNull(user);
            Assert.True((await users.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddMinutes(15))).Succeeded);
        }

        using (var lockedClient = Client(factory))
        {
            var callback = await StartAndFollowCallbackAsync(lockedClient, "Google", "/me", ct);
            Assert.Equal(HttpStatusCode.Redirect, callback.StatusCode);
            var login = await lockedClient.GetAsync(callback.Headers.Location, ct);
            Assert.Contains("tạm khóa", WebUtility.HtmlDecode(await login.Content.ReadAsStringAsync(ct)));
        }

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<LibraryUser>>();
            var user = await users.FindByEmailAsync(GoogleEmail);
            Assert.NotNull(user);
            user.LockoutEnd = null;
            user.IsActive = false;
            Assert.True((await users.UpdateAsync(user)).Succeeded);
        }

        using var inactiveClient = Client(factory);
        var inactiveCallback = await StartAndFollowCallbackAsync(inactiveClient, "Google", "/me", ct);
        Assert.Equal(HttpStatusCode.Redirect, inactiveCallback.StatusCode);
        var inactiveLogin = await inactiveClient.GetAsync(inactiveCallback.Headers.Location, ct);
        Assert.Contains("ngừng hoạt động", WebUtility.HtmlDecode(await inactiveLogin.Content.ReadAsStringAsync(ct)));
    }

    [Theory]
    [InlineData("Google")]
    [InlineData("Facebook")]
    public async Task Pending_borrow_cart_continues_after_linked_external_login(string provider)
    {
        var ct = TestContext.Current.CancellationToken;
        using var factory = new OAuthWebFactory(addFakeProviders: true);
        var bookId = await CreateLinkedExternalReaderAndBookAsync(factory, provider, ct);
        using var client = Client(factory);
        var loginHtml = await (await client.GetAsync("/login", ct)).Content.ReadAsStringAsync(ct);

        var pending = await client.PostAsync("/borrow-cart/add", new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = AntiForgeryToken(loginHtml),
                ["BookId"] = bookId.ToString(),
                ["ReturnUrl"] = $"/books/{bookId}"
            }), ct);
        Assert.Equal(HttpStatusCode.Unauthorized, pending.StatusCode);

        var callback = await StartAndFollowCallbackAsync(
            client,
            provider,
            "/borrow-cart/continue",
            ct);
        Assert.Equal(HttpStatusCode.Redirect, callback.StatusCode);
        Assert.Equal("/borrow-cart/continue", callback.Headers.Location?.OriginalString);

        var continuation = await client.GetAsync(callback.Headers.Location, ct);
        var continuationHtml = await continuation.Content.ReadAsStringAsync(ct);
        Assert.Equal(HttpStatusCode.OK, continuation.StatusCode);
        Assert.Contains($"data-book-id=\"{bookId}\"", continuationHtml);
        Assert.Contains("data-can-add=\"true\"", continuationHtml);
    }

    private const string GoogleEmail = "integration.oauth.google@libraryhub.local";
    private const string FacebookEmail = "integration.oauth.facebook@libraryhub.local";
    private const string Password = "Reader@OAuth2026!";

    private static HttpClient Client(WebApplicationFactory<Program> factory) =>
        factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
            HandleCookies = true
        });

    private static async Task<HttpResponseMessage> StartAndFollowCallbackAsync(
        HttpClient client,
        string provider,
        string? returnUrl,
        CancellationToken cancellationToken)
    {
        var loginHtml = await (await client.GetAsync("/login", cancellationToken))
            .Content.ReadAsStringAsync(cancellationToken);
        var form = new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = AntiForgeryToken(loginHtml),
            ["Provider"] = provider
        };
        if (returnUrl is not null) form["ReturnUrl"] = returnUrl;
        var challenge = await client.PostAsync(
            "/external-login",
            new FormUrlEncodedContent(form),
            cancellationToken);
        Assert.Equal(HttpStatusCode.Redirect, challenge.StatusCode);
        return await client.GetAsync(challenge.Headers.Location, cancellationToken);
    }

    private static async Task<HttpResponseMessage> CreateExternalReaderAsync(
        HttpClient client,
        string provider,
        string expectedRedirect,
        CancellationToken cancellationToken)
    {
        var confirmation = await StartAndFollowCallbackAsync(client, provider, "/me", cancellationToken);
        var html = await confirmation.Content.ReadAsStringAsync(cancellationToken);
        Assert.Equal(HttpStatusCode.OK, confirmation.StatusCode);
        var created = await client.PostAsync("/external-login/confirm", new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = AntiForgeryToken(html),
                ["DisplayName"] = $"Độc giả {provider}",
                ["ReaderType"] = "0",
                ["ReturnUrl"] = "/me"
            }), cancellationToken);
        Assert.Equal(HttpStatusCode.Redirect, created.StatusCode);
        Assert.StartsWith(expectedRedirect, created.Headers.Location?.OriginalString ?? "", StringComparison.Ordinal);
        return created;
    }

    private static async Task CreatePasswordReaderAsync(
        WebApplicationFactory<Program> factory,
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<LibraryUser>>();
        var user = new LibraryUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            DisplayName = "Độc giả có sẵn",
            AccountKind = AccountKind.Reader
        };
        Assert.True((await users.CreateAsync(user, password)).Succeeded);
        Assert.True((await users.AddToRoleAsync(user, "Reader")).Succeeded);
        var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        db.Readers.Add(new Reader
        {
            UserAccountId = user.Id,
            ReaderCode = $"DG-OAUTH-{Guid.NewGuid():N}"[..21].ToUpperInvariant(),
            FullName = user.DisplayName,
            Email = email,
            EmailConfirmed = true,
            RegisteredOn = DateOnly.FromDateTime(DateTime.UtcNow),
            ExpiresOn = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)),
            MaxBorrowBooks = 10
        });
        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task<int> CreateLinkedExternalReaderAndBookAsync(
        WebApplicationFactory<Program> factory,
        string provider,
        CancellationToken cancellationToken)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<LibraryUser>>();
        var normalizedProvider = provider.ToLowerInvariant();
        var email = $"integration.oauth.{normalizedProvider}@libraryhub.local";
        var user = new LibraryUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            DisplayName = $"Độc giả {provider}",
            AccountKind = AccountKind.Reader
        };
        Assert.True((await users.CreateAsync(user)).Succeeded);
        Assert.True((await users.AddToRoleAsync(user, "Reader")).Succeeded);
        Assert.True((await users.AddLoginAsync(user, new UserLoginInfo(
            provider,
            $"test-provider-key-{normalizedProvider}",
            provider))).Succeeded);

        var suffix = Guid.NewGuid().ToString("N");
        var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        db.Readers.Add(new Reader
        {
            UserAccountId = user.Id,
            ReaderCode = $"DG-OAUTH-{suffix}"[..21].ToUpperInvariant(),
            FullName = user.DisplayName,
            Email = email,
            EmailConfirmed = true,
            RegisteredOn = DateOnly.FromDateTime(DateTime.UtcNow),
            ExpiresOn = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)),
            MaxBorrowBooks = 10
        });
        var book = new Book
        {
            Isbn = $"978{Random.Shared.NextInt64(1_000_000_000, 9_999_999_999)}",
            Title = $"OAuth borrow cart {provider}",
            TotalQuantity = 2,
            AvailableQuantity = 2,
            Status = BookStatus.Available,
            Category = new Category { Name = $"OAuth category {suffix}" },
            Author = new Author { Name = $"OAuth author {suffix}" },
            Publisher = new Publisher { Name = $"OAuth publisher {suffix}" },
            Shelf = new Shelf { Code = $"O-{suffix[..10]}", Name = $"OAuth shelf {suffix}" }
        };
        db.Books.Add(book);
        await db.SaveChangesAsync(cancellationToken);
        return book.Id;
    }

    private static async Task AssertExternalLoginCountAsync(
        WebApplicationFactory<Program> factory,
        int expected,
        CancellationToken cancellationToken)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        Assert.Equal(expected, await db.UserLogins.CountAsync(cancellationToken));
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

    private sealed class OAuthWebFactory(
        bool addFakeProviders = false,
        bool registerRealProviders = false,
        string googleScenario = "verified-email",
        string facebookScenario = "unverified-email") : WebApplicationFactory<Program>
    {
        private readonly string databaseName = $"OAuth-{Guid.NewGuid():N}";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            if (registerRealProviders)
            {
                builder.UseSetting("Authentication:Google:ClientId", "test-google-client-id");
                builder.UseSetting("Authentication:Google:ClientSecret", "test-google-client-secret");
                builder.UseSetting("Authentication:Facebook:AppId", "test-facebook-app-id");
                builder.UseSetting("Authentication:Facebook:AppSecret", "test-facebook-app-secret");
            }

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<LibraryDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<LibraryDbContext>>();
                services.RemoveAll<LibraryDbContext>();
                services.AddDbContext<LibraryDbContext>(options => options.UseInMemoryDatabase(databaseName));
                if (addFakeProviders)
                {
                    services.AddAuthentication()
                        .AddScheme<AuthenticationSchemeOptions, FakeExternalAuthenticationHandler>(
                            "Google",
                            "Google",
                            options => options.ClaimsIssuer = googleScenario)
                        .AddScheme<AuthenticationSchemeOptions, FakeExternalAuthenticationHandler>(
                            "Facebook",
                            "Facebook",
                            options => options.ClaimsIssuer = facebookScenario);
                }
            });
        }
    }

    private sealed class FakeExternalAuthenticationHandler(
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
                new(ClaimTypes.NameIdentifier, $"test-provider-key-{normalizedProvider}"),
                new(ClaimTypes.Name, $"Độc giả {Scheme.Name}"),
                new(ClaimTypes.Role, "Staff"),
                new(ClaimTypes.Role, "Administrator")
            };
            if (!string.Equals(Options.ClaimsIssuer, "missing-email", StringComparison.Ordinal))
                claims.Add(new Claim(ClaimTypes.Email, $"integration.oauth.{normalizedProvider}@libraryhub.local"));
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
}
