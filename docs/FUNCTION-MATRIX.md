# Ma trận chức năng và đường dẫn LibraryHub.Next

Tài liệu đối chiếu ngày 31/07/2026. “Nút truy cập” là nhãn hiện trên giao diện,
không phải tên phương thức kỹ thuật.

## Công khai và tài khoản

| Chức năng | URL giao diện | Nút hoặc liên kết truy cập | Vai trò |
|---|---|---|---|
| Trang chủ, sách nổi bật, sách mới, danh mục | `/` | `Khám phá`, `Danh mục sách` | Tất cả |
| Tìm theo tên, tác giả, danh mục hoặc ISBN | `/?q=...#catalog` | `Tìm kiếm` | Tất cả |
| Chi tiết sách, tồn kho, đánh giá, bình luận | `/books/{id}` | Chọn bìa hoặc tên sách | Tất cả |
| Đăng ký độc giả | `/register` | `Đăng ký` | Chưa đăng nhập |
| Đăng nhập | `/login` | `Đăng nhập` | Chưa đăng nhập |
| Quên mật khẩu | `/forgot-password` | `Quên mật khẩu?` | Chưa đăng nhập |
| Đặt lại mật khẩu | `/reset-password` | Nút trong email đặt lại mật khẩu | Chưa đăng nhập |
| Gửi lại email xác thực | `/resend-confirmation` | `Gửi lại email xác thực` sau khi đăng nhập bị chặn | Chưa đăng nhập |
| Xác thực email | `/confirm-email` | `Xác thực email` trong thư | Chưa đăng nhập |
| Đăng xuất | `/logout` | `Đăng xuất` | Đã đăng nhập |

## Không gian độc giả

| Chức năng | URL giao diện | Nút hoặc liên kết truy cập | Vai trò |
|---|---|---|---|
| Tổng quan cá nhân | `/me` | `Không gian của tôi`, `Tổng quan` | Độc giả |
| Giỏ mượn nhiều sách | `/me/cart` | `Giỏ mượn`, `Thêm vào giỏ mượn` | Độc giả |
| Tạo phiếu mượn | `/me/cart` | `Gửi phiếu mượn` | Độc giả |
| Phiếu mượn và chi tiết từng sách | `/me/borrows` | `Phiếu mượn` | Độc giả |
| Yêu cầu gia hạn | `/me/borrows` | `Xin gia hạn` | Độc giả |
| Đặt trước sách | `/books/{id}`, `/me/reservations` | `Đặt trước` | Độc giả |
| Xem và thanh toán phạt mô phỏng | `/me/fines` | `Ghi nhận thanh toán` | Độc giả |
| Lịch sử và tiến độ đọc | `/me/history`, `/books/{id}` | `Lịch sử đọc`, `Lưu tiến độ` | Độc giả |
| Sách yêu thích | `/me/favorites`, `/books/{id}` | `Yêu thích` | Độc giả |
| Đánh giá sao | `/books/{id}` | `Lưu đánh giá` | Độc giả |
| Bình luận sách | `/books/{id}` | `Đăng bình luận` | Độc giả |
| Chia sẻ sách | `/books/{id}` | `Chia sẻ` | Độc giả hoặc khách |
| Thông báo cá nhân | `/me/notifications` | `Thông báo`, `Đánh dấu tất cả đã đọc` | Độc giả |
| Cập nhật hồ sơ | `/me/profile` | `Hồ sơ`, `Lưu hồ sơ` | Độc giả |
| Hỗ trợ và trò chuyện thời gian thực | `/support` | `Hỗ trợ`, `Tạo yêu cầu hỗ trợ`, `Gửi trả lời` | Độc giả |
| Trợ giúp nhanh và trợ lý tự động | `/support` | Khối gợi ý nhanh trong trung tâm hỗ trợ | Độc giả |

## Vận hành thư viện

