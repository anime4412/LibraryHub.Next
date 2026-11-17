/*
  Optional LocalDB demo-content normalization.
  Targets only the deterministic demo identities/codes created for LibraryHubNext.
  The script is idempotent and does not touch unrelated user data.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = 'reader.test@libraryhub.local')
   AND NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = 'reader.demo@libraryhub.local')
BEGIN
    UPDATE AspNetUsers
    SET Email = 'reader.demo@libraryhub.local',
        NormalizedEmail = 'READER.DEMO@LIBRARYHUB.LOCAL',
        UserName = 'reader.demo@libraryhub.local',
        NormalizedUserName = 'READER.DEMO@LIBRARYHUB.LOCAL',
        DisplayName = N'Nguyễn Minh Anh'
    WHERE Email = 'reader.test@libraryhub.local';
END;

UPDATE AspNetUsers
SET DisplayName = N'Nguyễn Minh Anh'
WHERE Email = 'reader.demo@libraryhub.local';

UPDATE Readers
SET ReaderCode = N'DG-2026-001',
    FullName = N'Nguyễn Minh Anh',
    Email = 'reader.demo@libraryhub.local',
    Bio = N'Yêu những trang sách giàu cảm hứng và luôn sẵn sàng khám phá tri thức mới.',
    UpdatedAt = SYSDATETIMEOFFSET()
WHERE Email IN ('reader.test@libraryhub.local', 'reader.demo@libraryhub.local');

IF EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = 'admin.test@libraryhub.local')
   AND NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = 'admin.demo@libraryhub.local')
BEGIN
    UPDATE AspNetUsers
    SET Email = 'admin.demo@libraryhub.local',
        NormalizedEmail = 'ADMIN.DEMO@LIBRARYHUB.LOCAL',
        UserName = 'admin.demo@libraryhub.local',
        NormalizedUserName = 'ADMIN.DEMO@LIBRARYHUB.LOCAL',
        DisplayName = N'Quản trị viên LibraryHub'
    WHERE Email = 'admin.test@libraryhub.local';
END;

UPDATE AspNetUsers
SET DisplayName = N'Quản trị viên LibraryHub'
WHERE Email = 'admin.demo@libraryhub.local';

IF EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = 'staff.phase1@libraryhub.local')
   AND NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = 'staff.demo@libraryhub.local')
BEGIN
    UPDATE AspNetUsers
    SET Email = 'staff.demo@libraryhub.local',
        NormalizedEmail = 'STAFF.DEMO@LIBRARYHUB.LOCAL',
        UserName = 'staff.demo@libraryhub.local',
        NormalizedUserName = 'STAFF.DEMO@LIBRARYHUB.LOCAL',
        DisplayName = N'Trần Hoàng Nam'
    WHERE Email = 'staff.phase1@libraryhub.local';
END;

UPDATE AspNetUsers
SET DisplayName = N'Trần Hoàng Nam'
WHERE Email = 'staff.demo@libraryhub.local';

UPDATE Employees
SET EmployeeCode = N'NV-2026-001',
    FullName = N'Trần Hoàng Nam',
    Email = 'staff.demo@libraryhub.local',
    UpdatedAt = SYSDATETIMEOFFSET()
WHERE EmployeeCode IN (N'NV-AUDIT', N'NV-2026-001');

UPDATE SecurityRoles
SET Code = N'LIBRARY_STAFF',
    Name = N'Nhân viên thư viện',
    Description = N'Thực hiện nghiệp vụ sách, mượn trả, độc giả và hỗ trợ.',
    UpdatedAt = SYSDATETIMEOFFSET()
WHERE Code IN (N'PHASE1_STAFF', N'LIBRARY_STAFF');

UPDATE position
SET position.Code = N'LIBRARIAN',
    position.Name = N'Thủ thư',
    position.Description = N'Phụ trách nghiệp vụ thư viện và hỗ trợ độc giả.',
    position.UpdatedAt = SYSDATETIMEOFFSET()
FROM Positions position
JOIN Employees employee ON employee.PositionId = position.Id
WHERE employee.EmployeeCode = N'NV-2026-001';

UPDATE category
SET category.Name = N'Văn hóa đọc',
    category.Description = N'Sách về văn hóa đọc, gia đình và đời sống tinh thần.',
    category.UpdatedAt = SYSDATETIMEOFFSET()
FROM Categories category
JOIN Books book ON book.CategoryId = category.Id
WHERE book.Isbn = N'9780000000999';

UPDATE author
SET author.Name = N'Nguyễn Khánh Linh',
    author.Nationality = N'Việt Nam',
    author.Biography = N'Tác giả viết về văn hóa đọc và ký ức gia đình.',
    author.UpdatedAt = SYSDATETIMEOFFSET()
FROM Authors author
JOIN Books book ON book.AuthorId = author.Id
WHERE book.Isbn = N'9780000000999';

UPDATE publisher
SET publisher.Name = N'Nhà xuất bản Văn hóa và Tri thức',
    publisher.Address = N'Thành phố Hồ Chí Minh',
    publisher.UpdatedAt = SYSDATETIMEOFFSET()
FROM Publishers publisher
JOIN Books book ON book.PublisherId = publisher.Id
WHERE book.Isbn = N'9780000000999';

UPDATE shelf
SET shelf.Code = N'VH-A01',
    shelf.LocationCode = N'T1-VH-A01',
    shelf.Name = N'Văn hóa đọc',
    shelf.Area = N'Tầng 1',
    shelf.Description = N'Kệ sách văn hóa đọc và đời sống.',
    shelf.UpdatedAt = SYSDATETIMEOFFSET()
FROM Shelves shelf
JOIN Books book ON book.ShelfId = shelf.Id
WHERE book.Isbn = N'9780000000999';

UPDATE Books
SET Title = N'Nếp Nhà Trong Trang Sách',
    Description = N'Những câu chuyện ấm áp về gia đình, ký ức và thói quen đọc được truyền qua nhiều thế hệ.',
    Edition = N'Bản tiêu chuẩn',
    Keywords = N'văn hóa đọc, gia đình, ký ức',
    CreatedBy = N'Demo LibraryHub',
    UpdatedAt = SYSDATETIMEOFFSET()
WHERE Isbn = N'9780000000999';

UPDATE BookComments
SET Content = N'Một cuốn sách nhẹ nhàng, gần gũi và rất đáng đọc.',
    UpdatedAt = SYSDATETIMEOFFSET()
WHERE BookId = (SELECT TOP (1) Id FROM Books WHERE Isbn = N'9780000000999')
  AND Content = N'Audit comment';

UPDATE BookRatings
SET Review = N'Nội dung sâu sắc, cách kể chuyện tự nhiên.'
WHERE BookId = (SELECT TOP (1) Id FROM Books WHERE Isbn = N'9780000000999')
  AND Review = N'Audit review';

UPDATE BorrowSlips
SET Code = N'BR-DEMO-2026-001',
    Note = N'Phiếu mượn mẫu đang được theo dõi.'
WHERE Code IN (N'BR-AUDIT-MANUAL', N'BR-DEMO-2026-001');

UPDATE BorrowSlips
SET Note = N'Độc giả đăng ký mượn sách tại thư viện.'
WHERE Note = N'Phase 1 integration';

UPDATE Reservations
SET Note = N'Độc giả muốn nhận sách khi có bản trống.'
WHERE Note IN (N'Audit reservation', N'Phase 1 reservation');

UPDATE Fines
SET Code = N'FN-DEMO-2026-001',
    Reason = N'Khoản phạt mẫu đã được thanh toán đầy đủ.'
WHERE Code IN (N'FN-AUDIT-MANUAL', N'FN-DEMO-2026-001');

DECLARE @DemoTicketId int =
(
    SELECT TOP (1) ticket.Id
    FROM SupportTickets ticket
    JOIN Readers reader ON reader.Id = ticket.ReaderId
    WHERE ticket.Subject IN (N'Audit support', N'Hỗ trợ gia hạn thẻ')
      AND reader.Email = 'reader.demo@libraryhub.local'
    ORDER BY ticket.Id
);

IF @DemoTicketId IS NOT NULL
BEGIN
    UPDATE SupportTickets
    SET Subject = N'Hỗ trợ gia hạn thẻ'
    WHERE Id = @DemoTicketId;

    ;WITH OrderedMessages AS
    (
        SELECT Id, ROW_NUMBER() OVER (ORDER BY Id) AS RowNumber
        FROM SupportMessages
        WHERE SupportTicketId = @DemoTicketId
    )
    UPDATE message
    SET Content = CASE ordered.RowNumber
        WHEN 1 THEN N'Tôi cần hỗ trợ về thời hạn thẻ thư viện.'
        WHEN 2 THEN N'Tôi đã bổ sung số điện thoại và địa chỉ.'
        WHEN 3 THEN N'Nhân viên đã tiếp nhận và đang kiểm tra thông tin.'
        WHEN 4 THEN N'Cảm ơn thư viện, tôi sẽ chờ kết quả.'
        ELSE N'Thông tin đã được cập nhật; thẻ của bạn đang hoạt động bình thường.'
    END
    FROM SupportMessages message
    JOIN OrderedMessages ordered ON ordered.Id = message.Id;
END;

COMMIT TRANSACTION;

SELECT Email, DisplayName
FROM AspNetUsers
WHERE Email IN
(
    'reader.demo@libraryhub.local',
    'staff.demo@libraryhub.local',
    'admin.demo@libraryhub.local'
)
ORDER BY Email;
