# Biên bản dọn dữ liệu kiểm thử Giai đoạn 4

Ngày thực hiện: 31/07/2026  
> Tài liệu lịch sử này chỉ áp dụng cho `(localdb)\MSSQLLocalDB` / `LibraryHubNext` cũ.
> Không chạy script cleanup này trên `ADMIN\SQLEXPRESS` / `LibraryHubNext` mới.

## Căn cứ xác định dữ liệu kiểm thử

Các định danh và mẫu tên dưới đây được tạo trực tiếp bởi
`tests/LibraryHub.IntegrationTests/LocalDbIntegrationTests.cs`. Chúng không phải
dữ liệu nhập tay:

- Tài khoản: `integration.admin@libraryhub.local`,
  `integration.reader@libraryhub.local`,
  `integration.staff@libraryhub.local`.
- Reader: `Integration Reader`.
- Employee/role/position: `NV-INTEGRATION`, `INTEGRATION_STAFF`, `INTEGRATION`.
- Sách có tiêu đề `Integration Book`, `Borrow flow ...`,
  `Reservation flow ...`, `Public page ...`, `Stage 3 community ...`.
- Ticket có chủ đề `Stage 3 support` hoặc `Kiểm tra HTTP Giai đoạn 3`.
- Danh mục được sinh kèm sách có hậu tố GUID.
- Ảnh `book-*.png` dài đúng 12 byte, chứa chữ ký PNG kiểm thử và không được
  bản ghi sách nào tham chiếu.

## Kết quả trước khi dọn

| Dữ liệu | Số lượng |
|---|---:|
| Tài khoản Identity Integration | 3 |
| Reader Integration | 1 |
| Employee Integration | 1 |
| Role Integration | 1 |
| Position Integration | 1 |
| Sách Integration/Stage 3 | 26 |
| Phiếu mượn liên quan | 45 |
| Khoản phạt liên quan | 33 |
| Ticket liên quan | 9 |
| Ảnh PNG kiểm thử không được tham chiếu | 5 |

Script chạy trong transaction tại `work/cleanup-stage4-localdb.sql`. Lần chạy
đầu gặp yêu cầu `QUOTED_IDENTIFIER` của SQL Server và đã rollback toàn bộ nhờ
`XACT_ABORT`. Sau khi bổ sung đầy đủ các `SET` option, lần chạy thứ hai hoàn tất.

## Kết quả sau khi dọn

- Tài khoản `integration.*` còn lại: **0**.
- Sách Integration/Stage 3 theo các mẫu trên còn lại: **0**.
- Ảnh PNG kiểm thử 12 byte còn lại: **0**.
- Ba tài khoản demo ổn định vẫn còn:
  `admin.demo@libraryhub.local`, `reader.demo@libraryhub.local`,
  `staff.demo@libraryhub.local`.
- 10 tài khoản có tiền tố `stage2.` vẫn còn nguyên vì không nằm trong phạm vi
  dọn Giai đoạn 4.
- Không xóa dữ liệu ngoài database LocalDB `LibraryHubNext`.

Các tệp ảnh kiểm thử đã xóa:

- `book-070c9874891d44b1b6f50fdb802da069.png`
- `book-718b78db9d5b476fa05a17a036c9968f.png`
- `book-9ef115d8e8c74a3aadd4e44eb9b96845.png`
- `book-b32c31d7550143b28c1ad089ddc30e1e.png`
- `book-d553b167b3e441b491b472a1e9928ee5.png`
- `book-10ddbeced10a4859847a102ea5789ba6.png`
- `book-288e4cd2aa1944dc9612b60e3388ce76.png`
- `book-364013d1de584c3b8927aaa0adef7ce7.png`
- `book-ef75d529bd4a43b08fbe1544bcd33db2.png`

Các tệp này là dữ liệu sinh tự động, không có bản ghi tham chiếu và không cần
khôi phục.
