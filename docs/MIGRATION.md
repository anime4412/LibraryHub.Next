# Kế hoạch di trú dữ liệu

## Nguyên tắc

- Blue/green: DB cũ chỉ đọc trong cửa sổ cutover; DB mới là đích riêng.
- Bảo toàn ID `int` của entity nghiệp vụ bằng `IDENTITY_INSERT`.
- Tạo GUID mới cho tài khoản; `#ReaderAccountMap` giữ ánh xạ trong ETL.
- Password hash Identity cũ có thể chuyển sang `AspNetUsers.PasswordHash`; bắt buộc
  đổi mật khẩu ở lần đăng nhập đầu. OAuth link cần người dùng liên kết lại.
- Không tự tạo mật khẩu cho Employee vì bảng cũ không phải tài khoản đăng nhập.
- Cột string status được chuẩn hoá, ưu tiên enum; giá trị không hợp lệ đi vào báo cáo
  đối soát thay vì tự đoán âm thầm.

## Mapping đáng chú ý

| Cũ | Mới |
|---|---|
| `Readers.Password/Role` | `AspNetUsers.PasswordHash` + Identity roles |
| `Readers.FeaturedBookIds` | `ReaderFeaturedBooks` (tối đa 4, giữ thứ tự) |
| `SupportTickets.AssignedStaffId -> Readers` | `AssignedStaffAccountId -> AspNetUsers` |
| `BorrowSlips.Status` + `BorrowStatus` | `BorrowSlips.Status` enum |
| `Reservations.Status` + `BooksStatus` | `Reservations.Status` enum |
| `Fines.IsPaid/Status` | `Fines.Status` enum + `PaidAmount` |
| `BorrowDetails.IsReturned/IsLost/IsDamaged` | `ReturnedQuantity` + `BorrowReturns` |
| `Position.Name`-based salary | `Position.SalaryAmount` lấy từ dữ liệu Employee hiện hữu |

## Quy trình

1. Rotate Google/Facebook/SMTP secret; backup full + checksum thư mục upload.
2. Deploy schema mới và chạy smoke test trên DB rỗng.
3. Chạy `01-migrate-data.sql` trên bản sao, xử lý mọi dòng trong mục `RECONCILIATION`.
4. Chạy `02-copy-uploads.ps1 -WhatIf`; kiểm tra extension, kích thước, checksum.
5. UAT: đăng nhập, catalog, mượn nhiều sách, duyệt/nhận, trả một phần/hàng loạt,
   phạt, gia hạn, reservation, hồ sơ, support, notification, RBAC.
6. Cutover: bật maintenance ở hệ cũ, backup cuối, chạy delta ETL, đối soát số lượng,
   chuyển connection/app, giám sát.
7. Giữ DB cũ read-only tối thiểu 30 ngày; rollback bằng cách chuyển app về DB cũ.

## Đối soát bắt buộc

- Count và min/max ID cho mọi bảng.
- Tổng `Books.TotalQuantity/AvailableQuantity`.
- Tổng phiếu theo từng status; tổng tiền phạt/đã trả.
- Reader không có account map; FK orphan; duplicate email/ISBN/CCCD.
- Tất cả đường dẫn media tồn tại và hash copy trùng.
