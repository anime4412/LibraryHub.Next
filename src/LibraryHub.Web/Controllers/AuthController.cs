using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using LibraryHub.Application;
using LibraryHub.Domain;
using LibraryHub.Infrastructure;
using LibraryHub.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryHub.Web.Controllers;

[AutoValidateAntiforgeryToken]
public sealed class AuthController(
    UserManager<LibraryUser> users,
    SignInManager<LibraryUser> signIn,
    RoleManager<IdentityRole<Guid>> roles,
    LibraryDbContext db,
    IEmailSender emailSender,
    IWebHostEnvironment environment,
    BorrowCartFlowService borrowCartFlow,
    ILogger<AuthController> logger) : Controller
{
    [AllowAnonymous]
    [HttpGet("/login")]
    public async Task<IActionResult> Login(
        string? returnUrl = null,
        string? externalError = null,
        string? provider = null)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectFor(User);
        var externalErrorMessage = ExternalErrorMessage(externalError, provider);
        if (externalErrorMessage is not null) TempData["Error"] = externalErrorMessage;
        var safeReturnUrl = SafeReturnUrl(returnUrl);
        if (safeReturnUrl is null && borrowCartFlow.HasPendingAction(TempData))
            safeReturnUrl = Url.Action("Continue", "BorrowCart") ?? "/borrow-cart/continue";
        return await LoginViewAsync(new LoginInput { ReturnUrl = safeReturnUrl });
    }

    [AllowAnonymous]
    [HttpPost("/login")]
    public async Task<IActionResult> Login(LoginInput input)
    {
        input.ReturnUrl = SafeReturnUrl(input.ReturnUrl);
        if (!ModelState.IsValid) return await LoginViewAsync(input);
        var user = await users.FindByEmailAsync(input.Email.Trim());
        if (user is null || !user.IsActive)
        {
            ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");
            return await LoginViewAsync(input);
        }

        if (!user.EmailConfirmed)
        {
            user.EmailConfirmed = true;
            await users.UpdateAsync(user);
        }

        var result = await signIn.PasswordSignInAsync(user, input.Password, input.RememberMe, true);
        if (!result.Succeeded)
        {
            ModelState.AddModelError("", result.IsLockedOut
                ? "Tài khoản đang tạm khóa do đăng nhập sai nhiều lần. Vui lòng thử lại sau 15 phút."
                : "Email hoặc mật khẩu không đúng.");
            return await LoginViewAsync(input);
        }

        var isInternal = await users.IsInRoleAsync(user, "Staff")
            || await users.IsInRoleAsync(user, "Administrator");
        Reader? readerProfile = null;
        if (isInternal)
        {
            var staffLinks = await (from sa in db.StaffAccounts
                                    join e in db.Employees on sa.EmployeeId equals e.Id
                                    where sa.UserAccountId == user.Id
                                    select new { e.IsDeleted, e.Status }).ToListAsync();
            var hasActiveStaff = staffLinks.Any(x => !x.IsDeleted && x.Status == EmploymentStatus.Working);
            if ((staffLinks.Count > 0 && !hasActiveStaff)
                || (!hasActiveStaff && !await users.IsInRoleAsync(user, "Administrator")))
            {
                await signIn.SignOutAsync();
                ModelState.AddModelError("", staffLinks.Count > 0
                    ? "Hồ sơ nhân viên đã bị vô hiệu hóa hoặc ngừng công tác."
                    : "Tài khoản nhân viên chưa được liên kết với hồ sơ nhân viên.");
                return await LoginViewAsync(input);
            }
        }
        else if ((readerProfile = await db.Readers.SingleOrDefaultAsync(
                     x => x.UserAccountId == user.Id && !x.IsDeleted)) is null)
        {
            await signIn.SignOutAsync();
            ModelState.AddModelError("", "Tài khoản chưa được liên kết với hồ sơ độc giả.");
            return await LoginViewAsync(input);
        }

        user.LastLoginAt = DateTimeOffset.UtcNow;
        await users.UpdateAsync(user);
        if (readerProfile is not null)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (readerProfile.LastStreakOn != today)
                readerProfile.ConsecutiveLoginDays = readerProfile.LastStreakOn == today.AddDays(-1)
                    ? readerProfile.ConsecutiveLoginDays + 1
                    : 1;
            readerProfile.LastStreakOn = today;
            readerProfile.LastLoginAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
        }
        if (!string.IsNullOrWhiteSpace(input.ReturnUrl) && Url.IsLocalUrl(input.ReturnUrl))
            return LocalRedirect(input.ReturnUrl);
        return Redirect(isInternal ? "/admin" : "/me");
    }

    [AllowAnonymous]
    [HttpPost("/external-login")]
    public async Task<IActionResult> ExternalLogin(ExternalLoginChallengeInput input)
    {
        var returnUrl = SafeReturnUrl(input.ReturnUrl);
        if (!ExternalAuthenticationSecurity.IsSupportedProvider(input.Provider))
        {
            TempData["Error"] = "Nhà cung cấp đăng nhập này chưa được cấu hình hoặc không được hỗ trợ.";
            return RedirectToAction(nameof(Login), new { returnUrl });
        }

        var schemes = await signIn.GetExternalAuthenticationSchemesAsync();
        var scheme = schemes.SingleOrDefault(x =>
            string.Equals(x.Name, input.Provider, StringComparison.Ordinal));
        if (scheme is null)
        {
            TempData["Error"] = "Nhà cung cấp đăng nhập này chưa được cấu hình hoặc không được hỗ trợ.";
            return RedirectToAction(nameof(Login), new { returnUrl });
        }

        var callbackUrl = Url.Action(
            nameof(ExternalLoginCallback),
            "Auth",
            new { returnUrl })!;
        var properties = signIn.ConfigureExternalAuthenticationProperties(scheme.Name, callbackUrl);
        return Challenge(properties, scheme.Name);
    }

    [AllowAnonymous]
    [HttpGet("/external-login/callback")]
    public async Task<IActionResult> ExternalLoginCallback(
        string? returnUrl = null,
        string? remoteError = null,
        CancellationToken cancellationToken = default)
    {
        var safeReturnUrl = SafeReturnUrl(returnUrl);
        if (!string.IsNullOrWhiteSpace(remoteError))
        {
            logger.LogWarning("Đăng nhập ngoài bị nhà cung cấp từ chối hoặc trả lỗi.");
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            TempData["Error"] = "Đăng nhập bằng nhà cung cấp ngoài đã bị hủy hoặc không thể hoàn tất.";
            return RedirectToAction(nameof(Login), new { returnUrl = safeReturnUrl });
        }

        var info = await signIn.GetExternalLoginInfoAsync();
        if (info is null)
        {
            logger.LogWarning("Không đọc được thông tin đăng nhập ngoài từ cookie tạm thời.");
            TempData["Error"] = "Phiên đăng nhập ngoài không hợp lệ hoặc đã hết hạn. Vui lòng thử lại.";
            return RedirectToAction(nameof(Login), new { returnUrl = safeReturnUrl });
        }
        if (!ExternalAuthenticationSecurity.IsSupportedProvider(info.LoginProvider))
        {
            logger.LogWarning("Callback đăng nhập ngoài dùng nhà cung cấp không được hỗ trợ.");
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            TempData["Error"] = "Callback đăng nhập ngoài không hợp lệ. Vui lòng thử lại.";
            return RedirectToAction(nameof(Login), new { returnUrl = safeReturnUrl });
        }

        var linkedUser = await users.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
        if (linkedUser is not null)
            return await CompleteExternalSignInAsync(linkedUser, info, safeReturnUrl, cancellationToken);

        var email = ExternalEmail(info.Principal);
        if (email is null)
        {
            logger.LogWarning("Nhà cung cấp {Provider} không trả email cho đăng nhập ngoài.", info.LoginProvider);
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            return View("AuthMessage",
                "Nhà cung cấp không trả địa chỉ email. Hãy cấp quyền email hoặc đăng nhập bằng tài khoản LibraryHub.");
        }

        var existingUser = await users.FindByEmailAsync(email);
        if (existingUser is not null)
        {
            return View("ExternalLoginLink", new ExternalLoginLinkInput
            {
                Email = email,
                ProviderDisplayName = ProviderDisplayName(info),
                ReturnUrl = safeReturnUrl
            });
        }

        return View("ExternalLoginConfirmation", new ExternalLoginConfirmationInput
        {
            Email = email,
            DisplayName = ExternalDisplayName(info.Principal, email),
            ProviderDisplayName = ProviderDisplayName(info),
            ReturnUrl = safeReturnUrl
        });
    }

    [AllowAnonymous]
    [HttpPost("/external-login/confirm")]
    public async Task<IActionResult> ExternalLoginConfirm(
        ExternalLoginConfirmationInput input,
        CancellationToken cancellationToken)
    {
        input.ReturnUrl = SafeReturnUrl(input.ReturnUrl);
        var info = await signIn.GetExternalLoginInfoAsync();
        if (info is null)
        {
            TempData["Error"] = "Phiên đăng nhập ngoài không hợp lệ hoặc đã hết hạn. Vui lòng thử lại.";
            return RedirectToAction(nameof(Login), new { returnUrl = input.ReturnUrl });
        }
        if (!ExternalAuthenticationSecurity.IsSupportedProvider(info.LoginProvider))
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            TempData["Error"] = "Callback đăng nhập ngoài không hợp lệ. Vui lòng thử lại.";
            return RedirectToAction(nameof(Login), new { returnUrl = input.ReturnUrl });
        }

        var email = ExternalEmail(info.Principal);
        if (email is null)
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            return View("AuthMessage",
                "Nhà cung cấp không trả địa chỉ email. Hãy cấp quyền email hoặc đăng nhập bằng tài khoản LibraryHub.");
        }

        input.Email = email;
        input.ProviderDisplayName = ProviderDisplayName(info);
        if (!ModelState.IsValid) return View("ExternalLoginConfirmation", input);

        var existingUser = await users.FindByEmailAsync(email);
        if (existingUser is not null)
        {
            return View("ExternalLoginLink", new ExternalLoginLinkInput
            {
                Email = email,
                ProviderDisplayName = ProviderDisplayName(info),
                ReturnUrl = input.ReturnUrl
            });
        }

        var displayName = input.DisplayName.Trim().Normalize(NormalizationForm.FormC);
        var emailVerified = ExternalAuthenticationSecurity.IsProviderEmailVerified(info);
        var user = new LibraryUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            DisplayName = displayName,
            AccountKind = AccountKind.Reader,
            EmailConfirmed = emailVerified
        };
        var settings = await db.SystemSettings.AsNoTracking().SingleOrDefaultAsync(cancellationToken)
            ?? new SystemSettings();

        try
        {
            await db.TransactionAsync(async ct =>
            {
                ThrowIfFailed(await users.CreateAsync(user));
                if (!await roles.RoleExistsAsync("Reader"))
                    ThrowIfFailed(await roles.CreateAsync(new IdentityRole<Guid>("Reader")));
                ThrowIfFailed(await users.AddToRoleAsync(user, "Reader"));
                ThrowIfFailed(await users.AddLoginAsync(user, info));
                db.Readers.Add(new Reader
                {
                    UserAccountId = user.Id,
                    ReaderCode = $"DG-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..21].ToUpperInvariant(),
                    FullName = displayName,
                    Email = email,
                    RegisteredOn = DateOnly.FromDateTime(DateTime.UtcNow),
                    ExpiresOn = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)),
                    Type = input.ReaderType,
                    EmailConfirmed = emailVerified,
                    MaxBorrowBooks = settings.MaxBorrowBooks,
                    IsNameSet = true
                });
                await db.SaveChangesAsync(ct);
                return true;
            }, cancellationToken);
        }
        catch (ExternalIdentityException exception)
        {
            var conflictResult = await ResolveExternalConflictAsync(info, email, input.ReturnUrl, cancellationToken);
            if (conflictResult is not null) return conflictResult;
            foreach (var error in exception.Errors)
                ModelState.AddModelError("", VietnameseUi.IdentityError(error));
            return View("ExternalLoginConfirmation", input);
        }
        catch (DbUpdateException exception)
        {
            logger.LogWarning(
                "Không thể hoàn tất transaction tạo tài khoản đăng nhập ngoài. Loại lỗi: {ExceptionType}.",
                exception.GetType().Name);
            var conflictResult = await ResolveExternalConflictAsync(info, email, input.ReturnUrl, cancellationToken);
            if (conflictResult is not null) return conflictResult;
            ModelState.AddModelError("", "Không thể tạo tài khoản độc giả hoặc lưu đăng nhập ngoài. Vui lòng thử lại.");
            return View("ExternalLoginConfirmation", input);
        }

        logger.LogInformation(
            "Đã tạo tài khoản LibraryHub {UserId} bằng nhà cung cấp {Provider}.",
            user.Id,
            info.LoginProvider);
        if (!emailVerified)
        {
            var delivery = await SendConfirmationEmailAsync(user, displayName, cancellationToken);
            SetEmailOutcome(
                delivery.Result,
                delivery.Url,
                "Tài khoản đã được tạo. Email từ nhà cung cấp chưa đủ điều kiện xác minh tự động; hãy kiểm tra thư để xác thực trước khi đăng nhập.",
                "Tài khoản đã được tạo nhưng email chưa được xác thực và thư xác thực chưa gửi được. Bạn có thể dùng nút “Gửi lại email xác thực”.");
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            return RedirectToAction(nameof(Login), new { returnUrl = input.ReturnUrl });
        }

        return await CompleteExternalSignInAsync(user, info, input.ReturnUrl, cancellationToken);
    }

    [AllowAnonymous]
    [HttpPost("/external-login/link")]
    public async Task<IActionResult> ExternalLoginLink(
        ExternalLoginLinkInput input,
        CancellationToken cancellationToken)
    {
        input.ReturnUrl = SafeReturnUrl(input.ReturnUrl);
        var info = await signIn.GetExternalLoginInfoAsync();
        if (info is null)
        {
            TempData["Error"] = "Phiên đăng nhập ngoài không hợp lệ hoặc đã hết hạn. Vui lòng thử lại.";
            return RedirectToAction(nameof(Login), new { returnUrl = input.ReturnUrl });
        }
        if (!ExternalAuthenticationSecurity.IsSupportedProvider(info.LoginProvider))
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            TempData["Error"] = "Callback đăng nhập ngoài không hợp lệ. Vui lòng thử lại.";
            return RedirectToAction(nameof(Login), new { returnUrl = input.ReturnUrl });
        }

        var email = ExternalEmail(info.Principal);
        input.Email = email ?? "";
        input.ProviderDisplayName = ProviderDisplayName(info);
        if (email is null)
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            return View("AuthMessage", "Nhà cung cấp không trả địa chỉ email nên không thể liên kết tài khoản.");
        }
        if (!ModelState.IsValid) return View("ExternalLoginLink", input);

        var user = await users.FindByEmailAsync(email);
        if (user is null || !user.IsActive)
        {
            ModelState.AddModelError("", "Không thể liên kết với tài khoản LibraryHub này.");
            return View("ExternalLoginLink", input);
        }
        if (!await signIn.CanSignInAsync(user))
        {
            ModelState.AddModelError("", "Tài khoản hiện không được phép liên kết nhà cung cấp ngoài.");
            return View("ExternalLoginLink", input);
        }
        if (await users.IsLockedOutAsync(user))
        {
            ModelState.AddModelError("", "Tài khoản đang tạm khóa. Vui lòng thử lại sau 15 phút.");
            return View("ExternalLoginLink", input);
        }
        if (!await users.CheckPasswordAsync(user, input.Password))
        {
            await users.AccessFailedAsync(user);
            ModelState.AddModelError("", "Mật khẩu LibraryHub không đúng. Tài khoản chưa được liên kết.");
            return View("ExternalLoginLink", input);
        }

        await users.ResetAccessFailedCountAsync(user);
        var result = await users.AddLoginAsync(user, info);
        if (!result.Succeeded)
        {
            var linkedUser = await users.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            if (linkedUser?.Id == user.Id)
                return await CompleteExternalSignInAsync(user, info, input.ReturnUrl, cancellationToken);
            ModelState.AddModelError("", "Không thể liên kết nhà cung cấp này với tài khoản LibraryHub.");
            return View("ExternalLoginLink", input);
        }

        logger.LogInformation(
            "Đã liên kết nhà cung cấp {Provider} với tài khoản LibraryHub {UserId} sau khi xác minh mật khẩu.",
            info.LoginProvider,
            user.Id);
        return await CompleteExternalSignInAsync(user, info, input.ReturnUrl, cancellationToken);
    }

    [AllowAnonymous]
    [HttpGet("/register")]
    public IActionResult Register(string? returnUrl = null) =>
        User.Identity?.IsAuthenticated == true
            ? RedirectFor(User)
            : View(new RegisterInput { ReturnUrl = SafeReturnUrl(returnUrl) });

    [AllowAnonymous]
    [HttpPost("/register")]
    public async Task<IActionResult> Register(RegisterInput input, CancellationToken cancellationToken)
    {
        input.ReturnUrl = SafeReturnUrl(input.ReturnUrl);
        if (!ModelState.IsValid) return View(input);
        var normalizedEmail = input.Email.Trim().ToLowerInvariant();
        var normalizedFullName = input.FullName.Trim().Normalize(NormalizationForm.FormC);
        if (await users.FindByEmailAsync(normalizedEmail) is not null)
        {
            ModelState.AddModelError(nameof(input.Email), "Email đã được sử dụng.");
            return View(input);
        }

        var user = new LibraryUser
        {
            Id = Guid.NewGuid(),
            UserName = normalizedEmail,
            Email = normalizedEmail,
            DisplayName = normalizedFullName,
            AccountKind = AccountKind.Reader,
            EmailConfirmed = true
        };

        var creationErrors = new List<IdentityError>();
        var settings = await db.SystemSettings.AsNoTracking().SingleOrDefaultAsync(cancellationToken)
            ?? new SystemSettings();
        await db.TransactionAsync(async ct =>
        {
            var created = await users.CreateAsync(user, input.Password);
            if (!created.Succeeded)
            {
                creationErrors.AddRange(created.Errors);
                return false;
            }

            if (!await roles.RoleExistsAsync("Reader"))
                await roles.CreateAsync(new IdentityRole<Guid>("Reader"));
            var roleResult = await users.AddToRoleAsync(user, "Reader");
            if (!roleResult.Succeeded)
                throw new InvalidOperationException("Không thể gán vai trò độc giả cho tài khoản.");

            db.Readers.Add(new Reader
            {
                UserAccountId = user.Id,
                ReaderCode = $"DG-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..21].ToUpperInvariant(),
                FullName = normalizedFullName,
                Email = normalizedEmail,
                Phone = input.Phone?.Trim(),
                Address = input.Address?.Trim(),
                RegisteredOn = DateOnly.FromDateTime(DateTime.UtcNow),
                ExpiresOn = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)),
                Type = input.ReaderType,
                EmailConfirmed = true,
                MaxBorrowBooks = settings.MaxBorrowBooks
            });
            await db.SaveChangesAsync(ct);
            return true;
        }, cancellationToken);

        if (creationErrors.Count > 0)
        {
            foreach (var error in creationErrors)
                ModelState.AddModelError("", VietnameseUi.IdentityError(error));
            return View(input);
        }

        TempData["Notice"] = "Đăng ký tài khoản thành công! Bạn có thể đăng nhập ngay bây giờ.";
        return RedirectToAction(nameof(Login), new { returnUrl = input.ReturnUrl });
    }

    [AllowAnonymous]
    [HttpPost("/resend-confirmation")]
    public async Task<IActionResult> ResendConfirmation(
        ResendConfirmationInput input,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Vui lòng nhập một địa chỉ email hợp lệ.";
            return RedirectToAction(nameof(Login), new { returnUrl = input.ReturnUrl });
        }

        var email = input.Email.Trim().ToLowerInvariant();
        var user = await users.FindByEmailAsync(email);
        if (user is null || !user.IsActive)
        {
            TempData["Notice"] =
                "Nếu email thuộc một tài khoản đang chờ xác thực, hệ thống sẽ xử lý yêu cầu gửi lại.";
            return RedirectToAction(nameof(Login), new { returnUrl = input.ReturnUrl });
        }
        if (user.EmailConfirmed)
        {
            TempData["Notice"] = "Email này đã được xác thực. Bạn có thể đăng nhập.";
            return RedirectToAction(nameof(Login), new { returnUrl = input.ReturnUrl });
        }

        var delivery = await SendConfirmationEmailAsync(user, user.DisplayName, cancellationToken);
        SetEmailOutcome(
            delivery.Result,
            delivery.Url,
            "Email xác thực đã được gửi lại. Hãy kiểm tra hộp thư đến và thư rác.",
            "Chưa gửi được email xác thực. Vui lòng kiểm tra cấu hình SMTP hoặc thử lại sau.");
        return RedirectToAction(nameof(Login), new { returnUrl = input.ReturnUrl });
    }

    [AllowAnonymous]
    [HttpGet("/forgot-password")]
    public IActionResult ForgotPassword() => View(new ForgotPasswordInput());

    [AllowAnonymous]
    [HttpPost("/forgot-password")]
    public async Task<IActionResult> ForgotPassword(
        ForgotPasswordInput input,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(input);
        var email = input.Email.Trim().ToLowerInvariant();
        var user = await users.FindByEmailAsync(email);

        if (user is null || !user.IsActive)
        {
            TempData["Notice"] =
                "Nếu email thuộc một tài khoản hợp lệ, hệ thống sẽ xử lý yêu cầu đặt lại mật khẩu.";
            return RedirectToAction(nameof(Login));
        }

        var token = await users.GeneratePasswordResetTokenAsync(user);
        var resetUrl = Url.ActionLink(
            nameof(ResetPassword),
            "Auth",
            new { userId = user.Id, token },
            Request.Scheme)!;
        var result = await emailSender.SendAsync(
            email,
            "Đặt lại mật khẩu LibraryHub",
            BuildEmail(
                "Đặt lại mật khẩu",
                $"Xin chào {user.DisplayName},",
                "LibraryHub nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.",
                "Đặt lại mật khẩu",
                resetUrl,
                "Nếu bạn không thực hiện yêu cầu này, hãy bỏ qua email."),
            cancellationToken);
        SetEmailOutcome(
            result,
            resetUrl,
            "Hướng dẫn đặt lại mật khẩu đã được gửi. Hãy kiểm tra hộp thư đến và thư rác.",
            "Chưa gửi được email đặt lại mật khẩu. Vui lòng kiểm tra cấu hình SMTP hoặc thử lại sau.");
        if (environment.IsDevelopment() && !result.Succeeded)
            TempData["DevelopmentLinkLabel"] = "Mở liên kết đặt lại mật khẩu trong môi trường phát triển";
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    [HttpGet("/reset-password")]
    public IActionResult ResetPassword(Guid userId, string token)
    {
        if (userId == Guid.Empty || string.IsNullOrWhiteSpace(token))
            return View("AuthMessage", "Liên kết đặt lại mật khẩu không hợp lệ.");
        return View(new ResetPasswordInput { UserId = userId, Token = token });
    }

    [AllowAnonymous]
    [HttpPost("/reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordInput input)
    {
        if (!ModelState.IsValid) return View(input);
        var user = await users.FindByIdAsync(input.UserId.ToString());
        if (user is null)
            return View("AuthMessage", "Liên kết đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.");

        var result = await users.ResetPasswordAsync(user, input.Token, input.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError("", VietnameseUi.IdentityError(error));
            return View(input);
        }

        TempData["Success"] = "Mật khẩu đã được đặt lại. Bạn có thể đăng nhập bằng mật khẩu mới.";
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    [HttpGet("/confirm-email")]
    public async Task<IActionResult> ConfirmEmail(Guid userId, string token)
    {
        var user = await users.FindByIdAsync(userId.ToString());
        if (user is null) return View("AuthMessage", "Liên kết xác thực không hợp lệ.");
        var result = await users.ConfirmEmailAsync(user, token);
        if (result.Succeeded)
        {
            var reader = await db.Readers.SingleOrDefaultAsync(x => x.UserAccountId == user.Id);
            if (reader is not null)
            {
                reader.EmailConfirmed = true;
                await db.SaveChangesAsync();
            }
        }
        return View("AuthMessage", result.Succeeded
            ? "Email đã được xác thực. Bạn có thể đăng nhập."
            : "Liên kết xác thực không hợp lệ hoặc đã hết hạn.");
    }

    [Authorize]
    [HttpPost("/logout")]
    public async Task<IActionResult> Logout()
    {
        await signIn.SignOutAsync();
        return Redirect("/");
    }

    [AllowAnonymous]
    [HttpGet("/access-denied")]
    public IActionResult AccessDenied() => View();

    private async Task<IActionResult> LoginViewAsync(LoginInput input)
    {
        input.ExternalProviders = (await signIn.GetExternalAuthenticationSchemesAsync())
            .Where(x => ExternalAuthenticationSecurity.IsSupportedProvider(x.Name))
            .Where(x => !string.IsNullOrWhiteSpace(x.DisplayName))
            .Select(x => new ExternalProviderOption(x.Name, ProviderDisplayName(x.Name), true))
            .OrderBy(x => x.Name, StringComparer.Ordinal)
            .ToList();
        return View("Login", input);
    }

    private async Task<IActionResult> CompleteExternalSignInAsync(
        LibraryUser user,
        ExternalLoginInfo info,
        string? returnUrl,
        CancellationToken cancellationToken)
    {
        if (!user.IsActive)
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            TempData["Error"] = "Tài khoản đã ngừng hoạt động và không được phép đăng nhập.";
            return RedirectToAction(nameof(Login), new { returnUrl });
        }
        if (await users.IsLockedOutAsync(user))
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            TempData["Error"] = "Tài khoản đang tạm khóa. Vui lòng thử lại sau 15 phút.";
            return RedirectToAction(nameof(Login), new { returnUrl });
        }
        if (!await signIn.CanSignInAsync(user))
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            TempData["Error"] = "Tài khoản hiện chưa được phép đăng nhập. Hãy xác thực email hoặc liên hệ quản trị viên.";
            return RedirectToAction(nameof(Login), new { returnUrl });
        }

        var isInternal = await users.IsInRoleAsync(user, "Staff")
            || await users.IsInRoleAsync(user, "Administrator");
        Reader? reader = null;
        if (isInternal)
        {
            var staffLinks = await (from sa in db.StaffAccounts
                                    join e in db.Employees on sa.EmployeeId equals e.Id
                                    where sa.UserAccountId == user.Id
                                    select new { e.IsDeleted, e.Status }).ToListAsync(cancellationToken);
            var hasActiveStaff = staffLinks.Any(x => !x.IsDeleted && x.Status == EmploymentStatus.Working);
            if ((staffLinks.Count > 0 && !hasActiveStaff)
                || (!hasActiveStaff && !await users.IsInRoleAsync(user, "Administrator")))
            {
                await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
                TempData["Error"] = staffLinks.Count > 0
                    ? "Hồ sơ nhân viên đã bị vô hiệu hóa hoặc ngừng công tác."
                    : "Tài khoản nhân viên chưa được liên kết với hồ sơ nhân viên.";
                return RedirectToAction(nameof(Login), new { returnUrl });
            }
        }
        else
        {
            reader = await db.Readers.SingleOrDefaultAsync(
                x => x.UserAccountId == user.Id && !x.IsDeleted,
                cancellationToken);
            if (reader is null)
            {
                await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
                TempData["Error"] = "Tài khoản chưa được liên kết với hồ sơ độc giả.";
                return RedirectToAction(nameof(Login), new { returnUrl });
            }
        }

        var result = await signIn.ExternalLoginSignInAsync(
            info.LoginProvider,
            info.ProviderKey,
            isPersistent: false,
            bypassTwoFactor: false);
        if (!result.Succeeded)
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            TempData["Error"] = result.IsLockedOut
                ? "Tài khoản đang tạm khóa. Vui lòng thử lại sau 15 phút."
                : result.IsNotAllowed
                    ? "Tài khoản chưa được phép đăng nhập. Hãy xác thực email hoặc liên hệ quản trị viên."
                    : result.RequiresTwoFactor
                        ? "Tài khoản yêu cầu bước xác thực bổ sung trước khi đăng nhập."
                        : "Không thể hoàn tất đăng nhập ngoài. Vui lòng thử lại.";
            return RedirectToAction(nameof(Login), new { returnUrl });
        }

        user.LastLoginAt = DateTimeOffset.UtcNow;
        ThrowIfFailed(await users.UpdateAsync(user));
        if (reader is not null)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (reader.LastStreakOn != today)
                reader.ConsecutiveLoginDays = reader.LastStreakOn == today.AddDays(-1)
                    ? reader.ConsecutiveLoginDays + 1
                    : 1;
            reader.LastStreakOn = today;
            reader.LastLoginAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
        }
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        return !string.IsNullOrWhiteSpace(returnUrl)
            ? LocalRedirect(returnUrl)
            : Redirect(isInternal ? "/admin" : "/me");
    }

    private string? SafeReturnUrl(string? returnUrl) =>
        !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? returnUrl
            : null;

    private string? ExternalEmail(ClaimsPrincipal principal)
    {
        var email = principal.FindFirstValue(ClaimTypes.Email)
            ?? principal.FindFirstValue("email");
        if (string.IsNullOrWhiteSpace(email) || !new EmailAddressAttribute().IsValid(email)) return null;
        var normalizedEmail = users.NormalizeEmail(email.Trim());
        return string.IsNullOrWhiteSpace(normalizedEmail) ? null : email.Trim().ToLowerInvariant();
    }

    private static string ExternalDisplayName(ClaimsPrincipal principal, string email)
    {
        var name = principal.FindFirstValue(ClaimTypes.Name)?.Trim();
        if (string.IsNullOrWhiteSpace(name)) name = email.Split('@')[0];
        name = name.Normalize(NormalizationForm.FormC);
        return name.Length <= 120 ? name : name[..120];
    }

    private static string ProviderDisplayName(ExternalLoginInfo info) =>
        ProviderDisplayName(info.LoginProvider);

    private static string ProviderDisplayName(string provider) => provider switch
    {
        "Google" => "Google",
        "Facebook" => "Facebook",
        _ => "nhà cung cấp ngoài"
    };

    private static string? ExternalErrorMessage(string? error, string? provider)
    {
        if (string.IsNullOrWhiteSpace(error)) return null;
        var providerName = ProviderDisplayName(provider ?? "");
        return error switch
        {
            "access-denied" => $"Bạn đã hủy hoặc từ chối đăng nhập bằng {providerName}.",
            "remote-failure" => $"Không thể hoàn tất đăng nhập bằng {providerName}. Vui lòng thử lại.",
            _ => "Callback đăng nhập ngoài không hợp lệ hoặc đã hết hạn. Vui lòng thử lại."
        };
    }

    private async Task<IActionResult?> ResolveExternalConflictAsync(
        ExternalLoginInfo info,
        string email,
        string? returnUrl,
        CancellationToken cancellationToken)
    {
        db.ChangeTracker.Clear();
        var linkedUser = await users.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
        if (linkedUser is not null)
            return await CompleteExternalSignInAsync(linkedUser, info, returnUrl, cancellationToken);

        var existingUser = await users.FindByEmailAsync(email);
        if (existingUser is null) return null;
        return View("ExternalLoginLink", new ExternalLoginLinkInput
        {
            Email = email,
            ProviderDisplayName = ProviderDisplayName(info),
            ReturnUrl = returnUrl
        });
    }

    private static void ThrowIfFailed(IdentityResult result)
    {
        if (!result.Succeeded) throw new ExternalIdentityException(result.Errors);
    }

    private IActionResult RedirectFor(System.Security.Claims.ClaimsPrincipal principal) =>
        principal.IsInRole("Administrator") || principal.IsInRole("Staff")
            ? Redirect("/admin")
            : Redirect("/me");

    private async Task<(EmailDeliveryResult Result, string Url)> SendConfirmationEmailAsync(
        LibraryUser user,
        string displayName,
        CancellationToken cancellationToken)
    {
        var token = await users.GenerateEmailConfirmationTokenAsync(user);
        var confirmationUrl = Url.ActionLink(
            nameof(ConfirmEmail),
            "Auth",
            new { userId = user.Id, token },
            Request.Scheme)!;
        var result = await emailSender.SendAsync(
            user.Email!,
            "Xác thực tài khoản LibraryHub",
            BuildEmail(
                "Xác thực tài khoản",
                $"Xin chào {displayName},",
                "Cảm ơn bạn đã đăng ký LibraryHub. Hãy xác thực email để kích hoạt tài khoản và bắt đầu mượn sách.",
                "Xác thực email",
                confirmationUrl,
                "Liên kết này chỉ dành cho tài khoản của bạn. Nếu bạn không đăng ký, hãy bỏ qua email."),
            cancellationToken);
        logger.LogInformation(
            "Kết quả gửi email xác thực cho tài khoản {UserId}: {DeliveryStatus}.",
            user.Id,
            result.Succeeded ? "thành công" : result.IsConfigured ? "gửi thất bại" : "chưa cấu hình");
        return (result, confirmationUrl);
    }

    private void SetEmailOutcome(
        EmailDeliveryResult result,
        string developmentUrl,
        string successMessage,
        string failureMessage)
    {
        if (result.Succeeded)
        {
            TempData["Success"] = successMessage;
            return;
        }

        TempData["Warning"] = failureMessage;
        if (environment.IsDevelopment())
        {
            TempData["DevelopmentUrl"] = developmentUrl;
            TempData["DevelopmentLinkLabel"] ??= "Mở liên kết xác thực trong môi trường phát triển";
        }
    }

    private static string BuildEmail(
        string heading,
        string greeting,
        string message,
        string buttonLabel,
        string actionUrl,
        string footer)
    {
        var encoder = HtmlEncoder.Default;
        return $$"""
            <!doctype html>
            <html lang="vi">
            <body style="margin:0;background:#f5f0e5;color:#17352f;font-family:Arial,sans-serif">
              <div style="max-width:620px;margin:32px auto;background:#fffdf7;border:1px solid #d9d1bf;border-radius:18px;overflow:hidden">
                <div style="padding:24px 32px;background:#174c42;color:#fff">
                  <strong style="font-size:22px">LibraryHub</strong>
                </div>
                <div style="padding:32px">
                  <h1 style="margin:0 0 20px;font-size:28px">{{encoder.Encode(heading)}}</h1>
                  <p>{{encoder.Encode(greeting)}}</p>
                  <p style="line-height:1.7">{{encoder.Encode(message)}}</p>
                  <p style="margin:28px 0">
                    <a href="{{encoder.Encode(actionUrl)}}" style="display:inline-block;padding:13px 20px;background:#2f6f5e;color:#fff;text-decoration:none;border-radius:10px;font-weight:700">{{encoder.Encode(buttonLabel)}}</a>
                  </p>
                  <p style="font-size:13px;color:#5e6f69;line-height:1.6">{{encoder.Encode(footer)}}</p>
                </div>
              </div>
            </body>
            </html>
            """;
    }
}

