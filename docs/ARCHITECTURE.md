# Kiến trúc và ERD

## Quyết định chính

- Modular monolith, dependency `Web → Application → Domain`; Infrastructure triển
  khai interface của Application.
- Identity dùng `LibraryUser` (GUID). `Reader` và `Employee` là hồ sơ nghiệp vụ,
  không chứa password.
- Hai ranh giới quyền tường minh: Identity role `Reader/Staff/Administrator` kiểm
  soát vùng ứng dụng; RBAC `Role/Permission/RolePermission/UserPermission` kiểm soát
  action chi tiết của nhân viên. `StaffAccount` nối tài khoản với Employee.
- Support ticket gán cho `LibraryUser` staff, không còn FK đến Reader.
- Enum là nguồn sự thật duy nhất. Cột `Status` string cũ chỉ dùng ở bước ETL.
- Aggregate mượn/trả dùng transaction, `Idempotency-Key`, unique index và rowversion.
- Không dùng global soft-delete filter trên entity có lịch sử để tránh làm biến mất
  phiếu cũ; query catalog/admin phải lọc `IsDeleted` rõ ràng.
- Lương cấu hình bởi `Position.SalaryAmount`; tiền phạt/gia hạn/thời hạn nhận sách
  lấy từ `SystemSettings`.

## Luồng mượn

```mermaid
stateDiagram-v2
    [*] --> PendingApproval
    PendingApproval --> ApprovedAwaitingPickup: staff duyệt / giữ tồn kho
    PendingApproval --> Cancelled: từ chối hoặc độc giả hủy
    ApprovedAwaitingPickup --> Borrowing: xác nhận đã giao sách
    ApprovedAwaitingPickup --> CanceledDueToTimeout: worker hết 3 ngày / hoàn kho
    Borrowing --> PendingExtension: yêu cầu gia hạn một chi tiết
    PendingExtension --> Borrowing: duyệt hoặc từ chối
    Borrowing --> Overdue: quá hạn
    Borrowing --> Returned: trả đủ, nguyên vẹn
    Borrowing --> LostOrDamaged: trả đủ có mất/hỏng
    Overdue --> Returned
    Overdue --> LostOrDamaged
```

## ERD rút gọn nhưng đủ phạm vi

```mermaid
erDiagram
    LibraryUser ||--o| Reader : authenticates
    LibraryUser ||--o| StaffAccount : authenticates
    StaffAccount }o--|| Employee : represents
    Employee }o--|| Position : holds
    Employee }o--|| EmployeeStatusOption : has
    Employee }o--o| Role : assigned
    Role ||--o{ RolePermission : grants
    Permission ||--o{ RolePermission : included
    Employee ||--o{ UserPermission : overrides
    Permission ||--o{ UserPermission : overridden

    Category ||--o{ Book : classifies
    Author ||--o{ Book : writes
    Publisher ||--o{ Book : publishes
    Shelf ||--o{ Book : stores

    Reader ||--o{ BorrowSlip : owns
    BorrowSlip ||--|{ BorrowDetail : contains
    Book ||--o{ BorrowDetail : borrowed
    BorrowDetail ||--o{ BorrowReturn : returned_in
    BorrowSlip ||--o{ Fine : produces
    Fine ||--|{ FineDetail : itemizes
    Reader ||--o{ Reservation : makes
    Book ||--o{ Reservation : targets
    SystemSettings ||--|| SystemSettings : singleton

    Reader ||--o{ BookFavorite : favorites
    Reader ||--o{ BookComment : comments
    BookComment ||--o{ BookCommentLike : liked
    Reader ||--o{ BookRating : rates
    Reader ||--o{ BookViewHistory : views
    Reader ||--o{ ReadingHistory : reads
    Reader ||--o{ ProfileComment : guestbook
    Reader ||--o{ ProfileTagPin : pins
    Reader ||--o{ CommentReport : reports
    Reader ||--o{ ReaderFeaturedBook : features

    Reader ||--o{ SupportTicket : opens
    LibraryUser ||--o{ SupportTicket : handles
    SupportTicket ||--o{ SupportMessage : contains
    LibraryUser ||--o{ Notification : receives
```

## Module Web

- Public/reader: catalog, book detail, favorite, comment, rating, reading progress,
  profile, borrowing, reservations, fines, notifications and support.
- Internal: dashboard, catalog CRUD, circulation, batch return, fines, reader/HR,
  roles/permissions, settings and reports.
- SignalR: `/hubs/notifications`, `/hubs/support`.
- API: `/api/v1`; write endpoint authenticated, staff/admin endpoint has policy.

## Các bước còn cần trước production

- Thêm integration test SQL Server thật và concurrency test nhiều request đồng thời.
- Bổ sung antivirus/content scanning cho upload, object storage nếu deploy nhiều node.
- Hoàn thiện payment gateway thật; endpoint hiện chỉ ghi nhận giao dịch đã xác minh.
- Bổ sung audit log bất biến cho quyền, payment, trả sách và thay đổi cấu hình.
- UAT toàn bộ màn hình CRUD sau khi nhập dữ liệu thật.
