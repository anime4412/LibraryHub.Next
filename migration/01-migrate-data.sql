/*
  Chạy bằng SSMS ở SQLCMD Mode hoặc sqlcmd.
  Luôn chạy trên bản sao trước. DB đích phải đã được tạo bằng EF migration và rỗng.
*/
:setvar SourceDb "LibraryDb_Old"
:setvar TargetDb "LibraryHubNext"

USE [$(TargetDb)];
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF EXISTS (SELECT 1 FROM dbo.Books)
    THROW 51000, N'DB đích không rỗng. Dừng ETL để tránh nhân đôi dữ liệu.', 1;

BEGIN TRY
BEGIN TRANSACTION;

CREATE TABLE #ReaderAccountMap (ReaderId int PRIMARY KEY, AccountId uniqueidentifier NOT NULL);
INSERT #ReaderAccountMap SELECT Id, NEWID() FROM [$(SourceDb)].dbo.Readers;

-- Identity platform roles
IF NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles WHERE NormalizedName = 'READER')
    INSERT dbo.AspNetRoles(Id,Name,NormalizedName,ConcurrencyStamp) VALUES(NEWID(),'Reader','READER',CONVERT(nvarchar(36),NEWID()));
IF NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles WHERE NormalizedName = 'STAFF')
    INSERT dbo.AspNetRoles(Id,Name,NormalizedName,ConcurrencyStamp) VALUES(NEWID(),'Staff','STAFF',CONVERT(nvarchar(36),NEWID()));
IF NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles WHERE NormalizedName = 'ADMINISTRATOR')
    INSERT dbo.AspNetRoles(Id,Name,NormalizedName,ConcurrencyStamp) VALUES(NEWID(),'Administrator','ADMINISTRATOR',CONVERT(nvarchar(36),NEWID()));

INSERT dbo.AspNetUsers
(
    Id,UserName,NormalizedUserName,Email,NormalizedEmail,EmailConfirmed,PasswordHash,
    SecurityStamp,ConcurrencyStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,
    LockoutEnd,LockoutEnabled,AccessFailedCount,AccountKind,DisplayName,IsActive,
    MustChangePassword,LastLoginAt
)
SELECT
    m.AccountId,r.Email,UPPER(r.Email),r.Email,UPPER(r.Email),r.EmailConfirmed,r.Password,
    CONVERT(nvarchar(36),NEWID()),CONVERT(nvarchar(36),NEWID()),r.Phone,
    CASE WHEN r.IsVerified=1 THEN 1 ELSE 0 END,0,NULL,1,0,
    CASE WHEN r.Role='Admin' THEN 2 WHEN r.Role='Librarian' THEN 1 ELSE 0 END,
    COALESCE(NULLIF(r.FullName,''),r.Email),CASE WHEN r.IsActive=1 AND r.IsDeleted=0 THEN 1 ELSE 0 END,
    1,r.LastLoginDate
FROM [$(SourceDb)].dbo.Readers r JOIN #ReaderAccountMap m ON m.ReaderId=r.Id;

INSERT dbo.AspNetUserRoles(UserId,RoleId)
SELECT m.AccountId, ar.Id
FROM [$(SourceDb)].dbo.Readers r
JOIN #ReaderAccountMap m ON m.ReaderId=r.Id
JOIN dbo.AspNetRoles ar ON ar.NormalizedName =
    CASE WHEN r.Role='Admin' THEN 'ADMINISTRATOR' WHEN r.Role='Librarian' THEN 'STAFF' ELSE 'READER' END;

-- Lookup tables
SET IDENTITY_INSERT dbo.Categories ON;
INSERT dbo.Categories(Id,Name,Description,IsDeleted,DeletedAt,CreatedAt,UpdatedAt)
SELECT Id,Name,Description,0,NULL,CreatedDate,COALESCE(UpdatedDate,CreatedDate)
FROM [$(SourceDb)].dbo.Categories;
SET IDENTITY_INSERT dbo.Categories OFF;

