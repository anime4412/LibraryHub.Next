using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SOF1022_NHOM1_SD2003_Ver2.Attributes;
using SOF1022_NHOM1_SD2003_Ver2.Datas;
using SOF1022_NHOM1_SD2003_Ver2.Models;
using SOF1022_NHOM1_SD2003_Ver2.Models.ViewModels;
using SOF1022_NHOM1_SD2003_Ver2.Models.Enums;
using SOF1022_NHOM1_SD2003_Ver2.Services;
using SOF1022_NHOM1_SD2003_Ver2.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Facebook;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

public class AccountController : Controller
{
    private readonly LibraryDbContext _context;
    private readonly IEmailService _emailService;


    public AccountController(LibraryDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password, bool rememberMe = false)
    {
        var identifier = (email ?? "").Trim();
        var rawPassword = (password ?? "").Trim();

        if (string.IsNullOrWhiteSpace(identifier) || string.IsNullOrWhiteSpace(rawPassword))
        {
            ModelState.AddModelError("", "Vui lòng nhập đầy đủ tài khoản và mật khẩu");
            return View();
        }

        // Support login by Email, ReaderCode, EmployeeId, Phone, or aliases
        var normalizedId = identifier.ToLower();

        var user = await _context.Readers.FirstOrDefaultAsync(x =>
            x.Email.ToLower() == normalizedId ||
            x.ReaderCode.ToLower() == normalizedId ||
            x.Phone == identifier ||
            // Admin
            ((normalizedId == "admin" || normalizedId == "nv001" || normalizedId == "admin@library.com") && (x.Role == "Admin" || x.Email == "admin@library.com")) ||
            // Quản lý / Manager
            ((normalizedId == "quanly@library.com" || normalizedId == "manager@library.com" || normalizedId == "quanly" || normalizedId == "manager" || normalizedId == "nv002" || normalizedId == "rd_nv002") && x.Email == "quanly@library.com") ||
            // Thủ thư / Librarian
            ((normalizedId == "thuthu@library.com" || normalizedId == "librarian@library.com" || normalizedId == "thuthu" || normalizedId == "librarian" || normalizedId == "nv003" || normalizedId == "rd_nv003") && x.Email == "thuthu@library.com") ||
            // Thủ kho / Warehouse
            ((normalizedId == "kho@library.com" || normalizedId == "warehouse@library.com" || normalizedId == "kho" || normalizedId == "warehouse" || normalizedId == "nv004" || normalizedId == "rd_nv004") && x.Email == "kho@library.com") ||
            // CSKH / Support
            ((normalizedId == "cskh@library.com" || normalizedId == "support@library.com" || normalizedId == "cskh" || normalizedId == "support" || normalizedId == "nv006" || normalizedId == "rd_nv006") && x.Email == "cskh@library.com") ||
            // Độc giả / Reader
            ((normalizedId == "reader@library.com" || normalizedId == "docgia@gmail.com" || normalizedId == "reader" || normalizedId == "docgia" || normalizedId == "dg001") && (x.Role == "Reader" || x.Email == "reader@library.com" || x.Email == "docgia@gmail.com"))
        );

        if (user == null)
        {
            var emp = await _context.Employees.FirstOrDefaultAsync(e =>
                e.Email.ToLower() == normalizedId ||
                e.EmployeeId.ToLower() == normalizedId ||
                e.Phone == identifier
            );
            if (emp != null)
            {
                user = await _context.Readers.FirstOrDefaultAsync(x => x.Email.ToLower() == emp.Email.ToLower());
            }
        }

        if (user == null)
        {
            ModelState.AddModelError("", "Sai tài khoản hoặc mật khẩu");
            return View();
        }

        var hasher = new PasswordHasher<Readers>();
        bool isPasswordValid = false;

        // 1. Check standard default passwords for convenience
        bool isDefaultAdminPassword = (user.Role == "Admin" || user.Email == "admin@library.com") &&
            (rawPassword == "Admin@123" || rawPassword == "123456" || rawPassword.ToLower() == "admin");

        bool isDefaultReaderPassword = (user.Role == "Reader" || user.Email == "reader@library.com" || user.Email == "docgia@gmail.com") &&
            (rawPassword == "Reader@123" || rawPassword == "Docgia@123" || rawPassword == "123456" || rawPassword.ToLower() == "reader");

        bool isDefaultStaffPassword = (user.Role != "Reader" && !string.IsNullOrEmpty(user.Role)) &&
            (rawPassword.Equals("Quanly@123", StringComparison.OrdinalIgnoreCase) ||
             rawPassword.Equals("Manager@123", StringComparison.OrdinalIgnoreCase) ||
             rawPassword.Equals("Thuthu@123", StringComparison.OrdinalIgnoreCase) ||
             rawPassword.Equals("Librarian@123", StringComparison.OrdinalIgnoreCase) ||
             rawPassword.Equals("Kho@123", StringComparison.OrdinalIgnoreCase) ||
             rawPassword.Equals("Warehouse@123", StringComparison.OrdinalIgnoreCase) ||
             rawPassword.Equals("Cskh@123", StringComparison.OrdinalIgnoreCase) ||
             rawPassword.Equals("Support@123", StringComparison.OrdinalIgnoreCase) ||
             rawPassword.Equals("Admin@123", StringComparison.OrdinalIgnoreCase) ||
             rawPassword == "123456");

        bool isPlainTextMatch = (!string.IsNullOrEmpty(user.Password) && user.Password == rawPassword);

        if (isDefaultAdminPassword || isDefaultReaderPassword || isDefaultStaffPassword || isPlainTextMatch)
        {
            isPasswordValid = true;
            user.Password = hasher.HashPassword(user, rawPassword);
            try
            {
                _context.Readers.Update(user);
                await _context.SaveChangesAsync();
            }
            catch { }
        }
        else
        {
            try
            {
                var verifyResult = hasher.VerifyHashedPassword(
                    user,
                    user.Password ?? "",
                    rawPassword
                );

                if (verifyResult != PasswordVerificationResult.Failed)
                {
                    isPasswordValid = true;
                }
            }
            catch
            {
                if (user.Password == rawPassword)
                {
                    isPasswordValid = true;
                    user.Password = hasher.HashPassword(user, rawPassword);
                }
            }
        }

        if (!isPasswordValid)
        {
            ModelState.AddModelError("", "Sai tài khoản hoặc mật khẩu");
            return View();
        }

        // Kiểm tra trạng thái tài khoản bị khóa / tạm ngưng
        if (!user.IsActive && user.Role != "Admin")
        {
            ModelState.AddModelError("", "Tài khoản của bạn đã bị tạm ngưng hoặc khóa bởi Quản trị viên.");
            return View();
        }

        // Tự động xác thực email cho admin hoặc độc giả đã có thông tin
        user.EmailConfirmed = true;
        if (user.Role == "Admin" || (!string.IsNullOrEmpty(user.Phone) && !string.IsNullOrEmpty(user.Address)))
        {
            user.IsVerified = true;
        }

        // Set session
        HttpContext.Session.SetString("UserId", user.Id.ToString());
        HttpContext.Session.SetString("UserName", user.FullName ?? "");
        HttpContext.Session.SetString("Role", user.Role ?? "Reader");
        HttpContext.Session.SetString("RememberMe", rememberMe.ToString());
        HttpContext.Session.SetString("IsVerified", user.IsVerified ? "true" : "false");

        // Save EmployeeId to Session if user email matches an Employee
        var employee = await _context.Employees.Include(e => e.Role).FirstOrDefaultAsync(e => e.Email == user.Email && e.StatusId == 1 && !e.IsDeleted);
        if (employee != null)
        {
            HttpContext.Session.SetString("EmployeeId", employee.Id.ToString());
            if (employee.Role != null && !string.IsNullOrEmpty(employee.Role.Name))
            {
                if (employee.Role.Name == "Admin" && user.Role != "Admin")
                {
                    user.Role = "Admin";
                    _context.Readers.Update(user);
                    await _context.SaveChangesAsync();
                    HttpContext.Session.SetString("Role", "Admin");
                }
            }
        }

        // Sign in via Cookie Authentication as well
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName ?? user.Email ?? "User"),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            new Claim(ClaimTypes.Role, user.Role ?? "Reader")
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = rememberMe,
            ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(7) : DateTimeOffset.UtcNow.AddHours(12)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties
        );

        // Update login streak
        var today = DateTime.Today;
        if (user.LastStreakDate.HasValue)
        {
            var lastStreakDate = user.LastStreakDate.Value.Date;
            if (lastStreakDate == today.AddDays(-1))
            {
                user.ConsecutiveLoginDays++;
            }
            else if (lastStreakDate < today.AddDays(-1))
            {
                user.ConsecutiveLoginDays = 1;
            }
        }
        else
        {
            user.ConsecutiveLoginDays = 1;
        }

        user.LastLoginDate = DateTime.Now;
        user.LastStreakDate = today;
        _context.Readers.Update(user);
        await _context.SaveChangesAsync();

        // Check if user needs to change password
        if (user.ForcePasswordChange)
        {
            TempData["ForcePasswordChange"] = true;
            return RedirectToAction("ChangePassword");
        }

        // Nhân viên và Quản trị viên được chuyển thẳng vào trang Quản trị Dashboard
        if (user.Role != "Reader" && !string.IsNullOrEmpty(user.Role))
        {
            return RedirectToAction("Index", "Dashboard");
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Edit()
    {
        return RedirectToAction(nameof(Profile));
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(Readers reader)
    {
        ModelState.Remove("ReaderCode");
        ModelState.Remove("ReadersType");
        ModelState.Remove("Genders");
        reader.Email = reader.Email?.Trim();
        reader.FullName = reader.FullName?.Trim();
        reader.Phone = reader.Phone?.Trim();
        reader.IdentityNumber = reader.IdentityNumber?.Trim();
        reader.Address = reader.Address?.Trim();

        if (!string.IsNullOrWhiteSpace(reader.Email))
        {
            bool emailExists = await _context.Readers.AnyAsync(
                x => x.Email!.ToLower() == reader.Email!.ToLower());

            if (emailExists)
            {
                ModelState.AddModelError("Email", "Email đã tồn tại");
            }
        }
        foreach (var item in ModelState)
        {
            foreach (var error in item.Value.Errors)
            {
                Console.WriteLine($"KEY: {item.Key}");
                Console.WriteLine($"ERROR: {error.ErrorMessage}");
            }
        }
        if (ModelState.IsValid)
        {
            try
            {
                // Sử dụng transaction để tránh race condition
                using var transaction = await _context.Database.BeginTransactionAsync();

                // Check lại email trong transaction để đảm bảo không trùng
                bool emailExistsInTransaction = await _context.Readers.AnyAsync(
                    x => x.Email!.ToLower() == reader.Email!.ToLower());

                if (emailExistsInTransaction)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("Email", "Email đã tồn tại");
                    return View(reader);
                }

                // Lấy ReaderCode mới với lock để tránh trùng
                var maxReaderCode = await _context.Readers
                    .Where(x => x.ReaderCode != null && x.ReaderCode.StartsWith("DG"))
                    .OrderByDescending(x => x.ReaderCode)
                    .Select(x => x.ReaderCode)
                    .FirstOrDefaultAsync();

                int nextNumber = 1;
                if (!string.IsNullOrEmpty(maxReaderCode))
                {
                    string numberPart = maxReaderCode.Substring(2);
                    if (int.TryParse(numberPart, out int currentNumber))
                    {
                        nextNumber = currentNumber + 1;
                    }
                }

                reader.ReaderCode = $"DG{nextNumber:0004}";
                reader.RegisterDate = DateTime.Now;
                reader.ExpirationDate = reader.RegisterDate.AddYears(1);
                reader.Role = "Reader";
                reader.IsActive = true;
                reader.Avatar ??= "/images/default-avatar.png";
                reader.EmailConfirmed = false;
                reader.EmailConfirmationToken = Guid.NewGuid().ToString();
                reader.EmailConfirmationSentDate = DateTime.Now;

                var hasher = new PasswordHasher<Readers>();
                reader.Password = hasher.HashPassword(reader, reader.Password!);

                Console.WriteLine("Password sau khi hash:");
                Console.WriteLine(reader.Password);

                _context.Readers.Add(reader);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Gửi email xác nhận
                try
                {
                    var confirmationLink = Url.Action(nameof(ConfirmEmail), "Account",
                        new { token = reader.EmailConfirmationToken, email = reader.Email }, Request.Scheme);

                    await _emailService.SendEmailConfirmationAsync(reader.Email!, confirmationLink, reader.FullName!);
                }
                catch (Exception emailEx)
                {
                    Console.WriteLine($"Lỗi gửi email: {emailEx.Message}");
                    TempData["Error"] = "Đăng ký thành công nhưng không thể gửi email xác nhận. Vui lòng yêu cầu gửi lại.";
                }

                TempData["Success"] = "Đăng ký thành công. Vui lòng kiểm tra email để xác nhận tài khoản.";
                ViewData["Email"] = reader.Email;
                return View("EmailConfirmationSent");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Đăng ký thất bại. Vui lòng thử lại.");
                Console.WriteLine("===== ERROR =====");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.InnerException?.Message);
                Console.WriteLine(ex.InnerException?.InnerException?.Message);
                return View(reader);
            }
        }

        return View(reader);
    }

    public async Task<IActionResult> ConfirmEmail(string token, string email)
    {
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
        {
            TempData["Error"] = "Liên kết xác nhận không hợp lệ.";
            return RedirectToAction("Login");
        }

        var user = await _context.Readers.FirstOrDefaultAsync(x => x.Email == email);

        if (user == null)
        {
            TempData["Error"] = "Không tìm thấy tài khoản.";
            return RedirectToAction("Login");
        }

        if (user.EmailConfirmed)
        {
            TempData["Success"] = "Email đã được xác nhận trước đó.";
            return RedirectToAction("Login");
        }

        if (user.EmailConfirmationToken != token)
        {
            TempData["Error"] = "Liên kết xác nhận không hợp lệ hoặc đã hết hạn.";
            return RedirectToAction("Login");
        }

        if (user.EmailConfirmationSentDate.HasValue &&
            (DateTime.Now - user.EmailConfirmationSentDate.Value).TotalHours > 24)
        {
            TempData["Error"] = "Liên kết xác nhận đã hết hạn (24 giờ).";
            return RedirectToAction("Login");
        }

        user.EmailConfirmed = true;
        user.EmailConfirmationToken = null;
        user.EmailConfirmationSentDate = null;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Email đã được xác nhận thành công. Bạn có thể đăng nhập ngay.";
        return RedirectToAction("Login");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendConfirmationEmail(string email)
    {
        var user = await _context.Readers.FirstOrDefaultAsync(x => x.Email == email);

        if (user == null)
        {
            TempData["Error"] = "Không tìm thấy tài khoản với email này.";
            return RedirectToAction("Login");
        }

        if (user.EmailConfirmed)
        {
            TempData["Success"] = "Email đã được xác nhận trước đó. Bạn có thể đăng nhập.";
            return RedirectToAction("Login");
        }

        // Generate new token
        user.EmailConfirmationToken = Guid.NewGuid().ToString();
        user.EmailConfirmationSentDate = DateTime.Now;

        await _context.SaveChangesAsync();

        try
        {
            var confirmationLink = Url.Action(nameof(ConfirmEmail), "Account",
                new { token = user.EmailConfirmationToken, email = user.Email }, Request.Scheme);

            await _emailService.SendEmailConfirmationAsync(user.Email!, confirmationLink, user.FullName!);
            TempData["Success"] = "Email xác nhận đã được gửi lại. Vui lòng kiểm tra hộp thư.";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi gửi email: {ex.Message}");
            TempData["Error"] = "Không thể gửi email xác nhận. Vui lòng thử lại sau.";
        }

        return RedirectToAction("Login");
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        TempData["Success"] = "Đăng xuất thành công.";
        return RedirectToAction("Login", "Account");
    }

    public IActionResult AccessDenied()
    {
        return View();
    }

    private async Task<Readers> CreateOrGetReaderFromOAuth(string email, string fullName, string providerPrefix)
    {
        var user = await _context.Readers.FirstOrDefaultAsync(x => x.Email == email);

        if (user == null)
        {
            var hasher = new PasswordHasher<Readers>();
            
            // Get next ReaderCode
            var maxReaderCode = await _context.Readers
                .Where(x => x.ReaderCode != null && x.ReaderCode.StartsWith(providerPrefix))
                .OrderByDescending(x => x.ReaderCode)
                .Select(x => x.ReaderCode)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (!string.IsNullOrEmpty(maxReaderCode))
            {
                string numberPart = maxReaderCode.Substring(providerPrefix.Length);
                if (int.TryParse(numberPart, out int currentNumber))
                {
                    nextNumber = currentNumber + 1;
                }
            }

            user = new Readers
            {
                Email = email,
                FullName = fullName,
                ReaderCode = $"{providerPrefix}{nextNumber:0004}",
                Password = hasher.HashPassword(null!, Guid.NewGuid().ToString()),
                Role = "Reader",
                IsActive = true,
                RegisterDate = DateTime.Now,
                ExpirationDate = DateTime.Now.AddYears(1),
                Avatar = "/images/default-avatar.png",
                Address = "Chưa cập nhật",
                Phone = "Chưa cập nhật",
                IdentityNumber = "Chưa cập nhật",
                DateOfBirth = DateTime.Now.AddYears(-18),
                EmailConfirmed = true,
                ReadersType = ReadersType.Student,
                Genders = Genders.Other
            };

            _context.Readers.Add(user);
            await _context.SaveChangesAsync();
        }

        return user;
    }

    public IActionResult ChangePassword()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
        {
            return RedirectToAction("Login");
        }
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userIdString = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdString))
        {
            return RedirectToAction("Login");
        }

        int userId = int.Parse(userIdString);
        var user = await _context.Readers.FindAsync(userId);

        if (user == null)
        {
            return RedirectToAction("Login");
        }

        var hasher = new PasswordHasher<Readers>();

        // Kiểm tra mật khẩu cũ
        var verifyResult = hasher.VerifyHashedPassword(
            user,
            user.Password!,
            model.OldPassword
        );

        if (verifyResult == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError("", "Mật khẩu cũ không đúng");
            return View(model);
        }

        // Không cho phép trùng mật khẩu cũ
        if (model.OldPassword == model.NewPassword)
        {
            ModelState.AddModelError("NewPassword", "Mật khẩu mới phải khác mật khẩu cũ");
            return View(model);
        }

        // Hash mật khẩu mới
        user.Password = hasher.HashPassword(user, model.NewPassword);
        user.ForcePasswordChange = false;

        _context.Readers.Update(user);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Đổi mật khẩu thành công.";
        
        // Redirect based on role
        if (user.Role == "Admin")
        {
            return RedirectToAction("Index", "Dashboard");
        }
        
        return RedirectToAction("Index", "Home");
    }

    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ForgotPassword(string email)
    {
        var user = _context.Readers.FirstOrDefault(x => x.Email == email);

        if (user == null)
        {
            ModelState.AddModelError("", "Email không tồn tại");
        }

        if (ModelState.IsValid)
        {
            TempData["Success"] = "Vui lòng liên hệ quản trị viên để cấp lại mật khẩu.";
            return RedirectToAction("Login");
        }

        return View();
    }

    public IActionResult GoogleLogin()
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(GoogleResponse))
        };

        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    public async Task<IActionResult> GoogleResponse()
    {
        var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        if (result?.Principal == null || !result.Succeeded)
        {
            return RedirectToAction("Login");
        }

        var email = result.Principal.FindFirstValue(ClaimTypes.Email);
        var fullName = result.Principal.FindFirstValue(ClaimTypes.Name);

        if (string.IsNullOrEmpty(email))
        {
            TempData["Error"] = "Không lấy được email Google.";
            return RedirectToAction("Login");
        }

        var user = await CreateOrGetReaderFromOAuth(email, fullName ?? "Google User", "GG");

        HttpContext.Session.SetString("UserId", user.Id.ToString());
        HttpContext.Session.SetString("UserName", user.FullName ?? "");
        HttpContext.Session.SetString("Role", user.Role);
        HttpContext.Session.SetString("IsVerified", user.IsVerified.ToString().ToLower());

        // Save EmployeeId to Session if user email matches an Employee
        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == user.Email && e.StatusId == 1);
        if (employee != null)
        {
            HttpContext.Session.SetString("EmployeeId", employee.Id.ToString());
        }

        // Update login streak for OAuth login
        var today = DateTime.Today;
        if (user.LastStreakDate.HasValue)
        {
            var lastStreakDate = user.LastStreakDate.Value.Date;
            
            if (lastStreakDate == today.AddDays(-1))
            {
                user.ConsecutiveLoginDays++;
            }
            else if (lastStreakDate < today.AddDays(-1))
            {
                user.ConsecutiveLoginDays = 1;
            }
        }
        else
        {
            user.ConsecutiveLoginDays = 1;
        }
        
        user.LastLoginDate = DateTime.Now;
        user.LastStreakDate = today;
        _context.Readers.Update(user);
        await _context.SaveChangesAsync();

        // Redirect to onboarding if display name not set
        if (!user.IsNameSet)
        {
            return RedirectToAction("CompleteProfile", "Account");
        }

        return RedirectToAction("Index", "Home");
    }

    public IActionResult FacebookLogin()
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(FacebookResponse))
        };

        return Challenge(properties, FacebookDefaults.AuthenticationScheme);
    }

    // GET: Account/AdminProfile (For Admin/Staff only)
    [SessionAuthorize("Admin")]
    public async Task<IActionResult> AdminProfile()
    {
        var userIdString = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
        {
            return RedirectToAction("Login");
        }

        var user = await _context.Readers
            .FirstOrDefaultAsync(r => r.Id == userId && !r.IsDeleted);

        if (user == null)
        {
            return RedirectToAction("Login");
        }

        return View(user);
    }

    // POST: Account/UpdateAdminProfile
    [HttpPost]
    [ValidateAntiForgeryToken]
    [SessionAuthorize("Admin")]
    public async Task<IActionResult> UpdateAdminProfile(string phone, string address)
    {
        var userIdString = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
        {
            return Json(new { success = false, message = "User not authenticated" });
        }

        var user = await _context.Readers
            .FirstOrDefaultAsync(r => r.Id == userId && !r.IsDeleted);

        if (user == null)
        {
            return Json(new { success = false, message = "User not found" });
        }

        user.Phone = phone;
        user.Address = address;

        _context.Readers.Update(user);
        await _context.SaveChangesAsync();

        return Json(new { success = true, message = "Cập nhật hồ sơ thành công" });
    }

    // POST: Account/UpdatePersonalInfo
    [HttpPost]
    [ValidateAntiForgeryToken]
    [SessionAuthorize]
    public async Task<IActionResult> UpdatePersonalInfo(string phone, string address, string dateOfBirth)
    {
        var userIdString = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
        {
            return Json(new { success = false, message = "User not authenticated" });
        }

        var user = await _context.Readers
            .FirstOrDefaultAsync(r => r.Id == userId && !r.IsDeleted);

        if (user == null)
        {
            return Json(new { success = false, message = "User not found" });
        }

        // Update phone
        if (!string.IsNullOrEmpty(phone))
        {
            user.Phone = phone;
        }

        // Update address
        if (!string.IsNullOrEmpty(address))
        {
            user.Address = address;
        }

        // Update date of birth
        if (!string.IsNullOrEmpty(dateOfBirth) && DateTime.TryParse(dateOfBirth, out DateTime dob))
        {
            user.DateOfBirth = dob;
        }

        _context.Readers.Update(user);
        await _context.SaveChangesAsync();

        return Json(new { success = true, message = "Cập nhật thông tin cá nhân thành công" });
    }

    // GET: Account/Profile
    [SessionAuthorize]
    public async Task<IActionResult> Profile(string? id)
    {
        var userIdString = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int currentUserId))
        {
            return RedirectToAction("Login");
        }

        Readers user;
        bool isOwnProfile;

        // Determine if viewing own profile or someone else's
        if (string.IsNullOrEmpty(id))
        {
            // Viewing own profile
            isOwnProfile = true;
            user = await _context.Readers.FindAsync(currentUserId);
            if (user == null)
            {
                return RedirectToAction("Login");
            }
        }
        else
        {
            // Viewing someone else's profile by ReaderCode
            isOwnProfile = false;
            user = await _context.Readers.FirstOrDefaultAsync(r => r.ReaderCode == id);
            if (user == null)
            {
                return NotFound("Profile not found");
            }
        }

        ViewBag.IsOwnProfile = isOwnProfile;

        // Use the actual user ID for quest tracking (either own or viewed profile)
        var userId = user.Id;

        // Quest Tracking System
        var totalBorrowed = await _context.BorrowSlips
            .Where(bs => bs.ReaderId == userId)
            .CountAsync();
        
        var favoriteCount = await _context.BookFavorites
            .Where(f => f.ReaderId == userId)
            .CountAsync();
        
        // Check late night borrowing (11 PM - 3 AM)
        var lateNightBorrows = await _context.BorrowSlips
            .Where(bs => bs.ReaderId == userId)
            .CountAsync(bs => bs.BorrowDate.Hour >= 23 || bs.BorrowDate.Hour < 3);

        // Total comments count
        var totalComments = await _context.BookComments
            .Where(c => c.ReaderId == userId)
            .CountAsync();

        // Use consecutive login days for streak calculation
        var streakDays = user.ConsecutiveLoginDays;

        // Define quest structure with targets and progress (Increased difficulty)
        var badgeQuests = new List<dynamic>
        {
            new
            {
                BadgeName = " Học Giả Đại Thừa",
                Description = "Mượn thành công 100 cuốn sách từ thư viện",
                Current = totalBorrowed,
                Target = 100,
                Progress = Math.Min(100, (totalBorrowed * 100) / 100),
                IsUnlocked = totalBorrowed >= 100,
                Icon = "fas fa-book-open",
                IconUrl = "/images/Icons/hoc-gia-dai-thua.png"
            },
            new
            {
                BadgeName = " Chúa Tể Đêm Trường",
                Description = "Mượn sách trong khung giờ cú đêm 23h - 3h sáng đủ 30 lần",
                Current = lateNightBorrows,
                Target = 30,
                Progress = Math.Min(100, (lateNightBorrows * 100) / 30),
                IsUnlocked = lateNightBorrows >= 30,
                Icon = "fas fa-moon",
                IconUrl = "/images/Icons/chua-te-dem-truong.gif"
            },
            new
            {
                BadgeName = " Tri Kỷ Văn Học",
                Description = "Đóng góp 50 bình luận/đánh giá chất lượng cho các đầu sách",
                Current = totalComments,
                Target = 50,
                Progress = Math.Min(100, (totalComments * 100) / 50),
                IsUnlocked = totalComments >= 50,
                Icon = "fas fa-comments",
                IconUrl = "/images/Icons/tri-ky-van-hoc.png"
            },
            new
            {
                BadgeName = " Kẻ Chinh Phục Tri Thức",
                Description = "Truy cập hệ thống liên tục trong vòng 7 ngày",
                Current = streakDays,
                Target = 7,
                Progress = Math.Min(100, (streakDays * 100) / 7),
                IsUnlocked = streakDays >= 7,
                Icon = "fas fa-fire",
                IconUrl = "/images/Icons/ke-chinh-phuc.png"
            },
            new
            {
                BadgeName = " Độc Giả Tập Sự",
                Description = "Danh hiệu mặc định cho người mới",
                Current = 1,
                Target = 1,
                Progress = 100,
                IsUnlocked = true,
                Icon = "fas fa-seedling",
                IconUrl = "/images/Icons/doc-gia-tap-su.png"
            }
        };

        // Get unlocked badges list
        var unlockedBadges = badgeQuests
            .Where(q => q.IsUnlocked)
            .Select(q => q.BadgeName)
            .ToList();

        // Set default active badge if none set or if current badge is locked
        if (string.IsNullOrEmpty(user.ActiveBadge) || !unlockedBadges.Contains(user.ActiveBadge))
        {
            user.ActiveBadge = unlockedBadges.First();
            _context.Readers.Update(user);
            await _context.SaveChangesAsync();
        }

        // Pass quest data to view
        ViewBag.BadgeQuests = badgeQuests;
        ViewBag.UnlockedBadges = unlockedBadges;

        // Fetch featured books
        List<Books> featuredBooks = new List<Books>();
        if (!string.IsNullOrEmpty(user.FeaturedBookIds))
        {
            var bookIds = user.FeaturedBookIds.Split(',')
                .Where(id => int.TryParse(id, out _))
                .Select(int.Parse)
                .ToList();

            featuredBooks = await _context.Books
                .Where(b => bookIds.Contains(b.Id))
                .ToListAsync();
        }

        // Fetch user's borrowed and favorited books for bookshelf management
        var borrowedBookIds = await _context.BorrowDetails
            .Where(bd => bd.BorrowSlips.ReaderId == userId)
            .Select(bd => bd.BookId)
            .Distinct()
            .ToListAsync();

        var favoritedBookIds = await _context.BookFavorites
            .Where(f => f.ReaderId == userId)
            .Select(f => f.BookId)
            .Distinct()
            .ToListAsync();

        var availableBooks = await _context.Books
            .Include(b => b.Category)
            .Where(b => borrowedBookIds.Contains(b.Id) || favoritedBookIds.Contains(b.Id))
            .ToListAsync();

        // Fetch categories for bookshelf filter
        ViewBag.Categories = await _context.Categories.ToListAsync();

        // Fetch guestbook messages for the profile
        var guestbookMessages = await _context.ProfileComments
            .Include(pc => pc.Author)
            .Where(pc => pc.ProfileOwnerId == userId)
            .OrderByDescending(pc => pc.CreatedAt)
            .Select(pc => new
            {
                pc.Id,
                pc.Content,
                pc.CreatedAt,
                pc.AuthorId,
                AuthorName = pc.Author.FullName,
                AuthorAvatar = pc.Author.Avatar,
                AuthorReaderCode = pc.Author.ReaderCode
            })
            .ToListAsync();

        // Fetch tag pins for the profile
        var tagPins = await _context.ProfileTagPins
            .Include(ptp => ptp.Author)
            .Where(ptp => ptp.ProfileOwnerId == userId)
            .OrderByDescending(ptp => ptp.CreatedAt)
            .Select(ptp => new
            {
                ptp.Id,
                ptp.TagKeysJson,
                ptp.PresetMessageKey,
                ptp.CreatedAt,
                ptp.AuthorId,
                AuthorName = ptp.Author.FullName,
                AuthorAvatar = ptp.Author.Avatar,
                AuthorReaderCode = ptp.Author.ReaderCode
            })
            .ToListAsync();

        // Resolve tag keys to labels/emojis via catalog (outside expression tree)
        var resolvedTagPins = tagPins.Select(ptp =>
        {
            var tags = GuestbookTagCatalog.GetTagsByKeys(
                JsonSerializer.Deserialize<List<string>>(ptp.TagKeysJson) ?? new List<string>()
            );
            var presetMessage = !string.IsNullOrEmpty(ptp.PresetMessageKey)
                ? GuestbookTagCatalog.GetPresetMessageByKey(ptp.PresetMessageKey)?.Message
                : null;

            return new
            {
                ptp.Id,
                ptp.CreatedAt,
                ptp.AuthorId,
                ptp.AuthorName,
                ptp.AuthorAvatar,
                ptp.AuthorReaderCode,
                Tags = tags,
                PresetMessage = presetMessage
            };
        }).ToList();

        ViewBag.FeaturedBooks = featuredBooks;
        ViewBag.AvailableBooks = availableBooks;
        ViewBag.GuestbookMessages = guestbookMessages;
        ViewBag.TagPins = resolvedTagPins;

        return View(user);
    }

    // POST: Account/UpdateCover
    [HttpPost]
    [SessionAuthorize]
    public async Task<IActionResult> UpdateCover(IFormFile? coverFile, string? coverUrl)
    {
        var userIdString = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
        {
            return Json(new { success = false, message = "User not authenticated" });
        }

        var user = await _context.Readers.FindAsync(userId);
        if (user == null)
        {
            return Json(new { success = false, message = "User not found" });
        }

        string newCoverUrl = "";

        // Handle file upload
        if (coverFile != null && coverFile.Length > 0)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "covers");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Preserve original extension (including .gif for animated GIFs)
            var fileExtension = Path.GetExtension(coverFile.FileName);
            var uniqueFileName = $"cover_{userId}_{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await coverFile.CopyToAsync(stream);
            }

            newCoverUrl = $"/uploads/covers/{uniqueFileName}";
        }
        // Handle URL input
        else if (!string.IsNullOrEmpty(coverUrl))
        {
            newCoverUrl = coverUrl;
        }
        else
        {
            return Json(new { success = false, message = "Please provide either a file or URL" });
        }

        user.CoverImageUrl = newCoverUrl;
        _context.Readers.Update(user);
        await _context.SaveChangesAsync();

        return Json(new { success = true, message = "Cover updated successfully", newUrl = newCoverUrl });
    }

    // POST: Account/UpdateProfile
    [HttpPost]
    [SessionAuthorize]
    public async Task<IActionResult> UpdateProfile(string phone, DateTime? dateOfBirth, string address, string bio, string activeBadge)
    {
        try
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return Json(new { success = false, message = "User not authenticated" });
            }

            var user = await _context.Readers.FindAsync(userId);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            user.Phone = phone;
            user.DateOfBirth = dateOfBirth;
            user.Address = address;
            user.Bio = bio;
            user.ActiveBadge = activeBadge;

            // Set IsVerified to true if phone and address are valid
            if (!string.IsNullOrEmpty(phone) && !string.IsNullOrEmpty(address))
            {
                user.IsVerified = true;
                HttpContext.Session.SetString("IsVerified", "true");
            }

            _context.Readers.Update(user);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Profile updated successfully" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    // GET: Account/CompleteProfile (Onboarding for first-time users)
    [SessionAuthorize]
    public async Task<IActionResult> CompleteProfile()
    {
        var userIdString = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
        {
            return RedirectToAction("Login");
        }

        var user = await _context.Readers.FindAsync(userId);
        if (user == null)
        {
            return RedirectToAction("Login");
        }

        // If already completed onboarding, redirect to home
        if (user.IsNameSet)
        {
            return RedirectToAction("Index", "Home");
        }

        return View();
    }

    // POST: Account/CompleteProfile
    [HttpPost]
    [SessionAuthorize]
    public async Task<IActionResult> CompleteProfile([FromBody] CompleteProfileRequest model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errorMsg = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return BadRequest(new { success = false, message = errorMsg });
            }

            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return BadRequest(new { success = false, message = "User not authenticated" });
            }

            var user = await _context.Readers.FindAsync(userId);
            if (user == null)
            {
                return BadRequest(new { success = false, message = "User not found" });
            }

            var displayName = model?.DisplayName?.Trim();

            // Validate display name format
            if (string.IsNullOrWhiteSpace(displayName))
            {
                return BadRequest(new { success = false, message = "Tên hiển thị không được để trống" });
            }

            // Format validation: letters, numbers, spaces only, 3-30 chars
            if (displayName.Length < 3 || displayName.Length > 30)
            {
                return BadRequest(new { success = false, message = "Tên hiển thị phải từ 3 đến 30 ký tự" });
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(displayName, @"^[a-zA-Z0-9\sÀ-Ỹà-ỹ]+$"))
            {
                return BadRequest(new { success = false, message = "Tên hiển thị chỉ được chứa chữ cái, số và khoảng trắng" });
            }

            // Check uniqueness
            bool nameExists = await _context.Readers
                .AnyAsync(r => r.FullName!.ToLower() == displayName.ToLower() && r.Id != userId);

            if (nameExists)
            {
                return BadRequest(new { success = false, message = "Tên hiển thị này đã tồn tại, vui lòng chọn tên khác" });
            }

            // Update user
            user.FullName = displayName;
            user.IsNameSet = true;
            _context.Readers.Update(user);
            await _context.SaveChangesAsync();

            // Update session
            HttpContext.Session.SetString("UserName", displayName);

            return Ok(new { success = true, message = "Cài đặt tên hiển thị thành công", redirectUrl = Url.Action("Index", "Home") });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
        }
    }

    // DTO for CompleteProfile request
    public class CompleteProfileRequest
    {
        public string DisplayName { get; set; }
    }

    // POST: Account/ValidateDisplayName (AJAX validation)
    [HttpPost]
    public async Task<IActionResult> ValidateDisplayName(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return Json(new { isValid = false, message = "Tên hiển thị không được để trống" });
        }

        displayName = displayName.Trim();

        if (displayName.Length < 3 || displayName.Length > 30)
        {
            return Json(new { isValid = false, message = "Tên hiển thị phải từ 3 đến 30 ký tự" });
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(displayName, @"^[a-zA-Z0-9\sÀ-Ỹà-ỹ]+$"))
        {
            return Json(new { isValid = false, message = "Tên hiển thị chỉ được chứa chữ cái, số và khoảng trắng" });
        }

        // Check profanity (basic list - can be expanded)
        var profanityList = new[] { "fuck", "shit", "damn", "bitch", "cunt", "dick", "ass", "pussy", "whore", "slut", "địt", "lồn", "cặc", "đm", "vl", "vkl", "cc" };
        var lowerName = displayName.ToLower();
        foreach (var profanity in profanityList)
        {
            if (lowerName.Contains(profanity))
            {
                return Json(new { isValid = false, message = "Tên hiển thị chứa từ không phù hợp" });
            }
        }

        // Check uniqueness
        var userIdString = HttpContext.Session.GetString("UserId");
        int? currentUserId = null;
        if (!string.IsNullOrEmpty(userIdString) && int.TryParse(userIdString, out int parsedId))
        {
            currentUserId = parsedId;
        }

        bool nameExists = await _context.Readers
            .AnyAsync(r => r.FullName!.ToLower() == displayName.ToLower() && (currentUserId == null || r.Id != currentUserId));

        if (nameExists)
        {
            return Json(new { isValid = false, message = "Tên hiển thị này đã tồn tại, vui lòng chọn tên khác" });
        }

        return Json(new { isValid = true, message = "Tên hiển thị hợp lệ" });
    }

    // GET: Account/GetProfileData (AJAX for profile modal)
    [SessionAuthorize]
    public async Task<IActionResult> GetProfileData()
    {
        var userIdString = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
        {
            return Json(new { success = false, message = "User not authenticated" });
        }

        var user = await _context.Readers.FindAsync(userId);
        if (user == null)
        {
            return Json(new { success = false, message = "User not found" });
        }

        return Json(new
        {
            success = true,
            displayName = user.FullName,
            email = user.Email,
            phone = user.Phone
        });
    }

    // POST: Account/UpdateDisplayName (Inline update from profile modal)
    [HttpPost]
    [ValidateAntiForgeryToken]
    [SessionAuthorize]
    public async Task<IActionResult> UpdateDisplayName(string displayName)
    {
        var userIdString = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
        {
            return Json(new { success = false, message = "User not authenticated" });
        }

        var user = await _context.Readers.FindAsync(userId);
        if (user == null)
        {
            return Json(new { success = false, message = "User not found" });
        }

        // Validate display name format
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return Json(new { success = false, message = "Tên hiển thị không được để trống" });
        }

        displayName = displayName.Trim();

        if (displayName.Length < 3 || displayName.Length > 30)
        {
            return Json(new { success = false, message = "Tên hiển thị phải từ 3 đến 30 ký tự" });
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(displayName, @"^[a-zA-Z0-9\sÀ-Ỹà-ỹ]+$"))
        {
            return Json(new { success = false, message = "Tên hiển thị chỉ được chứa chữ cái, số và khoảng trắng" });
        }

        // Check uniqueness (exclude current user)
        bool nameExists = await _context.Readers
            .AnyAsync(r => r.FullName!.ToLower() == displayName.ToLower() && r.Id != userId);

        if (nameExists)
        {
            return Json(new { success = false, message = "Tên hiển thị này đã tồn tại, vui lòng chọn tên khác" });
        }

        // Update user
        user.FullName = displayName;
        _context.Readers.Update(user);
        await _context.SaveChangesAsync();

        // Update session
        HttpContext.Session.SetString("UserName", displayName);

        return Json(new { success = true, message = "Cập nhật tên hiển thị thành công", newDisplayName = displayName });
    }

    // POST: Account/UpdateAvatar
    [HttpPost]
    [SessionAuthorize]
    public async Task<IActionResult> UpdateAvatar(IFormFile avatarFile)
    {
        var userIdString = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
        {
            return Json(new { success = false, message = "User not authenticated" });
        }

        var user = await _context.Readers.FindAsync(userId);
        if (user == null)
        {
            return Json(new { success = false, message = "User not found" });
        }

        if (avatarFile == null || avatarFile.Length == 0)
        {
            return Json(new { success = false, message = "Vui lòng chọn một file" });
        }

        // Validate file type - accept images including GIF
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var fileExtension = Path.GetExtension(avatarFile.FileName).ToLowerInvariant();
        
        if (!allowedExtensions.Contains(fileExtension))
        {
            return Json(new { success = false, message = "Chỉ chấp nhận các định dạng ảnh: JPG, PNG, GIF, WEBP" });
        }

        // Validate file size (max 5MB for animated GIFs)
        if (avatarFile.Length > 5 * 1024 * 1024)
        {
            return Json(new { success = false, message = "Kích thước file không được vượt quá 5MB" });
        }

        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var uniqueFileName = $"avatar_{userId}_{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await avatarFile.CopyToAsync(stream);
        }

        var newAvatarUrl = $"/uploads/avatars/{uniqueFileName}";
        user.Avatar = newAvatarUrl;
        _context.Readers.Update(user);
        await _context.SaveChangesAsync();

        return Json(new { success = true, message = "Cập nhật ảnh đại diện thành công", newUrl = newAvatarUrl });
    }

    // POST: Account/UpdateFullPageBackground
    [HttpPost]
    [SessionAuthorize]
    public async Task<IActionResult> UpdateFullPageBackground(IFormFile? backgroundFile, string? backgroundUrl)
    {
        var userIdString = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
        {
            return Json(new { success = false, message = "User not authenticated" });
        }

        var user = await _context.Readers.FindAsync(userId);
        if (user == null)
        {
            return Json(new { success = false, message = "User not found" });
        }

        string newBackgroundUrl = "";

        // Handle file upload (supports images and videos: .mp4, .webm, .gif, .jpg, .png, etc.)
        if (backgroundFile != null && backgroundFile.Length > 0)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "backgrounds");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Preserve original extension (including .mp4, .webm for videos, .gif for animated GIFs)
            var fileExtension = Path.GetExtension(backgroundFile.FileName);
            var uniqueFileName = $"bg_{userId}_{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await backgroundFile.CopyToAsync(stream);
            }

            newBackgroundUrl = $"/uploads/backgrounds/{uniqueFileName}";
        }
        // Handle URL input
        else if (!string.IsNullOrEmpty(backgroundUrl))
        {
            newBackgroundUrl = backgroundUrl;
        }
        else
        {
            return Json(new { success = false, message = "Please provide either a file or URL" });
        }

        user.FullPageBackgroundUrl = newBackgroundUrl;
        _context.Readers.Update(user);
        await _context.SaveChangesAsync();

        return Json(new { success = true, message = "Background updated successfully", newUrl = newBackgroundUrl });
    }

    // POST: Account/RemoveFullPageBackground
    [HttpPost]
    [SessionAuthorize]
    public async Task<IActionResult> RemoveFullPageBackground()
    {
        var userIdString = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
        {
            return Json(new { success = false, message = "User not authenticated" });
        }

        var user = await _context.Readers.FindAsync(userId);
        if (user == null)
        {
            return Json(new { success = false, message = "User not found" });
        }

        user.FullPageBackgroundUrl = null;
        _context.Readers.Update(user);
        await _context.SaveChangesAsync();

        return Json(new { success = true, message = "Đã xóa nền toàn trang thành công" });
    }

    // POST: Account/UpdateBioAndBadge
    [HttpPost]
    [SessionAuthorize]
    public async Task<IActionResult> UpdateBioAndBadge(string bio, string activeBadge)
    {
        try
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return Json(new { success = false, message = "User not authenticated" });
            }

            var user = await _context.Readers.FindAsync(userId);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            // Validate badge selection - recalculate unlocked badges
            var totalBorrowed = await _context.BorrowSlips
                .Where(bs => bs.ReaderId == userId)
                .CountAsync();
            
            var lateNightBorrows = await _context.BorrowSlips
                .Where(bs => bs.ReaderId == userId)
                .CountAsync(bs => bs.BorrowDate.Hour >= 23 || bs.BorrowDate.Hour < 3);

            var totalComments = await _context.BookComments
                .Where(c => c.ReaderId == userId)
                .CountAsync();

            var unlockedBadges = new List<string>();
            if (totalBorrowed >= 100) unlockedBadges.Add(" Học Giả Đại Thừa");
            if (lateNightBorrows >= 30) unlockedBadges.Add(" Chúa Tể Đêm Trường");
            if (totalComments >= 50) unlockedBadges.Add(" Tri Kỷ Văn Học");
            unlockedBadges.Add(" Độc Giả Tập Sự");

            // Check if selected badge is unlocked
            if (!string.IsNullOrEmpty(activeBadge) && !unlockedBadges.Contains(activeBadge))
            {
                return Json(new { success = false, message = "Bạn chưa mở khóa danh hiệu này. Hoàn thành nhiệm vụ để mở khóa!" });
            }

            // Update bio (limit to 150 characters)
            if (!string.IsNullOrEmpty(bio))
            {
                user.Bio = bio.Length > 150 ? bio.Substring(0, 150) : bio;
            }
            else
            {
                user.Bio = "";
            }

            // Update active badge
            if (!string.IsNullOrEmpty(activeBadge))
            {
                user.ActiveBadge = activeBadge;
            }

            _context.Readers.Update(user);
            await _context.SaveChangesAsync();

            // Map badge name to icon URL
            string activeBadgeIconUrl = "";
            if (!string.IsNullOrEmpty(user.ActiveBadge))
            {
                activeBadgeIconUrl = user.ActiveBadge switch
                {
                    " Học Giả Đại Thừa" => "/images/Icons/hoc-gia-dai-thua.png",
                    " Chúa Tể Đêm Trường" => "/images/Icons/chua-te-dem-truong.gif",
                    " Tri Kỷ Văn Học" => "/images/Icons/tri-ky-van-hoc.png",
                    " Kẻ Chinh Phục Tri Thức" => "/images/Icons/ke-chinh-phuc.png",
                    " Độc Giả Tập Sự" => "/images/Icons/doc-gia-tap-su.png",
                    _ => ""
                };
            }

            return Json(new { success = true, message = "Bio và danh hiệu đã được cập nhật", bio = user.Bio, activeBadge = user.ActiveBadge, activeBadgeIconUrl = activeBadgeIconUrl });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    // POST: Account/UpdateFeaturedBooks
    [HttpPost]
    [SessionAuthorize]
    public async Task<IActionResult> UpdateFeaturedBooks(List<int> bookIds)
    {
        try
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return Json(new { success = false, message = "User not authenticated" });
            }

            var user = await _context.Readers.FindAsync(userId);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            // Validate list size (max 4 books)
            if (bookIds != null && bookIds.Count > 4)
            {
                return Json(new { success = false, message = "Chỉ được chọn tối đa 4 sách cho kệ danh dự" });
            }

            // Save as comma-separated string
            user.FeaturedBookIds = bookIds != null && bookIds.Any() 
                ? string.Join(",", bookIds) 
                : null;

            _context.Readers.Update(user);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Kệ sách danh dự đã được cập nhật" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    // POST: Account/RemoveFeaturedBook
    [HttpPost]
    [SessionAuthorize]
    public async Task<IActionResult> RemoveFeaturedBook(int bookId)
    {
        try
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return Json(new { success = false, message = "User not authenticated" });
            }

            var user = await _context.Readers.FindAsync(userId);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            // Parse existing featured book IDs
            var featuredBookIds = new List<int>();
            if (!string.IsNullOrEmpty(user.FeaturedBookIds))
            {
                featuredBookIds = user.FeaturedBookIds.Split(',')
                    .Where(x => int.TryParse(x, out _))
                    .Select(int.Parse)
                    .ToList();
            }

            // Remove the book if it exists
            if (featuredBookIds.Contains(bookId))
            {
                featuredBookIds.Remove(bookId);
            }

            // Save back as comma-separated string (null if empty)
            user.FeaturedBookIds = featuredBookIds.Any()
                ? string.Join(",", featuredBookIds)
                : null;

            _context.Readers.Update(user);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã gỡ sách khỏi kệ" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    // POST: Account/PostGuestbookMessage
    [HttpPost]
    [SessionAuthorize]
    public async Task<IActionResult> PostGuestbookMessage(int profileOwnerId, string content)
    {
        try
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int authorId))
            {
                return Json(new { success = false, message = "User not authenticated" });
            }

            // Prevent self-posting
            if (authorId == profileOwnerId)
            {
                return Json(new { success = false, message = "Bạn không thể tự gửi lưu bút cho chính mình" });
            }

            // Validate content
            if (string.IsNullOrWhiteSpace(content))
            {
                return Json(new { success = false, message = "Vui lòng nhập nội dung" });
            }

            if (content.Length > 500)
            {
                return Json(new { success = false, message = "Nội dung không được quá 500 ký tự" });
            }

            // Basic spam prevention: check if user posted in last 30 seconds on this specific profile
            var lastComment = await _context.ProfileComments
                .Where(pc => pc.AuthorId == authorId && pc.ProfileOwnerId == profileOwnerId)
                .OrderByDescending(pc => pc.CreatedAt)
                .FirstOrDefaultAsync();

            if (lastComment != null && (DateTime.Now - lastComment.CreatedAt).TotalSeconds < 30)
            {
                return Json(new { success = false, message = "Vui lòng đợi 30 giây giữa các lần để lại lưu bút!" });
            }

            // Profanity filter - banned words list
            var bannedWords = new[] 
            { 
                "đm", "dm", "dmm", "đmm", "vcl", "vl", "vll", "cl", "clmm", "lồn", "l0n", "lon", 
                "cặc", "cặc", "cac", "cc", "địt", "dit", "đjt", "djt", "buồi", "buoi", "bui",
                "đĩ", "di", "đéo", "deo", "đéo", "khốn", "khon", "khốn nạn", "khon nan",
                "chó", "cho", "cho dead", "chó chết", "cho che", "mẹ", "me", "mẹ mày", "me may",
                "bố mày", "bo may", "cha mày", "cha may", "quảng cáo", "quang cao", "qc",
                "hack", "lừa đảo", "lua dao", "scam", "phishing", "fake", "gây rối", "gay roi",
                "spam", "rác", "rac", "ngu", "ngu ngốc", "ngu ngoc", "đần", "dan", "kẻ",
                "ke", "đồ", "do", "con", "cn", "phản động", "phan dong", "chính trị", "chinh tri"
            };

            var contentLower = content.ToLower();
            var foundProfanity = bannedWords.FirstOrDefault(word => contentLower.Contains(word));

            if (foundProfanity != null)
            {
                // Replace profanity with asterisks
                var sanitizedContent = content;
                foreach (var word in bannedWords)
                {
                    sanitizedContent = System.Text.RegularExpressions.Regex.Replace(
                        sanitizedContent, 
                        word, 
                        new string('*', word.Length), 
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase
                    );
                }
                content = sanitizedContent;
            }

            var profileComment = new ProfileComment
            {
                ProfileOwnerId = profileOwnerId,
                AuthorId = authorId,
                Content = content.Trim(),
                CreatedAt = DateTime.Now
            };

            _context.ProfileComments.Add(profileComment);
            await _context.SaveChangesAsync();

            // Fetch the comment with author info for response
            var newComment = await _context.ProfileComments
                .Include(pc => pc.Author)
                .Where(pc => pc.Id == profileComment.Id)
                .Select(pc => new
                {
                    pc.Id,
                    pc.Content,
                    pc.CreatedAt,
                    AuthorName = pc.Author.FullName,
                    AuthorAvatar = pc.Author.Avatar,
                    AuthorReaderCode = pc.Author.ReaderCode
                })
                .FirstOrDefaultAsync();

            return Json(new { success = true, message = "Đã gửi lưu bút thành công", comment = newComment });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    // POST: Account/DeleteGuestbookMessage
    [HttpPost]
    [SessionAuthorize]
    public async Task<IActionResult> DeleteGuestbookMessage(int commentId)
    {
        try
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int currentUserId))
            {
                return Json(new { success = false, message = "User not authenticated" });
            }

            var comment = await _context.ProfileComments.FindAsync(commentId);
            if (comment == null)
            {
                return Json(new { success = false, message = "Comment not found" });
            }

            // Allow deletion only if user is author OR profile owner
            if (comment.AuthorId != currentUserId && comment.ProfileOwnerId != currentUserId)
            {
                return Json(new { success = false, message = "Bạn không có quyền xóa lưu bút này" });
            }

            _context.ProfileComments.Remove(comment);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã xóa lưu bút thành công" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    // POST: Account/PostProfileTagPin
    [HttpPost]
    [SessionAuthorize]
    public async Task<IActionResult> PostProfileTagPin(int profileOwnerId, List<string> tagKeys, string? presetMessageKey)
    {
        try
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int authorId))
            {
                return Json(new { success = false, message = "User not authenticated" });
            }

            // Don't allow pinning to own profile
            if (profileOwnerId == authorId)
            {
                return Json(new { success = false, message = "Bạn không thể ghim cảm xúc cho chính mình" });
            }

            // Validate tag keys
            if (tagKeys == null || tagKeys.Count == 0 || tagKeys.Count > 3)
            {
                return Json(new { success = false, message = "Vui lòng chọn từ 1 đến 3 thẻ cảm xúc" });
            }

            // Validate each tag key exists in catalog
            foreach (var key in tagKeys)
            {
                if (!GuestbookTagCatalog.IsValidTagKey(key))
                {
                    return Json(new { success = false, message = "Thẻ cảm xúc không hợp lệ" });
                }
            }

            // Validate preset message key if provided
            if (!string.IsNullOrEmpty(presetMessageKey) && !GuestbookTagCatalog.IsValidPresetMessageKey(presetMessageKey))
            {
                return Json(new { success = false, message = "Lời chúc không hợp lệ" });
            }

            // Check spam prevention (30 seconds between pins from same author to same profile owner)
            var lastPin = await _context.ProfileTagPins
                .Where(ptp => ptp.ProfileOwnerId == profileOwnerId && ptp.AuthorId == authorId)
                .OrderByDescending(ptp => ptp.CreatedAt)
                .FirstOrDefaultAsync();

            if (lastPin != null && (DateTime.Now - lastPin.CreatedAt).TotalSeconds < 30)
            {
                return Json(new { success = false, message = "Vui lòng đợi 30 giây trước khi ghim cảm xúc tiếp theo" });
            }

            // Create tag pin
            var tagPin = new ProfileTagPin
            {
                ProfileOwnerId = profileOwnerId,
                AuthorId = authorId,
                TagKeysJson = JsonSerializer.Serialize(tagKeys),
                PresetMessageKey = presetMessageKey,
                CreatedAt = DateTime.Now
            };

            _context.ProfileTagPins.Add(tagPin);
            await _context.SaveChangesAsync();

            // Fetch the pin with author info for response
            var newPin = await _context.ProfileTagPins
                .Include(ptp => ptp.Author)
                .Where(ptp => ptp.Id == tagPin.Id)
                .Select(ptp => new
                {
                    ptp.Id,
                    ptp.CreatedAt,
                    ptp.AuthorId,
                    AuthorName = ptp.Author.FullName,
                    AuthorAvatar = ptp.Author.Avatar,
                    AuthorReaderCode = ptp.Author.ReaderCode,
                    ptp.TagKeysJson,
                    ptp.PresetMessageKey
                })
                .FirstOrDefaultAsync();

            // Resolve tags and preset message outside expression tree
            var resolvedTags = GuestbookTagCatalog.GetTagsByKeys(
                JsonSerializer.Deserialize<List<string>>(newPin.TagKeysJson) ?? new List<string>()
            );
            var resolvedPresetMessage = !string.IsNullOrEmpty(newPin.PresetMessageKey)
                ? GuestbookTagCatalog.GetPresetMessageByKey(newPin.PresetMessageKey)?.Message
                : null;

            var responsePin = new
            {
                newPin.Id,
                newPin.CreatedAt,
                newPin.AuthorId,
                newPin.AuthorName,
                newPin.AuthorAvatar,
                newPin.AuthorReaderCode,
                Tags = resolvedTags,
                PresetMessage = resolvedPresetMessage
            };

            return Json(new { success = true, message = "Đã ghim cảm xúc thành công", pin = responsePin });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    // POST: Account/DeleteProfileTagPin
    [HttpPost]
    [SessionAuthorize]
    public async Task<IActionResult> DeleteProfileTagPin(int pinId)
    {
        try
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int currentUserId))
            {
                return Json(new { success = false, message = "User not authenticated" });
            }

            var pin = await _context.ProfileTagPins.FindAsync(pinId);
            if (pin == null)
            {
                return Json(new { success = false, message = "Pin not found" });
            }

            // Allow deletion only if user is author OR profile owner
            if (pin.AuthorId != currentUserId && pin.ProfileOwnerId != currentUserId)
            {
                return Json(new { success = false, message = "Bạn không có quyền xóa ghim cảm xúc này" });
            }

            _context.ProfileTagPins.Remove(pin);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã xóa ghim cảm xúc thành công" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    public async Task<IActionResult> FacebookResponse()
    {
        var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        if (result?.Principal == null || !result.Succeeded)
        {
            return RedirectToAction("Login");
        }

        var email = result.Principal.FindFirstValue(ClaimTypes.Email);
        var fullName = result.Principal.FindFirstValue(ClaimTypes.Name);

        if (string.IsNullOrEmpty(email))
        {
            var fbId = result.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
            email = $"{fbId}@facebook.com";
        }

        var user = await CreateOrGetReaderFromOAuth(email, fullName ?? "Facebook User", "FB");

        HttpContext.Session.SetString("UserId", user.Id.ToString());
        HttpContext.Session.SetString("UserName", user.FullName ?? "");
        HttpContext.Session.SetString("Role", user.Role);
        HttpContext.Session.SetString("IsVerified", user.IsVerified.ToString().ToLower());

        // Save EmployeeId to Session if user email matches an Employee
        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == user.Email && e.StatusId == 1);
        if (employee != null)
        {
            HttpContext.Session.SetString("EmployeeId", employee.Id.ToString());
        }

        // Update login streak for OAuth login
        var today = DateTime.Today;
        if (user.LastStreakDate.HasValue)
        {
            var lastStreakDate = user.LastStreakDate.Value.Date;
            
            if (lastStreakDate == today.AddDays(-1))
            {
                user.ConsecutiveLoginDays++;
            }
            else if (lastStreakDate < today.AddDays(-1))
            {
                user.ConsecutiveLoginDays = 1;
            }
        }
        else
        {
            user.ConsecutiveLoginDays = 1;
        }
        
        user.LastLoginDate = DateTime.Now;
        user.LastStreakDate = today;
        _context.Readers.Update(user);
        await _context.SaveChangesAsync();

        // Redirect to onboarding if display name not set
        if (!user.IsNameSet)
        {
            return RedirectToAction("CompleteProfile", "Account");
        }

        return RedirectToAction("Index", "Home");
    }
}