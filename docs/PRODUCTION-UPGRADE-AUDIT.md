# Kiểm kê trước đợt nâng cấp production

Ngày kiểm kê: 31/07/2026  
Phạm vi: chỉ `LibraryHub.Next` và LocalDB `LibraryHubNext`.

## Điểm khôi phục

- Bản sao lưu trước nâng cấp:
  `Backups/LibraryHub.Next-before-production-upgrade-20260731-144349.zip`
- Bản sao lưu không chứa `bin`, `obj`, log hoặc kết quả test tạm.
- Không sửa dự án cũ, không xóa dữ liệu LocalDB.

## Hiện trạng kỹ thuật

| Hạng mục | Hiện trạng trước nâng cấp | Khoảng trống đã xác minh |
|---|---|---|
| Solution | Modular monolith gồm Domain, Application, Infrastructure, Web, UnitTests và IntegrationTests | Không cần đổi kiến trúc lớn |
| Xác thực nội bộ | Identity, cookie, đăng ký/đăng nhập/quên mật khẩu/xác thực email bằng giao diện tiếng Việt | Chưa có quản lý đăng nhập ngoài hoàn chỉnh |
| Google/Facebook | Đã có package và chỉ đăng ký provider khi đủ secret | Route hiện tại chỉ challenge, chưa có callback Identity để đăng nhập, tạo hoặc liên kết tài khoản; chưa có nút điều kiện |
| Hộp thoại | Phần lớn phản hồi dùng khối thông báo trong trang | Còn 4 lệnh `confirm()` trực tiếp ở quản lý sách, danh mục, nhân viên và chức vụ |
| Trang công khai | Trang chủ, tìm kiếm, sách nổi bật/mới/danh mục đều lấy từ DB | Chi tiết sách chưa có sách liên quan; một số metadata còn thiếu |
| Reader | 9 khu vực thật: tổng quan, giỏ, phiếu, đặt trước, phạt, lịch sử, yêu thích, thông báo, hồ sơ | Chưa có Trung tâm hướng dẫn theo vai trò |
| Staff/Admin | Dashboard, sách, danh mục, lưu thông, độc giả, nhân viên, hỗ trợ, RBAC, cấu hình | Nhãn permission ở RBAC đang ưu tiên mã kỹ thuật thay vì tên tiếng Việt |
| Realtime | NotificationHub và SupportHub có auth; hỗ trợ/notification dùng dữ liệu thật | Cần kiểm tra lại end-to-end bằng trình duyệt |
| Thiết kế | Một file design system dùng chung, light/dark và breakpoint responsive | Cần rà soát trực quan đủ bốn kích thước và focus/modal mới |
| Dữ liệu | LocalDB có 18 account, 16 reader, 1 employee, 9 book, 2 borrow, 2 reservation, 2 fine, 1 ticket, 9 notification | Có account kiểm thử cũ tiền tố `stage2`; không tự xóa vì chưa được xác nhận là dữ liệu bỏ |
| Test | Có 6 unit test và 19 integration test cho các luồng trọng yếu | Chưa có test External Login callback, dialog component và Help Center |
| Production | Secret không nằm trong source; có `.env.example`, tài liệu email và migration | Chưa có tài liệu Production Readiness tổng hợp; chưa xác minh OAuth/SMTP thật vì chưa có secret |

## Quét tĩnh trước thay đổi

- `alert()`: 0.
- `prompt()`: 0.
- `confirm()`: 4.
- Liên kết `href="#"`, `javascript:void(0)`, URL rỗng: 0 trong source mới.
- Nút rỗng/form action rỗng: 0.
- Nút Google/Facebook: chưa có trong giao diện mới.
- OAuth cũ từng in Client ID/Secret ra console; cách này tuyệt đối không được mang sang dự án mới.
- Dự án cũ tự tạo email giả khi Facebook không trả email; dự án mới phải yêu cầu email thật, không tái sử dụng hành vi này.

## Kế hoạch thực hiện

1. Thêm toast/modal dùng chung, có quản lý focus, Escape, click nền, loading và hiển thị `ProblemDetails`; thay cả 4 `confirm()`.
2. Hoàn thiện External Login của Identity: challenge an toàn, callback, account mới/cũ, liên kết login, thiếu email, lỗi provider; chỉ hiện provider đã cấu hình.
3. Hoàn thiện dữ liệu thật ở chi tiết sách và nhãn quyền theo vai trò; rà soát no-op/hard-code lần hai.
4. Tạo Trung tâm “Hướng dẫn sử dụng” có tìm kiếm, mục lục và nội dung theo Reader/Staff/Admin.
5. Bổ sung hardening phù hợp hiện trạng và `docs/PRODUCTION-READINESS.md`; không tạo migration nếu schema không đổi.
6. Chạy Release build, toàn bộ test, kiểm tra JavaScript, HTTP/log, sau đó kiểm tra bằng Browser theo vai trò, theme và kích thước màn hình.

## Cập nhật sau nâng cấp ngày 31/07/2026

