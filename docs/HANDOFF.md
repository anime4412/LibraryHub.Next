# Handoff LibraryHub.Next

Ngày checkpoint: 31/07/2026

## Điểm tiếp tục an toàn

- Mã nguồn: `D:\DoAnThuVien\SOF1022_NHOM1_SD2003\Source\SOF1022_NHOM1_SD2003_Ver2\LibraryHub.Next`
- Solution: `LibraryHub.Next.slnx`
- Startup project: `src/LibraryHub.Web/LibraryHub.Web.csproj`
- Database được phép dùng: LocalDB `LibraryHubNext`
- Bản sao lưu trước đợt nâng cấp:
  `D:\DoAnThuVien\SOF1022_NHOM1_SD2003\Source\SOF1022_NHOM1_SD2003_Ver2\Backups\LibraryHub.Next-before-production-upgrade-20260731-144349.zip`
- Không sửa dự án cũ, không xóa dữ liệu và không tạo migration trong checkpoint này.

## Phạm vi đã hoàn thành ở checkpoint này

1. Kiểm kê source, route, test và dữ liệu LocalDB trước thay đổi; kết quả ở
   `docs/PRODUCTION-UPGRADE-AUDIT.md`.
2. Thêm component phản hồi dùng chung trong design system:
   - `LibraryHub.notify(text, error)` cho toast có live region và nút đóng.
   - `LibraryHub.askConfirmation(...)` dùng native `<dialog>`, hỗ trợ Escape,
     click nền, đưa focus vào nút Hủy và trả focus về phần tử gọi.
3. Thay đủ 4 `window.confirm()` ở:
   - xóa mềm sách;
   - xóa mềm bản ghi danh mục;
   - xóa mềm chức vụ;
   - xóa mềm hồ sơ nhân viên.
4. Các thao tác vẫn gọi đúng API có xác thực/RBAC hiện hữu; không thay đổi
   endpoint, payload, transaction hoặc dữ liệu.

## Kết quả kiểm chứng

| Kiểm tra | Kết quả |
|---|---|
| Release build | Đạt, 0 warning, 0 error |
| Unit test | 6/6 đạt |
| Integration test LocalDB | 19/19 đạt |
| JavaScript syntax | 13/13 file đạt bằng Node runtime đi kèm Codex |
| Quét `alert()/confirm()/prompt()` trực tiếp | 0 kết quả trong Razor/JavaScript |
| Migration/schema | Không thay đổi |
| Dữ liệu LocalDB | Không chủ động xóa hoặc sửa ngoài hoạt động cô lập của integration test |

Lưu ý: SDK đang là .NET 10 preview và in thông báo `NETSDK1057`; đây không phải
warning build nhưng cần chuyển sang .NET 10 SDK chính thức/LTS phù hợp trước
production.

## Chưa kiểm chứng trong checkpoint này

- Chưa kiểm tra trực quan modal mới bằng Browser. Web server nền không khởi tạo
  được trong phiên cuối do môi trường PowerShell có hai biến `Path/PATH`, làm
  `Start-Process` báo trùng khóa. Build, unit test, integration test và syntax
  test vẫn chạy thành công.
- Chưa thử thao tác xóa thật qua modal để tránh thay đổi dữ liệu LocalDB.
- OAuth Google/Facebook chưa thể đăng nhập thật vì không có secret; không được
  báo là đã hoạt động.
- SMTP thật/Gmail, responsive đầy đủ và SignalR end-to-end không được kiểm tra
  lại trong checkpoint rút gọn này.

## Phạm vi còn dang dở theo yêu cầu nâng cấp toàn diện

Ưu tiên phiên sau theo thứ tự:

1. Hoàn thiện Identity External Login:
   callback an toàn, tài khoản mới/cũ, liên kết provider, thiếu email, lỗi
   provider và chỉ hiện nút khi provider đủ cấu hình.
2. Bổ sung integration test cho External Login callback và kiểm tra Browser
   cho modal/toast (Escape, click nền, focus, light/dark, mobile).
3. Hoàn thiện chi tiết sách bằng dữ liệu DB, gồm sách liên quan và metadata còn
   thiếu; tiếp tục quét dữ liệu giả/nút no-op.
4. Đổi giao diện RBAC để tên quyền tiếng Việt là nội dung chính, mã permission
   là chú thích kỹ thuật nhỏ.
5. Tạo Trung tâm “Hướng dẫn sử dụng” theo Reader/Staff/Admin.
6. Tạo `docs/PRODUCTION-READINESS.md`, hardening production và báo cáo cuối có
   ảnh kiểm chứng theo vai trò.

Không mở rộng kiến trúc và không tạo migration trừ khi phiên sau thực sự thay
đổi schema.

## Lệnh kiểm tra lại

