USE [SOF1022_NHOM1_SD2003_Ver2];
GO

UPDATE Readers SET 
    FullName = N'Trần Minh Khang', 
    Address = N'Tòa nhà FPT Polytechnic, Mỹ Đình, Hà Nội' 
WHERE Email = 'sinhvien01@fpt.edu.vn';

UPDATE Readers SET 
    FullName = N'Nguyễn Quỳnh Như', 
    Address = N'Ngõ 130 Xuân Thủy, Cầu Giấy, Hà Nội' 
WHERE Email = 'sinhvien02@fpt.edu.vn';

UPDATE Employees SET FullName = N'Nguyễn Hoàng Long', Address = N'Số 8 Tôn Thất Thuyết, Nam Từ Liêm, Hà Nội' WHERE Email = 'admin@library.com';
UPDATE Employees SET FullName = N'Trần Thị Mai Phương', Address = N'275 Nguyễn Trãi, Thanh Xuân, Hà Nội' WHERE Email = 'quanly@library.com';
UPDATE Employees SET FullName = N'Lê Minh Cường', Address = N'12 Kim Mã, Ba Đình, Hà Nội' WHERE Email = 'thuthu@library.com';
UPDATE Employees SET FullName = N'Phạm Thu Hà', Address = N'45 Cầu Giấy, Cầu Giấy, Hà Nội' WHERE Email = 'kho@library.com';
UPDATE Employees SET FullName = N'Đặng Văn Hùng', Address = N'88 Láng Hạ, Đống Đa, Hà Nội' WHERE Email = 'ketoan@library.com';
UPDATE Employees SET FullName = N'Vũ Ngọc Lan', Address = N'15 Hai Bà Trưng, Hoàn Kiếm, Hà Nội' WHERE Email = 'cskh@library.com';

UPDATE Books SET Keywords = N'Số Đỏ, Vũ Trọng Phụng, Văn học hiện thực, Miễn phí' WHERE Title = N'Số Đỏ';
UPDATE Books SET Keywords = N'Tắt Đèn, Ngô Tất Tố, Chị Dậu, Văn học hiện thực' WHERE Title = N'Tắt Đèn';
UPDATE Books SET Keywords = N'Chí Phèo, Nam Cao, Thị Nở, Bá Kiến, Văn học kinh điển' WHERE Title = N'Chí Phèo';
UPDATE Books SET Keywords = N'Truyện Kiều, Nguyễn Du, Thúy Kiều, Lục bát, Văn học cổ điển' WHERE Title = N'Truyện Kiều';
UPDATE Books SET Keywords = N'Hoàng Tử Bé, The Little Prince, Saint-Exupéry, Triết lý' WHERE Title = N'Hoàng Tử Bé';
UPDATE Books SET Keywords = N'Ông Già Và Biển Cả, Ernest Hemingway, Nobel Văn học' WHERE Title = N'Ông Già Và Biển Cả';
UPDATE Books SET Keywords = N'Binh Pháp Tôn Tử, Tôn Vũ, Chiến lược, Quân sự, Triết học' WHERE Title = N'Binh Pháp Tôn Tử';
UPDATE Books SET Keywords = N'Chiến Binh Cầu Vồng, Andrea Hirata, Giáo dục, Nghị lực sống' WHERE Title = N'Chiến Binh Cầu Vồng';

UPDATE Positions SET Name = N'Admin' WHERE Id = 1;
UPDATE Positions SET Name = N'Quản lý thư viện' WHERE Id = 2;
UPDATE Positions SET Name = N'Thủ thư' WHERE Id = 3;
UPDATE Positions SET Name = N'Nhân viên kho' WHERE Id = 4;
UPDATE Positions SET Name = N'Kế toán' WHERE Id = 5;
UPDATE Positions SET Name = N'Chăm sóc khách hàng' WHERE Id = 6;
GO