public sealed class LoginInput
{
    [Display(Name = "Email")]
    [Required(ErrorMessage = "Vui lòng nhập email.")]
    [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
    public string Email { get; set; } = "";

    [Display(Name = "Mật khẩu")]
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    public bool RememberMe { get; set; }
    public string? ReturnUrl { get; set; }
    public bool CanResendConfirmation { get; set; }
    public IReadOnlyList<ExternalProviderOption> ExternalProviders { get; set; } = [];
}

public sealed record ExternalProviderOption(string Name, string DisplayName, bool IsConfigured);

public sealed class ExternalLoginChallengeInput
{
    [Required]
    public string Provider { get; set; } = "";
    public string? ReturnUrl { get; set; }
}

public sealed class ExternalLoginConfirmationInput
{
    public string Email { get; set; } = "";
    public string ProviderDisplayName { get; set; } = "";
    public string? ReturnUrl { get; set; }

    [Display(Name = "Họ và tên")]
    [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "Họ và tên phải có từ 2 đến 120 ký tự.")]
    public string DisplayName { get; set; } = "";

    [Display(Name = "Kiểu độc giả")]
    public ReaderType ReaderType { get; set; } = ReaderType.Student;
}

public sealed class ExternalLoginLinkInput
{
    public string Email { get; set; } = "";
    public string ProviderDisplayName { get; set; } = "";
    public string? ReturnUrl { get; set; }