```powershell
cd D:\DoAnThuVien\SOF1022_NHOM1_SD2003\Source\SOF1022_NHOM1_SD2003_Ver2\LibraryHub.Next
dotnet build LibraryHub.Next.slnx --configuration Release --no-restore
dotnet test LibraryHub.Next.slnx --configuration Release --no-build --no-restore
dotnet run --configuration Release --project src\LibraryHub.Web
```

Các tài khoản demo dưới đây chỉ thuộc LocalDB cũ và không được chuyển sang SQL Express:

- Reader: `reader.demo@libraryhub.local` / `Reader@Test2026!`
- Staff: `staff.demo@libraryhub.local` / `Staff@Integration2026!`
- Admin: `admin.demo@libraryhub.local` / `Admin@Test2026!`

## Checkpoint OAuth, Help Center và production readiness — 31/07/2026

> Workspace hiện tại không chứa `.git`. Vì vậy không thể chạy `git status`, xác nhận
> HEAD chứa commit `a4e0c4c92c64eaa0a8d91e7f3ad2d87e9f913b1e`, xem diff chuẩn hoặc
> tạo commit checkpoint mới. Không khởi tạo repository mới; mọi thay đổi chỉ nằm dưới
> `LibraryHub.Next`.

### Đã thực hiện

- Kiểm tra trực quan modal xóa mềm Sách, Danh mục, Nhân viên và Chức vụ tại
  1920×1080, 1366×768 và 390×844: đúng đối tượng, nút xác nhận/hủy, Escape,
  click nền, chuyển/trả focus, light/dark, không tràn khung và console không lỗi.
  Không bấm xác nhận nên không thay đổi dữ liệu; loading/disabled, toast API và chống
  gửi hai lần ở pha mutation chưa được kiểm chứng trực quan.
- Hoàn thiện External Login Google/Facebook: provider theo cấu hình, callback cho tài
  khoản mới/đã liên kết/email đã tồn tại, liên kết sau khi xác minh mật khẩu, lỗi/từ
  chối/thiếu email, trạng thái khóa/không được phép, anti-forgery, URL quay lại cục bộ
  và log an toàn. Hidden `ReturnUrl` không render lại URL ngoài từ ModelState.
- Thêm `docs/OAUTH-CONFIGURATION.md`. Không có secret thật nên đăng nhập Google/Facebook
  thực tế **chưa được kiểm chứng**.
- Thêm `/help` cho Độc giả/Nhân viên/Quản trị viên, có mục lục, tìm kiếm, bước đánh số,
  FAQ, link thật, lọc theo vai trò/quyền, responsive, dark mode và bàn phím.
- Thêm `docs/PRODUCTION-READINESS.md` với bốn nhóm trạng thái và blocker bắt buộc.
- Không tạo migration, không thay đổi schema và không chủ động xóa dữ liệu.

## Bổ sung kiểm chứng lưu dữ liệu OAuth — 31/07/2026

- Sửa hai form `ExternalLoginConfirmation` và `ExternalLoginLink` để sinh anti-forgery
  token rõ ràng. Trước sửa, callback hiển thị được nhưng POST xác nhận có thể bị HTTP 400.
- Thêm integration test mô phỏng handshake Google và Facebook: tạo `AspNetUsers`,
  `AspNetUserRoles`, `AspNetUserLogins`, `Readers`, đăng nhập lại và cập nhật
  `LastLoginAt`; dữ liệu test được dọn sau mỗi case.
- Thêm `migration/07-audit-oauth-logins.sql`, chỉ đọc dữ liệu liên kết OAuth và không
  xuất ProviderKey, password hash, token, cookie hoặc secret.
- Không tạo migration/schema change. Google/Facebook thật vẫn chưa đăng nhập được trong
  phiên này vì chưa có Client ID/Secret thật.

## Bổ sung nút Google/Facebook trên giao diện đăng nhập — 31/07/2026

- Trang `/login` luôn hiển thị hai lựa chọn Google và Facebook. Provider có đủ cấu hình
  sẽ render form POST có anti-forgery; provider chưa cấu hình render nút disabled với
  nhãn `Chưa cấu hình` và không gửi request.
- Đã kiểm tra trực quan desktop và mobile 390×844 ở light/dark mode: không tràn ngang,
  thứ tự một cột đúng trên mobile và console không có lỗi.
- Release build đạt 0 warning/error; 4/4 integration test OAuth đạt.
- Credential Google/Facebook thật vẫn chưa được cung cấp nên hai nút hiện đang disabled;
  đăng nhập với provider thật chưa được kiểm chứng.

### Kết quả kiểm chứng cuối

