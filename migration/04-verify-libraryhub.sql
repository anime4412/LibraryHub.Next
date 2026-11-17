/* Chạy với sqlcmd -d LibraryHubNext hoặc chọn LibraryHubNext trong SSMS. */
SET NOCOUNT ON;

IF DB_NAME() <> N'LibraryHubNext'
    THROW 51000, N'Hãy chạy script trên database LibraryHubNext.', 1;

SELECT DB_NAME() AS DatabaseName,
       CAST(SERVERPROPERTY('ServerName') AS nvarchar(128)) AS ServerName,
       (SELECT COUNT(*) FROM dbo.__EFMigrationsHistory) AS AppliedMigrations,
       (SELECT COUNT(*) FROM dbo.Books WHERE IsDeleted = 0) AS ActiveBooks,
       (SELECT COUNT(*) FROM dbo.Categories WHERE IsDeleted = 0) AS ActiveCategories,
       (SELECT COUNT(*) FROM dbo.AspNetUsers) AS Accounts;
