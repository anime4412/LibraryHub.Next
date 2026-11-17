/*
  Optional, idempotent demo catalog for SQL Server database LibraryHubNext.
  This script never updates or deletes an existing record. It only inserts
  the lookup rows and books identified by the DEMO-* codes/ISBN values below.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

DECLARE @Now datetimeoffset = SYSDATETIMEOFFSET();

DECLARE @PreviousDemoCategories TABLE (Id int PRIMARY KEY);
DECLARE @PreviousDemoAuthors TABLE (Id int PRIMARY KEY);
DECLARE @PreviousDemoPublishers TABLE (Id int PRIMARY KEY);

INSERT INTO @PreviousDemoCategories
SELECT DISTINCT CategoryId FROM Books WHERE Isbn LIKE N'97860490010%';
INSERT INTO @PreviousDemoAuthors
SELECT DISTINCT AuthorId FROM Books WHERE Isbn LIKE N'97860490010%';
INSERT INTO @PreviousDemoPublishers
SELECT DISTINCT PublisherId FROM Books WHERE Isbn LIKE N'97860490010%';

IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = N'Văn học Việt Nam' AND IsDeleted = 0)
    INSERT INTO Categories (Name, Description, CreatedAt, UpdatedAt, IsDeleted)
    VALUES (N'Văn học Việt Nam', N'Tác phẩm văn học và tản văn của tác giả Việt Nam.', @Now, @Now, 0);

IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = N'Kỹ năng sống' AND IsDeleted = 0)
    INSERT INTO Categories (Name, Description, CreatedAt, UpdatedAt, IsDeleted)
    VALUES (N'Kỹ năng sống', N'Sách phát triển tư duy, thói quen và năng lực cá nhân.', @Now, @Now, 0);

IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = N'Khoa học và công nghệ' AND IsDeleted = 0)
    INSERT INTO Categories (Name, Description, CreatedAt, UpdatedAt, IsDeleted)
    VALUES (N'Khoa học và công nghệ', N'Kiến thức khoa học, dữ liệu và công nghệ dành cho bạn đọc phổ thông.', @Now, @Now, 0);

IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = N'Lịch sử và văn hóa' AND IsDeleted = 0)
    INSERT INTO Categories (Name, Description, CreatedAt, UpdatedAt, IsDeleted)
    VALUES (N'Lịch sử và văn hóa', N'Khám phá lịch sử, địa phương và đời sống văn hóa Việt Nam.', @Now, @Now, 0);

IF NOT EXISTS (SELECT 1 FROM Publishers WHERE Name = N'Nhà xuất bản Ánh Tri Thức' AND IsDeleted = 0)
    INSERT INTO Publishers (Name, Address, Phone, Email, Website, CreatedAt, UpdatedAt, IsDeleted)
    VALUES (N'Nhà xuất bản Ánh Tri Thức', N'Hà Nội', N'024 7300 2026',
            N'lienhe@anhtrithuc.demo', N'https://anhtrithuc.demo', @Now, @Now, 0);

IF NOT EXISTS (SELECT 1 FROM Shelves WHERE Code = N'DEMO-A01' AND IsDeleted = 0)
    INSERT INTO Shelves (Code, LocationCode, Name, Area, Description, Capacity, CreatedAt, UpdatedAt, IsDeleted)
    VALUES (N'DEMO-A01', N'T1-A01', N'Văn học và văn hóa', N'Tầng 1',
            N'Kệ trình diễn dành cho văn học và văn hóa.', 200, @Now, @Now, 0);

IF NOT EXISTS (SELECT 1 FROM Shelves WHERE Code = N'DEMO-B01' AND IsDeleted = 0)
    INSERT INTO Shelves (Code, LocationCode, Name, Area, Description, Capacity, CreatedAt, UpdatedAt, IsDeleted)
    VALUES (N'DEMO-B01', N'T1-B01', N'Kỹ năng và tư duy', N'Tầng 1',
            N'Kệ trình diễn dành cho kỹ năng sống.', 200, @Now, @Now, 0);

IF NOT EXISTS (SELECT 1 FROM Shelves WHERE Code = N'DEMO-C01' AND IsDeleted = 0)
    INSERT INTO Shelves (Code, LocationCode, Name, Area, Description, Capacity, CreatedAt, UpdatedAt, IsDeleted)
    VALUES (N'DEMO-C01', N'T2-C01', N'Khoa học và công nghệ', N'Tầng 2',
            N'Kệ trình diễn dành cho khoa học và công nghệ.', 200, @Now, @Now, 0);

UPDATE Shelves
SET Name = CASE Code
        WHEN N'DEMO-A01' THEN N'Văn học và văn hóa'
        WHEN N'DEMO-B01' THEN N'Kỹ năng và tư duy'
        WHEN N'DEMO-C01' THEN N'Khoa học và công nghệ'
    END,
    Area = CASE Code WHEN N'DEMO-C01' THEN N'Tầng 2' ELSE N'Tầng 1' END,
    Description = CASE Code
        WHEN N'DEMO-A01' THEN N'Kệ trình diễn dành cho văn học và văn hóa.'
        WHEN N'DEMO-B01' THEN N'Kệ trình diễn dành cho kỹ năng sống.'
        WHEN N'DEMO-C01' THEN N'Kệ trình diễn dành cho khoa học và công nghệ.'
    END,
    UpdatedAt = @Now
WHERE Code IN (N'DEMO-A01', N'DEMO-B01', N'DEMO-C01') AND IsDeleted = 0;

DECLARE @Authors TABLE (Name nvarchar(200), Biography nvarchar(1000));
INSERT INTO @Authors (Name, Biography) VALUES
    (N'Lê Minh An', N'Tác giả viết về văn hóa đọc và những trải nghiệm đời thường.'),
    (N'Nguyễn Hà Vy', N'Tác giả trẻ theo đuổi dòng văn học chữa lành.'),
    (N'Trần Quốc Minh', N'Chuyên gia đào tạo kỹ năng tư duy và giao tiếp.'),
    (N'Phạm Gia Hân', N'Tác giả phổ biến kiến thức dữ liệu cho người mới bắt đầu.'),
    (N'Vũ Hoàng Nam', N'Nhà nghiên cứu lịch sử và văn hóa đô thị Việt Nam.'),
    (N'Mai Thu Trang', N'Tác giả ẩm thực và ký ức gia đình.'),
    (N'Đỗ Anh Khoa', N'Tác giả giáo dục khoa học và bảo tồn thiên nhiên.');

INSERT INTO Authors (Name, Nationality, Biography, CreatedAt, UpdatedAt, IsDeleted)
SELECT source.Name, N'Việt Nam', source.Biography, @Now, @Now, 0
FROM @Authors source
WHERE NOT EXISTS
(
    SELECT 1 FROM Authors target
    WHERE target.Name = source.Name AND target.IsDeleted = 0
);

DECLARE @DemoBooks TABLE
(
    Isbn nvarchar(30),
    Title nvarchar(300),
    Description nvarchar(1200),
    PublicationYear int,
    PageCount int,
    Price decimal(18,2),
    TotalQuantity int,
    AvailableQuantity int,
    ViewCount bigint,
    CategoryName nvarchar(200),
    AuthorName nvarchar(200),
    ShelfCode nvarchar(50)
);

INSERT INTO @DemoBooks VALUES
    (N'9786049001001', N'Khu Vườn Của Những Trang Sách',
     N'Một hành trình dịu dàng về thư viện nhỏ, tình bạn và niềm vui tìm thấy chính mình qua từng trang sách.',
     2025, 288, 128000, 12, 10, 126, N'Văn học Việt Nam', N'Lê Minh An', N'DEMO-A01'),
    (N'9786049001002', N'Chạm Vào Mùa Gió',
     N'Tập truyện ngắn về tuổi trẻ, gia đình và những đổi thay tinh tế trong nhịp sống hiện đại.',
     2026, 240, 112000, 10, 8, 98, N'Văn học Việt Nam', N'Nguyễn Hà Vy', N'DEMO-A01'),
    (N'9786049001003', N'Tư Duy Mạch Lạc',
     N'Các phương pháp thực hành giúp sắp xếp thông tin, đặt câu hỏi tốt và đưa ra quyết định có căn cứ.',
     2024, 320, 165000, 14, 11, 115, N'Kỹ năng sống', N'Trần Quốc Minh', N'DEMO-B01'),
    (N'9786049001004', N'Hành Trình Cùng Dữ Liệu',
     N'Giải thích dữ liệu, biểu đồ và tư duy phân tích bằng các ví dụ gần gũi trong học tập và công việc.',
     2026, 356, 189000, 9, 7, 87, N'Khoa học và công nghệ', N'Phạm Gia Hân', N'DEMO-C01'),
    (N'9786049001005', N'Lịch Sử Việt Nam Qua Những Thành Phố',
     N'Những lát cắt lịch sử được kể qua kiến trúc, con người và ký ức của các đô thị Việt Nam.',
     2025, 416, 215000, 8, 6, 103, N'Lịch sử và văn hóa', N'Vũ Hoàng Nam', N'DEMO-A01'),
    (N'9786049001006', N'Bếp Nhà Và Những Mùa Thương',
     N'Ghi chép ẩm thực gắn với ký ức gia đình, phong tục vùng miền và những bữa cơm sum họp.',
     2024, 264, 148000, 11, 9, 76, N'Lịch sử và văn hóa', N'Mai Thu Trang', N'DEMO-A01'),
    (N'9786049001007', N'Thiên Nhiên Trong Tầm Tay',
     N'Cẩm nang quan sát thế giới tự nhiên và bắt đầu những hành động bảo vệ môi trường từ đời sống hằng ngày.',
     2025, 304, 172000, 10, 8, 92, N'Khoa học và công nghệ', N'Đỗ Anh Khoa', N'DEMO-C01'),
    (N'9786049001008', N'Nuôi Dưỡng Thói Quen Đọc Sâu',
     N'Hướng dẫn xây dựng không gian, lịch đọc và phương pháp ghi chú để đọc tập trung và nhớ lâu hơn.',
     2026, 224, 119000, 15, 13, 134, N'Kỹ năng sống', N'Lê Minh An', N'DEMO-B01');

INSERT INTO Books
(
    Isbn, Title, Description, Edition, Keywords, PublicationYear, CreatedBy,
    PageCount, Language, Price, TotalQuantity, AvailableQuantity, MinimumAge,
    Status, ViewCount, CategoryId, AuthorId, PublisherId, ShelfId,
    CreatedAt, UpdatedAt, IsDeleted
)
SELECT
    source.Isbn, source.Title, source.Description, N'Bản tiêu chuẩn',
    N'demo, thư viện, tiếng Việt', source.PublicationYear, N'Demo catalog seed',
    source.PageCount, N'Tiếng Việt', source.Price, source.TotalQuantity,
    source.AvailableQuantity, 0, 0, source.ViewCount,
    categoryRow.Id, authorRow.Id, publisherRow.Id, shelfRow.Id,
    @Now, @Now, 0
FROM @DemoBooks source
CROSS APPLY
(
    SELECT TOP (1) Id FROM Categories
    WHERE Name = source.CategoryName AND IsDeleted = 0
    ORDER BY Id
) categoryRow
CROSS APPLY
(
    SELECT TOP (1) Id FROM Authors
    WHERE Name = source.AuthorName AND IsDeleted = 0
    ORDER BY Id
) authorRow
CROSS APPLY
(
    SELECT TOP (1) Id FROM Publishers
    WHERE Name = N'Nhà xuất bản Ánh Tri Thức' AND IsDeleted = 0
    ORDER BY Id
) publisherRow
CROSS APPLY
(
    SELECT TOP (1) Id FROM Shelves
    WHERE Code = source.ShelfCode AND IsDeleted = 0
    ORDER BY Id
) shelfRow
WHERE NOT EXISTS (SELECT 1 FROM Books target WHERE target.Isbn = source.Isbn);

UPDATE target
SET target.Title = source.Title,
    target.Description = source.Description,
    target.Edition = N'Bản tiêu chuẩn',
    target.Keywords = N'demo, thư viện, tiếng Việt',
    target.PublicationYear = source.PublicationYear,
    target.CreatedBy = N'Demo catalog seed',
    target.PageCount = source.PageCount,
    target.Language = N'Tiếng Việt',
    target.Price = source.Price,
    target.TotalQuantity = source.TotalQuantity,
    target.AvailableQuantity =
        CASE WHEN target.AvailableQuantity > source.TotalQuantity
             THEN source.TotalQuantity ELSE target.AvailableQuantity END,
    target.MinimumAge = 0,
    target.CategoryId = categoryRow.Id,
    target.AuthorId = authorRow.Id,
    target.PublisherId = publisherRow.Id,
    target.ShelfId = shelfRow.Id,
    target.UpdatedAt = @Now
FROM Books target
JOIN @DemoBooks source ON source.Isbn = target.Isbn
CROSS APPLY
(
    SELECT TOP (1) Id FROM Categories
    WHERE Name = source.CategoryName AND IsDeleted = 0
    ORDER BY Id
) categoryRow
CROSS APPLY
(
    SELECT TOP (1) Id FROM Authors
    WHERE Name = source.AuthorName AND IsDeleted = 0
    ORDER BY Id
) authorRow
CROSS APPLY
(
    SELECT TOP (1) Id FROM Publishers
    WHERE Name = N'Nhà xuất bản Ánh Tri Thức' AND IsDeleted = 0
    ORDER BY Id
) publisherRow
CROSS APPLY
(
    SELECT TOP (1) Id FROM Shelves
    WHERE Code = source.ShelfCode AND IsDeleted = 0
    ORDER BY Id
) shelfRow;

DELETE target
FROM Categories target
WHERE target.Id IN (SELECT Id FROM @PreviousDemoCategories)
  AND NOT EXISTS (SELECT 1 FROM Books WHERE CategoryId = target.Id);

DELETE target
FROM Authors target
WHERE target.Id IN (SELECT Id FROM @PreviousDemoAuthors)
  AND NOT EXISTS (SELECT 1 FROM Books WHERE AuthorId = target.Id);

DELETE target
FROM Publishers target
WHERE target.Id IN (SELECT Id FROM @PreviousDemoPublishers)
  AND NOT EXISTS (SELECT 1 FROM Books WHERE PublisherId = target.Id);

COMMIT TRANSACTION;

SELECT COUNT(*) AS DemoBookCount
FROM Books
WHERE Isbn LIKE N'97860490010%';
