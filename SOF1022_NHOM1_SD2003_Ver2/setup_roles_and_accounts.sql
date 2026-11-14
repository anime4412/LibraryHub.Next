-- 1. Sync Positions
SET IDENTITY_INSERT Positions ON;
IF NOT EXISTS (SELECT 1 FROM Positions WHERE Id = 5)
    INSERT INTO Positions (Id, Name, Description, CanManageBooks, CanManageUsers, CanManageLoans, CanManageReports, CanManageSystem, CreatedAt)
    VALUES (5, N'Kế toán', N'Quản lý tài chính và xử lý phạt tiền', 0, 0, 0, 1, 0, GETDATE());

IF NOT EXISTS (SELECT 1 FROM Positions WHERE Id = 6)
    INSERT INTO Positions (Id, Name, Description, CanManageBooks, CanManageUsers, CanManageLoans, CanManageReports, CanManageSystem, CreatedAt)
    VALUES (6, N'Chăm sóc khách hàng', N'Hỗ trợ bạn đọc và trực kênh tư vấn', 0, 1, 1, 0, 0, GETDATE());
SET IDENTITY_INSERT Positions OFF;

UPDATE Positions SET Name = N'Admin', Description = N'Quản trị viên hệ thống' WHERE Id = 1;
UPDATE Positions SET Name = N'Quản lý thư viện', Description = N'Giám sát vận hành và duyệt tài liệu' WHERE Id = 2;
UPDATE Positions SET Name = N'Thủ thư', Description = N'Quản lý danh mục sách và xử lý mượn trả tại quầy' WHERE Id = 3;
UPDATE Positions SET Name = N'Nhân viên kho', Description = N'Quản lý kệ sách, kiểm kê và phân bổ vị trí' WHERE Id = 4;
UPDATE Positions SET Name = N'Kế toán', Description = N'Quản lý tài chính và xử lý phạt tiền' WHERE Id = 5;
UPDATE Positions SET Name = N'Chăm sóc khách hàng', Description = N'Hỗ trợ bạn đọc và trực kênh tư vấn' WHERE Id = 6;

-- 2. Sync Employees PositionId & RoleId
UPDATE Employees SET PositionId = 1, RoleId = 1 WHERE EmployeeId = 'NV001';
UPDATE Employees SET PositionId = 2, RoleId = 2 WHERE EmployeeId = 'NV002';
UPDATE Employees SET PositionId = 3, RoleId = 3 WHERE EmployeeId = 'NV003';
UPDATE Employees SET PositionId = 4, RoleId = 4 WHERE EmployeeId = 'NV004';
UPDATE Employees SET PositionId = 5, RoleId = 5 WHERE EmployeeId = 'NV005';
UPDATE Employees SET PositionId = 6, RoleId = 6 WHERE EmployeeId = 'NV006';

-- 3. Add Missing Loan & Reservation Permissions to Role 3 (Thủ thư)
DECLARE @PermList TABLE (PermCode NVARCHAR(50));
INSERT INTO @PermList VALUES 
('borrow.view'), ('borrow.create'), ('borrow.edit'), ('borrow.confirm_return'),
('reservation.view'), ('reservation.create'), ('reservation.edit'), ('reservation.confirm'), ('reservation.cancel');

INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT 3, p.Id
FROM Permissions p
JOIN @PermList pl ON p.Code = pl.PermCode
WHERE NOT EXISTS (
    SELECT 1 FROM RolePermissions rp WHERE rp.RoleId = 3 AND rp.PermissionId = p.Id
);

-- 4. Create Login Accounts in Readers for the 5 Staff Members
IF NOT EXISTS (SELECT 1 FROM Readers WHERE Email = 'quanly@library.com')
BEGIN
    INSERT INTO Readers (ReaderCode, FullName, DateOfBirth, Phone, Email, Address, IdentityNumber, RegisterDate, ExpirationDate, ConsecutiveLoginDays, IsCommentBlocked, MaxBorrowBooks, Password, Role, IsActive, EmailConfirmed, ForcePasswordChange, ReadersType, Genders, IsDeleted, IsNameSet, IsVerified)
    VALUES ('RD_NV002', N'Trần Thị Mai Phương', '1992-07-22', '0987654321', 'quanly@library.com', N'275 Nguyễn Trãi, Thanh Xuân, Hà Nội', '001192023456', GETDATE(), DATEADD(YEAR, 5, GETDATE()), 0, 0, 10, 'Quanly@123', N'Quản lý thư viện', 1, 1, 0, 1, 0, 0, 1, 1);
END
ELSE
BEGIN
    UPDATE Readers SET Role = N'Quản lý thư viện', IsActive = 1, EmailConfirmed = 1, IsVerified = 1 WHERE Email = 'quanly@library.com';
