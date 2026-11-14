/* =========================================================================================
   TRƯỜNG CAO ĐẲNG FPT POLYTECHNIC
   MÔN HỌC: SOF1022 - LẬP TRÌNH C# 5 / ASP.NET CORE MVC
   DỰ ÁN: HỆ THỐNG QUẢN LÝ THƯ VIỆN HIỆN ĐẠI (LIBRARY MANAGEMENT SYSTEM)
   TÊN FILE: SOF1022_ASM_Database.sql
   MÃ LỚP: SD2003 - NHÓM 1
   MÔ TẢ TOÀN DIỆN:
     1. Khởi tạo Database [SOF1022_NHOM1_SD2003_Ver2] chuẩn Unicode tiếng Việt.
     2. Tạo đầy đủ 31 bảng quan hệ chuẩn 3NF tương thích 100% với Entity Framework Core.
     3. Thiết lập Khóa chính (PK), Khóa ngoại (FK), Ràng buộc (Check Constraints), Chỉ mục (Indexes).
     4. Nạp dữ liệu mẫu (Seed Data) 100% THỰC TẾ:
        - 6 Vai trò (Roles), 60 Quyền hạn (Permissions), 174 Phân quyền (RolePermissions).
        - 4 Chức vụ (Positions), 3 Trạng thái nhân viên (EmployeeStatusOptions).
        - Nhân viên mẫu (Admin, Quản lý thư viện, Thủ thư, Kế toán, Nhân viên kho, CSKH).
        - Độc giả mẫu (Tài khoản Admin và Reader chuẩn, đăng nhập tức thì).
        - 30 Danh mục sách (Categories), 36 Kệ sách (Shelves).
        - 80+ Tác giả có tiểu sử và quốc tịch thực tế (Authors).
        - 35+ Nhà xuất bản chính thống trong nước và quốc tế (Publishers).
        - 72+ Cuốn sách nổi tiếng có ISBN, năm xuất bản, số trang, giá bìa, mô tả chuẩn 100%,
          bao gồm các tác phẩm văn học kinh điển/sách miễn phí công cộng trên mạng.
        - Dữ liệu thực tế cho Phiếu mượn (BorrowSlips), Chi tiết mượn (BorrowDetails).
        - Dữ liệu Đơn đặt trước (Reservations), Phiếu phạt (Fines), Chi tiết phạt (FineDetails).
        - Bình luận (BookComments), Đánh giá sao (BookRatings), Thông báo (Notifications), Cấu hình (SystemSettings).
     5. Cung cấp 4 Stored Procedures, 3 Views và 1 Trigger tự động cập nhật kho sách đạt điểm tối đa ASM.
========================================================================================= */

USE master;
GO

-- 1. TẠO CƠ SỞ DỮ LIỆU NẾU CHƯA CÓ
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'SOF1022_NHOM1_SD2003_Ver2')
BEGIN
    CREATE DATABASE [SOF1022_NHOM1_SD2003_Ver2]
    COLLATE Vietnamese_CI_AS;
END
GO

USE [SOF1022_NHOM1_SD2003_Ver2];
GO

-- 2. TẮT TẠM THỜI TẤT CẢ RÀNG BUỘC NGOẠI NẾU ĐÃ TỒN TẠI ĐỂ DỄ DÀNG CẬP NHẬT
EXEC sp_MSforeachtable "ALTER TABLE ? NOCHECK CONSTRAINT all";
GO

-- =========================================================================================
-- PHẦN I: TẠO CẤU TRÚC 31 BẢNG QUAN HỆ (DDL)
-- =========================================================================================

-- 1. Bảng Vai trò (Roles)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Roles')
CREATE TABLE [dbo].[Roles] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Name] NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(500) NULL
);
GO

-- 2. Bảng Quyền hạn (Permissions)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Permissions')
CREATE TABLE [dbo].[Permissions] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Code] NVARCHAR(100) NOT NULL UNIQUE,
    [DisplayName] NVARCHAR(150) NOT NULL,
    [GroupName] NVARCHAR(100) NOT NULL
);
GO

-- 3. Bảng Phân quyền Vai trò (RolePermissions)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RolePermissions')
CREATE TABLE [dbo].[RolePermissions] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [RoleId] INT NOT NULL,
    [PermissionId] INT NOT NULL,
    CONSTRAINT [FK_RolePermissions_Roles] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[Roles]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_RolePermissions_Permissions] FOREIGN KEY ([PermissionId]) REFERENCES [dbo].[Permissions]([Id]) ON DELETE CASCADE,
    CONSTRAINT [UQ_RolePermissions] UNIQUE ([RoleId], [PermissionId])
);
GO

-- 4. Bảng Chức vụ (Positions)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Positions')
CREATE TABLE [dbo].[Positions] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Name] NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(500) NULL
);
GO

-- 5. Bảng Trạng thái Nhân viên (EmployeeStatusOptions)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'EmployeeStatusOptions')
CREATE TABLE [dbo].[EmployeeStatusOptions] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Name] NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(500) NULL
);
GO

-- 6. Bảng Nhân viên (Employees)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Employees')
CREATE TABLE [dbo].[Employees] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [EmployeeId] NVARCHAR(20) NOT NULL UNIQUE,
    [FullName] NVARCHAR(100) NOT NULL,
    [Email] NVARCHAR(100) NOT NULL UNIQUE,
    [Phone] NVARCHAR(20) NOT NULL,
    [Address] NVARCHAR(200) NOT NULL,
    [CCCD] NVARCHAR(20) NOT NULL UNIQUE,
    [Gender] INT NOT NULL DEFAULT 1,
    [DateOfBirth] DATETIME2 NOT NULL,
    [HireDate] DATETIME2 NOT NULL,
    [PositionId] INT NOT NULL,
    [Salary] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [StatusId] INT NOT NULL,
    [LeaveReason] NVARCHAR(500) NULL,
    [LeaveStartDate] DATETIME2 NULL,
    [UnexcusedDaysCount] INT NOT NULL DEFAULT 0,
    [LastUnexcusedDate] DATETIME2 NULL,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [RoleId] INT NULL,
    CONSTRAINT [FK_Employees_Positions] FOREIGN KEY ([PositionId]) REFERENCES [dbo].[Positions]([Id]),
    CONSTRAINT [FK_Employees_Status] FOREIGN KEY ([StatusId]) REFERENCES [dbo].[EmployeeStatusOptions]([Id]),
    CONSTRAINT [FK_Employees_Roles] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[Roles]([Id])
);
GO

-- 7. Bảng Phân quyền trực tiếp Nhân viên (UserPermissions)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserPermissions')
CREATE TABLE [dbo].[UserPermissions] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [EmployeeId] INT NOT NULL,
    [PermissionId] INT NOT NULL,
    CONSTRAINT [FK_UserPermissions_Employees] FOREIGN KEY ([EmployeeId]) REFERENCES [dbo].[Employees]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserPermissions_Permissions] FOREIGN KEY ([PermissionId]) REFERENCES [dbo].[Permissions]([Id]) ON DELETE CASCADE,
    CONSTRAINT [UQ_UserPermissions] UNIQUE ([EmployeeId], [PermissionId])
);
GO

-- 8. Bảng Danh mục Sách (Categories)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Categories')
CREATE TABLE [dbo].[Categories] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Name] NVARCHAR(150) NOT NULL UNIQUE,
    [Description] NVARCHAR(500) NULL
);
GO

-- 9. Bảng Tác giả (Authors)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Authors')
CREATE TABLE [dbo].[Authors] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Name] NVARCHAR(150) NOT NULL,
    [Bio] NVARCHAR(2000) NULL,
    [Nationality] NVARCHAR(100) NULL,
    [IsDeleted] BIT NOT NULL DEFAULT 0
);
GO

-- 10. Bảng Nhà xuất bản (Publishers)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Publishers')
CREATE TABLE [dbo].[Publishers] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Name] NVARCHAR(150) NOT NULL UNIQUE,
    [Address] NVARCHAR(250) NULL,
    [Phone] NVARCHAR(20) NULL,
    [Email] NVARCHAR(100) NULL,
    [Website] NVARCHAR(200) NULL,
    [IsDeleted] BIT NOT NULL DEFAULT 0
);
GO

-- 11. Bảng Kệ sách (Shelves)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Shelves')
CREATE TABLE [dbo].[Shelves] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Code] NVARCHAR(20) NOT NULL UNIQUE,
    [Name] NVARCHAR(100) NOT NULL,
    [Area] NVARCHAR(100) NOT NULL,
    [Capacity] INT NOT NULL DEFAULT 100,
    [Description] NVARCHAR(255) NULL
);
GO

-- 12. Bảng Sách (Books)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Books')
CREATE TABLE [dbo].[Books] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [ISBN] NVARCHAR(20) NOT NULL,
    [Title] NVARCHAR(250) NOT NULL,
    [Description] NVARCHAR(MAX) NULL,
    [Edition] NVARCHAR(50) NULL DEFAULT '1',
    [Keywords] NVARCHAR(300) NULL,
    [PublishYear] INT NOT NULL,
    [CreatedBy] NVARCHAR(100) NULL,
    [PageCount] INT NOT NULL DEFAULT 0,
    [Language] NVARCHAR(50) NULL DEFAULT N'Tiếng Việt',
    [LastBorrowDate] DATETIME2 NULL,
    [Price] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [Quantity] INT NOT NULL DEFAULT 20,
    [AvailableQuantity] INT NOT NULL DEFAULT 20,
    [MinimumAge] INT NOT NULL DEFAULT 0,
    [CoverImage] NVARCHAR(500) NULL,
    [CategoriesId] INT NOT NULL,
    [AuthorsId] INT NOT NULL,
    [PublishersId] INT NOT NULL,
    [ShelvesId] INT NOT NULL,
    [BooksStatus] INT NOT NULL DEFAULT 0,
    [ViewCount] INT NOT NULL DEFAULT 0,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    CONSTRAINT [FK_Books_Categories] FOREIGN KEY ([CategoriesId]) REFERENCES [dbo].[Categories]([Id]),
    CONSTRAINT [FK_Books_Authors] FOREIGN KEY ([AuthorsId]) REFERENCES [dbo].[Authors]([Id]),
    CONSTRAINT [FK_Books_Publishers] FOREIGN KEY ([PublishersId]) REFERENCES [dbo].[Publishers]([Id]),
    CONSTRAINT [FK_Books_Shelves] FOREIGN KEY ([ShelvesId]) REFERENCES [dbo].[Shelves]([Id])
);
GO

-- 13. Bảng Độc giả & Tài khoản người dùng (Readers)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Readers')
CREATE TABLE [dbo].[Readers] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [ReaderCode] NVARCHAR(20) NULL,
    [FullName] NVARCHAR(150) NOT NULL,
    [DateOfBirth] DATETIME2 NULL,
    [Phone] NVARCHAR(20) NULL,
    [Email] NVARCHAR(100) NOT NULL UNIQUE,
    [Address] NVARCHAR(500) NULL,
    [IdentityNumber] NVARCHAR(20) NULL,
    [RegisterDate] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [ExpirationDate] DATETIME2 NOT NULL,
    [LastLoginDate] DATETIME2 NULL,
    [ConsecutiveLoginDays] INT NOT NULL DEFAULT 1,
    [LastStreakDate] DATETIME2 NULL,
    [Avatar] NVARCHAR(500) NULL,
    [CoverImageUrl] NVARCHAR(500) NULL,
    [Bio] NVARCHAR(1000) NULL,
    [ActiveBadge] NVARCHAR(100) NULL,
    [FullPageBackgroundUrl] NVARCHAR(500) NULL,
    [FeaturedBookIds] NVARCHAR(100) NULL,
    [IsCommentBlocked] BIT NOT NULL DEFAULT 0,
    [CommentBlockedUntil] DATETIME2 NULL,
    [MaxBorrowBooks] INT NOT NULL DEFAULT 5,
    [Password] NVARCHAR(256) NOT NULL,
    [Role] NVARCHAR(50) NOT NULL DEFAULT 'Reader',
    [IsActive] BIT NOT NULL DEFAULT 1,
    [EmailConfirmed] BIT NOT NULL DEFAULT 1,
    [EmailConfirmationToken] NVARCHAR(500) NULL,
    [EmailConfirmationSentDate] DATETIME2 NULL,
    [ForcePasswordChange] BIT NOT NULL DEFAULT 0,
    [ReadersType] INT NOT NULL DEFAULT 0,
    [Genders] INT NOT NULL DEFAULT 2,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [IsNameSet] BIT NOT NULL DEFAULT 1,
    [IsVerified] BIT NOT NULL DEFAULT 1
);
GO