| Hạng mục | Trạng thái sau đợt này |
|---|---|
| Modal dùng chung | 4/4 luồng xóa mềm dùng dialog; Browser đạt ở 1920×1080, 1366×768, 390×844, light/dark, Escape, nền, focus và console |
| Mutation modal | Không xác nhận xóa để bảo vệ dữ liệu; loading/disabled, toast API và chống gửi hai lần sau xác nhận chưa kiểm chứng trực quan |
| OAuth Identity | Callback/liên kết đã hoàn thiện ở mức code/build/test; provider chỉ hiện khi đủ cấu hình; không có secret trong source |
| OAuth thật | Chưa kiểm chứng vì không có Client ID/Secret thật; không tuyên bố Google/Facebook đăng nhập thành công |
| Open redirect | URL ngoài bị loại ở controller và không render lại từ ModelState trong hidden input; có integration test |
| Help Center | Đã thêm cho Reader/Staff/Administrator, lọc theo vai trò/quyền; đã kiểm tra Browser bằng cả ba tài khoản |
| Production readiness | Đã tạo `docs/PRODUCTION-READINESS.md`; SQL production, SMTP, OAuth thật, Data Protection keys, rate limit, health/monitoring, upload hardening, privacy và rollback vẫn là blocker |
| Release build | Đạt, 0 warning, 0 error |
| Unit/Integration/JavaScript | 6/6; 22/22 LocalDB; 14/14 syntax |
| Migration/schema/data | Không tạo migration, không đổi schema, không chủ động xóa dữ liệu |

Workspace được bàn giao không có `.git`; không thể xác minh commit gốc hoặc tạo commit
mới. Các file `.vs`, ZIP và backup ngoài `LibraryHub.Next` không bị chỉnh sửa hay đưa
vào phạm vi.

Các mục phụ thuộc secret hoặc dịch vụ ngoài chỉ được ghi là **chưa kiểm chứng** cho đến khi đăng nhập/gửi thư thật thành công.

## Cập nhật lưu thông ngày 01/08/2026

| Hạng mục | Trạng thái sau sửa |
|---|---|
| Thời điểm trừ kho | Chỉ trừ khi nhân viên bấm `Giao sách`; tạo và duyệt phiếu không trừ kho |
| Đặt trước sẵn sàng | Gửi thông báo “Sách đã về thư viện”, đặt hạn nhận theo cấu hình mặc định 3 ngày |
| Giao đặt trước | Tạo phiếu `Đang mượn`, trừ kho một lần, gửi thông báo; replay không tạo phiếu/trừ kho lần hai |
| Quá hạn nhận | Worker xử lý cả phiếu mượn đã duyệt và đặt trước sẵn sàng; dữ liệu legacy có hạn `NULL` được tính từ ngày tạo, không bị hủy ngay; không cộng trả tồn kho vì trước đó chưa trừ |
| Hàng đợi vận hành | Chỉ còn trạng thái đang xử lý; bản ghi đã trả/mất/hư hỏng vẫn ở database và khoản phạt vẫn hiện diện |
| Đồng thời | Khóa cập nhật bản ghi phiếu/đặt trước/sách trên SQL Server; row version hiện có vẫn là lớp bảo vệ bổ sung |
| Kiểm chứng | Release 0 warning/error; unit 8/8; integration SQL Express 24/24; JavaScript 14/14 |
| Trực quan | Đạt 1920×1080, 1366×768, 390×844 và dark mode bằng Chrome headless cô lập; console 0 lỗi |
| Migration/schema/data | Không tạo migration/đổi schema và không bấm thao tác nghiệp vụ; đăng nhập QA có thể cập nhật `LastLoginAt`, worker chỉ cập nhật phiếu thực sự quá hạn |

Browser tích hợp không mở được localhost (`net::ERR_BLOCKED_BY_CLIENT`) và Chrome extension
không khả dụng. Vì không có bản đặt trước `Sẵn sàng nhận sách` trong database ứng dụng, nút
giao của nhánh này được xác minh bằng integration test HTML/API thay vì bấm trực quan trên dữ liệu thật.

## Cập nhật vòng đời sách — 01/08/2026

| Hạng mục | Kết quả |
|---|---|
| Ngừng phục vụ thông thường | Có impact preview; chặn khi còn lượt mượn đang hoạt động; giữ nguyên tổng/khả dụng |
| Thu hồi khẩn cấp | Quyền riêng cho Administrator; giữ phiếu đang mượn; thông báo một lần; hạn trả sớm tùy chọn |
| Yêu cầu/đặt trước chưa giao | Hủy có lịch sử và thông báo; không trừ/cộng kho, không tạo phạt |
| Danh mục và yêu cầu mới | Sách ngừng/bảo trì bị loại khỏi catalog có thể mượn và bị chặn ở API |
| Trả sách bị thu hồi | Không đưa bản trả về tồn khả dụng cho đến khi kiểm kê xong |
| Khôi phục/bảo trì | Khôi phục sang Maintenance; hoàn tất bảo trì nhận số lượng kiểm kê và mới mở phục vụ |
| Xóa vĩnh viễn | Administrator-only; bị chặn nếu có bất kỳ lịch sử nghiệp vụ/cộng đồng nào |
| Đồng thời/idempotency | Transaction + khóa dòng SQL Server; một yêu cầu thắng, yêu cầu đồng thời còn lại nhận conflict |
| Audit/notification | Lưu loại/lý do/người/thời điểm; notification có correlation key chống lặp |
| Migration | Đã tạo và đạt trên `LibraryHubNext_IntegrationTests`; chưa áp lên `LibraryHubNext` chính |
| Kiểm thử | Release 0/0; unit 25/25; integration 29/29; JavaScript 14/14; 22 case lifecycle tự động đạt |
| Browser | 1920×1080, 1366×768, 390×844; dark mode; Escape/nền/focus; console 0 lỗi |

Browser tích hợp lần này truy cập được localhost qua tiến trình `dotnet` riêng và database integration.
Nó phát hiện hai lỗi UI (Escape không trả focus ổn định và modal giữ vị trí cuộn cũ); cả hai đã
được sửa và kiểm tra lại. Không thực hiện ngừng phục vụ hoặc xóa vĩnh viễn trên database chính.

Do workspace không có `.git`, audit không thể xác nhận HEAD/commit gốc, tạo diff chuẩn hoặc tạo
commit checkpoint. Không khởi tạo repository mới và không đưa file ngoài `LibraryHub.Next` vào phạm vi.
