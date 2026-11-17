# Production Readiness

Tài liệu này là checklist triển khai, không phải xác nhận hệ thống đã chạy production. Không thực hiện migration, thay đổi schema, triển khai hay thay đổi dữ liệu trong đợt rà soát này.

## Đã sẵn sàng

- Release build và các bộ test tự động hiện có chạy được trên checkpoint phát triển.
- Cookie đăng nhập đặt `HttpOnly`, `Secure=Always`, `SameSite=Lax`; Identity có khóa tạm thời sau nhiều lần sai và yêu cầu email xác thực.
- MVC POST dùng anti-forgery toàn cục; API có authorization/policy/permission checks; output Razor được encode mặc định.
- Exception handler trả thông báo production an toàn, không đưa stack trace vào phản hồi 500.
- OAuth chỉ được đăng ký khi đủ cấu hình, không lưu token, không chứa secret trong source, chặn open redirect và không tự liên kết theo email.
- Upload ảnh hiện kiểm tra dung lượng tối đa 5 MB, phần mở rộng và chữ ký tệp; tên file lưu trữ do server tạo.
- HTTPS redirection và HSTS đã bật.

## Cần cấu hình khi triển khai

- Dùng SQL Server production thay SQL Express phát triển. Cấp `ConnectionStrings__DefaultConnection` từ secret store/biến môi trường với tài khoản DB có quyền tối thiểu; production không fallback về SQL Server local.
- Cấu hình tên miền, chứng chỉ TLS, reverse proxy và forwarded headers; xác nhận ứng dụng nhìn thấy đúng scheme/host công khai.
- Khai báo callback OAuth production và secret Google/Facebook theo `docs/OAUTH-CONFIGURATION.md`.
- Cấu hình SMTP thật bằng secret store, xác minh SPF/DKIM/DMARC, địa chỉ gửi và luồng reset/xác thực email.
- Lưu Data Protection keys ở kho bền vững dùng chung giữa các instance, mã hóa key khi lưu và đặt application name ổn định.
- Chọn nơi lưu ảnh bền vững (volume/object storage), quyền ghi tối thiểu, CDN nếu dùng và chính sách sao lưu/xóa.
- Cấu hình mức log, retention, correlation ID, cảnh báo và quyền truy cập log; tuyệt đối không ghi password, token, secret, cookie hoặc dữ liệu nhạy cảm đầy đủ.
- Tạo tài khoản Administrator ban đầu bằng quy trình bảo mật có mật khẩu duy nhất, buộc đổi/luân chuyển và ghi nhận người sở hữu.

## Chưa sẵn sàng hoặc chưa kiểm chứng

- Chưa chạy trên SQL Server production; hiệu năng, collation, quyền DB và kết nối mã hóa chưa được kiểm chứng.
- Chưa diễn tập migration production, backup trước migration, restore và rollback. Migration phải được duyệt và chạy như một bước riêng có bản sao lưu.
- Forwarded headers/reverse proxy production chưa được kiểm chứng.
- Chưa có tên miền/chứng chỉ production trong phạm vi này.
- Đăng nhập Google/Facebook thật chưa được kiểm chứng vì không có Client ID/Secret thật.
- SMTP production và khả năng giao thư chưa được kiểm chứng.
- Chưa cấu hình kho Data Protection keys bền vững/chia sẻ.
- Chưa có rate limiting rõ ràng cho login, register, resend email, support và API nhạy cảm.
- Chưa có hệ thống logging/monitoring tập trung và cảnh báo vận hành được kiểm chứng.
- Chưa có health checks riêng cho ứng dụng, database, SMTP và storage.
- Chưa có quét malware/ảnh độc hại, quota tổng, object storage production hoặc quy trình dọn orphan upload.
- Chưa có chính sách riêng tư/cookie và trang thông tin xử lý dữ liệu ở mức phát hành thật.
- Chưa có runbook triển khai/rollback hoàn chỉnh và chưa diễn tập restore.
- Dark mode, responsive và các modal đã kiểm tra trình duyệt ở môi trường local; không thay thế kiểm thử trên hạ tầng/trình duyệt production mục tiêu.

## Bắt buộc hoàn thành trước khi mở cho người dùng thật

1. Chốt SQL Server production, secret connection string và nguyên tắc quyền tối thiểu; chạy rehearsal migration trên bản sao dữ liệu.
2. Tạo backup có kiểm chứng, thử restore, xác định thời gian phục hồi và viết thủ tục rollback ứng dụng/database.
3. Hoàn tất HTTPS, domain, reverse proxy/forwarded headers và kiểm thử cookie, redirect, HSTS từ Internet.
4. Cấu hình SMTP thật và kiểm thử xác thực email, quên mật khẩu, lỗi gửi và chống lạm dụng.
5. Nếu bật OAuth, cấu hình callback/secret thật và kiểm thử đầy đủ Google/Facebook; nếu chưa đạt, giữ provider không cấu hình để nút không xuất hiện.
6. Thiết lập Data Protection keys bền vững, rate limiting, health checks, monitoring/cảnh báo và quy trình trực sự cố.
7. Rà soát CSRF, XSS, authorization theo ma trận vai trò trên môi trường staging; thực hiện kiểm thử bảo mật cho upload và các endpoint nhạy cảm.
8. Chọn storage ảnh production, bổ sung kiểm soát malware/quota phù hợp và kiểm thử backup/restore ảnh.
9. Loại bỏ hoặc vô hiệu hóa tài khoản/dữ liệu demo khỏi môi trường production; không dùng mật khẩu demo. Tạo Administrator ban đầu theo quy trình được phê duyệt.
10. Công bố chính sách riêng tư/cookie cơ bản, thông tin liên hệ hỗ trợ và quy trình xử lý/xóa dữ liệu.
11. Chạy smoke test sau triển khai và diễn tập rollback trước ngày mở cửa.

