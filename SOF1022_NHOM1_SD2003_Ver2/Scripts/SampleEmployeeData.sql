INSERT INTO Employees
(EmployeeId, FullName, Email, Phone, Address, CCCD, Gender, DateOfBirth, HireDate,
PositionId, Salary, StatusId, LeaveReason, LeaveStartDate,
UnexcusedDaysCount, LastUnexcusedDate, IsDeleted)
VALUES

('NV001', N'Nguyễn Văn An', 'an.nguyen@library.com', '0912345678',
N'Hà Nội', '001234567890', 1, '1995-05-10', '2020-03-15',
1, 5000000, 1, NULL, NULL, 0, NULL, 0),

('NV002', N'Trần Thị Bình', 'binh.tran@library.com', '0987654321',
N'Hồ Chí Minh', '001234567891', 2, '1993-11-20', '2018-06-01',
2, 5000000, 1, NULL, NULL, 0, NULL, 0),

('NV003', N'Lê Minh Cường', 'cuong.le@library.com', '0901234567',
N'Đà Nẵng', '001234567892', 1, '1990-09-15', '2015-07-10',
3, 10000000, 1, NULL, NULL, 0, NULL, 0),

('NV004', N'Phạm Thu Dung', 'dung.pham@library.com', '0934567890',
N'Hải Phòng', '001234567893', 2, '1996-08-12', '2021-01-15',
1, 5000000, 1, NULL, NULL, 0, NULL, 0),

('NV005', N'Hoàng Văn Đức', 'duc.hoang@library.com', '0978123456',
N'Nam Định', '001234567894', 1, '1988-04-18', '2012-09-01',
4, 15000000, 1, NULL, NULL, 0, NULL, 0),

('NV006', N'Ngô Thị Hạnh', 'hanh.ngo@library.com', '0961234567',
N'Nghệ An', '001234567895', 2, '1997-12-01', '2022-02-10',
2, 5000000, 1, NULL, NULL, 0, NULL, 0),

('NV007', N'Đặng Quốc Huy', 'huy.dang@library.com', '0911223344',
N'Quảng Ninh', '001234567896', 1, '1992-02-25', '2017-03-15',
3, 10000000, 1, NULL, NULL, 0, NULL, 0),

('NV008', N'Bùi Thanh Lan', 'lan.bui@library.com', '0988776655',
N'Hà Nam', '001234567897', 2, '1994-06-30', '2019-08-20',
1, 5000000, 1, NULL, NULL, 0, NULL, 0),

('NV009', N'Phan Gia Long', 'long.phan@library.com', '0944556677',
N'Thanh Hóa', '001234567898', 1, '1991-01-11', '2016-05-12',
2, 5000000, 2,
N'Nghỉ tạm thời vì lý do cá nhân',
'2025-06-01',
2,
'2025-05-28',
0),

('NV010', N'Võ Ngọc Mai', 'mai.vo@library.com', '0922334455',
N'Khánh Hòa', '001234567899', 2, '1998-07-19', '2023-01-03',
1, 5000000, 1, NULL, NULL, 0, NULL, 0),

('NV011', N'Đỗ Văn Nam', 'nam.do@library.com', '0919988776',
N'Hải Dương', '001234567900', 1, '1989-10-05', '2014-02-15',
4, 15000000, 1, NULL, NULL, 0, NULL, 0),

('NV012', N'Lý Thị Oanh', 'oanh.ly@library.com', '0977112233',
N'Bắc Giang', '001234567901', 2, '1995-03-21', '2020-07-06',
2, 5000000, 1, NULL, NULL, 0, NULL, 0),

('NV013', N'Tạ Văn Phúc', 'phuc.ta@library.com', '0933445566',
N'Bình Dương', '001234567902', 1, '1993-09-09', '2018-04-16',
3, 10000000, 1, NULL, NULL, 0, NULL, 0),

('NV014', N'Đinh Thị Quyên', 'quyen.dinh@library.com', '0966554433',
N'Cần Thơ', '001234567903', 2, '1994-02-13', '2019-06-18',
1, 5000000, 1, NULL, NULL, 0, NULL, 0),

('NV015', N'Nguyễn Hải Sơn', 'son.nguyen@library.com', '0909988776',
N'Hà Nội', '001234567904', 1, '1987-05-17', '2011-09-10',
4, 15000000, 1, NULL, NULL, 0, NULL, 0),

('NV016', N'Chu Thị Trang', 'trang.chu@library.com', '0915566778',
N'Bắc Ninh', '001234567905', 2, '1999-04-28', '2024-01-08',
2, 5000000, 1, NULL, NULL, 0, NULL, 0),

('NV017', N'Vũ Minh Tú', 'tu.vu@library.com', '0944221133',
N'Hưng Yên', '001234567906', 1, '1991-11-14', '2016-10-17',
3, 10000000, 2,
N'Đình chỉ công tác',
'2025-03-15',
5,
'2025-03-10',
0),

('NV018', N'Hồ Thị Yến', 'yen.ho@library.com', '0977445566',
N'Đồng Nai', '001234567907', 2, '1996-01-07', '2021-05-20',
1, 5000000, 1, NULL, NULL, 0, NULL, 0),

('NV019', N'Nguyễn Văn Khánh', 'khanh.nguyen@library.com', '0923456789',
N'Vĩnh Phúc', '001234567908', 1, '1986-08-22', '2010-03-01',
4, 15000000, 3,
N'Nghỉ việc theo nguyện vọng',
'2024-12-31',
0,
NULL,
0),

('NV020', N'Trịnh Thu Hà', 'ha.trinh@library.com', '0988123456',
N'Thái Bình', '001234567909', 2, '1997-10-16', '2022-08-01',
2, 5000000, 1, NULL, NULL, 0, NULL, 0);
-- ============================================
-- KIỂM TRA DỮ LIỆU
-- ============================================
-- SELECT COUNT(*) AS TotalEmployees FROM Employees;
-- SELECT StatusId, COUNT(*) AS Count FROM Employees GROUP BY StatusId;
-- SELECT * FROM Employees ORDER BY HireDate DESC;

--add-migration initialcreate
--update-database