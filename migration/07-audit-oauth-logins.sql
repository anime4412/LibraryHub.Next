SET NOCOUNT ON;

IF DB_NAME() NOT IN (N'LibraryHubNext', N'LibraryHubNext_IntegrationTests')
    THROW 51000, N'Script chỉ được phép đọc LibraryHubNext hoặc LibraryHubNext_IntegrationTests.', 1;

IF OBJECT_ID(N'dbo.AspNetUsers', N'U') IS NULL
   OR OBJECT_ID(N'dbo.AspNetUserLogins', N'U') IS NULL
   OR OBJECT_ID(N'dbo.Readers', N'U') IS NULL
    THROW 51001, N'Database chưa có đủ bảng Identity/Readers. Hãy áp dụng migration hiện có trước.', 1;

SELECT
    l.LoginProvider AS [NhaCungCap],
    COUNT_BIG(*) AS [SoTaiKhoanDaLienKet],
    MAX(u.LastLoginAt) AS [LanDangNhapMoiNhat]
FROM dbo.AspNetUserLogins AS l
INNER JOIN dbo.AspNetUsers AS u ON u.Id = l.UserId
WHERE l.LoginProvider IN (N'Google', N'Facebook')
GROUP BY l.LoginProvider
ORDER BY l.LoginProvider;

SELECT
    u.Email,
    u.DisplayName,
    l.LoginProvider AS [NhaCungCap],
    l.ProviderDisplayName AS [TenHienThiNhaCungCap],
    CASE WHEN u.PasswordHash IS NULL
        THEN N'Chi dang nhap ngoai'
        ELSE N'Co mat khau LibraryHub'
    END AS [KieuTaiKhoan],
    u.EmailConfirmed,
    u.IsActive,
    u.LastLoginAt AS [LanDangNhapTaiKhoan],
    r.ReaderCode,
    r.FullName AS [TenDocGia],
    r.LastLoginAt AS [LanDangNhapDocGia]
FROM dbo.AspNetUserLogins AS l
INNER JOIN dbo.AspNetUsers AS u ON u.Id = l.UserId
LEFT JOIN dbo.Readers AS r ON r.UserAccountId = u.Id AND r.IsDeleted = 0
WHERE l.LoginProvider IN (N'Google', N'Facebook')
ORDER BY u.LastLoginAt DESC, u.Email;

-- Cố ý không xuất ProviderKey, PasswordHash, token, cookie hoặc secret.
-- Script chỉ đọc; không có INSERT, UPDATE, DELETE, MERGE, DROP hay TRUNCATE.
