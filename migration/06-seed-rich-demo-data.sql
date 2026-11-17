/*
  LibraryHub.Next - rich demo dataset

  Chạy bằng sqlcmd hoặc SSMS SQLCMD Mode.
  Ví dụ:
    sqlcmd -S "ADMIN\SQLEXPRESS" -E -C -d "LibraryHubNext" -f 65001 -b \
      -i "migration\06-seed-rich-demo-data.sql"

  Đặc tính an toàn:
  - Idempotent theo ISBN, code, email và khóa DEMO-*.
  - Không xóa, không truncate và không sửa password/token/secret.
  - Chỉ bổ sung dữ liệu nghiệp vụ gắn tài khoản khi đủ hồ sơ liên kết.
  - Ảnh bìa dùng Open Library Covers API; avatar dùng DiceBear HTTP API.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_NAME() NOT IN (N'LibraryHubNext', N'LibraryHubNext_IntegrationTests')
    THROW 51000, N'Hãy chọn đúng database LibraryHubNext hoặc LibraryHubNext_IntegrationTests.', 1;

IF OBJECT_ID(N'dbo.__EFMigrationsHistory', N'U') IS NULL
    THROW 51000, N'Database đích chưa có schema EF Core của LibraryHub.Next.', 1;

DECLARE @Now datetimeoffset = SYSDATETIMEOFFSET();

BEGIN TRY
BEGIN TRANSACTION;

/* -------------------------------------------------------------------------- */
/* 1. Cấu hình, chức vụ, trạng thái nhân viên và vai trò nghiệp vụ            */
/* -------------------------------------------------------------------------- */

IF NOT EXISTS (SELECT 1 FROM dbo.SystemSettings)
BEGIN
    INSERT dbo.SystemSettings
        (MaxExtensionDays, MaxExtensionCount, LateFeePerDay, MaxBorrowBooks,
         BorrowPeriodDays, PickupWindowDays, CreatedAt, UpdatedAt)
    VALUES (7, 1, 5000, 10, 14, 3, @Now, @Now);
END;

DECLARE @Positions TABLE
(
    Code nvarchar(50) PRIMARY KEY,
    Name nvarchar(200),
    Description nvarchar(1000),
    SalaryAmount decimal(18,2),
    AllowSalaryOverride bit
);
INSERT @Positions VALUES
    (N'DEMO-LIBRARIAN', N'Thủ thư', N'Phụ trách mượn, trả và hỗ trợ độc giả tại quầy.', 12000000, 1),
    (N'DEMO-CIRCULATION', N'Chuyên viên lưu thông', N'Theo dõi phiếu mượn, đặt trước, gia hạn và khoản phạt.', 13500000, 1),
    (N'DEMO-CATALOG', N'Chuyên viên biên mục', N'Quản lý sách, tác giả, nhà xuất bản, danh mục và kệ.', 14000000, 1),
    (N'DEMO-SUPPORT', N'Chuyên viên hỗ trợ', N'Tiếp nhận ticket và hướng dẫn độc giả sử dụng hệ thống.', 11500000, 1);

INSERT dbo.Positions
    (Code, Name, Description, SalaryAmount, AllowSalaryOverride, IsDeleted, DeletedAt, CreatedAt, UpdatedAt)
SELECT p.Code, p.Name, p.Description, p.SalaryAmount, p.AllowSalaryOverride, 0, NULL, @Now, @Now
FROM @Positions p
WHERE NOT EXISTS (SELECT 1 FROM dbo.Positions target WHERE target.Code = p.Code);

DECLARE @EmployeeStatuses TABLE
(
    Code nvarchar(50) PRIMARY KEY,
    Name nvarchar(200),
    Description nvarchar(1000),
    DisplayOrder int,
    Status int
);
INSERT @EmployeeStatuses VALUES
    (N'DEMO-WORKING', N'Đang làm việc', N'Nhân viên đang làm việc bình thường.', 10, 0),
    (N'DEMO-LEAVE', N'Nghỉ phép', N'Nhân viên đang trong kỳ nghỉ đã được duyệt.', 20, 0),
    (N'DEMO-SUSPENDED', N'Tạm đình chỉ', N'Tạm ngừng quyền xử lý nghiệp vụ.', 30, 1),
    (N'DEMO-RESIGNED', N'Đã nghỉ việc', N'Hồ sơ nhân viên đã kết thúc công tác.', 40, 2);

INSERT dbo.EmployeeStatusOptions
    (Code, Name, Description, DisplayOrder, Status, IsDeleted, DeletedAt, CreatedAt, UpdatedAt)
SELECT s.Code, s.Name, s.Description, s.DisplayOrder, s.Status, 0, NULL, @Now, @Now
FROM @EmployeeStatuses s
WHERE NOT EXISTS (SELECT 1 FROM dbo.EmployeeStatusOptions target WHERE target.Code = s.Code);

DECLARE @SecurityRoles TABLE
(
    Code nvarchar(50) PRIMARY KEY,
    Name nvarchar(200),
    Description nvarchar(1000)
);
INSERT @SecurityRoles VALUES
    (N'DEMO-LIBRARY-STAFF', N'Nhân viên thư viện', N'Quyền cơ bản để phục vụ độc giả và lưu thông sách.'),
    (N'DEMO-CIRCULATION-STAFF', N'Nhân viên lưu thông', N'Quản lý duyệt, giao, trả, gia hạn và đặt trước.'),
    (N'DEMO-CATALOG-MANAGER', N'Quản lý biên mục', N'Quản lý sách và toàn bộ dữ liệu tra cứu.'),
    (N'DEMO-SUPPORT-STAFF', N'Nhân viên hỗ trợ', N'Tiếp nhận và xử lý ticket hỗ trợ.');

INSERT dbo.SecurityRoles
    (Code, Name, Description, IsSystem, IsDeleted, DeletedAt, CreatedAt, UpdatedAt)
SELECT r.Code, r.Name, r.Description, 0, 0, NULL, @Now, @Now
FROM @SecurityRoles r
WHERE NOT EXISTS (SELECT 1 FROM dbo.SecurityRoles target WHERE target.Code = r.Code);

