using System.Text;
using LibraryHub.Application;
using LibraryHub.Domain;
using LibraryHub.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LibraryHub.Web;

public static class InitialAccountSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var services = scope.ServiceProvider;
        var db = services.GetRequiredService<LibraryDbContext>();
        if (!db.Database.IsRelational()) return;

        var userManager = services.GetRequiredService<UserManager<LibraryUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var permissionSeeder = services.GetRequiredService<IPermissionSeeder>();

        // 1. Đồng bộ các vai trò Identity
        foreach (var roleName in new[] { "Administrator", "Staff", "Reader" })
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
            }
        }

        // 2. Đồng bộ các mã quyền RBAC
        await permissionSeeder.SeedAsync();

        // 3. Khởi tạo / cập nhật chức vụ (Positions)
        var now = DateTimeOffset.UtcNow;
        var positions = new Dictionary<string, Position>(StringComparer.OrdinalIgnoreCase);
        var existingPositions = await db.Positions.ToListAsync();
        foreach (var p in existingPositions) positions[p.Code] = p;

        Position GetOrCreatePosition(string code, string name, decimal salary)
        {
            if (positions.TryGetValue(code, out var pos)) return pos;
            pos = new Position
            {
                Code = code,
                Name = name,
                SalaryAmount = salary,
                AllowSalaryOverride = true,
                CreatedAt = now,
                UpdatedAt = now
            };
            db.Positions.Add(pos);
            positions[code] = pos;
            return pos;
        }

        var posAdmin = GetOrCreatePosition("ADMIN", "Quản trị viên", 25000000m);
        var posLibrarian = GetOrCreatePosition("DEMO-LIBRARIAN", "Thủ thư", 12000000m);
        posAdmin.Name = "Quản trị viên";
        posAdmin.IsDeleted = false;
        posLibrarian.Name = "Thủ thư";
        posLibrarian.IsDeleted = false;
        await db.SaveChangesAsync();

        // 4. Khởi tạo / cập nhật nhóm quyền nghiệp vụ (chỉ giữ 2 nhóm nhân sự: Quản trị viên & Thủ thư)
        var securityRoles = new Dictionary<string, Role>(StringComparer.OrdinalIgnoreCase);
        var existingSecurityRoles = await db.SecurityRoles.ToListAsync();
        foreach (var r in existingSecurityRoles) securityRoles[r.Code] = r;

        Role GetOrCreateSecurityRole(string code, string name)
        {
            if (securityRoles.TryGetValue(code, out var r))
            {
                r.Name = name;
                r.IsDeleted = false;
                return r;
            }
            r = new Role
            {
                Code = code,
                Name = name,
                IsSystem = true,
                CreatedAt = now,
                UpdatedAt = now
            };
            db.SecurityRoles.Add(r);
            securityRoles[code] = r;
            return r;
        }

        var roleAdmin = GetOrCreateSecurityRole("ADMIN", "Quản trị viên");
        var roleLibrarianSec = GetOrCreateSecurityRole("DEMO-CIRCULATION-STAFF", "Thủ thư");
        await db.SaveChangesAsync();

        var activeEmpRoleIds = await db.Employees.Where(e => !e.IsDeleted && e.RoleId.HasValue)
            .Select(e => e.RoleId!.Value).Distinct().ToListAsync();
        foreach (var r in existingSecurityRoles)
        {
            if (r.Id != roleAdmin.Id && r.Id != roleLibrarianSec.Id && !activeEmpRoleIds.Contains(r.Id))
                r.IsDeleted = true;
        }
        var activeEmpPosIds = await db.Employees.Where(e => !e.IsDeleted)
            .Select(e => e.PositionId).Distinct().ToListAsync();
        foreach (var p in existingPositions)
        {
            if (p.Id != posAdmin.Id && p.Id != posLibrarian.Id && !activeEmpPosIds.Contains(p.Id))
                p.IsDeleted = true;
        }
        await db.SaveChangesAsync();

        // Cấp toàn bộ quyền cho role ADMIN nếu chưa có
        var allPermissions = await db.Permissions.ToListAsync();
        var adminExistingPermissions = await db.RolePermissions.Where(rp => rp.RoleId == roleAdmin.Id).Select(rp => rp.PermissionId).ToListAsync();
        foreach (var perm in allPermissions)
        {
            if (!adminExistingPermissions.Contains(perm.Id))
            {
                db.RolePermissions.Add(new RolePermission
                {
                    RoleId = roleAdmin.Id,
                    PermissionId = perm.Id,
                    Effect = PermissionEffect.Allow
                });
            }
        }
        await db.SaveChangesAsync();

        // 5. Hàm tạo hoặc cập nhật tài khoản
        async Task<LibraryUser> EnsureUserAsync(
            string email,
            string password,
            string displayName,
            AccountKind kind,
            string identityRole)
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();
            var user = await userManager.FindByEmailAsync(normalizedEmail);
            if (user is null)
            {
                user = new LibraryUser
                {
                    Id = Guid.NewGuid(),
                    UserName = normalizedEmail,
                    Email = normalizedEmail,
                    DisplayName = displayName,
                    AccountKind = kind,
                    EmailConfirmed = true,
                    IsActive = true,
                    MustChangePassword = false
                };
                var createResult = await userManager.CreateAsync(user, password);
                if (!createResult.Succeeded)
                {
                    throw new InvalidOperationException($"Không thể tạo tài khoản {email}: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                user.EmailConfirmed = true;
                user.IsActive = true;
                user.MustChangePassword = false;
                user.DisplayName = displayName;
                user.AccountKind = kind;
                await userManager.UpdateAsync(user);

                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                await userManager.ResetPasswordAsync(user, token, password);
            }

            if (!await userManager.IsInRoleAsync(user, identityRole))
            {
                await userManager.AddToRoleAsync(user, identityRole);
            }

            return user;
        }

        async Task EnsureStaffAsync(
            string email,
            string password,
            string fullName,
            string employeeCode,
            string phone,
            Position position,
            Role securityRole,
            string identityRole = "Staff")
        {
            var user = await EnsureUserAsync(
                email,
                password,
                fullName,
                identityRole == "Administrator" ? AccountKind.Administrator : AccountKind.Internal,
                identityRole);

            var employee = await db.Employees.SingleOrDefaultAsync(e => e.EmployeeCode == employeeCode);
            if (employee is null)
            {
                employee = new Employee
                {
                    EmployeeCode = employeeCode,
                    FullName = fullName,
                    Email = email,
                    Phone = phone,
                    Address = "Hà Nội",
                    NationalId = "00120" + Math.Abs(email.GetHashCode()).ToString().PadLeft(7, '0')[..7],
                    DateOfBirth = new DateOnly(1995, 1, 1),
                    HiredOn = new DateOnly(2022, 1, 1),
                    PositionId = position.Id,
                    Salary = position.SalaryAmount,
                    RoleId = securityRole.Id,
                    Status = EmploymentStatus.Working,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                db.Employees.Add(employee);
                await db.SaveChangesAsync();
            }
            else
            {
                employee.Email = email;
                employee.FullName = fullName;
                employee.PositionId = position.Id;
                employee.RoleId = securityRole.Id;
                employee.Salary = position.SalaryAmount;
                employee.Status = EmploymentStatus.Working;
                employee.IsDeleted = false;
                await db.SaveChangesAsync();
            }

            var staffAccount = await db.StaffAccounts.SingleOrDefaultAsync(sa => sa.UserAccountId == user.Id);
            if (staffAccount is null)
            {
                db.StaffAccounts.Add(new StaffAccount
                {
                    UserAccountId = user.Id,
                    EmployeeId = employee.Id
                });
                await db.SaveChangesAsync();
            }
            else if (staffAccount.EmployeeId != employee.Id)
            {
                staffAccount.EmployeeId = employee.Id;
                await db.SaveChangesAsync();
            }
        }

        async Task EnsureReaderAsync(
            string email,
            string password,
            string fullName,
            string readerCode,
            string phone)
        {
            var user = await EnsureUserAsync(email, password, fullName, AccountKind.Reader, "Reader");

            var reader = await db.Readers.SingleOrDefaultAsync(r => r.UserAccountId == user.Id || r.Email == email);
            if (reader is null)
            {
                reader = new Reader
                {
                    UserAccountId = user.Id,
                    ReaderCode = readerCode,
                    FullName = fullName,
                    Email = email,
                    Phone = phone,
                    Address = "Hà Nội",
                    RegisteredOn = DateOnly.FromDateTime(DateTime.UtcNow),
                    ExpiresOn = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)),
                    EmailConfirmed = true,
                    MaxBorrowBooks = 10,
                    Type = ReaderType.Student,
                    IsDeleted = false,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                db.Readers.Add(reader);
                await db.SaveChangesAsync();
            }
            else
            {
                reader.UserAccountId = user.Id;
                reader.Email = email;
                reader.FullName = fullName;
                reader.EmailConfirmed = true;
                reader.IsDeleted = false;
                await db.SaveChangesAsync();
            }
        }

        // 6. Khởi tạo 3 nhóm tài khoản chính: Quản trị viên, Thủ thư, Độc giả
        // Admin (Quản trị viên)
        await EnsureStaffAsync("admin@library.com", "Admin@123", "Nguyễn Hoàng Long", "NV001", "0901234567", posAdmin, roleAdmin, "Administrator");

        // Librarian (Thủ thư)
        await EnsureStaffAsync("librarian@library.com", "Librarian@123", "Lê Văn Hùng", "NV003", "0903456789", posLibrarian, roleLibrarianSec, "Staff");

        // Reader (Độc giả)
        await EnsureReaderAsync("reader@library.com", "Reader@123", "Lê Phú An", "DG001", "0911223344");
        await EnsureReaderAsync("docgia@gmail.com", "Reader@123", "Nguyễn Văn An", "DG002", "0922334455");

        // Gỡ bỏ các tài khoản CSKH / Quản lý / Thủ kho dư thừa nếu từng được seed trước đó
        foreach (var removedEmail in new[] { "manager@library.com", "warehouse@library.com", "support@library.com" })
        {
            var removedUser = await userManager.FindByEmailAsync(removedEmail);
            if (removedUser is null) continue;
            var staffLinks = await db.StaffAccounts.Where(x => x.UserAccountId == removedUser.Id).ToListAsync();
            if (staffLinks.Count > 0) db.StaffAccounts.RemoveRange(staffLinks);
            var empRows = await db.Employees.Where(x => x.Email == removedEmail).ToListAsync();
            foreach (var emp in empRows) emp.IsDeleted = true;
            await db.SaveChangesAsync();
            await userManager.DeleteAsync(removedUser);
        }

        // 7. Tự động xác thực toàn bộ tài khoản độc giả và người dùng có sẵn trong DB (chỉ chạy SQL thô khi dùng Relational DB)
        if (db.Database.IsRelational())
        {
            await db.Database.ExecuteSqlRawAsync("UPDATE AspNetUsers SET EmailConfirmed = 1, IsActive = 1");
            await db.Database.ExecuteSqlRawAsync("UPDATE Readers SET EmailConfirmed = 1, IsDeleted = 0");
        }

        // 8. Gán ảnh minh họa mặc định cho các danh mục chưa có ảnh
        var defaultCategoryImages = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Khoa học tự nhiên"] = "/uploads/categories/khoa-hoc-tu-nhien.jpg",
            ["Khoa học viễn tưởng"] = "/uploads/categories/khoa-hoc-vien-tuong.jpg",
            ["Lập trình & bảo mật"] = "/uploads/categories/lap-trinh-bao-mat.jpg",
            ["Phát triển bản thân"] = "/uploads/categories/phat-trien-ban-than.jpg",
            ["Tâm lý & chữa lành"] = "/uploads/categories/tam-ly-chua-lanh.jpg",
            ["Tiểu thuyết đương đại"] = "/uploads/categories/tieu-thuyet-duong-dai.jpg",
            ["Toán & khoa học máy tính"] = "/uploads/categories/toan-khoa-hoc-may-tinh.jpg",
            ["Văn học kinh điển"] = "/uploads/categories/van-hoc-kinh-dien.jpg"
        };
        var categories = await db.Categories.Where(x => !x.IsDeleted).ToListAsync();
        var updatedCategoryImages = false;
        foreach (var category in categories)
        {
            if (string.IsNullOrWhiteSpace(category.ImagePath)
                && defaultCategoryImages.TryGetValue(category.Name.Trim(), out var imagePath))
            {
                category.ImagePath = imagePath;
                updatedCategoryImages = true;
            }
        }
        if (updatedCategoryImages)
            await db.SaveChangesAsync();
    }
}