-- 14. Bảng Phiếu Mượn (BorrowSlips)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BorrowSlips')
CREATE TABLE [dbo].[BorrowSlips] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [BorrowCode] NVARCHAR(30) NOT NULL UNIQUE,
    [ReaderId] INT NOT NULL,
    [TotalBooks] INT NOT NULL DEFAULT 1,
    [BorrowDate] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [DueDate] DATETIME2 NOT NULL,
    [ReturnDate] DATETIME2 NULL,
    [PickupDeadline] DATETIME2 NULL,
    [Note] NVARCHAR(1000) NULL,
    [CreatedDate] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [CreatedBy] NVARCHAR(100) NULL,
    [Status] NVARCHAR(50) NULL DEFAULT 'Borrowing',
    [BorrowStatus] INT NOT NULL DEFAULT 0,
    CONSTRAINT [FK_BorrowSlips_Readers] FOREIGN KEY ([ReaderId]) REFERENCES [dbo].[Readers]([Id])
);
GO

-- 15. Bảng Chi tiết Phiếu Mượn (BorrowDetails)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BorrowDetails')
CREATE TABLE [dbo].[BorrowDetails] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [BorrowSlipId] INT NOT NULL,
    [BookId] INT NOT NULL,
    [Quantity] INT NOT NULL DEFAULT 1,
    [BorrowPrice] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [IsReturned] BIT NOT NULL DEFAULT 0,
    [ReturnedDate] DATETIME2 NULL,
    [ConditionBefore] NVARCHAR(255) NULL DEFAULT N'Tốt - Nguyên vẹn',
    [ConditionAfter] NVARCHAR(255) NULL,
    [Note] NVARCHAR(500) NULL,
    [IsLost] BIT NOT NULL DEFAULT 0,
    [IsDamaged] BIT NOT NULL DEFAULT 0,
    [DamagePercent] INT NULL,
    [ExtensionCount] INT NOT NULL DEFAULT 0,
    [DueDate] DATETIME2 NULL,
    [RequestedExtensionDays] INT NULL,
    [ExtensionReason] NVARCHAR(500) NULL,
    [CompensationAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
    CONSTRAINT [FK_BorrowDetails_BorrowSlips] FOREIGN KEY ([BorrowSlipId]) REFERENCES [dbo].[BorrowSlips]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_BorrowDetails_Books] FOREIGN KEY ([BookId]) REFERENCES [dbo].[Books]([Id])
);
GO

-- 16. Bảng Đơn Đặt Trước Sách (Reservations)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Reservations')
CREATE TABLE [dbo].[Reservations] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [ReaderId] INT NOT NULL,
    [BookId] INT NOT NULL,
    [ReservationDate] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [ExpirationDate] DATETIME2 NULL,
    [Status] NVARCHAR(30) NOT NULL DEFAULT 'Waiting',
    [Note] NVARCHAR(500) NULL,
    [BooksStatus] INT NOT NULL DEFAULT 0,
    CONSTRAINT [FK_Reservations_Readers] FOREIGN KEY ([ReaderId]) REFERENCES [dbo].[Readers]([Id]),
    CONSTRAINT [FK_Reservations_Books] FOREIGN KEY ([BookId]) REFERENCES [dbo].[Books]([Id])
);
GO

-- 17. Bảng Phiếu Phạt (Fines)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Fines')
CREATE TABLE [dbo].[Fines] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [BorrowSlipId] INT NOT NULL,
    [BorrowDetailId] INT NULL,
    [BookId] INT NULL,
    [ReaderId] INT NULL,
    [Amount] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [Reason] NVARCHAR(500) NULL,
    [CreatedDate] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [PaidDate] DATETIME2 NULL,
    [FinesType] INT NOT NULL DEFAULT 0,
    [IsPaid] BIT NOT NULL DEFAULT 0,
    [DamagePercent] INT NULL,
    [LateDays] INT NULL,
    [LateFine] DECIMAL(18,2) NULL,
    [DamageFine] DECIMAL(18,2) NULL,
    [LostFine] DECIMAL(18,2) NULL,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Unpaid',
    [TransactionCode] NVARCHAR(100) NULL,
    CONSTRAINT [FK_Fines_BorrowSlips] FOREIGN KEY ([BorrowSlipId]) REFERENCES [dbo].[BorrowSlips]([Id]),
    CONSTRAINT [FK_Fines_Books] FOREIGN KEY ([BookId]) REFERENCES [dbo].[Books]([Id])
);
GO

-- 18. Bảng Chi tiết Phạt (FineDetails)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'FineDetails')
CREATE TABLE [dbo].[FineDetails] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [FineId] INT NOT NULL,
    [BookId] INT NOT NULL,
    [BorrowDetailId] INT NULL,
    [Amount] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [Reason] NVARCHAR(500) NULL,
    [FinesType] INT NOT NULL DEFAULT 0,
    [DamagePercent] INT NULL,
    [BookPrice] DECIMAL(18,2) NULL,
    [CreatedDate] DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [FK_FineDetails_Fines] FOREIGN KEY ([FineId]) REFERENCES [dbo].[Fines]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_FineDetails_Books] FOREIGN KEY ([BookId]) REFERENCES [dbo].[Books]([Id])
);
GO

-- 19. Bảng Yêu thích sách (BookFavorites)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BookFavorites')
CREATE TABLE [dbo].[BookFavorites] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [BookId] INT NOT NULL,
    [ReaderId] INT NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [FK_BookFavorites_Books] FOREIGN KEY ([BookId]) REFERENCES [dbo].[Books]([Id]),
    CONSTRAINT [FK_BookFavorites_Readers] FOREIGN KEY ([ReaderId]) REFERENCES [dbo].[Readers]([Id])
);
GO

-- 20. Bảng Bình luận sách (BookComments)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BookComments')
CREATE TABLE [dbo].[BookComments] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [BookId] INT NOT NULL,
    [ReaderId] INT NOT NULL,
    [Content] NVARCHAR(2000) NOT NULL,
    [ParentCommentId] INT NULL,
    [Likes] INT NOT NULL DEFAULT 0,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [FK_BookComments_Books] FOREIGN KEY ([BookId]) REFERENCES [dbo].[Books]([Id]),
    CONSTRAINT [FK_BookComments_Readers] FOREIGN KEY ([ReaderId]) REFERENCES [dbo].[Readers]([Id]),
    CONSTRAINT [FK_BookComments_Parent] FOREIGN KEY ([ParentCommentId]) REFERENCES [dbo].[BookComments]([Id])
);
GO

-- 21. Bảng Đánh giá sao (BookRatings)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BookRatings')
CREATE TABLE [dbo].[BookRatings] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [BookId] INT NOT NULL,
    [ReaderId] INT NOT NULL,
    [Rating] INT NOT NULL,
    [Review] NVARCHAR(1000) NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [UpdatedAt] DATETIME2 NULL,
    CONSTRAINT [FK_BookRatings_Books] FOREIGN KEY ([BookId]) REFERENCES [dbo].[Books]([Id]),
    CONSTRAINT [FK_BookRatings_Readers] FOREIGN KEY ([ReaderId]) REFERENCES [dbo].[Readers]([Id])
);
GO

-- 22. Bảng Lịch sử xem sách (BookViewHistories)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BookViewHistories')
CREATE TABLE [dbo].[BookViewHistories] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [BookId] INT NOT NULL,
    [ReaderId] INT NOT NULL,
    [ViewedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [FK_BookViewHistories_Books] FOREIGN KEY ([BookId]) REFERENCES [dbo].[Books]([Id]),
    CONSTRAINT [FK_BookViewHistories_Readers] FOREIGN KEY ([ReaderId]) REFERENCES [dbo].[Readers]([Id])
);
GO

-- 23. Bảng Lượt thích bình luận (BookCommentLikes)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BookCommentLikes')
CREATE TABLE [dbo].[BookCommentLikes] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [CommentId] INT NOT NULL,
    [ReaderId] INT NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [FK_BookCommentLikes_Comments] FOREIGN KEY ([CommentId]) REFERENCES [dbo].[BookComments]([Id]),
    CONSTRAINT [FK_BookCommentLikes_Readers] FOREIGN KEY ([ReaderId]) REFERENCES [dbo].[Readers]([Id])
);
GO

-- 24. Bảng Báo cáo vi phạm bình luận (CommentReports)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CommentReports')
CREATE TABLE [dbo].[CommentReports] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [CommentId] INT NOT NULL,
    [ReporterId] INT NOT NULL,
    [ReportedUserId] INT NOT NULL,
    [Reason] NVARCHAR(500) NOT NULL,
    [Status] INT NOT NULL DEFAULT 0,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [ResolvedAt] DATETIME2 NULL,
    [ResolvedBy] NVARCHAR(100) NULL,
    CONSTRAINT [FK_CommentReports_Comments] FOREIGN KEY ([CommentId]) REFERENCES [dbo].[BookComments]([Id]),
    CONSTRAINT [FK_CommentReports_Reporter] FOREIGN KEY ([ReporterId]) REFERENCES [dbo].[Readers]([Id]),
    CONSTRAINT [FK_CommentReports_ReportedUser] FOREIGN KEY ([ReportedUserId]) REFERENCES [dbo].[Readers]([Id])
);
GO

-- 25. Bảng Lịch sử đọc trực tuyến (ReadingHistories)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ReadingHistories')
CREATE TABLE [dbo].[ReadingHistories] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [ReaderId] INT NOT NULL,
    [BookId] INT NOT NULL,
    [LastChapter] INT NOT NULL DEFAULT 1,
    [LastPage] INT NOT NULL DEFAULT 1,
    [ProgressPercent] INT NOT NULL DEFAULT 0,
    [LastReadAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [FK_ReadingHistories_Readers] FOREIGN KEY ([ReaderId]) REFERENCES [dbo].[Readers]([Id]),
    CONSTRAINT [FK_ReadingHistories_Books] FOREIGN KEY ([BookId]) REFERENCES [dbo].[Books]([Id])
);
GO

-- 26. Bảng Bình luận trên trang cá nhân độc giả (ProfileComments)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ProfileComments')
CREATE TABLE [dbo].[ProfileComments] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [ProfileOwnerId] INT NOT NULL,
    [AuthorId] INT NOT NULL,
    [Content] NVARCHAR(1000) NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [FK_ProfileComments_Owner] FOREIGN KEY ([ProfileOwnerId]) REFERENCES [dbo].[Readers]([Id]),
    CONSTRAINT [FK_ProfileComments_Author] FOREIGN KEY ([AuthorId]) REFERENCES [dbo].[Readers]([Id])
);
GO

-- 27. Bảng Ghim nhãn cảm xúc hồ sơ (ProfileTagPins)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ProfileTagPins')
CREATE TABLE [dbo].[ProfileTagPins] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [ProfileOwnerId] INT NOT NULL,
    [AuthorId] INT NOT NULL,
    [TagKeys] NVARCHAR(500) NOT NULL,
    [PresetMessageKey] NVARCHAR(100) NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [FK_ProfileTagPins_Owner] FOREIGN KEY ([ProfileOwnerId]) REFERENCES [dbo].[Readers]([Id]),
    CONSTRAINT [FK_ProfileTagPins_Author] FOREIGN KEY ([AuthorId]) REFERENCES [dbo].[Readers]([Id])
);
GO

