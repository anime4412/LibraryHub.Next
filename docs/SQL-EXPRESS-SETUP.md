# SQL Server Express cho LibraryHub.Next

## Cấu hình hiện tại

- Server: `ADMIN\SQLEXPRESS`
- Database ứng dụng: `LibraryHubNext`
- Database integration test: `LibraryHubNext_IntegrationTests`
- Xác thực: Windows Authentication (`Trusted_Connection=True`)

Connection string phát triển:

```text
Server=ADMIN\SQLEXPRESS;Database=LibraryHubNext;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;Encrypt=False
```

`Encrypt=False` chỉ phù hợp cho SQL Express trên máy phát triển. Khi triển khai thật,
dùng kết nối mã hóa và cấp connection string từ secret store/biến môi trường.

## Tạo hoặc cập nhật database bằng migration có sẵn

Chạy tại thư mục `LibraryHub.Next`:

```powershell
dotnet ef database update --project src\LibraryHub.Infrastructure --startup-project src\LibraryHub.Web
```

Lệnh này áp dụng migration đang có; không tạo migration mới và không xóa dữ liệu.

## Nhập dữ liệu mẫu từ file SQL

File `migration/02-seed-demo-catalog.sql` là script idempotent: chạy lại không nhân đôi
các sách mẫu được nhận diện bằng ISBN demo.

```powershell
sqlcmd -S "ADMIN\SQLEXPRESS" -E -C -d "LibraryHubNext" -f 65001 -b -i "migration\02-seed-demo-catalog.sql"
```

Sau đó kiểm tra:

```powershell
sqlcmd -S "ADMIN\SQLEXPRESS" -E -C -d "LibraryHubNext" -f 65001 -b -i "migration\04-verify-libraryhub.sql"
```

Để chỉ đọc và rà soát các bản ghi mang dấu hiệu dữ liệu integration test:

```powershell
sqlcmd -S "ADMIN\SQLEXPRESS" -E -C -d "LibraryHubNext" -f 65001 -b -i "migration\05-audit-integration-data.sql"
```

Script audit không chứa thao tác sửa hoặc xóa dữ liệu.

Sau khi có người dùng đăng nhập Google/Facebook thật, dùng script chỉ đọc sau để
kiểm tra tài khoản và hồ sơ độc giả đã được liên kết trong database:

```powershell
sqlcmd -S "ADMIN\SQLEXPRESS" -E -C -d "LibraryHubNext" -f 65001 -b -i "migration\07-audit-oauth-logins.sql"
```

Kết quả không xuất `ProviderKey`, password hash, token, cookie hoặc secret. Liên kết
provider được lưu trong `AspNetUserLogins`; tài khoản, hồ sơ và thời điểm đăng nhập
được lưu lần lượt trong `AspNetUsers` và `Readers`.

## Bộ dữ liệu trình diễn mở rộng

`migration/06-seed-rich-demo-data.sql` bổ sung một bộ dữ liệu lớn, idempotent:

- 32 sách thuộc 8 danh mục và 8 kệ;
- tác giả, nhà xuất bản, 4 chức vụ, 4 trạng thái nhân viên và 4 vai trò nghiệp vụ;
- ảnh bìa HTTPS từ Open Library Covers API;
- avatar mẫu từ DiceBear;
- nếu có đủ tài khoản `integration.reader`, `integration.staff` và `integration.admin`:
  5 phiếu mượn, 3 đặt trước, 2 khoản phạt, 2 ticket, thông báo, đánh giá,
  yêu thích, bình luận, lịch sử đọc và sách nổi bật.

Chạy trên database ứng dụng khi bạn đã xem nội dung file:

```powershell
sqlcmd -S "ADMIN\SQLEXPRESS" -E -C -d "LibraryHubNext" -f 65001 -b -i "migration\06-seed-rich-demo-data.sql"
```

File không tạo tài khoản/password, không xóa dữ liệu và không chứa secret. Script đã được
chạy hai lần trên `LibraryHubNext_IntegrationTests` để kiểm tra idempotency, toàn bộ
32 URL ảnh trả HTTP thành công và 22/22 integration test đạt sau khi nạp dữ liệu.

Ảnh bìa được tham chiếu trực tiếp theo [Open Library Covers API](https://openlibrary.org/dev/docs/api/covers);
avatar dùng [DiceBear HTTP API](https://www.dicebear.com/how-to-use/http-api). Môi trường
demo cần Internet để hiển thị các ảnh này; production nên cân nhắc cache/tải ảnh về storage
được kiểm soát và tuân thủ điều khoản nguồn ảnh.

Có thể mở cùng các file `.sql` trong SSMS, chọn đúng database `LibraryHubNext` rồi chạy.
Không chạy `03-normalize-localdb-demo-content.sql` trên database mới nếu chưa có các tài
khoản demo mà script đó nhắm tới.

## Chuyển dữ liệu giữa máy bằng file `.sql`

Với dữ liệu mẫu không nhạy cảm, dùng SSMS:

1. Nhấp phải database nguồn → **Tasks** → **Generate Scripts**.
2. Chọn các bảng dữ liệu mẫu cần chuyển, không chọn bảng chứa tài khoản/token nếu không cần.
3. Trong **Advanced**, đặt **Types of data to script** thành `Data only` hoặc
   `Schema and data` tùy database đích.
4. Lưu dưới dạng UTF-8 `.sql`.
5. Trước khi nhập, đọc lại `USE [...]`, tên database và mọi lệnh `DELETE`, `DROP`,
   `TRUNCATE`; không chạy nếu phạm vi không đúng.
6. Nhập bằng SSMS hoặc:

```powershell
sqlcmd -S "ADMIN\SQLEXPRESS" -E -C -d "LibraryHubNext" -f 65001 -b -i "duong-dan\du-lieu-mau.sql"
```

Đối với toàn bộ database hoặc dữ liệu thật, ưu tiên backup/restore `.bak` thay vì script
INSERT lớn. Không đưa password hash, token, OAuth secret, SMTP secret hoặc dữ liệu cá nhân
thật vào file `.sql` được commit/chia sẻ.

## Integration test

Integration test mặc định dùng database riêng `LibraryHubNext_IntegrationTests` trên cùng
SQL Express để không ghi dữ liệu test vào `LibraryHubNext`. Có thể ghi đè server/database:

```powershell
$env:LIBRARYHUB_TEST_CONNECTION = "Server=ADMIN\SQLEXPRESS;Database=LibraryHubNext_IntegrationTests;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;Encrypt=False"
dotnet test tests\LibraryHub.IntegrationTests\LibraryHub.IntegrationTests.csproj --configuration Release
```
