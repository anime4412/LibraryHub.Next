# Bàn giao LibraryHub.Next

## 1. Vị trí và cấu trúc

- Mã nguồn: `D:\User\NET107_TB02149_LAB\SOF1022_NHOM1_SD2003_Ver2\LibraryHub.Next`
- Solution: `LibraryHub.Next.slnx`
- Startup project: `src\LibraryHub.Web\LibraryHub.Web.csproj`
- Database ứng dụng: `LibraryHubNext` trên `ADMIN\SQLEXPRESS`
- Database integration test: `LibraryHubNext_IntegrationTests` trên `ADMIN\SQLEXPRESS`

```text
src/
  LibraryHub.Domain/          Thực thể, enum và quy tắc miền
  LibraryHub.Application/     Use case mượn/trả/phạt, quyền và hỗ trợ
  LibraryHub.Infrastructure/  EF Core, Identity, SQL Server và migration
  LibraryHub.Web/             Razor MVC, API v1, SignalR, worker và design system
tests/
  LibraryHub.UnitTests/
  LibraryHub.IntegrationTests/
docs/                         Kiến trúc, migration và tài liệu bàn giao
migration/                    ETL dữ liệu cũ và công cụ copy upload
```

## 2. Yêu cầu môi trường

- Windows 10/11.
- Visual Studio 2022 17.14 trở lên, workload **ASP.NET and web development**.
- .NET 10 SDK phù hợp với `global.json`.
- SQL Server Express instance `ADMIN\SQLEXPRESS`.
- EF Core CLI: `dotnet tool install --global dotnet-ef` nếu máy chưa có.

OAuth, SMTP, thanh toán thật và object storage không được bật trong cấu hình mặc
định. Chỉ cấu hình chúng bằng User Secrets, biến môi trường hoặc secret manager;
không ghi secret vào `appsettings*.json`.

## 3. Mở và chạy bằng Visual Studio 2022

1. Mở Visual Studio, chọn **Open a project or solution**.
2. Chọn `LibraryHub.Next.slnx`.
3. Trong Solution Explorer, bấm phải `LibraryHub.Web` rồi chọn
   **Set as Startup Project**.
4. Kiểm tra connection string trong `src\LibraryHub.Web\appsettings.json` hoặc
   User Secrets.
5. Mở **Tools > NuGet Package Manager > Package Manager Console** và chạy
   migration, hoặc chạy lệnh ở mục 4.
6. Chọn profile `LibraryHub.Web`, nhấn `F5` hoặc `Ctrl+F5`.

Profile mặc định mở `https://localhost:49239` và `http://localhost:49240`.

## 4. Migration, build và test

```powershell
cd D:\User\NET107_TB02149_LAB\SOF1022_NHOM1_SD2003_Ver2\LibraryHub.Next
dotnet restore
dotnet ef database update --project src/LibraryHub.Infrastructure --startup-project src/LibraryHub.Web
dotnet build LibraryHub.Next.slnx --configuration Release
dotnet test tests/LibraryHub.UnitTests/LibraryHub.UnitTests.csproj --configuration Release
dotnet test tests/LibraryHub.IntegrationTests/LibraryHub.IntegrationTests.csproj --configuration Release
dotnet run --project src/LibraryHub.Web
```

Connection string dùng cho bàn giao:

```text
Server=ADMIN\SQLEXPRESS;Database=LibraryHubNext;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;Encrypt=False
```

## 5. Tài khoản và phân quyền

### Reader

Reader tự tạo tài khoản tại `/register`. Mật khẩu phải dài tối thiểu 8 ký tự,
có ít nhất một chữ cái và một chữ số. Đăng ký tạo đồng thời
`AspNetUsers`, role Identity `Reader` và hồ sơ `Readers`.

Database `LibraryHubNext` mới trên SQL Express không được seed tài khoản hoặc mật khẩu
demo. Tạo tài khoản qua luồng đăng ký/Identity và cấp quyền Administrator bằng quy trình
quản trị có kiểm soát. Không chép các mật khẩu demo LocalDB cũ sang database này.

Đổi mật khẩu demo trước khi dùng ngoài máy phát triển.

### Staff

1. Admin tạo hồ sơ Employee và gán Position/Role nghiệp vụ tại
   `/admin/employees`.
2. Tạo tài khoản Identity nội bộ bằng quy trình provisioning có kiểm soát.
3. Gán role Identity `Staff`, đặt `AccountKind = Internal`, tạo đúng một bản ghi
   `StaffAccounts` nối `UserAccountId` với `EmployeeId`.
4. Gán quyền mặc định qua role nghiệp vụ; chỉ dùng `UserPermission` cho override.
   Chọn **Không ghi đè (kế thừa role)** để xóa override.

Phiên bản này chưa có màn hình tự phục vụ để tạo Identity Staff. Không chuyển một
Reader đã có dữ liệu mượn thành Staff bằng cách sửa SQL tùy ý. Với production nên
bổ sung công cụ provisioning có audit log và bắt buộc đổi mật khẩu lần đầu.