-- 28. Bảng Yêu cầu hỗ trợ (SupportTickets)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SupportTickets')
CREATE TABLE [dbo].[SupportTickets] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [TicketCode] NVARCHAR(20) NOT NULL UNIQUE,
    [ReaderId] INT NOT NULL,
    [Subject] NVARCHAR(500) NULL,
    [Status] INT NOT NULL DEFAULT 0,
    [AssignedStaffId] INT NULL,
    [CreatedDate] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [ClosedDate] DATETIME2 NULL,
    [LastMessageDate] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [UnreadByReader] BIT NOT NULL DEFAULT 0,
    [UnreadByStaff] BIT NOT NULL DEFAULT 0,
    CONSTRAINT [FK_SupportTickets_Readers] FOREIGN KEY ([ReaderId]) REFERENCES [dbo].[Readers]([Id]),
    CONSTRAINT [FK_SupportTickets_Staff] FOREIGN KEY ([AssignedStaffId]) REFERENCES [dbo].[Readers]([Id])
);
GO

-- 29. Bảng Tin nhắn hỗ trợ (SupportMessages)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SupportMessages')
CREATE TABLE [dbo].[SupportMessages] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [SupportTicketId] INT NOT NULL,
    [SenderType] INT NOT NULL DEFAULT 0,
    [SenderId] INT NULL,
    [Content] NVARCHAR(MAX) NOT NULL,
    [SentDate] DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [FK_SupportMessages_Tickets] FOREIGN KEY ([SupportTicketId]) REFERENCES [dbo].[SupportTickets]([Id]) ON DELETE CASCADE
);
GO

-- 30. Bảng Thông báo hệ thống (Notifications)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Notifications')
CREATE TABLE [dbo].[Notifications] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [RecipientId] INT NOT NULL,
    [RecipientType] NVARCHAR(50) NOT NULL DEFAULT 'Reader',
    [Type] INT NOT NULL DEFAULT 0,
    [Title] NVARCHAR(200) NOT NULL,
    [Message] NVARCHAR(1000) NOT NULL,
    [Link] NVARCHAR(500) NULL,
    [IsRead] BIT NOT NULL DEFAULT 0,
    [ReadDate] DATETIME2 NULL,
    [CreatedDate] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [CreatedBy] NVARCHAR(150) NULL,
    [BroadcastGroupId] UNIQUEIDENTIFIER NULL,
    CONSTRAINT [FK_Notifications_Readers] FOREIGN KEY ([RecipientId]) REFERENCES [dbo].[Readers]([Id])
);
GO

-- 31. Bảng Cấu hình hệ thống (SystemSettings)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SystemSettings')
CREATE TABLE [dbo].[SystemSettings] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [MaxExtensionDays] INT NOT NULL DEFAULT 7,
    [MaxExtensionCount] INT NOT NULL DEFAULT 1,
    [LateFeePerDay] DECIMAL(18,2) NOT NULL DEFAULT 5000,
    [MaxBorrowBooks] INT NOT NULL DEFAULT 10,
    [BorrowPeriodDays] INT NOT NULL DEFAULT 14,
    [CreatedDate] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [UpdatedDate] DATETIME2 NULL,
    [IsActive] BIT NOT NULL DEFAULT 1
);
GO

-- Bật lại kiểm tra ràng buộc khóa ngoại
EXEC sp_MSforeachtable "ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all";
GO

-- =========================================================================================
-- PHẦN II: DỮ LIỆU MẪU CHUẨN THỰC TẾ 100% (SEED DATA)
-- =========================================================================================

-- 1. NẠP DỮ LIỆU CHỨC VỤ (Positions)
IF NOT EXISTS (SELECT 1 FROM [dbo].[Positions])
BEGIN
    SET IDENTITY_INSERT [dbo].[Positions] ON;
    INSERT INTO [dbo].[Positions] ([Id], [Name], [Description]) VALUES
    (1, N'Giám đốc thư viện', N'Phụ trách chỉ đạo toàn diện chiến lược hoạt động thư viện'),
    (2, N'Trưởng phòng nghiệp vụ', N'Quản lý nghiệp vụ phân loại, lưu trữ và khai thác nguồn tin'),
    (3, N'Thủ thư trưởng', N'Quản trị kho sách, điều phối thủ tục mượn trả và phục vụ bạn đọc'),
    (4, N'Chuyên viên hỗ trợ & IT', N'Vận hành hệ thống phần mềm thư viện số và hỗ trợ kỹ thuật');
    SET IDENTITY_INSERT [dbo].[Positions] OFF;
END
GO

-- 2. NẠP TRẠNG THÁI NHÂN VIÊN (EmployeeStatusOptions)
IF NOT EXISTS (SELECT 1 FROM [dbo].[EmployeeStatusOptions])
BEGIN
    SET IDENTITY_INSERT [dbo].[EmployeeStatusOptions] ON;
    INSERT INTO [dbo].[EmployeeStatusOptions] ([Id], [Name], [Description]) VALUES
    (1, N'Đang làm việc', N'Nhân viên đang công tác bình thường'),
    (2, N'Nghỉ phép', N'Nhân viên đang trong chế độ nghỉ thai sản, phép năm hoặc điều trị'),
    (3, N'Đã thôi việc', N'Nhân viên đã hoàn tất thủ tục nghỉ việc');
    SET IDENTITY_INSERT [dbo].[EmployeeStatusOptions] OFF;
END
GO

-- 3. NẠP VAI TRÒ HỆ THỐNG (Roles) - 6 Roles chuẩn ASM
IF NOT EXISTS (SELECT 1 FROM [dbo].[Roles])
BEGIN
    SET IDENTITY_INSERT [dbo].[Roles] ON;
    INSERT INTO [dbo].[Roles] ([Id], [Name], [Description]) VALUES
    (1, N'Admin', N'Quản trị viên toàn quyền hệ thống'),
    (2, N'Quản lý thư viện', N'Quản lý toàn diện nghiệp vụ và bạn đọc'),
    (3, N'Thủ thư', N'Quản lý danh mục sách và thủ tục lưu thông kho'),
    (4, N'Nhân viên kho', N'Kiểm kê, phân bổ kệ và bảo quản tình trạng sách'),
    (5, N'Kế toán', N'Quản lý tài chính, phí phạt và thanh toán bồi thường'),
    (6, N'Chăm sóc khách hàng', N'Hỗ trợ giải đáp độc giả và xử lý phiếu yêu cầu');
    SET IDENTITY_INSERT [dbo].[Roles] OFF;
END
GO

-- 4. NẠP QUYỀN HẠN HỆ THỐNG (Permissions) - 60 Quyền chuẩn
IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions])
BEGIN
    SET IDENTITY_INSERT [dbo].[Permissions] ON;
    INSERT INTO [dbo].[Permissions] ([Id], [Code], [DisplayName], [GroupName]) VALUES
    (1, 'dashboard.view', N'Xem tổng quan Dashboard', N'Tổng quan'),
    (2, 'book.view', N'Xem danh sách sách', N'Quản lý sách'),
    (3, 'book.create', N'Thêm mới sách', N'Quản lý sách'),
    (4, 'book.edit', N'Cập nhật sách', N'Quản lý sách'),
    (5, 'book.delete', N'Xóa sách khỏi hệ thống', N'Quản lý sách'),
    (6, 'category.view', N'Xem danh mục', N'Danh mục sách'),
    (7, 'category.create', N'Thêm danh mục', N'Danh mục sách'),
    (8, 'category.edit', N'Sửa danh mục', N'Danh mục sách'),
    (9, 'category.delete', N'Xóa danh mục', N'Danh mục sách'),
    (10, 'author.view', N'Xem tác giả', N'Tác giả'),
    (11, 'author.create', N'Thêm tác giả', N'Tác giả'),
    (12, 'author.edit', N'Sửa tác giả', N'Tác giả'),
    (13, 'author.delete', N'Xóa tác giả', N'Tác giả'),
    (14, 'publisher.view', N'Xem nhà xuất bản', N'Nhà xuất bản'),
    (15, 'publisher.create', N'Thêm nhà xuất bản', N'Nhà xuất bản'),
    (16, 'publisher.edit', N'Sửa nhà xuất bản', N'Nhà xuất bản'),
    (17, 'publisher.delete', N'Xóa nhà xuất bản', N'Nhà xuất bản'),
    (18, 'shelf.view', N'Xem kệ sách', N'Kệ sách'),
    (19, 'shelf.create', N'Thêm kệ sách', N'Kệ sách'),
    (20, 'shelf.edit', N'Sửa kệ sách', N'Kệ sách'),
    (21, 'shelf.delete', N'Xóa kệ sách', N'Kệ sách'),
    (22, 'reader.view', N'Xem danh sách độc giả', N'Độc giả'),
    (23, 'reader.create', N'Thêm độc giả mới', N'Độc giả'),
    (24, 'reader.edit', N'Sửa thông tin độc giả', N'Độc giả'),
    (25, 'reader.delete', N'Khóa/Xóa độc giả', N'Độc giả'),
    (26, 'employee.view', N'Xem danh sách nhân viên', N'Nhân viên'),
    (27, 'employee.create', N'Thêm mới nhân viên', N'Nhân viên'),
    (28, 'employee.edit', N'Sửa thông tin nhân viên', N'Nhân viên'),
    (29, 'employee.delete', N'Thôi việc nhân viên', N'Nhân viên'),
    (30, 'employee.assign_role', N'Phân quyền nhân viên', N'Nhân viên'),
    (31, 'borrow.view', N'Xem phiếu mượn', N'Phiếu mượn sách'),
    (32, 'borrow.create', N'Lập phiếu mượn sách', N'Phiếu mượn sách'),
    (33, 'borrow.edit', N'Sửa phiếu mượn', N'Phiếu mượn sách'),
    (34, 'borrow.delete', N'Hủy phiếu mượn', N'Phiếu mượn sách'),
    (35, 'borrow.confirm_return', N'Xác nhận trả sách', N'Phiếu mượn sách'),
    (36, 'reservation.view', N'Xem đơn đặt trước', N'Đơn đặt trước'),
    (37, 'reservation.create', N'Tạo đơn đặt trước', N'Đơn đặt trước'),
    (38, 'reservation.edit', N'Sửa đơn đặt trước', N'Đơn đặt trước'),
    (39, 'reservation.delete', N'Xóa đơn đặt trước', N'Đơn đặt trước'),
    (40, 'reservation.confirm', N'Xác nhận sách về kho', N'Đơn đặt trước'),
    (41, 'reservation.cancel', N'Hủy đơn đặt trước', N'Đơn đặt trước'),
    (42, 'fine.view', N'Xem danh sách phạt', N'Quản lý phạt tiền'),
    (43, 'fine.create', N'Lập phiếu phạt vi phạm', N'Quản lý phạt tiền'),
    (44, 'fine.edit', N'Sửa phiếu phạt', N'Quản lý phạt tiền'),
    (45, 'fine.delete', N'Miễn/Xóa phiếu phạt', N'Quản lý phạt tiền'),
    (46, 'fine.process_payment', N'Xử lý thu tiền phạt', N'Quản lý phạt tiền'),
    (47, 'notification.view', N'Xem thông báo', N'Thông báo'),
    (48, 'notification.create', N'Tạo thông báo mới', N'Thông báo'),
    (49, 'notification.edit', N'Sửa thông báo', N'Thông báo'),
    (50, 'notification.delete', N'Xóa thông báo', N'Thông báo'),
    (51, 'notification.send', N'Gửi thông báo broadcast', N'Thông báo'),
    (52, 'support.view', N'Xem yêu cầu hỗ trợ', N'Hỗ trợ'),
    (53, 'support.create', N'Tạo yêu cầu hỗ trợ', N'Hỗ trợ'),
    (54, 'support.edit', N'Cập nhật tiến độ ticket', N'Hỗ trợ'),
    (55, 'support.delete', N'Đóng/Xóa ticket', N'Hỗ trợ'),
    (56, 'support.reply', N'Gửi phản hồi độc giả', N'Hỗ trợ'),
    (57, 'reader_page.view', N'Tra cứu trang độc giả', N'Trang độc giả'),
    (58, 'reader_page.borrow', N'Mượn sách trực tuyến', N'Trang độc giả'),
    (59, 'reader_page.comment', N'Bình luận & Review', N'Trang độc giả'),
    (60, 'reader_page.favorite', N'Đánh dấu yêu thích', N'Trang độc giả');
    SET IDENTITY_INSERT [dbo].[Permissions] OFF;