DECLARE @RolePermissionMap TABLE (RoleCode nvarchar(50), PermissionCode nvarchar(100));
INSERT @RolePermissionMap VALUES
    (N'DEMO-LIBRARY-STAFF', N'catalog.view'),
    (N'DEMO-LIBRARY-STAFF', N'reader.view'),
    (N'DEMO-LIBRARY-STAFF', N'borrow.approve'),
    (N'DEMO-LIBRARY-STAFF', N'borrow.confirm_pickup'),
    (N'DEMO-LIBRARY-STAFF', N'borrow.confirm_return'),
    (N'DEMO-LIBRARY-STAFF', N'support.claim'),
    (N'DEMO-CIRCULATION-STAFF', N'catalog.view'),
    (N'DEMO-CIRCULATION-STAFF', N'reader.view'),
    (N'DEMO-CIRCULATION-STAFF', N'reader.edit'),
    (N'DEMO-CIRCULATION-STAFF', N'borrow.approve'),
    (N'DEMO-CIRCULATION-STAFF', N'borrow.confirm_pickup'),
    (N'DEMO-CIRCULATION-STAFF', N'borrow.approve_extension'),
    (N'DEMO-CIRCULATION-STAFF', N'borrow.confirm_return'),
    (N'DEMO-CIRCULATION-STAFF', N'reservation.manage'),
    (N'DEMO-CATALOG-MANAGER', N'catalog.view'),
    (N'DEMO-CATALOG-MANAGER', N'catalog.manage'),
    (N'DEMO-CATALOG-MANAGER', N'book.create'),
    (N'DEMO-CATALOG-MANAGER', N'book.edit'),
    (N'DEMO-CATALOG-MANAGER', N'book.delete'),
    (N'DEMO-SUPPORT-STAFF', N'reader.view'),
    (N'DEMO-SUPPORT-STAFF', N'support.claim');

INSERT dbo.RolePermissions (RoleId, PermissionId, Effect, CreatedAt, UpdatedAt)
SELECT role.Id, permission.Id, 1, @Now, @Now
FROM @RolePermissionMap map
JOIN dbo.SecurityRoles role ON role.Code = map.RoleCode
JOIN dbo.Permissions permission ON permission.Code = map.PermissionCode
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.RolePermissions target
    WHERE target.RoleId = role.Id AND target.PermissionId = permission.Id
);

/* -------------------------------------------------------------------------- */
/* 2. Danh mục, kệ và 32 đầu sách                                             */
/* -------------------------------------------------------------------------- */

DECLARE @Categories TABLE
(
    Name nvarchar(200) PRIMARY KEY,
    Description nvarchar(1000),
    ShelfCode nvarchar(50),
    ShelfName nvarchar(200),
    Area nvarchar(100)
);
INSERT @Categories VALUES
    (N'Toán & khoa học máy tính', N'Toán rời rạc, thuật toán và nền tảng khoa học máy tính.', N'DEMO-CS01', N'Toán và khoa học máy tính', N'Tầng 2'),
    (N'Lập trình & bảo mật', N'Lập trình, kiến trúc máy tính, lịch sử hacker và an toàn thông tin.', N'DEMO-CS02', N'Lập trình và bảo mật', N'Tầng 2'),
    (N'Khoa học viễn tưởng', N'Tiểu thuyết khám phá công nghệ, không gian và xã hội tương lai.', N'DEMO-SF01', N'Khoa học viễn tưởng', N'Tầng 1'),
    (N'Phát triển bản thân', N'Thói quen, năng suất và tư duy phát triển bền vững.', N'DEMO-SH01', N'Phát triển bản thân', N'Tầng 1'),
    (N'Tâm lý & chữa lành', N'Sách về nhận thức, cảm xúc và hành trình hồi phục.', N'DEMO-SH02', N'Tâm lý và chữa lành', N'Tầng 1'),
    (N'Tiểu thuyết đương đại', N'Tác phẩm hư cấu hiện đại dành cho bạn đọc trẻ và trưởng thành.', N'DEMO-FI01', N'Tiểu thuyết đương đại', N'Tầng 1'),
    (N'Văn học kinh điển', N'Những tác phẩm có ảnh hưởng lâu dài trong lịch sử văn học.', N'DEMO-FI02', N'Văn học kinh điển', N'Tầng 1'),
    (N'Khoa học tự nhiên', N'Sinh học, vật lý và các chủ đề khoa học phổ thông.', N'DEMO-SC01', N'Khoa học tự nhiên', N'Tầng 2');

INSERT dbo.Categories (Name, Description, IsDeleted, DeletedAt, CreatedAt, UpdatedAt)
SELECT c.Name, c.Description, 0, NULL, @Now, @Now
FROM @Categories c
WHERE NOT EXISTS (SELECT 1 FROM dbo.Categories target WHERE target.Name = c.Name AND target.IsDeleted = 0);

INSERT dbo.Shelves
    (Code, LocationCode, Name, Area, Description, Capacity, IsDeleted, DeletedAt, CreatedAt, UpdatedAt)
SELECT c.ShelfCode, CONCAT(N'LH-', c.ShelfCode), c.ShelfName, c.Area,
       CONCAT(N'Kệ dữ liệu mẫu: ', c.Name), 300, 0, NULL, @Now, @Now
FROM @Categories c
WHERE NOT EXISTS (SELECT 1 FROM dbo.Shelves target WHERE target.Code = c.ShelfCode);

DECLARE @Books TABLE
(
    Isbn nvarchar(30) PRIMARY KEY,
    Title nvarchar(300),
    Description nvarchar(1200),
    PublicationYear int,
    PageCount int,
    Price decimal(18,2),
    TotalQuantity int,
    AvailableQuantity int,
    MinimumAge int,
    Status int,
    ViewCount bigint,
    CategoryName nvarchar(200),
    AuthorName nvarchar(300),
    PublisherName nvarchar(300),
    ShelfCode nvarchar(50),
    CoverId bigint,
    Keywords nvarchar(500)
);

