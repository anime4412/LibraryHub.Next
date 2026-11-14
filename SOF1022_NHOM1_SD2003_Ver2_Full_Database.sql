-- =========================================================================
-- TRƯỜNG CAO ĐẲNG FPT POLYTECHNIC - KHOA CÔNG NGHỆ THÔNG TIN
-- DỰ ÁN .NET 1 (SOF1022 / NET108) - LỚP SD2003 - NHÓM 1
-- ĐỀ TÀI: XÂY DỰNG HỆ THỐNG WEBSITE QUẢN LÝ THƯ VIỆN ĐIỆN TỬ HIỆN ĐẠI
-- DATABASE: SOF1022_NHOM1_SD2003_Ver2
-- HỆ QUẢN TRỊ CSDL: MICROSOFT SQL SERVER 2019 / 2022
-- =========================================================================

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'SOF1022_NHOM1_SD2003_Ver2')
BEGIN
    CREATE DATABASE [SOF1022_NHOM1_SD2003_Ver2] COLLATE SQL_Latin1_General_CP1_CI_AS;
END;
GO

USE [SOF1022_NHOM1_SD2003_Ver2];
GO

-- =========================================================================
-- PHẦN 1: CẤU TRÚC BẢNG, KHÓA CHÍNH, KHÓA NGOẠI, INDEX (33 BẢNG QUAN HỆ)
-- =========================================================================

IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE TABLE [Authors] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(150) NOT NULL,
        [DateOfBirth] datetime2 NULL,
        [Nationality] nvarchar(100) NULL,
        [Biography] nvarchar(1000) NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedDate] datetime2 NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Authors] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE TABLE [Categories] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedDate] datetime2 NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Categories] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE TABLE [EmployeeStatusOptions] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(50) NOT NULL,
        [Description] nvarchar(200) NULL,
        [DisplayOrder] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_EmployeeStatusOptions] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE TABLE [Positions] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(50) NOT NULL,
        [Description] nvarchar(200) NULL,
        [CanManageBooks] bit NOT NULL,
        [CanManageUsers] bit NOT NULL,
        [CanManageLoans] bit NOT NULL,
        [CanManageReports] bit NOT NULL,
        [CanManageSystem] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Positions] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE TABLE [Publishers] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(150) NOT NULL,
        [Address] nvarchar(255) NULL,
        [Phone] nvarchar(max) NULL,
        [Email] nvarchar(max) NULL,
        [Website] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedDate] datetime2 NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Publishers] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE TABLE [Readers] (
        [Id] int NOT NULL IDENTITY,
        [ReaderCode] nvarchar(20) NULL,
        [FullName] nvarchar(150) NOT NULL,
        [DateOfBirth] datetime2 NOT NULL,
        [Phone] nvarchar(max) NOT NULL,
        [Email] nvarchar(max) NOT NULL,
        [Address] nvarchar(500) NOT NULL,
        [IdentityNumber] nvarchar(20) NOT NULL,
        [RegisterDate] datetime2 NOT NULL,
        [ExpirationDate] datetime2 NOT NULL,
        [LastLoginDate] datetime2 NULL,
        [ConsecutiveLoginDays] int NOT NULL,
        [LastStreakDate] datetime2 NULL,
        [Avatar] nvarchar(max) NULL,
        [CoverImageUrl] nvarchar(max) NULL,
        [Bio] nvarchar(max) NULL,
        [ActiveBadge] nvarchar(max) NULL,
        [FullPageBackgroundUrl] nvarchar(max) NULL,
        [FeaturedBookIds] nvarchar(max) NULL,
        [IsCommentBlocked] bit NOT NULL,
        [CommentBlockedUntil] datetime2 NULL,
        [MaxBorrowBooks] int NOT NULL,
        [Password] nvarchar(256) NOT NULL,
        [Role] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        [EmailConfirmed] bit NOT NULL,
        [EmailConfirmationToken] nvarchar(max) NULL,
        [EmailConfirmationSentDate] datetime2 NULL,
        [ForcePasswordChange] bit NOT NULL,
        [ReadersType] int NOT NULL,
        [Genders] int NOT NULL,
        [IsDeleted] bit NOT NULL,
        [IsNameSet] bit NOT NULL,
        CONSTRAINT [PK_Readers] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE TABLE [Shelves] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(20) NOT NULL,
        [LocationCode] nvarchar(20) NULL,
        [Name] nvarchar(100) NOT NULL,
        [Area] nvarchar(100) NULL,
        [Description] nvarchar(300) NULL,
        [Capacity] int NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedDate] datetime2 NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Shelves] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE TABLE [SystemSettings] (
        [Id] int NOT NULL IDENTITY,
        [MaxExtensionDays] int NOT NULL,
        [MaxExtensionCount] int NOT NULL,
        [LateFeePerDay] decimal(18,2) NOT NULL,
        [MaxBorrowBooks] int NOT NULL,
        [BorrowPeriodDays] int NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedDate] datetime2 NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_SystemSettings] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE TABLE [Employees] (
        [Id] int NOT NULL IDENTITY,
        [EmployeeId] nvarchar(20) NOT NULL,
        [FullName] nvarchar(100) NOT NULL,
        [Email] nvarchar(100) NOT NULL,
        [Phone] nvarchar(max) NOT NULL,
        [Address] nvarchar(200) NOT NULL,
        [CCCD] nvarchar(max) NOT NULL,
        [Gender] int NOT NULL,
        [DateOfBirth] datetime2 NOT NULL,
        [HireDate] datetime2 NOT NULL,
        [PositionId] int NOT NULL,
        [Salary] decimal(18,2) NOT NULL,
        [StatusId] int NOT NULL,
        [LeaveReason] nvarchar(500) NULL,
        [LeaveStartDate] datetime2 NULL,
        [UnexcusedDaysCount] int NOT NULL,
        [LastUnexcusedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Employees] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Employees_EmployeeStatusOptions_StatusId] FOREIGN KEY ([StatusId]) REFERENCES [EmployeeStatusOptions] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Employees_Positions_PositionId] FOREIGN KEY ([PositionId]) REFERENCES [Positions] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE TABLE [BorrowSlips] (
        [Id] int NOT NULL IDENTITY,
        [BorrowCode] nvarchar(30) NOT NULL,
        [ReaderId] int NOT NULL,
        [TotalBooks] int NOT NULL,
        [BorrowDate] datetime2 NOT NULL,
        [DueDate] datetime2 NOT NULL,
        [ReturnDate] datetime2 NULL,
        [PickupDeadline] datetime2 NULL,
        [Note] nvarchar(max) NULL,
        [CreatedDate] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [Status] nvarchar(max) NULL,
        [BorrowStatus] int NOT NULL,
        [ReadersId] int NULL,
        CONSTRAINT [PK_BorrowSlips] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_BorrowSlips_Readers_ReaderId] FOREIGN KEY ([ReaderId]) REFERENCES [Readers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_BorrowSlips_Readers_ReadersId] FOREIGN KEY ([ReadersId]) REFERENCES [Readers] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE TABLE [Notifications] (
        [Id] int NOT NULL IDENTITY,
        [RecipientId] int NOT NULL,
        [RecipientType] nvarchar(50) NOT NULL,
        [Type] int NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Message] nvarchar(1000) NOT NULL,
        [Link] nvarchar(500) NULL,
        [IsRead] bit NOT NULL,
        [ReadDate] datetime2 NULL,
        [CreatedDate] datetime2 NOT NULL,
        [CreatedBy] nvarchar(150) NULL,
        [BroadcastGroupId] uniqueidentifier NULL,
        CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Notifications_Readers_RecipientId] FOREIGN KEY ([RecipientId]) REFERENCES [Readers] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE TABLE [ProfileComments] (
        [Id] int NOT NULL IDENTITY,
        [ProfileOwnerId] int NOT NULL,
        [AuthorId] int NOT NULL,
        [Content] nvarchar(500) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ProfileComments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProfileComments_Readers_AuthorId] FOREIGN KEY ([AuthorId]) REFERENCES [Readers] ([Id]),
        CONSTRAINT [FK_ProfileComments_Readers_ProfileOwnerId] FOREIGN KEY ([ProfileOwnerId]) REFERENCES [Readers] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE TABLE [Books] (
        [Id] int NOT NULL IDENTITY,
        [ISBN] nvarchar(20) NOT NULL,
        [Title] nvarchar(250) NOT NULL,
        [Description] nvarchar(max) NULL,
        [Edition] nvarchar(50) NULL,
        [Keywords] nvarchar(300) NULL,
        [PublishYear] int NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [PageCount] int NOT NULL,
        [Language] nvarchar(max) NULL,
        [LastBorrowDate] datetime2 NULL,
        [Price] decimal(18,2) NOT NULL,
        [Quantity] int NOT NULL,
        [AvailableQuantity] int NOT NULL,
        [MinimumAge] int NOT NULL,
        [CoverImage] nvarchar(max) NULL,
        [CategoriesId] int NOT NULL,
        [AuthorsId] int NOT NULL,
        [PublishersId] int NOT NULL,
        [ShelvesId] int NOT NULL,
        [BooksStatus] int NOT NULL,
        [ViewCount] int NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Books] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Books_Authors_AuthorsId] FOREIGN KEY ([AuthorsId]) REFERENCES [Authors] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Books_Categories_CategoriesId] FOREIGN KEY ([CategoriesId]) REFERENCES [Categories] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Books_Publishers_PublishersId] FOREIGN KEY ([PublishersId]) REFERENCES [Publishers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Books_Shelves_ShelvesId] FOREIGN KEY ([ShelvesId]) REFERENCES [Shelves] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE TABLE [SupportTickets] (
        [Id] int NOT NULL IDENTITY,
        [TicketCode] nvarchar(20) NOT NULL,
        [ReaderId] int NOT NULL,
        [Subject] nvarchar(500) NULL,
        [Status] int NOT NULL,
        [AssignedEmployeeId] int NULL,
        [CreatedDate] datetime2 NOT NULL,
        [ClosedDate] datetime2 NULL,
        [LastMessageDate] datetime2 NOT NULL,
        [UnreadByReader] bit NOT NULL,
        [UnreadByStaff] bit NOT NULL,
        CONSTRAINT [PK_SupportTickets] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SupportTickets_Employees_AssignedEmployeeId] FOREIGN KEY ([AssignedEmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SupportTickets_Readers_ReaderId] FOREIGN KEY ([ReaderId]) REFERENCES [Readers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE TABLE [BookComments] (
        [Id] int NOT NULL IDENTITY,
        [BookId] int NOT NULL,
        [ReaderId] int NOT NULL,
        [Content] nvarchar(2000) NOT NULL,
        [ParentCommentId] int NULL,
        [Likes] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_BookComments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_BookComments_BookComments_ParentCommentId] FOREIGN KEY ([ParentCommentId]) REFERENCES [BookComments] ([Id]),
        CONSTRAINT [FK_BookComments_Books_BookId] FOREIGN KEY ([BookId]) REFERENCES [Books] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_BookComments_Readers_ReaderId] FOREIGN KEY ([ReaderId]) REFERENCES [Readers] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE TABLE [BookFavorites] (
        [Id] int NOT NULL IDENTITY,
        [BookId] int NOT NULL,
        [ReaderId] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_BookFavorites] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_BookFavorites_Books_BookId] FOREIGN KEY ([BookId]) REFERENCES [Books] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_BookFavorites_Readers_ReaderId] FOREIGN KEY ([ReaderId]) REFERENCES [Readers] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE TABLE [BookRatings] (
        [Id] int NOT NULL IDENTITY,
        [BookId] int NOT NULL,
        [ReaderId] int NOT NULL,
        [Rating] int NOT NULL,
        [Review] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_BookRatings] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_BookRatings_Books_BookId] FOREIGN KEY ([BookId]) REFERENCES [Books] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_BookRatings_Readers_ReaderId] FOREIGN KEY ([ReaderId]) REFERENCES [Readers] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE TABLE [BookViewHistories] (
        [Id] int NOT NULL IDENTITY,
        [BookId] int NOT NULL,
        [ReaderId] int NULL,
        [SessionId] nvarchar(max) NULL,
        [ViewedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_BookViewHistories] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_BookViewHistories_Books_BookId] FOREIGN KEY ([BookId]) REFERENCES [Books] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_BookViewHistories_Readers_ReaderId] FOREIGN KEY ([ReaderId]) REFERENCES [Readers] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE TABLE [BorrowDetails] (
        [Id] int NOT NULL IDENTITY,
        [BorrowSlipId] int NOT NULL,
        [BookId] int NOT NULL,
        [Quantity] int NOT NULL,
        [BorrowPrice] decimal(18,2) NOT NULL,
        [IsReturned] bit NOT NULL,
        [ReturnedDate] datetime2 NULL,
        [ConditionBefore] nvarchar(max) NULL,
        [ConditionAfter] nvarchar(max) NULL,
        [Note] nvarchar(max) NULL,
        [IsLost] bit NOT NULL,
        [IsDamaged] bit NOT NULL,
        [DamagePercent] int NULL,
        [ExtensionCount] int NOT NULL,
        [DueDate] datetime2 NULL,
        [RequestedExtensionDays] int NULL,
        [ExtensionReason] nvarchar(max) NULL,
        [CompensationAmount] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_BorrowDetails] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_BorrowDetails_Books_BookId] FOREIGN KEY ([BookId]) REFERENCES [Books] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_BorrowDetails_BorrowSlips_BorrowSlipId] FOREIGN KEY ([BorrowSlipId]) REFERENCES [BorrowSlips] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE TABLE [ReadingHistories] (
        [Id] int NOT NULL IDENTITY,
        [BookId] int NOT NULL,
        [ReaderId] int NOT NULL,
        [CurrentPage] int NOT NULL,
        [TotalPages] int NOT NULL,
        [LastReadAt] datetime2 NOT NULL,
        [StartedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ReadingHistories] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ReadingHistories_Books_BookId] FOREIGN KEY ([BookId]) REFERENCES [Books] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ReadingHistories_Readers_ReaderId] FOREIGN KEY ([ReaderId]) REFERENCES [Readers] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE TABLE [Reservations] (
        [Id] int NOT NULL IDENTITY,
        [ReaderId] int NOT NULL,
        [BookId] int NOT NULL,
        [ReservationDate] datetime2 NOT NULL,
        [ExpirationDate] datetime2 NULL,
        [Status] nvarchar(30) NOT NULL,
        [Note] nvarchar(max) NULL,
        [BooksStatus] int NOT NULL,
        [BooksId] int NULL,
        [ReadersId] int NULL,
        CONSTRAINT [PK_Reservations] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Reservations_Books_BookId] FOREIGN KEY ([BookId]) REFERENCES [Books] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Reservations_Books_BooksId] FOREIGN KEY ([BooksId]) REFERENCES [Books] ([Id]),
        CONSTRAINT [FK_Reservations_Readers_ReaderId] FOREIGN KEY ([ReaderId]) REFERENCES [Readers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Reservations_Readers_ReadersId] FOREIGN KEY ([ReadersId]) REFERENCES [Readers] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE TABLE [SupportMessages] (
        [Id] int NOT NULL IDENTITY,
        [SupportTicketId] int NOT NULL,
        [SenderType] int NOT NULL,
        [SenderId] int NULL,
        [Content] nvarchar(max) NOT NULL,
        [SentDate] datetime2 NOT NULL,
        CONSTRAINT [PK_SupportMessages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SupportMessages_SupportTickets_SupportTicketId] FOREIGN KEY ([SupportTicketId]) REFERENCES [SupportTickets] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE TABLE [BookCommentLikes] (
        [Id] int NOT NULL IDENTITY,
        [BookCommentId] int NOT NULL,
        [ReaderId] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_BookCommentLikes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_BookCommentLikes_BookComments_BookCommentId] FOREIGN KEY ([BookCommentId]) REFERENCES [BookComments] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_BookCommentLikes_Readers_ReaderId] FOREIGN KEY ([ReaderId]) REFERENCES [Readers] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE TABLE [CommentReports] (
        [Id] int NOT NULL IDENTITY,
        [CommentId] int NOT NULL,
        [ReporterId] int NOT NULL,
        [ReportedUserId] int NOT NULL,
        [Reason] nvarchar(500) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_CommentReports] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CommentReports_BookComments_CommentId] FOREIGN KEY ([CommentId]) REFERENCES [BookComments] ([Id]),
        CONSTRAINT [FK_CommentReports_Readers_ReportedUserId] FOREIGN KEY ([ReportedUserId]) REFERENCES [Readers] ([Id]),
        CONSTRAINT [FK_CommentReports_Readers_ReporterId] FOREIGN KEY ([ReporterId]) REFERENCES [Readers] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE TABLE [Fines] (
        [Id] int NOT NULL IDENTITY,
        [BorrowSlipId] int NOT NULL,
        [BorrowDetailId] int NULL,
        [BookId] int NULL,
        [ReaderId] int NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Reason] nvarchar(max) NULL,
        [CreatedDate] datetime2 NOT NULL,
        [PaidDate] datetime2 NULL,
        [FinesType] int NOT NULL,
        [IsPaid] bit NOT NULL,
        [DamagePercent] int NULL,
        [LateDays] int NULL,
        [LateFine] decimal(18,2) NULL,
        [DamageFine] decimal(18,2) NULL,
        [LostFine] decimal(18,2) NULL,
        [Status] nvarchar(max) NOT NULL,
        [TransactionCode] nvarchar(max) NULL,
        [BorrowDetailsId] int NULL,
        CONSTRAINT [PK_Fines] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Fines_Books_BookId] FOREIGN KEY ([BookId]) REFERENCES [Books] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Fines_BorrowDetails_BorrowDetailsId] FOREIGN KEY ([BorrowDetailsId]) REFERENCES [BorrowDetails] ([Id]),
        CONSTRAINT [FK_Fines_BorrowSlips_BorrowSlipId] FOREIGN KEY ([BorrowSlipId]) REFERENCES [BorrowSlips] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE TABLE [FineDetails] (
        [Id] int NOT NULL IDENTITY,
        [FineId] int NOT NULL,
        [BookId] int NOT NULL,
        [BorrowDetailId] int NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Reason] nvarchar(max) NULL,
        [FinesType] int NOT NULL,
        [DamagePercent] int NULL,
        [BookPrice] decimal(18,2) NULL,
        [CreatedDate] datetime2 NOT NULL,
        CONSTRAINT [PK_FineDetails] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_FineDetails_Books_BookId] FOREIGN KEY ([BookId]) REFERENCES [Books] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_FineDetails_BorrowDetails_BorrowDetailId] FOREIGN KEY ([BorrowDetailId]) REFERENCES [BorrowDetails] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_FineDetails_Fines_FineId] FOREIGN KEY ([FineId]) REFERENCES [Fines] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE INDEX [IX_BookCommentLikes_BookCommentId] ON [BookCommentLikes] ([BookCommentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE INDEX [IX_BookCommentLikes_ReaderId] ON [BookCommentLikes] ([ReaderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE INDEX [IX_BookComments_BookId] ON [BookComments] ([BookId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE INDEX [IX_BookComments_ParentCommentId] ON [BookComments] ([ParentCommentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE INDEX [IX_BookComments_ReaderId] ON [BookComments] ([ReaderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE INDEX [IX_BookFavorites_BookId] ON [BookFavorites] ([BookId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE INDEX [IX_BookFavorites_ReaderId] ON [BookFavorites] ([ReaderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE INDEX [IX_BookRatings_BookId] ON [BookRatings] ([BookId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE INDEX [IX_BookRatings_ReaderId] ON [BookRatings] ([ReaderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE INDEX [IX_Books_AuthorsId] ON [Books] ([AuthorsId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE INDEX [IX_Books_CategoriesId] ON [Books] ([CategoriesId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE INDEX [IX_Books_PublishersId] ON [Books] ([PublishersId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE INDEX [IX_Books_ShelvesId] ON [Books] ([ShelvesId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE INDEX [IX_BookViewHistories_BookId] ON [BookViewHistories] ([BookId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE INDEX [IX_BookViewHistories_ReaderId] ON [BookViewHistories] ([ReaderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE INDEX [IX_BorrowDetails_BookId] ON [BorrowDetails] ([BookId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE INDEX [IX_BorrowDetails_BorrowSlipId] ON [BorrowDetails] ([BorrowSlipId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE INDEX [IX_BorrowSlips_ReaderId] ON [BorrowSlips] ([ReaderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE INDEX [IX_BorrowSlips_ReadersId] ON [BorrowSlips] ([ReadersId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE INDEX [IX_CommentReports_CommentId] ON [CommentReports] ([CommentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE INDEX [IX_CommentReports_ReportedUserId] ON [CommentReports] ([ReportedUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE INDEX [IX_CommentReports_ReporterId] ON [CommentReports] ([ReporterId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE INDEX [IX_Employees_PositionId] ON [Employees] ([PositionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE INDEX [IX_Employees_StatusId] ON [Employees] ([StatusId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE INDEX [IX_FineDetails_BookId] ON [FineDetails] ([BookId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE INDEX [IX_FineDetails_BorrowDetailId] ON [FineDetails] ([BorrowDetailId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE INDEX [IX_FineDetails_FineId] ON [FineDetails] ([FineId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE INDEX [IX_Fines_BookId] ON [Fines] ([BookId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE INDEX [IX_Fines_BorrowDetailId] ON [Fines] ([BorrowDetailId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE INDEX [IX_Fines_BorrowDetailsId] ON [Fines] ([BorrowDetailsId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE INDEX [IX_Fines_BorrowSlipId] ON [Fines] ([BorrowSlipId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE INDEX [IX_Notifications_BroadcastGroupId] ON [Notifications] ([BroadcastGroupId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE INDEX [IX_Notifications_RecipientId_IsRead] ON [Notifications] ([RecipientId], [IsRead]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE INDEX [IX_ProfileComments_AuthorId] ON [ProfileComments] ([AuthorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE INDEX [IX_ProfileComments_ProfileOwnerId] ON [ProfileComments] ([ProfileOwnerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE INDEX [IX_ReadingHistories_BookId] ON [ReadingHistories] ([BookId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE INDEX [IX_ReadingHistories_ReaderId] ON [ReadingHistories] ([ReaderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE INDEX [IX_Reservations_BookId] ON [Reservations] ([BookId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE INDEX [IX_Reservations_BooksId] ON [Reservations] ([BooksId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE INDEX [IX_Reservations_ReaderId] ON [Reservations] ([ReaderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE INDEX [IX_Reservations_ReadersId] ON [Reservations] ([ReadersId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE INDEX [IX_SupportMessages_SupportTicketId] ON [SupportMessages] ([SupportTicketId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE INDEX [IX_SupportTickets_AssignedEmployeeId] ON [SupportTickets] ([AssignedEmployeeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE INDEX [IX_SupportTickets_ReaderId] ON [SupportTickets] ([ReaderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SupportTickets_TicketCode] ON [SupportTickets] ([TicketCode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729044112_initialcreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260729044112_initialcreate', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729142038_AddProfileTagPins'
)
BEGIN
    CREATE TABLE [ProfileTagPins] (
        [Id] int NOT NULL IDENTITY,
        [ProfileOwnerId] int NOT NULL,
        [AuthorId] int NOT NULL,
        [TagKeysJson] nvarchar(150) NOT NULL,
        [PresetMessageKey] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ProfileTagPins] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProfileTagPins_Readers_AuthorId] FOREIGN KEY ([AuthorId]) REFERENCES [Readers] ([Id]),
        CONSTRAINT [FK_ProfileTagPins_Readers_ProfileOwnerId] FOREIGN KEY ([ProfileOwnerId]) REFERENCES [Readers] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729142038_AddProfileTagPins'
)
BEGIN
    CREATE INDEX [IX_ProfileTagPins_AuthorId] ON [ProfileTagPins] ([AuthorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729142038_AddProfileTagPins'
)
BEGIN
    CREATE INDEX [IX_ProfileTagPins_ProfileOwnerId] ON [ProfileTagPins] ([ProfileOwnerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729142038_AddProfileTagPins'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260729142038_AddProfileTagPins', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730012051_RenameSupportTicketAssignedEmployeeToAssignedStaff'
)
BEGIN
    ALTER TABLE [SupportTickets] DROP CONSTRAINT [FK_SupportTickets_Employees_AssignedEmployeeId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730012051_RenameSupportTicketAssignedEmployeeToAssignedStaff'
)
BEGIN
    EXEC sp_rename N'[SupportTickets].[AssignedEmployeeId]', N'AssignedStaffId', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730012051_RenameSupportTicketAssignedEmployeeToAssignedStaff'
)
BEGIN
    EXEC sp_rename N'[SupportTickets].[IX_SupportTickets_AssignedEmployeeId]', N'IX_SupportTickets_AssignedStaffId', 'INDEX';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730012051_RenameSupportTicketAssignedEmployeeToAssignedStaff'
)
BEGIN
    ALTER TABLE [SupportTickets] ADD CONSTRAINT [FK_SupportTickets_Readers_AssignedStaffId] FOREIGN KEY ([AssignedStaffId]) REFERENCES [Readers] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730012051_RenameSupportTicketAssignedEmployeeToAssignedStaff'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260730012051_RenameSupportTicketAssignedEmployeeToAssignedStaff', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021918_AddIsVerifiedToReaders'
)
BEGIN
    ALTER TABLE [Readers] ADD [IsVerified] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730021918_AddIsVerifiedToReaders'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260730021918_AddIsVerifiedToReaders', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730041246_AddPhanQuyenNhanVien'
)
BEGIN
    ALTER TABLE [Employees] ADD [RoleId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730041246_AddPhanQuyenNhanVien'
)
BEGIN
    CREATE TABLE [Permissions] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(50) NOT NULL,
        [DisplayName] nvarchar(100) NOT NULL,
        [GroupName] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_Permissions] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730041246_AddPhanQuyenNhanVien'
)
BEGIN
    CREATE TABLE [Roles] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(50) NOT NULL,
        [Description] nvarchar(200) NULL,
        CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730041246_AddPhanQuyenNhanVien'
)
BEGIN
    CREATE TABLE [UserPermissions] (
        [Id] int NOT NULL IDENTITY,
        [EmployeeId] int NOT NULL,
        [PermissionId] int NOT NULL,
        [IsGranted] bit NOT NULL,
        CONSTRAINT [PK_UserPermissions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserPermissions_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_UserPermissions_Permissions_PermissionId] FOREIGN KEY ([PermissionId]) REFERENCES [Permissions] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730041246_AddPhanQuyenNhanVien'
)
BEGIN
    CREATE TABLE [RolePermissions] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] int NOT NULL,
        [PermissionId] int NOT NULL,
        CONSTRAINT [PK_RolePermissions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RolePermissions_Permissions_PermissionId] FOREIGN KEY ([PermissionId]) REFERENCES [Permissions] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_RolePermissions_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730041246_AddPhanQuyenNhanVien'
)
BEGIN
    CREATE INDEX [IX_Employees_RoleId] ON [Employees] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730041246_AddPhanQuyenNhanVien'
)
BEGIN
    CREATE INDEX [IX_RolePermissions_PermissionId] ON [RolePermissions] ([PermissionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730041246_AddPhanQuyenNhanVien'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RolePermissions_RoleId_PermissionId] ON [RolePermissions] ([RoleId], [PermissionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730041246_AddPhanQuyenNhanVien'
)
BEGIN
    CREATE UNIQUE INDEX [IX_UserPermissions_EmployeeId_PermissionId] ON [UserPermissions] ([EmployeeId], [PermissionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730041246_AddPhanQuyenNhanVien'
)
BEGIN
    CREATE INDEX [IX_UserPermissions_PermissionId] ON [UserPermissions] ([PermissionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730041246_AddPhanQuyenNhanVien'
)
BEGIN
    ALTER TABLE [Employees] ADD CONSTRAINT [FK_Employees_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730041246_AddPhanQuyenNhanVien'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260730041246_AddPhanQuyenNhanVien', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730054042_UpdateRolesAndPermissions'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260730054042_UpdateRolesAndPermissions', N'10.0.9');
END;

COMMIT;
GO



-- =========================================================================
-- PHẦN 2: DỮ LIỆU MẪU CHUẨN HÓA (SEED DATA TOÀN DIỆN CHO CÁC BẢNG CHÍNH)
-- =========================================================================


-- -------------------------------------------------------------
-- Dữ liệu bảng: Roles (Tổng cộng: 5 bản ghi)
-- -------------------------------------------------------------
SET IDENTITY_INSERT [Roles] ON;
GO
INSERT INTO [Roles] ([Id], [Name], [Description]) VALUES
    (1, N'Admin', N'Quản trị viên hệ thống - Đủ mọi quyền'),
    (2, N'Quản lý thư viện', N'Quản lý toàn bộ hoạt động thư viện'),
    (3, N'Thủ thư', N'Quản lý sách và kho'),
    (4, N'Nhân viên kho', N'Quản lý nhập xuất kho'),
    (6, N'Chăm sóc khách hàng', N'Hỗ trợ độc giả và xử lý mượn trả');
GO
SET IDENTITY_INSERT [Roles] OFF;
GO


-- -------------------------------------------------------------
-- Dữ liệu bảng: Positions (Tổng cộng: 5 bản ghi)
-- -------------------------------------------------------------
SET IDENTITY_INSERT [Positions] ON;
GO
INSERT INTO [Positions] ([Id], [Name], [Description], [CanManageBooks], [CanManageUsers], [CanManageLoans], [CanManageReports], [CanManageSystem], [CreatedAt], [UpdatedAt]) VALUES
    (1, N'Admin', N'Quáº£n trá»‹ viÃªn há»‡ thá»‘ng', 1, 1, 1, 1, 1, '2026-11-06 13:40:07.402', NULL),
    (2, N'Quản lý thư viện', N'GiÃ¡m sÃ¡t váº­n hÃ nh vÃ  duyá»‡t tÃ i liá»‡u', 1, 0, 1, 0, 0, '2026-11-06 13:40:07.402', NULL),
    (3, N'Thủ thư', N'Quáº£n lÃ½ danh má»¥c sÃ¡ch vÃ  xá»­ lÃ½ mÆ°á»£n tráº£ táº¡i quáº§y', 0, 1, 1, 0, 0, '2026-11-06 13:40:07.402', NULL),
    (4, N'Nhân viên kho', N'Quáº£n lÃ½ ká»‡ sÃ¡ch, kiá»ƒm kÃª vÃ  phÃ¢n bá»• vá»‹ trÃ­', 0, 0, 0, 1, 0, '2026-11-06 13:40:07.402', NULL),
    (6, N'Chăm sóc khách hàng', N'Há»— trá»£ báº¡n Ä‘á»c vÃ  trá»±c kÃªnh tÆ° váº¥n', 0, 1, 1, 0, 0, '2026-11-08 17:14:40.550', NULL);
GO
SET IDENTITY_INSERT [Positions] OFF;
GO


-- -------------------------------------------------------------
-- Dữ liệu bảng: Permissions (Tổng cộng: 60 bản ghi)
-- -------------------------------------------------------------
SET IDENTITY_INSERT [Permissions] ON;
GO
INSERT INTO [Permissions] ([Id], [Code], [DisplayName], [GroupName]) VALUES
    (1, N'dashboard.view', N'Xem tổng quan', N'Tổng quan'),
    (2, N'book.view', N'Xem sách', N'Quản lý sách'),
    (3, N'book.create', N'Thêm sách', N'Quản lý sách'),
    (4, N'book.edit', N'Sửa sách', N'Quản lý sách'),
    (5, N'book.delete', N'Xóa sách', N'Quản lý sách'),
    (6, N'category.view', N'Xem danh mục', N'Danh mục sách'),
    (7, N'category.create', N'Thêm danh mục', N'Danh mục sách'),
    (8, N'category.edit', N'Sửa danh mục', N'Danh mục sách'),
    (9, N'category.delete', N'Xóa danh mục', N'Danh mục sách'),
    (10, N'author.view', N'Xem tác giả', N'Tác giả'),
    (11, N'author.create', N'Thêm tác giả', N'Tác giả'),
    (12, N'author.edit', N'Sửa tác giả', N'Tác giả'),
    (13, N'author.delete', N'Xóa tác giả', N'Tác giả'),
    (14, N'publisher.view', N'Xem nhà xuất bản', N'Nhà xuất bản'),
    (15, N'publisher.create', N'Thêm nhà xuất bản', N'Nhà xuất bản'),
    (16, N'publisher.edit', N'Sửa nhà xuất bản', N'Nhà xuất bản'),
    (17, N'publisher.delete', N'Xóa nhà xuất bản', N'Nhà xuất bản'),
    (18, N'shelf.view', N'Xem kệ sách', N'Kệ sách'),
    (19, N'shelf.create', N'Thêm kệ sách', N'Kệ sách'),
    (20, N'shelf.edit', N'Sửa kệ sách', N'Kệ sách'),
    (21, N'shelf.delete', N'Xóa kệ sách', N'Kệ sách'),
    (22, N'reader.view', N'Xem độc giả', N'Độc giả'),
    (23, N'reader.create', N'Thêm độc giả', N'Độc giả'),
    (24, N'reader.edit', N'Sửa độc giả', N'Độc giả'),
    (25, N'reader.delete', N'Xóa độc giả', N'Độc giả'),
    (26, N'employee.view', N'Xem nhân viên', N'Nhân viên'),
    (27, N'employee.create', N'Thêm nhân viên', N'Nhân viên'),
    (28, N'employee.edit', N'Sửa nhân viên', N'Nhân viên'),
    (29, N'employee.delete', N'Xóa nhân viên', N'Nhân viên'),
    (30, N'employee.assign_role', N'Phân quyền nhân viên', N'Nhân viên'),
    (31, N'borrow.view', N'Xem phiếu mượn', N'Phiếu mượn sách'),
    (32, N'borrow.create', N'Tạo phiếu mượn', N'Phiếu mượn sách'),
    (33, N'borrow.edit', N'Sửa phiếu mượn', N'Phiếu mượn sách'),
    (34, N'borrow.delete', N'Xóa phiếu mượn', N'Phiếu mượn sách'),
    (35, N'borrow.confirm_return', N'Xác nhận trả', N'Phiếu mượn sách'),
    (36, N'reservation.view', N'Xem đơn đặt trước', N'Đơn đặt trước'),
    (37, N'reservation.create', N'Tạo đơn đặt trước', N'Đơn đặt trước'),
    (38, N'reservation.edit', N'Sửa đơn đặt trước', N'Đơn đặt trước'),
    (39, N'reservation.delete', N'Xóa đơn đặt trước', N'Đơn đặt trước'),
    (40, N'reservation.confirm', N'Xác nhận đặt trước', N'Đơn đặt trước'),
    (41, N'reservation.cancel', N'Hủy đặt trước', N'Đơn đặt trước'),
    (42, N'fine.view', N'Xem phạt', N'Quản lý phạt tiền'),
    (43, N'fine.create', N'Tạo phiếu phạt', N'Quản lý phạt tiền'),
    (44, N'fine.edit', N'Sửa phiếu phạt', N'Quản lý phạt tiền'),
    (45, N'fine.delete', N'Xóa phiếu phạt', N'Quản lý phạt tiền'),
    (46, N'fine.process_payment', N'Xử lý thanh toán', N'Quản lý phạt tiền'),
    (47, N'notification.view', N'Xem thông báo', N'Thông báo'),
    (48, N'notification.create', N'Tạo thông báo', N'Thông báo'),
    (49, N'notification.edit', N'Sửa thông báo', N'Thông báo'),
    (50, N'notification.delete', N'Xóa thông báo', N'Thông báo');
GO
INSERT INTO [Permissions] ([Id], [Code], [DisplayName], [GroupName]) VALUES
    (51, N'notification.send', N'Gửi thông báo', N'Thông báo'),
    (52, N'support.view', N'Xem yêu cầu hỗ trợ', N'Hỗ trợ'),
    (53, N'support.create', N'Tạo yêu cầu hỗ trợ', N'Hỗ trợ'),
    (54, N'support.edit', N'Sửa yêu cầu hỗ trợ', N'Hỗ trợ'),
    (55, N'support.delete', N'Xóa yêu cầu hỗ trợ', N'Hỗ trợ'),
    (56, N'support.reply', N'Phản hồi yêu cầu', N'Hỗ trợ'),
    (57, N'reader_page.view', N'Xem trang độc giả', N'Trang độc giả'),
    (58, N'reader_page.borrow', N'Đặt mượn sách', N'Trang độc giả'),
    (59, N'reader_page.comment', N'Bình luận sách', N'Trang độc giả'),
    (60, N'reader_page.favorite', N'Yêu thích sách', N'Trang độc giả');
GO
SET IDENTITY_INSERT [Permissions] OFF;
GO


-- -------------------------------------------------------------
-- Dữ liệu bảng: RolePermissions (Tổng cộng: 142 bản ghi)
-- -------------------------------------------------------------
SET IDENTITY_INSERT [RolePermissions] ON;
GO
INSERT INTO [RolePermissions] ([Id], [RoleId], [PermissionId]) VALUES
    (1, 1, 1),
    (2, 1, 2),
    (3, 1, 3),
    (4, 1, 4),
    (5, 1, 5),
    (6, 1, 6),
    (7, 1, 7),
    (8, 1, 8),
    (9, 1, 9),
    (10, 1, 10),
    (11, 1, 11),
    (12, 1, 12),
    (13, 1, 13),
    (14, 1, 14),
    (15, 1, 15),
    (16, 1, 16),
    (17, 1, 17),
    (18, 1, 18),
    (19, 1, 19),
    (20, 1, 20),
    (21, 1, 21),
    (22, 1, 22),
    (23, 1, 23),
    (24, 1, 24),
    (25, 1, 25),
    (26, 1, 26),
    (27, 1, 27),
    (28, 1, 28),
    (29, 1, 29),
    (30, 1, 30),
    (31, 1, 31),
    (32, 1, 32),
    (33, 1, 33),
    (34, 1, 34),
    (35, 1, 35),
    (36, 1, 36),
    (37, 1, 37),
    (38, 1, 38),
    (39, 1, 39),
    (40, 1, 40),
    (41, 1, 41),
    (42, 1, 42),
    (43, 1, 43),
    (44, 1, 44),
    (45, 1, 45),
    (46, 1, 46),
    (47, 1, 47),
    (48, 1, 48),
    (49, 1, 49),
    (50, 1, 50);
GO
INSERT INTO [RolePermissions] ([Id], [RoleId], [PermissionId]) VALUES
    (51, 1, 51),
    (52, 1, 52),
    (53, 1, 53),
    (54, 1, 54),
    (55, 1, 55),
    (56, 1, 56),
    (57, 1, 57),
    (58, 1, 58),
    (59, 1, 59),
    (60, 1, 60),
    (272, 2, 1),
    (273, 2, 2),
    (274, 2, 6),
    (275, 2, 10),
    (276, 2, 14),
    (277, 2, 18),
    (278, 2, 22),
    (280, 2, 31),
    (281, 2, 36),
    (282, 2, 42),
    (283, 2, 47),
    (284, 2, 51),
    (285, 2, 52),
    (286, 2, 57),
    (287, 2, 58),
    (288, 2, 59),
    (289, 2, 60),
    (290, 3, 1),
    (291, 3, 2),
    (292, 3, 3),
    (293, 3, 4),
    (294, 3, 6),
    (295, 3, 7),
    (296, 3, 8),
    (297, 3, 10),
    (298, 3, 11),
    (299, 3, 12),
    (300, 3, 14),
    (301, 3, 15),
    (302, 3, 16),
    (357, 3, 18),
    (358, 3, 20),
    (303, 3, 22),
    (304, 3, 31),
    (305, 3, 32),
    (306, 3, 33),
    (307, 3, 35),
    (308, 3, 36),
    (309, 3, 37),
    (310, 3, 38);
GO
INSERT INTO [RolePermissions] ([Id], [RoleId], [PermissionId]) VALUES
    (311, 3, 40),
    (312, 3, 41),
    (359, 3, 42),
    (360, 3, 43),
    (361, 3, 44),
    (313, 3, 57),
    (314, 3, 58),
    (315, 3, 59),
    (316, 3, 60),
    (317, 4, 1),
    (318, 4, 2),
    (356, 4, 3),
    (319, 4, 4),
    (362, 4, 6),
    (363, 4, 10),
    (364, 4, 14),
    (320, 4, 18),
    (321, 4, 19),
    (322, 4, 20),
    (323, 4, 21),
    (324, 4, 57),
    (325, 4, 58),
    (326, 4, 59),
    (327, 4, 60),
    (338, 6, 1),
    (339, 6, 22),
    (340, 6, 23),
    (341, 6, 24),
    (342, 6, 47),
    (343, 6, 48),
    (344, 6, 49),
    (345, 6, 50),
    (346, 6, 51),
    (347, 6, 52),
    (348, 6, 53),
    (349, 6, 54),
    (350, 6, 55),
    (351, 6, 56),
    (352, 6, 57),
    (353, 6, 58),
    (354, 6, 59),
    (355, 6, 60);
GO
SET IDENTITY_INSERT [RolePermissions] OFF;
GO


-- -------------------------------------------------------------
-- Dữ liệu bảng: EmployeeStatusOptions (Tổng cộng: 3 bản ghi)
-- -------------------------------------------------------------
SET IDENTITY_INSERT [EmployeeStatusOptions] ON;
GO
INSERT INTO [EmployeeStatusOptions] ([Id], [Name], [Description], [DisplayOrder], [CreatedAt], [UpdatedAt]) VALUES
    (1, N'Đang làm việc', N'Nhân viên đang hoạt động bình thường', 1, '2026-11-06 13:40:07.432', NULL),
    (2, N'Tạm ngưng', N'Nhân viên tạm thời nghỉ việc', 2, '2026-11-06 13:40:07.432', NULL),
    (3, N'Đã nghỉ việc', N'Nhân viên đã thôi việc', 3, '2026-11-06 13:40:07.432', NULL);
GO
SET IDENTITY_INSERT [EmployeeStatusOptions] OFF;
GO


-- -------------------------------------------------------------
-- Dữ liệu bảng: Categories (Tổng cộng: 30 bản ghi)
-- -------------------------------------------------------------
SET IDENTITY_INSERT [Categories] ON;
GO
INSERT INTO [Categories] ([Id], [Name], [Description], [CreatedDate], [UpdatedDate], [IsActive]) VALUES
    (1, N'Kỹ năng sống', N'Phát triển bản thân', '2026-11-06 13:40:07.046', NULL, 1),
    (2, N'Kinh doanh', NULL, '2026-11-06 13:40:07.047', NULL, 1),
    (3, N'Công nghệ thông tin', NULL, '2026-11-06 13:40:07.047', NULL, 1),
    (4, N'Văn học', NULL, '2026-11-06 13:40:07.047', NULL, 1),
    (5, N'Tiểu thuyết', NULL, '2026-11-06 13:40:07.047', NULL, 1),
    (6, N'Fantasy', NULL, '2026-11-06 13:40:07.047', NULL, 1),
    (7, N'Khoa học', NULL, '2026-11-06 13:40:07.047', NULL, 1),
    (8, N'Lịch sử', NULL, '2026-11-06 13:40:07.047', NULL, 1),
    (9, N'Thiếu nhi', NULL, '2026-11-06 13:40:07.047', NULL, 1),
    (10, N'Tâm lý học', NULL, '2026-11-06 13:40:07.047', NULL, 1),
    (11, N'Trinh thám', NULL, '2026-11-06 13:40:07.047', NULL, 1),
    (12, N'Lập trình', NULL, '2026-11-06 13:40:07.047', NULL, 1),
    (13, N'DevOps', NULL, '2026-11-06 13:40:07.047', NULL, 1),
    (14, N'AI', NULL, '2026-11-06 13:40:07.047', NULL, 1),
    (15, N'Machine Learning', NULL, '2026-11-06 13:40:07.047', NULL, 1),
    (16, N'Web Development', NULL, '2026-11-06 13:40:07.047', NULL, 1),
    (17, N'Database', NULL, '2026-11-06 13:40:07.047', NULL, 1),
    (18, N'Microservices', NULL, '2026-11-06 13:40:07.047', NULL, 1),
    (19, N'Architecture', NULL, '2026-11-06 13:40:07.047', NULL, 1),
    (20, N'Lập trình hướng đối tượng', NULL, '2026-11-06 13:40:07.047', NULL, 1),
    (21, N'Giải thuật', NULL, '2026-11-06 13:40:07.047', NULL, 1),
    (22, N'Hệ điều hành', NULL, '2026-11-06 13:40:07.047', NULL, 1),
    (23, N'Mạng máy tính', NULL, '2026-11-06 13:40:07.047', NULL, 1),
    (24, N'Bảo mật', NULL, '2026-11-06 13:40:07.047', NULL, 1),
    (25, N'Thiết kế phần mềm', NULL, '2026-11-06 13:40:07.047', NULL, 1),
    (26, N'Quản lý dự án', NULL, '2026-11-06 13:40:07.047', NULL, 1),
    (27, N'Kỹ năng mềm', NULL, '2026-11-06 13:40:07.047', NULL, 1),
    (28, N'Lịch sử công nghệ', NULL, '2026-11-06 13:40:07.047', NULL, 1),
    (29, N'Khoa học máy tính', NULL, '2026-11-06 13:40:07.047', NULL, 1),
    (30, N'Khoa học tự nhiên', NULL, '2026-11-06 13:40:07.047', NULL, 1);
GO
SET IDENTITY_INSERT [Categories] OFF;
GO


-- -------------------------------------------------------------
-- Dữ liệu bảng: Authors (Tổng cộng: 94 bản ghi)
-- -------------------------------------------------------------
SET IDENTITY_INSERT [Authors] ON;
GO
INSERT INTO [Authors] ([Id], [Name], [DateOfBirth], [Nationality], [Biography], [IsDeleted], [CreatedDate], [UpdatedDate], [IsActive]) VALUES
    (1, N'Dale Carnegie', '1888-11-24 00:00:00.000', N'Hoa Ká»³', N'Dale Carnegie (1888 - 1955) lÃ  tÃ¡c gia vÃ  nhÃ  thuyáº¿t trÃ¬nh lá»—i láº¡c ngÆ°á»i Má»¹, cha Ä‘áº» cá»§a cÃ¡c phÆ°Æ¡ng phÃ¡p phÃ¡t triá»ƒn ká»¹ nÄƒng á»©ng xá»­ xÃ£ há»™i. Ã”ng lÃ  tÃ¡c giáº£ kiá»‡t tÃ¡c "Äáº¯c NhÃ¢n TÃ¢m" (How to Win Friends and Influence People) - tÃ¡c pháº©m ká»¹ nÄƒng sá»‘ng bÃ¡n cháº¡y nháº¥t má»i thá»i Ä‘áº¡i vá»›i hÆ¡n 30 triá»‡u báº£n trÃªn tháº¿ giá»›i, vÃ  cuá»‘n "Quáº³ng GÃ¡nh Lo Äi VÃ  Vui Sá»‘ng".', 0, '2026-11-06 13:40:07.257', NULL, 1),
    (2, N'Paulo Coelho', '1947-08-24 00:00:00.000', N'Brazil', N'Paulo Coelho (sinh nÄƒm 1947) lÃ  tiá»ƒu thuyáº¿t gia huyá»n thoáº¡i ngÆ°á»i Brazil. TÃ¡c pháº©m xuáº¥t sáº¯c nháº¥t cá»§a Ã´ng, "NhÃ  Giáº£ Kim" (The Alchemist), Ä‘Ã£ Ä‘Æ°á»£c dá»‹ch sang hÆ¡n 80 thá»© tiáº¿ng vÃ  bÃ¡n ra hÆ¡n 150 triá»‡u báº£n, truyá»n cáº£m há»©ng mÃ£nh liá»‡t vá» hÃ nh trÃ¬nh theo Ä‘uá»•i Æ°á»›c mÆ¡ vÃ  tiáº¿ng gá»i con tim.', 0, '2026-11-06 13:40:07.257', NULL, 1),
    (3, N'James Clear', '1986-01-22 00:00:00.000', N'Hoa Ká»³', N'James Clear (sinh nÄƒm 1986) lÃ  tÃ¡c giáº£ vÃ  chuyÃªn gia hÃ ng Ä‘áº§u tháº¿ giá»›i vá» tÃ¢m lÃ½ hÃ nh vi vÃ  thÃ³i quen. TÃ¡c pháº©m "Atomic Habits" (Thay Äá»•i TÃ­ Hon, Hiá»‡u Quáº£ Báº¥t Ngá») giá»¯ vá»‹ trÃ­ Best-seller nhiá»u nÄƒm liÃªn tiáº¿p trÃªn The New York Times, giÃºp hÃ ng triá»‡u Ä‘á»™c giáº£ thiáº¿t láº­p há»‡ thá»‘ng thÃ³i quen xuáº¥t sáº¯c.', 0, '2026-11-06 13:40:07.257', NULL, 1),
    (4, N'Robert C. Martin', '1952-12-05 00:00:00.000', N'Hoa Ká»³', N'Robert Cecil Martin (sinh nÄƒm 1952), thÆ°á»ng Ä‘Æ°á»£c biáº¿t Ä‘áº¿n lÃ  "Uncle Bob", lÃ  chuyÃªn gia pháº§n má»m ká»³ cá»±u, ngÆ°á»i Ä‘á»“ng sÃ¡ng láº­p TuyÃªn ngÃ´n Agile vÃ  bá»™ nguyÃªn lÃ½ thiáº¿t káº¿ SOLID. Ã”ng lÃ  tÃ¡c giáº£ cÃ¡c cuá»‘n sÃ¡ch gá»‘i Ä‘áº§u giÆ°á»ng cá»§a láº­p trÃ¬nh viÃªn tháº¿ giá»›i: "Clean Code", "Clean Architecture", "The Clean Coder".', 0, '2026-11-06 13:40:07.257', NULL, 1),
    (5, N'Yuval Noah Harari', '1976-02-24 00:00:00.000', N'Israel', N'Yuval Noah Harari (sinh nÄƒm 1976) lÃ  nhÃ  sá»­ há»c, giÃ¡o sÆ° táº¡i Äáº¡i há»c Hebrew Jerusalem. Ã”ng ná»•i tiáº¿ng toÃ n cáº§u vá»›i bá»™ ba tÃ¡c pháº©m lá»‹ch sá»­ - tÆ°Æ¡ng lai Ä‘á»“ sá»™: "Sapiens: LÆ°á»£c Sá»­ LoÃ i NgÆ°á»i", "Homo Deus" vÃ  "21 BÃ i Há»c Cho Tháº¿ Ká»· 21", mang Ä‘áº¿n gÃ³c nhÃ¬n sÃ¢u sáº¯c vá» tiáº¿n trÃ¬nh tiáº¿n hÃ³a vÃ  vÄƒn minh nhÃ¢n loáº¡i.', 0, '2026-11-06 13:40:07.257', NULL, 1),
    (6, N'Robert Kiyosaki', '1947-04-08 00:00:00.000', N'Hoa Ká»³', N'Robert Toru Kiyosaki (sinh nÄƒm 1947) lÃ  doanh nhÃ¢n, nhÃ  Ä‘áº§u tÆ° vÃ  tÃ¡c giáº£ tÃ i chÃ­nh ngÆ°á»i Má»¹ gá»‘c Nháº­t. Bá»™ sÃ¡ch "Dáº¡y Con LÃ m GiÃ u" (Rich Dad Poor Dad) cá»§a Ã´ng Ä‘Ã£ thay Ä‘á»•i tÆ° duy tÃ i chÃ­nh cá»§a hÃ ng triá»‡u ngÆ°á»i kháº¯p tháº¿ giá»›i thÃ´ng qua mÃ´ hÃ¬nh kim tá»© Ä‘á»“ vÃ  tÆ° duy tá»± do tÃ i chÃ­nh.', 0, '2026-11-06 13:40:07.257', NULL, 1),
    (7, N'Napoleon Hill', '1883-10-26 00:00:00.000', N'Hoa Ká»³', N'Oliver Napoleon Hill (1883 - 1970) lÃ  tÃ¡c giáº£ cÃ³ táº§m áº£nh hÆ°á»Ÿng lá»›n nháº¥t lá»‹ch sá»­ trong thá»ƒ loáº¡i vÄƒn há»c truyá»n cáº£m há»©ng thÃ nh cÃ´ng. TÃ¡c pháº©m "Think and Grow Rich" (NghÄ© GiÃ u VÃ  LÃ m GiÃ u, 1937) Ä‘Æ°á»£c Ã´ng Ä‘Ãºc káº¿t tá»« 20 nÄƒm nghiÃªn cá»©u vÃ  phá»ng váº¥n hÆ¡n 500 vÄ© nhÃ¢n nÆ°á»›c Má»¹ nhÆ° Thomas Edison, Henry Ford.', 0, '2026-11-06 13:40:07.257', NULL, 1),
    (8, N'George Orwell', '1903-06-25 00:00:00.000', N'VÆ°Æ¡ng quá»‘c Anh', N'George Orwell (1903 - 1950, tÃªn tháº­t Eric Arthur Blair) lÃ  nhÃ  vÄƒn, nhÃ  bÃ¡o vÃ  nhÃ  phÃª bÃ¬nh vÄƒn há»c lá»—i láº¡c ngÆ°á»i Anh tháº¿ ká»· 20. Ã”ng lÃ  tÃ¡c giáº£ cá»§a hai kiá»‡t tÃ¡c kinh Ä‘iá»ƒn nhÃ¢n loáº¡i: "1984" vÃ  "Tráº¡i SÃºc Váº­t" (Animal Farm), Ä‘Æ°a ra nhá»¯ng chiÃªm nghiá»‡m sÃ¢u sáº¯c vá» tá»± do tÆ° tÆ°á»Ÿng vÃ  xÃ£ há»™i.', 0, '2026-11-06 13:40:07.257', NULL, 1),
    (9, N'J.K. Rowling', '1965-07-31 00:00:00.000', N'VÆ°Æ¡ng quá»‘c Anh', N'J.K. Rowling (sinh nÄƒm 1965, tÃªn tháº­t Joanne Rowling) lÃ  ná»¯ vÄƒn sÄ© ngÆ°á»i Anh, tÃ¡c giáº£ cá»§a bá»™ tiá»ƒu thuyáº¿t ká»³ áº£o "Harry Potter". TÃ¡c pháº©m bÃ¡n hÆ¡n 600 triá»‡u báº£n trÃªn toÃ n cáº§u, Ä‘Æ°á»£c dá»‹ch ra gáº§n 90 thá»© tiáº¿ng vÃ  trá»Ÿ thÃ nh má»™t trong nhá»¯ng hiá»‡n tÆ°á»£ng vÄƒn há»c lá»›n nháº¥t má»i thá»i Ä‘áº¡i.', 0, '2026-11-06 13:40:07.257', NULL, 1),
    (10, N'Haruki Murakami', '1949-01-12 00:00:00.000', N'Nháº­t Báº£n', N'Haruki Murakami (sinh nÄƒm 1949) lÃ  má»™t trong nhá»¯ng tiá»ƒu thuyáº¿t gia Ä‘Æ°Æ¡ng Ä‘áº¡i ná»•i tiáº¿ng nháº¥t cá»§a vÄƒn há»c Nháº­t Báº£n vÃ  tháº¿ giá»›i. Phong cÃ¡ch viáº¿t cá»§a Ã´ng káº¿t há»£p Ä‘á»™c Ä‘Ã¡o giá»¯a hiá»‡n thá»±c huyá»n áº£o, Ã¢m nháº¡c vÃ  triáº¿t lÃ½ cÃ´ Ä‘Æ¡n, thá»ƒ hiá»‡n qua cÃ¡c kiá»‡t tÃ¡c "Rá»«ng Na Uy", "Kafka BÃªn Bá» Biá»ƒn", "1Q84".', 0, '2026-11-06 13:40:07.257', NULL, 1),
    (11, N'Nguyễn Nhật Ánh', NULL, N'Vietnam', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (12, N'Tô Hoài', NULL, N'Vietnam', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (13, N'Rosie Nguyễn', NULL, N'Vietnam', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (14, N'Stephen R. Covey', '1932-10-24 00:00:00.000', N'Hoa Ká»³', N'Stephen Richards Covey (1932 - 2012) lÃ  chuyÃªn gia quáº£n trá»‹ vÃ  phÃ¡t triá»ƒn lÃ£nh Ä‘áº¡o ná»•i tiáº¿ng cá»§a Má»¹. Cuá»‘n sÃ¡ch kinh Ä‘iá»ƒn "The 7 Habits of Highly Effective People" (7 ThÃ³i Quen Cá»§a NgÆ°á»i ThÃ nh Äáº¡t) cá»§a Ã´ng Ä‘Ã£ bÃ¡n hÆ¡n 40 triá»‡u báº£n vÃ  Ä‘Æ°á»£c táº¡p chÃ­ Forbes xáº¿p vÃ o nhÃ³m cÃ¡c cuá»‘n sÃ¡ch quáº£n trá»‹ cÃ³ áº£nh hÆ°á»Ÿng nháº¥t tháº¿ ká»· 20.', 0, '2026-11-06 13:40:07.257', NULL, 1),
    (15, N'Mark Manson', NULL, N'USA', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (16, N'Cal Newport', NULL, N'USA', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (17, N'Martin Fowler', NULL, N'United Kingdom', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (18, N'Eric Evans', NULL, N'USA', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (19, N'Kent Beck', NULL, N'USA', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (20, N'Donald Knuth', NULL, N'USA', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (21, N'Andrew Hunt', NULL, N'USA', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (22, N'David Thomas', NULL, N'USA', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (23, N'Joshua Bloch', NULL, N'USA', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (24, N'Steve McConnell', NULL, N'USA', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (25, N'Martin Kleppmann', NULL, N'United Kingdom', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (26, N'Erich Gamma', NULL, N'Switzerland', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (27, N'Richard Helm', NULL, N'Australia', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (28, N'Ralph Johnson', NULL, N'USA', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (29, N'John Vlissides', NULL, N'USA', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (30, N'Gene Kim', NULL, N'USA', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (31, N'Jez Humble', NULL, N'United Kingdom', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (32, N'Sam Newman', NULL, N'United Kingdom', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (33, N'Thomas H. Cormen', NULL, N'USA', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (34, N'Charles E. Leiserson', NULL, N'USA', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (35, N'Ronald Rivest', NULL, N'USA', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (36, N'Clifford Stein', NULL, N'USA', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (37, N'Andrew S. Tanenbaum', NULL, N'Netherlands', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (38, N'Herbert Schildt', NULL, N'USA', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (39, N'Ian Goodfellow', NULL, N'USA', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (40, N'Aurélien Géron', NULL, N'France', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (41, N'Andrew Ng', NULL, N'USA', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (42, N'Stephen Hawking', '1942-01-08 00:00:00.000', N'VÆ°Æ¡ng quá»‘c Anh', N'Stephen William Hawking (1942 - 2018) lÃ  nhÃ  váº­t lÃ½ lÃ½ thuyáº¿t, nhÃ  vÅ© trá»¥ há»c vÃ  tÃ¡c giáº£ ngÆ°á»i Anh vÄ© Ä‘áº¡i. DÃ¹ máº¯c cÄƒn bá»‡nh xÆ¡ cá»©ng teo cÆ¡ tá»« tráº», Ã´ng Ä‘Ã£ cá»‘ng hiáº¿n trá»n Ä‘á»i cho khoa há»c vá»›i lÃ½ thuyáº¿t há»‘ Ä‘en vÃ  cuá»‘n sÃ¡ch phá»• biáº¿n khoa há»c kinh Ä‘iá»ƒn "LÆ°á»£c Sá»­ Thá»i Gian" (A Brief History of Time).', 0, '2026-11-06 13:40:07.257', NULL, 1),
    (43, N'Carl Sagan', NULL, N'USA', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (44, N'Daniel Kahneman', '1934-03-05 00:00:00.000', N'Israel', N'Daniel Kahneman (1934 - 2024) lÃ  nhÃ  tÃ¢m lÃ½ há»c lá»—i láº¡c ngÆ°á»i Má»¹ gá»‘c Israel Ä‘oáº¡t giáº£i Nobel Kinh táº¿ nÄƒm 2002. Cuá»‘n sÃ¡ch kiá»‡t tÃ¡c "Thinking, Fast and Slow" (TÆ° Duy Nhanh VÃ  Cháº­m) cá»§a Ã´ng lÃ  cÃ´ng trÃ¬nh kinh Ä‘iá»ƒn vá» tÃ¢m lÃ½ há»c hÃ nh vi vÃ  kinh táº¿ há»c nháº­n thá»©c.', 0, '2026-11-06 13:40:07.257', NULL, 1),
    (45, N'Robert Greene', NULL, N'USA', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (46, N'Chris Voss', NULL, N'USA', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (47, N'Dan Brown', '1964-06-22 00:00:00.000', N'Hoa Ká»³', N'Dan Brown (sinh nÄƒm 1964) lÃ  tÃ¡c giáº£ tiá»ƒu thuyáº¿t ly ká»³ bÃ­ áº©n ná»•i tiáº¿ng toÃ n cáº§u ngÆ°á»i Má»¹. Ã”ng lÃ  cha Ä‘áº» cá»§a cÃ¡c tÃ¡c pháº©m cháº¥n Ä‘á»™ng vÄƒn Ä‘Ã n káº¿t há»£p nghá»‡ thuáº­t, biá»ƒu tÆ°á»£ng há»c vÃ  máº­t mÃ£ há»c nhÆ° "Máº­t MÃ£ Da Vinci" (The Da Vinci Code), "ThiÃªn Tháº§n VÃ  Ãc Quá»·", "Há»a Ngá»¥c".', 0, '2026-11-06 13:40:07.257', NULL, 1),
    (48, N'J.R.R. Tolkien', NULL, N'United Kingdom', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (49, N'Antoine de Saint-Exupéry', '1900-06-29 00:00:00.000', N'PhÃ¡p', N'Antoine de Saint-ExupÃ©ry (1900 - 1944) lÃ  nhÃ  vÄƒn vÃ  phi cÃ´ng tiÃªn phong ngÆ°á»i PhÃ¡p. TÃ¡c pháº©m truyá»‡n ngá»¥ ngÃ´n báº¥t há»§ "HoÃ ng Tá»­ BÃ©" (Le Petit Prince) cá»§a Ã´ng lÃ  má»™t trong nhá»¯ng cuá»‘n sÃ¡ch Ä‘Æ°á»£c dá»‹ch nhiá»u nháº¥t vÃ  yÃªu thÃ­ch nháº¥t trong lá»‹ch sá»­ vÄƒn há»c tháº¿ giá»›i.', 0, '2026-11-06 13:40:07.257', NULL, 1),
    (50, N'Jane Austen', NULL, N'United Kingdom', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1);
GO
INSERT INTO [Authors] ([Id], [Name], [DateOfBirth], [Nationality], [Biography], [IsDeleted], [CreatedDate], [UpdatedDate], [IsActive]) VALUES
    (51, N'F. Scott Fitzgerald', NULL, N'USA', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (52, N'Jeff Sutherland', NULL, N'USA', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (53, N'Michael Feathers', NULL, N'USA', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (54, N'Kathy Sierra', NULL, N'USA', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (55, N'Frederick Brooks Jr.', NULL, N'USA', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (56, N'Seymour Cray', NULL, N'USA', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (57, N'Grace Hopper', NULL, N'USA', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (58, N'Tim Berners-Lee', NULL, N'United Kingdom', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (59, N'Linus Torvalds', NULL, N'USA', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (60, N'Dennis Ritchie', NULL, N'USA', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (61, N'Ken Thompson', NULL, N'USA', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (62, N'Bjarne Stroustrup', NULL, N'Denmark', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (63, N'Guido van Rossum', NULL, N'Netherlands', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (64, N'Brendan Eich', NULL, N'USA', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (65, N'Rasmus Lerdorf', NULL, N'USA', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (66, N'John Sonmez', NULL, N'USA', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (67, N'Robert Sedgewick', NULL, N'USA', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (68, N'Stuart Russell', NULL, N'United Kingdom', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (69, N'Eric Matthes', NULL, N'USA', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (70, N'Kalen Delaney', NULL, N'USA', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (71, N'Nigel Poulton', NULL, N'United Kingdom', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (72, N'Marko Luksa', NULL, N'Slovenia', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (73, N'Chris Richardson', NULL, N'USA', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (74, N'Héctor García', NULL, N'Spain', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (75, N'Microsoft', NULL, N'USA', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (76, N'GoF (Gang of Four)', NULL, N'Various', NULL, 0, '2026-11-06 13:40:07.257', NULL, 1),
    (81, N'Nam Cao', '1915-10-29 00:00:00.000', N'Viá»‡t Nam', N'Nam Cao (1915 - 1951, tÃªn tháº­t Tráº§n Há»¯u Tri) lÃ  nhÃ  vÄƒn hiá»‡n thá»±c phÃª phÃ¡n xuáº¥t sáº¯c nháº¥t cá»§a vÄƒn há»c Viá»‡t Nam tháº¿ ká»· 20, Liá»‡t sÄ© khÃ¡ng chiáº¿n chá»‘ng PhÃ¡p. CÃ¡c tÃ¡c pháº©m kinh Ä‘iá»ƒn cá»§a Ã´ng nhÆ° "ChÃ­ PhÃ¨o", "LÃ£o Háº¡c", "Äá»i Thá»«a", "Sá»‘ng MÃ²n" Ä‘Ã£ kháº¯c há»a sÃ¢u sáº¯c bi ká»‹ch con ngÆ°á»i vÃ  Ä‘á» cao nhÃ¢n pháº©m.', 0, '2026-11-07 19:27:49.283', NULL, 1),
    (83, N'Ernest Hemingway', '1899-07-21 00:00:00.000', N'Hoa Ká»³', N'Ernest Miller Hemingway (1899 - 1961) lÃ  Ä‘áº¡i vÄƒn hÃ o ngÆ°á»i Má»¹ Ä‘oáº¡t giáº£i Nobel VÄƒn há»c nÄƒm 1954 vÃ  giáº£i Pulitzer nÄƒm 1953. Vá»›i nguyÃªn lÃ½ "Táº£ng bÄƒng trÃ´i" ná»•i tiáº¿ng, Ã´ng Ä‘Ã£ sÃ¡ng táº¡o nÃªn nhá»¯ng kiá»‡t tÃ¡c báº¥t há»§ nhÆ° "Ã”ng GiÃ  VÃ  Biá»ƒn Cáº£", "ChuÃ´ng Nguyá»‡n Há»“n Ai", "GiÃ£ Tá»« VÅ© KhÃ­".', 0, '2026-11-07 19:27:49.283', NULL, 1),
    (84, N'Hector Malot', '1830-05-20 00:00:00.000', N'PhÃ¡p', N'Hector Malot (1830 - 1907) lÃ  nhÃ  vÄƒn PhÃ¡p ná»•i tiáº¿ng vá»›i nhá»¯ng tÃ¡c pháº©m giÃ u tÃ­nh nhÃ¢n vÄƒn vÃ  giÃ¡o dá»¥c thiáº¿u nhi. Hai kiá»‡t tÃ¡c Ä‘á»ƒ Ä‘á»i cá»§a Ã´ng lÃ  "KhÃ´ng Gia ÄÃ¬nh" (Sans Famille) vÃ  "Trong Gia ÄÃ¬nh" (En Famille) Ä‘Ã£ sÆ°á»Ÿi áº¥m tÃ¢m há»“n bao tháº¿ há»‡ Ä‘á»™c giáº£ vá» nghá»‹ lá»±c phi thÆ°á»ng vÃ  lÃ²ng nhÃ¢n Ã¡i.', 0, '2026-11-07 19:27:49.283', NULL, 1),
    (85, N'Victor Hugo', '1802-02-26 00:00:00.000', N'PhÃ¡p', N'Victor Hugo (1802 - 1885) lÃ  Ä‘áº¡i thi hÃ o, tiá»ƒu thuyáº¿t gia vÃ  chÃ­nh khÃ¡ch kiá»‡t xuáº¥t ngÆ°á»i PhÃ¡p, ngá»n cá» Ä‘áº§u cá»§a chá»§ nghÄ©a lÃ£ng máº¡n. Ã”ng lÃ  tÃ¡c giáº£ cá»§a cÃ¡c thiÃªn trÆ°á»ng tiá»ƒu thuyáº¿t nhÃ¢n Ä‘áº¡o báº¥t há»§ "Nhá»¯ng NgÆ°á»i Khá»‘n Khá»•" (Les MisÃ©rables) vÃ  "NhÃ  Thá» Äá»©c BÃ  Paris" (Notre-Dame de Paris).', 0, '2026-11-07 19:27:49.283', NULL, 1),
    (86, N'Arthur Conan Doyle', '1859-05-22 00:00:00.000', N'VÆ°Æ¡ng quá»‘c Anh', N'Sir Arthur Conan Doyle (1859 - 1930) lÃ  nhÃ  vÄƒn, bÃ¡c sÄ© ngÆ°á»i Scotland. Ã”ng Ä‘Ã£ sÃ¡ng táº¡o nÃªn nhÃ¢n váº­t thÃ¡m tá»­ lá»«ng danh nháº¥t lá»‹ch sá»­ vÄƒn há»c "Sherlock Holmes" cÃ¹ng ngÆ°á»i báº¡n thÃ¢n bÃ¡c sÄ© Watson, táº¡o nÃªn tÆ°á»£ng Ä‘Ã i báº¥t há»§ cá»§a thá»ƒ loáº¡i truyá»‡n trinh thÃ¡m tháº¿ giá»›i.', 0, '2026-11-07 19:27:49.283', NULL, 1),
    (88, N'Andrea Hirata', '1967-10-24 00:00:00.000', N'Indonesia', N'Andrea Hirata (sinh nÄƒm 1967) lÃ  nhÃ  vÄƒn danh tiáº¿ng ngÆ°á»i Indonesia. Cuá»‘n tiá»ƒu thuyáº¿t Ä‘áº§u tay "Chiáº¿n Binh Cáº§u Vá»“ng" (Laskar Pelangi) cá»§a Ã´ng dá»±a trÃªn kÃ½ á»©c tuá»•i thÆ¡ nghÃ¨o khÃ³ trÃªn Ä‘áº£o Belitung Ä‘Ã£ phÃ¡ vá»¡ má»i ká»· lá»¥c xuáº¥t báº£n táº¡i Indonesia, Ä‘Æ°á»£c dá»‹ch sang hÆ¡n 30 thá»© tiáº¿ng vÃ  truyá»n cáº£m há»©ng mÃ£nh liá»‡t vá» sá»©c máº¡nh cá»§a giÃ¡o dá»¥c.', 0, '2026-11-07 19:27:49.283', NULL, 1),
    (91, N'Vũ Trọng Phụng', NULL, N'Việt Nam', N'Ông vua phóng sự đất Bắc, nhà văn hiện thực phê phán xuất sắc nhất văn học Việt Nam thời tiền chiến (1912 - 1939)', 0, '2026-11-07 19:44:29.930', NULL, 1),
    (92, N'Ngô Tất Tố', NULL, N'Việt Nam', N'Nhà văn, nhà báo, nhà nghiên cứu văn hóa hiện thực xuất sắc với kiệt tác Tắt Đèn (1894 - 1954)', 0, '2026-11-07 19:44:29.930', NULL, 1),
    (93, N'Nam Cao', '1915-10-29 00:00:00.000', N'Viá»‡t Nam', N'Nam Cao (1915 - 1951, tÃªn tháº­t Tráº§n Há»¯u Tri) lÃ  nhÃ  vÄƒn hiá»‡n thá»±c phÃª phÃ¡n xuáº¥t sáº¯c nháº¥t cá»§a vÄƒn há»c Viá»‡t Nam tháº¿ ká»· 20, Liá»‡t sÄ© khÃ¡ng chiáº¿n chá»‘ng PhÃ¡p. CÃ¡c tÃ¡c pháº©m kinh Ä‘iá»ƒn cá»§a Ã´ng nhÆ° "ChÃ­ PhÃ¨o", "LÃ£o Háº¡c", "Äá»i Thá»«a", "Sá»‘ng MÃ²n" Ä‘Ã£ kháº¯c há»a sÃ¢u sáº¯c bi ká»‹ch con ngÆ°á»i vÃ  Ä‘á» cao nhÃ¢n pháº©m.', 0, '2026-11-07 19:44:29.930', NULL, 1),
    (94, N'Nguyễn Du', NULL, N'Việt Nam', N'Đại thi hào dân tộc, Danh nhân văn hóa thế giới, tác giả kiệt tác Đoạn Trường Tân Thanh - Truyện Kiều (1765 - 1820)', 0, '2026-11-07 19:44:29.930', NULL, 1),
    (95, N'Ernest Hemingway', '1899-07-21 00:00:00.000', N'Hoa Ká»³', N'Ernest Miller Hemingway (1899 - 1961) lÃ  Ä‘áº¡i vÄƒn hÃ o ngÆ°á»i Má»¹ Ä‘oáº¡t giáº£i Nobel VÄƒn há»c nÄƒm 1954 vÃ  giáº£i Pulitzer nÄƒm 1953. Vá»›i nguyÃªn lÃ½ "Táº£ng bÄƒng trÃ´i" ná»•i tiáº¿ng, Ã´ng Ä‘Ã£ sÃ¡ng táº¡o nÃªn nhá»¯ng kiá»‡t tÃ¡c báº¥t há»§ nhÆ° "Ã”ng GiÃ  VÃ  Biá»ƒn Cáº£", "ChuÃ´ng Nguyá»‡n Há»“n Ai", "GiÃ£ Tá»« VÅ© KhÃ­".', 0, '2026-11-07 19:44:29.930', NULL, 1),
    (96, N'Hector Malot', '1830-05-20 00:00:00.000', N'PhÃ¡p', N'Hector Malot (1830 - 1907) lÃ  nhÃ  vÄƒn PhÃ¡p ná»•i tiáº¿ng vá»›i nhá»¯ng tÃ¡c pháº©m giÃ u tÃ­nh nhÃ¢n vÄƒn vÃ  giÃ¡o dá»¥c thiáº¿u nhi. Hai kiá»‡t tÃ¡c Ä‘á»ƒ Ä‘á»i cá»§a Ã´ng lÃ  "KhÃ´ng Gia ÄÃ¬nh" (Sans Famille) vÃ  "Trong Gia ÄÃ¬nh" (En Famille) Ä‘Ã£ sÆ°á»Ÿi áº¥m tÃ¢m há»“n bao tháº¿ há»‡ Ä‘á»™c giáº£ vá» nghá»‹ lá»±c phi thÆ°á»ng vÃ  lÃ²ng nhÃ¢n Ã¡i.', 0, '2026-11-07 19:44:29.930', NULL, 1),
    (97, N'Victor Hugo', '1802-02-26 00:00:00.000', N'PhÃ¡p', N'Victor Hugo (1802 - 1885) lÃ  Ä‘áº¡i thi hÃ o, tiá»ƒu thuyáº¿t gia vÃ  chÃ­nh khÃ¡ch kiá»‡t xuáº¥t ngÆ°á»i PhÃ¡p, ngá»n cá» Ä‘áº§u cá»§a chá»§ nghÄ©a lÃ£ng máº¡n. Ã”ng lÃ  tÃ¡c giáº£ cá»§a cÃ¡c thiÃªn trÆ°á»ng tiá»ƒu thuyáº¿t nhÃ¢n Ä‘áº¡o báº¥t há»§ "Nhá»¯ng NgÆ°á»i Khá»‘n Khá»•" (Les MisÃ©rables) vÃ  "NhÃ  Thá» Äá»©c BÃ  Paris" (Notre-Dame de Paris).', 0, '2026-11-07 19:44:29.930', NULL, 1),
    (98, N'Arthur Conan Doyle', '1859-05-22 00:00:00.000', N'VÆ°Æ¡ng quá»‘c Anh', N'Sir Arthur Conan Doyle (1859 - 1930) lÃ  nhÃ  vÄƒn, bÃ¡c sÄ© ngÆ°á»i Scotland. Ã”ng Ä‘Ã£ sÃ¡ng táº¡o nÃªn nhÃ¢n váº­t thÃ¡m tá»­ lá»«ng danh nháº¥t lá»‹ch sá»­ vÄƒn há»c "Sherlock Holmes" cÃ¹ng ngÆ°á»i báº¡n thÃ¢n bÃ¡c sÄ© Watson, táº¡o nÃªn tÆ°á»£ng Ä‘Ã i báº¥t há»§ cá»§a thá»ƒ loáº¡i truyá»‡n trinh thÃ¡m tháº¿ giá»›i.', 0, '2026-11-07 19:44:29.930', NULL, 1),
    (99, N'Tôn Vũ', NULL, N'Trung Quốc', N'Danh tướng, nhà quân sự, nhà triết học lỗi lạc thời Xuân Thu ở Trung Quốc, tác giả Binh Pháp Tôn Tử', 0, '2026-11-07 19:44:29.930', NULL, 1),
    (100, N'Andrea Hirata', '1967-10-24 00:00:00.000', N'Indonesia', N'Andrea Hirata (sinh nÄƒm 1967) lÃ  nhÃ  vÄƒn danh tiáº¿ng ngÆ°á»i Indonesia. Cuá»‘n tiá»ƒu thuyáº¿t Ä‘áº§u tay "Chiáº¿n Binh Cáº§u Vá»“ng" (Laskar Pelangi) cá»§a Ã´ng dá»±a trÃªn kÃ½ á»©c tuá»•i thÆ¡ nghÃ¨o khÃ³ trÃªn Ä‘áº£o Belitung Ä‘Ã£ phÃ¡ vá»¡ má»i ká»· lá»¥c xuáº¥t báº£n táº¡i Indonesia, Ä‘Æ°á»£c dá»‹ch sang hÆ¡n 30 thá»© tiáº¿ng vÃ  truyá»n cáº£m há»©ng mÃ£nh liá»‡t vá» sá»©c máº¡nh cá»§a giÃ¡o dá»¥c.', 0, '2026-11-07 19:44:29.930', NULL, 1),
    (101, N'Kim Lân', NULL, N'Việt Nam', N'Cây bút tài hoa của làng quê Việt Nam với những truyện ngắn kinh điển như Vợ Nhặt, Làng (1920 - 2007)', 0, '2026-11-07 19:44:29.930', NULL, 1),
    (102, N'Nguyễn Minh Châu', NULL, N'Việt Nam', N'Nhà văn tiền phong của công cuộc đổi mới văn học Việt Nam, tác giả Chiếc Thuyền Ngoài Xa (1930 - 1989)', 0, '2026-11-07 19:44:29.930', NULL, 1);
GO
SET IDENTITY_INSERT [Authors] OFF;
GO


-- -------------------------------------------------------------
-- Dữ liệu bảng: Publishers (Tổng cộng: 42 bản ghi)
-- -------------------------------------------------------------
SET IDENTITY_INSERT [Publishers] ON;
GO
INSERT INTO [Publishers] ([Id], [Name], [Address], [Phone], [Email], [Website], [IsDeleted], [CreatedDate], [UpdatedDate], [IsActive]) VALUES
    (1, N'NXB Trẻ', NULL, NULL, NULL, NULL, 0, '2026-11-06 13:40:07.321', NULL, 1),
    (2, N'NXB Kim Đồng', NULL, NULL, NULL, NULL, 0, '2026-11-06 13:40:07.321', NULL, 1),
    (3, N'NXB Giáo Dục', NULL, NULL, NULL, NULL, 0, '2026-11-06 13:40:07.321', NULL, 1),
    (4, N'NXB Lao Động', NULL, NULL, NULL, NULL, 0, '2026-11-06 13:40:07.321', NULL, 1),
    (5, N'NXB Văn Học', NULL, NULL, NULL, NULL, 0, '2026-11-06 13:40:07.321', NULL, 1),
    (6, N'NXB Hội Nhà Văn', NULL, NULL, NULL, NULL, 0, '2026-11-06 13:40:07.321', NULL, 1),
    (7, N'NXB Tổng Hợp', NULL, NULL, NULL, NULL, 0, '2026-11-06 13:40:07.321', NULL, 1),
    (8, N'NXB Thế Giới', NULL, NULL, NULL, NULL, 0, '2026-11-06 13:40:07.321', NULL, 1),
    (9, N'O''Reilly', NULL, NULL, NULL, NULL, 0, '2026-11-06 13:40:07.321', NULL, 1),
    (10, N'Packt', NULL, NULL, NULL, NULL, 0, '2026-11-06 13:40:07.321', NULL, 1),
    (11, N'Manning', NULL, NULL, NULL, NULL, 0, '2026-11-06 13:40:07.321', NULL, 1),
    (12, N'Pearson', NULL, NULL, NULL, NULL, 0, '2026-11-06 13:40:07.321', NULL, 1),
    (13, N'Addison Wesley', NULL, NULL, NULL, NULL, 0, '2026-11-06 13:40:07.321', NULL, 1),
    (14, N'Penguin', NULL, NULL, NULL, NULL, 0, '2026-11-06 13:40:07.321', NULL, 1),
    (15, N'Harper Collins', NULL, NULL, NULL, NULL, 0, '2026-11-06 13:40:07.321', NULL, 1),
    (16, N'Coursera Press', NULL, NULL, NULL, NULL, 0, '2026-11-06 13:40:07.321', NULL, 1),
    (17, N'Independently Published', NULL, NULL, NULL, NULL, 0, '2026-11-06 13:40:07.321', NULL, 1),
    (18, N'Microsoft Press', NULL, NULL, NULL, NULL, 0, '2026-11-06 13:40:07.321', NULL, 1),
    (19, N'Prentice Hall', NULL, NULL, NULL, NULL, 0, '2026-11-06 13:40:07.321', NULL, 1),
    (20, N'No Starch Press', NULL, NULL, NULL, NULL, 0, '2026-11-06 13:40:07.321', NULL, 1),
    (21, N'IT Revolution', NULL, NULL, NULL, NULL, 0, '2026-11-06 13:40:07.321', NULL, 1),
    (22, N'O''Reilly Media', NULL, NULL, NULL, NULL, 0, '2026-11-06 13:40:07.321', NULL, 1),
    (23, N'Apress', NULL, NULL, NULL, NULL, 0, '2026-11-06 13:40:07.321', NULL, 1),
    (24, N'Wiley', NULL, NULL, NULL, NULL, 0, '2026-11-06 13:40:07.321', NULL, 1),
    (25, N'Springer', NULL, NULL, NULL, NULL, 0, '2026-11-06 13:40:07.321', NULL, 1),
    (26, N'Cambridge University Press', NULL, NULL, NULL, NULL, 0, '2026-11-06 13:40:07.321', NULL, 1),
    (27, N'MIT Press', NULL, NULL, NULL, NULL, 0, '2026-11-06 13:40:07.321', NULL, 1),
    (28, N'Pearson Education', NULL, NULL, NULL, NULL, 0, '2026-11-06 13:40:07.321', NULL, 1),
    (29, N'Bantam', NULL, NULL, NULL, NULL, 0, '2026-11-06 13:40:07.321', NULL, 1),
    (30, N'Scribner', NULL, NULL, NULL, NULL, 0, '2026-11-06 13:40:07.321', NULL, 1),
    (31, N'Doubleday', NULL, NULL, NULL, NULL, 0, '2026-11-06 13:40:07.321', NULL, 1),
    (37, N'NXB Tổng Hợp TP.HCM', N'62 Nguyá»…n Thá»‹ Minh Khai, Q.1, TP.HCM', N'02838225340', N'tonghop@nxbhcm.com.vn', N'https://nxbhcm.com.vn', 0, '2026-11-07 19:28:39.726', NULL, 1),
    (39, N'NXB Tri Thức', N'53 Nguyá»…n Du, Hai BÃ  TrÆ°ng, HÃ  Ná»™i', N'02439447239', N'lienhe@nxbtrithuc.com.vn', N'https://nxbtrithuc.com.vn', 0, '2026-11-07 19:28:39.726', NULL, 1),
    (41, N'NXB Quân Đội Nhân Dân', N'23 LÃ½ Nam Äáº¿, HoÃ n Kiáº¿m, HÃ  Ná»™i', N'02437472097', N'nxbqdnd@gmail.com', N'https://nxbqdnd.com.vn', 0, '2026-11-07 19:28:39.726', NULL, 1),
    (42, N'Penguin Random House', N'1745 Broadway, New York, NY 10019, USA', N'+12127829000', N'ecustomerservice@penguinrandomhouse.com', N'https://penguinrandomhouse.com', 0, '2026-11-07 19:28:39.726', NULL, 1),
    (43, N'O''Reilly Media', N'1005 Gravenstein Hwy N, Sebastopol, CA 95472, USA', N'+17078277000', N'support@oreilly.com', N'https://oreilly.com', 0, '2026-11-07 19:28:39.726', NULL, 1),
    (44, N'Bloomsbury Publishing', N'50 Bedford Square, London WC1B 3DP, UK', N'+442076315600', N'contact@bloomsbury.com', N'https://bloomsbury.com', 0, '2026-11-07 19:28:39.726', NULL, 1),
    (45, N'Addison-Wesley Professional', N'Boston, Massachusetts, USA', N'+18009220579', N'info@pearson.com', N'https://informit.com', 0, '2026-11-07 19:28:39.726', NULL, 1),
    (46, N'Prentice Hall', N'Upper Saddle River, New Jersey, USA', N'+18009220579', N'support@pearson.com', N'https://pearson.com', 0, '2026-11-07 19:28:39.726', NULL, 1),
    (47, N'Microsoft Press', N'One Microsoft Way, Redmond, WA 98052, USA', N'+14258828080', N'mspress@microsoft.com', N'https://microsoftpressstore.com', 0, '2026-11-07 19:28:39.726', NULL, 1),
    (48, N'No Starch Press', N'245 8th St, San Francisco, CA 94103, USA', N'+14158639900', N'info@nostarch.com', N'https://nostarch.com', 0, '2026-11-07 19:28:39.726', NULL, 1),
    (49, N'Manning Publications', N'209 Bruce Run Rd, Shelter Island, NY 11964, USA', N'+16317490000', N'support@manning.com', N'https://manning.com', 0, '2026-11-07 19:28:39.726', NULL, 1);
GO
SET IDENTITY_INSERT [Publishers] OFF;
GO


-- -------------------------------------------------------------
-- Dữ liệu bảng: Shelves (Tổng cộng: 36 bản ghi)
-- -------------------------------------------------------------
SET IDENTITY_INSERT [Shelves] ON;
GO
INSERT INTO [Shelves] ([Id], [Code], [LocationCode], [Name], [Area], [Description], [Capacity], [CreatedDate], [UpdatedDate], [IsActive]) VALUES
    (1, N'A1', NULL, N'Kệ A1', N'Tầng 1', NULL, 300, '2026-11-06 13:40:07.361', NULL, 1),
    (2, N'A2', NULL, N'Kệ A2', N'Tầng 1', NULL, 300, '2026-11-06 13:40:07.362', NULL, 1),
    (3, N'A3', NULL, N'Kệ A3', N'Tầng 1', NULL, 300, '2026-11-06 13:40:07.362', NULL, 1),
    (4, N'B1', NULL, N'Kệ B1', N'Tầng 2', NULL, 300, '2026-11-06 13:40:07.362', NULL, 1),
    (5, N'B2', NULL, N'Kệ B2', N'Tầng 2', NULL, 300, '2026-11-06 13:40:07.362', NULL, 1),
    (6, N'B3', NULL, N'Kệ B3', N'Tầng 2', NULL, 300, '2026-11-06 13:40:07.362', NULL, 1),
    (7, N'C1', NULL, N'Kệ C1', N'Tầng 3', NULL, 300, '2026-11-06 13:40:07.362', NULL, 1),
    (8, N'C2', NULL, N'Kệ C2', N'Tầng 3', NULL, 300, '2026-11-06 13:40:07.362', NULL, 1),
    (9, N'C3', NULL, N'Kệ C3', N'Tầng 3', NULL, 300, '2026-11-06 13:40:07.362', NULL, 1),
    (10, N'D1', NULL, N'Kệ D1', N'Tầng 4', NULL, 300, '2026-11-06 13:40:07.362', NULL, 1),
    (11, N'D2', NULL, N'Kệ D2', N'Tầng 4', NULL, 300, '2026-11-06 13:40:07.362', NULL, 1),
    (12, N'D3', NULL, N'Kệ D3', N'Tầng 4', NULL, 300, '2026-11-06 13:40:07.362', NULL, 1),
    (13, N'E1', NULL, N'Kệ E1', N'Tầng 5', NULL, 300, '2026-11-06 13:40:07.362', NULL, 1),
    (14, N'E2', NULL, N'Kệ E2', N'Tầng 5', NULL, 300, '2026-11-06 13:40:07.362', NULL, 1),
    (15, N'E3', NULL, N'Kệ E3', N'Tầng 5', NULL, 300, '2026-11-06 13:40:07.362', NULL, 1),
    (16, N'F1', NULL, N'Kệ F1', N'Tầng 6', NULL, 300, '2026-11-06 13:40:07.362', NULL, 1),
    (17, N'F2', NULL, N'Kệ F2', N'Tầng 6', NULL, 300, '2026-11-06 13:40:07.362', NULL, 1),
    (18, N'F3', NULL, N'Kệ F3', N'Tầng 6', NULL, 300, '2026-11-06 13:40:07.362', NULL, 1),
    (19, N'G1', NULL, N'Kệ G1', N'Tầng 7', NULL, 300, '2026-11-06 13:40:07.362', NULL, 1),
    (20, N'G2', NULL, N'Kệ G2', N'Tầng 7', NULL, 300, '2026-11-06 13:40:07.362', NULL, 1),
    (21, N'G3', NULL, N'Kệ G3', N'Tầng 7', NULL, 300, '2026-11-06 13:40:07.362', NULL, 1),
    (22, N'H1', NULL, N'Kệ H1', N'Tầng 8', NULL, 300, '2026-11-06 13:40:07.362', NULL, 1),
    (23, N'H2', NULL, N'Kệ H2', N'Tầng 8', NULL, 300, '2026-11-06 13:40:07.362', NULL, 1),
    (24, N'H3', NULL, N'Kệ H3', N'Tầng 8', NULL, 300, '2026-11-06 13:40:07.362', NULL, 1),
    (25, N'I1', NULL, N'Kệ I1', N'Tầng 9', NULL, 300, '2026-11-06 13:40:07.362', NULL, 1),
    (26, N'I2', NULL, N'Kệ I2', N'Tầng 9', NULL, 300, '2026-11-06 13:40:07.362', NULL, 1),
    (27, N'I3', NULL, N'Kệ I3', N'Tầng 9', NULL, 300, '2026-11-06 13:40:07.362', NULL, 1),
    (28, N'J1', NULL, N'Kệ J1', N'Tầng 10', NULL, 300, '2026-11-06 13:40:07.362', NULL, 1),
    (29, N'J2', NULL, N'Kệ J2', N'Tầng 10', NULL, 300, '2026-11-06 13:40:07.362', NULL, 1),
    (30, N'J3', NULL, N'Kệ J3', N'Tầng 10', NULL, 300, '2026-11-06 13:40:07.362', NULL, 1),
    (31, N'K1', NULL, N'Kệ K1', N'Tầng 11', NULL, 300, '2026-11-06 13:40:07.362', NULL, 1),
    (32, N'K2', NULL, N'Kệ K2', N'Tầng 11', NULL, 300, '2026-11-06 13:40:07.362', NULL, 1),
    (33, N'K3', NULL, N'Kệ K3', N'Tầng 11', NULL, 300, '2026-11-06 13:40:07.362', NULL, 1),
    (34, N'L1', NULL, N'Kệ L1', N'Tầng 12', NULL, 300, '2026-11-06 13:40:07.362', NULL, 1),
    (35, N'L2', NULL, N'Kệ L2', N'Tầng 12', NULL, 300, '2026-11-06 13:40:07.362', NULL, 1),
    (36, N'L3', NULL, N'Kệ L3', N'Tầng 12', NULL, 300, '2026-11-06 13:40:07.362', NULL, 1);
GO
SET IDENTITY_INSERT [Shelves] OFF;
GO


-- -------------------------------------------------------------
-- Dữ liệu bảng: Books (Tổng cộng: 68 bản ghi)
-- -------------------------------------------------------------
SET IDENTITY_INSERT [Books] ON;
GO
INSERT INTO [Books] ([Id], [ISBN], [Title], [Description], [Edition], [Keywords], [PublishYear], [CreatedBy], [PageCount], [Language], [LastBorrowDate], [Price], [Quantity], [AvailableQuantity], [MinimumAge], [CoverImage], [CategoriesId], [AuthorsId], [PublishersId], [ShelvesId], [BooksStatus], [ViewCount], [IsDeleted]) VALUES
    (1, N'978604000001', N'Đắc Nhân Tâm', N'Đắc Nhân Tâm (How to Win Friends and Influence People) của Dale Carnegie là cuốn sách kinh điển và bán chạy nhất mọi thời đại về nghệ thuật đối nhân xử thế và giao tiếp ứng xử. Tác phẩm mang đến những nguyên tắc vàng vượt thời gian để kết nối chân thành với người khác, thấu hiểu tâm lý con người, tạo dựng lòng tin và dẫn dắt người khác bằng sự tôn trọng và tình yêu thương.', N'1', N'Đắc Nhân Tâm', 1936, N'SeedData', 320, N'Tiếng Việt', '2026-11-08 01:51:24.686', 120000.00, 20, 18, 0, N'/images/books/dac-nhan-tam.jpg', 1, 1, 1, 1, 0, 35, 0),
    (2, N'978604000002', N'Nhà Giả Kim', N'Nhà Giả Kim (The Alchemist) là kiệt tác văn học thế giới của nhà văn Paulo Coelho, kể về hành trình phiêu lưu đầy huyền thoại của chàng chăn cừu Santiago đi tìm kho báu bí ẩn tại Kim Tự Tháp Ai Cập. Cuốn sách gửi gắm thông điệp sâu sắc: Khi bạn thực sự khao khát một điều gì, cả vũ trụ sẽ hợp lực giúp bạn đạt được điều đó.', N'1', N'Nhà Giả Kim', 1988, N'SeedData', 228, N'Tiếng Việt', '2026-11-08 01:51:24.686', 99000.00, 20, 17, 0, N'/images/books/nha-gia-kim.jpg', 5, 2, 7, 2, 0, 3, 0),
    (3, N'9786047761821', N'Atomic Habits', N'Atomic Habits (Thay Đổi Tí Hon, Hiệu Quả Bất Ngờ) của James Clear phân tích sâu sắc khoa học hành vi con người và đưa ra quy trình 4 bước để xây dựng thói quen tốt và loại bỏ thói quen xấu: Làm cho rõ ràng, Hấp dẫn, Dễ dàng và Thỏa mãn. Chỉ với 1% tiến bộ tích cực mỗi ngày, bạn sẽ tạo nên sự chuyển biến phi thường trong cuộc sống.', N'1', N'Atomic Habits', 2018, N'SeedData', 388, N'Tiếng Việt', '2026-11-08 01:51:24.686', 189000.00, 20, 19, 0, N'/images/books/atomic-habits.jpg', 1, 3, 8, 3, 0, 0, 0),
    (4, N'978604000004', N'Clean Code', N'Clean Code (Mã Sạch) của Robert C. Martin (Uncle Bob) là cuốn sách kinh điển dành cho mọi lập trình viên phần mềm chuyên nghiệp. Tác phẩm hướng dẫn cách viết mã nguồn dễ đọc, dễ hiểu, dễ kiểm thử và bảo trì thông qua các nguyên tắc đặt tên, thiết kế hàm, xử lý lỗi và cấu trúc lớp.', N'1', N'Clean Code', 2008, N'SeedData', 464, N'Tiếng Việt', '2026-11-08 01:51:24.686', 420000.00, 20, 18, 0, N'/images/books/clean-code.jpg', 3, 4, 13, 4, 0, 0, 0),
    (5, N'9786049459528', N'Sapiens', N'Sapiens: Lược Sử Loài Người của giáo sư Yuval Noah Harari đưa độc giả qua hành trình tiến hóa ngoạn mục của loài người từ thời kỳ đồ đá cho đến kỷ nguyên hiện đại. Tác phẩm phân tích sâu sắc cách mà khả năng tư duy trừu tượng, niềm tin tập thể, cuộc cách mạng nông nghiệp và khoa học đã đưa Homo sapiens trở thành loài thống trị hành tinh.', N'1', N'Sapiens', 2011, N'SeedData', 560, N'Tiếng Việt', '2026-11-08 01:51:24.686', 239000.00, 20, 19, 0, N'/images/books/sapiens.jpg', 7, 5, 39, 5, 0, 3, 0),
    (6, N'978604000006', N'Cha Giàu Cha Nghèo', N'Cha Giàu Cha Nghèo (Rich Dad Poor Dad) của Robert Kiyosaki là cuốn sách tài chính cá nhân bán chạy nhất mọi thời đại, làm thay đổi tư duy tài chính của hàng triệu người trên thế giới về đầu tư, tài sản và tiêu sản.', N'1', N'Cha Giàu Cha Nghèo', 1997, N'SeedData', 320, N'Tiếng Việt', '2026-11-08 01:51:24.686', 150000.00, 20, 18, 0, N'/images/books/cha-giau-cha-ngheo.jpg', 2, 6, 1, 2, 0, 0, 0),
    (7, N'978604000007', N'Think and Grow Rich', N'Think and Grow Rich (13 Nguyên Tắc Nghĩ Giàu Làm Giàu) của Napoleon Hill là kết quả sau hơn 20 năm nghiên cứu cuộc đời và sự nghiệp của 500 cá nhân thành công nhất nước Mỹ. Cuốn sách đúc kết 13 nguyên tắc làm chủ tư duy, biến khát vọng cháy bỏng thành của cải và thành công vượt bậc.', N'1', N'Think and Grow Rich', 1937, N'SeedData', 340, N'Tiếng Việt', '2026-11-08 01:51:24.686', 145000.00, 20, 19, 0, N'/images/books/think-and-grow-rich.jpg', 2, 7, 6, 2, 0, 3, 0),
    (8, N'9786046985471', N'1984', N'1984 là kiệt tác văn học kinh điển của George Orwell khắc họa một xã hội tương lai giả tưởng đặt dưới sự giám sát toàn năng của Anh Cả (Big Brother). Tác phẩm cảnh báo sâu sắc về sự thao túng thông tin, bóp méo lịch sử, xâm phạm tự do cá nhân và sức mạnh kiểm soát tâm lý của các thể chế độc tài.', N'1', N'1984', 1949, N'SeedData', 372, N'Tiếng Việt', '2026-11-08 01:51:24.686', 125000.00, 20, 19, 0, N'/images/books/1984.jpg', 4, 8, 5, 4, 0, 2, 0),
    (9, N'978604000009', N'Harry Potter và Hòn đá Phù thủy', N'Harry Potter và Hòn đá Phù thủy là tập đầu tiên mở ra thế giới phù thủy kỳ diệu của J.K. Rowling. Cậu bé mồ côi Harry Potter khám phá ra thân thế thực sự của mình vào sinh nhật 11 tuổi, bước vào Trường Phù thủy và Pháp sư Hogwarts, kết thân với Ron và Hermione, và cùng nhau bảo vệ Hòn đá Phù thủy khỏi bàn tay của kẻ thù hắc ám.', N'1', N'Harry Potter và Hòn đá Phù thủy', 1997, N'SeedData', 350, N'Tiếng Việt', '2026-11-08 01:51:24.686', 180000.00, 20, 19, 0, N'/images/books/harry-potter-1.jpg', 6, 9, 1, 5, 0, 1, 0),
    (10, N'978604000010', N'Harry Potter và Phòng chứa Bí mật', N'Harry Potter và Phòng chứa Bí mật ghi lại năm học thứ hai đầy bí ẩn và nguy hiểm tại Hogwarts. Khi những dòng chữ nhuốm máu xuất hiện trên tường và các học sinh lần lượt bị hóa đá, Harry phải đối mặt với tiếng gọi của ngôn ngữ rắn, cuốn nhật ký ma thuật của Tom Riddle và quái vật khổng lồ ẩn nấp sâu dưới lòng đất.', N'1', N'Harry Potter và Phòng chứa Bí mật', 1998, N'SeedData', 360, N'Tiếng Việt', '2026-11-08 01:51:24.686', 180000.00, 20, 19, 0, N'/images/books/harry-potter-2.jpg', 6, 9, 1, 5, 0, 1, 0),
    (11, N'978604000011', N'Harry Potter và Tù nhân Azkaban', N'Harry Potter và Tù nhân Azkaban là phần thứ ba trong bộ tiểu thuyết giả tưởng lừng danh của J.K. Rowling. Harry bước vào năm học thứ ba tại Hogwarts cùng bí mật về Sirius Black - tù nhân vượt ngục nguy hiểm nhất Azkaban.', N'1', N'Harry Potter và Tù nhân Azkaban', 1999, N'SeedData', 420, N'Tiếng Việt', NULL, 190000.00, 20, 20, 0, N'/images/books/harry-potter-3.jpg', 6, 9, 1, 5, 0, 0, 0),
    (12, N'978604000012', N'Harry Potter và Chiếc cốc lửa', N'Harry Potter và Chiếc cốc lửa cuốn hút độc giả vào Giải đấu Tam Pháp Thuật nghẹt thở quy tụ học sinh từ ba trường phù thủy danh tiếng. Bị cuốn vào thử thách sinh tử dù chưa đủ tuổi, Harry phải vượt qua rồng lửa, người cá và mê cung ma quái, trước khi đối mặt với sự hồi sinh đen tối của Chúa tể Voldemort.', N'1', N'Harry Potter và Chiếc cốc lửa', 2000, N'SeedData', 640, N'Tiếng Việt', NULL, 230000.00, 20, 20, 0, N'/images/books/harry-potter-4.jpg', 6, 9, 1, 5, 0, 0, 0),
    (13, N'978604000013', N'Harry Potter và Hội Phượng Hoàng', N'Harry Potter và Hội Phượng Hoàng đánh dấu giai đoạn đen tối và căng thẳng khi Bộ Pháp Thuật phủ nhận sự trở lại của Voldemort và tìm cách kiểm soát trường học. Harry cùng các bạn thành lập Đoàn quân Dumbledore bí mật để rèn luyện phép phòng thủ, chuẩn bị cho cuộc chiến lớn tại Sở Bí Mật.', N'1', N'Harry Potter và Hội Phượng Hoàng', 2003, N'SeedData', 760, N'Tiếng Việt', NULL, 260000.00, 20, 20, 0, N'/images/books/harry-potter-5.jpg', 6, 9, 1, 6, 0, 0, 0),
    (14, N'978604000014', N'Harry Potter và Hoàng tử lai', N'Harry Potter và Hoàng tử lai cùng Thầy Dumbledore dẫn dắt người đọc đi sâu vào quá khứ và tâm lý đen tối của Tom Riddle để tìm ra điểm yếu chí mạng của hắn – các Trường Sinh Linh Giá. Song song đó, cuốn sách độc dược kỳ lạ của Hoàng tử lai mang đến cho Harry vô số bí thuật lẫn những hệ quả khôn lường.', N'1', N'Harry Potter và Hoàng tử lai', 2005, N'SeedData', 650, N'Tiếng Việt', NULL, 240000.00, 20, 20, 0, N'/images/books/harry-potter-6.jpg', 6, 9, 1, 6, 0, 1, 0),
    (15, N'978604000015', N'Harry Potter và Bảo bối Tử thần', N'Harry Potter và Bảo bối Tử thần là hồi kết huy hoàng và bi tráng cho toàn bộ thiên sử thi pháp thuật. Không còn sự che chở của mái trường Hogwarts, bộ ba Harry, Ron và Hermione phải lang bạt khắp nơi để truy tìm và phá hủy từng Trường Sinh Linh Giá, dẫn tới trận đại chiến định mệnh bảo vệ thế giới phù thủy.', N'1', N'Harry Potter và Bảo bối Tử thần', 2007, N'SeedData', 700, N'Tiếng Việt', NULL, 260000.00, 20, 20, 0, N'/images/books/harry-potter-7.jpg', 6, 9, 1, 6, 0, 0, 0),
    (16, N'978604000016', N'Deep Work', N'Deep Work (Làm Ra Làm, Chơi Ra Chơi) của Cal Newport định hướng khả năng tập trung cao độ không xao nhãng trong một thế giới tràn ngập phiền nhiễu số.', N'1', N'Deep Work', 2016, N'SeedData', 304, N'Tiếng Việt', NULL, 175000.00, 20, 20, 0, N'/images/books/deep-work.jpg', 1, 16, 10, 1, 0, 0, 0),
    (17, N'978604000017', N'Digital Minimalism', N'Digital Minimalism (Tối Giản Kỹ Thuật Số) của Cal Newport đưa ra triết lý sử dụng công nghệ chọn lọc, tập trung vào giá trị cốt lõi và loại bỏ phiền nhiễu.', N'1', N'Digital Minimalism', 2019, N'SeedData', 290, N'Tiếng Việt', NULL, 170000.00, 20, 20, 0, N'/images/books/digital-minimalism.jpg', 1, 16, 10, 1, 0, 0, 0),
    (18, N'978604000018', N'The 7 Habits of Highly Effective People', N'7 Thói Quen Của Người Thành Đạt (The 7 Habits of Highly Effective People) của Stephen R. Covey là một trong những cuốn sách quản trị và phát triển cá nhân có tầm ảnh hưởng lớn nhất mọi thời đại. Tác phẩm hướng dẫn quy trình chuyển biến từ phụ thuộc sang độc lập và tương thuộc lẫn nhau, giúp độc giả làm chủ cuộc đời và đạt hiệu quả đỉnh cao trong công việc.', N'1', N'The 7 Habits of Highly Effective People', 1989, N'SeedData', 430, N'Tiếng Việt', NULL, 195000.00, 20, 20, 0, N'/images/books/7habits.jpg', 1, 14, 1, 1, 0, 0, 0),
    (19, N'978604000019', N'The Subtle Art of Not Giving a F*ck', N'Nghệ Thuật Tinh Tế Của Việc "Đếch" Quan Tâm của Mark Manson mang đến một góc nhìn thực tế và tỉnh táo trước xu hướng tôn vinh sự tích cực độc hại. Cuốn sách giúp bạn nhận ra rằng cuộc sống vốn đầy rẫy khó khăn, điều quan trọng là biết chọn lọc những giá trị thực sự xứng đáng để dành tâm sức và năng lượng giải quyết.', N'1', N'The Subtle Art of Not Giving a F*ck', 2016, N'SeedData', 260, N'Tiếng Việt', NULL, 175000.00, 20, 20, 0, N'/images/books/subtle-art.jpg', 1, 15, 10, 1, 0, 0, 0),
    (20, N'978604000020', N'Kafka bên bờ biển', N'Kafka bên bờ biển của Haruki Murakami là tác phẩm hiện thực huyền ảo xuất sắc, đưa người đọc vào cuộc phiêu lưu siêu thực của cậu bé 15 tuổi Kafka Tamura.', N'1', N'Kafka bên bờ biển', 2002, N'SeedData', 520, N'Tiếng Việt', NULL, 190000.00, 20, 20, 0, N'/images/books/kafka.jpg', 4, 10, 6, 4, 0, 0, 0),
    (21, N'978604000021', N'Rừng Na Uy', N'Rừng Na Uy là kiệt tác văn học nổi tiếng nhất của Haruki Murakami. Tác phẩm đưa người đọc trôi vào dòng hồi ức man mác buồn của nhân vật Toru Watanabe về tuổi trẻ những năm 1960 ở Tokyo, giữa tình cảm giằng xé với Naoko – cô gái mang tổn thương tâm hồn sâu sắc – và Midori hoạt bát, tràn đầy sức sống.', N'1', N'Rừng Na Uy', 1987, N'SeedData', 390, N'Tiếng Việt', NULL, 170000.00, 20, 20, 0, N'/images/books/norwegian-wood.jpg', 4, 10, 6, 4, 0, 0, 0),
    (22, N'978604000022', N'Tôi thấy hoa vàng trên cỏ xanh', N'Tôi Thấy Hoa Vàng Trên Cỏ Xanh của nhà văn Nguyễn Nhật Ánh là khúc ca đồng quê trong trẻo, đưa người đọc trở về những ký ức tuổi thơ êm đềm nơi làng quê miền Trung. Tác phẩm khắc họa tình anh em máu thịt giữa Thiều và Tường, những rung động đầu đời ngây thơ và những câu chuyện dung dị nhưng ấm áp tình người.', N'1', N'Tôi thấy hoa vàng trên cỏ xanh', 2010, N'SeedData', 380, N'Tiếng Việt', NULL, 120000.00, 20, 20, 0, N'/images/books/hoa-vang.jpg', 4, 11, 1, 2, 0, 0, 0),
    (23, N'978604000023', N'Mắt Biếc', N'Mắt Biếc là một trong những tác phẩm giàu cảm xúc nhất của Nguyễn Nhật Ánh. Câu chuyện xoay quanh mối tình đơn phương thủy chung, day dứt của Ngạn dành cho cô bạn gái thanh mai trúc mã Hà Lan sở hữu đôi mắt biếc biếc. Dù dòng đời đổi thay và bao biến cố ập đến, tình yêu mộc mạc của Ngạn vẫn mãi vẹn nguyên cùng thời gian.', N'1', N'Mắt Biếc', 1990, N'SeedData', 360, N'Tiếng Việt', NULL, 115000.00, 20, 20, 0, N'/images/books/mat-biec.jpg', 4, 11, 1, 2, 0, 0, 0),
    (24, N'978604000024', N'Dế Mèn Phiêu Lưu Ký', N'Dế Mèn Phiêu Lưu Ký của nhà văn Tô Hoài là tác phẩm văn học thiếu nhi kinh điển của Việt Nam. Cuốn sách kể về hành trình vạn dặm của chú Dế Mèn dũng cảm nhưng từng bồng bột, kiêu ngạo; qua bao trải nghiệm sinh tử để trưởng thành, đúc kết bài học về lòng nhân ái, tình bạn bè và khát vọng hòa bình cho muôn loài.', N'1', N'Dế Mèn Phiêu Lưu Ký', 1941, N'SeedData', 220, N'Tiếng Việt', NULL, 85000.00, 20, 20, 0, N'/images/books/de-men.jpg', 9, 12, 2, 3, 0, 0, 0),
    (25, N'978604000025', N'Tuổi Trẻ Đáng Giá Bao Nhiêu', N'Tuổi Trẻ Đáng Giá Bao Nhiêu của tác giả Rosie Nguyễn là cuốn sách gối đầu giường của đông đảo độc giả trẻ Việt Nam. Tác phẩm chia sẻ những trải nghiệm thực tế về việc học tập suốt đời, văn hóa đọc sách, tinh thần tự lập và thói quen xách ba lô lên đi để khám phá thế giới cũng như tìm thấy sứ mệnh đích thực của bản thân.', N'1', N'Tuổi Trẻ Đáng Giá Bao Nhiêu', 2018, N'SeedData', 300, N'Tiếng Việt', NULL, 110000.00, 20, 20, 0, N'/images/books/tuoi-tre.jpg', 1, 13, 1, 1, 0, 0, 0),
    (26, N'978604000026', N'The Pragmatic Programmer', N'The Pragmatic Programmer của Andrew Hunt và David Thomas cung cấp những lời khuyên thực tế vô giá giúp lập trình viên nâng tầm tư duy và làm chủ nghề nghiệp.', N'1', N'The Pragmatic Programmer', 1999, N'SeedData', 352, N'Tiếng Việt', NULL, 420000.00, 20, 20, 0, N'/images/books/pragmatic-programmer.jpg', 3, 21, 13, 4, 0, 0, 0),
    (27, N'978604000027', N'Refactoring', N'Refactoring (Tái Cấu Trúc Mã Nguồn) của Martin Fowler cung cấp danh mục các kỹ thuật cải tiến thiết kế mã nguồn mà không làm thay đổi hành vi bên ngoài.', N'1', N'Refactoring', 1999, N'SeedData', 448, N'Tiếng Việt', NULL, 450000.00, 20, 20, 0, N'/images/books/refactoring.jpg', 3, 17, 13, 4, 0, 0, 0),
    (28, N'978604000028', N'Design Patterns', N'Design Patterns: Elements of Reusable Object-Oriented Software là tác phẩm kinh điển của Gang of Four (Erich Gamma, Richard Helm, Ralph Johnson, John Vlissides), giới thiệu 23 mẫu thiết kế hướng đối tượng chuẩn mực giúp giải quyết các bài toán kiến trúc phần mềm phức tạp một cách thanh lịch và tái sử dụng.', N'1', N'Design Patterns', 1994, N'SeedData', 395, N'Tiếng Việt', NULL, 480000.00, 20, 20, 0, N'/images/books/design-patterns.jpg', 3, 26, 13, 4, 0, 0, 0),
    (29, N'978604000029', N'Domain Driven Design', N'Domain-Driven Design của Eric Evans giới thiệu phương pháp thiết kế phần mềm xoay quanh mô hình nghiệp vụ thực tế phức tạp.', N'1', N'Domain Driven Design', 2003, N'SeedData', 560, N'Tiếng Việt', NULL, 520000.00, 20, 20, 0, N'/images/books/ddd.jpg', 3, 18, 13, 5, 0, 0, 0),
    (30, N'978604000030', N'Code Complete', N'Code Complete của Steve McConnell là cẩm nang toàn diện bậc nhất về kỹ nghệ xây dựng phần mềm chất lượng cao.', N'1', N'Code Complete', 2004, N'SeedData', 900, N'Tiếng Việt', NULL, 550000.00, 20, 20, 0, N'/images/books/code-complete.jpg', 3, 24, 18, 5, 0, 0, 0),
    (31, N'978604000031', N'Working Effectively with Legacy Code', N'Working Effectively with Legacy Code của Michael Feathers giúp các kỹ sư thuần hóa và hiện đại hóa hệ thống mã nguồn cũ.', N'1', N'Working Effectively with Legacy Code', 2004, N'SeedData', 456, N'Tiếng Việt', NULL, 470000.00, 20, 20, 0, N'/images/books/legacy-code.jpg', 3, 53, 19, 5, 0, 0, 0),
    (32, N'978604000032', N'Head First Design Patterns', N'Head First Design Patterns mang đến phong cách học tập mẫu thiết kế phần mềm hoàn toàn mới lạ thông qua hình ảnh minh họa sinh động, câu đố và tình huống thực tế hài hước. Cuốn sách giúp bạn nắm vững các nguyên lý OOP cốt lõi và các mẫu thiết kế kinh điển như Observer, Decorator, Factory, Singleton và Strategy.', N'1', N'Head First Design Patterns', 2004, N'SeedData', 694, N'Tiếng Việt', NULL, 450000.00, 20, 20, 0, N'/images/books/head-first-design-patterns.jpg', 3, 54, 9, 6, 0, 0, 0),
    (33, N'978604000033', N'Effective Java', N'Effective Java của Joshua Bloch là kim chỉ nam chứa 90 điều đúc kết giúp lập trình viên viết code Java tối ưu, an toàn và chuẩn mực.', N'1', N'Effective Java', 2018, N'SeedData', 416, N'Tiếng Việt', NULL, 460000.00, 20, 20, 0, N'/images/books/effective-java.jpg', 3, 23, 13, 6, 0, 0, 0),
    (34, N'978604000034', N'The Clean Coder', N'Clean Code (Mã Sạch) của Robert C. Martin (Uncle Bob) là cuốn sách kinh điển dành cho mọi lập trình viên phần mềm chuyên nghiệp. Tác phẩm hướng dẫn cách viết mã nguồn dễ đọc, dễ hiểu, dễ kiểm thử và bảo trì thông qua các nguyên tắc đặt tên, thiết kế hàm, xử lý lỗi và cấu trúc lớp.', N'1', N'The Clean Coder', 2011, N'SeedData', 256, N'Tiếng Việt', NULL, 350000.00, 20, 20, 0, N'/images/books/clean-coder.jpg', 3, 4, 19, 4, 0, 0, 0),
    (35, N'978604000035', N'Agile Principles', N'Agile Principles, Patterns, and Practices in C# của Robert C. Martin giải thích chi tiết các nguyên tắc cốt lõi của Agile, lập trình cực hạn (XP) và trọn bộ 5 nguyên lý SOLID nổi tiếng. Cuốn sách là tài liệu học tập không thể thiếu để xây dựng kiến trúc phần mềm linh hoạt, dễ thích ứng với mọi thay đổi.', N'1', N'Agile Principles', 2001, N'SeedData', 300, N'Tiếng Việt', NULL, 320000.00, 20, 20, 0, N'/images/books/agile.jpg', 3, 4, 19, 4, 0, 0, 0),
    (36, N'978604000036', N'Scrum', N'Scrum: Nghệ Thuật Làm Việc Gấp Đôi Trong Nửa Thời Gian của Jeff Sutherland giới thiệu khung phương pháp linh hoạt (Agile) thành công nhất thế giới.', N'1', N'Scrum', 2014, N'SeedData', 280, N'Tiếng Việt', NULL, 250000.00, 20, 20, 0, N'/images/books/scrum.jpg', 3, 52, 1, 5, 0, 0, 0),
    (37, N'978604000037', N'Soft Skills', N'Soft Skills: Cẩm Nang Kỹ Năng Mềm Dành Cho Lập Trình Viên của John Sonmez bao quát các khía cạnh then chốt ngoài chuyên môn kỹ thuật: xây dựng thương hiệu cá nhân, kỹ năng đàm phán lương, quản lý tài chính, rèn luyện thể chất và duy trì lối sống lành mạnh cho giới công nghệ.', N'1', N'Soft Skills', 2014, N'SeedData', 504, N'Tiếng Việt', NULL, 380000.00, 20, 20, 0, N'/images/books/soft-skills.jpg', 1, 66, 11, 19, 0, 0, 0),
    (38, N'978604000038', N'The Mythical Man Month', N'The Mythical Man-Month của Frederick Brooks Jr. là tác phẩm kinh điển về quản lý dự án công nghệ thông tin. Nổi tiếng với Định luật Brooks ("Thêm người vào một dự án phần mềm đang trễ chỉ làm nó trễ hơn"), cuốn sách đúc kết những bài học xương máu về giao tiếp tổ chức, kiến trúc hệ thống và quy luật vận hành dự án.', N'1', N'The Mythical Man Month', 1975, N'SeedData', 322, N'Tiếng Việt', NULL, 360000.00, 20, 20, 0, N'/images/books/mythical-man-month.jpg', 3, 55, 13, 5, 0, 0, 0),
    (39, N'978604000039', N'The Phoenix Project', N'The Phoenix Project đưa độc giả qua một câu chuyện sống động về ứng dụng DevOps và văn hóa hợp tác nhằm xoay chuyển tình thế dự án IT then chốt.', N'1', N'The Phoenix Project', 2013, N'SeedData', 432, N'Tiếng Việt', NULL, 390000.00, 20, 20, 0, N'/images/books/phoenix-project.jpg', 3, 30, 21, 6, 0, 1, 0),
    (40, N'978604000040', N'Algorithms', N'Algorithms của Robert Sedgewick và Kevin Wayne là giáo trình kinh điển về thuật toán và cấu trúc dữ liệu nền tảng.', N'1', N'Algorithms', 2009, N'SeedData', 700, N'Tiếng Việt', NULL, 500000.00, 20, 20, 0, N'/images/books/algorithms.jpg', 3, 67, 13, 6, 0, 0, 0),
    (41, N'978604000041', N'Artificial Intelligence', N'Artificial Intelligence: A Modern Approach của Stuart Russell và Peter Norvig là giáo trình AI tiêu chuẩn được giảng dạy tại hơn 1.500 trường đại học trên toàn cầu, bao quát từ thuật toán tìm kiếm, logic suy luận, học máy cho đến thị giác máy tính và xử lý ngôn ngữ tự nhiên hiện đại.', N'1', N'Artificial Intelligence', 2020, N'SeedData', 580, N'Tiếng Việt', NULL, 480000.00, 20, 20, 0, N'/images/books/artificial-intelligence.jpg', 3, 68, 12, 7, 0, 0, 0),
    (42, N'978604000042', N'Machine Learning Basics', N'Machine Learning Basics tổng hợp những kiến thức nền tảng và cốt lõi nhất về học máy: từ hồi quy tuyến tính, hồi quy logistic, mạng nơ-ron nhân tạo cho đến các thuật toán học có giám sát và học không giám sát, cung cấp nền tảng vững chắc cho các nhà nghiên cứu và kỹ sư dữ liệu.', N'1', N'Machine Learning Basics', 2021, N'SeedData', 450, N'Tiếng Việt', NULL, 420000.00, 20, 20, 0, N'/images/books/machine-learning.jpg', 3, 41, 16, 7, 0, 0, 0),
    (43, N'978604000043', N'Python Crash Course', N'Python Crash Course của Eric Matthes là cẩm nang dẫn dắt người mới bắt đầu làm quen với lập trình Python một cách nhanh chóng và hiệu quả. Cuốn sách kết hợp giữa việc nắm vững lý thuyết cú pháp cốt lõi và thực hành thông qua 3 dự án lớn: phát triển game 2D, trực quan hóa dữ liệu và xây dựng ứng dụng web với Django.', N'1', N'Python Crash Course', 2022, N'SeedData', 540, N'Tiếng Việt', NULL, 390000.00, 20, 20, 0, N'/images/books/python-crash-course.jpg', 3, 69, 20, 7, 0, 0, 0),
    (44, N'978604000044', N'Learning ASP.NET Core', N'Learning ASP.NET Core cung cấp lộ trình bài bản để xây dựng các ứng dụng web và Web API hiện đại, hiệu năng cao trên nền tảng .NET Core. Tác phẩm đi sâu vào các cơ chế then chốt như Dependency Injection, Middleware pipeline, Routing, cấu hình bảo mật với Identity và tương tác dữ liệu với Entity Framework Core.', N'1', N'Learning ASP.NET Core', 2023, N'SeedData', 600, N'Tiếng Việt', NULL, 450000.00, 20, 20, 0, N'/images/books/aspnetcore.jpg', 3, 75, 18, 8, 0, 0, 0),
    (45, N'978604000045', N'Entity Framework Core', N'Entity Framework Core là tài liệu hướng dẫn toàn diện về ORM hiện đại của nền tảng .NET: thiết kế cơ sở dữ liệu Code-First, cấu hình Fluent API, tối ưu hóa truy vấn LINQ, quản lý Migrations và xử lý giao dịch dữ liệu quy mô lớn trong các ứng dụng doanh nghiệp.', N'1', N'Entity Framework Core', 2023, N'SeedData', 420, N'Tiếng Việt', NULL, 350000.00, 20, 20, 0, N'/images/books/efcore.jpg', 3, 75, 18, 8, 0, 0, 0),
    (46, N'978604000046', N'SQL Server Internals', N'SQL Server Internals đi sâu vào kiến trúc bên trong của Microsoft SQL Server: cơ chế lưu trữ trang và phần mở rộng (Pages & Extents), cấu trúc chỉ mục B-Tree, cơ chế khóa và cô lập giao dịch (Locking & Concurrency), bộ tối ưu hóa truy vấn và kỹ thuật tinh chỉnh hiệu năng cơ sở dữ liệu chuyên sâu.', N'1', N'SQL Server Internals', 2019, N'SeedData', 720, N'Tiếng Việt', NULL, 520000.00, 20, 20, 0, N'/images/books/sql-server.jpg', 3, 70, 18, 8, 0, 0, 0),
    (47, N'978604000047', N'Docker Deep Dive', N'Docker Deep Dive là cẩm nang thực chiến toàn diện về công nghệ đóng gói container: từ kiến trúc Docker Engine, viết Dockerfile tối ưu, quản lý Image, Container, Volume, Network cho đến triển khai ứng dụng đa dịch vụ bằng Docker Compose.', N'1', N'Docker Deep Dive', 2020, N'SeedData', 380, N'Tiếng Việt', NULL, 340000.00, 20, 20, 0, N'/images/books/docker.jpg', 3, 71, 17, 8, 0, 2, 0),
    (48, N'978604000048', N'Kubernetes in Action', N'Kubernetes in Action của Marko Luksa cung cấp hướng dẫn chuyên sâu từ cơ bản đến nâng cao về điều phối container trên nền tảng Kubernetes.', N'1', N'Kubernetes in Action', 2021, N'SeedData', 640, N'Tiếng Việt', NULL, 510000.00, 20, 20, 0, N'/images/books/kubernetes.jpg', 3, 72, 11, 8, 0, 0, 0),
    (49, N'978604000049', N'Microservices Patterns', N'Microservices Patterns của Chris Richardson hướng dẫn toàn diện các mẫu kiến trúc microservices thực tế trong doanh nghiệp.', N'1', N'Microservices Patterns', 2018, N'SeedData', 520, N'Tiếng Việt', NULL, 490000.00, 20, 20, 0, N'/images/books/microservices.jpg', 3, 73, 11, 9, 0, 0, 0),
    (50, N'978604000050', N'Clean Architecture', N'Clean Architecture: Kiến Trúc Phần Mềm Sạch của Robert C. Martin đúc kết các quy tắc vàng để tạo nên những hệ thống phần mềm độc lập với framework, UI, cơ sở dữ liệu và các tác nhân bên ngoài. Cuốn sách giúp bạn thiết kế những hệ thống phần mềm có khả năng kiểm thử dễ dàng và trường tồn qua nhiều thập kỷ.', N'1', N'Clean Architecture', 2017, N'SeedData', 432, N'Tiếng Việt', NULL, 430000.00, 20, 20, 0, N'/images/books/clean-architecture.jpg', 3, 4, 19, 9, 0, 0, 0);
GO
INSERT INTO [Books] ([Id], [ISBN], [Title], [Description], [Edition], [Keywords], [PublishYear], [CreatedBy], [PageCount], [Language], [LastBorrowDate], [Price], [Quantity], [AvailableQuantity], [MinimumAge], [CoverImage], [CategoriesId], [AuthorsId], [PublishersId], [ShelvesId], [BooksStatus], [ViewCount], [IsDeleted]) VALUES
    (51, N'978604000081', N'Pride and Prejudice', N'Pride and Prejudice (Kiêu Hãnh và Định Kiến) của Jane Austen là kiệt tác văn học lãng mạn kinh điển của nước Anh thế kỷ 19.', N'1', N'Pride and Prejudice', 1813, N'SeedData', 432, N'Tiếng Việt', NULL, 180000.00, 20, 20, 0, N'/images/books/pride-prejudice.jpg', 4, 50, 14, 10, 0, 0, 0),
    (52, N'978604000082', N'Sense and Sensibility', N'Sense and Sensibility (Lý Trí và Tình Cảm) của Jane Austen khám phá tâm lý và lựa chọn của hai chị em Elinor và Marianne trước tình yêu và chuẩn mực xã hội.', N'1', N'Sense and Sensibility', 1811, N'SeedData', 410, N'Tiếng Việt', NULL, 180000.00, 20, 20, 0, N'/images/books/sense-sensibility.jpg', 4, 50, 14, 10, 0, 0, 0),
    (53, N'978604000083', N'Emma', N'Emma là kiệt tác văn học châm biếm sâu cay và hóm hỉnh của Jane Austen. Nhân vật chính Emma Woodhouse là một tiểu thư giàu có, thông minh nhưng thích can thiệp vào chuyện tình cảm của người khác. Trải qua những ngộ nhận và hiểu lầm dở khóc dở cười, cô dần trưởng thành và nhận ra hạnh phúc đích thực luôn ở ngay bên cạnh mình.', N'1', N'Emma', 1815, N'SeedData', 450, N'Tiếng Việt', NULL, 190000.00, 20, 20, 0, N'/images/books/emma.jpg', 4, 50, 14, 10, 0, 0, 0),
    (54, N'978604000084', N'The Great Gatsby', N'The Great Gatsby (Gatsby Vĩ Đại) của F. Scott Fitzgerald khắc họa sâu sắc giấc mơ Mỹ hào nhoáng nhưng bi kịch trong thập niên 1920.', N'1', N'The Great Gatsby', 1925, N'SeedData', 240, N'Tiếng Việt', NULL, 170000.00, 20, 20, 0, N'/images/books/great-gatsby.jpg', 4, 51, 30, 11, 0, 0, 0),
    (55, N'978604000085', N'The Da Vinci Code', N'The Da Vinci Code (Mật Mã Da Vinci) của Dan Brown là tiểu thuyết trinh thám lịch sử tôn giáo kịch tính và hấp dẫn trên toàn thế giới.', N'1', N'The Da Vinci Code', 2003, N'SeedData', 592, N'Tiếng Việt', NULL, 250000.00, 20, 20, 0, N'/images/books/da-vinci-code.jpg', 5, 47, 31, 11, 0, 0, 0),
    (56, N'978604000086', N'Angels and Demons', N'Thiên Thần và Ác Quỷ (Angels and Demons) là cuộc chạy đua nghẹt thở của Robert Langdon tại Vatican để ngăn chặn âm mưu hủy diệt của hội kín Illuminati bằng bom phản vật chất, hòa quyện giữa lịch sử tôn giáo, nghệ thuật và khoa học hiện đại.', N'1', N'Angels and Demons', 2000, N'SeedData', 616, N'Tiếng Việt', NULL, 250000.00, 20, 20, 0, N'/images/books/angels-demons.jpg', 5, 47, 31, 11, 0, 0, 0),
    (57, N'978604000087', N'Inferno', N'Hỏa Ngục (Inferno) đưa Robert Langdon vào trung tâm nước Ý, lần theo những biểu tượng trong tác phẩm Hỏa Ngục của Dante để ngăn chặn một đại dịch diệt chủng toàn cầu do một nhà khoa học cuồng tín phát tán.', N'1', N'Inferno', 2013, N'SeedData', 620, N'Tiếng Việt', NULL, 260000.00, 20, 20, 0, N'/images/books/inferno.jpg', 5, 47, 31, 19, 0, 1, 0),
    (58, N'978604000088', N'Origin', N'Origin (Nguồn Cội) của Dan Brown theo chân giáo sư Robert Langdon khám phá câu hỏi lớn nhất của nhân loại: Chúng ta đến từ đâu và chúng ta đang đi về đâu.', N'1', N'Origin', 2017, N'SeedData', 540, N'Tiếng Việt', NULL, 250000.00, 20, 20, 0, N'/images/books/origin.jpg', 5, 47, 31, 11, 0, 0, 0),
    (59, N'978604000089', N'A Brief History of Time', N'Lược Sử Thời Gian (A Brief History of Time) của Stephen Hawking là cuốn sách khoa học vũ trụ kinh điển, giải thích một cách dễ hiểu về Vụ Nổ Lớn (Big Bang), Hố Đen, Không-Thời Gian và hành trình đi tìm Thuyết Vạn Vật (Theory of Everything) của nền vật lý hiện đại.', N'1', N'A Brief History of Time', 1988, N'SeedData', 256, N'Tiếng Việt', NULL, 220000.00, 20, 20, 0, N'/images/books/brief-history-time.jpg', 7, 42, 29, 7, 0, 0, 0),
    (60, N'9786047743476', N'Ikigai', N'Ikigai - Đi Tìm Lý Do Thức Dậy Mỗi Sáng là cuốn sách nổi tiếng thế giới của Héctor García và Francesc Miralles, hé mở bí quyết trường thọ và lối sống thanh thản của cư dân làng Ogimi, vùng Okinawa (Nhật Bản). Cuốn sách dẫn dắt người đọc qua triết lý 4 vòng tròn giao thoa: Đam mê, Sứ mệnh, Chuyên môn và Sự nghiệp, kết hợp cùng quy tắc ăn no 80% (Hara Hachi Bu) và tinh thần cộng đồng Moai để tìm thấy niềm vui trọn vẹn trong từng khoảnh khắc.', N'1', N'Ikigai', 2016, N'SeedData', 208, N'Tiếng Việt', NULL, 119000.00, 20, 19, 0, N'/images/books/ikigai.jpg', 1, 74, 8, 1, 0, 35, 0),
    (61, N'978604000101', N'Số Đỏ', N'Số Đỏ là tác phẩm trào phúng kiệt xuất của văn học Việt Nam thế kỷ 20 của nhà văn Vũ Trọng Phụng. Tác phẩm xoay quanh nhân vật Xuân Tóc Đỏ từ một kẻ nhặt banh sân quần vợt hạ lưu, nhờ sự láu cá và những biến động bi hài của xã hội thực dân nửa phong kiến thời kỳ Âu hóa mà leo lên đỉnh cao danh vọng, trở thành đốc tờ, anh hùng cứu quốc và biểu tượng của tầng lớp thượng lưu rởm đời.', N'1', N'Số Đỏ, Vũ Trọng Phụng, Văn học hiện thực, Miễn phí', 1938, N'Admin', 320, N'Tiếng Việt', NULL, 95000.00, 50, 50, 16, N'/images/books/so-do.jpg', 4, 91, 5, 2, 0, 1523, 0),
    (62, N'978604000102', N'Tắt Đèn', N'Tắt Đèn là bức tranh hiện thực bi thảm và cảm động sâu sắc về đời sống cơ cực của người nông dân Việt Nam trước năm 1945 dưới ách sưu cao thuế nặng của chế độ thực dân. Hình tượng Chị Dậu kiên cường, giàu đức hy sinh bảo vệ chồng con đến cùng trước bọn cai lệ tàn bạo đã trở thành biểu tượng kinh điển của văn học hiện thực phê phán Việt Nam.', N'1', N'Tắt Đèn, Ngô Tất Tố, Chị Dậu, Văn học hiện thực', 1939, N'Admin', 240, N'Tiếng Việt', NULL, 85000.00, 45, 45, 12, N'/images/books/tat-den.jpg', 4, 92, 5, 2, 0, 1850, 0),
    (63, N'978604000103', N'Chí Phèo', N'Chí Phèo là truyện ngắn hiện thực bất hủ của nhà văn Nam Cao. Tác phẩm kể về bi kịch đau đớn của người nông dân lương thiện bị nhà tù thực dân và sự tàn bạo của tầng lớp cường hào làng Vũ Đại hủy hoại cả nhân hình lẫn nhân tính, biến thành con quỷ dữ. Cuộc gặp gỡ với Thị Nở cùng bát cháo hành ấm áp tình người đã đánh thức khát khao lương thiện nhưng cánh cửa trở về xã hội loài người đã vĩnh viễn khép lại.', N'1', N'Chí Phèo, Nam Cao, Thị Nở, Bá Kiến, Văn học kinh điển', 1941, N'Admin', 195, N'Tiếng Việt', NULL, 75000.00, 40, 40, 14, N'/images/books/chi-pheo.jpg', 4, 81, 6, 2, 0, 2400, 0),
    (64, N'978604000104', N'Truyện Kiều', N'Truyện Kiều (Đoạn trường tân thanh) của Đại thi hào dân tộc Nguyễn Du là đỉnh cao chói lọi của văn học cổ điển Việt Nam được viết bằng thể thơ lục bát chuẩn mực gồm 3.254 câu. Tác phẩm ngợi ca vẻ đẹp tài sắc và phẩm hạnh thanh cao của Thúy Kiều, đồng thời tố cáo xã hội kim tiền tàn bạo chà đạp lên quyền sống của người phụ nữ tài hoa bạc mệnh.', N'1', N'Truyện Kiều, Nguyễn Du, Thúy Kiều, Lục bát, Văn học cổ điển', 1820, N'Admin', 380, N'Tiếng Việt', NULL, 120000.00, 60, 60, 10, N'/images/books/truyen-kieu.jpg', 4, 94, 5, 3, 0, 3101, 0),
    (65, N'978604000105', N'Hoàng Tử Bé', N'Hoàng Tử Bé (Le Petit Prince) của nhà văn kiêm phi công Antoine de Saint-Exupéry là tác phẩm văn học thiếu nhi và triết lý kinh điển được dịch ra hơn 300 ngôn ngữ trên thế giới. Tác phẩm gửi gắm thông điệp bất hủ về tình yêu thương, sự thuần khiết và trách nhiệm: "Người ta chỉ thấy rõ bằng trái tim. Điều cốt lõi vô hình đối với mắt trần." Một cuốn sách nuôi dưỡng tâm hồn cho cả trẻ em lẫn người lớn.', N'1', N'Hoàng Tử Bé, The Little Prince, Saint-Exupéry, Triết lý', 1943, N'Admin', 120, N'Tiếng Việt', NULL, 85000.00, 60, 60, 6, N'/images/books/little-prince.jpg', 9, 49, 2, 3, 0, 2757, 0),
    (66, N'978604000106', N'Ông Già Và Biển Cả', N'Ông Già Và Biển Cả (The Old Man and the Sea) là kiệt tác đoạt giải Nobel Văn học 1954 của Ernest Hemingway. Với phong cách nguyên lý tảng băng trôi độc đáo, tác phẩm kể về chuyến ra khơi đơn độc kéo dài 84 ngày của lão ngư Santiago và cuộc chiến đấu nghẹt thở nhưng đầy kiêu hãnh với con cá kiếm khổng lồ trên dòng Vịnh, khẳng định chân lý: "Con người sinh ra không phải để chiến bại. Con người có thể bị hủy diệt chứ không thể bị đánh bại.".', N'1', N'Ông Già Và Biển Cả, Ernest Hemingway, Nobel Văn học', 1952, N'Admin', 160, N'Tiếng Việt', NULL, 80000.00, 40, 40, 10, N'/images/books/ong-gia-va-bien-ca.jpg', 4, 83, 5, 4, 0, 1920, 0),
    (67, N'978604000107', N'Binh Pháp Tôn Tử', N'Binh Pháp Tôn Tử (The Art of War) là bộ trước tác quân sự và triết học chiến lược cổ xưa nhất nhân loại gồm 13 thiên của danh tướng Tôn Vũ. Không chỉ bàn về chiến tranh, tác phẩm là đỉnh cao tư duy về nghệ thuật đàm phán, quản trị nhân sự, nắm bắt thời thế và chiến lược cạnh tranh kinh doanh hiện đại với phương châm bất hủ: "Biết người biết ta, trăm trận không nguy.".', N'1', N'Binh Pháp Tôn Tử, Tôn Vũ, Chiến lược, Quân sự, Triết học', -500, N'Admin', 220, N'Tiếng Việt', NULL, 110000.00, 35, 35, 16, N'/images/books/binh-phap-ton-tu.jpg', 2, 99, 41, 5, 0, 2200, 0),
    (68, N'978604000108', N'Chiến Binh Cầu Vồng', N'Chiến Binh Cầu Vồng (The Rainbow Troops) của Andrea Hirata là câu chuyện có thật đầy xúc động về 10 học trò nghèo tại ngôi trường xập xệ trên đảo Belitong (Indonesia) cùng cô giáo trẻ Muhammadiyah hết lòng vì học trò. Bất chấp sự nghèo đói và đe dọa đóng cửa, tình yêu tri thức và khát khao vươn lên đã biến các em thành những chiến binh dũng cảm, thắp sáng niềm tin về sức mạnh kỳ diệu của giáo dục.', N'1', N'Chiến Binh Cầu Vồng, Andrea Hirata, Giáo dục, Nghị lực sống', 2005, N'Admin', 530, N'Tiếng Việt', NULL, 135000.00, 45, 45, 8, N'/images/books/chien-binh-cau-vong.jpg', 4, 88, 6, 6, 0, 2102, 0);
GO
SET IDENTITY_INSERT [Books] OFF;
GO


-- -------------------------------------------------------------
-- Dữ liệu bảng: Readers (Tổng cộng: 8 bản ghi)
-- -------------------------------------------------------------
SET IDENTITY_INSERT [Readers] ON;
GO
INSERT INTO [Readers] ([Id], [ReaderCode], [FullName], [DateOfBirth], [Phone], [Email], [Address], [IdentityNumber], [RegisterDate], [ExpirationDate], [LastLoginDate], [ConsecutiveLoginDays], [LastStreakDate], [Avatar], [CoverImageUrl], [Bio], [ActiveBadge], [FullPageBackgroundUrl], [FeaturedBookIds], [IsCommentBlocked], [CommentBlockedUntil], [MaxBorrowBooks], [Password], [Role], [IsActive], [EmailConfirmed], [EmailConfirmationToken], [EmailConfirmationSentDate], [ForcePasswordChange], [ReadersType], [Genders], [IsDeleted], [IsNameSet], [IsVerified]) VALUES
    (1, N'ADMIN001', N'Nguyễn Hoàng Long', '1990-01-01 00:00:00.000', N'0123456789', N'admin@library.com', N'Số 8 Tôn Thất Thuyết, Nam Từ Liêm, Hà Nội', N'000000000001', '2026-11-06 13:40:08.124', '2036-11-06 13:40:08.124', '2026-11-11 21:09:28.033', 1, '2026-11-11 00:00:00.000', NULL, NULL, NULL, N' Độc Giả Tập Sự', NULL, NULL, 0, NULL, 1000, N'AQAAAAIAAYagAAAAEDdGches6FS/0wx4fFoD95qmUgoj0Z74byH1UjabvxVnwwKjWmk8Qs0xakECHvbPIA==', N'Admin', 1, 1, NULL, NULL, 0, 0, 2, 0, 0, 1),
    (2, N'DG001', N'Lê Phú An', '2004-01-01 00:00:00.000', N'0941205808', N'reader@library.com', N'TP Hồ Chí Minh', N'000000000002', '2026-11-06 13:40:08.246', '2027-11-06 13:40:08.246', '2026-11-11 21:09:42.615', 1, '2026-11-11 00:00:00.000', NULL, NULL, NULL, N' Độc Giả Tập Sự', NULL, NULL, 0, NULL, 10, N'AQAAAAIAAYagAAAAEKrHolc+1eKBQowY3PeO8dP+6eTBvdTfKUW8CMtArnJOdFQ/e1oy7Zwy96c/EV9xeg==', N'Reader', 1, 1, NULL, NULL, 0, 0, 2, 0, 0, 1),
    (8, N'DG002', N'Trần Minh Khang', '2003-11-12 00:00:00.000', N'0977889900', N'sinhvien01@fpt.edu.vn', N'Tòa nhà FPT Polytechnic, Mỹ Đình, Hà Nội', N'001203009876', '2026-11-07 19:29:10.093', '2029-11-07 19:29:10.093', NULL, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, 5, N'AQAAAAIAAYagAAAAENK6XzL3p3H2k+RjO+K9sQyR5pX1iV7X3Q+jK2mX4w==', N'Reader', 1, 1, NULL, NULL, 0, 0, 1, 0, 1, 1),
    (9, N'DG003', N'Nguyễn Quỳnh Như', '2004-04-25 00:00:00.000', N'0966554433', N'sinhvien02@fpt.edu.vn', N'Ngõ 130 Xuân Thủy, Cầu Giấy, Hà Nội', N'001204008765', '2026-11-07 19:29:10.093', '2029-11-07 19:29:10.093', NULL, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, 5, N'AQAAAAIAAYagAAAAENK6XzL3p3H2k+RjO+K9sQyR5pX1iV7X3Q+jK2mX4w==', N'Reader', 1, 1, NULL, NULL, 0, 0, 2, 0, 1, 1),
    (20, N'RD_NV002', N'Trần Thị Mai Phương', '1992-07-22 00:00:00.000', N'0987654321', N'quanly@library.com', N'275 Nguyễn Trãi, Thanh Xuân, Hà Nội', N'001192023456', '2026-11-08 17:15:25.813', '2031-11-08 17:15:25.813', '2026-11-11 21:09:28.333', 1, '2026-11-11 00:00:00.000', NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, 10, N'AQAAAAIAAYagAAAAEKAQk9ugIvpGhJ4L7dgqO7JgJPtWhOcqL7V8AzQoVGGYc96HBMzbI0VikdFhqBh94Q==', N'Quản lý thư viện', 1, 1, NULL, NULL, 0, 1, 0, 0, 1, 1),
    (21, N'RD_NV003', N'Lê Minh Cường', '1994-11-10 00:00:00.000', N'0903123456', N'thuthu@library.com', N'12 Kim Mã, Ba Đình, Hà Nội', N'001094034567', '2026-11-08 17:15:25.813', '2031-11-08 17:15:25.813', '2026-11-11 21:09:28.550', 1, '2026-11-11 00:00:00.000', NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, 10, N'AQAAAAIAAYagAAAAEPtoWUA47ypIT3dfKPcyiz3XCMmjZrfi2H2WedgucpE9CnyXkXZ/4BcANpg9H5iSTA==', N'Thủ thư', 1, 1, NULL, NULL, 0, 1, 1, 0, 1, 1),
    (22, N'RD_NV004', N'Phạm Thu Hà', '1996-05-18 00:00:00.000', N'0978987654', N'kho@library.com', N'45 Cầu Giấy, Cầu Giấy, Hà Nội', N'001196045678', '2026-11-08 17:15:25.813', '2031-11-08 17:15:25.813', '2026-11-11 21:09:28.778', 1, '2026-11-11 00:00:00.000', NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, 10, N'AQAAAAIAAYagAAAAEH0R++s5ALa0A+3Z4Gh6GZB2Xs0FuGSVzoND6QpXTHhPQ4vUCEdPXgkmagpHptRg8w==', N'Nhân viên kho', 1, 1, NULL, NULL, 0, 1, 0, 0, 1, 1),
    (24, N'RD_NV006', N'Vũ Ngọc Lan', '1998-12-05 00:00:00.000', N'0932123456', N'cskh@library.com', N'15 Hai Bà Trưng, Hoàn Kiếm, Hà Nội', N'001198067890', '2026-11-08 17:15:25.816', '2031-11-08 17:15:25.816', '2026-11-11 21:09:29.018', 1, '2026-11-11 00:00:00.000', NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, 10, N'AQAAAAIAAYagAAAAEPOxytWkJwgrgq70j6BV1r+9jPzLyA22GvHOFlE0ka3Dnlt9bqaLCBv+nxlwRyLvQQ==', N'Chăm sóc khách hàng', 1, 1, NULL, NULL, 0, 1, 0, 0, 1, 1);
GO
SET IDENTITY_INSERT [Readers] OFF;
GO


-- -------------------------------------------------------------
-- Dữ liệu bảng: Employees (Tổng cộng: 5 bản ghi)
-- -------------------------------------------------------------
SET IDENTITY_INSERT [Employees] ON;
GO
INSERT INTO [Employees] ([Id], [EmployeeId], [FullName], [Email], [Phone], [Address], [CCCD], [Gender], [DateOfBirth], [HireDate], [PositionId], [Salary], [StatusId], [LeaveReason], [LeaveStartDate], [UnexcusedDaysCount], [LastUnexcusedDate], [IsDeleted], [RoleId]) VALUES
    (1, N'NV001', N'Nguyễn Hoàng Long', N'admin@library.com', N'0912345678', N'Số 8 Tôn Thất Thuyết, Nam Từ Liêm, Hà Nội', N'001095012345', 1, '1990-03-15 00:00:00.000', '2019-01-01 00:00:00.000', 1, 28000000.00, 1, NULL, NULL, 0, NULL, 0, 1),
    (2, N'NV002', N'Trần Thị Mai Phương', N'quanly@library.com', N'0987654321', N'275 Nguyễn Trãi, Thanh Xuân, Hà Nội', N'001192023456', 2, '1992-07-22 00:00:00.000', '2020-03-15 00:00:00.000', 2, 18500000.00, 1, NULL, NULL, 0, NULL, 0, 2),
    (3, N'NV003', N'Lê Minh Cường', N'thuthu@library.com', N'0903123456', N'12 Kim Mã, Ba Đình, Hà Nội', N'001094034567', 1, '1994-11-10 00:00:00.000', '2021-06-01 00:00:00.000', 3, 12000000.00, 1, NULL, NULL, 0, NULL, 0, 3),
    (4, N'NV004', N'Phạm Thu Hà', N'kho@library.com', N'0978987654', N'45 Cầu Giấy, Cầu Giấy, Hà Nội', N'001196045678', 2, '1996-05-18 00:00:00.000', '2022-02-10 00:00:00.000', 4, 10500000.00, 1, NULL, NULL, 0, NULL, 0, 4),
    (6, N'NV006', N'Vũ Ngọc Lan', N'cskh@library.com', N'0932123456', N'15 Hai Bà Trưng, Hoàn Kiếm, Hà Nội', N'001198067890', 2, '1998-12-05 00:00:00.000', '2023-01-05 00:00:00.000', 6, 9500000.00, 1, NULL, NULL, 0, NULL, 0, 6);
GO
SET IDENTITY_INSERT [Employees] OFF;
GO


-- -------------------------------------------------------------
-- Dữ liệu bảng: BorrowSlips (Tổng cộng: 10 bản ghi)
-- -------------------------------------------------------------
SET IDENTITY_INSERT [BorrowSlips] ON;
GO
INSERT INTO [BorrowSlips] ([Id], [BorrowCode], [ReaderId], [TotalBooks], [BorrowDate], [DueDate], [ReturnDate], [PickupDeadline], [Note], [CreatedDate], [CreatedBy], [Status], [BorrowStatus], [ReadersId]) VALUES
    (1, N'PM202611001', 2, 2, '2026-11-02 19:12:29.130', '2026-11-16 19:12:29.130', NULL, NULL, N'Mượn tài liệu nghiên cứu học tập', '2026-11-02 19:12:29.130', N'thuthu@library.com', N'Borrowing', 0, NULL),
    (2, N'PM202610002', 2, 1, '2026-10-18 19:12:29.133', '2026-11-01 19:12:29.133', '2026-10-31 19:12:29.133', NULL, N'Đã hoàn tất trả sách đúng hạn', '2026-10-18 19:12:29.133', N'thuthu@library.com', N'Returned', 1, NULL),
    (3, N'PM202610003', 2, 1, '2026-10-13 19:12:29.133', '2026-10-27 19:12:29.133', NULL, NULL, N'Đã gửi email nhắc nhở quá hạn 11 ngày', '2026-10-13 19:12:29.133', N'thuthu@library.com', N'Overdue', 2, NULL),
    (4, N'PM202606001', 8, 2, '2026-06-15 10:00:00.000', '2026-06-29 10:00:00.000', '2026-06-28 14:00:00.000', NULL, N'MÆ°á»£n há»c hÃ¨', '2026-06-15 00:00:00.000', NULL, NULL, 3, NULL),
    (5, N'PM202607001', 9, 1, '2026-07-20 11:30:00.000', '2026-08-03 11:30:00.000', '2026-08-01 16:00:00.000', NULL, N'Äá»c thÆ° giÃ£n', '2026-07-20 00:00:00.000', NULL, NULL, 3, NULL),
    (6, N'PM202608001', 2, 3, '2026-08-10 09:15:00.000', '2026-08-24 09:15:00.000', '2026-08-23 15:30:00.000', NULL, N'NghiÃªn cá»©u vÄƒn há»c', '2026-08-10 00:00:00.000', NULL, NULL, 3, NULL),
    (7, N'PM202608002', 8, 2, '2026-08-25 14:00:00.000', '2026-09-08 14:00:00.000', '2026-09-07 10:00:00.000', NULL, N'TÃ i liá»‡u tham kháº£o', '2026-08-25 00:00:00.000', NULL, NULL, 3, NULL),
    (8, N'PM202609001', 9, 2, '2026-09-05 16:45:00.000', '2026-09-19 16:45:00.000', '2026-09-18 11:00:00.000', NULL, N'Äá»c tuáº§n Ä‘áº§u khÃ³a', '2026-09-05 00:00:00.000', NULL, NULL, 3, NULL),
    (9, N'PM202609002', 2, 1, '2026-09-22 13:20:00.000', '2026-10-06 13:20:00.000', '2026-10-05 17:00:00.000', NULL, N'MÆ°á»£n thÃªm', '2026-09-22 00:00:00.000', NULL, NULL, 3, NULL),
    (10, N'PM202610001', 8, 3, '2026-10-05 08:30:00.000', '2026-10-19 08:30:00.000', '2026-10-18 16:30:00.000', NULL, N'Ã”n táº­p', '2026-10-05 00:00:00.000', NULL, NULL, 3, NULL);
GO
SET IDENTITY_INSERT [BorrowSlips] OFF;
GO


-- -------------------------------------------------------------
-- Dữ liệu bảng: BorrowDetails (Tổng cộng: 19 bản ghi)
-- -------------------------------------------------------------
SET IDENTITY_INSERT [BorrowDetails] ON;
GO
INSERT INTO [BorrowDetails] ([Id], [BorrowSlipId], [BookId], [Quantity], [BorrowPrice], [IsReturned], [ReturnedDate], [ConditionBefore], [ConditionAfter], [Note], [IsLost], [IsDamaged], [DamagePercent], [ExtensionCount], [DueDate], [RequestedExtensionDays], [ExtensionReason], [CompensationAmount]) VALUES
    (4, 1, 1, 1, 0.00, 0, NULL, NULL, NULL, NULL, 0, 0, NULL, 0, NULL, NULL, NULL, 0.00),
    (5, 1, 2, 1, 0.00, 0, NULL, NULL, NULL, NULL, 0, 0, NULL, 0, NULL, NULL, NULL, 0.00),
    (6, 2, 2, 1, 0.00, 0, NULL, NULL, NULL, NULL, 0, 0, NULL, 0, NULL, NULL, NULL, 0.00),
    (7, 3, 4, 1, 0.00, 0, NULL, NULL, NULL, NULL, 0, 0, NULL, 0, NULL, NULL, NULL, 0.00),
    (8, 3, 6, 1, 0.00, 0, NULL, NULL, NULL, NULL, 0, 0, NULL, 0, NULL, NULL, NULL, 0.00),
    (9, 4, 1, 1, 0.00, 1, NULL, NULL, NULL, NULL, 0, 0, NULL, 0, NULL, NULL, NULL, 0.00),
    (10, 4, 3, 1, 0.00, 1, NULL, NULL, NULL, NULL, 0, 0, NULL, 0, NULL, NULL, NULL, 0.00),
    (11, 5, 2, 1, 0.00, 1, NULL, NULL, NULL, NULL, 0, 0, NULL, 0, NULL, NULL, NULL, 0.00),
    (12, 6, 4, 1, 0.00, 1, NULL, NULL, NULL, NULL, 0, 0, NULL, 0, NULL, NULL, NULL, 0.00),
    (13, 6, 5, 1, 0.00, 1, NULL, NULL, NULL, NULL, 0, 0, NULL, 0, NULL, NULL, NULL, 0.00),
    (14, 6, 6, 1, 0.00, 1, NULL, NULL, NULL, NULL, 0, 0, NULL, 0, NULL, NULL, NULL, 0.00),
    (15, 7, 7, 1, 0.00, 1, NULL, NULL, NULL, NULL, 0, 0, NULL, 0, NULL, NULL, NULL, 0.00),
    (16, 7, 8, 1, 0.00, 1, NULL, NULL, NULL, NULL, 0, 0, NULL, 0, NULL, NULL, NULL, 0.00),
    (17, 8, 1, 1, 0.00, 1, NULL, NULL, NULL, NULL, 0, 0, NULL, 0, NULL, NULL, NULL, 0.00),
    (18, 8, 9, 1, 0.00, 1, NULL, NULL, NULL, NULL, 0, 0, NULL, 0, NULL, NULL, NULL, 0.00),
    (19, 9, 10, 1, 0.00, 1, NULL, NULL, NULL, NULL, 0, 0, NULL, 0, NULL, NULL, NULL, 0.00),
    (20, 10, 1, 1, 0.00, 1, NULL, NULL, NULL, NULL, 0, 0, NULL, 0, NULL, NULL, NULL, 0.00),
    (21, 10, 2, 1, 0.00, 1, NULL, NULL, NULL, NULL, 0, 0, NULL, 0, NULL, NULL, NULL, 0.00),
    (22, 10, 3, 1, 0.00, 1, NULL, NULL, NULL, NULL, 0, 0, NULL, 0, NULL, NULL, NULL, 0.00);
GO
SET IDENTITY_INSERT [BorrowDetails] OFF;
GO


-- -------------------------------------------------------------
-- Dữ liệu bảng: Reservations (Tổng cộng: 2 bản ghi)
-- -------------------------------------------------------------
SET IDENTITY_INSERT [Reservations] ON;
GO
INSERT INTO [Reservations] ([Id], [ReaderId], [BookId], [ReservationDate], [ExpirationDate], [Status], [Note], [BooksStatus], [BooksId], [ReadersId]) VALUES
    (2, 2, 61, '2026-11-07 19:27:49.430', '2026-11-14 19:27:49.430', N'Approved', N'Sách đã sẵn sàng tại quầy số 1, vui lòng đến nhận trước ngày hết hạn', 1, NULL, NULL),
    (3, 2, 65, '2026-11-06 19:27:49.430', '2026-11-13 19:27:49.430', N'Waiting', N'Đang chờ bạn đọc trước trả sách', 0, NULL, NULL);
GO
SET IDENTITY_INSERT [Reservations] OFF;
GO


-- -------------------------------------------------------------
-- Dữ liệu bảng: Fines (Tổng cộng: 1 bản ghi)
-- -------------------------------------------------------------
SET IDENTITY_INSERT [Fines] ON;
GO
INSERT INTO [Fines] ([Id], [BorrowSlipId], [BorrowDetailId], [BookId], [ReaderId], [Amount], [Reason], [CreatedDate], [PaidDate], [FinesType], [IsPaid], [DamagePercent], [LateDays], [LateFine], [DamageFine], [LostFine], [Status], [TransactionCode], [BorrowDetailsId]) VALUES
    (1, 3, 3, 4, 2, 55000.00, N'Quá hạn trả sách Clean Code 11 ngày (5.000đ/ngày)', '2026-11-07 19:12:29.133', NULL, 0, 0, NULL, 11, 55000.00, NULL, NULL, N'Unpaid', NULL, NULL);
GO
SET IDENTITY_INSERT [Fines] OFF;
GO


-- -------------------------------------------------------------
-- Dữ liệu bảng: SystemSettings (Tổng cộng: 1 bản ghi)
-- -------------------------------------------------------------
SET IDENTITY_INSERT [SystemSettings] ON;
GO
INSERT INTO [SystemSettings] ([Id], [MaxExtensionDays], [MaxExtensionCount], [LateFeePerDay], [MaxBorrowBooks], [BorrowPeriodDays], [CreatedDate], [UpdatedDate], [IsActive]) VALUES
    (1, 7, 1, 5000.00, 5, 14, '2026-11-07 19:12:29.060', NULL, 1);
GO
SET IDENTITY_INSERT [SystemSettings] OFF;
GO

