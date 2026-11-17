-- =========================================================================
-- LIBRARYHUB.NEXT - FULL SCHEMA (EF CORE CODE FIRST) & DEMO DATA SCRIPT
-- Generated: 2026-11-16 22:37:11
-- SQL Server 2019 / 2022 / LocalDB compatible
-- =========================================================================
IF DB_ID(N'LibraryHubNext') IS NULL
BEGIN
    CREATE DATABASE [LibraryHubNext];
END;
GO
USE [LibraryHubNext];
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

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
CREATE TABLE [AspNetRoles] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);

CREATE TABLE [AspNetUsers] (
    [Id] uniqueidentifier NOT NULL,
    [AccountKind] int NOT NULL,
    [DisplayName] nvarchar(max) NOT NULL,
    [IsActive] bit NOT NULL,
    [MustChangePassword] bit NOT NULL,
    [LastLoginAt] datetimeoffset NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);

CREATE TABLE [Authors] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [DateOfBirth] date NULL,
    [Nationality] nvarchar(max) NULL,
    [Biography] nvarchar(max) NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [RowVersion] rowversion NOT NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_Authors] PRIMARY KEY ([Id])
);

CREATE TABLE [BookCommentLikes] (
    [Id] int NOT NULL IDENTITY,
    [BookCommentId] int NOT NULL,
    [ReaderId] int NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_BookCommentLikes] PRIMARY KEY ([Id])
);

CREATE TABLE [BookComments] (
    [Id] int NOT NULL IDENTITY,
    [BookId] int NOT NULL,
    [ReaderId] int NOT NULL,
    [ParentCommentId] int NULL,
    [Content] nvarchar(max) NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [RowVersion] rowversion NOT NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_BookComments] PRIMARY KEY ([Id])
);

CREATE TABLE [BookFavorites] (
    [Id] int NOT NULL IDENTITY,
    [BookId] int NOT NULL,
    [ReaderId] int NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_BookFavorites] PRIMARY KEY ([Id])
);

CREATE TABLE [BookRatings] (
    [Id] int NOT NULL IDENTITY,
    [BookId] int NOT NULL,
    [ReaderId] int NOT NULL,
    [Stars] int NOT NULL,
    [Review] nvarchar(max) NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_BookRatings] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_BookRating_Stars] CHECK ([Stars] BETWEEN 1 AND 5)
);

CREATE TABLE [BookViewHistories] (
    [Id] int NOT NULL IDENTITY,
    [BookId] int NOT NULL,
    [ReaderId] int NULL,
    [SessionId] nvarchar(max) NULL,
    [ViewedAt] datetimeoffset NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_BookViewHistories] PRIMARY KEY ([Id])
);

CREATE TABLE [BorrowReturns] (
    [Id] int NOT NULL IDENTITY,
    [BorrowDetailId] int NOT NULL,
    [Quantity] int NOT NULL,
    [Condition] int NOT NULL,
    [DamagePercent] decimal(5,2) NOT NULL,
    [ConditionNote] nvarchar(max) NULL,
    [ProcessedByAccountId] uniqueidentifier NOT NULL,
    [IdempotencyKey] nvarchar(450) NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_BorrowReturns] PRIMARY KEY ([Id])
);

CREATE TABLE [Categories] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [RowVersion] rowversion NOT NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_Categories] PRIMARY KEY ([Id])
);

CREATE TABLE [CommentReports] (
    [Id] int NOT NULL IDENTITY,
    [ReporterId] int NOT NULL,
    [ReportedUserId] int NOT NULL,
    [BookCommentId] int NULL,
    [ProfileCommentId] int NULL,
    [Reason] nvarchar(max) NOT NULL,
    [Status] int NOT NULL,
    [ReviewedByAccountId] uniqueidentifier NULL,
    [Resolution] nvarchar(max) NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_CommentReports] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_CommentReport_Target] CHECK (([BookCommentId] IS NOT NULL AND [ProfileCommentId] IS NULL) OR ([BookCommentId] IS NULL AND [ProfileCommentId] IS NOT NULL))
);

CREATE TABLE [EmployeeStatusOptions] (
    [Id] int NOT NULL IDENTITY,
    [Code] nvarchar(max) NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NULL,
    [DisplayOrder] int NOT NULL,
    [Status] int NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [RowVersion] rowversion NOT NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_EmployeeStatusOptions] PRIMARY KEY ([Id])
);

CREATE TABLE [Fines] (
    [Id] int NOT NULL IDENTITY,
    [Code] nvarchar(450) NOT NULL,
    [BorrowSlipId] int NOT NULL,
    [ReaderId] int NOT NULL,
    [Type] int NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [PaidAmount] decimal(18,2) NOT NULL,
    [Reason] nvarchar(max) NULL,
    [Status] int NOT NULL,
    [TransactionCode] nvarchar(max) NULL,
    [PaidAt] datetimeoffset NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_Fines] PRIMARY KEY ([Id])
);

CREATE TABLE [Notifications] (
    [Id] int NOT NULL IDENTITY,
    [RecipientAccountId] uniqueidentifier NOT NULL,
    [Type] int NOT NULL,
    [Title] nvarchar(max) NOT NULL,
    [Message] nvarchar(max) NOT NULL,
    [Link] nvarchar(max) NULL,
    [IsRead] bit NOT NULL,
    [ReadAt] datetimeoffset NULL,
    [CreatedByAccountId] uniqueidentifier NULL,
    [BroadcastGroupId] uniqueidentifier NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id])
);

CREATE TABLE [Permissions] (
    [Id] int NOT NULL IDENTITY,
    [Code] nvarchar(450) NOT NULL,
    [DisplayName] nvarchar(max) NOT NULL,
    [GroupName] nvarchar(max) NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_Permissions] PRIMARY KEY ([Id])
);

CREATE TABLE [Positions] (
    [Id] int NOT NULL IDENTITY,
    [Code] nvarchar(450) NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NULL,
    [SalaryAmount] decimal(18,2) NOT NULL,
    [AllowSalaryOverride] bit NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [RowVersion] rowversion NOT NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_Positions] PRIMARY KEY ([Id])
);

CREATE TABLE [ProfileComments] (
    [Id] int NOT NULL IDENTITY,
    [ProfileOwnerId] int NOT NULL,
    [AuthorId] int NOT NULL,
    [Content] nvarchar(max) NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [RowVersion] rowversion NOT NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_ProfileComments] PRIMARY KEY ([Id])
);

CREATE TABLE [ProfileTagPins] (
    [Id] int NOT NULL IDENTITY,
    [ProfileOwnerId] int NOT NULL,
    [AuthorId] int NOT NULL,
    [TagKeysJson] nvarchar(max) NOT NULL,
    [PresetMessageKey] nvarchar(max) NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_ProfileTagPins] PRIMARY KEY ([Id])
);

CREATE TABLE [Publishers] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [Address] nvarchar(max) NULL,
    [Phone] nvarchar(max) NULL,
    [Email] nvarchar(max) NULL,
    [Website] nvarchar(max) NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [RowVersion] rowversion NOT NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_Publishers] PRIMARY KEY ([Id])
);

CREATE TABLE [ReaderFeaturedBooks] (
    [Id] int NOT NULL IDENTITY,
    [ReaderId] int NOT NULL,
    [BookId] int NOT NULL,
    [DisplayOrder] int NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_ReaderFeaturedBooks] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_FeaturedBook_Order] CHECK ([DisplayOrder] BETWEEN 1 AND 4)
);

CREATE TABLE [Readers] (
    [Id] int NOT NULL IDENTITY,
    [UserAccountId] uniqueidentifier NOT NULL,
    [ReaderCode] nvarchar(450) NOT NULL,
    [FullName] nvarchar(max) NOT NULL,
    [DateOfBirth] date NULL,
    [Phone] nvarchar(max) NULL,
    [Email] nvarchar(450) NOT NULL,
    [Address] nvarchar(max) NULL,
    [IdentityNumber] nvarchar(max) NULL,
    [RegisteredOn] date NOT NULL,
    [ExpiresOn] date NOT NULL,
    [LastLoginAt] datetimeoffset NULL,
    [ConsecutiveLoginDays] int NOT NULL,
    [LastStreakOn] date NULL,
    [AvatarPath] nvarchar(max) NULL,
    [CoverImagePath] nvarchar(max) NULL,
    [Bio] nvarchar(max) NULL,
    [ActiveBadge] nvarchar(max) NULL,
    [FullPageBackgroundPath] nvarchar(max) NULL,
    [CommentLockMode] int NOT NULL,
    [CommentBlockedUntil] datetimeoffset NULL,
    [MaxBorrowBooks] int NOT NULL,
    [Type] int NOT NULL,
    [Gender] int NOT NULL,
    [EmailConfirmed] bit NOT NULL,
    [ForcePasswordChange] bit NOT NULL,
    [IsNameSet] bit NOT NULL,
    [IsVerified] bit NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [RowVersion] rowversion NOT NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_Readers] PRIMARY KEY ([Id])
);

CREATE TABLE [ReadingHistories] (
    [Id] int NOT NULL IDENTITY,
    [BookId] int NOT NULL,
    [ReaderId] int NOT NULL,
    [CurrentPage] int NOT NULL,
    [TotalPages] int NOT NULL,
    [LastReadAt] datetimeoffset NOT NULL,
    [StartedAt] datetimeoffset NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_ReadingHistories] PRIMARY KEY ([Id])
);

CREATE TABLE [Reservations] (
    [Id] int NOT NULL IDENTITY,
    [ReaderId] int NOT NULL,
    [BookId] int NOT NULL,
    [ReservedAt] datetimeoffset NOT NULL,
    [ExpiresAt] datetimeoffset NULL,
    [Status] int NOT NULL,
    [Note] nvarchar(max) NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_Reservations] PRIMARY KEY ([Id])
);

CREATE TABLE [RolePermissions] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] int NOT NULL,
    [PermissionId] int NOT NULL,
    [Effect] int NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_RolePermissions] PRIMARY KEY ([Id])
);

CREATE TABLE [SecurityRoles] (
    [Id] int NOT NULL IDENTITY,
    [Code] nvarchar(450) NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NULL,
    [IsSystem] bit NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [RowVersion] rowversion NOT NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_SecurityRoles] PRIMARY KEY ([Id])
);

CREATE TABLE [Shelves] (
    [Id] int NOT NULL IDENTITY,
    [Code] nvarchar(max) NOT NULL,
    [LocationCode] nvarchar(max) NULL,
    [Name] nvarchar(max) NOT NULL,
    [Area] nvarchar(max) NULL,
    [Description] nvarchar(max) NULL,
    [Capacity] int NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [RowVersion] rowversion NOT NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_Shelves] PRIMARY KEY ([Id])
);

CREATE TABLE [StaffAccounts] (
    [Id] int NOT NULL IDENTITY,
    [UserAccountId] uniqueidentifier NOT NULL,
    [EmployeeId] int NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_StaffAccounts] PRIMARY KEY ([Id])
);

CREATE TABLE [SupportTickets] (
    [Id] int NOT NULL IDENTITY,
    [Code] nvarchar(450) NOT NULL,
    [ReaderId] int NOT NULL,
    [AssignedStaffAccountId] uniqueidentifier NULL,
    [Subject] nvarchar(max) NOT NULL,
    [Status] int NOT NULL,
    [LastMessageAt] datetimeoffset NOT NULL,
    [UnreadByReader] bit NOT NULL,
    [UnreadByStaff] bit NOT NULL,
    [ClosedAt] datetimeoffset NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_SupportTickets] PRIMARY KEY ([Id])
);

CREATE TABLE [SystemSettings] (
    [Id] int NOT NULL IDENTITY,
    [MaxExtensionDays] int NOT NULL,
    [MaxExtensionCount] int NOT NULL,
    [LateFeePerDay] decimal(18,2) NOT NULL,
    [MaxBorrowBooks] int NOT NULL,
    [BorrowPeriodDays] int NOT NULL,
    [PickupWindowDays] int NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_SystemSettings] PRIMARY KEY ([Id])
);

