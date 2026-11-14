using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SOF1022_NHOM1_SD2003_Ver2.Datas;
using SOF1022_NHOM1_SD2003_Ver2.Models;
using SOF1022_NHOM1_SD2003_Ver2.Models.Enums;
using SOF1022_NHOM1_SD2003_Ver2.Models.ViewModels;
using SOF1022_NHOM1_SD2003_Ver2.Attributes;
using SOF1022_NHOM1_SD2003_Ver2.Helpers;

[SessionAuthorize(Roles = "Admin,Quản lý thư viện")]
[PermissionAuthorize("employee.view")]
public class EmployeesController : Controller
{
    private readonly LibraryDbContext _context;

    public EmployeesController(LibraryDbContext context)
    {
        _context = context;
    }

    // ========================= INDEX + SEARCH =========================
    // Search by: EmployeeId, FullName, Email, Phone, CCCD, Position
    public async Task<IActionResult> Index(string? searchString, int? statusFilter = 1)
    {
        var query = _context.Employees
            .Include(e => e.Position)
            .Include(e => e.EmployeeStatusOption)
            .AsQueryable();

        // Apply status filter based on parameter
        if (statusFilter == 3) // Đã nghỉ việc - show both StatusId=3 and soft-deleted
        {
            query = query.IgnoreQueryFilters().Where(e => e.StatusId == 3 || e.IsDeleted);
        }
        else if (statusFilter == 2) // Tạm ngưng việc
        {
            query = query.Where(e => e.StatusId == 2 && !e.IsDeleted);
        }
        else // Đang làm việc (default)
        {
            query = query.Where(e => e.StatusId == 1 && !e.IsDeleted);
        }

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            searchString = searchString.Trim();

            query = query.Where(e =>
                (e.EmployeeId != null && e.EmployeeId.Contains(searchString)) ||
                (e.FullName != null && e.FullName.Contains(searchString)) ||
                (e.Email != null && e.Email.Contains(searchString)) ||
                (e.Phone != null && e.Phone.Contains(searchString)) ||
                (e.CCCD != null && e.CCCD.Contains(searchString)) ||
                (e.Position != null && e.Position.Name != null && e.Position.Name.Contains(searchString))
            );
        }

        ViewBag.SearchString = searchString;
        ViewBag.StatusFilter = statusFilter;

        // Calculate dashboard metrics (independent of filter)
        var allEmployeesQuery = _context.Employees.IgnoreQueryFilters();
        ViewBag.TotalEmployees = await allEmployeesQuery.CountAsync();
        ViewBag.ActiveEmployees = await _context.Employees.CountAsync(e => e.StatusId == 1 && !e.IsDeleted);
        ViewBag.SuspendedEmployees = await _context.Employees.CountAsync(e => e.StatusId == 2 && !e.IsDeleted);
        ViewBag.ArchivedEmployees = await allEmployeesQuery.CountAsync(e => e.StatusId == 3 || e.IsDeleted);

        // Auto-termination check: Check for employees with 5+ consecutive unexcused days
        await CheckAutoTermination();

        var employees = await query
            .OrderBy(e => e.EmployeeId)
            .ToListAsync();

        // Debug: Log status names
        foreach (var emp in employees)
        {
            System.Diagnostics.Debug.WriteLine($"Employee: {emp.EmployeeId}, StatusId: {emp.StatusId}, StatusName: {emp.EmployeeStatusOption?.Name}");
        }

