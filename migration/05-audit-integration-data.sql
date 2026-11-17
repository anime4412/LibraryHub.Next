/*
  Chỉ đọc: liệt kê dữ liệu có dấu hiệu được tạo bởi integration test.
  Script không UPDATE/DELETE/TRUNCATE/DROP bất kỳ dữ liệu nào.
*/
SET NOCOUNT ON;

IF DB_NAME() <> N'LibraryHubNext'
    THROW 51000, N'Hãy chạy script trên database LibraryHubNext.', 1;

SELECT Id, Email, DisplayName, IsActive, EmailConfirmed
FROM dbo.AspNetUsers
WHERE Email LIKE N'integration.%@libraryhub.local'
   OR Email LIKE N'stage2.%@libraryhub.local'
ORDER BY Email;

SELECT Id, Isbn, Title, IsDeleted, CreatedAt
FROM dbo.Books
WHERE Title LIKE N'Integration Book%'
   OR Title LIKE N'Public page %'
   OR Title LIKE N'Borrow flow %'
   OR Title LIKE N'Reservation flow %'
   OR Title LIKE N'Stage 3 community %'
ORDER BY Id;

SELECT Id, Name, IsDeleted, CreatedAt
FROM dbo.Categories
WHERE Name LIKE N'Integration-%'
   OR Name LIKE N'Category %'
ORDER BY Id;