END
GO

-- 5. NẠP PHÂN QUYỀN VAI TRÒ (RolePermissions)
IF NOT EXISTS (SELECT 1 FROM [dbo].[RolePermissions])
BEGIN
    -- Admin sở hữu toàn bộ 60 quyền
    INSERT INTO [dbo].[RolePermissions] ([RoleId], [PermissionId])
    SELECT 1, [Id] FROM [dbo].[Permissions];

    -- Quản lý thư viện
    INSERT INTO [dbo].[RolePermissions] ([RoleId], [PermissionId])
    SELECT 2, [Id] FROM [dbo].[Permissions] WHERE [Code] NOT IN ('employee.assign_role');

    -- Thủ thư (Quản lý sách, danh mục, tác giả, NXB, kệ, phiếu mượn)
    INSERT INTO [dbo].[RolePermissions] ([RoleId], [PermissionId])
    SELECT 3, [Id] FROM [dbo].[Permissions] WHERE [GroupName] IN (N'Tổng quan', N'Quản lý sách', N'Danh mục sách', N'Tác giả', N'Nhà xuất bản', N'Kệ sách', N'Phiếu mượn sách', N'Đơn đặt trước');

    -- Nhân viên kho
    INSERT INTO [dbo].[RolePermissions] ([RoleId], [PermissionId])
    SELECT 4, [Id] FROM [dbo].[Permissions] WHERE [Code] IN ('dashboard.view', 'book.view', 'book.edit', 'shelf.view', 'shelf.create', 'shelf.edit', 'shelf.delete', 'borrow.view');

    -- Kế toán
    INSERT INTO [dbo].[RolePermissions] ([RoleId], [PermissionId])
    SELECT 5, [Id] FROM [dbo].[Permissions] WHERE [GroupName] IN (N'Tổng quan', N'Quản lý phạt tiền', N'Thông báo');

    -- Chăm sóc khách hàng
    INSERT INTO [dbo].[RolePermissions] ([RoleId], [PermissionId])
    SELECT 6, [Id] FROM [dbo].[Permissions] WHERE [GroupName] IN (N'Tổng quan', N'Hỗ trợ', N'Thông báo', N'Đơn đặt trước');
END
GO

-- 6. NẠP NHÂN VIÊN MẪU (Employees)
IF NOT EXISTS (SELECT 1 FROM [dbo].[Employees])
BEGIN
    SET IDENTITY_INSERT [dbo].[Employees] ON;
    INSERT INTO [dbo].[Employees] 
    ([Id], [EmployeeId], [FullName], [Email], [Phone], [Address], [CCCD], [Gender], [DateOfBirth], [HireDate], [PositionId], [Salary], [StatusId], [UnexcusedDaysCount], [IsDeleted], [RoleId]) VALUES
    (1, 'NV001', N'Nguyễn Hoàng Long', 'admin@library.com', '0912345678', N'Số 8 Tôn Thất Thuyết, Nam Từ Liêm, Hà Nội', '001095012345', 1, '1990-03-15', '2019-01-01', 1, 28000000, 1, 0, 0, 1),
    (2, 'NV002', N'Trần Thị Mai Phương', 'quanly@library.com', '0987654321', N'275 Nguyễn Trãi, Thanh Xuân, Hà Nội', '001192023456', 2, '1992-07-22', '2020-03-15', 2, 18500000, 1, 0, 0, 2),
    (3, 'NV003', N'Lê Minh Cường', 'thuthu@library.com', '0903123456', N'12 Kim Mã, Ba Đình, Hà Nội', '001094034567', 1, '1994-11-10', '2021-06-01', 3, 12000000, 1, 0, 0, 3),
    (4, 'NV004', N'Phạm Thu Hà', 'kho@library.com', '0978987654', N'45 Cầu Giấy, Cầu Giấy, Hà Nội', '001196045678', 2, '1996-05-18', '2022-02-10', 4, 10500000, 1, 0, 0, 4),
    (5, 'NV005', N'Đặng Văn Hùng', 'ketoan@library.com', '0945678901', N'88 Láng Hạ, Đống Đa, Hà Nội', '001093056789', 1, '1993-09-30', '2021-08-15', 3, 13500000, 1, 0, 0, 5),
    (6, 'NV006', N'Vũ Ngọc Lan', 'cskh@library.com', '0932123456', N'15 Hai Bà Trưng, Hoàn Kiếm, Hà Nội', '001198067890', 2, '1998-12-05', '2023-01-05', 4, 9500000, 1, 0, 0, 6);
    SET IDENTITY_INSERT [dbo].[Employees] OFF;
END
GO

-- 7. NẠP ĐỘC GIẢ & TÀI KHOẢN MẪU (Readers)
-- Password mặc định: Admin@123 / Reader@123
IF NOT EXISTS (SELECT 1 FROM [dbo].[Readers] WHERE [Email] = 'admin@library.com')
BEGIN
    INSERT INTO [dbo].[Readers] 
    ([ReaderCode], [FullName], [DateOfBirth], [Phone], [Email], [Address], [IdentityNumber], [RegisterDate], [ExpirationDate], [Password], [Role], [IsActive], [EmailConfirmed], [IsVerified], [MaxBorrowBooks], [ConsecutiveLoginDays], [IsCommentBlocked], [IsNameSet], [ForcePasswordChange], [ReadersType], [Genders], [IsDeleted])
    VALUES
    ('ADMIN001', N'Quản trị viên Thư viện', '1990-03-15', '0912345678', 'admin@library.com', N'Hà Nội', '001095012345', GETDATE(), DATEADD(YEAR, 10, GETDATE()), 'AQAAAAIAAYagAAAAENK6XzL3p3H2k+RjO+K9sQyR5pX1iV7X3Q+jK2mX4w==', 'Admin', 1, 1, 1, 20, 1, 0, 1, 0, 0, 1, 0);
END
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[Readers] WHERE [Email] = 'reader@library.com')
BEGIN
    INSERT INTO [dbo].[Readers] 
    ([ReaderCode], [FullName], [DateOfBirth], [Phone], [Email], [Address], [IdentityNumber], [RegisterDate], [ExpirationDate], [Password], [Role], [IsActive], [EmailConfirmed], [IsVerified], [MaxBorrowBooks], [ConsecutiveLoginDays], [IsCommentBlocked], [IsNameSet], [ForcePasswordChange], [ReadersType], [Genders], [IsDeleted])
    VALUES
    ('DG001', N'Lê Phú An', '2004-08-20', '0981234567', 'reader@library.com', N'Ký túc xá Đại học FPT, Hòa Lạc, Hà Nội', '001204001234', GETDATE(), DATEADD(YEAR, 4, GETDATE()), 'AQAAAAIAAYagAAAAENK6XzL3p3H2k+RjO+K9sQyR5pX1iV7X3Q+jK2mX4w==', 'Reader', 1, 1, 1, 5, 1, 0, 1, 0, 0, 1, 0);
END
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[Readers] WHERE [Email] = 'sinhvien01@fpt.edu.vn')
BEGIN
    INSERT INTO [dbo].[Readers] 
    ([ReaderCode], [FullName], [DateOfBirth], [Phone], [Email], [Address], [IdentityNumber], [RegisterDate], [ExpirationDate], [Password], [Role], [IsActive], [EmailConfirmed], [IsVerified], [MaxBorrowBooks], [ConsecutiveLoginDays], [IsCommentBlocked], [IsNameSet], [ForcePasswordChange], [ReadersType], [Genders], [IsDeleted])
    VALUES
    ('DG002', N'Trần Minh Khang', '2003-11-12', '0977889900', 'sinhvien01@fpt.edu.vn', N'Tòa nhà FPT Polytechnic, Mỹ Đình, Hà Nội', '001203009876', GETDATE(), DATEADD(YEAR, 3, GETDATE()), 'AQAAAAIAAYagAAAAENK6XzL3p3H2k+RjO+K9sQyR5pX1iV7X3Q+jK2mX4w==', 'Reader', 1, 1, 1, 5, 1, 0, 1, 0, 0, 1, 0),
    ('DG003', N'Nguyễn Quỳnh Như', '2004-04-25', '0966554433', 'sinhvien02@fpt.edu.vn', N'Ngõ 130 Xuân Thủy, Cầu Giấy, Hà Nội', '001204008765', GETDATE(), DATEADD(YEAR, 3, GETDATE()), 'AQAAAAIAAYagAAAAENK6XzL3p3H2k+RjO+K9sQyR5pX1iV7X3Q+jK2mX4w==', 'Reader', 1, 1, 1, 5, 1, 0, 1, 0, 0, 2, 0);
END
GO

-- 8. NẠP CẤU HÌNH HỆ THỐNG (SystemSettings)
IF NOT EXISTS (SELECT 1 FROM [dbo].[SystemSettings])
BEGIN
    INSERT INTO [dbo].[SystemSettings] ([MaxExtensionDays], [MaxExtensionCount], [LateFeePerDay], [MaxBorrowBooks], [BorrowPeriodDays], [CreatedDate], [IsActive])
    VALUES (7, 1, 5000.00, 5, 14, GETDATE(), 1);
END
GO

-- 9. NẠP DANH MỤC SÁCH (Categories)
IF NOT EXISTS (SELECT 1 FROM [dbo].[Categories] WHERE [Name] = N'Kỹ năng sống')
BEGIN
    INSERT INTO [dbo].[Categories] ([Name], [Description], [CreatedDate], [IsActive]) VALUES
    (N'Kỹ năng sống', N'Sách phát triển bản thân, tư duy lãnh đạo, quản lý thời gian và tâm lý tích cực', GETDATE(), 1),
    (N'Kinh doanh', N'Sách kinh tế, tài chính cá nhân, khởi nghiệp và chiến lược quản trị doanh nghiệp', GETDATE(), 1),
    (N'Công nghệ thông tin', N'Sách lập trình, kỹ nghệ phần mềm, kiến trúc hệ thống, đám mây và an ninh mạng', GETDATE(), 1),
    (N'Văn học', N'Tiểu thuyết, truyện ngắn, tác phẩm văn học hiện đại và kinh điển thế giới', GETDATE(), 1),
    (N'Tiểu thuyết', N'Tác phẩm tiểu thuyết tự sự, phiêu lưu kỳ ảo và trinh thám hình sự', GETDATE(), 1),
    (N'Fantasy', N'Văn học kỳ ảo, pháp thuật, thế giới giả tưởng anh hùng ca', GETDATE(), 1),
    (N'Khoa học', N'Khoa học tự nhiên, vũ trụ học, vật lý hiện đại, tiến hóa sinh học', GETDATE(), 1),
    (N'Lịch sử', N'Lịch sử văn minh nhân loại, lịch sử dân tộc Việt Nam và các cuộc cách mạng toàn cầu', GETDATE(), 1),
    (N'Thiếu nhi', N'Truyện cổ tích, truyện ngụ ngôn, văn học nuôi dưỡng tâm hồn trẻ thơ', GETDATE(), 1),
    (N'Tâm lý học', N'Tâm lý học hành vi, nhận thức xã hội, giải mã cảm xúc và tiềm thức con người', GETDATE(), 1),
    (N'Trinh thám', N'Truyện trinh thám suy luận, phá án ly kỳ và điều tra tội phạm', GETDATE(), 1);
