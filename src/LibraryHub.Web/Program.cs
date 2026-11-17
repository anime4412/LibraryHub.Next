using LibraryHub.Application;
using LibraryHub.Infrastructure;
using LibraryHub.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
if (builder.Environment.IsDevelopment())
{
    builder.Logging.AddFilter("Microsoft.AspNetCore.Authentication", LogLevel.Information);
    builder.Logging.AddFilter("Microsoft.AspNetCore.Authentication.OAuth", LogLevel.Information);
    builder.Logging.AddFilter("Microsoft.AspNetCore.Authentication.Google", LogLevel.Information);
    builder.Logging.AddFilter("Microsoft.AspNetCore.Authentication.Facebook", LogLevel.Information);
    builder.Logging.AddFilter("Microsoft.AspNetCore.DataProtection", LogLevel.Information);
    builder.Logging.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);
}

builder.Services
    .AddDataProtection()
    .SetApplicationName("LibraryHub.Web");

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.ForwardLimit = 1;
    if (builder.Configuration.GetValue<bool>("ReverseProxy:TrustAnyForwardedHeaders"))
    {
        options.KnownIPNetworks.Clear();
        options.KnownProxies.Clear();
    }
});

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Title = context.ProblemDetails.Status switch
        {
            StatusCodes.Status400BadRequest => "Yêu cầu không hợp lệ",
            StatusCodes.Status401Unauthorized => "Bạn chưa đăng nhập",
            StatusCodes.Status403Forbidden => "Bạn không có quyền thực hiện thao tác này",
            StatusCodes.Status404NotFound => "Không tìm thấy dữ liệu",
            StatusCodes.Status409Conflict => "Dữ liệu đang xung đột",
            StatusCodes.Status500InternalServerError => "Hệ thống gặp sự cố",
            _ => context.ProblemDetails.Title
        };
        if (context.ProblemDetails.Status == StatusCodes.Status500InternalServerError)
            context.ProblemDetails.Detail = "Hệ thống chưa thể xử lý yêu cầu. Vui lòng thử lại sau.";
    };
});
builder.Services.AddExceptionHandler<DomainRuleExceptionHandler>();
builder.Services.AddControllersWithViews(options =>
{
    var messages = options.ModelBindingMessageProvider;
    messages.SetAttemptedValueIsInvalidAccessor((value, field) =>
        $"Giá trị “{value}” không hợp lệ cho {field}.");
    messages.SetMissingBindRequiredValueAccessor(field => $"Trường {field} là bắt buộc.");
    messages.SetMissingKeyOrValueAccessor(() => "Thiếu tên trường hoặc giá trị.");
    messages.SetMissingRequestBodyRequiredValueAccessor(() => "Nội dung yêu cầu không được để trống.");
    messages.SetNonPropertyAttemptedValueIsInvalidAccessor(value => $"Giá trị “{value}” không hợp lệ.");
    messages.SetNonPropertyUnknownValueIsInvalidAccessor(() => "Giá trị đã nhập không hợp lệ.");
    messages.SetNonPropertyValueMustBeANumberAccessor(() => "Giá trị phải là một số.");
    messages.SetUnknownValueIsInvalidAccessor(field => $"Giá trị của {field} không hợp lệ.");
    messages.SetValueIsInvalidAccessor(value => $"Giá trị “{value}” không hợp lệ.");
    messages.SetValueMustBeANumberAccessor(field => $"{field} phải là một số.");
    messages.SetValueMustNotBeNullAccessor(field => $"{field} không được để trống.");
});
builder.Services.AddSignalR();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<IFineCalculator, FineCalculator>();
builder.Services.AddScoped<IBorrowService, BorrowService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<BorrowCartFlowService>();
builder.Services.AddScoped<IBookLifecycleService, BookLifecycleService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddSingleton<IQuickHelpService, QuickHelpService>();
builder.Services.AddSingleton<IChatbotService, RuleBasedChatbotService>();
builder.Services.AddHttpClient("Gemini", client => client.Timeout = TimeSpan.FromSeconds(15));
builder.Services.AddScoped<GeminiChatbotService>();
builder.Services.AddScoped<IRealtimeGateway, SignalRRealtimeGateway>();
builder.Services.AddHostedService<PickupExpirationWorker>();

builder.Services.AddIdentityApiEndpoints<LibraryUser>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.SignIn.RequireConfirmedEmail = false;
    options.User.RequireUniqueEmail = true;
})
.AddRoles<IdentityRole<Guid>>()
.AddEntityFrameworkStores<LibraryDbContext>()
.AddPasswordValidator<VietnamesePasswordValidator>();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.AccessDeniedPath = "/access-denied";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api")
            || context.Request.Path.StartsWithSegments("/hubs"))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api")
            || context.Request.Path.StartsWithSegments("/hubs"))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});