SET IDENTITY_INSERT dbo.Authors ON;
INSERT dbo.Authors(Id,Name,DateOfBirth,Nationality,Biography,IsDeleted,DeletedAt,CreatedAt,UpdatedAt)
SELECT Id,Name,CAST(DateOfBirth AS date),Nationality,Biography,IsDeleted,NULL,CreatedDate,COALESCE(UpdatedDate,CreatedDate)
FROM [$(SourceDb)].dbo.Authors;
SET IDENTITY_INSERT dbo.Authors OFF;

SET IDENTITY_INSERT dbo.Publishers ON;
INSERT dbo.Publishers(Id,Name,Address,Phone,Email,Website,IsDeleted,DeletedAt,CreatedAt,UpdatedAt)
SELECT Id,Name,Address,Phone,Email,Website,IsDeleted,NULL,CreatedDate,COALESCE(UpdatedDate,CreatedDate)
FROM [$(SourceDb)].dbo.Publishers;
SET IDENTITY_INSERT dbo.Publishers OFF;

SET IDENTITY_INSERT dbo.Shelves ON;
INSERT dbo.Shelves(Id,Code,LocationCode,Name,Area,Description,Capacity,IsDeleted,DeletedAt,CreatedAt,UpdatedAt)
SELECT Id,Code,LocationCode,Name,Area,Description,Capacity,0,NULL,CreatedDate,COALESCE(UpdatedDate,CreatedDate)
FROM [$(SourceDb)].dbo.Shelves;
SET IDENTITY_INSERT dbo.Shelves OFF;

SET IDENTITY_INSERT dbo.Positions ON;
INSERT dbo.Positions(Id,Code,Name,Description,SalaryAmount,AllowSalaryOverride,IsDeleted,DeletedAt,CreatedAt,UpdatedAt)
SELECT p.Id,CONCAT('POS-',p.Id),p.Name,p.Description,
       COALESCE((SELECT MIN(e.Salary) FROM [$(SourceDb)].dbo.Employees e WHERE e.PositionId=p.Id),0),
       0,0,NULL,p.CreatedAt,COALESCE(p.UpdatedAt,p.CreatedAt)
FROM [$(SourceDb)].dbo.Positions p;
SET IDENTITY_INSERT dbo.Positions OFF;

SET IDENTITY_INSERT dbo.EmployeeStatusOptions ON;
INSERT dbo.EmployeeStatusOptions(Id,Code,Name,Description,DisplayOrder,Status,IsDeleted,DeletedAt,CreatedAt,UpdatedAt)
SELECT Id,CONCAT('STATUS-',Id),Name,Description,DisplayOrder,
       CASE WHEN Id=2 THEN 1 WHEN Id=3 THEN 2 ELSE 0 END,0,NULL,CreatedAt,COALESCE(UpdatedAt,CreatedAt)
FROM [$(SourceDb)].dbo.EmployeeStatusOptions;
SET IDENTITY_INSERT dbo.EmployeeStatusOptions OFF;

SET IDENTITY_INSERT dbo.SecurityRoles ON;
INSERT dbo.SecurityRoles(Id,Code,Name,Description,IsSystem,IsDeleted,DeletedAt,CreatedAt,UpdatedAt)
SELECT Id,CONCAT('ROLE-',Id),Name,Description,CASE WHEN Name='Admin' THEN 1 ELSE 0 END,0,NULL,SYSUTCDATETIME(),SYSUTCDATETIME()
FROM [$(SourceDb)].dbo.Roles;
SET IDENTITY_INSERT dbo.SecurityRoles OFF;

SET IDENTITY_INSERT dbo.Permissions ON;
INSERT dbo.Permissions(Id,Code,DisplayName,GroupName,CreatedAt,UpdatedAt)
SELECT Id,Code,DisplayName,GroupName,SYSUTCDATETIME(),SYSUTCDATETIME()
FROM [$(SourceDb)].dbo.Permissions;
SET IDENTITY_INSERT dbo.Permissions OFF;

SET IDENTITY_INSERT dbo.RolePermissions ON;
INSERT dbo.RolePermissions(Id,RoleId,PermissionId,Effect,CreatedAt,UpdatedAt)
SELECT Id,RoleId,PermissionId,1,SYSUTCDATETIME(),SYSUTCDATETIME()
FROM [$(SourceDb)].dbo.RolePermissions;
SET IDENTITY_INSERT dbo.RolePermissions OFF;