| Kiểm tra | Kết quả |
|---|---|
| .NET SDK | `10.0.301` |
| Release build | Đạt, 0 warning, 0 error |
| Unit test | 6/6 đạt |
| Integration test SQL Express | 22/22 đạt trên database riêng `LibraryHubNext_IntegrationTests` |
| JavaScript syntax | 14/14 đạt bằng Node đóng gói, gọi bằng đường dẫn tuyệt đối |
| Pattern cấm | Không có `alert(`, `confirm(`, `prompt(`, `href="#"`, `javascript:void(0)` |
| Nhãn tiếng Anh rà soát | Đã đổi `Edition`/`Keywords` thành `Ấn bản`/`Từ khóa` |
| Migration/schema | Không thay đổi |

Integration chạy song song trong sandbox từng bị LocalDB error 50/`0x89c50118` ở bước
khởi tạo. Lần chạy cuối tuần tự ngoài sandbox đạt 22/22.

Các tài khoản trên không tồn tại trong `ADMIN\SQLEXPRESS` / `LibraryHubNext` mới.

## Checkpoint chuyển sang SQL Server Express — 31/07/2026

- Connection string ứng dụng: `ADMIN\SQLEXPRESS` / `LibraryHubNext`, Windows Authentication.
- Đã tạo database bằng 3 EF migration hiện có; không tạo migration mới.
- Đã nhập idempotent `migration/02-seed-demo-catalog.sql`: 8 sách mẫu, 4 danh mục mẫu.
- Startup ứng dụng đã seed 18 permission và 3 Identity role; không seed tài khoản/mật khẩu.
- Integration test dùng database tách biệt `LibraryHubNext_IntegrationTests`: 22/22 đạt.
- Release build 0 warning/error; unit test 6/6.
- Web Release đã được xác nhận HTTP 200 và đọc đúng sách mẫu từ SQL Express.
- Hướng dẫn SSMS/sqlcmd và chuyển dữ liệu mẫu nằm tại `docs/SQL-EXPRESS-SETUP.md`.
- Lần integration đầu tiên sau khi đổi server đã ghi nhầm vào database ứng dụng do
  `ConfigureAppConfiguration` chạy sau lúc Infrastructure đọc connection string. Đã sửa
  bằng cách thay trực tiếp đăng ký `LibraryDbContext`; chạy lại 22/22 và xác nhận dữ liệu
  chỉ tăng trong `LibraryHubNext_IntegrationTests`.
- `LibraryHubNext` hiện có thêm 5 sách, 6 danh mục và 4 tài khoản do lần test ghi nhầm.
  Chưa xóa vì chưa có phê duyệt. Dùng `migration/05-audit-integration-data.sql` để xem
  các bản ghi ứng viên trước khi quyết định dọn.

## Checkpoint bộ dữ liệu trình diễn mở rộng — 31/07/2026

- Đã tạo `migration/06-seed-rich-demo-data.sql` gồm 32 sách/8 danh mục và dữ liệu
  tham chiếu, lưu thông, cộng đồng, hỗ trợ, thông báo theo schema hiện hành.
- Ảnh bìa dùng Open Library Covers API; avatar dùng DiceBear. 32/32 URL bìa trả HTTP hợp lệ.
- Script idempotent, transaction + `XACT_ABORT`, không xóa dữ liệu, không tạo password/secret.
- Đã chạy hai lần trên `LibraryHubNext_IntegrationTests`; số bản ghi DEMO không tăng ở lần hai.
- Sau seed, integration test 22/22 đạt.
- Chưa chạy file này trên `LibraryHubNext`; database ứng dụng chỉ thay đổi khi người vận hành
  chủ động chạy lệnh trong `docs/SQL-EXPRESS-SETUP.md`.

## Checkpoint sửa luồng đặt trước và giao sách — 01/08/2026

- Sửa thời điểm cập nhật tồn kho: tạo/duyệt phiếu mượn và chuyển đặt trước sang
  `Sẵn sàng nhận sách` không còn trừ `AvailableQuantity`; chỉ thao tác `Giao sách`
  mới trừ tồn kho và cập nhật số lượng hiển thị công khai.
- Phiếu mượn được duyệt có hạn nhận theo `SystemSettings.PickupWindowDays` (mặc định 3 ngày).
  Đặt trước sẵn sàng nhận cũng có `ExpiresAt`; worker 5 phút xử lý cả hai loại quá hạn,
  chuyển sang trạng thái hết hiệu lực và gửi thông báo cho độc giả.
  Với dữ liệu cũ có hạn nhận `NULL`, hạn hiệu lực được tính từ ngày yêu cầu/đặt cộng số ngày
  cấu hình; worker không hủy ngay các bản ghi legacy còn trong thời hạn.
- Đặt trước `Sẵn sàng nhận sách` có nút `Giao sách`. Thao tác này tạo phiếu mượn ở trạng thái
  `Đang mượn`, có hạn trả, gửi thông báo và có idempotency key theo mã đặt trước.