CREATE TABLE [UserPermissions] (
    [Id] int NOT NULL IDENTITY,
    [EmployeeId] int NOT NULL,
    [PermissionId] int NOT NULL,
    [Effect] int NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_UserPermissions] PRIMARY KEY ([Id])
);

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] uniqueidentifier NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] uniqueidentifier NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserRoles] (
    [UserId] uniqueidentifier NOT NULL,
    [RoleId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserTokens] (
    [UserId] uniqueidentifier NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [FineDetails] (
    [Id] int NOT NULL IDENTITY,
    [FineId] int NOT NULL,
    [BookId] int NOT NULL,
    [BorrowDetailId] int NULL,
    [Type] int NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Reason] nvarchar(max) NULL,
    [DamagePercent] decimal(5,2) NULL,
    [BookPrice] decimal(18,2) NOT NULL,
    [LateDays] int NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_FineDetails] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_FineDetails_Fines_FineId] FOREIGN KEY ([FineId]) REFERENCES [Fines] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Employees] (
    [Id] int NOT NULL IDENTITY,
    [EmployeeCode] nvarchar(450) NOT NULL,
    [FullName] nvarchar(max) NOT NULL,
    [Email] nvarchar(450) NOT NULL,
    [Phone] nvarchar(max) NOT NULL,
    [Address] nvarchar(max) NOT NULL,
    [NationalId] nvarchar(450) NOT NULL,
    [Gender] int NOT NULL,
    [DateOfBirth] date NOT NULL,
    [HiredOn] date NOT NULL,
    [PositionId] int NOT NULL,
    [Salary] decimal(18,2) NOT NULL,
    [Status] int NOT NULL,
    [StatusOptionId] int NULL,
    [LeaveReason] nvarchar(max) NULL,
    [LeaveStartedOn] date NULL,
    [UnexcusedDays] int NOT NULL,
    [LastUnexcusedOn] date NULL,
    [RoleId] int NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [RowVersion] rowversion NOT NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_Employees] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Employees_Positions_PositionId] FOREIGN KEY ([PositionId]) REFERENCES [Positions] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [BorrowSlips] (
    [Id] int NOT NULL IDENTITY,
    [Code] nvarchar(450) NOT NULL,
    [ReaderId] int NOT NULL,
    [TotalBooks] int NOT NULL,
    [RequestedAt] datetimeoffset NOT NULL,
    [BorrowedAt] datetimeoffset NULL,
    [DueAt] datetimeoffset NULL,
    [ReturnedAt] datetimeoffset NULL,
    [PickupExpiresAt] datetimeoffset NULL,
    [Note] nvarchar(max) NULL,
    [CreatedByAccountId] uniqueidentifier NOT NULL,
    [Status] int NOT NULL,
    [IdempotencyKey] nvarchar(450) NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_BorrowSlips] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BorrowSlips_Readers_ReaderId] FOREIGN KEY ([ReaderId]) REFERENCES [Readers] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [Books] (
    [Id] int NOT NULL IDENTITY,
    [Isbn] nvarchar(450) NOT NULL,
    [Title] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NULL,
    [Edition] nvarchar(max) NULL,
    [Keywords] nvarchar(max) NULL,
    [PublicationYear] int NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [PageCount] int NOT NULL,
    [Language] nvarchar(max) NULL,
    [LastBorrowedAt] datetimeoffset NULL,
    [Price] decimal(18,2) NOT NULL,
    [TotalQuantity] int NOT NULL,
    [AvailableQuantity] int NOT NULL,
    [MinimumAge] int NOT NULL,
    [CoverImagePath] nvarchar(max) NULL,
    [Status] int NOT NULL,
    [ViewCount] bigint NOT NULL,
    [CategoryId] int NOT NULL,
    [AuthorId] int NOT NULL,
    [PublisherId] int NOT NULL,
    [ShelfId] int NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [RowVersion] rowversion NOT NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetimeoffset NULL,
    CONSTRAINT [PK_Books] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Book_Price] CHECK ([Price] >= 0),
    CONSTRAINT [CK_Book_Quantity] CHECK ([TotalQuantity] >= 0 AND [AvailableQuantity] >= 0 AND [AvailableQuantity] <= [TotalQuantity]),
    CONSTRAINT [FK_Books_Authors_AuthorId] FOREIGN KEY ([AuthorId]) REFERENCES [Authors] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Books_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Books_Publishers_PublisherId] FOREIGN KEY ([PublisherId]) REFERENCES [Publishers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Books_Shelves_ShelfId] FOREIGN KEY ([ShelfId]) REFERENCES [Shelves] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [SupportMessages] (
    [Id] int NOT NULL IDENTITY,
    [SupportTicketId] int NOT NULL,
    [SenderType] int NOT NULL,
    [SenderAccountId] uniqueidentifier NULL,
    [Content] nvarchar(max) NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_SupportMessages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SupportMessages_SupportTickets_SupportTicketId] FOREIGN KEY ([SupportTicketId]) REFERENCES [SupportTickets] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [BorrowDetails] (
    [Id] int NOT NULL IDENTITY,
    [BorrowSlipId] int NOT NULL,
    [BookId] int NOT NULL,
    [Quantity] int NOT NULL,
    [ReturnedQuantity] int NOT NULL,
    [BorrowPrice] decimal(18,2) NOT NULL,
    [ConditionBefore] nvarchar(max) NULL,
    [Note] nvarchar(max) NULL,
    [ExtensionCount] int NOT NULL,
    [DueAt] datetimeoffset NULL,
    [RequestedExtensionDays] int NULL,
    [ExtensionReason] nvarchar(max) NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_BorrowDetails] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_BorrowDetail_Quantity] CHECK ([Quantity] > 0 AND [ReturnedQuantity] >= 0 AND [ReturnedQuantity] <= [Quantity]),
    CONSTRAINT [FK_BorrowDetails_Books_BookId] FOREIGN KEY ([BookId]) REFERENCES [Books] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_BorrowDetails_BorrowSlips_BorrowSlipId] FOREIGN KEY ([BorrowSlipId]) REFERENCES [BorrowSlips] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;

CREATE UNIQUE INDEX [IX_BookCommentLikes_BookCommentId_ReaderId] ON [BookCommentLikes] ([BookCommentId], [ReaderId]);

CREATE UNIQUE INDEX [IX_BookFavorites_BookId_ReaderId] ON [BookFavorites] ([BookId], [ReaderId]);

CREATE UNIQUE INDEX [IX_BookRatings_BookId_ReaderId] ON [BookRatings] ([BookId], [ReaderId]);

CREATE INDEX [IX_Books_AuthorId] ON [Books] ([AuthorId]);

CREATE INDEX [IX_Books_CategoryId] ON [Books] ([CategoryId]);

CREATE UNIQUE INDEX [IX_Books_Isbn] ON [Books] ([Isbn]);

CREATE INDEX [IX_Books_PublisherId] ON [Books] ([PublisherId]);

CREATE INDEX [IX_Books_ShelfId] ON [Books] ([ShelfId]);

CREATE INDEX [IX_BorrowDetails_BookId] ON [BorrowDetails] ([BookId]);

CREATE UNIQUE INDEX [IX_BorrowDetails_BorrowSlipId_BookId] ON [BorrowDetails] ([BorrowSlipId], [BookId]);

CREATE UNIQUE INDEX [IX_BorrowReturns_IdempotencyKey_BorrowDetailId] ON [BorrowReturns] ([IdempotencyKey], [BorrowDetailId]);

CREATE UNIQUE INDEX [IX_BorrowSlips_Code] ON [BorrowSlips] ([Code]);

CREATE UNIQUE INDEX [IX_BorrowSlips_IdempotencyKey] ON [BorrowSlips] ([IdempotencyKey]);

CREATE INDEX [IX_BorrowSlips_ReaderId] ON [BorrowSlips] ([ReaderId]);

CREATE UNIQUE INDEX [IX_Employees_Email] ON [Employees] ([Email]);

CREATE UNIQUE INDEX [IX_Employees_EmployeeCode] ON [Employees] ([EmployeeCode]);

CREATE UNIQUE INDEX [IX_Employees_NationalId] ON [Employees] ([NationalId]);

CREATE INDEX [IX_Employees_PositionId] ON [Employees] ([PositionId]);

CREATE INDEX [IX_FineDetails_FineId] ON [FineDetails] ([FineId]);

CREATE UNIQUE INDEX [IX_Fines_Code] ON [Fines] ([Code]);

CREATE INDEX [IX_Notifications_RecipientAccountId_IsRead_CreatedAt] ON [Notifications] ([RecipientAccountId], [IsRead], [CreatedAt]);

CREATE UNIQUE INDEX [IX_Permissions_Code] ON [Permissions] ([Code]);

CREATE UNIQUE INDEX [IX_Positions_Code] ON [Positions] ([Code]);

CREATE UNIQUE INDEX [IX_ReaderFeaturedBooks_ReaderId_BookId] ON [ReaderFeaturedBooks] ([ReaderId], [BookId]);

CREATE UNIQUE INDEX [IX_ReaderFeaturedBooks_ReaderId_DisplayOrder] ON [ReaderFeaturedBooks] ([ReaderId], [DisplayOrder]);

CREATE UNIQUE INDEX [IX_Readers_Email] ON [Readers] ([Email]);

CREATE UNIQUE INDEX [IX_Readers_ReaderCode] ON [Readers] ([ReaderCode]);

CREATE UNIQUE INDEX [IX_Readers_UserAccountId] ON [Readers] ([UserAccountId]);

CREATE UNIQUE INDEX [IX_ReadingHistories_BookId_ReaderId] ON [ReadingHistories] ([BookId], [ReaderId]);

CREATE INDEX [IX_Reservations_ReaderId_BookId_Status] ON [Reservations] ([ReaderId], [BookId], [Status]);

CREATE UNIQUE INDEX [IX_RolePermissions_RoleId_PermissionId] ON [RolePermissions] ([RoleId], [PermissionId]);

CREATE UNIQUE INDEX [IX_SecurityRoles_Code] ON [SecurityRoles] ([Code]);

CREATE UNIQUE INDEX [IX_StaffAccounts_EmployeeId] ON [StaffAccounts] ([EmployeeId]);

CREATE UNIQUE INDEX [IX_StaffAccounts_UserAccountId] ON [StaffAccounts] ([UserAccountId]);

CREATE INDEX [IX_SupportMessages_SupportTicketId] ON [SupportMessages] ([SupportTicketId]);

CREATE UNIQUE INDEX [IX_SupportTickets_Code] ON [SupportTickets] ([Code]);