-- People
SET IDENTITY_INSERT dbo.Readers ON;
INSERT dbo.Readers
(
 Id,UserAccountId,ReaderCode,FullName,DateOfBirth,Phone,Email,Address,IdentityNumber,
 RegisteredOn,ExpiresOn,LastLoginAt,ConsecutiveLoginDays,LastStreakOn,AvatarPath,
 CoverImagePath,Bio,ActiveBadge,FullPageBackgroundPath,CommentLockMode,CommentBlockedUntil,
 MaxBorrowBooks,Type,Gender,EmailConfirmed,ForcePasswordChange,IsNameSet,IsVerified,
 IsDeleted,DeletedAt,CreatedAt,UpdatedAt
)
SELECT
 r.Id,m.AccountId,r.ReaderCode,COALESCE(NULLIF(r.FullName,''),r.Email),CAST(r.DateOfBirth AS date),
 r.Phone,r.Email,r.Address,r.IdentityNumber,CAST(r.RegisterDate AS date),CAST(r.ExpirationDate AS date),
 r.LastLoginDate,r.ConsecutiveLoginDays,CAST(r.LastStreakDate AS date),r.Avatar,r.CoverImageUrl,
 r.Bio,r.ActiveBadge,r.FullPageBackgroundUrl,
 CASE WHEN r.IsCommentBlocked=0 THEN 0 WHEN r.CommentBlockedUntil IS NULL THEN 2 ELSE 1 END,
 r.CommentBlockedUntil,r.MaxBorrowBooks,r.ReadersType,r.Genders,r.EmailConfirmed,
 1,r.IsNameSet,r.IsVerified,r.IsDeleted,NULL,r.RegisterDate,COALESCE(r.LastLoginDate,r.RegisterDate)
FROM [$(SourceDb)].dbo.Readers r JOIN #ReaderAccountMap m ON m.ReaderId=r.Id;
SET IDENTITY_INSERT dbo.Readers OFF;

SET IDENTITY_INSERT dbo.Employees ON;
INSERT dbo.Employees
(
 Id,EmployeeCode,FullName,Email,Phone,Address,NationalId,Gender,DateOfBirth,HiredOn,
 PositionId,Salary,Status,StatusOptionId,LeaveReason,LeaveStartedOn,UnexcusedDays,
 LastUnexcusedOn,RoleId,IsDeleted,DeletedAt,CreatedAt,UpdatedAt
)
SELECT Id,EmployeeId,FullName,Email,Phone,Address,CCCD,Gender,CAST(DateOfBirth AS date),CAST(HireDate AS date),
 PositionId,Salary,CASE WHEN StatusId=2 THEN 1 WHEN StatusId=3 THEN 2 ELSE 0 END,StatusId,
 LeaveReason,CAST(LeaveStartDate AS date),UnexcusedDaysCount,CAST(LastUnexcusedDate AS date),
 RoleId,IsDeleted,NULL,HireDate,COALESCE(LeaveStartDate,HireDate)
FROM [$(SourceDb)].dbo.Employees;
SET IDENTITY_INSERT dbo.Employees OFF;

SET IDENTITY_INSERT dbo.UserPermissions ON;
INSERT dbo.UserPermissions(Id,EmployeeId,PermissionId,Effect,CreatedAt,UpdatedAt)
SELECT Id,EmployeeId,PermissionId,CASE WHEN IsGranted=1 THEN 1 ELSE 0 END,SYSUTCDATETIME(),SYSUTCDATETIME()
FROM [$(SourceDb)].dbo.UserPermissions;
SET IDENTITY_INSERT dbo.UserPermissions OFF;

