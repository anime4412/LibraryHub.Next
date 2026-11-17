# Thay đổi so với dự án cũ

| Vấn đề cũ | Cách xử lý mới |
|---|---|
| OAuth/SMTP secret trong appsettings | Không chứa secret; đọc User Secrets/biến môi trường |
| In ClientId/ClientSecret ra console | Không có log secret |
| Seed mật khẩu `123456` | Không seed user/password; policy tối thiểu 8 ký tự, có chữ và số, kèm lockout |
| Reader vừa là tài khoản, hồ sơ và staff support | `LibraryUser`, `Reader`, `StaffAccount`, `Employee` tách biệt |
| `Readers.Role` là string tự do | Identity role kiểm soát `Reader/Staff/Administrator` |
| RBAC dựa vào tên `"Admin"` bypass | Permission được tra theo code; user override thắng role |
| Status string + enum song song | Chỉ còn enum |
| Lương hard-code theo tên chức vụ | `Position.SalaryAmount` + `AllowSalaryOverride` |
| Phạt 5.000đ hard-code trong service | Đọc `SystemSettings.LateFeePerDay` |
| Gia hạn có nơi hard-code 1/2 lần | Chỉ đọc `SystemSettings.MaxExtensionCount` |
| Logic mượn/trả ở controller và service trùng | `BorrowService` là nguồn nghiệp vụ duy nhất |
| Trả nhiều sách có nguy cơ lệch kho | Transaction + `BorrowReturn` + idempotency + rowversion |
| FK bóng `ReadersId`, `BooksId`, `BorrowDetailsId` | Fluent mapping FK duy nhất, index/constraint rõ |
| Regex họ tên dễ sai Unicode | Normalize Form C, kiểm tra Unicode letter |
| Soft-delete global làm mất lịch sử Include | Lọc ở query sử dụng; aggregate lịch sử luôn đọc được |
| CSS phân mảnh | Một design system token/component, responsive, light/dark |
| Không có test | 5 test lõi cho phạt, HR, quyền, mượn và trả |

Secret trong zip cũ phải được thu hồi/đổi ngay, kể cả khi repository chưa public.
