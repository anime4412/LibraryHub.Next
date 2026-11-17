# Báo cáo kiểm tra trực quan LibraryHub.Next

Ngày kiểm tra: 31/07/2026  
Môi trường: ASP.NET Core Release, SQL Server LocalDB `LibraryHubNext`,
Browser trong ứng dụng Codex.

## Phạm vi đã kiểm tra

| Khu vực | Tài khoản | Kích thước | Giao diện | Kết quả |
|---|---|---:|---|---|
| Trang chủ và danh mục sách | Công khai | 1920×1080 | Sáng | Đạt; 9 đầu sách, sách mới, danh mục và tồn kho dùng dữ liệu DB |
| Đăng nhập | Công khai | 1366×768 | Tối | Đạt; nhãn và liên kết tiếng Việt |
| Đăng ký | Công khai | 390×844 | Tối | Đạt; yêu cầu mật khẩu hiển thị trước khi gửi, lựa chọn loại độc giả tiếng Việt |
| Tổng quan độc giả | Reader | 1366×768 | Tối | Đạt; dữ liệu đúng tài khoản đăng nhập |
| Phiếu mượn độc giả | Reader | 390×844 | Tối | Đạt; tab vuốt ngang, không lộ thanh cuộn |
| Hỗ trợ thời gian thực | Reader | 768×1024 | Tối | Đạt; nội dung và trạng thái tiếng Việt |
| Chi tiết sách và giỏ mượn | Reader | 1366×768 | Tối | Đạt; thêm sách, dựng giỏ từ API và xóa khỏi giỏ hoạt động |
| Bảng điều khiển vận hành | Staff | 1366×768 | Tối | Đạt; menu theo permission, dữ liệu thật |
| Chặn trang phân quyền | Staff | 1366×768 | Tối | Đạt; trả trang 403 tiếng Việt |
| Phân quyền và bỏ ghi đè | Admin | 1366×768 | Tối | Đạt; hiển thị role, permission code và tùy chọn kế thừa |
| Kho sách | Admin | 768×1024 | Tối | Đạt; tìm/lọc, nút thêm/sửa/xóa và bảng responsive |
| Cấu hình nghiệp vụ | Admin | 1366×768 | Tối | Đạt; giá trị lấy từ DB, form tiếng Việt |

## Ảnh kiểm chứng

- `screenshots/05-home-light-1920.png`
- `screenshots/01-login-desktop.png`
- `screenshots/02-register-mobile.png`
- `screenshots/06-reader-dark-1366.png`
- `screenshots/07-reader-borrows-mobile.png`
- `screenshots/08-reader-support-tablet.png`
- `screenshots/09-staff-dashboard-1366.png`
- `screenshots/10-admin-rbac-1366.png`
- `screenshots/11-admin-books-tablet.png`

## Ghi chú

- Mã quyền (`book.edit`, `borrow.confirm_return`...) và mã vai trò
  (`LIBRARY_STAFF`) được giữ nguyên trên màn hình phân quyền vì đây là định danh
  kỹ thuật, không phải nhãn nghiệp vụ.
- Không kiểm tra hộp thư Gmail thật vì máy không có User Secrets/biến môi trường
  SMTP và không có địa chỉ Gmail được cung cấp. Ứng dụng đã hiển thị đúng trạng
  thái “chưa gửi được” và chỉ cung cấp liên kết xác thực cục bộ trong Development.
- Browser không ghi nhận trang 500 hoặc lỗi JavaScript không được xử lý trong
  các màn hình trên.