-- Catalog
SET IDENTITY_INSERT dbo.Books ON;
INSERT dbo.Books
(
 Id,Isbn,Title,Description,Edition,Keywords,PublicationYear,CreatedBy,PageCount,Language,
 LastBorrowedAt,Price,TotalQuantity,AvailableQuantity,MinimumAge,CoverImagePath,Status,
 ViewCount,CategoryId,AuthorId,PublisherId,ShelfId,IsDeleted,DeletedAt,CreatedAt,UpdatedAt
)
SELECT Id,ISBN,Title,Description,Edition,Keywords,PublishYear,CreatedBy,PageCount,Language,
 LastBorrowDate,Price,Quantity,AvailableQuantity,MinimumAge,CoverImage,BooksStatus,ViewCount,
 CategoriesId,AuthorsId,PublishersId,ShelvesId,IsDeleted,NULL,SYSUTCDATETIME(),SYSUTCDATETIME()
FROM [$(SourceDb)].dbo.Books;
SET IDENTITY_INSERT dbo.Books OFF;

SET IDENTITY_INSERT dbo.ReaderFeaturedBooks ON;
INSERT dbo.ReaderFeaturedBooks(Id,ReaderId,BookId,DisplayOrder,CreatedAt,UpdatedAt)
SELECT ROW_NUMBER() OVER(ORDER BY r.Id,s.ordinal),r.Id,TRY_CONVERT(int,LTRIM(RTRIM(s.value))),CONVERT(int,s.ordinal),SYSUTCDATETIME(),SYSUTCDATETIME()
FROM [$(SourceDb)].dbo.Readers r
CROSS APPLY STRING_SPLIT(r.FeaturedBookIds,',',1) s
WHERE TRY_CONVERT(int,LTRIM(RTRIM(s.value))) IS NOT NULL AND s.ordinal<=4
  AND EXISTS(SELECT 1 FROM [$(SourceDb)].dbo.Books b WHERE b.Id=TRY_CONVERT(int,LTRIM(RTRIM(s.value))));
SET IDENTITY_INSERT dbo.ReaderFeaturedBooks OFF;

-- Circulation
SET IDENTITY_INSERT dbo.BorrowSlips ON;
INSERT dbo.BorrowSlips
(
 Id,Code,ReaderId,TotalBooks,RequestedAt,BorrowedAt,DueAt,ReturnedAt,PickupExpiresAt,
 Note,CreatedByAccountId,Status,IdempotencyKey,CreatedAt,UpdatedAt
)
SELECT b.Id,b.BorrowCode,b.ReaderId,b.TotalBooks,b.CreatedDate,
 CASE WHEN b.BorrowStatus IN(2,3,4,6,9) THEN b.BorrowDate END,
 b.DueDate,b.ReturnDate,b.PickupDeadline,b.Note,m.AccountId,b.BorrowStatus,
 CONCAT('legacy-borrow-',b.Id),b.CreatedDate,COALESCE(b.ReturnDate,b.CreatedDate)
FROM [$(SourceDb)].dbo.BorrowSlips b JOIN #ReaderAccountMap m ON m.ReaderId=b.ReaderId;
SET IDENTITY_INSERT dbo.BorrowSlips OFF;

SET IDENTITY_INSERT dbo.BorrowDetails ON;
INSERT dbo.BorrowDetails
(
 Id,BorrowSlipId,BookId,Quantity,ReturnedQuantity,BorrowPrice,ConditionBefore,Note,
 ExtensionCount,DueAt,RequestedExtensionDays,ExtensionReason,CreatedAt,UpdatedAt
)
SELECT Id,BorrowSlipId,BookId,Quantity,CASE WHEN IsReturned=1 THEN Quantity ELSE 0 END,
 BorrowPrice,ConditionBefore,Note,ExtensionCount,DueDate,RequestedExtensionDays,ExtensionReason,
 SYSUTCDATETIME(),COALESCE(ReturnedDate,SYSUTCDATETIME())
FROM [$(SourceDb)].dbo.BorrowDetails;
SET IDENTITY_INSERT dbo.BorrowDetails OFF;

SET IDENTITY_INSERT dbo.BorrowReturns ON;
INSERT dbo.BorrowReturns
(
 Id,BorrowDetailId,Quantity,Condition,DamagePercent,ConditionNote,ProcessedByAccountId,
 IdempotencyKey,CreatedAt,UpdatedAt
)
SELECT d.Id,d.Id,d.Quantity,
 CASE WHEN d.IsLost=1 THEN 2 WHEN d.IsDamaged=1 THEN 1 ELSE 0 END,
 COALESCE(d.DamagePercent,0),COALESCE(d.ConditionAfter,d.Note),m.AccountId,
 CONCAT('legacy-return-',d.Id),COALESCE(d.ReturnedDate,b.ReturnDate,b.CreatedDate),
 COALESCE(d.ReturnedDate,b.ReturnDate,b.CreatedDate)