### Admin

Admin là tài khoản Identity nội bộ có role `Administrator` và
`AccountKind = Administrator`. Không seed mật khẩu mặc định. Tạo bằng quy trình
provisioning riêng của môi trường, sau đó bắt buộc đổi mật khẩu. Role này bỏ qua
kiểm tra permission nghiệp vụ nhưng vẫn chịu authentication, lockout và chính
sách mật khẩu.

## 6. Chức năng theo vai trò

| Vai trò | Chức năng chính |
|---|---|
| Công khai | Tìm và xem danh mục, chi tiết sách, bình luận/đánh giá công khai, đăng ký/đăng nhập, Quick Help |
| Reader | Hồ sơ `/me`, giỏ mượn nhiều sách, phiếu mượn/gia hạn, đặt trước, khoản phạt và ghi nhận thanh toán mô phỏng, thông báo, yêu thích, đánh giá, bình luận, tiến độ đọc, ticket/chat hỗ trợ |
| Staff | Dashboard theo quyền, sách/danh mục, circulation, độc giả, nhân viên/chức vụ, hỗ trợ; mọi API ghi kiểm tra permission code |
| Admin | Toàn bộ chức năng Staff, RBAC role/permission/override, cấu hình nghiệp vụ và broadcast thông báo |

Các danh sách lớn có tìm kiếm/lọc/phân trang server-side. Circulation phân trang
độc lập cho phiếu mượn, đặt trước và khoản phạt.

## 7. Kịch bản demo mượn–trả

1. Đăng nhập Reader, mở một sách còn hàng, chọn **Thêm vào giỏ mượn**; có thể
   thêm nhiều sách.
2. Mở `/me`, kiểm tra số lượng và gửi phiếu. API dùng `Idempotency-Key`.
3. Đăng xuất, đăng nhập Staff có `borrow.approve`; vào
   `/admin/circulation`, duyệt phiếu.
4. Staff chọn **Xác nhận đã lấy** khi Reader đến nhận. Tồn kho được cập nhật trong
   transaction.
5. Nếu cần, Reader gửi yêu cầu gia hạn; Staff có `borrow.approve_extension`
   duyệt hoặc từ chối theo cấu hình.
6. Khi trả, Staff mở từng chi tiết, nhập số lượng, tình trạng, phần trăm hư hỏng
   và ghi chú. Request replay trả lại cùng `fineId`/`fineAmount`, không tạo lần
   trả hoặc khoản phạt thứ hai.
7. Trả trễ, hư hỏng hoặc mất sách sinh Fine và FineDetails. Reader xem tại
   `/me#fines` và ghi nhận thanh toán mô phỏng. Hai thanh toán cạnh tranh không
   được vượt số tiền còn nợ.

## 8. Hạn chế và hướng phát triển

- Thanh toán chỉ là ghi nhận giao dịch, chưa kết nối cổng thanh toán.
- OAuth và SMTP chỉ hoạt động khi có secret hợp lệ; cấu hình mặc định không có
  secret.
- Cấu hình Gmail và kiểm tra email thật theo `EMAIL-SETUP.md`; hệ thống không báo
  “đã gửi” nếu SMTP chưa cấu hình hoặc gửi thất bại.
- Chưa có UI provisioning tài khoản Identity cho Staff/Admin và chưa có audit
  log đầy đủ cho thao tác này.
- Upload ảnh đang dùng filesystem cục bộ; production nên dùng malware scanning,
  CDN/object storage và lifecycle policy.
- Trạng thái nghiệp vụ đã được Việt hóa tập trung; mã quyền như
  `borrow.confirm_return` vẫn giữ nguyên vì là định danh kỹ thuật ổn định.
- Trang Reader hiện giới hạn 50 bản ghi gần nhất cho một số vùng; có thể tách
  thành trang phân trang riêng khi dữ liệu lớn.
- Cần bổ sung test trình duyệt tự động chạy trong CI cho nhiều engine, accessibility
  WCAG và kiểm tra tương phản.

## 9. Dữ liệu demo và tài liệu kiểm chứng

Hai script tùy chọn dưới đây chỉ dùng cho LocalDB trình diễn, chạy lặp lại không
tạo sách hoặc tài khoản trùng:

1. Chạy `migration/02-seed-demo-catalog.sql` để thêm tám đầu sách tiếng Việt.
2. Chạy `migration/03-normalize-localdb-demo-content.sql` để chuẩn hóa ba tài
   khoản và dữ liệu mẫu sang nội dung chuyên nghiệp.

Xem thêm `ARCHITECTURE.md`, `MIGRATION.md`, `CHANGES.md`,
`FUNCTION-MATRIX.md`, `EMAIL-SETUP.md`, `VISUAL-TEST-REPORT.md` và
`TEST-DATA-CLEANUP.md` trong cùng thư mục.