CREATE UNIQUE INDEX [IX_UserPermissions_EmployeeId_PermissionId] ON [UserPermissions] ([EmployeeId], [PermissionId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260730131754_InitialCreate', N'10.0.9');

COMMIT;
GO

BEGIN TRANSACTION;
CREATE INDEX [IX_UserPermissions_PermissionId] ON [UserPermissions] ([PermissionId]);

CREATE INDEX [IX_SupportTickets_AssignedStaffAccountId] ON [SupportTickets] ([AssignedStaffAccountId]);

CREATE INDEX [IX_SupportTickets_ReaderId] ON [SupportTickets] ([ReaderId]);

CREATE INDEX [IX_SupportMessages_SenderAccountId] ON [SupportMessages] ([SenderAccountId]);

CREATE INDEX [IX_RolePermissions_PermissionId] ON [RolePermissions] ([PermissionId]);

CREATE INDEX [IX_Reservations_BookId] ON [Reservations] ([BookId]);

CREATE INDEX [IX_ReadingHistories_ReaderId] ON [ReadingHistories] ([ReaderId]);

CREATE INDEX [IX_ReaderFeaturedBooks_BookId] ON [ReaderFeaturedBooks] ([BookId]);

CREATE INDEX [IX_ProfileTagPins_AuthorId] ON [ProfileTagPins] ([AuthorId]);

CREATE INDEX [IX_ProfileTagPins_ProfileOwnerId] ON [ProfileTagPins] ([ProfileOwnerId]);

CREATE INDEX [IX_ProfileComments_AuthorId] ON [ProfileComments] ([AuthorId]);

CREATE INDEX [IX_ProfileComments_ProfileOwnerId] ON [ProfileComments] ([ProfileOwnerId]);

CREATE INDEX [IX_Notifications_CreatedByAccountId] ON [Notifications] ([CreatedByAccountId]);

CREATE INDEX [IX_Fines_BorrowSlipId] ON [Fines] ([BorrowSlipId]);

CREATE INDEX [IX_Fines_ReaderId] ON [Fines] ([ReaderId]);

CREATE INDEX [IX_FineDetails_BookId] ON [FineDetails] ([BookId]);

CREATE INDEX [IX_FineDetails_BorrowDetailId] ON [FineDetails] ([BorrowDetailId]);

CREATE INDEX [IX_Employees_RoleId] ON [Employees] ([RoleId]);

CREATE INDEX [IX_Employees_StatusOptionId] ON [Employees] ([StatusOptionId]);

CREATE INDEX [IX_CommentReports_BookCommentId] ON [CommentReports] ([BookCommentId]);

CREATE INDEX [IX_CommentReports_ProfileCommentId] ON [CommentReports] ([ProfileCommentId]);

CREATE INDEX [IX_CommentReports_ReportedUserId] ON [CommentReports] ([ReportedUserId]);

CREATE INDEX [IX_CommentReports_ReporterId] ON [CommentReports] ([ReporterId]);

CREATE INDEX [IX_BorrowReturns_BorrowDetailId] ON [BorrowReturns] ([BorrowDetailId]);

CREATE INDEX [IX_BookViewHistories_BookId] ON [BookViewHistories] ([BookId]);

CREATE INDEX [IX_BookViewHistories_ReaderId] ON [BookViewHistories] ([ReaderId]);

CREATE INDEX [IX_BookRatings_ReaderId] ON [BookRatings] ([ReaderId]);

CREATE INDEX [IX_BookFavorites_ReaderId] ON [BookFavorites] ([ReaderId]);

CREATE INDEX [IX_BookComments_BookId] ON [BookComments] ([BookId]);

CREATE INDEX [IX_BookComments_ParentCommentId] ON [BookComments] ([ParentCommentId]);

CREATE INDEX [IX_BookComments_ReaderId] ON [BookComments] ([ReaderId]);

CREATE INDEX [IX_BookCommentLikes_ReaderId] ON [BookCommentLikes] ([ReaderId]);

ALTER TABLE [BookCommentLikes] ADD CONSTRAINT [FK_BookCommentLikes_BookComments_BookCommentId] FOREIGN KEY ([BookCommentId]) REFERENCES [BookComments] ([Id]) ON DELETE CASCADE;

ALTER TABLE [BookCommentLikes] ADD CONSTRAINT [FK_BookCommentLikes_Readers_ReaderId] FOREIGN KEY ([ReaderId]) REFERENCES [Readers] ([Id]);

ALTER TABLE [BookComments] ADD CONSTRAINT [FK_BookComments_BookComments_ParentCommentId] FOREIGN KEY ([ParentCommentId]) REFERENCES [BookComments] ([Id]);

ALTER TABLE [BookComments] ADD CONSTRAINT [FK_BookComments_Books_BookId] FOREIGN KEY ([BookId]) REFERENCES [Books] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [BookComments] ADD CONSTRAINT [FK_BookComments_Readers_ReaderId] FOREIGN KEY ([ReaderId]) REFERENCES [Readers] ([Id]);

ALTER TABLE [BookFavorites] ADD CONSTRAINT [FK_BookFavorites_Books_BookId] FOREIGN KEY ([BookId]) REFERENCES [Books] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [BookFavorites] ADD CONSTRAINT [FK_BookFavorites_Readers_ReaderId] FOREIGN KEY ([ReaderId]) REFERENCES [Readers] ([Id]);

ALTER TABLE [BookRatings] ADD CONSTRAINT [FK_BookRatings_Books_BookId] FOREIGN KEY ([BookId]) REFERENCES [Books] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [BookRatings] ADD CONSTRAINT [FK_BookRatings_Readers_ReaderId] FOREIGN KEY ([ReaderId]) REFERENCES [Readers] ([Id]);

ALTER TABLE [BookViewHistories] ADD CONSTRAINT [FK_BookViewHistories_Books_BookId] FOREIGN KEY ([BookId]) REFERENCES [Books] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [BookViewHistories] ADD CONSTRAINT [FK_BookViewHistories_Readers_ReaderId] FOREIGN KEY ([ReaderId]) REFERENCES [Readers] ([Id]);

ALTER TABLE [BorrowReturns] ADD CONSTRAINT [FK_BorrowReturns_BorrowDetails_BorrowDetailId] FOREIGN KEY ([BorrowDetailId]) REFERENCES [BorrowDetails] ([Id]) ON DELETE CASCADE;

ALTER TABLE [CommentReports] ADD CONSTRAINT [FK_CommentReports_BookComments_BookCommentId] FOREIGN KEY ([BookCommentId]) REFERENCES [BookComments] ([Id]);

ALTER TABLE [CommentReports] ADD CONSTRAINT [FK_CommentReports_ProfileComments_ProfileCommentId] FOREIGN KEY ([ProfileCommentId]) REFERENCES [ProfileComments] ([Id]);

ALTER TABLE [CommentReports] ADD CONSTRAINT [FK_CommentReports_Readers_ReportedUserId] FOREIGN KEY ([ReportedUserId]) REFERENCES [Readers] ([Id]);

ALTER TABLE [CommentReports] ADD CONSTRAINT [FK_CommentReports_Readers_ReporterId] FOREIGN KEY ([ReporterId]) REFERENCES [Readers] ([Id]);

ALTER TABLE [Employees] ADD CONSTRAINT [FK_Employees_EmployeeStatusOptions_StatusOptionId] FOREIGN KEY ([StatusOptionId]) REFERENCES [EmployeeStatusOptions] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Employees] ADD CONSTRAINT [FK_Employees_SecurityRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [SecurityRoles] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [FineDetails] ADD CONSTRAINT [FK_FineDetails_Books_BookId] FOREIGN KEY ([BookId]) REFERENCES [Books] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [FineDetails] ADD CONSTRAINT [FK_FineDetails_BorrowDetails_BorrowDetailId] FOREIGN KEY ([BorrowDetailId]) REFERENCES [BorrowDetails] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Fines] ADD CONSTRAINT [FK_Fines_BorrowSlips_BorrowSlipId] FOREIGN KEY ([BorrowSlipId]) REFERENCES [BorrowSlips] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Fines] ADD CONSTRAINT [FK_Fines_Readers_ReaderId] FOREIGN KEY ([ReaderId]) REFERENCES [Readers] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Notifications] ADD CONSTRAINT [FK_Notifications_AspNetUsers_CreatedByAccountId] FOREIGN KEY ([CreatedByAccountId]) REFERENCES [AspNetUsers] ([Id]);