END

IF NOT EXISTS (SELECT 1 FROM Readers WHERE Email = 'thuthu@library.com')
BEGIN
    INSERT INTO Readers (ReaderCode, FullName, DateOfBirth, Phone, Email, Address, IdentityNumber, RegisterDate, ExpirationDate, ConsecutiveLoginDays, IsCommentBlocked, MaxBorrowBooks, Password, Role, IsActive, EmailConfirmed, ForcePasswordChange, ReadersType, Genders, IsDeleted, IsNameSet, IsVerified)
    VALUES ('RD_NV003', N'Lê Minh Cường', '1994-11-10', '0903123456', 'thuthu@library.com', N'12 Kim Mã, Ba Đình, Hà Nội', '001094034567', GETDATE(), DATEADD(YEAR, 5, GETDATE()), 0, 0, 10, 'Thuthu@123', N'Thủ thư', 1, 1, 0, 1, 1, 0, 1, 1);
END
ELSE
BEGIN
    UPDATE Readers SET Role = N'Thủ thư', IsActive = 1, EmailConfirmed = 1, IsVerified = 1 WHERE Email = 'thuthu@library.com';
END

IF NOT EXISTS (SELECT 1 FROM Readers WHERE Email = 'kho@library.com')
BEGIN
    INSERT INTO Readers (ReaderCode, FullName, DateOfBirth, Phone, Email, Address, IdentityNumber, RegisterDate, ExpirationDate, ConsecutiveLoginDays, IsCommentBlocked, MaxBorrowBooks, Password, Role, IsActive, EmailConfirmed, ForcePasswordChange, ReadersType, Genders, IsDeleted, IsNameSet, IsVerified)
    VALUES ('RD_NV004', N'Phạm Thu Hà', '1996-05-18', '0978987654', 'kho@library.com', N'45 Cầu Giấy, Cầu Giấy, Hà Nội', '001196045678', GETDATE(), DATEADD(YEAR, 5, GETDATE()), 0, 0, 10, 'Kho@123', N'Nhân viên kho', 1, 1, 0, 1, 0, 0, 1, 1);
END
ELSE
BEGIN
    UPDATE Readers SET Role = N'Nhân viên kho', IsActive = 1, EmailConfirmed = 1, IsVerified = 1 WHERE Email = 'kho@library.com';
END

IF NOT EXISTS (SELECT 1 FROM Readers WHERE Email = 'ketoan@library.com')
BEGIN
    INSERT INTO Readers (ReaderCode, FullName, DateOfBirth, Phone, Email, Address, IdentityNumber, RegisterDate, ExpirationDate, ConsecutiveLoginDays, IsCommentBlocked, MaxBorrowBooks, Password, Role, IsActive, EmailConfirmed, ForcePasswordChange, ReadersType, Genders, IsDeleted, IsNameSet, IsVerified)
    VALUES ('RD_NV005', N'Đặng Văn Hùng', '1993-09-30', '0945678901', 'ketoan@library.com', N'88 Láng Hạ, Đống Đa, Hà Nội', '001093056789', GETDATE(), DATEADD(YEAR, 5, GETDATE()), 0, 0, 10, 'Ketoan@123', N'Kế toán', 1, 1, 0, 1, 1, 0, 1, 1);
END
ELSE
BEGIN
    UPDATE Readers SET Role = N'Kế toán', IsActive = 1, EmailConfirmed = 1, IsVerified = 1 WHERE Email = 'ketoan@library.com';
END

IF NOT EXISTS (SELECT 1 FROM Readers WHERE Email = 'cskh@library.com')
BEGIN
    INSERT INTO Readers (ReaderCode, FullName, DateOfBirth, Phone, Email, Address, IdentityNumber, RegisterDate, ExpirationDate, ConsecutiveLoginDays, IsCommentBlocked, MaxBorrowBooks, Password, Role, IsActive, EmailConfirmed, ForcePasswordChange, ReadersType, Genders, IsDeleted, IsNameSet, IsVerified)
    VALUES ('RD_NV006', N'Vũ Ngọc Lan', '1998-12-05', '0932123456', 'cskh@library.com', N'15 Hai Bà Trưng, Hoàn Kiếm, Hà Nội', '001198067890', GETDATE(), DATEADD(YEAR, 5, GETDATE()), 0, 0, 10, 'Cskh@123', N'Chăm sóc khách hàng', 1, 1, 0, 1, 0, 0, 1, 1);
END
ELSE
BEGIN
    UPDATE Readers SET Role = N'Chăm sóc khách hàng', IsActive = 1, EmailConfirmed = 1, IsVerified = 1 WHERE Email = 'cskh@library.com';
END