INSERT @Books VALUES
    (N'9780534359454', N'Discrete Mathematics with Applications', N'Giáo trình nhập môn toán rời rạc với logic, tổ hợp, đồ thị và nhiều bài tập ứng dụng.', 1990, 889, 285000, 8, 5, 16, 0, 186, N'Toán & khoa học máy tính', N'Susanna S. Epp', N'Wadsworth Pub. Co.', N'DEMO-CS01', 9295752, N'toán rời rạc, logic, đồ thị'),
    (N'9780596158064', N'Learning Python', N'Tài liệu thực hành Python từ cú pháp nền tảng đến lập trình hướng đối tượng và module.', 1999, 591, 245000, 12, 8, 14, 0, 245, N'Lập trình & bảo mật', N'Mark Lutz', N'O''Reilly', N'DEMO-CS02', 1312568, N'Python, lập trình, phần mềm'),
    (N'9788576571100', N'The Difference Engine', N'Tiểu thuyết giả tưởng về một thế giới nơi máy tính cơ khí làm thay đổi lịch sử.', 1990, 395, 168000, 7, 4, 16, 0, 137, N'Khoa học viễn tưởng', N'William Gibson', N'Spectra/Bantam Books', N'DEMO-SF01', 4034359, N'công nghệ, lịch sử giả tưởng, steampunk'),
    (N'9781850180005', N'2001: A Space Odyssey', N'Hành trình khoa học viễn tưởng kinh điển về trí tuệ nhân tạo và khám phá không gian.', 1968, 237, 155000, 9, 6, 14, 0, 321, N'Khoa học viễn tưởng', N'Arthur C. Clarke', N'Debolsillo', N'DEMO-SF01', 11344400, N'không gian, trí tuệ nhân tạo, tương lai'),
    (N'9781449388393', N'Hackers', N'Lịch sử văn hóa hacker và những con người tiên phong của ngành máy tính cá nhân.', 1984, 456, 210000, 6, 3, 16, 0, 198, N'Lập trình & bảo mật', N'Steven Levy', N'Anchor Press/Doubleday', N'DEMO-CS02', 106396, N'hacker, lịch sử máy tính, công nghệ'),
    (N'9781119485544', N'Hacking for Dummies', N'Tổng quan thực hành về đánh giá lỗ hổng, kiểm thử và phòng vệ hệ thống.', 2004, 396, 225000, 5, 0, 18, 4, 164, N'Lập trình & bảo mật', N'Kevin Beaver', N'Wiley Pub.', N'DEMO-CS02', 299671, N'bảo mật, kiểm thử, an toàn thông tin'),
    (N'9780735605053', N'Code', N'Giải thích cách máy tính hoạt động từ mã Morse, mạch điện đến kiến trúc phần mềm.', 1999, 393, 198000, 10, 7, 14, 0, 276, N'Lập trình & bảo mật', N'Charles Petzold', N'Microsoft Press', N'DEMO-CS02', 460968, N'mã hóa, kiến trúc máy tính, nền tảng'),
    (N'9780356521640', N'The Player of Games', N'Tiểu thuyết về trò chơi, quyền lực và một nền văn minh ngoài Trái Đất.', 1988, 320, 145000, 8, 5, 16, 0, 119, N'Khoa học viễn tưởng', N'Iain M. Banks', N'Orbit', N'DEMO-SF01', 44077, N'không gian, trò chơi, xã hội'),

    (N'9789937074391', N'The Power of Your Subconscious Mind', N'Khám phá ảnh hưởng của tiềm thức lên thói quen, niềm tin và lựa chọn hằng ngày.', 1963, 222, 125000, 10, 8, 14, 0, 301, N'Phát triển bản thân', N'Joseph Murphy', N'Wildside Press', N'DEMO-SH01', 6553019, N'tiềm thức, tư duy, thói quen'),
    (N'9798893310153', N'Don''t Believe Everything You Think', N'Gợi ý cách quan sát suy nghĩ và giảm những vòng lặp tinh thần không cần thiết.', 2022, 192, 139000, 12, 9, 14, 0, 212, N'Tâm lý & chữa lành', N'Joseph Nguyen', N'Authors Equity', N'DEMO-SH02', 15160816, N'suy nghĩ, nhận thức, bình an'),
    (N'9780345816023', N'12 Rules for Life', N'Mười hai nguyên tắc suy ngẫm về trách nhiệm cá nhân, trật tự và ý nghĩa cuộc sống.', 2015, 480, 189000, 8, 5, 16, 0, 287, N'Phát triển bản thân', N'Jordan B. Peterson', N'Planeta', N'DEMO-SH01', 8131760, N'nguyên tắc sống, trách nhiệm, tâm lý'),
    (N'9786070788734', N'The Mountain Is You', N'Hướng dẫn nhận diện hành vi tự cản trở và xây dựng khả năng thích nghi cảm xúc.', 2020, 248, 149000, 11, 7, 14, 0, 239, N'Tâm lý & chữa lành', N'Brianna Wiest', N'Thought Catalog Books', N'DEMO-SH02', 13838236, N'chữa lành, cảm xúc, thay đổi'),
    (N'9781949759297', N'A Gentle Reminder', N'Những lời nhắc dịu dàng về giá trị bản thân, ranh giới và sự trưởng thành.', 2020, 146, 119000, 9, 6, 14, 0, 191, N'Tâm lý & chữa lành', N'Bianca Sparacino', N'Thought Catalog', N'DEMO-SH02', 14424196, N'giá trị bản thân, chữa lành, cảm xúc'),
    (N'9781949759440', N'When You''re Ready, This Is How You Heal', N'Các bài viết ngắn về quá trình hồi phục và xây dựng lại kết nối với chính mình.', 2022, 224, 155000, 7, 4, 14, 0, 173, N'Tâm lý & chữa lành', N'Brianna Wiest', N'Thought Catalog Books', N'DEMO-SH02', 15000579, N'hồi phục, chữa lành, tự nhận thức'),
    (N'9781605090962', N'Eat That Frog!', N'Phương pháp ưu tiên việc quan trọng và hạn chế trì hoãn trong công việc.', 2001, 128, 99000, 14, 11, 14, 0, 265, N'Phát triển bản thân', N'Brian Tracy', N'Audio Literature', N'DEMO-SH01', 847534, N'năng suất, trì hoãn, quản lý thời gian'),
    (N'9798545118748', N'Atomic Habits', N'Cách xây dựng thói quen tốt bằng những thay đổi nhỏ, rõ ràng và có thể duy trì.', 2016, 322, 175000, 18, 13, 14, 0, 512, N'Phát triển bản thân', N'James Clear', N'Editorial Planeta', N'DEMO-SH01', 12539702, N'thói quen, cải thiện, năng suất'),

    (N'9780241753859', N'The Summer I Turned Pretty Trilogy', N'Bộ truyện trưởng thành về mùa hè, gia đình, tình bạn và những lựa chọn đầu đời.', 2009, 851, 239000, 7, 5, 13, 0, 184, N'Tiểu thuyết đương đại', N'Jenny Han', N'Simon & Schuster BFYR', N'DEMO-FI01', 7719210, N'tuổi trẻ, gia đình, tình bạn'),
    (N'9786053243113', N'The Picture of Dorian Gray', N'Tác phẩm kinh điển về cái đẹp, đạo đức và cái giá của ham muốn vĩnh cửu.', 1890, 246, 129000, 10, 7, 16, 0, 337, N'Văn học kinh điển', N'Oscar Wilde', N'Illustrated Editions Co.', N'DEMO-FI02', 14314858, N'kinh điển, đạo đức, nghệ thuật'),
    (N'9781250237170', N'The Silent Patient', N'Tiểu thuyết tâm lý bí ẩn xoay quanh một nữ họa sĩ im lặng sau biến cố.', 2018, 352, 169000, 12, 8, 16, 0, 402, N'Tiểu thuyết đương đại', N'Alex Michaelides', N'Orion Publishing Group', N'DEMO-FI01', 9407338, N'tâm lý, bí ẩn, điều tra'),
    (N'9780062111234', N'Shatter Me', N'Câu chuyện giả tưởng tuổi trẻ về quyền năng, tự do và khả năng lựa chọn.', 2011, 357, 159000, 9, 5, 13, 0, 244, N'Tiểu thuyết đương đại', N'Tahereh Mafi', N'HarperCollins', N'DEMO-FI01', 6974992, N'giả tưởng, tuổi trẻ, tự do'),
    (N'9798667360360', N'Frankenstein', N'Tác phẩm kinh điển đặt câu hỏi về khoa học, trách nhiệm và bản chất con người.', 1818, 240, 115000, 8, 0, 16, 4, 355, N'Văn học kinh điển', N'Mary Shelley', N'Classic Comic Store', N'DEMO-FI02', 12356249, N'kinh điển, khoa học, trách nhiệm'),
    (N'9781955876162', N'The Cruel Prince', N'Tiểu thuyết kỳ ảo về quyền lực, âm mưu và hành trình tìm vị trí trong thế giới tiên.', 2018, 370, 165000, 10, 6, 13, 0, 218, N'Tiểu thuyết đương đại', N'Holly Black', N'Holly Black Editions', N'DEMO-FI01', 8361789, N'kỳ ảo, quyền lực, tuổi trẻ'),
    (N'9780143774266', N'Diary of a Wimpy Kid', N'Nhật ký hài hước về trường học, gia đình và những rắc rối thường ngày.', 2007, 224, 119000, 15, 12, 9, 0, 289, N'Tiểu thuyết đương đại', N'Jeff Kinney', N'Presvika', N'DEMO-FI01', 14376136, N'hài hước, trường học, thiếu nhi'),
    (N'9788408258360', N'It Ends With Us', N'Tiểu thuyết đương đại về tình yêu, lựa chọn khó khăn và sức mạnh để thay đổi.', 2012, 384, 179000, 11, 7, 16, 0, 376, N'Tiểu thuyết đương đại', N'Colleen Hoover', N'Atria Books', N'DEMO-FI01', 10473609, N'tình cảm, gia đình, lựa chọn'),

    (N'9780321623539', N'Biology', N'Giáo trình tổng quan về tế bào, di truyền, tiến hóa, sinh thái và đa dạng sinh học.', 1987, 1267, 320000, 5, 2, 16, 0, 221, N'Khoa học tự nhiên', N'Neil A. Campbell', N'Addison-Wesley', N'DEMO-SC01', 581911, N'sinh học, di truyền, tiến hóa'),
    (N'9781491514931', N'The Fall of Hyperion', N'Tiếp nối hành trình khoa học viễn tưởng về trí tuệ nhân tạo và số phận nhân loại.', 1990, 561, 185000, 7, 4, 16, 0, 172, N'Khoa học viễn tưởng', N'Dan Simmons', N'Robert Laffont', N'DEMO-SF01', 484900, N'không gian, trí tuệ nhân tạo, sử thi'),
    (N'9781800249158', N'Death''s End', N'Phần kết của bộ Ba Vật Thể, mở rộng xung đột từ Trái Đất đến quy mô vũ trụ.', 2010, 724, 219000, 8, 5, 16, 0, 264, N'Khoa học viễn tưởng', N'Lưu Từ Hân', N'Head of Zeus', N'DEMO-SF01', 7893958, N'vũ trụ, vật lý, văn minh ngoài Trái Đất'),
    (N'9780671458423', N'Roadside Picnic', N'Tiểu thuyết khoa học viễn tưởng về vùng cấm, di vật ngoài hành tinh và lòng tham.', 1978, 170, 135000, 6, 3, 16, 0, 143, N'Khoa học viễn tưởng', N'Arkady & Boris Strugatsky', N'Stalker Editions', N'DEMO-SF01', 6752719, N'vùng cấm, ngoài hành tinh, triết học'),
    (N'9788427014060', N'The Caves of Steel', N'Truyện trinh thám khoa học viễn tưởng về robot và xã hội đô thị tương lai.', 1953, 259, 139000, 9, 6, 14, 0, 205, N'Khoa học viễn tưởng', N'Isaac Asimov', N'Granada', N'DEMO-SF01', 13790511, N'robot, trinh thám, tương lai'),
    (N'9783453318670', N'The Foundation Trilogy', N'Bộ sử thi khoa học viễn tưởng về khoa học dự báo và sự suy tàn của đế chế thiên hà.', 1950, 605, 229000, 8, 4, 14, 0, 419, N'Khoa học viễn tưởng', N'Isaac Asimov', N'Wilhelm Heyne Verlag', N'DEMO-SF01', 6502517, N'đế chế, thiên hà, khoa học dự báo'),
    (N'9788804686521', N'The Dark Forest', N'Phần hai của bộ Ba Vật Thể với chiến lược sinh tồn giữa các nền văn minh.', 2008, 574, 205000, 9, 6, 16, 0, 298, N'Khoa học viễn tưởng', N'Lưu Từ Hân', N'Actes Sud', N'DEMO-SF01', 10526598, N'vũ trụ, chiến lược, văn minh'),
    (N'9789685960434', N'The Naked Sun', N'Vụ án robot trong một xã hội xa lạ, nơi con người tránh tiếp xúc trực tiếp.', 1954, 226, 135000, 7, 4, 14, 0, 156, N'Khoa học viễn tưởng', N'Isaac Asimov', N'MLBD', N'DEMO-SF01', 6542967, N'robot, trinh thám, xã hội tương lai');