## Quy trình triển khai và rollback tối thiểu

1. Đóng băng artifact Release đã qua test, ghi phiên bản và checksum.
2. Sao lưu database và xác minh bản sao có thể đọc/restore.
3. Áp migration đã duyệt bằng tài khoản triển khai riêng; không cho ứng dụng tự ý sửa schema khi khởi động production.
4. Triển khai theo rolling/blue-green nếu có thể, kiểm tra health, login, catalog, circulation, email và log.
5. Nếu smoke test thất bại, rút traffic khỏi bản mới và quay lại artifact trước. Chỉ rollback database theo runbook đã diễn tập; không tự xóa migration hay dữ liệu.
6. Ghi nhận kết quả, người phê duyệt và thời điểm hoàn tất.

## Cập nhật readiness cho lưu thông — 01/08/2026

### Đã sẵn sàng ở mức source và kiểm thử local

- Tồn kho chỉ giảm khi giao sách; duyệt phiếu/chuẩn bị đặt trước không làm thay đổi số lượng.
- Hạn nhận mặc định 3 ngày được áp dụng cho phiếu đã duyệt và đặt trước sẵn sàng; worker tự
  vô hiệu hóa quá hạn và gửi thông báo. Bản ghi legacy có hạn `NULL` dùng ngày tạo làm mốc,
  tránh bị hủy ngay ở lần quét đầu.
- Phiếu trả/mất/hư hỏng được loại khỏi hàng đợi đang xử lý nhưng không bị xóa; khoản phạt và
  lịch sử database vẫn được giữ.
- Transaction giao/trả khóa bản ghi liên quan trên SQL Server và vẫn dùng row version hiện có.
- Luồng chính đã qua unit test 8/8 và integration test SQL Express 24/24.

### Chưa sẵn sàng hoặc chưa kiểm chứng ở production

- Chưa stress test nhiều nhân viên giao/trả đồng thời dưới tải production hoặc nhiều instance.
- Worker quá hạn hiện quét mỗi 5 phút trong tiến trình web; khi chạy nhiều instance cần xác minh
  chiến lược distributed lock/job scheduler và theo dõi retry/deadlock.
- Chưa kiểm chứng notification realtime, monitoring và cảnh báo worker trên hạ tầng production.

## Readiness cho vòng đời ngừng phục vụ sách — 01/08/2026

### Đã sẵn sàng ở mức source và kiểm thử local

- Có trạng thái và audit riêng cho ngừng thông thường/thu hồi khẩn cấp/khôi phục/bảo trì.
- Luồng ngừng, hủy yêu cầu chưa giao, hủy đặt trước, thông báo và cập nhật kho nằm trong transaction;
  các cập nhật nhạy cảm dùng khóa dòng SQL Server và idempotency key.
- Phân quyền tách riêng ngừng thông thường, thu hồi khẩn cấp, khôi phục, hoàn tất bảo trì và xóa
  vĩnh viễn. Endpoint vẫn kiểm tra quyền server-side.
- Lịch sử mượn, đặt trước, phạt, đánh giá, bình luận và các liên kết nghiệp vụ ngăn xóa vĩnh viễn.
- Release build và 54 test hiện có đạt; trong đó 22 case tự động kiểm tra riêng vòng đời sách.

### Cần cấu hình khi triển khai

- Review migration `20260801042438_BookServiceLifecycle`, lập backup và rehearsal trên bản sao
  production trước khi áp dụng bằng tài khoản triển khai có kiểm soát.
- Xác định nhóm người dùng thực tế được cấp quyền thu hồi khẩn cấp và xóa vĩnh viễn; mặc định Staff
  không có hai quyền này.
- Thiết lập giám sát lỗi transaction/deadlock và theo dõi notification thất bại hoặc bị retry.

### Chưa sẵn sàng/chưa kiểm chứng

- Migration chưa áp dụng lên `ADMIN\SQLEXPRESS / LibraryHubNext`; schema database chính hiện chưa
  tương thích với build mới.
- Chưa stress test thao tác đồng thời trên nhiều web instance hoặc tải production.
- Chưa kiểm chứng gửi notification realtime trên hạ tầng production.

### Bắt buộc trước khi mở cho người dùng thật

1. Backup có kiểm chứng và thử restore.
2. Review/duyệt migration, chạy rehearsal trên bản sao rồi mới áp migration vào production.
3. Smoke test catalog, mượn/trả, đặt trước, ngừng thông thường, thu hồi khẩn cấp, khôi phục và
   hoàn tất bảo trì bằng dữ liệu staging an toàn.
4. Xác nhận ma trận quyền và log audit với người vận hành; tuyệt đối không cấp quyền xóa vĩnh viễn
   đại trà.