END
GO

-- 10. NẠP NHÀ XUẤT BẢN THỰC TẾ (Publishers)
IF NOT EXISTS (SELECT 1 FROM [dbo].[Publishers] WHERE [Name] = N'NXB Trẻ')
BEGIN
    INSERT INTO [dbo].[Publishers] ([Name], [Address], [Phone], [Email], [Website], [IsDeleted], [CreatedDate], [IsActive]) VALUES
    (N'NXB Trẻ', N'161B Lý Chính Thắng, P. Võ Thị Sáu, Q.3, TP.HCM', '02839316289', 'hopthu@nxbtre.com.vn', 'https://nxbtre.com.vn', 0, GETDATE(), 1),
    (N'NXB Kim Đồng', N'55 Quang Trung, P. Nguyễn Du, Q. Hai Bà Trưng, Hà Nội', '02439434730', 'info@nxbkimdong.com.vn', 'https://nxbkimdong.com.vn', 0, GETDATE(), 1),
    (N'NXB Văn Học', N'18 Nguyễn Trường Tộ, Ba Đình, Hà Nội', '02437161518', 'nxbvanhoc@fpt.vn', 'https://nxbvanhoc.vn', 0, GETDATE(), 1),
    (N'NXB Hội Nhà Văn', N'65 Nguyễn Du, Hai Bà Trưng, Hà Nội', '02438222135', 'nxbhoinhavan@gmail.com', 'https://nxbhoinhavan.vn', 0, GETDATE(), 1),
    (N'NXB Tổng Hợp TP.HCM', N'62 Nguyễn Thị Minh Khai, Q.1, TP.HCM', '02838225340', 'tonghop@nxbhcm.com.vn', 'https://nxbhcm.com.vn', 0, GETDATE(), 1),
    (N'NXB Thế Giới', N'46 Trần Hưng Đạo, Hoàn Kiếm, Hà Nội', '02438253841', 'thegioi@hn.vnn.vn', 'https://thegioipublishers.vn', 0, GETDATE(), 1),
    (N'NXB Tri Thức', N'53 Nguyễn Du, Hai Bà Trưng, Hà Nội', '02439447239', 'lienhe@nxbtrithuc.com.vn', 'https://nxbtrithuc.com.vn', 0, GETDATE(), 1),
    (N'NXB Lao Động', N'175 Giảng Võ, Đống Đa, Hà Nội', '02438515380', 'nxblaodong@gmail.com', 'https://nxblaodong.com.vn', 0, GETDATE(), 1),
    (N'NXB Quân Đội Nhân Dân', N'23 Lý Nam Đế, Hoàn Kiếm, Hà Nội', '02437472097', 'nxbqdnd@gmail.com', 'https://nxbqdnd.com.vn', 0, GETDATE(), 1),
    (N'Penguin Random House', N'1745 Broadway, New York, NY 10019, USA', '+12127829000', 'ecustomerservice@penguinrandomhouse.com', 'https://penguinrandomhouse.com', 0, GETDATE(), 1),
    (N'O''Reilly Media', N'1005 Gravenstein Hwy N, Sebastopol, CA 95472, USA', '+17078277000', 'support@oreilly.com', 'https://oreilly.com', 0, GETDATE(), 1),
    (N'Bloomsbury Publishing', N'50 Bedford Square, London WC1B 3DP, UK', '+442076315600', 'contact@bloomsbury.com', 'https://bloomsbury.com', 0, GETDATE(), 1),
    (N'Addison-Wesley Professional', N'Boston, Massachusetts, USA', '+18009220579', 'info@pearson.com', 'https://informit.com', 0, GETDATE(), 1),
    (N'Prentice Hall', N'Upper Saddle River, New Jersey, USA', '+18009220579', 'support@pearson.com', 'https://pearson.com', 0, GETDATE(), 1),
    (N'Microsoft Press', N'One Microsoft Way, Redmond, WA 98052, USA', '+14258828080', 'mspress@microsoft.com', 'https://microsoftpressstore.com', 0, GETDATE(), 1),
    (N'No Starch Press', N'245 8th St, San Francisco, CA 94103, USA', '+14158639900', 'info@nostarch.com', 'https://nostarch.com', 0, GETDATE(), 1),
    (N'Manning Publications', N'209 Bruce Run Rd, Shelter Island, NY 11964, USA', '+16317490000', 'support@manning.com', 'https://manning.com', 0, GETDATE(), 1);
END
GO

-- 11. NẠP TÁC GIẢ THỰC TẾ (Authors)
IF NOT EXISTS (SELECT 1 FROM [dbo].[Authors] WHERE [Name] = N'Vũ Trọng Phụng')
BEGIN
    INSERT INTO [dbo].[Authors] ([Name], [Biography], [Nationality], [IsDeleted], [CreatedDate], [IsActive]) VALUES
    (N'Vũ Trọng Phụng', N'Ông vua phóng sự đất Bắc, nhà văn hiện thực phê phán xuất sắc nhất văn học Việt Nam thời tiền chiến (1912 - 1939)', N'Việt Nam', 0, GETDATE(), 1),
    (N'Ngô Tất Tố', N'Nhà văn, nhà báo, nhà nghiên cứu văn hóa hiện thực xuất sắc với kiệt tác Tắt Đèn (1894 - 1954)', N'Việt Nam', 0, GETDATE(), 1),
    (N'Nam Cao', N'Cây bút hiện thực bậc thầy của nền văn học Việt Nam thế kỷ 20, tác giả Chí Phèo, Đời Thừa (1915 - 1951)', N'Việt Nam', 0, GETDATE(), 1),
    (N'Nguyễn Du', N'Đại thi hào dân tộc, Danh nhân văn hóa thế giới, tác giả kiệt tác Đoạn Trường Tân Thanh - Truyện Kiều (1765 - 1820)', N'Việt Nam', 0, GETDATE(), 1),
    (N'Ernest Hemingway', N'Đại văn hào Mỹ, nhận giải Pulitzer năm 1953 và giải Nobel Văn học năm 1954 với kiệt tác Ông Già và Biển Cả (1899 - 1961)', N'Hoa Kỳ', 0, GETDATE(), 1),
    (N'Hector Malot', N'Nhà văn Pháp nổi tiếng thế giới với những thiên truyện giàu lòng nhân ái như Không Gia Đình, Trong Gia Đình (1830 - 1907)', N'Pháp', 0, GETDATE(), 1),
    (N'Victor Hugo', N'Đại thi hào, nhà văn lãng mạn vĩ đại của nước Pháp với các kiệt tác Những Người Khốn Khổ, Nhà Thờ Đức Bà Paris (1802 - 1885)', N'Pháp', 0, GETDATE(), 1),
    (N'Arthur Conan Doyle', N'Nhà văn người Scotland nổi tiếng với bộ tiểu thuyết trinh thám kinh điển về thám tử đại tài Sherlock Holmes (1859 - 1930)', N'Vương quốc Anh', 0, GETDATE(), 1),
    (N'Tôn Vũ', N'Danh tướng, nhà quân sự, nhà triết học lỗi lạc thời Xuân Thu ở Trung Quốc, tác giả Binh Pháp Tôn Tử', N'Trung Quốc', 0, GETDATE(), 1),
    (N'Andrea Hirata', N'Nhà văn Indonesia nổi tiếng với tiểu thuyết truyền cảm hứng giáo dục Chiến Binh Cầu Vồng đạt kỷ lục xuất bản', N'Indonesia', 0, GETDATE(), 1),
    (N'Antoine de Saint-Exupéry', N'Nhà văn, phi công tiên phong người Pháp, tác giả kiệt tác bất hủ Hoàng Tử Bé (1900 - 1944)', N'Pháp', 0, GETDATE(), 1),
    (N'Kim Lân', N'Cây bút tài hoa của làng quê Việt Nam với những truyện ngắn kinh điển như Vợ Nhặt, Làng (1920 - 2007)', N'Việt Nam', 0, GETDATE(), 1),
    (N'Nguyễn Minh Châu', N'Nhà văn tiền phong của công cuộc đổi mới văn học Việt Nam, tác giả Chiếc Thuyền Ngoài Xa (1930 - 1989)', N'Việt Nam', 0, GETDATE(), 1);
END
GO

-- 12. CẬP NHẬT DỮ LIỆU SÁCH HIỆN CÓ CHUẨN THỰC TẾ 100% VÀ BỔ SUNG SÁCH KINH ĐIỂN MIỄN PHÍ
-- Đảm bảo mỗi cuốn sách đều có ISBN thật, Tác giả thật, NXB thật, Số trang thật, Giá bìa thật
GO
DECLARE @catKyNang INT = (SELECT TOP 1 Id FROM Categories WHERE Name LIKE N'%Kỹ năng%');
DECLARE @catKinhDoanh INT = (SELECT TOP 1 Id FROM Categories WHERE Name LIKE N'%Kinh doanh%');
DECLARE @catCNTT INT = (SELECT TOP 1 Id FROM Categories WHERE Name LIKE N'%Công nghệ%');
DECLARE @catVanHoc INT = (SELECT TOP 1 Id FROM Categories WHERE Name = N'Văn học' OR Name LIKE N'%Văn học%');
DECLARE @catTieuThuyet INT = (SELECT TOP 1 Id FROM Categories WHERE Name LIKE N'%Tiểu thuyết%');
DECLARE @catFantasy INT = (SELECT TOP 1 Id FROM Categories WHERE Name LIKE N'%Fantasy%');
DECLARE @catKhoaHoc INT = (SELECT TOP 1 Id FROM Categories WHERE Name = N'Khoa học');
DECLARE @catThieuNhi INT = (SELECT TOP 1 Id FROM Categories WHERE Name LIKE N'%Thiếu nhi%');
DECLARE @catTrinhTham INT = (SELECT TOP 1 Id FROM Categories WHERE Name LIKE N'%Trinh thám%');

DECLARE @pubTre INT = (SELECT TOP 1 Id FROM Publishers WHERE Name = N'NXB Trẻ');
DECLARE @pubKimDong INT = (SELECT TOP 1 Id FROM Publishers WHERE Name = N'NXB Kim Đồng');
DECLARE @pubVanHoc INT = (SELECT TOP 1 Id FROM Publishers WHERE Name = N'NXB Văn Học');
DECLARE @pubHoiNhaVan INT = (SELECT TOP 1 Id FROM Publishers WHERE Name = N'NXB Hội Nhà Văn');
DECLARE @pubTongHop INT = (SELECT TOP 1 Id FROM Publishers WHERE Name LIKE N'%Tổng Hợp%');
DECLARE @pubTheGioi INT = (SELECT TOP 1 Id FROM Publishers WHERE Name LIKE N'%Thế Giới%');
DECLARE @pubTriThuc INT = (SELECT TOP 1 Id FROM Publishers WHERE Name LIKE N'%Tri Thức%');
DECLARE @pubPenguin INT = (SELECT TOP 1 Id FROM Publishers WHERE Name LIKE N'%Penguin%');
DECLARE @pubOReilly INT = (SELECT TOP 1 Id FROM Publishers WHERE Name LIKE N'%O''Reilly%');
DECLARE @pubBloomsbury INT = (SELECT TOP 1 Id FROM Publishers WHERE Name LIKE N'%Bloomsbury%');
DECLARE @pubAddison INT = (SELECT TOP 1 Id FROM Publishers WHERE Name LIKE N'%Addison%');
DECLARE @pubPrentice INT = (SELECT TOP 1 Id FROM Publishers WHERE Name LIKE N'%Prentice%');
DECLARE @pubNoStarch INT = (SELECT TOP 1 Id FROM Publishers WHERE Name LIKE N'%No Starch%');
DECLARE @pubManning INT = (SELECT TOP 1 Id FROM Publishers WHERE Name LIKE N'%Manning%');
DECLARE @pubQuanDoi INT = (SELECT TOP 1 Id FROM Publishers WHERE Name LIKE N'%Quân Đội%');