INSERT dbo.Authors (Name, Nationality, Biography, IsDeleted, DeletedAt, CreatedAt, UpdatedAt)
SELECT DISTINCT source.AuthorName, NULL,
       N'Tác giả của đầu sách trong bộ dữ liệu trình diễn LibraryHub. Metadata bìa tham chiếu Open Library.',
       0, NULL, @Now, @Now
FROM @Books source
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.Authors target
    WHERE target.Name = source.AuthorName AND target.IsDeleted = 0
);

INSERT dbo.Publishers
    (Name, Address, Phone, Email, Website, IsDeleted, DeletedAt, CreatedAt, UpdatedAt)
SELECT DISTINCT source.PublisherName, NULL, NULL, NULL, NULL, 0, NULL, @Now, @Now
FROM @Books source
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.Publishers target
    WHERE target.Name = source.PublisherName AND target.IsDeleted = 0
);

INSERT dbo.Books
(
    Isbn, Title, Description, Edition, Keywords, PublicationYear, CreatedBy,
    PageCount, Language, LastBorrowedAt, Price, TotalQuantity, AvailableQuantity,
    MinimumAge, CoverImagePath, Status, ViewCount, CategoryId, AuthorId,
    PublisherId, ShelfId, IsDeleted, DeletedAt, CreatedAt, UpdatedAt
)
SELECT source.Isbn, source.Title, source.Description, N'Bản dữ liệu trình diễn', source.Keywords,
       source.PublicationYear, N'LibraryHub rich demo seed', source.PageCount, N'Tiếng Anh',
       CASE WHEN source.ViewCount % 3 = 0 THEN DATEADD(day, -(source.ViewCount % 40), @Now) ELSE NULL END,
       source.Price, source.TotalQuantity, source.AvailableQuantity, source.MinimumAge,
       CONCAT(N'https://covers.openlibrary.org/b/id/', source.CoverId, N'-L.jpg?default=false'),
       source.Status, source.ViewCount, category.Id, author.Id, publisher.Id, shelf.Id,
       0, NULL, @Now, @Now