- Danh sách vận hành chỉ hiển thị phiếu đang xử lý: chờ duyệt, đã duyệt/chờ giao,
  đang mượn, quá hạn hoặc chờ duyệt gia hạn. Phiếu đã trả/mất/hư hỏng vẫn được giữ trong
  database; khoản phạt và thông báo trả sách vẫn giữ nguyên.
- Trạng thái đặt trước trên giao diện chỉ hiển thị các bước chuyển hợp lệ. `Hoàn tất` không còn
  là lựa chọn thủ công; trạng thái này chỉ được đặt sau khi giao sách thành công.
- Các truy vấn cập nhật phiếu, đặt trước và sách dùng `UPDLOCK, ROWLOCK` trên SQL Server,
  kết hợp row version hiện có để tránh hai thao tác giao/trả đồng thời làm sai tồn kho.
- Kiểm tra cuối: Release build 0 warning/error; unit test 8/8; integration test SQL Express
  24/24 trên `LibraryHubNext_IntegrationTests`; JavaScript syntax 14/14.
- Kiểm tra giao diện đọc-only đạt tại 1366×768, 1920×1080 và mobile 390×844, gồm light/dark,
  nút giao phiếu mượn, danh sách chuyển trạng thái hợp lệ, không tràn ngang và không có lỗi console.
  Browser tích hợp chặn localhost bằng `net::ERR_BLOCKED_BY_CLIENT`, nên kiểm tra được thực hiện
  bằng Chrome headless cô lập. Nút giao đặt trước được kiểm chứng bằng integration test render HTML;
  chưa bấm giao trên database ứng dụng để tránh thay đổi dữ liệu thật.
- Không tạo migration, không đổi schema, không bấm giao/xóa/chỉnh dữ liệu nghiệp vụ trong
  `LibraryHubNext`. Lần đăng nhập QA có thể cập nhật `LastLoginAt` của tài khoản kiểm thử;
  worker đang chạy sẽ chỉ cập nhật các phiếu thực sự quá hạn theo quy tắc mới.
- Workspace vẫn không có `.git`, vì vậy không thể tạo commit checkpoint.

## Checkpoint vòng đời ngừng phục vụ sách — 01/08/2026

- Thay thao tác xóa sách bằng vòng đời có kiểm soát: ngừng phục vụ thông thường, thu hồi
  khẩn cấp, khôi phục sang bảo trì, hoàn tất kiểm tra kho và xóa vĩnh viễn có điều kiện.
- Ngừng thông thường bị chặn khi còn độc giả đang mượn. Thu hồi khẩn cấp yêu cầu quyền riêng,
  giữ nguyên phiếu đang mượn, có thể đặt hạn trả sớm và không tự tạo tiền phạt do hạn thu hồi.
- Các yêu cầu chưa giao và đặt trước đang hoạt động được hủy trong transaction, giữ lịch sử,
  không thay đổi tồn kho và gửi thông báo idempotent. Sách đã ngừng/bảo trì không xuất hiện
  trong danh mục có thể mượn và không được nhận yêu cầu/đặt trước mới.
- Trả sách đang bị ngừng/thu hồi không cộng lại `AvailableQuantity`. Khôi phục luôn đưa sách
  về `Đang bảo trì / chờ kiểm tra`; chỉ bước hoàn tất bảo trì với số lượng kiểm kê thực tế mới
  đưa sách về trạng thái phục vụ.
- Xóa vĩnh viễn chỉ dành cho Administrator, chỉ với sách đã ngừng và hoàn toàn chưa có lịch sử
  nghiệp vụ/cộng đồng. Không cascade để xóa lịch sử.
- Migration `20260801042438_BookServiceLifecycle` đã được tạo và kiểm chứng trên database riêng
  `LibraryHubNext_IntegrationTests`. Migration giữ số lượng/lịch sử và chuyển sách soft-delete
  cũ sang trạng thái ngừng phục vụ có audit.
- Migration **chưa áp dụng** lên `ADMIN\SQLEXPRESS / LibraryHubNext` vì chưa có phê duyệt rõ ràng
  cho thay đổi schema/data. Không được chạy build mới với database chính trước khi backup,
  review migration và phê duyệt áp dụng.
- Browser tích hợp đạt ở 1920×1080, 1366×768 và 390×844, gồm dark mode, Escape, click nền,
  chuyển/trả focus, reset vị trí cuộn và console 0 lỗi. Không xác nhận ngừng/xóa trên database chính.
  Một sách kiểm thử đã được khôi phục và hoàn tất bảo trì trong database integration để xác minh UI.
- Kiểm tra cuối: .NET SDK `10.0.301`; Release build 0 warning/error; unit 25/25;
  integration SQL Express 29/29; JavaScript syntax 14/14; 22 case lifecycle tự động đạt.
- Workspace vẫn không có `.git`; không thể xem diff chuẩn, xác minh HEAD hoặc tạo commit checkpoint.