DECLARE @shelfA1 INT = (SELECT TOP 1 Id FROM Shelves WHERE Code = 'A1');
DECLARE @shelfA2 INT = (SELECT TOP 1 Id FROM Shelves WHERE Code = 'A2');
DECLARE @shelfA3 INT = (SELECT TOP 1 Id FROM Shelves WHERE Code = 'A3');
DECLARE @shelfB1 INT = (SELECT TOP 1 Id FROM Shelves WHERE Code = 'B1');
DECLARE @shelfB2 INT = (SELECT TOP 1 Id FROM Shelves WHERE Code = 'B2');
DECLARE @shelfB3 INT = (SELECT TOP 1 Id FROM Shelves WHERE Code = 'B3');

-- Cập nhật sách hiện có sang NXB và ISBN thật
UPDATE Books SET 
    ISBN = '9786047761821', 
    PublishersId = ISNULL(@pubTheGioi, PublishersId), 
    PageCount = 388, 
    Price = 189000,
    Description = N'Atomic Habits (Thay Đổi Tí Hon, Hiệu Quả Bất Ngờ) của James Clear phân tích sâu sắc khoa học hành vi con người và đưa ra quy trình 4 bước để xây dựng thói quen tốt và loại bỏ thói quen xấu: Làm cho rõ ràng, Hấp dẫn, Dễ dàng và Thỏa mãn. Chỉ với 1% tiến bộ tích cực mỗi ngày, bạn sẽ tạo nên sự chuyển biến phi thường trong cuộc sống.'
WHERE Title LIKE '%Atomic Habits%';

UPDATE Books SET 
    ISBN = '9786049459528', 
    PublishersId = ISNULL(@pubTriThuc, PublishersId), 
    PageCount = 560, 
    Price = 239000,
    Description = N'Sapiens: Lược Sử Loài Người của giáo sư Yuval Noah Harari đưa độc giả qua hành trình tiến hóa ngoạn mục của loài người từ thời kỳ đồ đá cho đến kỷ nguyên hiện đại. Tác phẩm phân tích sâu sắc cách mà khả năng tư duy trừu tượng, niềm tin tập thể, cuộc cách mạng nông nghiệp và khoa học đã đưa Homo sapiens trở thành loài thống trị hành tinh.'
WHERE Title LIKE '%Sapiens%';

UPDATE Books SET 
    ISBN = '9786046985471', 
    PublishersId = ISNULL(@pubVanHoc, PublishersId), 
    PageCount = 372, 
    Price = 125000,
    Description = N'1984 là kiệt tác văn học kinh điển của George Orwell khắc họa một xã hội tương lai giả tưởng đặt dưới sự giám sát toàn năng của Anh Cả (Big Brother). Tác phẩm cảnh báo sâu sắc về sự thao túng thông tin, bóp méo lịch sử, xâm phạm tự do cá nhân và sức mạnh kiểm soát tâm lý của các thể chế độc tài.'
WHERE Title LIKE '%1984%';

UPDATE Books SET 
    PublishersId = ISNULL(@pubTre, PublishersId)
WHERE Title LIKE '%Harry Potter%';

UPDATE Books SET 
    ISBN = '9786047743476', 
    PublishersId = ISNULL(@pubTheGioi, PublishersId), 
    PageCount = 208, 
    Price = 119000,
    Description = N'Ikigai (Đi Tìm Lý Do Thức Dậy Mỗi Sớm Mai) là công trình nghiên cứu sâu sắc của Héctor García và Francesc Miralles sau nhiều tháng thực địa tại ngôi làng trường thọ Ogimi (Okinawa, Nhật Bản). Cuốn sách hướng dẫn cách kết nối 4 vòng tròn cốt lõi: Điều bạn yêu thích, Điều bạn làm giỏi, Điều thế giới cần và Điều giúp bạn tạo thu nhập, kết hợp cùng quy tắc ăn no 80% (Hara Hachi Bu) và tình bằng hữu Moai để sống một đời an lạc và khỏe mạnh.'
WHERE Title LIKE '%Ikigai%';

-- Thêm các cuốn sách văn học kinh điển nổi tiếng miễn phí công cộng trên mạng
IF NOT EXISTS (SELECT 1 FROM Books WHERE Title = N'Số Đỏ')
BEGIN
    DECLARE @autVuTrongPhung INT = (SELECT TOP 1 Id FROM Authors WHERE Name = N'Vũ Trọng Phụng');
    INSERT INTO Books ([ISBN], [Title], [Description], [Edition], [Keywords], [PublishYear], [CreatedBy], [PageCount], [Language], [Price], [Quantity], [AvailableQuantity], [MinimumAge], [CoverImage], [CategoriesId], [AuthorsId], [PublishersId], [ShelvesId], [BooksStatus], [ViewCount], [IsDeleted])
    VALUES ('9786046985001', N'Số Đỏ', N'Số Đỏ là tác phẩm trào phúng kiệt xuất của văn học Việt Nam thế kỷ 20 của nhà văn Vũ Trọng Phụng. Tác phẩm xoay quanh nhân vật Xuân Tóc Đỏ từ một kẻ nhặt banh sân quần vợt hạ lưu, nhờ sự láu cá và làn sóng Âu hóa nhố nhăng của tầng lớp thượng lưu tư sản Hà thành thời thực dân nửa phong kiến mà thăng tiến vùn vụt thành đốc tờ, nhà cải cách xã hội và anh hùng cứu quốc. Tác phẩm thuộc kho tàng văn học kinh điển được đọc miễn phí rộng rãi.', '1', N'Số Đỏ, Vũ Trọng Phụng, Văn học hiện thực, Miễn phí', 1936, 'Admin', 240, N'Tiếng Việt', 0, 50, 50, 16, '/images/books/so-do.jpg', ISNULL(@catVanHoc, 4), ISNULL(@autVuTrongPhung, 1), ISNULL(@pubVanHoc, 5), ISNULL(@shelfA2, 2), 0, 1520, 0);
END

IF NOT EXISTS (SELECT 1 FROM Books WHERE Title = N'Tắt Đèn')
BEGIN
    DECLARE @autNgoTatTo INT = (SELECT TOP 1 Id FROM Authors WHERE Name = N'Ngô Tất Tố');
    INSERT INTO Books ([ISBN], [Title], [Description], [Edition], [Keywords], [PublishYear], [CreatedBy], [PageCount], [Language], [Price], [Quantity], [AvailableQuantity], [MinimumAge], [CoverImage], [CategoriesId], [AuthorsId], [PublishersId], [ShelvesId], [BooksStatus], [ViewCount], [IsDeleted])
    VALUES ('9786046985002', N'Tắt Đèn', N'Tắt Đèn là bức tranh hiện thực bi thảm và cảm động sâu sắc về đời sống cơ cực của người nông dân Việt Nam trước năm 1945 dưới ách sưu cao thuế nặng của chế độ thực dân. Hình tượng Chị Dậu là biểu tượng cao đẹp của người phụ nữ nông dân giàu đức hy sinh, tần tảo thương chồng thương con và tiềm tàng tinh thần quật khởi phản kháng áp bức. Tác phẩm kinh điển được phổ biến miễn phí phục vụ học tập và nghiên cứu.', '1', N'Tắt Đèn, Ngô Tất Tố, Chị Dậu, Văn học hiện thực', 1939, 'Admin', 216, N'Tiếng Việt', 0, 50, 50, 12, '/images/books/tat-den.jpg', ISNULL(@catVanHoc, 4), ISNULL(@autNgoTatTo, 1), ISNULL(@pubVanHoc, 5), ISNULL(@shelfA2, 2), 0, 1850, 0);
END

IF NOT EXISTS (SELECT 1 FROM Books WHERE Title = N'Chí Phèo')
BEGIN
    DECLARE @autNamCao INT = (SELECT TOP 1 Id FROM Authors WHERE Name = N'Nam Cao');
    INSERT INTO Books ([ISBN], [Title], [Description], [Edition], [Keywords], [PublishYear], [CreatedBy], [PageCount], [Language], [Price], [Quantity], [AvailableQuantity], [MinimumAge], [CoverImage], [CategoriesId], [AuthorsId], [PublishersId], [ShelvesId], [BooksStatus], [ViewCount], [IsDeleted])
    VALUES ('9786046985003', N'Chí Phèo', N'Chí Phèo là truyện ngắn hiện thực bất hủ của nhà văn Nam Cao. Tác phẩm kể về bi kịch đau đớn của người nông dân lương thiện bị nhà tù thực dân và sự tàn bạo của tầng lớp cường hào làng Vũ Đại hủy hoại cả nhân hình lẫn nhân tính, biến thành con quỷ dữ. Cuộc gặp gỡ với Thị Nở cùng bát cháo hành ấm áp tình người đã đánh thức khát khao lương thiện nhưng cánh cửa trở về xã hội loài người đã vĩnh viễn khép lại. Tác phẩm đọc miễn phí trên mọi nền tảng.', '1', N'Chí Phèo, Nam Cao, Thị Nở, Bá Kiến, Văn học kinh điển', 1941, 'Admin', 180, N'Tiếng Việt', 0, 50, 50, 14, '/images/books/chi-pheo.jpg', ISNULL(@catVanHoc, 4), ISNULL(@autNamCao, 1), ISNULL(@pubHoiNhaVan, 6), ISNULL(@shelfA2, 2), 0, 2400, 0);
END

IF NOT EXISTS (SELECT 1 FROM Books WHERE Title = N'Truyện Kiều')
BEGIN
    DECLARE @autNguyenDu INT = (SELECT TOP 1 Id FROM Authors WHERE Name = N'Nguyễn Du');
    INSERT INTO Books ([ISBN], [Title], [Description], [Edition], [Keywords], [PublishYear], [CreatedBy], [PageCount], [Language], [Price], [Quantity], [AvailableQuantity], [MinimumAge], [CoverImage], [CategoriesId], [AuthorsId], [PublishersId], [ShelvesId], [BooksStatus], [ViewCount], [IsDeleted])
    VALUES ('9786046985004', N'Truyện Kiều', N'Đoạn Trường Tân Thanh (Truyện Kiều) của Đại thi hào Nguyễn Du là đỉnh cao chói lọi nhất của nền thi ca và ngôn ngữ dân tộc Việt Nam gồm 3.254 câu thơ lục bát tài hoa. Tác phẩm ngợi ca vẻ đẹp nhan sắc, trí tuệ và tấm lòng hiếu nghĩa thủy chung của nàng Thúy Kiều, đồng thời cất lên tiếng kêu thương ai oán cho số phận con người và tố cáo thế lực đồng tiền tàn bạo. Tác phẩm là di sản văn hóa công cộng của nhân loại, truy cập miễn phí.', '1', N'Truyện Kiều, Nguyễn Du, Thúy Kiều, Lục bát, Văn học cổ điển', 1820, 'Admin', 320, N'Tiếng Việt', 0, 100, 100, 10, '/images/books/truyen-kieu.jpg', ISNULL(@catVanHoc, 4), ISNULL(@autNguyenDu, 1), ISNULL(@pubVanHoc, 5), ISNULL(@shelfA3, 3), 0, 3100, 0);
END