FROM [$(SourceDb)].dbo.BorrowDetails d
JOIN [$(SourceDb)].dbo.BorrowSlips b ON b.Id=d.BorrowSlipId
JOIN #ReaderAccountMap m ON m.ReaderId=b.ReaderId
WHERE d.IsReturned=1 OR d.IsLost=1 OR d.IsDamaged=1;
SET IDENTITY_INSERT dbo.BorrowReturns OFF;

SET IDENTITY_INSERT dbo.Reservations ON;
INSERT dbo.Reservations(Id,ReaderId,BookId,ReservedAt,ExpiresAt,Status,Note,CreatedAt,UpdatedAt)
SELECT Id,ReaderId,BookId,ReservationDate,ExpirationDate,BooksStatus,Note,ReservationDate,COALESCE(ExpirationDate,ReservationDate)
FROM [$(SourceDb)].dbo.Reservations;
SET IDENTITY_INSERT dbo.Reservations OFF;

SET IDENTITY_INSERT dbo.Fines ON;
INSERT dbo.Fines
(
 Id,Code,BorrowSlipId,ReaderId,Type,Amount,PaidAmount,Reason,Status,TransactionCode,
 PaidAt,CreatedAt,UpdatedAt
)
SELECT f.Id,CONCAT('FN-LEGACY-',f.Id),f.BorrowSlipId,COALESCE(f.ReaderId,b.ReaderId),
 f.FinesType,f.Amount,CASE WHEN f.IsPaid=1 THEN f.Amount ELSE 0 END,f.Reason,
 CASE WHEN f.IsPaid=1 OR f.Status IN(N'Đã thanh toán','Paid') THEN 3
      WHEN f.Status='PendingApproval' THEN 1 ELSE 0 END,
 f.TransactionCode,f.PaidDate,f.CreatedDate,COALESCE(f.PaidDate,f.CreatedDate)
FROM [$(SourceDb)].dbo.Fines f JOIN [$(SourceDb)].dbo.BorrowSlips b ON b.Id=f.BorrowSlipId;
SET IDENTITY_INSERT dbo.Fines OFF;

SET IDENTITY_INSERT dbo.FineDetails ON;
INSERT dbo.FineDetails
(Id,FineId,BookId,BorrowDetailId,Type,Amount,Reason,DamagePercent,BookPrice,LateDays,CreatedAt,UpdatedAt)
SELECT d.Id,d.FineId,d.BookId,d.BorrowDetailId,d.FinesType,d.Amount,d.Reason,d.DamagePercent,
 COALESCE(d.BookPrice,b.Price),COALESCE(f.LateDays,0),d.CreatedDate,d.CreatedDate
FROM [$(SourceDb)].dbo.FineDetails d
JOIN [$(SourceDb)].dbo.Books b ON b.Id=d.BookId
JOIN [$(SourceDb)].dbo.Fines f ON f.Id=d.FineId;
SET IDENTITY_INSERT dbo.FineDetails OFF;

SET IDENTITY_INSERT dbo.SystemSettings ON;
INSERT dbo.SystemSettings
(Id,MaxExtensionDays,MaxExtensionCount,LateFeePerDay,MaxBorrowBooks,BorrowPeriodDays,PickupWindowDays,CreatedAt,UpdatedAt)
SELECT Id,MaxExtensionDays,MaxExtensionCount,LateFeePerDay,MaxBorrowBooks,BorrowPeriodDays,3,CreatedDate,COALESCE(UpdatedDate,CreatedDate)
FROM [$(SourceDb)].dbo.SystemSettings;
SET IDENTITY_INSERT dbo.SystemSettings OFF;

-- Community
SET IDENTITY_INSERT dbo.BookFavorites ON;
INSERT dbo.BookFavorites(Id,BookId,ReaderId,CreatedAt,UpdatedAt)
SELECT Id,BookId,ReaderId,CreatedAt,CreatedAt FROM [$(SourceDb)].dbo.BookFavorites;
SET IDENTITY_INSERT dbo.BookFavorites OFF;