ALTER TABLE [Notifications] ADD CONSTRAINT [FK_Notifications_AspNetUsers_RecipientAccountId] FOREIGN KEY ([RecipientAccountId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE;

ALTER TABLE [ProfileComments] ADD CONSTRAINT [FK_ProfileComments_Readers_AuthorId] FOREIGN KEY ([AuthorId]) REFERENCES [Readers] ([Id]);

ALTER TABLE [ProfileComments] ADD CONSTRAINT [FK_ProfileComments_Readers_ProfileOwnerId] FOREIGN KEY ([ProfileOwnerId]) REFERENCES [Readers] ([Id]);

ALTER TABLE [ProfileTagPins] ADD CONSTRAINT [FK_ProfileTagPins_Readers_AuthorId] FOREIGN KEY ([AuthorId]) REFERENCES [Readers] ([Id]);

ALTER TABLE [ProfileTagPins] ADD CONSTRAINT [FK_ProfileTagPins_Readers_ProfileOwnerId] FOREIGN KEY ([ProfileOwnerId]) REFERENCES [Readers] ([Id]);

ALTER TABLE [ReaderFeaturedBooks] ADD CONSTRAINT [FK_ReaderFeaturedBooks_Books_BookId] FOREIGN KEY ([BookId]) REFERENCES [Books] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [ReaderFeaturedBooks] ADD CONSTRAINT [FK_ReaderFeaturedBooks_Readers_ReaderId] FOREIGN KEY ([ReaderId]) REFERENCES [Readers] ([Id]) ON DELETE CASCADE;

ALTER TABLE [ReadingHistories] ADD CONSTRAINT [FK_ReadingHistories_Books_BookId] FOREIGN KEY ([BookId]) REFERENCES [Books] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [ReadingHistories] ADD CONSTRAINT [FK_ReadingHistories_Readers_ReaderId] FOREIGN KEY ([ReaderId]) REFERENCES [Readers] ([Id]);

ALTER TABLE [Reservations] ADD CONSTRAINT [FK_Reservations_Books_BookId] FOREIGN KEY ([BookId]) REFERENCES [Books] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Reservations] ADD CONSTRAINT [FK_Reservations_Readers_ReaderId] FOREIGN KEY ([ReaderId]) REFERENCES [Readers] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [RolePermissions] ADD CONSTRAINT [FK_RolePermissions_Permissions_PermissionId] FOREIGN KEY ([PermissionId]) REFERENCES [Permissions] ([Id]) ON DELETE CASCADE;

ALTER TABLE [RolePermissions] ADD CONSTRAINT [FK_RolePermissions_SecurityRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [SecurityRoles] ([Id]) ON DELETE CASCADE;

ALTER TABLE [StaffAccounts] ADD CONSTRAINT [FK_StaffAccounts_AspNetUsers_UserAccountId] FOREIGN KEY ([UserAccountId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE;

ALTER TABLE [StaffAccounts] ADD CONSTRAINT [FK_StaffAccounts_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [SupportMessages] ADD CONSTRAINT [FK_SupportMessages_AspNetUsers_SenderAccountId] FOREIGN KEY ([SenderAccountId]) REFERENCES [AspNetUsers] ([Id]);

ALTER TABLE [SupportTickets] ADD CONSTRAINT [FK_SupportTickets_AspNetUsers_AssignedStaffAccountId] FOREIGN KEY ([AssignedStaffAccountId]) REFERENCES [AspNetUsers] ([Id]);

ALTER TABLE [SupportTickets] ADD CONSTRAINT [FK_SupportTickets_Readers_ReaderId] FOREIGN KEY ([ReaderId]) REFERENCES [Readers] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [UserPermissions] ADD CONSTRAINT [FK_UserPermissions_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE;

ALTER TABLE [UserPermissions] ADD CONSTRAINT [FK_UserPermissions_Permissions_PermissionId] FOREIGN KEY ([PermissionId]) REFERENCES [Permissions] ([Id]) ON DELETE CASCADE;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260730141111_AddDomainRelationships', N'10.0.9');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [BorrowReturns] ADD [FineAmount] decimal(18,2) NOT NULL DEFAULT 0.0;

ALTER TABLE [BorrowReturns] ADD [FineId] int NULL;

ALTER TABLE [BorrowReturns] ADD [ResultSlipId] int NOT NULL DEFAULT 0;

ALTER TABLE [BorrowReturns] ADD [ResultStatus] int NOT NULL DEFAULT 0;

GO
UPDATE [br]
SET [ResultSlipId] = [bd].[BorrowSlipId],
    [ResultStatus] = [bs].[Status],
    [FineId] = [f].[Id],
    [FineAmount] = COALESCE([f].[Amount], 0)
FROM [BorrowReturns] AS [br]
INNER JOIN [BorrowDetails] AS [bd] ON [bd].[Id] = [br].[BorrowDetailId]
INNER JOIN [BorrowSlips] AS [bs] ON [bs].[Id] = [bd].[BorrowSlipId]
OUTER APPLY
(
    SELECT TOP (1) [candidate].[Id], [candidate].[Amount]
    FROM [Fines] AS [candidate]
    WHERE [candidate].[BorrowSlipId] = [bd].[BorrowSlipId]
    ORDER BY [candidate].[CreatedAt] DESC, [candidate].[Id] DESC
) AS [f];

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260730161322_AddReturnReplaySnapshot', N'10.0.9');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Notifications] ADD [CorrelationKey] nvarchar(200) NULL;

ALTER TABLE [Books] ADD [LastLifecycleOperationKey] nvarchar(100) NULL;

ALTER TABLE [Books] ADD [RecallDeadline] datetimeoffset NULL;

ALTER TABLE [Books] ADD [RecallRequestedAt] datetimeoffset NULL;

ALTER TABLE [Books] ADD [RestoreReason] nvarchar(500) NULL;

ALTER TABLE [Books] ADD [RestoredAt] datetimeoffset NULL;

ALTER TABLE [Books] ADD [RestoredByAccountId] uniqueidentifier NULL;

ALTER TABLE [Books] ADD [SuspendedAt] datetimeoffset NULL;

ALTER TABLE [Books] ADD [SuspendedByAccountId] uniqueidentifier NULL;

ALTER TABLE [Books] ADD [SuspensionReason] nvarchar(500) NULL;

ALTER TABLE [Books] ADD [SuspensionType] int NULL;

GO
UPDATE [Books]
SET [Status] = 5,
    [SuspensionType] = 0,
    [SuspensionReason] = N'Chuyển đổi an toàn từ bản ghi xóa mềm trước khi áp dụng vòng đời hai giai đoạn.',
    [SuspendedAt] = COALESCE([DeletedAt], SYSUTCDATETIME()),
    [IsDeleted] = 0,
    [DeletedAt] = NULL
WHERE [IsDeleted] = 1;

GO
CREATE UNIQUE INDEX [IX_Notifications_RecipientAccountId_Type_CorrelationKey] ON [Notifications] ([RecipientAccountId], [Type], [CorrelationKey]) WHERE [CorrelationKey] IS NOT NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260801042438_BookServiceLifecycle', N'10.0.9');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Categories] ADD [ImagePath] nvarchar(2048) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261116121506_AddCategoryImagePath', N'10.0.9');

COMMIT;
GO



-- =========================================================================
-- SEED & CURRENT DATA
-- =========================================================================

-- Table: [AspNetRoles] (3 rows)
INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES
    ('3d0a6fd2-2463-4012-4c53-08df43a158ab', N'Reader', N'READER', N'ab1b9250-24ec-4510-ab27-62d1af15bf63'),
    ('9d2d8e54-e418-4b59-4c54-08df43a158ab', N'Staff', N'STAFF', N'16b2940f-47d3-46d4-82b2-2ab76cb96f8a'),
    ('fd151767-a27f-4536-4c55-08df43a158ab', N'Administrator', N'ADMINISTRATOR', N'4eba7fbd-d74a-4837-9f75-453f6be3bf1f');
GO

-- Table: [AspNetUsers] (5 rows)
INSERT INTO [dbo].[AspNetUsers] ([Id], [AccountKind], [DisplayName], [IsActive], [MustChangePassword], [LastLoginAt], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES
    ('e75d4f16-7ded-455b-8d01-4146d6db230c', 2, N'Nguyễn Hoàng Long', 1, 0, '2026-11-16 14:37:25.1932382 +00:00', N'admin@library.com', N'ADMIN@LIBRARY.COM', N'admin@library.com', N'ADMIN@LIBRARY.COM', 1, N'AQAAAAIAAYagAAAAEO8HYl37NAmT6t6g3eNS0TLAy7OKDLKu4DUfP8hGXoWBMW/Qa432CEn/zYzW1oBAXw==', N'BPUSLQ6I3TIJZAXGJY3T22SHPRNTGWHS', N'647c16ed-586a-4590-b57c-4bcb6789aecd', NULL, 0, 0, NULL, 1, 0),
    ('46eb04e9-b59f-4bdc-b41f-52938b9c4801', 0, N'Lê Phú An', 1, 0, '2026-11-16 14:37:17.2167215 +00:00', N'reader@library.com', N'READER@LIBRARY.COM', N'reader@library.com', N'READER@LIBRARY.COM', 1, N'AQAAAAIAAYagAAAAECd571u5UqSZVRGez6kkSQq3+GkHstwhmab2BKEIqDpdwo4HBuDBPaQjgXAN5QQHzQ==', N'OYLU3A34GCZ4V7BMRQYNEFFHGID7IAFZ', N'f42ab53f-e995-4638-bc1a-35ff70e84d28', NULL, 0, 0, NULL, 1, 0),
    ('adc51e48-f0c5-4403-8400-9607f059f3e3', 0, N'Nguyễn Văn An', 1, 0, '2026-11-16 06:10:38.6508293 +00:00', N'docgia@gmail.com', N'DOCGIA@GMAIL.COM', N'docgia@gmail.com', N'DOCGIA@GMAIL.COM', 1, N'AQAAAAIAAYagAAAAEIUq8I9FGXLGeoN2AcPpdz91KqpNqG90bH1k3ZzQ5sP69CB5hvh97RJPdYqWUvTHrw==', N'AQAFYPSVMRCMBBMNSUL6KX7KJBTNQF4C', N'9225ae76-8827-4de8-a985-46c0627ee936', NULL, 0, 0, NULL, 1, 0),
    ('4b7932d9-a8b8-4d47-8e25-d1de88b05711', 1, N'Lê Văn Hùng', 1, 0, '2026-11-16 13:57:13.5436736 +00:00', N'librarian@library.com', N'LIBRARIAN@LIBRARY.COM', N'librarian@library.com', N'LIBRARIAN@LIBRARY.COM', 1, N'AQAAAAIAAYagAAAAEJwSnALJOiXR/KLWTJlCmlZseT7pbtye1pZjMS910fBN+bRVnbT7FjVTCDwR4p+cAg==', N'TDVLKY6Z3FUP6MJHLO5KTHSEQMXDV5XR', N'c2bd52ff-50b6-4fc1-bbe0-c66b8b1305d0', NULL, 0, 0, NULL, 1, 0),
    ('f08b7129-81f6-459e-a762-e9e6c18de847', 0, N'anh Nam', 1, 0, NULL, N'gamemod5512k@gmail.com', N'GAMEMOD5512K@GMAIL.COM', N'gamemod5512k@gmail.com', N'GAMEMOD5512K@GMAIL.COM', 1, N'AQAAAAIAAYagAAAAEGMGwF0b+M1Z2MqYr0J4T7UOhF/yhbqtIBGuGsyUXu4xhFraN5++DHkxLE5JkD1XxA==', N'27JJ4O6N3LG4DAR26ZH2QSQZWWMSSLMG', N'43c32789-df38-4577-97b6-7091af1a1228', NULL, 0, 0, NULL, 1, 0);
GO

-- Table: [AspNetUserRoles] (5 rows)
INSERT INTO [dbo].[AspNetUserRoles] ([UserId], [RoleId]) VALUES
    ('46eb04e9-b59f-4bdc-b41f-52938b9c4801', '3d0a6fd2-2463-4012-4c53-08df43a158ab'),
    ('adc51e48-f0c5-4403-8400-9607f059f3e3', '3d0a6fd2-2463-4012-4c53-08df43a158ab'),
    ('f08b7129-81f6-459e-a762-e9e6c18de847', '3d0a6fd2-2463-4012-4c53-08df43a158ab'),
    ('4b7932d9-a8b8-4d47-8e25-d1de88b05711', '9d2d8e54-e418-4b59-4c54-08df43a158ab'),
    ('e75d4f16-7ded-455b-8d01-4146d6db230c', 'fd151767-a27f-4536-4c55-08df43a158ab');
GO

-- Table: [SystemSettings] (1 rows)
SET IDENTITY_INSERT [dbo].[SystemSettings] ON;
INSERT INTO [dbo].[SystemSettings] ([Id], [MaxExtensionDays], [MaxExtensionCount], [LateFeePerDay], [MaxBorrowBooks], [BorrowPeriodDays], [PickupWindowDays], [CreatedAt], [UpdatedAt]) VALUES
    (1, 7, 1, 5000.00, 10, 14, 3, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00');
SET IDENTITY_INSERT [dbo].[SystemSettings] OFF;
GO

-- Table: [SecurityRoles] (5 rows)
SET IDENTITY_INSERT [dbo].[SecurityRoles] ON;
INSERT INTO [dbo].[SecurityRoles] ([Id], [Code], [Name], [Description], [IsSystem], [CreatedAt], [UpdatedAt], [IsDeleted], [DeletedAt]) VALUES
    (1, N'DEMO-CATALOG-MANAGER', N'Quản lý biên mục', N'Quản lý sách và toàn bộ dữ liệu tra cứu.', 0, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 13:53:27.0595852 +00:00', 1, NULL),
    (2, N'DEMO-CIRCULATION-STAFF', N'Thủ thư', N'Quản lý duyệt, giao, trả, gia hạn và đặt trước.', 0, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 13:50:32.6484962 +00:00', 0, NULL),
    (3, N'DEMO-LIBRARY-STAFF', N'Nhân viên thư viện', N'Quyền cơ bản để phục vụ độc giả và lưu thông sách.', 0, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 13:53:27.0595852 +00:00', 1, NULL),
    (4, N'DEMO-SUPPORT-STAFF', N'Nhân viên hỗ trợ', N'Tiếp nhận và xử lý ticket hỗ trợ.', 0, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 13:50:32.6484962 +00:00', 1, NULL),
    (5, N'ADMIN', N'Quản trị viên', NULL, 1, '2026-11-16 06:07:03.3118554 +00:00', '2026-11-16 13:50:32.6484962 +00:00', 0, NULL);
SET IDENTITY_INSERT [dbo].[SecurityRoles] OFF;
GO

-- Table: [Permissions] (23 rows)
SET IDENTITY_INSERT [dbo].[Permissions] ON;
INSERT INTO [dbo].[Permissions] ([Id], [Code], [DisplayName], [GroupName], [CreatedAt], [UpdatedAt]) VALUES
    (1, N'book.create', N'Thêm sách', N'Sách', '2026-11-16 05:20:36.2898918 +00:00', '2026-11-16 05:20:36.2898918 +00:00'),
    (2, N'book.edit', N'Sửa sách', N'Sách', '2026-11-16 05:20:36.2898918 +00:00', '2026-11-16 05:20:36.2898918 +00:00'),
    (3, N'book.delete', N'Quyền xóa sách cũ (tương thích)', N'Sách', '2026-11-16 05:20:36.2898918 +00:00', '2026-11-16 05:20:36.2898918 +00:00'),
    (4, N'book.suspend', N'Ngừng phục vụ sách', N'Sách', '2026-11-16 05:20:36.2898918 +00:00', '2026-11-16 05:20:36.2898918 +00:00'),
    (5, N'book.emergency_recall', N'Thu hồi sách khẩn cấp', N'Sách', '2026-11-16 05:20:36.2898918 +00:00', '2026-11-16 05:20:36.2898918 +00:00'),
    (6, N'book.restore', N'Khôi phục sách', N'Sách', '2026-11-16 05:20:36.2898918 +00:00', '2026-11-16 05:20:36.2898918 +00:00'),
    (7, N'book.complete_maintenance', N'Hoàn tất bảo trì sách', N'Sách', '2026-11-16 05:20:36.2898918 +00:00', '2026-11-16 05:20:36.2898918 +00:00'),
    (8, N'book.permanent_delete', N'Xóa vĩnh viễn sách chưa sử dụng', N'Sách', '2026-11-16 05:20:36.2898918 +00:00', '2026-11-16 05:20:36.2898918 +00:00'),
    (9, N'borrow.approve', N'Duyệt hoặc từ chối phiếu mượn', N'Mượn trả', '2026-11-16 05:20:36.2898918 +00:00', '2026-11-16 05:20:36.2898918 +00:00'),
    (10, N'borrow.confirm_pickup', N'Xác nhận giao sách', N'Mượn trả', '2026-11-16 05:20:36.2898918 +00:00', '2026-11-16 05:20:36.2898918 +00:00'),
    (11, N'borrow.approve_extension', N'Duyệt gia hạn', N'Mượn trả', '2026-11-16 05:20:36.2898918 +00:00', '2026-11-16 05:20:36.2898918 +00:00'),
    (12, N'borrow.confirm_return', N'Xác nhận trả sách', N'Mượn trả', '2026-11-16 05:20:36.2898918 +00:00', '2026-11-16 05:20:36.2898918 +00:00'),
    (13, N'reservation.manage', N'Quản lý đặt trước', N'Đặt trước', '2026-11-16 05:20:36.2898918 +00:00', '2026-11-16 05:20:36.2898918 +00:00'),
    (14, N'support.claim', N'Tiếp nhận yêu cầu hỗ trợ', N'Hỗ trợ', '2026-11-16 05:20:36.2898918 +00:00', '2026-11-16 05:20:36.2898918 +00:00'),
    (15, N'employee.view', N'Xem nhân viên', N'Nhân sự', '2026-11-16 05:20:36.2898918 +00:00', '2026-11-16 05:20:36.2898918 +00:00'),
    (16, N'employee.create', N'Thêm nhân viên', N'Nhân sự', '2026-11-16 05:20:36.2898918 +00:00', '2026-11-16 05:20:36.2898918 +00:00'),
    (17, N'employee.edit', N'Sửa nhân viên', N'Nhân sự', '2026-11-16 05:20:36.2898918 +00:00', '2026-11-16 05:20:36.2898918 +00:00'),
    (18, N'employee.delete', N'Xóa nhân viên', N'Nhân sự', '2026-11-16 05:20:36.2898918 +00:00', '2026-11-16 05:20:36.2898918 +00:00'),
    (19, N'reader.view', N'Xem độc giả', N'Độc giả', '2026-11-16 05:20:36.2898918 +00:00', '2026-11-16 05:20:36.2898918 +00:00'),
    (20, N'reader.edit', N'Sửa hồ sơ độc giả', N'Độc giả', '2026-11-16 05:20:36.2898918 +00:00', '2026-11-16 05:20:36.2898918 +00:00'),
    (21, N'report.view', N'Xem báo cáo', N'Báo cáo', '2026-11-16 05:20:36.2898918 +00:00', '2026-11-16 05:20:36.2898918 +00:00'),
    (22, N'catalog.view', N'Xem danh mục', N'Danh mục', '2026-11-16 05:20:36.2898918 +00:00', '2026-11-16 05:20:36.2898918 +00:00'),
    (23, N'catalog.manage', N'Quản lý danh mục', N'Danh mục', '2026-11-16 05:20:36.2898918 +00:00', '2026-11-16 05:20:36.2898918 +00:00');
SET IDENTITY_INSERT [dbo].[Permissions] OFF;
GO

-- Table: [RolePermissions] (91 rows)
SET IDENTITY_INSERT [dbo].[RolePermissions] ON;
INSERT INTO [dbo].[RolePermissions] ([Id], [RoleId], [PermissionId], [Effect], [CreatedAt], [UpdatedAt]) VALUES
    (1, 1, 1, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (2, 1, 2, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (3, 1, 4, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (4, 1, 6, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (5, 1, 7, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (6, 1, 9, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (7, 1, 10, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (8, 1, 11, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (9, 1, 12, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (10, 1, 13, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (11, 1, 14, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (12, 1, 15, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (13, 1, 19, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (14, 1, 20, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (15, 1, 21, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (16, 1, 22, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (17, 1, 23, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (18, 2, 1, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (19, 2, 2, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (20, 2, 4, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (21, 2, 6, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (22, 2, 7, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (23, 2, 9, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (24, 2, 10, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (25, 2, 11, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (26, 2, 12, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (27, 2, 13, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (28, 2, 14, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (29, 2, 15, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (30, 2, 19, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (31, 2, 20, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (32, 2, 21, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (33, 2, 22, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (34, 2, 23, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (35, 3, 1, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (36, 3, 2, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (37, 3, 4, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (38, 3, 6, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (39, 3, 7, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (40, 3, 9, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (41, 3, 10, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (42, 3, 11, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (43, 3, 12, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (44, 3, 13, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (45, 3, 14, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (46, 3, 15, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (47, 3, 19, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (48, 3, 20, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (49, 3, 21, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (50, 3, 22, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00');
INSERT INTO [dbo].[RolePermissions] ([Id], [RoleId], [PermissionId], [Effect], [CreatedAt], [UpdatedAt]) VALUES
    (51, 3, 23, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (52, 4, 1, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (53, 4, 2, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (54, 4, 4, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (55, 4, 6, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (56, 4, 7, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (57, 4, 9, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (58, 4, 10, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (59, 4, 11, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (60, 4, 12, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (61, 4, 13, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (62, 4, 14, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (63, 4, 15, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (64, 4, 19, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (65, 4, 20, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (66, 4, 21, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (67, 4, 22, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (68, 4, 23, 1, '2026-11-16 05:20:36.5335571 +00:00', '2026-11-16 05:20:36.5335571 +00:00'),
    (69, 5, 1, 1, '2026-11-16 06:07:03.3585538 +00:00', '2026-11-16 06:07:03.3585538 +00:00'),
    (70, 5, 2, 1, '2026-11-16 06:07:03.3585538 +00:00', '2026-11-16 06:07:03.3585538 +00:00'),
    (71, 5, 3, 1, '2026-11-16 06:07:03.3585538 +00:00', '2026-11-16 06:07:03.3585538 +00:00'),
    (72, 5, 4, 1, '2026-11-16 06:07:03.3585538 +00:00', '2026-11-16 06:07:03.3585538 +00:00'),
    (73, 5, 5, 1, '2026-11-16 06:07:03.3585538 +00:00', '2026-11-16 06:07:03.3585538 +00:00'),
    (74, 5, 6, 1, '2026-11-16 06:07:03.3585538 +00:00', '2026-11-16 06:07:03.3585538 +00:00'),
    (75, 5, 7, 1, '2026-11-16 06:07:03.3585538 +00:00', '2026-11-16 06:07:03.3585538 +00:00'),
    (76, 5, 8, 1, '2026-11-16 06:07:03.3585538 +00:00', '2026-11-16 06:07:03.3585538 +00:00'),
    (77, 5, 9, 1, '2026-11-16 06:07:03.3585538 +00:00', '2026-11-16 06:07:03.3585538 +00:00'),
    (78, 5, 10, 1, '2026-11-16 06:07:03.3585538 +00:00', '2026-11-16 06:07:03.3585538 +00:00'),
    (79, 5, 11, 1, '2026-11-16 06:07:03.3585538 +00:00', '2026-11-16 06:07:03.3585538 +00:00'),
    (80, 5, 12, 1, '2026-11-16 06:07:03.3585538 +00:00', '2026-11-16 06:07:03.3585538 +00:00'),
    (81, 5, 13, 1, '2026-11-16 06:07:03.3585538 +00:00', '2026-11-16 06:07:03.3585538 +00:00'),
    (82, 5, 14, 1, '2026-11-16 06:07:03.3585538 +00:00', '2026-11-16 06:07:03.3585538 +00:00'),
    (83, 5, 15, 1, '2026-11-16 06:07:03.3585538 +00:00', '2026-11-16 06:07:03.3585538 +00:00'),
    (84, 5, 16, 1, '2026-11-16 06:07:03.3585538 +00:00', '2026-11-16 06:07:03.3585538 +00:00'),
    (85, 5, 17, 1, '2026-11-16 06:07:03.3585538 +00:00', '2026-11-16 06:07:03.3585538 +00:00'),
    (86, 5, 18, 1, '2026-11-16 06:07:03.3585538 +00:00', '2026-11-16 06:07:03.3585538 +00:00'),
    (87, 5, 19, 1, '2026-11-16 06:07:03.3585538 +00:00', '2026-11-16 06:07:03.3585538 +00:00'),
    (88, 5, 20, 1, '2026-11-16 06:07:03.3585538 +00:00', '2026-11-16 06:07:03.3585538 +00:00'),
    (89, 5, 21, 1, '2026-11-16 06:07:03.3585538 +00:00', '2026-11-16 06:07:03.3585538 +00:00'),
    (90, 5, 22, 1, '2026-11-16 06:07:03.3585538 +00:00', '2026-11-16 06:07:03.3585538 +00:00'),
    (91, 5, 23, 1, '2026-11-16 06:07:03.3585538 +00:00', '2026-11-16 06:07:03.3585538 +00:00');
SET IDENTITY_INSERT [dbo].[RolePermissions] OFF;
GO

-- Table: [Positions] (5 rows)
SET IDENTITY_INSERT [dbo].[Positions] ON;
INSERT INTO [dbo].[Positions] ([Id], [Code], [Name], [Description], [SalaryAmount], [AllowSalaryOverride], [CreatedAt], [UpdatedAt], [IsDeleted], [DeletedAt]) VALUES
    (1, N'DEMO-CATALOG', N'Chuyên viên biên mục', N'Quản lý sách, tác giả, nhà xuất bản, danh mục và kệ.', 14000000.00, 1, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 13:53:27.0595852 +00:00', 1, NULL),
    (2, N'DEMO-CIRCULATION', N'Chuyên viên lưu thông', N'Theo dõi phiếu mượn, đặt trước, gia hạn và khoản phạt.', 13500000.00, 1, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 13:53:27.0595852 +00:00', 1, NULL),
    (3, N'DEMO-LIBRARIAN', N'Thủ thư', N'Phụ trách mượn, trả và hỗ trợ độc giả tại quầy.', 12000000.00, 1, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (4, N'DEMO-SUPPORT', N'Chuyên viên hỗ trợ', N'Tiếp nhận ticket và hướng dẫn độc giả sử dụng hệ thống.', 11500000.00, 1, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 13:50:32.6484962 +00:00', 1, NULL),
    (5, N'ADMIN', N'Quản trị viên', NULL, 25000000.00, 1, '2026-11-16 06:07:03.2194966 +00:00', '2026-11-16 06:07:03.2194966 +00:00', 0, NULL);
SET IDENTITY_INSERT [dbo].[Positions] OFF;
GO

-- Table: [EmployeeStatusOptions] (4 rows)
SET IDENTITY_INSERT [dbo].[EmployeeStatusOptions] ON;
INSERT INTO [dbo].[EmployeeStatusOptions] ([Id], [Code], [Name], [Description], [DisplayOrder], [Status], [CreatedAt], [UpdatedAt], [IsDeleted], [DeletedAt]) VALUES
    (1, N'DEMO-LEAVE', N'Nghỉ phép', N'Nhân viên đang trong kỳ nghỉ đã được duyệt.', 20, 0, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (2, N'DEMO-RESIGNED', N'Đã nghỉ việc', N'Hồ sơ nhân viên đã kết thúc công tác.', 40, 2, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (3, N'DEMO-SUSPENDED', N'Tạm đình chỉ', N'Tạm ngừng quyền xử lý nghiệp vụ.', 30, 1, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (4, N'DEMO-WORKING', N'Đang làm việc', N'Nhân viên đang làm việc bình thường.', 10, 0, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL);
SET IDENTITY_INSERT [dbo].[EmployeeStatusOptions] OFF;
GO

-- Table: [Employees] (5 rows)
SET IDENTITY_INSERT [dbo].[Employees] ON;
INSERT INTO [dbo].[Employees] ([Id], [EmployeeCode], [FullName], [Email], [Phone], [Address], [NationalId], [Gender], [DateOfBirth], [HiredOn], [PositionId], [Salary], [Status], [StatusOptionId], [LeaveReason], [LeaveStartedOn], [UnexcusedDays], [LastUnexcusedOn], [RoleId], [CreatedAt], [UpdatedAt], [IsDeleted], [DeletedAt]) VALUES
    (1, N'NV001', N'Nguyễn Hoàng Long', N'admin@library.com', N'0901234567', N'Hà Nội', N'001209795512', 0, '1995-01-01', '2022-01-01', 5, 25000000.00, 0, NULL, NULL, NULL, 0, NULL, 5, '2026-11-16 06:07:03.8716649 +00:00', '2026-11-16 06:07:03.8716649 +00:00', 0, NULL),
    (2, N'NV002', N'Trần Thị Bích Ngọc', N'manager@library.com', N'0902345678', N'Hà Nội', N'001202026465', 0, '1995-01-01', '2022-01-01', 1, 14000000.00, 0, NULL, NULL, NULL, 0, NULL, 1, '2026-11-16 06:07:04.0446211 +00:00', '2026-11-16 13:22:10.1680580 +00:00', 1, NULL),
    (3, N'NV003', N'Lê Văn Hùng', N'librarian@library.com', N'0903456789', N'Hà Nội', N'001209817522', 0, '1995-01-01', '2022-01-01', 3, 12000000.00, 0, NULL, NULL, NULL, 0, NULL, 2, '2026-11-16 06:07:04.1461636 +00:00', '2026-11-16 06:07:04.1461636 +00:00', 0, NULL),
    (4, N'NV004', N'Phạm Đức Thắng', N'warehouse@library.com', N'0904567890', N'Hà Nội', N'001201886607', 0, '1995-01-01', '2022-01-01', 2, 13500000.00, 0, NULL, NULL, NULL, 0, NULL, 3, '2026-11-16 06:07:04.2373776 +00:00', '2026-11-16 13:22:10.2446698 +00:00', 1, NULL),
    (5, N'NV005', N'Đỗ Quang Huy', N'support@library.com', N'0905678901', N'Hà Nội', N'001201085805', 0, '1995-01-01', '2022-01-01', 4, 11500000.00, 0, NULL, NULL, NULL, 0, NULL, 4, '2026-11-16 06:07:04.3601248 +00:00', '2026-11-16 13:22:10.2554626 +00:00', 1, NULL);
SET IDENTITY_INSERT [dbo].[Employees] OFF;
GO

-- Table: [StaffAccounts] (2 rows)
SET IDENTITY_INSERT [dbo].[StaffAccounts] ON;
INSERT INTO [dbo].[StaffAccounts] ([Id], [UserAccountId], [EmployeeId], [CreatedAt], [UpdatedAt]) VALUES
    (1, 'e75d4f16-7ded-455b-8d01-4146d6db230c', 1, '2026-11-16 06:07:03.9276978 +00:00', '2026-11-16 06:07:03.9276978 +00:00'),
    (3, '4b7932d9-a8b8-4d47-8e25-d1de88b05711', 3, '2026-11-16 06:07:04.1545266 +00:00', '2026-11-16 06:07:04.1545266 +00:00');
SET IDENTITY_INSERT [dbo].[StaffAccounts] OFF;
GO

-- Table: [Categories] (8 rows)
SET IDENTITY_INSERT [dbo].[Categories] ON;
INSERT INTO [dbo].[Categories] ([Id], [Name], [Description], [CreatedAt], [UpdatedAt], [IsDeleted], [DeletedAt], [ImagePath]) VALUES
    (1, N'Khoa học tự nhiên', N'Sinh học, vật lý và các chủ đề khoa học phổ thông.', '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:19:49.2828705 +00:00', 0, NULL, N'/uploads/categories/khoa-hoc-tu-nhien.jpg'),
    (2, N'Khoa học viễn tưởng', N'Tiểu thuyết khám phá công nghệ, không gian và xã hội tương lai.', '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:19:49.2828705 +00:00', 0, NULL, N'/uploads/categories/khoa-hoc-vien-tuong.jpg'),
    (3, N'Lập trình & bảo mật', N'Lập trình, kiến trúc máy tính, lịch sử hacker và an toàn thông tin.', '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:19:49.2828705 +00:00', 0, NULL, N'/uploads/categories/lap-trinh-bao-mat.jpg'),
    (4, N'Phát triển bản thân', N'Thói quen, năng suất và tư duy phát triển bền vững.', '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:19:49.2828705 +00:00', 0, NULL, N'/uploads/categories/phat-trien-ban-than.jpg'),
    (5, N'Tâm lý & chữa lành', N'Sách về nhận thức, cảm xúc và hành trình hồi phục.', '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:19:49.2828705 +00:00', 0, NULL, N'/uploads/categories/tam-ly-chua-lanh.jpg'),
    (6, N'Tiểu thuyết đương đại', N'Tác phẩm hư cấu hiện đại dành cho bạn đọc trẻ và trưởng thành.', '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:19:49.2828705 +00:00', 0, NULL, N'/uploads/categories/tieu-thuyet-duong-dai.jpg'),
    (7, N'Toán & khoa học máy tính', N'Toán rời rạc, thuật toán và nền tảng khoa học máy tính.', '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:19:49.2828705 +00:00', 0, NULL, N'/uploads/categories/toan-khoa-hoc-may-tinh.jpg'),
    (8, N'Văn học kinh điển', N'Những tác phẩm có ảnh hưởng lâu dài trong lịch sử văn học.', '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:19:49.2828705 +00:00', 0, NULL, N'/uploads/categories/van-hoc-kinh-dien.jpg');
SET IDENTITY_INSERT [dbo].[Categories] OFF;
GO

-- Table: [Authors] (28 rows)
SET IDENTITY_INSERT [dbo].[Authors] ON;
INSERT INTO [dbo].[Authors] ([Id], [Name], [DateOfBirth], [Nationality], [Biography], [CreatedAt], [UpdatedAt], [IsDeleted], [DeletedAt]) VALUES
    (1, N'Alex Michaelides', NULL, NULL, N'Tác giả của đầu sách trong bộ dữ liệu trình diễn LibraryHub. Metadata bìa tham chiếu Open Library.', '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (2, N'Arkady & Boris Strugatsky', NULL, NULL, N'Tác giả của đầu sách trong bộ dữ liệu trình diễn LibraryHub. Metadata bìa tham chiếu Open Library.', '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (3, N'Arthur C. Clarke', NULL, NULL, N'Tác giả của đầu sách trong bộ dữ liệu trình diễn LibraryHub. Metadata bìa tham chiếu Open Library.', '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (4, N'Bianca Sparacino', NULL, NULL, N'Tác giả của đầu sách trong bộ dữ liệu trình diễn LibraryHub. Metadata bìa tham chiếu Open Library.', '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (5, N'Brian Tracy', NULL, NULL, N'Tác giả của đầu sách trong bộ dữ liệu trình diễn LibraryHub. Metadata bìa tham chiếu Open Library.', '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (6, N'Brianna Wiest', NULL, NULL, N'Tác giả của đầu sách trong bộ dữ liệu trình diễn LibraryHub. Metadata bìa tham chiếu Open Library.', '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (7, N'Charles Petzold', NULL, NULL, N'Tác giả của đầu sách trong bộ dữ liệu trình diễn LibraryHub. Metadata bìa tham chiếu Open Library.', '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (8, N'Colleen Hoover', NULL, NULL, N'Tác giả của đầu sách trong bộ dữ liệu trình diễn LibraryHub. Metadata bìa tham chiếu Open Library.', '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (9, N'Dan Simmons', NULL, NULL, N'Tác giả của đầu sách trong bộ dữ liệu trình diễn LibraryHub. Metadata bìa tham chiếu Open Library.', '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (10, N'Holly Black', NULL, NULL, N'Tác giả của đầu sách trong bộ dữ liệu trình diễn LibraryHub. Metadata bìa tham chiếu Open Library.', '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (11, N'Iain M. Banks', NULL, NULL, N'Tác giả của đầu sách trong bộ dữ liệu trình diễn LibraryHub. Metadata bìa tham chiếu Open Library.', '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (12, N'Isaac Asimov', NULL, NULL, N'Tác giả của đầu sách trong bộ dữ liệu trình diễn LibraryHub. Metadata bìa tham chiếu Open Library.', '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (13, N'James Clear', NULL, NULL, N'Tác giả của đầu sách trong bộ dữ liệu trình diễn LibraryHub. Metadata bìa tham chiếu Open Library.', '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (14, N'Jeff Kinney', NULL, NULL, N'Tác giả của đầu sách trong bộ dữ liệu trình diễn LibraryHub. Metadata bìa tham chiếu Open Library.', '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (15, N'Jenny Han', NULL, NULL, N'Tác giả của đầu sách trong bộ dữ liệu trình diễn LibraryHub. Metadata bìa tham chiếu Open Library.', '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (16, N'Jordan B. Peterson', NULL, NULL, N'Tác giả của đầu sách trong bộ dữ liệu trình diễn LibraryHub. Metadata bìa tham chiếu Open Library.', '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (17, N'Joseph Murphy', NULL, NULL, N'Tác giả của đầu sách trong bộ dữ liệu trình diễn LibraryHub. Metadata bìa tham chiếu Open Library.', '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (18, N'Joseph Nguyen', NULL, NULL, N'Tác giả của đầu sách trong bộ dữ liệu trình diễn LibraryHub. Metadata bìa tham chiếu Open Library.', '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (19, N'Kevin Beaver', NULL, NULL, N'Tác giả của đầu sách trong bộ dữ liệu trình diễn LibraryHub. Metadata bìa tham chiếu Open Library.', '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (20, N'Lưu Từ Hân', NULL, NULL, N'Tác giả của đầu sách trong bộ dữ liệu trình diễn LibraryHub. Metadata bìa tham chiếu Open Library.', '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (21, N'Mark Lutz', NULL, NULL, N'Tác giả của đầu sách trong bộ dữ liệu trình diễn LibraryHub. Metadata bìa tham chiếu Open Library.', '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (22, N'Mary Shelley', NULL, NULL, N'Tác giả của đầu sách trong bộ dữ liệu trình diễn LibraryHub. Metadata bìa tham chiếu Open Library.', '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (23, N'Neil A. Campbell', NULL, NULL, N'Tác giả của đầu sách trong bộ dữ liệu trình diễn LibraryHub. Metadata bìa tham chiếu Open Library.', '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (24, N'Oscar Wilde', NULL, NULL, N'Tác giả của đầu sách trong bộ dữ liệu trình diễn LibraryHub. Metadata bìa tham chiếu Open Library.', '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (25, N'Steven Levy', NULL, NULL, N'Tác giả của đầu sách trong bộ dữ liệu trình diễn LibraryHub. Metadata bìa tham chiếu Open Library.', '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (26, N'Susanna S. Epp', NULL, NULL, N'Tác giả của đầu sách trong bộ dữ liệu trình diễn LibraryHub. Metadata bìa tham chiếu Open Library.', '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (27, N'Tahereh Mafi', NULL, NULL, N'Tác giả của đầu sách trong bộ dữ liệu trình diễn LibraryHub. Metadata bìa tham chiếu Open Library.', '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (28, N'William Gibson', NULL, NULL, N'Tác giả của đầu sách trong bộ dữ liệu trình diễn LibraryHub. Metadata bìa tham chiếu Open Library.', '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL);
SET IDENTITY_INSERT [dbo].[Authors] OFF;
GO

-- Table: [Publishers] (31 rows)
SET IDENTITY_INSERT [dbo].[Publishers] ON;
INSERT INTO [dbo].[Publishers] ([Id], [Name], [Address], [Phone], [Email], [Website], [CreatedAt], [UpdatedAt], [IsDeleted], [DeletedAt]) VALUES
    (1, N'Actes Sud', NULL, NULL, NULL, NULL, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (2, N'Addison-Wesley', NULL, NULL, NULL, NULL, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (3, N'Anchor Press/Doubleday', NULL, NULL, NULL, NULL, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (4, N'Atria Books', NULL, NULL, NULL, NULL, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (5, N'Audio Literature', NULL, NULL, NULL, NULL, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (6, N'Authors Equity', NULL, NULL, NULL, NULL, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (7, N'Classic Comic Store', NULL, NULL, NULL, NULL, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (8, N'Debolsillo', NULL, NULL, NULL, NULL, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (9, N'Editorial Planeta', NULL, NULL, NULL, NULL, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (10, N'Granada', NULL, NULL, NULL, NULL, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (11, N'HarperCollins', NULL, NULL, NULL, NULL, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (12, N'Head of Zeus', NULL, NULL, NULL, NULL, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (13, N'Holly Black Editions', NULL, NULL, NULL, NULL, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (14, N'Illustrated Editions Co.', NULL, NULL, NULL, NULL, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (15, N'Microsoft Press', NULL, NULL, NULL, NULL, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (16, N'MLBD', NULL, NULL, NULL, NULL, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (17, N'Orbit', NULL, NULL, NULL, NULL, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (18, N'O''Reilly', NULL, NULL, NULL, NULL, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (19, N'Orion Publishing Group', NULL, NULL, NULL, NULL, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (20, N'Planeta', NULL, NULL, NULL, NULL, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (21, N'Presvika', NULL, NULL, NULL, NULL, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (22, N'Robert Laffont', NULL, NULL, NULL, NULL, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (23, N'Simon & Schuster BFYR', NULL, NULL, NULL, NULL, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (24, N'Spectra/Bantam Books', NULL, NULL, NULL, NULL, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (25, N'Stalker Editions', NULL, NULL, NULL, NULL, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (26, N'Thought Catalog', NULL, NULL, NULL, NULL, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (27, N'Thought Catalog Books', NULL, NULL, NULL, NULL, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (28, N'Wadsworth Pub. Co.', NULL, NULL, NULL, NULL, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (29, N'Wildside Press', NULL, NULL, NULL, NULL, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (30, N'Wiley Pub.', NULL, NULL, NULL, NULL, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (31, N'Wilhelm Heyne Verlag', NULL, NULL, NULL, NULL, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL);
SET IDENTITY_INSERT [dbo].[Publishers] OFF;
GO

-- Table: [Shelves] (8 rows)
SET IDENTITY_INSERT [dbo].[Shelves] ON;
INSERT INTO [dbo].[Shelves] ([Id], [Code], [LocationCode], [Name], [Area], [Description], [Capacity], [CreatedAt], [UpdatedAt], [IsDeleted], [DeletedAt]) VALUES
    (1, N'DEMO-SC01', N'LH-DEMO-SC01', N'Khoa học tự nhiên', N'Tầng 2', N'Kệ dữ liệu mẫu: Khoa học tự nhiên', 300, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (2, N'DEMO-SF01', N'LH-DEMO-SF01', N'Khoa học viễn tưởng', N'Tầng 1', N'Kệ dữ liệu mẫu: Khoa học viễn tưởng', 300, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (3, N'DEMO-CS02', N'LH-DEMO-CS02', N'Lập trình và bảo mật', N'Tầng 2', N'Kệ dữ liệu mẫu: Lập trình & bảo mật', 300, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (4, N'DEMO-SH01', N'LH-DEMO-SH01', N'Phát triển bản thân', N'Tầng 1', N'Kệ dữ liệu mẫu: Phát triển bản thân', 300, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (5, N'DEMO-SH02', N'LH-DEMO-SH02', N'Tâm lý và chữa lành', N'Tầng 1', N'Kệ dữ liệu mẫu: Tâm lý & chữa lành', 300, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (6, N'DEMO-FI01', N'LH-DEMO-FI01', N'Tiểu thuyết đương đại', N'Tầng 1', N'Kệ dữ liệu mẫu: Tiểu thuyết đương đại', 300, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (7, N'DEMO-CS01', N'LH-DEMO-CS01', N'Toán và khoa học máy tính', N'Tầng 2', N'Kệ dữ liệu mẫu: Toán & khoa học máy tính', 300, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL),
    (8, N'DEMO-FI02', N'LH-DEMO-FI02', N'Văn học kinh điển', N'Tầng 1', N'Kệ dữ liệu mẫu: Văn học kinh điển', 300, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL);
SET IDENTITY_INSERT [dbo].[Shelves] OFF;
GO

-- Table: [Books] (32 rows)
SET IDENTITY_INSERT [dbo].[Books] ON;
INSERT INTO [dbo].[Books] ([Id], [Isbn], [Title], [Description], [Edition], [Keywords], [PublicationYear], [CreatedBy], [PageCount], [Language], [LastBorrowedAt], [Price], [TotalQuantity], [AvailableQuantity], [MinimumAge], [CoverImagePath], [Status], [ViewCount], [CategoryId], [AuthorId], [PublisherId], [ShelfId], [CreatedAt], [UpdatedAt], [IsDeleted], [DeletedAt], [LastLifecycleOperationKey], [RecallDeadline], [RecallRequestedAt], [RestoreReason], [RestoredAt], [RestoredByAccountId], [SuspendedAt], [SuspendedByAccountId], [SuspensionReason], [SuspensionType]) VALUES
    (1, N'9781250237170', N'The Silent Patient', N'Tiểu thuyết tâm lý bí ẩn xoay quanh một nữ họa sĩ im lặng sau biến cố.', N'Bản dữ liệu trình diễn', N'tâm lý, bí ẩn, điều tra', 2018, N'LibraryHub rich demo seed', 352, N'Tiếng Anh', '2026-11-14 12:20:11.5067114 +07:00', 169000.00, 12, 8, 16, N'https://covers.openlibrary.org/b/id/9407338-L.jpg?default=false', 0, 406, 6, 1, 19, 6, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
    (2, N'9780671458423', N'Roadside Picnic', N'Tiểu thuyết khoa học viễn tưởng về vùng cấm, di vật ngoài hành tinh và lòng tham.', N'Bản dữ liệu trình diễn', N'vùng cấm, ngoài hành tinh, triết học', 1978, N'LibraryHub rich demo seed', 170, N'Tiếng Anh', NULL, 135000.00, 6, 3, 16, N'https://covers.openlibrary.org/b/id/6752719-L.jpg?default=false', 0, 143, 2, 2, 25, 2, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
    (3, N'9781850180005', N'2001: A Space Odyssey', N'Hành trình khoa học viễn tưởng kinh điển về trí tuệ nhân tạo và khám phá không gian.', N'Bản dữ liệu trình diễn', N'không gian, trí tuệ nhân tạo, tương lai', 1968, N'LibraryHub rich demo seed', 237, N'Tiếng Anh', '2026-11-15 12:20:11.5067114 +07:00', 155000.00, 9, 6, 14, N'https://covers.openlibrary.org/b/id/11344400-L.jpg?default=false', 0, 323, 2, 3, 8, 2, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
    (4, N'9781949759297', N'A Gentle Reminder', N'Những lời nhắc dịu dàng về giá trị bản thân, ranh giới và sự trưởng thành.', N'Bản dữ liệu trình diễn', N'giá trị bản thân, chữa lành, cảm xúc', 2020, N'LibraryHub rich demo seed', 146, N'Tiếng Anh', NULL, 119000.00, 9, 6, 14, N'https://covers.openlibrary.org/b/id/14424196-L.jpg?default=false', 0, 191, 5, 4, 26, 5, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
    (5, N'9781605090962', N'Eat That Frog!', N'Phương pháp ưu tiên việc quan trọng và hạn chế trì hoãn trong công việc.', N'Bản dữ liệu trình diễn', N'năng suất, trì hoãn, quản lý thời gian', 2001, N'LibraryHub rich demo seed', 128, N'Tiếng Anh', NULL, 99000.00, 14, 11, 14, N'https://covers.openlibrary.org/b/id/847534-L.jpg?default=false', 0, 265, 4, 5, 5, 4, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
    (6, N'9781949759440', N'When You''re Ready, This Is How You Heal', N'Các bài viết ngắn về quá trình hồi phục và xây dựng lại kết nối với chính mình.', N'Bản dữ liệu trình diễn', N'hồi phục, chữa lành, tự nhận thức', 2022, N'LibraryHub rich demo seed', 224, N'Tiếng Anh', NULL, 155000.00, 7, 4, 14, N'https://covers.openlibrary.org/b/id/15000579-L.jpg?default=false', 0, 173, 5, 6, 27, 5, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
    (7, N'9786070788734', N'The Mountain Is You', N'Hướng dẫn nhận diện hành vi tự cản trở và xây dựng khả năng thích nghi cảm xúc.', N'Bản dữ liệu trình diễn', N'chữa lành, cảm xúc, thay đổi', 2020, N'LibraryHub rich demo seed', 248, N'Tiếng Anh', NULL, 149000.00, 11, 7, 14, N'https://covers.openlibrary.org/b/id/13838236-L.jpg?default=false', 0, 239, 5, 6, 27, 5, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
    (8, N'9780735605053', N'Code', N'Giải thích cách máy tính hoạt động từ mã Morse, mạch điện đến kiến trúc phần mềm.', N'Bản dữ liệu trình diễn', N'mã hóa, kiến trúc máy tính, nền tảng', 1999, N'LibraryHub rich demo seed', 393, N'Tiếng Anh', '2026-10-11 12:20:11.5067114 +07:00', 198000.00, 10, 7, 14, N'https://covers.openlibrary.org/b/id/460968-L.jpg?default=false', 0, 276, 3, 7, 15, 3, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
    (9, N'9788408258360', N'It Ends With Us', N'Tiểu thuyết đương đại về tình yêu, lựa chọn khó khăn và sức mạnh để thay đổi.', N'Bản dữ liệu trình diễn', N'tình cảm, gia đình, lựa chọn', 2012, N'LibraryHub rich demo seed', 384, N'Tiếng Anh', NULL, 179000.00, 11, 7, 16, N'https://covers.openlibrary.org/b/id/10473609-L.jpg?default=false', 0, 376, 6, 8, 4, 6, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
    (10, N'9781491514931', N'The Fall of Hyperion', N'Tiếp nối hành trình khoa học viễn tưởng về trí tuệ nhân tạo và số phận nhân loại.', N'Bản dữ liệu trình diễn', N'không gian, trí tuệ nhân tạo, sử thi', 1990, N'LibraryHub rich demo seed', 561, N'Tiếng Anh', NULL, 185000.00, 7, 4, 16, N'https://covers.openlibrary.org/b/id/484900-L.jpg?default=false', 0, 172, 2, 9, 22, 2, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
    (11, N'9781955876162', N'The Cruel Prince', N'Tiểu thuyết kỳ ảo về quyền lực, âm mưu và hành trình tìm vị trí trong thế giới tiên.', N'Bản dữ liệu trình diễn', N'kỳ ảo, quyền lực, tuổi trẻ', 2018, N'LibraryHub rich demo seed', 370, N'Tiếng Anh', NULL, 165000.00, 10, 6, 13, N'https://covers.openlibrary.org/b/id/8361789-L.jpg?default=false', 0, 218, 6, 10, 13, 6, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
    (12, N'9780356521640', N'The Player of Games', N'Tiểu thuyết về trò chơi, quyền lực và một nền văn minh ngoài Trái Đất.', N'Bản dữ liệu trình diễn', N'không gian, trò chơi, xã hội', 1988, N'LibraryHub rich demo seed', 320, N'Tiếng Anh', NULL, 145000.00, 8, 5, 16, N'https://covers.openlibrary.org/b/id/44077-L.jpg?default=false', 0, 119, 2, 11, 17, 2, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
    (13, N'9783453318670', N'The Foundation Trilogy', N'Bộ sử thi khoa học viễn tưởng về khoa học dự báo và sự suy tàn của đế chế thiên hà.', N'Bản dữ liệu trình diễn', N'đế chế, thiên hà, khoa học dự báo', 1950, N'LibraryHub rich demo seed', 605, N'Tiếng Anh', NULL, 229000.00, 8, 4, 14, N'https://covers.openlibrary.org/b/id/6502517-L.jpg?default=false', 0, 421, 2, 12, 31, 2, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
    (14, N'9788427014060', N'The Caves of Steel', N'Truyện trinh thám khoa học viễn tưởng về robot và xã hội đô thị tương lai.', N'Bản dữ liệu trình diễn', N'robot, trinh thám, tương lai', 1953, N'LibraryHub rich demo seed', 259, N'Tiếng Anh', NULL, 139000.00, 9, 6, 14, N'https://covers.openlibrary.org/b/id/13790511-L.jpg?default=false', 0, 205, 2, 12, 10, 2, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
    (15, N'9789685960434', N'The Naked Sun', N'Vụ án robot trong một xã hội xa lạ, nơi con người tránh tiếp xúc trực tiếp.', N'Bản dữ liệu trình diễn', N'robot, trinh thám, xã hội tương lai', 1954, N'LibraryHub rich demo seed', 226, N'Tiếng Anh', '2026-10-11 12:20:11.5067114 +07:00', 135000.00, 7, 4, 14, N'https://covers.openlibrary.org/b/id/6542967-L.jpg?default=false', 0, 156, 2, 12, 16, 2, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
    (16, N'9798545118748', N'Atomic Habits', N'Cách xây dựng thói quen tốt bằng những thay đổi nhỏ, rõ ràng và có thể duy trì.', N'Bản dữ liệu trình diễn', N'thói quen, cải thiện, năng suất', 2016, N'LibraryHub rich demo seed', 322, N'Tiếng Anh', '2026-11-16 13:53:55.0827924 +00:00', 175000.00, 18, 12, 14, N'https://covers.openlibrary.org/b/id/12539702-L.jpg?default=false', 0, 526, 4, 13, 9, 4, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 13:53:55.0828547 +00:00', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
    (17, N'9780143774266', N'Diary of a Wimpy Kid', N'Nhật ký hài hước về trường học, gia đình và những rắc rối thường ngày.', N'Bản dữ liệu trình diễn', N'hài hước, trường học, thiếu nhi', 2007, N'LibraryHub rich demo seed', 224, N'Tiếng Anh', NULL, 119000.00, 15, 12, 9, N'https://covers.openlibrary.org/b/id/14376136-L.jpg?default=false', 0, 289, 6, 14, 21, 6, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
    (18, N'9780241753859', N'The Summer I Turned Pretty Trilogy', N'Bộ truyện trưởng thành về mùa hè, gia đình, tình bạn và những lựa chọn đầu đời.', N'Bản dữ liệu trình diễn', N'tuổi trẻ, gia đình, tình bạn', 2009, N'LibraryHub rich demo seed', 851, N'Tiếng Anh', NULL, 239000.00, 7, 5, 13, N'https://covers.openlibrary.org/b/id/7719210-L.jpg?default=false', 0, 184, 6, 15, 23, 6, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
    (19, N'9780345816023', N'12 Rules for Life', N'Mười hai nguyên tắc suy ngẫm về trách nhiệm cá nhân, trật tự và ý nghĩa cuộc sống.', N'Bản dữ liệu trình diễn', N'nguyên tắc sống, trách nhiệm, tâm lý', 2015, N'LibraryHub rich demo seed', 480, N'Tiếng Anh', NULL, 189000.00, 8, 5, 16, N'https://covers.openlibrary.org/b/id/8131760-L.jpg?default=false', 0, 287, 4, 16, 20, 4, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
    (20, N'9789937074391', N'The Power of Your Subconscious Mind', N'Khám phá ảnh hưởng của tiềm thức lên thói quen, niềm tin và lựa chọn hằng ngày.', N'Bản dữ liệu trình diễn', N'tiềm thức, tư duy, thói quen', 1963, N'LibraryHub rich demo seed', 222, N'Tiếng Anh', NULL, 125000.00, 10, 8, 14, N'https://covers.openlibrary.org/b/id/6553019-L.jpg?default=false', 0, 302, 4, 17, 29, 4, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
    (21, N'9798893310153', N'Don''t Believe Everything You Think', N'Gợi ý cách quan sát suy nghĩ và giảm những vòng lặp tinh thần không cần thiết.', N'Bản dữ liệu trình diễn', N'suy nghĩ, nhận thức, bình an', 2022, N'LibraryHub rich demo seed', 192, N'Tiếng Anh', NULL, 139000.00, 12, 9, 14, N'https://covers.openlibrary.org/b/id/15160816-L.jpg?default=false', 0, 212, 5, 18, 6, 5, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
    (22, N'9781119485544', N'Hacking for Dummies', N'Tổng quan thực hành về đánh giá lỗ hổng, kiểm thử và phòng vệ hệ thống.', N'Bản dữ liệu trình diễn', N'bảo mật, kiểm thử, an toàn thông tin', 2004, N'LibraryHub rich demo seed', 396, N'Tiếng Anh', NULL, 225000.00, 5, 0, 18, N'https://covers.openlibrary.org/b/id/299671-L.jpg?default=false', 4, 164, 3, 19, 30, 3, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
    (23, N'9781800249158', N'Death''s End', N'Phần kết của bộ Ba Vật Thể, mở rộng xung đột từ Trái Đất đến quy mô vũ trụ.', N'Bản dữ liệu trình diễn', N'vũ trụ, vật lý, văn minh ngoài Trái Đất', 2010, N'LibraryHub rich demo seed', 724, N'Tiếng Anh', '2026-10-23 12:20:11.5067114 +07:00', 219000.00, 8, 5, 16, N'https://covers.openlibrary.org/b/id/7893958-L.jpg?default=false', 0, 264, 2, 20, 12, 2, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
    (24, N'9788804686521', N'The Dark Forest', N'Phần hai của bộ Ba Vật Thể với chiến lược sinh tồn giữa các nền văn minh.', N'Bản dữ liệu trình diễn', N'vũ trụ, chiến lược, văn minh', 2008, N'LibraryHub rich demo seed', 574, N'Tiếng Anh', NULL, 205000.00, 9, 6, 16, N'https://covers.openlibrary.org/b/id/10526598-L.jpg?default=false', 0, 298, 2, 20, 1, 2, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
    (25, N'9780596158064', N'Learning Python', N'Tài liệu thực hành Python từ cú pháp nền tảng đến lập trình hướng đối tượng và module.', N'Bản dữ liệu trình diễn', N'Python, lập trình, phần mềm', 1999, N'LibraryHub rich demo seed', 591, N'Tiếng Anh', NULL, 245000.00, 12, 8, 14, N'https://covers.openlibrary.org/b/id/1312568-L.jpg?default=false', 0, 245, 3, 21, 18, 3, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
    (26, N'9798667360360', N'Frankenstein', N'Tác phẩm kinh điển đặt câu hỏi về khoa học, trách nhiệm và bản chất con người.', N'Bản dữ liệu trình diễn', N'kinh điển, khoa học, trách nhiệm', 1818, N'LibraryHub rich demo seed', 240, N'Tiếng Anh', NULL, 115000.00, 8, 0, 16, N'https://covers.openlibrary.org/b/id/12356249-L.jpg?default=false', 4, 355, 8, 22, 7, 8, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
    (27, N'9780321623539', N'Biology', N'Giáo trình tổng quan về tế bào, di truyền, tiến hóa, sinh thái và đa dạng sinh học.', N'Bản dữ liệu trình diễn', N'sinh học, di truyền, tiến hóa', 1987, N'LibraryHub rich demo seed', 1267, N'Tiếng Anh', NULL, 320000.00, 5, 2, 16, N'https://covers.openlibrary.org/b/id/581911-L.jpg?default=false', 0, 221, 1, 23, 2, 1, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
    (28, N'9786053243113', N'The Picture of Dorian Gray', N'Tác phẩm kinh điển về cái đẹp, đạo đức và cái giá của ham muốn vĩnh cửu.', N'Bản dữ liệu trình diễn', N'kinh điển, đạo đức, nghệ thuật', 1890, N'LibraryHub rich demo seed', 246, N'Tiếng Anh', NULL, 129000.00, 10, 7, 16, N'https://covers.openlibrary.org/b/id/14314858-L.jpg?default=false', 0, 337, 8, 24, 14, 8, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
    (29, N'9781449388393', N'Hackers', N'Lịch sử văn hóa hacker và những con người tiên phong của ngành máy tính cá nhân.', N'Bản dữ liệu trình diễn', N'hacker, lịch sử máy tính, công nghệ', 1984, N'LibraryHub rich demo seed', 456, N'Tiếng Anh', '2026-10-09 12:20:11.5067114 +07:00', 210000.00, 6, 3, 16, N'https://covers.openlibrary.org/b/id/106396-L.jpg?default=false', 0, 199, 3, 25, 3, 3, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
    (30, N'9780534359454', N'Discrete Mathematics with Applications', N'Giáo trình nhập môn toán rời rạc với logic, tổ hợp, đồ thị và nhiều bài tập ứng dụng.', N'Bản dữ liệu trình diễn', N'toán rời rạc, logic, đồ thị', 1990, N'LibraryHub rich demo seed', 889, N'Tiếng Anh', '2026-10-21 12:20:11.5067114 +07:00', 285000.00, 8, 5, 16, N'https://covers.openlibrary.org/b/id/9295752-L.jpg?default=false', 0, 186, 7, 26, 28, 7, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
    (31, N'9780062111234', N'Shatter Me', N'Câu chuyện giả tưởng tuổi trẻ về quyền năng, tự do và khả năng lựa chọn.', N'Bản dữ liệu trình diễn', N'giả tưởng, tuổi trẻ, tự do', 2011, N'LibraryHub rich demo seed', 357, N'Tiếng Anh', NULL, 159000.00, 9, 5, 13, N'https://covers.openlibrary.org/b/id/6974992-L.jpg?default=false', 0, 244, 6, 27, 11, 6, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
    (32, N'9788576571100', N'The Difference Engine', N'Tiểu thuyết giả tưởng về một thế giới nơi máy tính cơ khí làm thay đổi lịch sử.', N'Bản dữ liệu trình diễn', N'công nghệ, lịch sử giả tưởng, steampunk', 1990, N'LibraryHub rich demo seed', 395, N'Tiếng Anh', NULL, 168000.00, 7, 4, 16, N'https://covers.openlibrary.org/b/id/4034359-L.jpg?default=false', 0, 137, 2, 28, 24, 2, '2026-11-16 12:20:11.5067114 +07:00', '2026-11-16 12:20:11.5067114 +07:00', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
SET IDENTITY_INSERT [dbo].[Books] OFF;
GO

-- Table: [Readers] (3 rows)
SET IDENTITY_INSERT [dbo].[Readers] ON;
INSERT INTO [dbo].[Readers] ([Id], [UserAccountId], [ReaderCode], [FullName], [DateOfBirth], [Phone], [Email], [Address], [IdentityNumber], [RegisteredOn], [ExpiresOn], [LastLoginAt], [ConsecutiveLoginDays], [LastStreakOn], [AvatarPath], [CoverImagePath], [Bio], [ActiveBadge], [FullPageBackgroundPath], [CommentLockMode], [CommentBlockedUntil], [MaxBorrowBooks], [Type], [Gender], [EmailConfirmed], [ForcePasswordChange], [IsNameSet], [IsVerified], [CreatedAt], [UpdatedAt], [IsDeleted], [DeletedAt]) VALUES
    (1, 'f08b7129-81f6-459e-a762-e9e6c18de847', N'DG-20261116-6A523072C', N'anh Nam', NULL, N'0966774364', N'gamemod5512k@gmail.com', N'nam định', NULL, '2026-11-16', '2027-11-16', NULL, 1, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, 10, 0, 0, 1, 0, 0, 0, '2026-11-16 05:25:55.9002790 +00:00', '2026-11-16 05:25:55.9002790 +00:00', 0, NULL),
    (2, '46eb04e9-b59f-4bdc-b41f-52938b9c4801', N'DG001', N'Lê Phú An', NULL, N'0911223344', N'reader@library.com', N'Hà Nội', NULL, '2026-11-16', '2027-11-16', '2026-11-16 14:37:17.2203848 +00:00', 1, '2026-11-16', NULL, NULL, NULL, NULL, NULL, 0, NULL, 10, 0, 0, 1, 0, 0, 0, '2026-11-16 06:07:04.6038829 +00:00', '2026-11-16 14:37:17.2204047 +00:00', 0, NULL),
    (3, 'adc51e48-f0c5-4403-8400-9607f059f3e3', N'DG002', N'Nguyễn Văn An', NULL, N'0922334455', N'docgia@gmail.com', N'Hà Nội', NULL, '2026-11-16', '2027-11-16', '2026-11-16 06:10:38.6529433 +00:00', 1, '2026-11-16', NULL, NULL, NULL, NULL, NULL, 0, NULL, 10, 0, 0, 1, 0, 0, 0, '2026-11-16 06:07:04.7287606 +00:00', '2026-11-16 06:10:38.6529440 +00:00', 0, NULL);
SET IDENTITY_INSERT [dbo].[Readers] OFF;
GO

-- Table: [BorrowSlips] (1 rows)
SET IDENTITY_INSERT [dbo].[BorrowSlips] ON;
INSERT INTO [dbo].[BorrowSlips] ([Id], [Code], [ReaderId], [TotalBooks], [RequestedAt], [BorrowedAt], [DueAt], [ReturnedAt], [PickupExpiresAt], [Note], [CreatedByAccountId], [Status], [IdempotencyKey], [CreatedAt], [UpdatedAt]) VALUES
    (1, N'BR-20261116-146B4B998', 2, 1, '2026-11-16 13:51:10.2012899 +00:00', '2026-11-16 13:53:55.0816652 +00:00', '2026-12-03 13:53:55.0817489 +00:00', '2026-11-16 13:54:16.9440310 +00:00', NULL, N'Kiem tra e2e', '46eb04e9-b59f-4bdc-b41f-52938b9c4801', 9, N'68687611-3498-4974-8f0d-411c66642580', '2026-11-16 13:51:10.4073693 +00:00', '2026-11-16 13:54:16.9440938 +00:00');
SET IDENTITY_INSERT [dbo].[BorrowSlips] OFF;
GO

-- Table: [BorrowDetails] (1 rows)
SET IDENTITY_INSERT [dbo].[BorrowDetails] ON;
INSERT INTO [dbo].[BorrowDetails] ([Id], [BorrowSlipId], [BookId], [Quantity], [ReturnedQuantity], [BorrowPrice], [ConditionBefore], [Note], [ExtensionCount], [DueAt], [RequestedExtensionDays], [ExtensionReason], [CreatedAt], [UpdatedAt]) VALUES
    (1, 1, 16, 1, 1, 175000.00, NULL, NULL, 1, '2026-12-03 13:53:55.0817489 +00:00', NULL, NULL, '2026-11-16 13:51:10.4073693 +00:00', '2026-11-16 13:54:16.9440938 +00:00');
SET IDENTITY_INSERT [dbo].[BorrowDetails] OFF;
GO

-- Table: [BorrowReturns] (1 rows)
SET IDENTITY_INSERT [dbo].[BorrowReturns] ON;
INSERT INTO [dbo].[BorrowReturns] ([Id], [BorrowDetailId], [Quantity], [Condition], [DamagePercent], [ConditionNote], [ProcessedByAccountId], [IdempotencyKey], [CreatedAt], [UpdatedAt], [FineAmount], [FineId], [ResultSlipId], [ResultStatus]) VALUES
    (1, 1, 1, 1, 25.00, N'Rach bia E2E', '4b7932d9-a8b8-4d47-8e25-d1de88b05711', N'410aff80-74b5-41b9-80b9-c0a991c8d99a', '2026-11-16 13:54:16.9440938 +00:00', '2026-11-16 13:54:17.0272386 +00:00', 43750.00, 1, 1, 9);
SET IDENTITY_INSERT [dbo].[BorrowReturns] OFF;
GO

-- Table: [Fines] (1 rows)
SET IDENTITY_INSERT [dbo].[Fines] ON;
INSERT INTO [dbo].[Fines] ([Id], [Code], [BorrowSlipId], [ReaderId], [Type], [Amount], [PaidAmount], [Reason], [Status], [TransactionCode], [PaidAt], [CreatedAt], [UpdatedAt]) VALUES
    (1, N'FN-20261116-664AE7F99', 1, 2, 2, 43750.00, 43750.00, N'hư hỏng 43,750đ', 3, N'QUAY-20261116135420', '2026-11-16 13:54:20.3122502 +00:00', '2026-11-16 13:54:16.9440938 +00:00', '2026-11-16 13:54:20.3122973 +00:00');
SET IDENTITY_INSERT [dbo].[Fines] OFF;
GO

-- Table: [FineDetails] (1 rows)
SET IDENTITY_INSERT [dbo].[FineDetails] ON;
INSERT INTO [dbo].[FineDetails] ([Id], [FineId], [BookId], [BorrowDetailId], [Type], [Amount], [Reason], [DamagePercent], [BookPrice], [LateDays], [CreatedAt], [UpdatedAt]) VALUES
    (1, 1, 16, 1, 2, 43750.00, N'hư hỏng 43,750đ', 25.00, 175000.00, 0, '2026-11-16 13:54:16.9440938 +00:00', '2026-11-16 13:54:16.9440938 +00:00');
SET IDENTITY_INSERT [dbo].[FineDetails] OFF;
GO

-- Table: [Notifications] (7 rows)
SET IDENTITY_INSERT [dbo].[Notifications] ON;
INSERT INTO [dbo].[Notifications] ([Id], [RecipientAccountId], [Type], [Title], [Message], [Link], [IsRead], [ReadAt], [CreatedByAccountId], [BroadcastGroupId], [CreatedAt], [UpdatedAt], [CorrelationKey]) VALUES
    (1, '46eb04e9-b59f-4bdc-b41f-52938b9c4801', 5, N'Đã gửi yêu cầu mượn', N'Phiếu BR-20261116-146B4B998 đang chờ duyệt.', N'/me/borrows/1', 0, NULL, NULL, NULL, '2026-11-16 13:51:10.5724870 +00:00', '2026-11-16 13:51:10.5724870 +00:00', N'1'),
    (2, '46eb04e9-b59f-4bdc-b41f-52938b9c4801', 6, N'Phiếu đã được duyệt', N'Đến nhận trước 19/11/2026 13:53.', N'/me/borrows/1', 0, NULL, NULL, NULL, '2026-11-16 13:53:52.8907062 +00:00', '2026-11-16 13:53:52.8907062 +00:00', N'1'),
    (3, '46eb04e9-b59f-4bdc-b41f-52938b9c4801', 8, N'Đã nhận sách', N'Hạn trả 30/11/2026.', N'/me/borrows/1', 0, NULL, NULL, NULL, '2026-11-16 13:53:55.1000438 +00:00', '2026-11-16 13:53:55.1000438 +00:00', N'1'),
    (4, '46eb04e9-b59f-4bdc-b41f-52938b9c4801', 16, N'Đã gửi yêu cầu gia hạn', N'Cần thêm thời gian đọc', N'/me/borrows/1', 0, NULL, NULL, NULL, '2026-11-16 13:54:05.7750229 +00:00', '2026-11-16 13:54:05.7750229 +00:00', N'1'),
    (5, '46eb04e9-b59f-4bdc-b41f-52938b9c4801', 17, N'Gia hạn được duyệt', N'Hạn mới 03/12/2026.', N'/me/borrows/1', 0, NULL, NULL, NULL, '2026-11-16 13:54:14.2026488 +00:00', '2026-11-16 13:54:14.2026488 +00:00', N'1'),
    (6, '46eb04e9-b59f-4bdc-b41f-52938b9c4801', 14, N'Đã hoàn tất trả sách', N'Phát sinh 43,750đ tiền phạt.', N'/me/borrows/1', 0, NULL, NULL, NULL, '2026-11-16 13:54:17.0342389 +00:00', '2026-11-16 13:54:17.0342389 +00:00', N'1'),
    (7, '46eb04e9-b59f-4bdc-b41f-52938b9c4801', 27, N'Thủ thư đã xác nhận thu phạt', N'Khoản phạt FN-20261116-664AE7F99 (43,750đ) đã được thanh toán tại quầy.', N'/me/fines/1', 0, NULL, NULL, NULL, '2026-11-16 13:54:20.3391887 +00:00', '2026-11-16 13:54:20.3391887 +00:00', N'fine-paid:1');
SET IDENTITY_INSERT [dbo].[Notifications] OFF;
GO