        return View(employees);
    }

    // ========================= AUTO-TERMINATION CHECK =========================
    private async Task CheckAutoTermination()
    {
        const int UNEXCUSED_THRESHOLD = 5; // 5 consecutive days

        var unexcusedEmployees = await _context.Employees
            .Where(e => !e.IsDeleted &&
                       e.StatusId == 2 && // Tạm ngưng
                       e.UnexcusedDaysCount >= UNEXCUSED_THRESHOLD &&
                       e.LastUnexcusedDate.HasValue)
            .ToListAsync();

        foreach (var employee in unexcusedEmployees)
        {
            // Check if the unexcused period has reached the threshold
            var daysSinceLastUnexcused = (DateTime.Now - employee.LastUnexcusedDate.Value).Days;

            if (daysSinceLastUnexcused >= UNEXCUSED_THRESHOLD)
            {
                // Auto-terminate
                employee.StatusId = 3; // Đã nghỉ việc
                employee.LeaveStartDate = employee.LeaveStartDate ?? DateTime.Now;
                System.Diagnostics.Debug.WriteLine($"Auto-terminated employee: {employee.EmployeeId} - {employee.FullName}");
            }
        }

        await _context.SaveChangesAsync();
    }

    // ========================= DETAILS =========================

    [HttpGet]
    public async Task<IActionResult> Details(int? id)
    {
        // ===================== Kiểm tra ID =====================

        if (id == null)
        {
            return NotFound();
        }

        try
        {
            // ===================== Lấy thông tin nhân viên =====================

            var employee = await _context.Employees
                .Include(e => e.Position)
                .Include(e => e.EmployeeStatusOption)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

            // ===================== Không tìm thấy =====================

            if (employee == null)
            {
                return NotFound();
            }

            // ===================== Hiển thị chi tiết =====================

            return View(employee);
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tải thông tin nhân viên.";

            return RedirectToAction(nameof(Index));
        }
    }

    // ========================= CREATE =========================
    [HttpGet]
    [PermissionAuthorize("employee.create")]
    public IActionResult Create()
    {
        // Populate dropdowns
        ViewBag.Positions = _context.Positions.OrderBy(p => p.Name).ToList();
        ViewBag.StatusOptions = _context.EmployeeStatusOptions.OrderBy(s => s.DisplayOrder).ToList();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [PermissionAuthorize("employee.create")]
    public async Task<IActionResult> Create(
    [Bind("Id,EmployeeId,FullName,Email,Phone,Address,CCCD,Gender,DateOfBirth,HireDate,PositionId,Salary,StatusId")]
    Employee employee)
    {
        // Chuẩn hóa dữ liệu
        employee.EmployeeId = employee.EmployeeId?.Trim();
        employee.FullName = employee.FullName?.Trim();
        employee.Email = employee.Email?.Trim();
        employee.Phone = employee.Phone?.Trim();
        employee.Address = employee.Address?.Trim();
        employee.CCCD = employee.CCCD?.Trim();

        // ===================== Auto-set salary based on position if not provided =====================
        if (employee.PositionId.HasValue && (employee.Salary <= 0))
        {
            var position = await _context.Positions
                .FirstOrDefaultAsync(p => p.Id == employee.PositionId.Value);
            
            if (position != null)
            {
                employee.Salary = position.Name switch
                {
                    "Thủ thư" => 12000000,
                    "Nhân viên kho" => 10500000,
                    "Chăm sóc khách hàng" => 9500000,
                    "Quản lý thư viện" => 18500000,
                    "Admin" => 28000000,
                    _ => 10000000
                };
            }
        }

        // ===================== Kiểm tra trùng Mã nhân viên =====================
        if (!string.IsNullOrWhiteSpace(employee.EmployeeId))
        {
            bool exists = await _context.Employees
                .AnyAsync(e => e.EmployeeId == employee.EmployeeId && !e.IsDeleted);

            if (exists)
            {
                ModelState.AddModelError(nameof(employee.EmployeeId), "Mã nhân viên đã tồn tại.");
            }
        }

        // ===================== Kiểm tra trùng Email =====================
        if (!string.IsNullOrWhiteSpace(employee.Email))
        {
            bool exists = await _context.Employees
                .AnyAsync(e => e.Email == employee.Email && !e.IsDeleted);

            if (exists)
            {
                ModelState.AddModelError(nameof(employee.Email), "Email đã tồn tại.");
            }
        }

        // ===================== Kiểm tra trùng CCCD =====================
        if (!string.IsNullOrWhiteSpace(employee.CCCD))
        {
            bool exists = await _context.Employees
                .AnyAsync(e => e.CCCD == employee.CCCD && !e.IsDeleted);

            if (exists)
            {
                ModelState.AddModelError(nameof(employee.CCCD), "CCCD/CMND đã tồn tại.");
            }
        }

        // Nếu có lỗi thì trả lại View
        if (!ModelState.IsValid)
        {
            // Repopulate dropdowns for redisplay
            ViewBag.Positions = _context.Positions.OrderBy(p => p.Name).ToList();
            ViewBag.StatusOptions = _context.EmployeeStatusOptions.OrderBy(s => s.DisplayOrder).ToList();
            return View(employee);
        }

        try
        {
            string positionName = "Thủ thư";

            // ===================== Auto-set RoleId based on Position =====================
            if (employee.PositionId.HasValue)
            {
                var position = await _context.Positions
                    .FirstOrDefaultAsync(p => p.Id == employee.PositionId.Value);
                
                if (position != null)
                {
                    positionName = position.Name;
                    var role = await _context.Roles
                        .FirstOrDefaultAsync(r => r.Name == position.Name);
                    
                    if (role != null)
                    {
                        employee.RoleId = role.Id;
                    }
                }
            }

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            // ===================== TỰ ĐỘNG TẠO TÀI KHOẢN ĐĂNG NHẬP TRONG READERS =====================
            string defaultPassword = $"{employee.EmployeeId}@123";
            if (!string.IsNullOrWhiteSpace(employee.Email))
            {
                var existingReader = await _context.Readers
                    .FirstOrDefaultAsync(r => r.Email == employee.Email);

                if (existingReader == null)
                {
                    var hasher = new PasswordHasher<Readers>();
                    var newReader = new Readers
                    {
                        ReaderCode = $"RD_{employee.EmployeeId}",
                        FullName = employee.FullName,
                        DateOfBirth = employee.DateOfBirth,
                        Phone = employee.Phone ?? "",
                        Email = employee.Email,
                        Address = employee.Address ?? "",
                        IdentityNumber = employee.CCCD ?? "",
                        RegisterDate = DateTime.Now,
                        ExpirationDate = DateTime.Now.AddYears(5),
                        ConsecutiveLoginDays = 0,
                        IsCommentBlocked = false,
                        MaxBorrowBooks = 10,
                        Password = hasher.HashPassword(null!, defaultPassword),
                        Role = positionName,
                        IsActive = (employee.StatusId == 1),
                        EmailConfirmed = true,
                        ForcePasswordChange = true,
                        ReadersType = ReadersType.Teacher,
                        Genders = (Genders)(int)employee.Gender,
                        IsDeleted = false,
                        IsNameSet = true,
                        IsVerified = true
                    };
                    _context.Readers.Add(newReader);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    existingReader.Role = positionName;
                    existingReader.IsActive = (employee.StatusId == 1);
                    existingReader.FullName = employee.FullName;
                    existingReader.Phone = employee.Phone ?? existingReader.Phone;
                    _context.Readers.Update(existingReader);
                    await _context.SaveChangesAsync();
                }
            }

            TempData["SuccessMessage"] = $"Thêm nhân viên \"{employee.FullName}\" thành công! Tài khoản đăng nhập ({employee.Email}) đã được tạo tự động với mật khẩu: {defaultPassword}";

            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty,
                "Không thể lưu dữ liệu. Vui lòng kiểm tra lại thông tin hoặc thử lại sau.");

            return View(employee);
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty,
                "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau.");

            return View(employee);
        }
    }

    // ========================= EDIT =========================

    [HttpGet]
    [PermissionAuthorize("employee.edit")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var employee = await _context.Employees
            .Include(e => e.Position)
            .Include(e => e.EmployeeStatusOption)
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

        if (employee == null)
        {
            return NotFound();
        }

        // Populate dropdowns
        ViewBag.Positions = _context.Positions.OrderBy(p => p.Name).ToList();
        ViewBag.StatusOptions = _context.EmployeeStatusOptions.OrderBy(s => s.DisplayOrder).ToList();

        return View(employee);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [PermissionAuthorize("employee.edit")]
    public async Task<IActionResult> Edit(
    int id,
    [Bind("Id,EmployeeId,FullName,Email,Phone,Address,CCCD,Gender,DateOfBirth,HireDate,PositionId,Salary,StatusId")]
    Employee employee)
    {
        // Debug: Log all received values
        System.Diagnostics.Debug.WriteLine($"=== EDIT POST RECEIVED ===");
        System.Diagnostics.Debug.WriteLine($"ID: {id}");
        System.Diagnostics.Debug.WriteLine($"Employee.Id: {employee.Id}");
        System.Diagnostics.Debug.WriteLine($"StatusId: {employee.StatusId}");
        System.Diagnostics.Debug.WriteLine($"PositionId: {employee.PositionId}");
        System.Diagnostics.Debug.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");

        if (!ModelState.IsValid)
        {
            foreach (var error in ModelState)
            {
                System.Diagnostics.Debug.WriteLine($"Field: {error.Key}, Errors: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
            }
        }

        // ===================== Kiểm tra ID =====================

        if (id != employee.Id)
        {
            return NotFound();
        }

        // ===================== Kiểm tra bản ghi có tồn tại =====================

        bool employeeExists = await _context.Employees
            .AnyAsync(e => e.Id == employee.Id && !e.IsDeleted);

        if (!employeeExists)
        {
            return NotFound();
        }

        // ===================== Chuẩn hóa dữ liệu =====================

        employee.EmployeeId = employee.EmployeeId?.Trim();
        employee.FullName = employee.FullName?.Trim();
        employee.Email = employee.Email?.Trim();
        employee.Phone = employee.Phone?.Trim();
        employee.Address = employee.Address?.Trim();
        employee.CCCD = employee.CCCD?.Trim();

        // ===================== Auto-set salary based on position if not provided =====================
        if (employee.PositionId.HasValue && (employee.Salary <= 0))
        {
            var position = await _context.Positions
                .FirstOrDefaultAsync(p => p.Id == employee.PositionId.Value);
            
            if (position != null)
            {
                employee.Salary = position.Name switch
                {
                    "Thủ thư" => 12000000,
                    "Nhân viên kho" => 10500000,
                    "Chăm sóc khách hàng" => 9500000,
                    "Quản lý thư viện" => 18500000,
                    "Admin" => 28000000,
                    _ => 10000000
                };
            }
        }

        // ===================== Kiểm tra trùng Mã nhân viên =====================

        if (!string.IsNullOrWhiteSpace(employee.EmployeeId))
        {
            bool duplicateEmployeeId = await _context.Employees
                .AnyAsync(e =>
                    e.EmployeeId == employee.EmployeeId &&
                    e.Id != employee.Id &&
                    !e.IsDeleted);

            if (duplicateEmployeeId)
            {
                ModelState.AddModelError(nameof(employee.EmployeeId),
                    "Mã nhân viên đã tồn tại.");
            }
        }

        // ===================== Kiểm tra trùng Email =====================

        if (!string.IsNullOrWhiteSpace(employee.Email))
        {
            bool duplicateEmail = await _context.Employees
                .AnyAsync(e =>
                    e.Email == employee.Email &&
                    e.Id != employee.Id &&
                    !e.IsDeleted);

            if (duplicateEmail)
            {
                ModelState.AddModelError(nameof(employee.Email),
                    "Email đã tồn tại.");
            }
        }

        // ===================== Kiểm tra trùng CCCD =====================

        if (!string.IsNullOrWhiteSpace(employee.CCCD))
        {
            bool duplicateCCCD = await _context.Employees
                .AnyAsync(e =>
                    e.CCCD == employee.CCCD &&
                    e.Id != employee.Id &&
                    !e.IsDeleted);

            if (duplicateCCCD)
            {
                ModelState.AddModelError(nameof(employee.CCCD),
                    "CCCD/CMND đã tồn tại.");
            }
        }

        // ===================== Kiểm tra Validation =====================

        if (!ModelState.IsValid)
        {
            // Repopulate dropdowns for redisplay
            ViewBag.Positions = _context.Positions.OrderBy(p => p.Name).ToList();
            ViewBag.StatusOptions = _context.EmployeeStatusOptions.OrderBy(s => s.DisplayOrder).ToList();
            return View(employee);
        }

        try
        {
            var dbEmployee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

            if (dbEmployee == null)
            {
                return NotFound();
            }

            // ===================== BẢO VỆ TÀI KHOẢN ROOT ADMIN =====================
            if ((dbEmployee.EmployeeId == "NV001" || dbEmployee.Email == "admin@library.com") && 
                (employee.StatusId != 1 || employee.PositionId != 1))
            {
                ModelState.AddModelError("", "Không thể khóa hoặc giáng chức tài khoản Quản trị viên tối cao (Root Admin).");
                ViewBag.Positions = _context.Positions.OrderBy(p => p.Name).ToList();
                ViewBag.StatusOptions = _context.EmployeeStatusOptions.OrderBy(s => s.DisplayOrder).ToList();
                return View(employee);
            }

            dbEmployee.EmployeeId = employee.EmployeeId;
            dbEmployee.FullName = employee.FullName;
            dbEmployee.Email = employee.Email;
            dbEmployee.Phone = employee.Phone;
            dbEmployee.Address = employee.Address;
            dbEmployee.CCCD = employee.CCCD;
            dbEmployee.Gender = employee.Gender;
            dbEmployee.DateOfBirth = employee.DateOfBirth;
            dbEmployee.HireDate = employee.HireDate;
            dbEmployee.PositionId = employee.PositionId;
            dbEmployee.Salary = employee.Salary;
            dbEmployee.StatusId = employee.StatusId;   // Cập nhật trạng thái

            string posName = "Thủ thư";

            // ===================== Auto-update RoleId based on Position =====================
            if (employee.PositionId.HasValue)
            {
                var position = await _context.Positions
                    .FirstOrDefaultAsync(p => p.Id == employee.PositionId.Value);
                
                if (position != null)
                {
                    posName = position.Name;
                    var role = await _context.Roles
                        .FirstOrDefaultAsync(r => r.Name == position.Name);
                    
                    if (role != null)
                    {
                        dbEmployee.RoleId = role.Id;
                    }
                }
            }

            // ===================== ĐỒNG BỘ TOÀN DIỆN VỚI TÀI KHOẢN ĐĂNG NHẬP READERS =====================
            if (!string.IsNullOrWhiteSpace(employee.Email))
            {
                var reader = await _context.Readers
                    .FirstOrDefaultAsync(r => r.Email == employee.Email || 
                        (r.FullName == dbEmployee.FullName && r.Phone == dbEmployee.Phone));
                
                if (reader != null)
                {
                    reader.Email = employee.Email;
                    reader.FullName = employee.FullName;
                    reader.Phone = employee.Phone ?? "";
                    reader.Address = employee.Address ?? "";
                    reader.DateOfBirth = employee.DateOfBirth;
                    reader.IdentityNumber = employee.CCCD ?? "";
                    if (!string.IsNullOrEmpty(posName))
                    {
                        reader.Role = posName;
                    }
                    reader.IsActive = (dbEmployee.StatusId == 1);
                    _context.Readers.Update(reader);
                }
                else
                {
                    var hasher = new PasswordHasher<Readers>();
                    var defaultPassword = $"{employee.EmployeeId}@123";
                    var newReader = new Readers
                    {
                        ReaderCode = $"RD_{employee.EmployeeId}",
                        FullName = employee.FullName,
                        DateOfBirth = employee.DateOfBirth,
                        Phone = employee.Phone ?? "",
                        Email = employee.Email,
                        Address = employee.Address ?? "",
                        IdentityNumber = employee.CCCD ?? "",
                        RegisterDate = DateTime.Now,
                        ExpirationDate = DateTime.Now.AddYears(5),
                        ConsecutiveLoginDays = 0,
                        IsCommentBlocked = false,
                        MaxBorrowBooks = 10,
                        Password = hasher.HashPassword(null!, defaultPassword),
                        Role = posName,
                        IsActive = (dbEmployee.StatusId == 1),
                        EmailConfirmed = true,
                        ForcePasswordChange = true,
                        ReadersType = ReadersType.Teacher,
                        Genders = (Genders)(int)employee.Gender,
                        IsDeleted = false,
                        IsNameSet = true,
                        IsVerified = true
                    };
                    _context.Readers.Add(newReader);
                }
            }

            // Debug: Log updated StatusId
            System.Diagnostics.Debug.WriteLine($"Updated StatusId: {dbEmployee.StatusId}");

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cập nhật nhân viên thành công.";

            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException)
        {
            bool exists = await _context.Employees
                .AnyAsync(e => e.Id == employee.Id && !e.IsDeleted);

            if (!exists)
            {
                return NotFound();
            }

            ModelState.AddModelError(string.Empty,
                "Dữ liệu đã được thay đổi bởi người dùng khác. Vui lòng tải lại trang và thử lại.");

            return View(employee);
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty,
                "Không thể cập nhật dữ liệu. Vui lòng kiểm tra lại thông tin hoặc thử lại sau.");

            return View(employee);
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty,
                "Đã xảy ra lỗi hệ thống. Vui lòng liên hệ quản trị viên.");

            return View(employee);
        }
    }

    // ========================= RESET PASSWORD =========================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPasswordFromPhanQuyen(int id)
    {
        try
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

            if (employee == null)
            {
                return Json(new { success = false, message = "Không tìm thấy nhân viên." });
            }

            if (string.IsNullOrWhiteSpace(employee.Email))
            {
                return Json(new { success = false, message = "Nhân viên không có email, không thể cấp lại mật khẩu." });
            }

            var reader = await _context.Readers
                .FirstOrDefaultAsync(r => r.Email == employee.Email);

            if (reader == null)
            {
                return Json(new { success = false, message = "Không tìm thấy tài khoản đăng nhập cho nhân viên này." });
            }

            // Generate new random password
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%";
            var random = new Random();
            var newPassword = new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            // Hash new password
            var hasher = new PasswordHasher<Readers>();
            var hashedPassword = hasher.HashPassword(null, newPassword);

            reader.Password = hashedPassword;
            reader.ForcePasswordChange = true;
            _context.Readers.Update(reader);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = $"Đã cấp lại mật khẩu cho {employee.FullName}. Mật khẩu mới: {newPassword} - nhân viên cần đổi mật khẩu khi đăng nhập lần đầu." });
        }
        catch (Exception)
        {
            return Json(new { success = false, message = "Đã xảy ra lỗi khi cấp lại mật khẩu." });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(int id)
    {
        try
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

            if (employee == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy nhân viên.";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(employee.Email))
            {
                TempData["ErrorMessage"] = "Nhân viên không có email, không thể cấp lại mật khẩu.";
                return RedirectToAction(nameof(Index));
            }

            var reader = await _context.Readers
                .FirstOrDefaultAsync(r => r.Email == employee.Email);

            if (reader == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tài khoản đăng nhập cho nhân viên này.";
                return RedirectToAction(nameof(Index));
            }

            // Generate new random password
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%";
            var random = new Random();
            var newPassword = new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            // Hash new password
            var hasher = new PasswordHasher<Readers>();
            var hashedPassword = hasher.HashPassword(null, newPassword);

            reader.Password = hashedPassword;
            reader.ForcePasswordChange = true;
            _context.Readers.Update(reader);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã cấp lại mật khẩu cho {employee.FullName}. Mật khẩu mới: {newPassword} - nhân viên cần đổi mật khẩu khi đăng nhập lần đầu.";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Đã xảy ra lỗi khi cấp lại mật khẩu.";
            return RedirectToAction(nameof(Index));
        }
    }

    // ========================= UPDATE STATUS QUICK =========================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatusQuick(int employeeId, int statusId, string? leaveReason)
    {
        try
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == employeeId && !e.IsDeleted);

            if (employee == null)
            {
                return Json(new { success = false, message = "Không tìm thấy nhân viên." });
            }

            // Bảo vệ tài khoản Root Admin
            if ((employee.EmployeeId == "NV001" || employee.Email == "admin@library.com") && statusId != 1)
            {
                return Json(new { success = false, message = "Không thể thay đổi trạng thái của tài khoản Quản trị viên tối cao." });
            }

            // Auto-populate "Nghỉ không phép" if reason is empty for status 2 or 3
            if ((statusId == 2 || statusId == 3) && string.IsNullOrWhiteSpace(leaveReason))
            {
                leaveReason = "Nghỉ không phép";
            }

            // Update status
            employee.StatusId = statusId;

            // Handle leave tracking logic
            if (statusId == 2) // Tạm ngưng
            {
                // Ensure not soft-deleted for temporary leave
                employee.IsDeleted = false;
                
                if (!string.IsNullOrWhiteSpace(leaveReason))
                {
                    // Excused leave
                    employee.LeaveReason = leaveReason;
                    employee.LeaveStartDate = DateTime.Now;
                    employee.UnexcusedDaysCount = 0;
                    employee.LastUnexcusedDate = null;
                }
                else
                {
                    // Unexcused leave
                    employee.LeaveReason = "Nghỉ không phép";
                    employee.LeaveStartDate = DateTime.Now;
                    employee.UnexcusedDaysCount = 1;
                    employee.LastUnexcusedDate = DateTime.Now;
                }
            }
            else if (statusId == 1) // Đang làm việc
            {
                // Ensure not soft-deleted for active employees
                employee.IsDeleted = false;
                
                // Reset leave tracking
                employee.LeaveReason = null;
                employee.LeaveStartDate = null;
                employee.UnexcusedDaysCount = 0;
                employee.LastUnexcusedDate = null;
            }
            else if (statusId == 3) // Đã nghỉ việc
            {
                // Soft-delete for archived employees
                employee.IsDeleted = true;
                
                // Archive - keep leave history but mark as terminated
                employee.LeaveReason = leaveReason ?? "Nghỉ không phép";
                employee.LeaveStartDate = employee.LeaveStartDate ?? DateTime.Now;
            }

            // Đồng bộ trạng thái khóa / mở khóa tài khoản đăng nhập Readers
            if (!string.IsNullOrWhiteSpace(employee.Email))
            {
                var reader = await _context.Readers
                    .FirstOrDefaultAsync(r => r.Email == employee.Email);
                if (reader != null)
                {
                    reader.IsActive = (statusId == 1);
                    _context.Readers.Update(reader);
                }
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Cập nhật trạng thái thành công." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Đã xảy ra lỗi: " + ex.Message });
        }
    }

    // ========================= SA THẢI (TERMINATE) =========================
    // Sets employee status to "Đã nghỉ việc" (3) - moves to Tab 3

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Terminate(int id)
    {
        try
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

            if (employee == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Nhân viên không tồn tại."
                });
            }

            // Bảo vệ Root Admin
            if (employee.EmployeeId == "NV001" || employee.Email == "admin@library.com")
            {
                return Json(new
                {
                    success = false,
                    message = "Không thể sa thải tài khoản Quản trị viên tối cao."
                });
            }

            employee.StatusId = 3; // Đã nghỉ việc
            employee.LeaveStartDate = employee.LeaveStartDate ?? DateTime.Now;

            // Khóa tài khoản đăng nhập Readers lập tức
            if (!string.IsNullOrWhiteSpace(employee.Email))
            {
                var reader = await _context.Readers
                    .FirstOrDefaultAsync(r => r.Email == employee.Email);
                if (reader != null)
                {
                    reader.IsActive = false;
                    _context.Readers.Update(reader);
                }
            }

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = $"Đã sa thải nhân viên \"{employee.FullName}\" và khóa tài khoản đăng nhập thành công."
            });
        }
        catch (Exception ex)
        {
            return Json(new
            {
                success = false,
                message = "Đã xảy ra lỗi hệ thống: " + ex.Message
            });
        }
    }

    // ========================= REHIRE / REACTIVATE =========================
    // Reactivates a soft-deleted employee for rehire

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rehire(int id)
    {
        try
        {
            var employee = await _context.Employees
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Nhân viên không tồn tại."
                });
            }

            // Reactivate employee
            employee.IsDeleted = false;
            employee.StatusId = 1; // Set to "Đang làm việc"
            
            // Reset leave tracking
            employee.LeaveReason = null;
            employee.LeaveStartDate = null;
            employee.UnexcusedDaysCount = 0;
            employee.LastUnexcusedDate = null;

            // Mở khóa lại tài khoản đăng nhập Readers
            if (!string.IsNullOrWhiteSpace(employee.Email))
            {
                var reader = await _context.Readers
                    .FirstOrDefaultAsync(r => r.Email == employee.Email);
                if (reader != null)
                {
                    reader.IsActive = true;
                    _context.Readers.Update(reader);
                }
            }

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Đã khôi phục hồ sơ và kích hoạt lại tài khoản nhân viên thành công!"
            });
        }
        catch (Exception ex)
        {
            return Json(new
            {
                success = false,
                message = "Đã xảy ra lỗi hệ thống: " + ex.Message
            });
        }
    }

    // ========================= PERMANENT ARCHIVE =========================
    // Permanently hides employee from all UI views while preserving database history

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ArchivePermanent(int id)
    {
        try
        {
            var employee = await _context.Employees
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Nhân viên không tồn tại."
                });
            }

            // Mark as permanently archived
            employee.IsDeleted = true;
            employee.StatusId = 3; // Ensure status is "Đã nghỉ việc"

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = $"Đã ẩn vĩnh viễn nhân viên \"{employee.FullName}\" khỏi danh sách. Dữ liệu vẫn được lưu trữ trong hệ thống."
            });
        }
        catch (Exception ex)
        {
            return Json(new
            {
                success = false,
                message = "Đã xảy ra lỗi hệ thống: " + ex.Message
            });
        }
    }

    // ========================= PHÂN QUYỀN =========================

    [HttpGet]
    [PermissionAuthorize("employee.assign_role")]
    public async Task<IActionResult> PhanQuyen(int id)
    {
        var employee = await _context.Employees
            .Include(e => e.Role)
            .Include(e => e.Position)
            .Include(e => e.UserPermissions)
            .ThenInclude(up => up.Permission)
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

        if (employee == null)
        {
            TempData["Error"] = "Nhân viên không tồn tại hoặc đã bị xóa.";
            return RedirectToAction("Index");
        }

        // If RoleId is not set, auto-select Role matching Position name
        if (!employee.RoleId.HasValue && employee.Position != null)
        {
            var matchingRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == employee.Position.Name);
            if (matchingRole != null)
            {
                employee.RoleId = matchingRole.Id;
                await _context.SaveChangesAsync();
            }
        }

        // Check if employee has Readers account
        Readers? readerAccount = null;
        if (!string.IsNullOrWhiteSpace(employee.Email))
        {
            readerAccount = await _context.Readers
                .FirstOrDefaultAsync(r => r.Email == employee.Email ||
                    (!string.IsNullOrEmpty(employee.EmployeeId) && r.ReaderCode == "RD_" + employee.EmployeeId));
        }

        var roles = await _context.Roles.OrderBy(r => r.Id).ToListAsync();
        var permissions = await _context.Permissions.OrderBy(p => p.GroupName).ThenBy(p => p.DisplayName).ToListAsync();
        var permissionGroups = permissions.GroupBy(p => p.GroupName).ToDictionary(g => g.Key, g => g.ToList());

        // Calculate accurate active permissions for this employee
        var activePermissionIds = new HashSet<int>();
        var currentRole = employee.Role ?? (employee.RoleId.HasValue ? roles.FirstOrDefault(r => r.Id == employee.RoleId.Value) : null);

        if (currentRole != null && string.Equals(currentRole.Name, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            activePermissionIds = permissions.Select(p => p.Id).ToHashSet();
        }
        else
        {
            if (employee.RoleId.HasValue)
            {
                var rolePermIds = await _context.RolePermissions
                    .Where(rp => rp.RoleId == employee.RoleId.Value)
                    .Select(rp => rp.PermissionId)
                    .ToListAsync();

                foreach (var pid in rolePermIds)
                {
                    activePermissionIds.Add(pid);
                }
            }

            // Apply overrides: grant or revoke
            foreach (var up in employee.UserPermissions)
            {
                if (up.IsGranted)
                {
                    activePermissionIds.Add(up.PermissionId);
                }
                else
                {
                    activePermissionIds.Remove(up.PermissionId);
                }
            }
        }

        var viewModel = new PhanQuyenViewModel
        {
            EmployeeId = employee.Id,
            EmployeeName = employee.FullName,
            EmployeeEmail = employee.Email,
            RoleId = employee.RoleId,
            Roles = roles,
            PermissionGroups = permissionGroups,
            UserPermissionIds = activePermissionIds,
            HasReaderAccount = readerAccount != null,
            ReaderAccountEmail = readerAccount?.Email
        };

        if (readerAccount == null)
        {
            TempData["Warning"] = "Nhân viên này chưa có tài khoản đăng nhập. Khi bạn lưu phân quyền, hệ thống sẽ tự động kích hoạt tài khoản.";
        }
        else
        {
            TempData["Info"] = $"Nhân viên đã có tài khoản đăng nhập (Email: {readerAccount.Email}).";
        }

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [PermissionAuthorize("employee.assign_role")]
    public async Task<IActionResult> PhanQuyen(PhanQuyenViewModel model)
    {
        var employee = await _context.Employees
            .Include(e => e.UserPermissions)
            .Include(e => e.Position)
            .Include(e => e.Role)
            .FirstOrDefaultAsync(e => e.Id == model.EmployeeId && !e.IsDeleted);

        if (employee == null)
        {
            TempData["Error"] = "Nhân viên không tồn tại hoặc đã bị xóa.";
            return RedirectToAction("Index");
        }

        // Bảo vệ tài khoản Root Admin
        if (employee.EmployeeId == "NV001" || employee.Email == "admin@library.com")
        {
            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            if (adminRole != null && model.RoleId != adminRole.Id)
            {
                TempData["Error"] = "Không thể giáng chức hoặc gỡ vai trò Admin của tài khoản Quản trị viên tối cao (Root Admin).";
                return RedirectToAction("PhanQuyen", new { id = employee.Id });
            }
        }

        // Cập nhật Role
        employee.RoleId = model.RoleId;
        string roleName = "Nhân viên";

        if (model.RoleId.HasValue)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == model.RoleId.Value);
            if (role != null)
            {
                roleName = role.Name;
                var position = await _context.Positions.FirstOrDefaultAsync(p => p.Name == role.Name);
                if (position != null)
                {
                    employee.PositionId = position.Id;
                }
            }
        }

        // Xóa các ghi đè phân quyền cũ
        _context.UserPermissions.RemoveRange(employee.UserPermissions);

        // Tính toán các quyền ghi đè (chỉ khi không phải Admin vì Admin sở hữu toàn quyền hệ thống)
        if (!string.Equals(roleName, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            var selectedIds = new HashSet<int>(model.SelectedPermissionIds ?? Enumerable.Empty<int>());
            var rolePermissions = model.RoleId.HasValue
                ? await _context.RolePermissions
                    .Where(rp => rp.RoleId == model.RoleId.Value)
                    .Select(rp => rp.PermissionId)
                    .ToListAsync()
                : new List<int>();

            var rolePermSet = new HashSet<int>(rolePermissions);

            // 1. Quyền được chọn nhưng vai trò không có -> ghi đè cấp quyền (IsGranted = true)
            foreach (var permissionId in selectedIds)
            {
                if (!rolePermSet.Contains(permissionId))
                {
                    employee.UserPermissions.Add(new UserPermission
                    {
                        EmployeeId = employee.Id,
                        PermissionId = permissionId,
                        IsGranted = true
                    });
                }
            }

            // 2. Quyền có trong vai trò nhưng bị bỏ chọn -> ghi đè tước quyền (IsGranted = false)
            foreach (var rolePermissionId in rolePermSet)
            {
                if (!selectedIds.Contains(rolePermissionId))
                {
                    employee.UserPermissions.Add(new UserPermission
                    {
                        EmployeeId = employee.Id,
                        PermissionId = rolePermissionId,
                        IsGranted = false
                    });
                }
            }
        }

        await _context.SaveChangesAsync();

        // Đồng bộ tài khoản Readers
        if (!string.IsNullOrWhiteSpace(employee.Email))
        {
            var readerAccount = await _context.Readers
                .FirstOrDefaultAsync(r => r.Email == employee.Email ||
                    (!string.IsNullOrEmpty(employee.EmployeeId) && r.ReaderCode == "RD_" + employee.EmployeeId));

            if (readerAccount == null)
            {
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%";
                var random = new Random();
                var tempPassword = new string(Enumerable.Repeat(chars, 8)
                    .Select(s => s[random.Next(s.Length)]).ToArray());

                var hasher = new PasswordHasher<Readers>();
                var hashedPassword = hasher.HashPassword(null!, tempPassword);

                var newReader = new Readers
                {
                    ReaderCode = $"RD_{employee.EmployeeId}",
                    Email = employee.Email,
                    FullName = employee.FullName,
                    Password = hashedPassword,
                    Role = roleName,
                    ForcePasswordChange = true,
                    IsActive = (employee.StatusId == 1),
                    EmailConfirmed = true,
                    Phone = employee.Phone ?? "",
                    Address = employee.Address ?? "",
                    IdentityNumber = employee.CCCD ?? "",
                    DateOfBirth = employee.DateOfBirth,
                    RegisterDate = DateTime.Now,
                    ExpirationDate = DateTime.Now.AddYears(5),
                    MaxBorrowBooks = 10,
                    ReadersType = ReadersType.Teacher,
                    Genders = (int)employee.Gender switch
                    {
                        1 => Genders.Male,
                        2 => Genders.Female,
                        _ => Genders.Other
                    }
                };

                _context.Readers.Add(newReader);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Đã lưu phân quyền cho nhân viên \"{employee.FullName}\" thành công. Đã tạo tài khoản với vai trò \"{roleName}\". Mật khẩu tạm thời: {tempPassword} - nhân viên cần đổi mật khẩu khi đăng nhập lần đầu.";
            }
            else
            {
                readerAccount.Role = roleName;
                readerAccount.FullName = employee.FullName;
                if (!string.IsNullOrWhiteSpace(employee.Phone)) readerAccount.Phone = employee.Phone;
                if (!string.IsNullOrWhiteSpace(employee.Address)) readerAccount.Address = employee.Address;
                if (!string.IsNullOrWhiteSpace(employee.CCCD)) readerAccount.IdentityNumber = employee.CCCD;
                readerAccount.IsActive = (employee.StatusId == 1);

                _context.Readers.Update(readerAccount);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Đã cập nhật phân quyền và đồng bộ vai trò \"{roleName}\" cho tài khoản của nhân viên \"{employee.FullName}\" thành công.";
            }
        }
        else
        {
            TempData["Success"] = $"Đã lưu phân quyền cho nhân viên \"{employee.FullName}\" thành công.";
        }

        // Cập nhật phiên làm việc nếu admin đang tự chỉnh sửa tài khoản của mình
        var currentEmployeeIdStr = HttpContext.Session.GetString("EmployeeId");
        if (!string.IsNullOrEmpty(currentEmployeeIdStr) && currentEmployeeIdStr == employee.Id.ToString())
        {
            HttpContext.Session.SetString("Role", roleName);
            HttpContext.Items[PermissionExtensions.ItemsKey] = null;
        }

        return RedirectToAction("Index");
    }

    // ========================= SOFT DELETE =========================
    // Sets IsDeleted = true - removes from all UI views

    [HttpPost]
    [ValidateAntiForgeryToken]
    [PermissionAuthorize("employee.delete")]
    public async Task<IActionResult> SoftDelete(int id)
    {
        try
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

            if (employee == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Nhân viên không tồn tại."
                });
            }

            // Bảo vệ Root Admin
            if (employee.EmployeeId == "NV001" || employee.Email == "admin@library.com")
            {
                return Json(new
                {
                    success = false,
                    message = "Không thể xóa tài khoản Quản trị viên tối cao."
                });
            }

            employee.IsDeleted = true;

            // Đồng bộ khóa tài khoản Readers
            if (!string.IsNullOrWhiteSpace(employee.Email))
            {
                var reader = await _context.Readers
                    .FirstOrDefaultAsync(r => r.Email == employee.Email);
                if (reader != null)
                {
                    reader.IsActive = false;
                    _context.Readers.Update(reader);
                }
            }

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = $"Đã xóa lưu trữ nhân viên \"{employee.FullName}\" và khóa tài khoản đăng nhập thành công."
            });
        }
        catch (Exception ex)
        {
            return Json(new
            {
                success = false,
                message = "Đã xảy ra lỗi hệ thống: " + ex.Message
            });
        }
    }

    // ========================= DELETE (LEGACY - REMOVED) =========================
    // Replaced with SoftDelete action

    // ========================= HELPER =========================
    private bool EmployeeExists(int id)
    {
        return _context.Employees.Any(e => e.Id == id);
    }
}