FROM @Books source
JOIN dbo.Categories category ON category.Name = source.CategoryName AND category.IsDeleted = 0
JOIN dbo.Authors author ON author.Name = source.AuthorName AND author.IsDeleted = 0
JOIN dbo.Publishers publisher ON publisher.Name = source.PublisherName AND publisher.IsDeleted = 0
JOIN dbo.Shelves shelf ON shelf.Code = source.ShelfCode AND shelf.IsDeleted = 0
WHERE NOT EXISTS (SELECT 1 FROM dbo.Books target WHERE target.Isbn = source.Isbn);

/* -------------------------------------------------------------------------- */
/* 3. Dữ liệu tương tác và lưu thông cho tài khoản integration hiện có        */
/* -------------------------------------------------------------------------- */

DECLARE @ReaderAccountId uniqueidentifier =
    (SELECT TOP (1) Id FROM dbo.AspNetUsers WHERE Email = N'integration.reader@libraryhub.local');
DECLARE @StaffAccountId uniqueidentifier =
    (SELECT TOP (1) Id FROM dbo.AspNetUsers WHERE Email = N'integration.staff@libraryhub.local');
DECLARE @AdminAccountId uniqueidentifier =
    (SELECT TOP (1) Id FROM dbo.AspNetUsers WHERE Email = N'integration.admin@libraryhub.local');
DECLARE @ReaderId int =
    (SELECT TOP (1) Id FROM dbo.Readers WHERE UserAccountId = @ReaderAccountId AND IsDeleted = 0);