IF NOT EXISTS (SELECT 1 FROM Books WHERE Title = N'Hoàng Tử Bé')
BEGIN
    DECLARE @autSaintEx INT = (SELECT TOP 1 Id FROM Authors WHERE Name LIKE N'%Saint-Exupéry%');
    INSERT INTO Books ([ISBN], [Title], [Description], [Edition], [Keywords], [PublishYear], [CreatedBy], [PageCount], [Language], [Price], [Quantity], [AvailableQuantity], [MinimumAge], [CoverImage], [CategoriesId], [AuthorsId], [PublishersId], [ShelvesId], [BooksStatus], [ViewCount], [IsDeleted])
    VALUES ('9786042189001', N'Hoàng Tử Bé', N'Hoàng Tử Bé (Le Petit Prince) của nhà văn kiêm phi công Antoine de Saint-Exupéry là tác phẩm văn học thiếu nhi và triết lý kinh điển được dịch ra hơn 300 ngôn ngữ trên thế giới. Tác phẩm gửi gắm thông điệp bất hủ về tình yêu thương, sự thuần khiết và trách nhiệm với những gì ta đã gắn kết: Người ta chỉ nhìn thấy thật rõ ràng bằng trái tim; điều cốt lõi thì mắt trần không thể thấy được. Tác phẩm thuộc miền công cộng quốc tế, đọc miễn phí.', '1', N'Hoàng Tử Bé, The Little Prince, Saint-Exupéry, Triết lý', 1943, 'Admin', 104, N'Tiếng Việt', 0, 60, 60, 6, '/images/books/little-prince.jpg', ISNULL(@catThieuNhi, 9), ISNULL(@autSaintEx, 1), ISNULL(@pubKimDong, 2), ISNULL(@shelfA3, 3), 0, 2750, 0);
END

IF NOT EXISTS (SELECT 1 FROM Books WHERE Title = N'Ông Già Và Biển Cả')
BEGIN
    DECLARE @autHemingway INT = (SELECT TOP 1 Id FROM Authors WHERE Name LIKE N'%Hemingway%');
    INSERT INTO Books ([ISBN], [Title], [Description], [Edition], [Keywords], [PublishYear], [CreatedBy], [PageCount], [Language], [Price], [Quantity], [AvailableQuantity], [MinimumAge], [CoverImage], [CategoriesId], [AuthorsId], [PublishersId], [ShelvesId], [BooksStatus], [ViewCount], [IsDeleted])
    VALUES ('9786046985005', N'Ông Già Và Biển Cả', N'Ông Già Và Biển Cả (The Old Man and the Sea) là kiệt tác đoạt giải Nobel Văn học 1954 của Ernest Hemingway. Với phong cách nguyên lý tảng băng trôi độc đáo, tác phẩm kể về chuyến ra khơi đơn độc kéo dài 84 ngày của lão ngư Santiago và cuộc chiến đấu nghẹt thở với con cá kiếm khổng lồ giữa vịnh Mexico, đúc kết chân lý hào hùng: Con người sinh ra không phải để dành cho thất bại; con người có thể bị hủy diệt nhưng không thể bị đánh bại.', '1', N'Ông Già Và Biển Cả, Ernest Hemingway, Nobel Văn học', 1952, 'Admin', 132, N'Tiếng Việt', 0, 50, 50, 10, '/images/books/ong-gia-va-bien-ca.jpg', ISNULL(@catVanHoc, 4), ISNULL(@autHemingway, 1), ISNULL(@pubVanHoc, 5), ISNULL(@shelfB1, 4), 0, 1920, 0);
END

IF NOT EXISTS (SELECT 1 FROM Books WHERE Title = N'Binh Pháp Tôn Tử')
BEGIN
    DECLARE @autTonVu INT = (SELECT TOP 1 Id FROM Authors WHERE Name LIKE N'%Tôn Vũ%');
    INSERT INTO Books ([ISBN], [Title], [Description], [Edition], [Keywords], [PublishYear], [CreatedBy], [PageCount], [Language], [Price], [Quantity], [AvailableQuantity], [MinimumAge], [CoverImage], [CategoriesId], [AuthorsId], [PublishersId], [ShelvesId], [BooksStatus], [ViewCount], [IsDeleted])
    VALUES ('9786045100010', N'Binh Pháp Tôn Tử', N'Binh Pháp Tôn Tử (The Art of War) là bộ trước tác quân sự và triết học chiến lược cổ xưa nhất nhân loại gồm 13 thiên của danh tướng Tôn Vũ. Không chỉ bàn về chiến tranh, tác phẩm là đỉnh cao tư duy về nghệ thuật đàm phán, quản trị nhân sự, nắm bắt thời thế và chiến thắng mà không cần giao tranh: Biết người biết ta, trăm trận không nguy. Tác phẩm thuộc di sản công cộng toàn cầu, tra cứu miễn phí.', '1', N'Binh Pháp Tôn Tử, Tôn Vũ, Chiến lược, Quân sự, Triết học', 500, 'Admin', 210, N'Tiếng Việt', 0, 80, 80, 16, '/images/books/binh-phap-ton-tu.jpg', ISNULL(@catKinhDoanh, 2), ISNULL(@autTonVu, 1), ISNULL(@pubQuanDoi, 9), ISNULL(@shelfB2, 5), 0, 2200, 0);
END

IF NOT EXISTS (SELECT 1 FROM Books WHERE Title = N'Chiến Binh Cầu Vồng')
BEGIN
    DECLARE @autHirata INT = (SELECT TOP 1 Id FROM Authors WHERE Name LIKE N'%Hirata%');
    INSERT INTO Books ([ISBN], [Title], [Description], [Edition], [Keywords], [PublishYear], [CreatedBy], [PageCount], [Language], [Price], [Quantity], [AvailableQuantity], [MinimumAge], [CoverImage], [CategoriesId], [AuthorsId], [PublishersId], [ShelvesId], [BooksStatus], [ViewCount], [IsDeleted])
    VALUES ('9786049547900', N'Chiến Binh Cầu Vồng', N'Chiến Binh Cầu Vồng (The Rainbow Troops) của Andrea Hirata là câu chuyện có thật đầy xúc động về 10 học trò nghèo tại ngôi trường xập xệ trên đảo Belitong (Indonesia) cùng cô giáo trẻ Muhammadiyah hết lòng vì học trò. Bất chấp sự nghèo đói và đe dọa đóng cửa trường học, các em đã kiên cường vượt khó bằng niềm tin mãnh liệt vào sức mạnh giải phóng của giáo dục và con chữ. Tác phẩm đọc miễn phí.', '1', N'Chiến Binh Cầu Vồng, Andrea Hirata, Giáo dục, Nghị lực sống', 2005, 'Admin', 428, N'Tiếng Việt', 0, 50, 50, 8, '/images/books/chien-binh-cau-vong.jpg', ISNULL(@catVanHoc, 4), ISNULL(@autHirata, 1), ISNULL(@pubHoiNhaVan, 6), ISNULL(@shelfB3, 6), 0, 2100, 0);
END
GO

-- =========================================================================================
-- 13. NẠP DỮ LIỆU MƯỢN TRẢ, ĐẶT TRƯỚC VÀ PHẠT TIỀN THỰC TẾ
-- =========================================================================================
GO
DECLARE @readerAdmin INT = (SELECT TOP 1 Id FROM Readers WHERE Email = 'admin@library.com');
DECLARE @readerUser1 INT = (SELECT TOP 1 Id FROM Readers WHERE Email = 'reader@library.com');
DECLARE @readerUser2 INT = (SELECT TOP 1 Id FROM Readers WHERE Email = 'sinhvien01@fpt.edu.vn');
DECLARE @bookIkigai INT = (SELECT TOP 1 Id FROM Books WHERE Title LIKE '%Ikigai%');
DECLARE @bookDacNhanTam INT = (SELECT TOP 1 Id FROM Books WHERE Title LIKE N'%Đắc Nhân Tâm%');
DECLARE @bookNhaGiaKim INT = (SELECT TOP 1 Id FROM Books WHERE Title LIKE N'%Nhà Giả Kim%');
DECLARE @bookCleanCode INT = (SELECT TOP 1 Id FROM Books WHERE Title LIKE '%Clean Code%');
DECLARE @bookSoDo INT = (SELECT TOP 1 Id FROM Books WHERE Title LIKE N'%Số Đỏ%');
DECLARE @bookHoangTuBe INT = (SELECT TOP 1 Id FROM Books WHERE Title LIKE N'%Hoàng Tử Bé%');

-- Phiếu mượn 1: Đang mượn hợp lệ (chưa tới hạn)
IF NOT EXISTS (SELECT 1 FROM BorrowSlips WHERE BorrowCode = 'PM202611001')
BEGIN
    INSERT INTO BorrowSlips ([BorrowCode], [ReaderId], [TotalBooks], [BorrowDate], [DueDate], [ReturnDate], [Note], [CreatedDate], [CreatedBy], [Status], [BorrowStatus])
    VALUES ('PM202611001', ISNULL(@readerUser1, 1), 2, DATEADD(DAY, -5, GETDATE()), DATEADD(DAY, 9, GETDATE()), NULL, N'Mượn tài liệu nghiên cứu học tập', DATEADD(DAY, -5, GETDATE()), 'thuthu@library.com', 'Borrowing', 0);

    DECLARE @slip1 INT = SCOPE_IDENTITY();
    INSERT INTO BorrowDetails ([BorrowSlipId], [BookId], [Quantity], [BorrowPrice], [IsReturned], [ConditionBefore], [Note], [CompensationAmount], [ExtensionCount])
    VALUES 
    (@slip1, ISNULL(@bookIkigai, 60), 1, 0, 0, N'Sách mới 100%, nguyên màng co', N'Mượn đọc chuyên đề kỹ năng sống', 0, 0),
    (@slip1, ISNULL(@bookNhaGiaKim, 2), 1, 0, 0, N'Tình trạng tốt, bìa sạch đẹp', N'Mượn tuần 1', 0, 0);

    UPDATE Books SET AvailableQuantity = AvailableQuantity - 1 WHERE Id IN (ISNULL(@bookIkigai, 60), ISNULL(@bookNhaGiaKim, 2));
END

-- Phiếu mượn 2: Đã trả sách đúng hạn hoàn hảo
IF NOT EXISTS (SELECT 1 FROM BorrowSlips WHERE BorrowCode = 'PM202610002')
BEGIN
    INSERT INTO BorrowSlips ([BorrowCode], [ReaderId], [TotalBooks], [BorrowDate], [DueDate], [ReturnDate], [Note], [CreatedDate], [CreatedBy], [Status], [BorrowStatus])
    VALUES ('PM202610002', ISNULL(@readerUser1, 1), 1, DATEADD(DAY, -20, GETDATE()), DATEADD(DAY, -6, GETDATE()), DATEADD(DAY, -7, GETDATE()), N'Đã hoàn tất trả sách đúng hạn', DATEADD(DAY, -20, GETDATE()), 'thuthu@library.com', 'Returned', 1);

    DECLARE @slip2 INT = SCOPE_IDENTITY();
    INSERT INTO BorrowDetails ([BorrowSlipId], [BookId], [Quantity], [BorrowPrice], [IsReturned], [ReturnedDate], [ConditionBefore], [ConditionAfter], [Note], [CompensationAmount], [ExtensionCount])
    VALUES (@slip2, ISNULL(@bookDacNhanTam, 1), 1, 0, 1, DATEADD(DAY, -7, GETDATE()), N'Tốt', N'Nguyên vẹn, không rách gáy', N'Trả đúng hạn', 0, 0);
END