| Chức năng | URL giao diện | Nút hoặc liên kết truy cập | Vai trò/quyền |
|---|---|---|---|
| Bảng điều khiển số liệu thật | `/admin` | `Quản trị`, `Tổng quan` | Nhân viên có `report.view`, quản trị viên |
| Danh sách và CRUD sách | `/admin/books` | `Kho sách`, `Thêm sách`, `Sửa`, `Xóa mềm` | Các quyền `book.*` tương ứng |
| Upload bìa sách an toàn | `/admin/books` | `Chọn tệp ảnh` trong biểu mẫu sách | `book.create` hoặc `book.edit` |
| Tác giả, danh mục, nhà xuất bản, kệ | `/admin/catalog` | `Tác giả & danh mục`, các nút thêm/sửa/xóa | Các quyền danh mục tương ứng |
| Tìm, lọc, phân trang circulation | `/admin/circulation` | Bộ lọc và phân trang từng nhóm | Quyền xem mượn/đặt trước/phạt |
| Duyệt hoặc từ chối phiếu mượn | `/admin/circulation` | `Duyệt`, `Từ chối` | `borrow.approve` |
| Xác nhận độc giả nhận sách | `/admin/circulation` | `Xác nhận đã lấy` | `borrow.confirm_pickup` |
| Duyệt hoặc từ chối gia hạn | `/admin/circulation` | `Duyệt gia hạn`, `Từ chối gia hạn` | `borrow.approve_extension` |
| Trả nhiều sách và ghi tình trạng | `/admin/circulation` | `Ghi nhận trả sách`, `Xác nhận trả` | `borrow.confirm_return` |
| Chuyển trạng thái đặt trước hợp lệ | `/admin/circulation` | Danh sách `Trạng thái đặt trước` | `reservation.manage` |
| Xem khoản phạt | `/admin/circulation` | Khối `Khoản phạt` | Nhân viên có một quyền vận hành circulation |
| Quản lý độc giả | `/admin/readers` | `Độc giả`, `Sửa hồ sơ` | `reader.view`, `reader.edit` |
| Quản lý nhân viên và chức vụ | `/admin/employees` | `Nhân viên & chức vụ` | Các quyền `employee.*` |
| Tiếp nhận và đóng yêu cầu hỗ trợ | `/admin/support` | `Nhận xử lý`, `Đóng yêu cầu` | `support.claim` |

## Quản trị hệ thống

| Chức năng | URL giao diện | Nút hoặc liên kết truy cập | Vai trò |
|---|---|---|---|
| Phân quyền theo vai trò | `/admin/rbac` | `Lưu quyền vai trò` | Quản trị viên |
| Ghi đè quyền nhân viên | `/admin/rbac` | `Áp dụng` | Quản trị viên |
| Trở về kế thừa quyền từ vai trò | `/admin/rbac` | `Không ghi đè (kế thừa vai trò)` | Quản trị viên |
| Cấu hình ngày mượn, gia hạn, mức phạt | `/admin/settings` | `Lưu cấu hình` | Quản trị viên |
| Gửi thông báo toàn hệ thống | `/notifications` | `Gửi thông báo` | Quản trị viên |
| Trung tâm thông báo | `/notifications` | `Thông báo` trên header | Người dùng đã đăng nhập |

## Tác vụ nền và bảo vệ kỹ thuật

| Chức năng | Điểm thực thi | Ghi chú |
|---|---|---|
| Hủy phiếu quá hạn nhận sách | `PickupExpirationWorker` | Chạy nền, dùng quy tắc nghiệp vụ và transaction |
| Idempotency tạo/trả sách | API mượn và trả | Request phát lại không tạo phiếu, lần trả hoặc khoản phạt thứ hai |
| Kiểm tra quyền nhân viên | API `/api/v1` | Dựa vào claim tài khoản và mã quyền; không tin dữ liệu quyền từ client |
| Realtime thông báo | `/hubs/notifications` | SignalR có xác thực |
| Realtime hỗ trợ | `/hubs/support` | SignalR có xác thực và kiểm tra quyền sở hữu yêu cầu |
| OAuth, SMTP và thanh toán thật | Không bật mặc định | Chỉ bật khi secret hợp lệ; thanh toán hiện là mô phỏng |