IF @ReaderId IS NOT NULL
BEGIN
    UPDATE dbo.Readers
    SET AvatarPath = CASE WHEN NULLIF(AvatarPath, N'') IS NULL
        THEN N'https://api.dicebear.com/9.x/initials/svg?seed=Integration%20Reader&backgroundColor=2f6f5e&fontFamily=Arial'
        ELSE AvatarPath END,
        Bio = CASE WHEN NULLIF(Bio, N'') IS NULL
        THEN N'Độc giả mẫu yêu thích công nghệ, văn học và những cuốn sách giúp phát triển bản thân.'
        ELSE Bio END,
        ActiveBadge = CASE WHEN NULLIF(ActiveBadge, N'') IS NULL THEN N'Bạn đọc tích cực' ELSE ActiveBadge END,
        UpdatedAt = @Now
    WHERE Id = @ReaderId;

    DECLARE @InteractionBooks TABLE (DisplayOrder int, Isbn nvarchar(30));
    INSERT @InteractionBooks VALUES
        (1, N'9798545118748'),
        (2, N'9780596158064'),
        (3, N'9781250237170'),
        (4, N'9783453318670');

    INSERT dbo.ReaderFeaturedBooks (ReaderId, BookId, DisplayOrder, CreatedAt, UpdatedAt)
    SELECT @ReaderId, book.Id, source.DisplayOrder, @Now, @Now
    FROM @InteractionBooks source
    JOIN dbo.Books book ON book.Isbn = source.Isbn
    WHERE NOT EXISTS
    (
        SELECT 1 FROM dbo.ReaderFeaturedBooks target
        WHERE target.ReaderId = @ReaderId
          AND (target.BookId = book.Id OR target.DisplayOrder = source.DisplayOrder)
    );

    INSERT dbo.BookFavorites (BookId, ReaderId, CreatedAt, UpdatedAt)
    SELECT book.Id, @ReaderId, DATEADD(day, -source.DisplayOrder, @Now), @Now
    FROM @InteractionBooks source
    JOIN dbo.Books book ON book.Isbn = source.Isbn
    WHERE NOT EXISTS
    (
        SELECT 1 FROM dbo.BookFavorites target
        WHERE target.BookId = book.Id AND target.ReaderId = @ReaderId
    );

    DECLARE @Ratings TABLE (Isbn nvarchar(30), Stars int, Review nvarchar(1000));
    INSERT @Ratings VALUES
        (N'9798545118748', 5, N'Cách trình bày rõ ràng, nhiều ví dụ nhỏ có thể áp dụng ngay.'),
        (N'9780596158064', 4, N'Nội dung dày nhưng rất hữu ích khi cần tra cứu Python có hệ thống.'),
        (N'9781250237170', 4, N'Nhịp truyện cuốn hút và phần kết tạo nhiều điều để thảo luận.'),
        (N'9783453318670', 5, N'Một thế giới khoa học viễn tưởng rộng lớn với nhiều ý tưởng đáng nhớ.');

    INSERT dbo.BookRatings (BookId, ReaderId, Stars, Review, CreatedAt, UpdatedAt)
    SELECT book.Id, @ReaderId, source.Stars, source.Review, DATEADD(day, -10, @Now), @Now
    FROM @Ratings source
    JOIN dbo.Books book ON book.Isbn = source.Isbn
    WHERE NOT EXISTS
    (
        SELECT 1 FROM dbo.BookRatings target
        WHERE target.BookId = book.Id AND target.ReaderId = @ReaderId
    );

    DECLARE @Comments TABLE (Isbn nvarchar(30), Content nvarchar(1000));
    INSERT @Comments VALUES
        (N'9798545118748', N'[DEMO] Mình thích cách tác giả biến mục tiêu lớn thành các hành động rất nhỏ.'),
        (N'9780735605053', N'[DEMO] Cuốn này giải thích nền tảng máy tính dễ hiểu dù chủ đề khá kỹ thuật.'),
        (N'9786053243113', N'[DEMO] Một tác phẩm kinh điển đáng đọc lại để nhìn rõ hơn về lựa chọn và trách nhiệm.');

    INSERT dbo.BookComments (BookId, ReaderId, ParentCommentId, Content, IsDeleted, DeletedAt, CreatedAt, UpdatedAt)
    SELECT book.Id, @ReaderId, NULL, source.Content, 0, NULL, DATEADD(day, -7, @Now), @Now
    FROM @Comments source
    JOIN dbo.Books book ON book.Isbn = source.Isbn
    WHERE NOT EXISTS
    (
        SELECT 1 FROM dbo.BookComments target
        WHERE target.ReaderId = @ReaderId AND target.BookId = book.Id AND target.Content = source.Content
    );

    DECLARE @Reading TABLE (Isbn nvarchar(30), CurrentPage int, TotalPages int, DaysAgo int);
    INSERT @Reading VALUES
        (N'9780596158064', 186, 591, 2),
        (N'9783453318670', 248, 605, 1);

    INSERT dbo.ReadingHistories
        (BookId, ReaderId, CurrentPage, TotalPages, LastReadAt, StartedAt, CreatedAt, UpdatedAt)
    SELECT book.Id, @ReaderId, source.CurrentPage, source.TotalPages,
           DATEADD(day, -source.DaysAgo, @Now), DATEADD(day, -21, @Now), DATEADD(day, -21, @Now), @Now
    FROM @Reading source
    JOIN dbo.Books book ON book.Isbn = source.Isbn
    WHERE NOT EXISTS
    (
        SELECT 1 FROM dbo.ReadingHistories target
        WHERE target.BookId = book.Id AND target.ReaderId = @ReaderId
    );

    DECLARE @Views TABLE (Isbn nvarchar(30), SessionId nvarchar(100), DaysAgo int);
    INSERT @Views VALUES
        (N'9798545118748', N'DEMO-VIEW-001', 12),
        (N'9780596158064', N'DEMO-VIEW-002', 9),
        (N'9781250237170', N'DEMO-VIEW-003', 7),
        (N'9783453318670', N'DEMO-VIEW-004', 5),
        (N'9786053243113', N'DEMO-VIEW-005', 3),
        (N'9780321623539', N'DEMO-VIEW-006', 1);

    INSERT dbo.BookViewHistories (BookId, ReaderId, SessionId, ViewedAt, CreatedAt, UpdatedAt)
    SELECT book.Id, @ReaderId, source.SessionId, DATEADD(day, -source.DaysAgo, @Now),
           DATEADD(day, -source.DaysAgo, @Now), @Now
    FROM @Views source
    JOIN dbo.Books book ON book.Isbn = source.Isbn
    WHERE NOT EXISTS (SELECT 1 FROM dbo.BookViewHistories target WHERE target.SessionId = source.SessionId);

    /* Phiếu đã trả */
    IF NOT EXISTS (SELECT 1 FROM dbo.BorrowSlips WHERE Code = N'DEMO-BR-001')
        INSERT dbo.BorrowSlips
            (Code, ReaderId, TotalBooks, RequestedAt, BorrowedAt, DueAt, ReturnedAt,
             PickupExpiresAt, Note, CreatedByAccountId, Status, IdempotencyKey, CreatedAt, UpdatedAt)
        VALUES
            (N'DEMO-BR-001', @ReaderId, 1, DATEADD(day,-62,@Now), DATEADD(day,-60,@Now),
             DATEADD(day,-46,@Now), DATEADD(day,-45,@Now), NULL, N'Phiếu mẫu đã hoàn tất.',
             @ReaderAccountId, 6, N'DEMO-BORROW-KEY-001', DATEADD(day,-62,@Now), @Now);

    /* Phiếu đang mượn */
    IF NOT EXISTS (SELECT 1 FROM dbo.BorrowSlips WHERE Code = N'DEMO-BR-002')
        INSERT dbo.BorrowSlips
            (Code, ReaderId, TotalBooks, RequestedAt, BorrowedAt, DueAt, ReturnedAt,
             PickupExpiresAt, Note, CreatedByAccountId, Status, IdempotencyKey, CreatedAt, UpdatedAt)
        VALUES
            (N'DEMO-BR-002', @ReaderId, 1, DATEADD(day,-7,@Now), DATEADD(day,-5,@Now),
             DATEADD(day,9,@Now), NULL, NULL, N'Phiếu mẫu đang mượn đúng hạn.',
             @ReaderAccountId, 2, N'DEMO-BORROW-KEY-002', DATEADD(day,-7,@Now), @Now);

    /* Phiếu quá hạn */
    IF NOT EXISTS (SELECT 1 FROM dbo.BorrowSlips WHERE Code = N'DEMO-BR-003')
        INSERT dbo.BorrowSlips
            (Code, ReaderId, TotalBooks, RequestedAt, BorrowedAt, DueAt, ReturnedAt,
             PickupExpiresAt, Note, CreatedByAccountId, Status, IdempotencyKey, CreatedAt, UpdatedAt)
        VALUES
            (N'DEMO-BR-003', @ReaderId, 1, DATEADD(day,-27,@Now), DATEADD(day,-25,@Now),
             DATEADD(day,-11,@Now), NULL, NULL, N'Phiếu mẫu quá hạn để trình diễn cảnh báo.',
             @ReaderAccountId, 4, N'DEMO-BORROW-KEY-003', DATEADD(day,-27,@Now), @Now);

    /* Phiếu chờ duyệt hai sách */
    IF NOT EXISTS (SELECT 1 FROM dbo.BorrowSlips WHERE Code = N'DEMO-BR-004')
        INSERT dbo.BorrowSlips
            (Code, ReaderId, TotalBooks, RequestedAt, BorrowedAt, DueAt, ReturnedAt,
             PickupExpiresAt, Note, CreatedByAccountId, Status, IdempotencyKey, CreatedAt, UpdatedAt)
        VALUES
            (N'DEMO-BR-004', @ReaderId, 2, DATEADD(hour,-18,@Now), NULL, NULL, NULL,
             NULL, N'Phiếu mẫu đang chờ nhân viên duyệt.', @ReaderAccountId, 0,
             N'DEMO-BORROW-KEY-004', DATEADD(hour,-18,@Now), @Now);

    /* Phiếu đã duyệt, chờ nhận */
    IF NOT EXISTS (SELECT 1 FROM dbo.BorrowSlips WHERE Code = N'DEMO-BR-005')
        INSERT dbo.BorrowSlips
            (Code, ReaderId, TotalBooks, RequestedAt, BorrowedAt, DueAt, ReturnedAt,
             PickupExpiresAt, Note, CreatedByAccountId, Status, IdempotencyKey, CreatedAt, UpdatedAt)
        VALUES
            (N'DEMO-BR-005', @ReaderId, 1, DATEADD(day,-1,@Now), NULL, NULL, NULL,
             DATEADD(day,2,@Now), N'Phiếu mẫu đã duyệt và đang chờ nhận.', @ReaderAccountId, 1,
             N'DEMO-BORROW-KEY-005', DATEADD(day,-1,@Now), @Now);

    DECLARE @BorrowDetails TABLE
    (
        SlipCode nvarchar(50), Isbn nvarchar(30), Quantity int, ReturnedQuantity int,
        ConditionBefore nvarchar(300), Note nvarchar(500), DueOffset int
    );
    INSERT @BorrowDetails VALUES
        (N'DEMO-BR-001', N'9798545118748', 1, 1, N'Nguyên vẹn', N'Đã trả đầy đủ.', -46),
        (N'DEMO-BR-002', N'9780596158064', 1, 0, N'Nguyên vẹn', N'Đang đọc.', 9),
        (N'DEMO-BR-003', N'9781250237170', 1, 0, N'Nguyên vẹn', N'Đang quá hạn.', -11),
        (N'DEMO-BR-004', N'9780735605053', 1, 0, N'Chờ giao sách', NULL, 14),
        (N'DEMO-BR-004', N'9786053243113', 1, 0, N'Chờ giao sách', NULL, 14),
        (N'DEMO-BR-005', N'9783453318670', 1, 0, N'Chờ nhận sách', NULL, 16);

    INSERT dbo.BorrowDetails
        (BorrowSlipId, BookId, Quantity, ReturnedQuantity, BorrowPrice, ConditionBefore,
         Note, ExtensionCount, DueAt, RequestedExtensionDays, ExtensionReason, CreatedAt, UpdatedAt)
    SELECT slip.Id, book.Id, source.Quantity, source.ReturnedQuantity, book.Price,
           source.ConditionBefore, source.Note, 0, DATEADD(day, source.DueOffset, @Now),
           NULL, NULL, slip.CreatedAt, @Now
    FROM @BorrowDetails source
    JOIN dbo.BorrowSlips slip ON slip.Code = source.SlipCode
    JOIN dbo.Books book ON book.Isbn = source.Isbn
    WHERE NOT EXISTS
    (
        SELECT 1 FROM dbo.BorrowDetails target
        WHERE target.BorrowSlipId = slip.Id AND target.BookId = book.Id
    );

    DECLARE @ReturnedDetailId int =
    (
        SELECT TOP (1) detail.Id
        FROM dbo.BorrowDetails detail
        JOIN dbo.BorrowSlips slip ON slip.Id = detail.BorrowSlipId
        WHERE slip.Code = N'DEMO-BR-001'
    );
    DECLARE @ReturnedSlipId int = (SELECT Id FROM dbo.BorrowSlips WHERE Code = N'DEMO-BR-001');

    IF @ReturnedDetailId IS NOT NULL
       AND @StaffAccountId IS NOT NULL
       AND NOT EXISTS
       (
           SELECT 1 FROM dbo.BorrowReturns
           WHERE BorrowDetailId = @ReturnedDetailId AND IdempotencyKey = N'DEMO-RETURN-KEY-001'
       )
        INSERT dbo.BorrowReturns
            (BorrowDetailId, Quantity, Condition, DamagePercent, ConditionNote,
             ProcessedByAccountId, IdempotencyKey, ResultSlipId, ResultStatus,
             FineId, FineAmount, CreatedAt, UpdatedAt)
        VALUES
            (@ReturnedDetailId, 1, 0, 0, N'Trả sách nguyên vẹn.', @StaffAccountId,
             N'DEMO-RETURN-KEY-001', @ReturnedSlipId, 6, NULL, 0,
             DATEADD(day,-45,@Now), @Now);

    IF NOT EXISTS (SELECT 1 FROM dbo.Fines WHERE Code = N'DEMO-FN-001')
    BEGIN
        INSERT dbo.Fines
            (Code, BorrowSlipId, ReaderId, Type, Amount, PaidAmount, Reason, Status,
             TransactionCode, PaidAt, CreatedAt, UpdatedAt)
        SELECT N'DEMO-FN-001', slip.Id, @ReaderId, 0, 55000, 0,
               N'Khoản phạt mẫu do trả trễ 11 ngày.', 0, NULL, NULL,
               DATEADD(day,-1,@Now), @Now
        FROM dbo.BorrowSlips slip WHERE slip.Code = N'DEMO-BR-003';
    END;

    IF NOT EXISTS (SELECT 1 FROM dbo.Fines WHERE Code = N'DEMO-FN-002')
    BEGIN
        INSERT dbo.Fines
            (Code, BorrowSlipId, ReaderId, Type, Amount, PaidAmount, Reason, Status,
             TransactionCode, PaidAt, CreatedAt, UpdatedAt)
        SELECT N'DEMO-FN-002', slip.Id, @ReaderId, 2, 15000, 15000,
               N'Khoản phạt mẫu đã thanh toán do bìa sách bị xước nhẹ.', 3,
               N'DEMO-TXN-001', DATEADD(day,-44,@Now), DATEADD(day,-45,@Now), @Now
        FROM dbo.BorrowSlips slip WHERE slip.Code = N'DEMO-BR-001';
    END;

    INSERT dbo.FineDetails
        (FineId, BookId, BorrowDetailId, Type, Amount, Reason, DamagePercent,
         BookPrice, LateDays, CreatedAt, UpdatedAt)
    SELECT fine.Id, book.Id, detail.Id, 0, 55000, N'Quá hạn 11 ngày.', NULL,
           book.Price, 11, fine.CreatedAt, @Now
    FROM dbo.Fines fine
    JOIN dbo.BorrowSlips slip ON slip.Id = fine.BorrowSlipId AND slip.Code = N'DEMO-BR-003'
    JOIN dbo.BorrowDetails detail ON detail.BorrowSlipId = slip.Id
    JOIN dbo.Books book ON book.Id = detail.BookId
    WHERE fine.Code = N'DEMO-FN-001'
      AND NOT EXISTS (SELECT 1 FROM dbo.FineDetails target WHERE target.FineId = fine.Id);

    INSERT dbo.FineDetails
        (FineId, BookId, BorrowDetailId, Type, Amount, Reason, DamagePercent,
         BookPrice, LateDays, CreatedAt, UpdatedAt)
    SELECT fine.Id, book.Id, detail.Id, 2, 15000, N'Bìa sách bị xước nhẹ.', 5,
           book.Price, 0, fine.CreatedAt, @Now
    FROM dbo.Fines fine
    JOIN dbo.BorrowSlips slip ON slip.Id = fine.BorrowSlipId AND slip.Code = N'DEMO-BR-001'
    JOIN dbo.BorrowDetails detail ON detail.BorrowSlipId = slip.Id
    JOIN dbo.Books book ON book.Id = detail.BookId
    WHERE fine.Code = N'DEMO-FN-002'
      AND NOT EXISTS (SELECT 1 FROM dbo.FineDetails target WHERE target.FineId = fine.Id);

    DECLARE @Reservations TABLE (Isbn nvarchar(30), Status int, DaysAgo int, ExpiresIn int, Note nvarchar(500));
    INSERT @Reservations VALUES
        (N'9783453318670', 0, 2, 5, N'[DEMO] Đang chờ xử lý.'),
        (N'9780143774266', 5, 3, 3, N'[DEMO] Đã được nhân viên duyệt.'),
        (N'9780735605053', 2, 20, -12, N'[DEMO] Đặt trước đã hoàn tất.');

    INSERT dbo.Reservations
        (ReaderId, BookId, ReservedAt, ExpiresAt, Status, Note, CreatedAt, UpdatedAt)
    SELECT @ReaderId, book.Id, DATEADD(day,-source.DaysAgo,@Now),
           DATEADD(day,source.ExpiresIn,@Now), source.Status, source.Note,
           DATEADD(day,-source.DaysAgo,@Now), @Now
    FROM @Reservations source
    JOIN dbo.Books book ON book.Isbn = source.Isbn
    WHERE NOT EXISTS
    (
        SELECT 1 FROM dbo.Reservations target
        WHERE target.ReaderId = @ReaderId AND target.BookId = book.Id AND target.Note = source.Note
    );

    IF NOT EXISTS (SELECT 1 FROM dbo.SupportTickets WHERE Code = N'DEMO-TK-001')
        INSERT dbo.SupportTickets
            (Code, ReaderId, AssignedStaffAccountId, Subject, Status, LastMessageAt,
             UnreadByReader, UnreadByStaff, ClosedAt, CreatedAt, UpdatedAt)
        VALUES
            (N'DEMO-TK-001', @ReaderId, @StaffAccountId,
             N'Hỏi về điều kiện gia hạn sách', 1, DATEADD(hour,-4,@Now),
             1, 0, NULL, DATEADD(day,-2,@Now), @Now);

    IF NOT EXISTS (SELECT 1 FROM dbo.SupportTickets WHERE Code = N'DEMO-TK-002')
        INSERT dbo.SupportTickets
            (Code, ReaderId, AssignedStaffAccountId, Subject, Status, LastMessageAt,
             UnreadByReader, UnreadByStaff, ClosedAt, CreatedAt, UpdatedAt)
        VALUES
            (N'DEMO-TK-002', @ReaderId, @StaffAccountId,
             N'Cập nhật thông tin liên hệ', 2, DATEADD(day,-6,@Now),
             0, 0, DATEADD(day,-6,@Now), DATEADD(day,-8,@Now), @Now);

    DECLARE @SupportMessages TABLE
    (
        TicketCode nvarchar(50), SenderType int, SenderAccountId uniqueidentifier,
        Content nvarchar(1500), HoursAgo int
    );
    INSERT @SupportMessages VALUES
        (N'DEMO-TK-001', 0, @ReaderAccountId, N'[DEMO] Tôi có thể gia hạn khi sách đang có người đặt trước không?', 30),
        (N'DEMO-TK-001', 1, @StaffAccountId, N'[DEMO] Nếu đã có lượt đặt trước tiếp theo, hệ thống sẽ không cho gia hạn. Bạn có thể kiểm tra trực tiếp trên phiếu mượn.', 4),
        (N'DEMO-TK-002', 0, @ReaderAccountId, N'[DEMO] Tôi muốn cập nhật số điện thoại trong hồ sơ.', 190),
        (N'DEMO-TK-002', 1, @StaffAccountId, N'[DEMO] Thông tin đã được hướng dẫn cập nhật tại mục Hồ sơ độc giả.', 150);

    INSERT dbo.SupportMessages
        (SupportTicketId, SenderType, SenderAccountId, Content, CreatedAt, UpdatedAt)
    SELECT ticket.Id, source.SenderType, source.SenderAccountId, source.Content,
           DATEADD(hour,-source.HoursAgo,@Now), DATEADD(hour,-source.HoursAgo,@Now)
    FROM @SupportMessages source
    JOIN dbo.SupportTickets ticket ON ticket.Code = source.TicketCode
    WHERE NOT EXISTS
    (
        SELECT 1 FROM dbo.SupportMessages target
        WHERE target.SupportTicketId = ticket.Id AND target.Content = source.Content
    );

    DECLARE @Notifications TABLE
    (
        Type int, Title nvarchar(300), Message nvarchar(1000), Link nvarchar(500),
        IsRead bit, DaysAgo int
    );
    INSERT @Notifications VALUES
        (2, N'Chào mừng đến LibraryHub', N'Khám phá danh mục mới và lưu sách bạn yêu thích.', N'/#catalog', 1, 30),
        (6, N'Phiếu DEMO-BR-005 đã được duyệt', N'Sách đang chờ bạn đến nhận trong thời hạn quy định.', N'/me/borrows', 0, 1),
        (12, N'Nhắc hạn trả sách', N'Phiếu DEMO-BR-002 còn 9 ngày trước hạn trả.', N'/me/borrows', 0, 0),
        (13, N'Phiếu DEMO-BR-003 đang quá hạn', N'Vui lòng trả sách hoặc liên hệ thư viện để được hỗ trợ.', N'/me/borrows', 0, 1),
        (25, N'Phát sinh khoản phạt DEMO-FN-001', N'Khoản phạt mẫu 55.000đ đang chờ thanh toán.', N'/me/fines', 0, 1),
        (33, N'Yêu cầu hỗ trợ đã được tiếp nhận', N'Nhân viên đang xử lý câu hỏi về gia hạn sách.', N'/support', 0, 2),
        (1, N'Danh mục trình diễn đã được cập nhật', N'LibraryHub vừa bổ sung nhiều sách công nghệ, văn học và phát triển bản thân.', N'/#catalog', 1, 3);

    INSERT dbo.Notifications
        (RecipientAccountId, Type, Title, Message, Link, IsRead, ReadAt,
         CreatedByAccountId, BroadcastGroupId, CreatedAt, UpdatedAt)
    SELECT @ReaderAccountId, source.Type, source.Title, source.Message, source.Link,
           source.IsRead, CASE WHEN source.IsRead = 1 THEN DATEADD(day,-source.DaysAgo,@Now) ELSE NULL END,
           @AdminAccountId, NULL, DATEADD(day,-source.DaysAgo,@Now), @Now
    FROM @Notifications source
    WHERE NOT EXISTS
    (
        SELECT 1 FROM dbo.Notifications target
        WHERE target.RecipientAccountId = @ReaderAccountId AND target.Title = source.Title
    );
END
ELSE
BEGIN
    PRINT N'Không tìm thấy hồ sơ integration.reader; đã bỏ qua dữ liệu mượn/đặt trước/tương tác.';
END;

COMMIT TRANSACTION;

SELECT
    (SELECT COUNT(*) FROM dbo.Books b JOIN @Books source ON source.Isbn = b.Isbn) AS RichDemoBooks,
    (SELECT COUNT(*) FROM dbo.Categories c JOIN @Categories source ON source.Name = c.Name WHERE c.IsDeleted = 0) AS RichDemoCategories,
    (SELECT COUNT(*) FROM dbo.BorrowSlips WHERE Code LIKE N'DEMO-BR-%') AS DemoBorrowSlips,
    (SELECT COUNT(*) FROM dbo.Reservations WHERE LEFT(Note, 6) = N'[DEMO]') AS DemoReservations,
    (SELECT COUNT(*) FROM dbo.SupportTickets WHERE Code LIKE N'DEMO-TK-%') AS DemoTickets;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