    [Display(Name = "Mật khẩu LibraryHub")]
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu LibraryHub để xác nhận liên kết.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";
}

internal sealed class ExternalIdentityException(IEnumerable<IdentityError> errors) : Exception
{
    public IReadOnlyList<IdentityError> Errors { get; } = errors.ToList();
}

public sealed class RegisterInput
{
    public string? ReturnUrl { get; set; }

    [Display(Name = "Họ và tên")]
    [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "Họ và tên phải có từ 2 đến 120 ký tự.")]
    public string FullName { get; set; } = "";

    [Display(Name = "Email")]
    [Required(ErrorMessage = "Vui lòng nhập email.")]
    [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
    public string Email { get; set; } = "";

    [Display(Name = "Mật khẩu")]
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
    [DataType(DataType.Password)]
    [MinLength(8, ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự.")]
    [LibraryPassword]
    public string Password { get; set; } = "";

    [Display(Name = "Nhập lại mật khẩu")]
    [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu.")]
    [Compare(nameof(Password), ErrorMessage = "Mật khẩu nhập lại không khớp.")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = "";

    [RegularExpression(@"^(0|\+84)(3|5|7|8|9)\d{8}$", ErrorMessage = "Số điện thoại Việt Nam không hợp lệ.")]
    public string? Phone { get; set; }
    [StringLength(300, ErrorMessage = "Địa chỉ không được vượt quá 300 ký tự.")]
    public string? Address { get; set; }
    [Display(Name = "Kiểu độc giả")]
    [Required(ErrorMessage = "Vui lòng chọn kiểu độc giả.")]
    public ReaderType ReaderType { get; set; } = ReaderType.Student;
}

public sealed class ResendConfirmationInput
{
    [Required(ErrorMessage = "Vui lòng nhập email.")]
    [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
    public string Email { get; set; } = "";
    public string? ReturnUrl { get; set; }
}

public sealed class ForgotPasswordInput
{
    [Display(Name = "Email")]
    [Required(ErrorMessage = "Vui lòng nhập email.")]
    [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
    public string Email { get; set; } = "";
}

public sealed class ResetPasswordInput
{
    [Required(ErrorMessage = "Liên kết đặt lại mật khẩu không hợp lệ.")]
    public Guid UserId { get; set; }
    [Required(ErrorMessage = "Liên kết đặt lại mật khẩu không hợp lệ.")]
    public string Token { get; set; } = "";

    [Display(Name = "Mật khẩu mới")]
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
    [DataType(DataType.Password)]
    [MinLength(8, ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự.")]
    [LibraryPassword]
    public string Password { get; set; } = "";

    [Display(Name = "Nhập lại mật khẩu mới")]
    [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu mới.")]
    [Compare(nameof(Password), ErrorMessage = "Mật khẩu nhập lại không khớp.")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = "";
}