builder.Services.ConfigureExternalCookie(options =>
{
    options.Cookie.Name = ".LibraryHub.External";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.Path = "/";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
    options.SlidingExpiration = false;
});
var auth = builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
});
var googleId = builder.Configuration["Authentication:Google:ClientId"];
var googleSecret = builder.Configuration["Authentication:Google:ClientSecret"];
if (!string.IsNullOrWhiteSpace(googleId) && !string.IsNullOrWhiteSpace(googleSecret))
{
    auth.AddGoogle(x =>
    {
        x.ClientId = googleId;
        x.ClientSecret = googleSecret;
        x.SaveTokens = false;
        x.CallbackPath = "/signin-google";
        x.SignInScheme = IdentityConstants.ExternalScheme;
        x.BackchannelTimeout = TimeSpan.FromSeconds(30);
        x.CorrelationCookie.HttpOnly = true;
        x.CorrelationCookie.SameSite = SameSiteMode.None;
        x.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
        x.Scope.Add("email");
        x.Scope.Add("profile");
        x.Events.OnCreatingTicket = context =>
        {
            var hasVerifiedEmail =
                (context.User.TryGetProperty("email_verified", out var emailVerified)
                    && emailVerified.ValueKind == System.Text.Json.JsonValueKind.True)
                || (context.User.TryGetProperty("verified_email", out var verifiedEmail)
                    && verifiedEmail.ValueKind == System.Text.Json.JsonValueKind.True);
            if (hasVerifiedEmail)
            {
                context.Identity?.AddClaim(new System.Security.Claims.Claim(
                    ExternalAuthenticationSecurity.EmailVerifiedClaimType,
                    bool.TrueString));
            }

            return Task.CompletedTask;
        };
        x.Events.OnRemoteFailure = context =>
        {
            context.Response.Redirect(
                "/login?externalError=remote-failure&provider=Google");

            context.HandleResponse();
            return Task.CompletedTask;
        };
        x.Events.OnAccessDenied = context =>
        {
            context.Response.Redirect(
                "/login?externalError=access-denied&provider=Google");

            context.HandleResponse();
            return Task.CompletedTask;
        };
    });
}
var facebookId = builder.Configuration["Authentication:Facebook:AppId"];
var facebookSecret = builder.Configuration["Authentication:Facebook:AppSecret"];
if (!string.IsNullOrWhiteSpace(facebookId) && !string.IsNullOrWhiteSpace(facebookSecret))
{
    auth.AddFacebook(x =>
    {
        x.AppId = facebookId;
        x.AppSecret = facebookSecret;
        x.SaveTokens = false;
        x.CallbackPath = "/signin-facebook";
        x.SignInScheme = IdentityConstants.ExternalScheme;
        x.BackchannelTimeout = TimeSpan.FromSeconds(30);
        x.CorrelationCookie.HttpOnly = true;
        x.CorrelationCookie.SameSite = SameSiteMode.None;
        x.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
        x.Scope.Add("email");
        x.Fields.Add("name");
        x.Fields.Add("email");
        x.Events.OnRemoteFailure = context =>
        {
            context.Response.Redirect(
                "/login?externalError=remote-failure&provider=Facebook");

            context.HandleResponse();
            return Task.CompletedTask;
        };
        x.Events.OnAccessDenied = context =>
        {
            context.Response.Redirect(
                "/login?externalError=access-denied&provider=Facebook");

            context.HandleResponse();
            return Task.CompletedTask;
        };
    });
}

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("InternalOnly", p => p.RequireRole("Staff", "Administrator"))
    .AddPolicy("AdministratorOnly", p => p.RequireRole("Administrator"));

var app = builder.Build();

if (string.IsNullOrWhiteSpace(googleId) || string.IsNullOrWhiteSpace(googleSecret))
    app.Logger.LogWarning("Đăng nhập Google chưa được bật vì thiếu ClientId hoặc ClientSecret.");
if (string.IsNullOrWhiteSpace(facebookId) || string.IsNullOrWhiteSpace(facebookSecret))
    app.Logger.LogWarning("Đăng nhập Facebook chưa được bật vì thiếu AppId hoặc AppSecret.");

app.Lifetime.ApplicationStarted.Register(() =>
{
    app.Logger.LogInformation("LibraryHub.Web đã khởi động và sẵn sàng nhận request.");
});
app.Lifetime.ApplicationStopping.Register(() =>
{
    app.Logger.LogWarning("LibraryHub.Web đang nhận yêu cầu dừng.");
});
app.Lifetime.ApplicationStopped.Register(() =>
{
    app.Logger.LogWarning("LibraryHub.Web đã dừng.");
});

try
{
    app.Logger.LogInformation("Bắt đầu đồng bộ quyền hệ thống.");
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
    if (db.Database.IsRelational())
        await db.Database.MigrateAsync();
    await scope.ServiceProvider.GetRequiredService<IPermissionSeeder>().SeedAsync();
    app.Logger.LogInformation("Đã đồng bộ quyền hệ thống.");

    app.Logger.LogInformation("Bắt đầu đồng bộ vai trò Identity.");
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    foreach (var role in new[] { "Reader", "Staff", "Administrator" })
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole<Guid>(role));

    app.Logger.LogInformation("Đã đồng bộ vai trò Identity.");

    app.Logger.LogInformation("Bắt đầu khởi tạo toàn bộ tài khoản và phân quyền mặc định.");
    await InitialAccountSeeder.SeedAsync(app.Services);
    app.Logger.LogInformation("Đã khởi tạo toàn bộ tài khoản và phân quyền mặc định.");
}
catch (Exception ex)
{
    app.Logger.LogCritical(ex, "Ứng dụng thất bại trong quá trình khởi tạo.");
    throw;
}

app.Logger.LogInformation("Ứng dụng chuẩn bị nhận request.");
app.UseForwardedHeaders();
app.UseExceptionHandler();
if (!app.Environment.IsDevelopment())
    app.UseHsts();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapLibraryApi();
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHub<SupportHub>("/hubs/support");
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.MapGet("/health", () => Results.Ok(new
{
    status = "OK",
    application = "LibraryHub.Web",
    utc = DateTimeOffset.UtcNow
})).AllowAnonymous();
app.Run();

public partial class Program;
