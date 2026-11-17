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
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
    CREATE TABLE [BookCommentLikes] (
        [Id] int NOT NULL IDENTITY,
        [BookCommentId] int NOT NULL,
        [ReaderId] int NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [UpdatedAt] datetimeoffset NOT NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_BookCommentLikes] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
    CREATE TABLE [BookFavorites] (
        [Id] int NOT NULL IDENTITY,
        [BookId] int NOT NULL,
        [ReaderId] int NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [UpdatedAt] datetimeoffset NOT NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_BookFavorites] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
    CREATE TABLE [StaffAccounts] (
        [Id] int NOT NULL IDENTITY,
        [UserAccountId] uniqueidentifier NOT NULL,
        [EmployeeId] int NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [UpdatedAt] datetimeoffset NOT NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_StaffAccounts] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] uniqueidentifier NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] uniqueidentifier NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] uniqueidentifier NOT NULL,
        [RoleId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] uniqueidentifier NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_BookCommentLikes_BookCommentId_ReaderId] ON [BookCommentLikes] ([BookCommentId], [ReaderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_BookFavorites_BookId_ReaderId] ON [BookFavorites] ([BookId], [ReaderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_BookRatings_BookId_ReaderId] ON [BookRatings] ([BookId], [ReaderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Books_AuthorId] ON [Books] ([AuthorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Books_CategoryId] ON [Books] ([CategoryId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Books_Isbn] ON [Books] ([Isbn]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Books_PublisherId] ON [Books] ([PublisherId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Books_ShelfId] ON [Books] ([ShelfId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_BorrowDetails_BookId] ON [BorrowDetails] ([BookId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_BorrowDetails_BorrowSlipId_BookId] ON [BorrowDetails] ([BorrowSlipId], [BookId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_BorrowReturns_IdempotencyKey_BorrowDetailId] ON [BorrowReturns] ([IdempotencyKey], [BorrowDetailId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_BorrowSlips_Code] ON [BorrowSlips] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_BorrowSlips_IdempotencyKey] ON [BorrowSlips] ([IdempotencyKey]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_BorrowSlips_ReaderId] ON [BorrowSlips] ([ReaderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Employees_Email] ON [Employees] ([Email]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Employees_EmployeeCode] ON [Employees] ([EmployeeCode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Employees_NationalId] ON [Employees] ([NationalId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Employees_PositionId] ON [Employees] ([PositionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_FineDetails_FineId] ON [FineDetails] ([FineId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Fines_Code] ON [Fines] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Notifications_RecipientAccountId_IsRead_CreatedAt] ON [Notifications] ([RecipientAccountId], [IsRead], [CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Permissions_Code] ON [Permissions] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Positions_Code] ON [Positions] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ReaderFeaturedBooks_ReaderId_BookId] ON [ReaderFeaturedBooks] ([ReaderId], [BookId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ReaderFeaturedBooks_ReaderId_DisplayOrder] ON [ReaderFeaturedBooks] ([ReaderId], [DisplayOrder]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Readers_Email] ON [Readers] ([Email]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Readers_ReaderCode] ON [Readers] ([ReaderCode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Readers_UserAccountId] ON [Readers] ([UserAccountId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ReadingHistories_BookId_ReaderId] ON [ReadingHistories] ([BookId], [ReaderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Reservations_ReaderId_BookId_Status] ON [Reservations] ([ReaderId], [BookId], [Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RolePermissions_RoleId_PermissionId] ON [RolePermissions] ([RoleId], [PermissionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SecurityRoles_Code] ON [SecurityRoles] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_StaffAccounts_EmployeeId] ON [StaffAccounts] ([EmployeeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_StaffAccounts_UserAccountId] ON [StaffAccounts] ([UserAccountId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SupportMessages_SupportTicketId] ON [SupportMessages] ([SupportTicketId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SupportTickets_Code] ON [SupportTickets] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_UserPermissions_EmployeeId_PermissionId] ON [UserPermissions] ([EmployeeId], [PermissionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730131754_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260730131754_InitialCreate', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    CREATE INDEX [IX_UserPermissions_PermissionId] ON [UserPermissions] ([PermissionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    CREATE INDEX [IX_SupportTickets_AssignedStaffAccountId] ON [SupportTickets] ([AssignedStaffAccountId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    CREATE INDEX [IX_SupportTickets_ReaderId] ON [SupportTickets] ([ReaderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    CREATE INDEX [IX_SupportMessages_SenderAccountId] ON [SupportMessages] ([SenderAccountId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    CREATE INDEX [IX_RolePermissions_PermissionId] ON [RolePermissions] ([PermissionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    CREATE INDEX [IX_Reservations_BookId] ON [Reservations] ([BookId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    CREATE INDEX [IX_ReadingHistories_ReaderId] ON [ReadingHistories] ([ReaderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    CREATE INDEX [IX_ReaderFeaturedBooks_BookId] ON [ReaderFeaturedBooks] ([BookId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    CREATE INDEX [IX_ProfileTagPins_AuthorId] ON [ProfileTagPins] ([AuthorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    CREATE INDEX [IX_ProfileTagPins_ProfileOwnerId] ON [ProfileTagPins] ([ProfileOwnerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    CREATE INDEX [IX_ProfileComments_AuthorId] ON [ProfileComments] ([AuthorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    CREATE INDEX [IX_ProfileComments_ProfileOwnerId] ON [ProfileComments] ([ProfileOwnerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    CREATE INDEX [IX_Notifications_CreatedByAccountId] ON [Notifications] ([CreatedByAccountId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    CREATE INDEX [IX_Fines_BorrowSlipId] ON [Fines] ([BorrowSlipId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    CREATE INDEX [IX_Fines_ReaderId] ON [Fines] ([ReaderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    CREATE INDEX [IX_FineDetails_BookId] ON [FineDetails] ([BookId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    CREATE INDEX [IX_FineDetails_BorrowDetailId] ON [FineDetails] ([BorrowDetailId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    CREATE INDEX [IX_Employees_RoleId] ON [Employees] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    CREATE INDEX [IX_Employees_StatusOptionId] ON [Employees] ([StatusOptionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    CREATE INDEX [IX_CommentReports_BookCommentId] ON [CommentReports] ([BookCommentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    CREATE INDEX [IX_CommentReports_ProfileCommentId] ON [CommentReports] ([ProfileCommentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    CREATE INDEX [IX_CommentReports_ReportedUserId] ON [CommentReports] ([ReportedUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    CREATE INDEX [IX_CommentReports_ReporterId] ON [CommentReports] ([ReporterId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    CREATE INDEX [IX_BorrowReturns_BorrowDetailId] ON [BorrowReturns] ([BorrowDetailId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    CREATE INDEX [IX_BookViewHistories_BookId] ON [BookViewHistories] ([BookId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    CREATE INDEX [IX_BookViewHistories_ReaderId] ON [BookViewHistories] ([ReaderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    CREATE INDEX [IX_BookRatings_ReaderId] ON [BookRatings] ([ReaderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    CREATE INDEX [IX_BookFavorites_ReaderId] ON [BookFavorites] ([ReaderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    CREATE INDEX [IX_BookComments_BookId] ON [BookComments] ([BookId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    CREATE INDEX [IX_BookComments_ParentCommentId] ON [BookComments] ([ParentCommentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    CREATE INDEX [IX_BookComments_ReaderId] ON [BookComments] ([ReaderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    CREATE INDEX [IX_BookCommentLikes_ReaderId] ON [BookCommentLikes] ([ReaderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    ALTER TABLE [BookCommentLikes] ADD CONSTRAINT [FK_BookCommentLikes_BookComments_BookCommentId] FOREIGN KEY ([BookCommentId]) REFERENCES [BookComments] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    ALTER TABLE [BookCommentLikes] ADD CONSTRAINT [FK_BookCommentLikes_Readers_ReaderId] FOREIGN KEY ([ReaderId]) REFERENCES [Readers] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    ALTER TABLE [BookComments] ADD CONSTRAINT [FK_BookComments_BookComments_ParentCommentId] FOREIGN KEY ([ParentCommentId]) REFERENCES [BookComments] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    ALTER TABLE [BookComments] ADD CONSTRAINT [FK_BookComments_Books_BookId] FOREIGN KEY ([BookId]) REFERENCES [Books] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    ALTER TABLE [BookComments] ADD CONSTRAINT [FK_BookComments_Readers_ReaderId] FOREIGN KEY ([ReaderId]) REFERENCES [Readers] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    ALTER TABLE [BookFavorites] ADD CONSTRAINT [FK_BookFavorites_Books_BookId] FOREIGN KEY ([BookId]) REFERENCES [Books] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    ALTER TABLE [BookFavorites] ADD CONSTRAINT [FK_BookFavorites_Readers_ReaderId] FOREIGN KEY ([ReaderId]) REFERENCES [Readers] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    ALTER TABLE [BookRatings] ADD CONSTRAINT [FK_BookRatings_Books_BookId] FOREIGN KEY ([BookId]) REFERENCES [Books] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    ALTER TABLE [BookRatings] ADD CONSTRAINT [FK_BookRatings_Readers_ReaderId] FOREIGN KEY ([ReaderId]) REFERENCES [Readers] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    ALTER TABLE [BookViewHistories] ADD CONSTRAINT [FK_BookViewHistories_Books_BookId] FOREIGN KEY ([BookId]) REFERENCES [Books] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    ALTER TABLE [BookViewHistories] ADD CONSTRAINT [FK_BookViewHistories_Readers_ReaderId] FOREIGN KEY ([ReaderId]) REFERENCES [Readers] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    ALTER TABLE [BorrowReturns] ADD CONSTRAINT [FK_BorrowReturns_BorrowDetails_BorrowDetailId] FOREIGN KEY ([BorrowDetailId]) REFERENCES [BorrowDetails] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    ALTER TABLE [CommentReports] ADD CONSTRAINT [FK_CommentReports_BookComments_BookCommentId] FOREIGN KEY ([BookCommentId]) REFERENCES [BookComments] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    ALTER TABLE [CommentReports] ADD CONSTRAINT [FK_CommentReports_ProfileComments_ProfileCommentId] FOREIGN KEY ([ProfileCommentId]) REFERENCES [ProfileComments] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    ALTER TABLE [CommentReports] ADD CONSTRAINT [FK_CommentReports_Readers_ReportedUserId] FOREIGN KEY ([ReportedUserId]) REFERENCES [Readers] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    ALTER TABLE [CommentReports] ADD CONSTRAINT [FK_CommentReports_Readers_ReporterId] FOREIGN KEY ([ReporterId]) REFERENCES [Readers] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    ALTER TABLE [Employees] ADD CONSTRAINT [FK_Employees_EmployeeStatusOptions_StatusOptionId] FOREIGN KEY ([StatusOptionId]) REFERENCES [EmployeeStatusOptions] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    ALTER TABLE [Employees] ADD CONSTRAINT [FK_Employees_SecurityRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [SecurityRoles] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    ALTER TABLE [FineDetails] ADD CONSTRAINT [FK_FineDetails_Books_BookId] FOREIGN KEY ([BookId]) REFERENCES [Books] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    ALTER TABLE [FineDetails] ADD CONSTRAINT [FK_FineDetails_BorrowDetails_BorrowDetailId] FOREIGN KEY ([BorrowDetailId]) REFERENCES [BorrowDetails] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    ALTER TABLE [Fines] ADD CONSTRAINT [FK_Fines_BorrowSlips_BorrowSlipId] FOREIGN KEY ([BorrowSlipId]) REFERENCES [BorrowSlips] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    ALTER TABLE [Fines] ADD CONSTRAINT [FK_Fines_Readers_ReaderId] FOREIGN KEY ([ReaderId]) REFERENCES [Readers] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    ALTER TABLE [Notifications] ADD CONSTRAINT [FK_Notifications_AspNetUsers_CreatedByAccountId] FOREIGN KEY ([CreatedByAccountId]) REFERENCES [AspNetUsers] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    ALTER TABLE [Notifications] ADD CONSTRAINT [FK_Notifications_AspNetUsers_RecipientAccountId] FOREIGN KEY ([RecipientAccountId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    ALTER TABLE [ProfileComments] ADD CONSTRAINT [FK_ProfileComments_Readers_AuthorId] FOREIGN KEY ([AuthorId]) REFERENCES [Readers] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    ALTER TABLE [ProfileComments] ADD CONSTRAINT [FK_ProfileComments_Readers_ProfileOwnerId] FOREIGN KEY ([ProfileOwnerId]) REFERENCES [Readers] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    ALTER TABLE [ProfileTagPins] ADD CONSTRAINT [FK_ProfileTagPins_Readers_AuthorId] FOREIGN KEY ([AuthorId]) REFERENCES [Readers] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    ALTER TABLE [ProfileTagPins] ADD CONSTRAINT [FK_ProfileTagPins_Readers_ProfileOwnerId] FOREIGN KEY ([ProfileOwnerId]) REFERENCES [Readers] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    ALTER TABLE [ReaderFeaturedBooks] ADD CONSTRAINT [FK_ReaderFeaturedBooks_Books_BookId] FOREIGN KEY ([BookId]) REFERENCES [Books] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    ALTER TABLE [ReaderFeaturedBooks] ADD CONSTRAINT [FK_ReaderFeaturedBooks_Readers_ReaderId] FOREIGN KEY ([ReaderId]) REFERENCES [Readers] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    ALTER TABLE [ReadingHistories] ADD CONSTRAINT [FK_ReadingHistories_Books_BookId] FOREIGN KEY ([BookId]) REFERENCES [Books] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    ALTER TABLE [ReadingHistories] ADD CONSTRAINT [FK_ReadingHistories_Readers_ReaderId] FOREIGN KEY ([ReaderId]) REFERENCES [Readers] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    ALTER TABLE [Reservations] ADD CONSTRAINT [FK_Reservations_Books_BookId] FOREIGN KEY ([BookId]) REFERENCES [Books] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    ALTER TABLE [Reservations] ADD CONSTRAINT [FK_Reservations_Readers_ReaderId] FOREIGN KEY ([ReaderId]) REFERENCES [Readers] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    ALTER TABLE [RolePermissions] ADD CONSTRAINT [FK_RolePermissions_Permissions_PermissionId] FOREIGN KEY ([PermissionId]) REFERENCES [Permissions] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    ALTER TABLE [RolePermissions] ADD CONSTRAINT [FK_RolePermissions_SecurityRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [SecurityRoles] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    ALTER TABLE [StaffAccounts] ADD CONSTRAINT [FK_StaffAccounts_AspNetUsers_UserAccountId] FOREIGN KEY ([UserAccountId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    ALTER TABLE [StaffAccounts] ADD CONSTRAINT [FK_StaffAccounts_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    ALTER TABLE [SupportMessages] ADD CONSTRAINT [FK_SupportMessages_AspNetUsers_SenderAccountId] FOREIGN KEY ([SenderAccountId]) REFERENCES [AspNetUsers] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    ALTER TABLE [SupportTickets] ADD CONSTRAINT [FK_SupportTickets_AspNetUsers_AssignedStaffAccountId] FOREIGN KEY ([AssignedStaffAccountId]) REFERENCES [AspNetUsers] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    ALTER TABLE [SupportTickets] ADD CONSTRAINT [FK_SupportTickets_Readers_ReaderId] FOREIGN KEY ([ReaderId]) REFERENCES [Readers] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    ALTER TABLE [UserPermissions] ADD CONSTRAINT [FK_UserPermissions_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    ALTER TABLE [UserPermissions] ADD CONSTRAINT [FK_UserPermissions_Permissions_PermissionId] FOREIGN KEY ([PermissionId]) REFERENCES [Permissions] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730141111_AddDomainRelationships'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260730141111_AddDomainRelationships', N'10.0.9');
END;

COMMIT;
GO