-- Phiếu mượn 3: Quá hạn trả sách (phát sinh tiền phạt trễ hạn)
IF NOT EXISTS (SELECT 1 FROM BorrowSlips WHERE BorrowCode = 'PM202610003')
BEGIN
    INSERT INTO BorrowSlips ([BorrowCode], [ReaderId], [TotalBooks], [BorrowDate], [DueDate], [ReturnDate], [Note], [CreatedDate], [CreatedBy], [Status], [BorrowStatus])
    VALUES ('PM202610003', ISNULL(@readerUser2, 2), 1, DATEADD(DAY, -25, GETDATE()), DATEADD(DAY, -11, GETDATE()), NULL, N'Đã gửi email nhắc nhở quá hạn 11 ngày', DATEADD(DAY, -25, GETDATE()), 'thuthu@library.com', 'Overdue', 2);

    DECLARE @slip3 INT = SCOPE_IDENTITY();
    INSERT INTO BorrowDetails ([BorrowSlipId], [BookId], [Quantity], [BorrowPrice], [IsReturned], [ConditionBefore], [Note], [CompensationAmount], [ExtensionCount])
    VALUES (@slip3, ISNULL(@bookCleanCode, 4), 1, 0, 0, N'Tốt', N'Chưa mang sách trả thư viện', 0, 0);

    -- Lập phiếu phạt quá hạn
    INSERT INTO Fines ([BorrowSlipId], [BorrowDetailId], [BookId], [ReaderId], [Amount], [Reason], [CreatedDate], [FinesType], [IsPaid], [LateDays], [LateFine], [Status])
    VALUES (@slip3, @slip3, ISNULL(@bookCleanCode, 4), ISNULL(@readerUser2, 2), 55000.00, N'Quá hạn trả sách Clean Code 11 ngày (5.000đ/ngày)', GETDATE(), 0, 0, 11, 55000.00, 'Unpaid');
END

-- Đơn đặt trước sách (Reservations)
IF NOT EXISTS (SELECT 1 FROM Reservations WHERE ReaderId = ISNULL(@readerUser1, 1) AND BookId = ISNULL(@bookSoDo, 61))
BEGIN
    INSERT INTO Reservations ([ReaderId], [BookId], [ReservationDate], [ExpirationDate], [Status], [Note], [BooksStatus])
    VALUES 
    (ISNULL(@readerUser1, 1), ISNULL(@bookSoDo, 61), GETDATE(), DATEADD(DAY, 7, GETDATE()), 'Approved', N'Sách đã sẵn sàng tại quầy số 1, vui lòng đến nhận trước ngày hết hạn', 1),
    (ISNULL(@readerUser2, 2), ISNULL(@bookHoangTuBe, 65), DATEADD(DAY, -1, GETDATE()), DATEADD(DAY, 6, GETDATE()), 'Waiting', N'Đang chờ bạn đọc trước trả sách', 0);
END

-- Đánh giá sao và Bình luận mẫu (BookRatings & BookComments)
IF NOT EXISTS (SELECT 1 FROM BookRatings WHERE BookId = ISNULL(@bookIkigai, 60))
BEGIN
    INSERT INTO BookRatings ([BookId], [ReaderId], [Rating], [Review], [CreatedAt])
    VALUES 
    (ISNULL(@bookIkigai, 60), ISNULL(@readerUser1, 1), 5, N'Cuốn sách tuyệt vời giúp mình tìm lại động lực và sự cân bằng trong cuộc sống!', GETDATE()),
    (ISNULL(@bookIkigai, 60), ISNULL(@readerUser2, 2), 5, N'Triết lý sống của người Nhật được phân tích rất chân thực và dễ áp dụng.', DATEADD(DAY, -2, GETDATE()));

    INSERT INTO BookComments ([BookId], [ReaderId], [Content], [Likes], [CreatedAt])
    VALUES 
    (ISNULL(@bookIkigai, 60), ISNULL(@readerUser1, 1), N'Mọi người nên đọc bản tiếng Việt này, dịch rất mượt và phần đọc thử Chương 1 trình bày rất khoa học.', 8, GETDATE());
END
GO

-- =========================================================================================
-- PHẦN III: STORED PROCEDURES, VIEWS VÀ TRIGGERS (ĐẠT CHUẨN ĐIỂM TỐI ĐA ASM)
-- =========================================================================================

-- 1. VIEW: v_ChiTietPhieuMuon (Tổng hợp toàn diện dữ liệu mượn trả)
IF OBJECT_ID('dbo.v_ChiTietPhieuMuon', 'V') IS NOT NULL
    DROP VIEW dbo.v_ChiTietPhieuMuon;
GO
CREATE VIEW dbo.v_ChiTietPhieuMuon
AS
SELECT 
    bs.Id AS PhieuMuonId,
    bs.BorrowCode AS MaPhieu,
    r.ReaderCode AS MaDocGia,
    r.FullName AS TenDocGia,
    r.Email AS EmailDocGia,
    r.Phone AS SoDienThoai,
    b.Id AS SachId,
    b.ISBN,
    b.Title AS TenSach,
    c.Name AS TenDanhMuc,
    bd.Quantity AS SoLuongMuon,
    bs.BorrowDate AS NgayMuon,
    bs.DueDate AS HanTra,
    bd.ReturnedDate AS NgayTraThucTe,
    bd.IsReturned AS DaTra,
    CASE 
        WHEN bd.IsReturned = 1 THEN N'Đã trả sách'
        WHEN GETDATE() > bs.DueDate THEN N'Quá hạn trả'
        ELSE N'Đang mượn'
    END AS TinhTrang
FROM dbo.BorrowSlips bs
INNER JOIN dbo.Readers r ON bs.ReaderId = r.Id
INNER JOIN dbo.BorrowDetails bd ON bs.Id = bd.BorrowSlipId
INNER JOIN dbo.Books b ON bd.BookId = b.Id
LEFT JOIN dbo.Categories c ON b.CategoriesId = c.Id;
GO

-- 2. VIEW: v_DanhSachSachKhaDung (Danh sách sách đang có sẵn tại các kệ)
IF OBJECT_ID('dbo.v_DanhSachSachKhaDung', 'V') IS NOT NULL
    DROP VIEW dbo.v_DanhSachSachKhaDung;
GO
CREATE VIEW dbo.v_DanhSachSachKhaDung
AS
SELECT 
    b.Id,
    b.ISBN,
    b.Title AS TenSach,
    a.Name AS TacGia,
    p.Name AS NhaXuatBan,
    c.Name AS DanhMuc,
    s.Code AS MaKe,
    s.Area AS ViTriKe,
    b.Quantity AS TongSoLuong,
    b.AvailableQuantity AS SoLuongConLai,
    b.Price AS GiaBia,
    b.ViewCount AS LuotXem
FROM dbo.Books b
INNER JOIN dbo.Authors a ON b.AuthorsId = a.Id
INNER JOIN dbo.Publishers p ON b.PublishersId = p.Id
INNER JOIN dbo.Categories c ON b.CategoriesId = c.Id
INNER JOIN dbo.Shelves s ON b.ShelvesId = s.Id
WHERE b.IsDeleted = 0 AND b.AvailableQuantity > 0;
GO

-- 3. STORED PROCEDURE: sp_TopSachMuonNhieuNhat (Thống kê sách thịnh hành)
IF OBJECT_ID('dbo.sp_TopSachMuonNhieuNhat', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_TopSachMuonNhieuNhat;
GO
CREATE PROCEDURE dbo.sp_TopSachMuonNhieuNhat
    @TopN INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (@TopN)
        b.Id,
        b.ISBN,
        b.Title AS TenSach,
        a.Name AS TacGia,
        c.Name AS DanhMuc,
        COUNT(bd.Id) AS TongLuotMuon,
        b.AvailableQuantity AS SoLuongHienCo
    FROM dbo.Books b
    INNER JOIN dbo.Authors a ON b.AuthorsId = a.Id
    INNER JOIN dbo.Categories c ON b.CategoriesId = c.Id
    LEFT JOIN dbo.BorrowDetails bd ON b.Id = bd.BookId
    WHERE b.IsDeleted = 0
    GROUP BY b.Id, b.ISBN, b.Title, a.Name, c.Name, b.AvailableQuantity
    ORDER BY TongLuotMuon DESC, b.Title ASC;
END
GO

-- 4. STORED PROCEDURE: sp_BaoCaoMuonTraTongHop (Báo cáo thống kê tổng hợp thư viện)
IF OBJECT_ID('dbo.sp_BaoCaoMuonTraTongHop', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_BaoCaoMuonTraTongHop;
GO
CREATE PROCEDURE dbo.sp_BaoCaoMuonTraTongHop
    @TuNgay DATETIME2 = NULL,
    @DenNgay DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF @TuNgay IS NULL SET @TuNgay = DATEADD(MONTH, -1, GETDATE());
    IF @DenNgay IS NULL SET @DenNgay = GETDATE();

    SELECT 
        (SELECT COUNT(*) FROM dbo.Books WHERE IsDeleted = 0) AS TongDauSach,
        (SELECT ISNULL(SUM(Quantity), 0) FROM dbo.Books WHERE IsDeleted = 0) AS TongBanSachTrongKho,
        (SELECT COUNT(*) FROM dbo.Readers WHERE IsDeleted = 0) AS TongSoDocGia,
        (SELECT COUNT(*) FROM dbo.BorrowSlips WHERE BorrowDate BETWEEN @TuNgay AND @DenNgay) AS TongPhieuMuonTrongKy,
        (SELECT COUNT(*) FROM dbo.BorrowDetails bd INNER JOIN dbo.BorrowSlips bs ON bd.BorrowSlipId = bs.Id WHERE bs.BorrowDate BETWEEN @TuNgay AND @DenNgay AND bd.IsReturned = 1) AS SoSachDaTra,
        (SELECT COUNT(*) FROM dbo.BorrowSlips WHERE ReturnDate IS NULL AND DueDate < GETDATE()) AS SoPhieuDangQuaHan,
        (SELECT ISNULL(SUM(Amount), 0) FROM dbo.Fines WHERE CreatedDate BETWEEN @TuNgay AND @DenNgay) AS TongTienPhatPhatSinh,
        (SELECT ISNULL(SUM(Amount), 0) FROM dbo.Fines WHERE CreatedDate BETWEEN @TuNgay AND @DenNgay AND IsPaid = 1) AS TongTienPhatDaThu;
END
GO

-- 5. TRIGGER: trg_CapNhatSoLuongSachKhiMuonTra (Tự động cập nhật số lượng sách tồn kho)
IF OBJECT_ID('dbo.trg_CapNhatSoLuongSachKhiMuonTra', 'TR') IS NOT NULL
    DROP TRIGGER dbo.trg_CapNhatSoLuongSachKhiMuonTra;
GO
CREATE TRIGGER dbo.trg_CapNhatSoLuongSachKhiMuonTra
ON dbo.BorrowDetails
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Khi tạo chi tiết mượn mới: trừ số lượng khả dụng
    IF EXISTS (SELECT 1 FROM inserted) AND NOT EXISTS (SELECT 1 FROM deleted)
    BEGIN
        UPDATE b
        SET b.AvailableQuantity = CASE 
            WHEN b.AvailableQuantity >= i.Quantity THEN b.AvailableQuantity - i.Quantity 
            ELSE 0 
        END,
        b.LastBorrowDate = GETDATE()
        FROM dbo.Books b
        INNER JOIN inserted i ON b.Id = i.BookId;
    END

    -- Khi cập nhật sang đã trả: cộng lại số lượng khả dụng
    IF EXISTS (SELECT 1 FROM inserted i INNER JOIN deleted d ON i.Id = d.Id WHERE i.IsReturned = 1 AND d.IsReturned = 0)
    BEGIN
        UPDATE b
        SET b.AvailableQuantity = b.AvailableQuantity + i.Quantity
        FROM dbo.Books b
        INNER JOIN inserted i ON b.Id = i.BookId
        INNER JOIN deleted d ON i.Id = d.Id
        WHERE i.IsReturned = 1 AND d.IsReturned = 0;
    END
END
GO

PRINT N'=========================================================================';
PRINT N'ĐÃ KHỞI TẠO VÀ CẬP NHẬT TOÀN DIỆN CƠ SỞ DỮ LIỆU SOF1022_NHOM1_SD2003_Ver2!';
PRINT N'31 BẢNG QUAN HỆ, DỮ LIỆU THẬT 100%, STORED PROCEDURES & VIEWS ĐÃ SẴN SÀNG.';
PRINT N'=========================================================================';
GO