SET IDENTITY_INSERT dbo.BookComments ON;
INSERT dbo.BookComments(Id,BookId,ReaderId,ParentCommentId,Content,IsDeleted,DeletedAt,CreatedAt,UpdatedAt)
SELECT Id,BookId,ReaderId,ParentCommentId,Content,0,NULL,CreatedAt,CreatedAt FROM [$(SourceDb)].dbo.BookComments;
SET IDENTITY_INSERT dbo.BookComments OFF;

SET IDENTITY_INSERT dbo.BookCommentLikes ON;
INSERT dbo.BookCommentLikes(Id,BookCommentId,ReaderId,CreatedAt,UpdatedAt)
SELECT Id,BookCommentId,ReaderId,CreatedAt,CreatedAt FROM [$(SourceDb)].dbo.BookCommentLikes;
SET IDENTITY_INSERT dbo.BookCommentLikes OFF;

SET IDENTITY_INSERT dbo.BookRatings ON;
INSERT dbo.BookRatings(Id,BookId,ReaderId,Stars,Review,CreatedAt,UpdatedAt)
SELECT Id,BookId,ReaderId,Rating,Review,CreatedAt,CreatedAt FROM [$(SourceDb)].dbo.BookRatings;
SET IDENTITY_INSERT dbo.BookRatings OFF;

SET IDENTITY_INSERT dbo.BookViewHistories ON;
INSERT dbo.BookViewHistories(Id,BookId,ReaderId,SessionId,ViewedAt,CreatedAt,UpdatedAt)
SELECT Id,BookId,ReaderId,SessionId,ViewedAt,ViewedAt,ViewedAt FROM [$(SourceDb)].dbo.BookViewHistories;
SET IDENTITY_INSERT dbo.BookViewHistories OFF;

SET IDENTITY_INSERT dbo.ReadingHistories ON;
INSERT dbo.ReadingHistories(Id,BookId,ReaderId,CurrentPage,TotalPages,LastReadAt,StartedAt,CreatedAt,UpdatedAt)
SELECT Id,BookId,ReaderId,CurrentPage,TotalPages,LastReadAt,StartedAt,StartedAt,LastReadAt FROM [$(SourceDb)].dbo.ReadingHistories;
SET IDENTITY_INSERT dbo.ReadingHistories OFF;

SET IDENTITY_INSERT dbo.ProfileComments ON;
INSERT dbo.ProfileComments(Id,ProfileOwnerId,AuthorId,Content,IsDeleted,DeletedAt,CreatedAt,UpdatedAt)
SELECT Id,ProfileOwnerId,AuthorId,Content,0,NULL,CreatedAt,CreatedAt FROM [$(SourceDb)].dbo.ProfileComments;
SET IDENTITY_INSERT dbo.ProfileComments OFF;

SET IDENTITY_INSERT dbo.ProfileTagPins ON;
INSERT dbo.ProfileTagPins(Id,ProfileOwnerId,AuthorId,TagKeysJson,PresetMessageKey,CreatedAt,UpdatedAt)
SELECT Id,ProfileOwnerId,AuthorId,TagKeysJson,PresetMessageKey,CreatedAt,CreatedAt FROM [$(SourceDb)].dbo.ProfileTagPins;
SET IDENTITY_INSERT dbo.ProfileTagPins OFF;

SET IDENTITY_INSERT dbo.CommentReports ON;
INSERT dbo.CommentReports
(Id,ReporterId,ReportedUserId,BookCommentId,ProfileCommentId,Reason,Status,ReviewedByAccountId,Resolution,CreatedAt,UpdatedAt)
SELECT Id,ReporterId,ReportedUserId,CommentId,NULL,Reason,0,NULL,NULL,CreatedAt,CreatedAt
FROM [$(SourceDb)].dbo.CommentReports;
SET IDENTITY_INSERT dbo.CommentReports OFF;

