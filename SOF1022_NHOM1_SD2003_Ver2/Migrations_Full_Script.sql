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