-- Support + notification
SET IDENTITY_INSERT dbo.SupportTickets ON;
INSERT dbo.SupportTickets
(Id,Code,ReaderId,AssignedStaffAccountId,Subject,Status,LastMessageAt,UnreadByReader,UnreadByStaff,ClosedAt,CreatedAt,UpdatedAt)
SELECT t.Id,t.TicketCode,t.ReaderId,m.AccountId,t.Subject,t.Status,t.LastMessageDate,
 t.UnreadByReader,t.UnreadByStaff,t.ClosedDate,t.CreatedDate,t.LastMessageDate
FROM [$(SourceDb)].dbo.SupportTickets t
LEFT JOIN #ReaderAccountMap m ON m.ReaderId=t.AssignedStaffId;
SET IDENTITY_INSERT dbo.SupportTickets OFF;

SET IDENTITY_INSERT dbo.SupportMessages ON;
INSERT dbo.SupportMessages
(Id,SupportTicketId,SenderType,SenderAccountId,Content,CreatedAt,UpdatedAt)
SELECT s.Id,s.SupportTicketId,s.SenderType,m.AccountId,s.Content,s.SentDate,s.SentDate
FROM [$(SourceDb)].dbo.SupportMessages s
LEFT JOIN #ReaderAccountMap m ON m.ReaderId=s.SenderId;
SET IDENTITY_INSERT dbo.SupportMessages OFF;

SET IDENTITY_INSERT dbo.Notifications ON;
INSERT dbo.Notifications
(Id,RecipientAccountId,Type,Title,Message,Link,IsRead,ReadAt,CreatedByAccountId,BroadcastGroupId,CreatedAt,UpdatedAt)
SELECT n.Id,m.AccountId,n.Type,n.Title,n.Message,n.Link,n.IsRead,n.ReadDate,NULL,n.BroadcastGroupId,n.CreatedDate,COALESCE(n.ReadDate,n.CreatedDate)
FROM [$(SourceDb)].dbo.Notifications n JOIN #ReaderAccountMap m ON m.ReaderId=n.RecipientId;
SET IDENTITY_INSERT dbo.Notifications OFF;

COMMIT TRANSACTION;

-- RECONCILIATION: mọi chênh lệch phải được giải thích trước cutover.
SELECT 'Readers' Entity,(SELECT COUNT(*) FROM [$(SourceDb)].dbo.Readers) SourceCount,(SELECT COUNT(*) FROM dbo.Readers) TargetCount
UNION ALL SELECT 'Books',(SELECT COUNT(*) FROM [$(SourceDb)].dbo.Books),(SELECT COUNT(*) FROM dbo.Books)
UNION ALL SELECT 'BorrowSlips',(SELECT COUNT(*) FROM [$(SourceDb)].dbo.BorrowSlips),(SELECT COUNT(*) FROM dbo.BorrowSlips)
UNION ALL SELECT 'BorrowDetails',(SELECT COUNT(*) FROM [$(SourceDb)].dbo.BorrowDetails),(SELECT COUNT(*) FROM dbo.BorrowDetails)
UNION ALL SELECT 'Reservations',(SELECT COUNT(*) FROM [$(SourceDb)].dbo.Reservations),(SELECT COUNT(*) FROM dbo.Reservations)
UNION ALL SELECT 'Fines',(SELECT COUNT(*) FROM [$(SourceDb)].dbo.Fines),(SELECT COUNT(*) FROM dbo.Fines)
UNION ALL SELECT 'SupportTickets',(SELECT COUNT(*) FROM [$(SourceDb)].dbo.SupportTickets),(SELECT COUNT(*) FROM dbo.SupportTickets);

SELECT
 (SELECT SUM(Quantity) FROM [$(SourceDb)].dbo.Books) OldTotal,
 (SELECT SUM(TotalQuantity) FROM dbo.Books) NewTotal,
 (SELECT SUM(AvailableQuantity) FROM [$(SourceDb)].dbo.Books) OldAvailable,
 (SELECT SUM(AvailableQuantity) FROM dbo.Books) NewAvailable,
 (SELECT SUM(Amount) FROM [$(SourceDb)].dbo.Fines) OldFine,
 (SELECT SUM(Amount) FROM dbo.Fines) NewFine;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
