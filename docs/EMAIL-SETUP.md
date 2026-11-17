# Cấu hình và kiểm tra email LibraryHub

LibraryHub không chứa mật khẩu SMTP trong source. Project Web đã có
`UserSecretsId`; cấu hình phát triển bằng User Secrets hoặc biến môi trường.

## Gmail bằng User Secrets

Gmail yêu cầu bật xác minh hai bước và tạo App Password riêng cho ứng dụng.
Không dùng mật khẩu đăng nhập Gmail thông thường.

```powershell
cd D:\DoAnThuVien\SOF1022_NHOM1_SD2003\Source\SOF1022_NHOM1_SD2003_Ver2\LibraryHub.Next
dotnet user-secrets set "Email:Smtp:Host" "smtp.gmail.com" --project src/LibraryHub.Web
dotnet user-secrets set "Email:Smtp:Port" "587" --project src/LibraryHub.Web
dotnet user-secrets set "Email:Smtp:Username" "dia-chi-gui@gmail.com" --project src/LibraryHub.Web
dotnet user-secrets set "Email:Smtp:Password" "APP_PASSWORD_CUA_BAN" --project src/LibraryHub.Web
dotnet user-secrets set "Email:Smtp:FromAddress" "dia-chi-gui@gmail.com" --project src/LibraryHub.Web
dotnet user-secrets set "Email:Smtp:FromName" "LibraryHub" --project src/LibraryHub.Web
```

Không chụp hoặc gửi nội dung `dotnet user-secrets list` nếu lệnh có thể làm lộ
App Password.

## Biến môi trường

Tên biến môi trường tương ứng:

```text
Email__Smtp__Host
Email__Smtp__Port
Email__Smtp__Username
Email__Smtp__Password
Email__Smtp__FromAddress
Email__Smtp__FromName
```

Trong production, lưu các biến này bằng secret manager của máy chủ hoặc nền tảng
triển khai. Không đặt chúng trong `appsettings.json`, file `.env` được commit,
pipeline log hoặc tham số dòng lệnh được ghi lịch sử.

## Kịch bản kiểm tra thật

1. Cấu hình Gmail gửi bằng một trong hai cách trên.
2. Chạy ứng dụng ở HTTPS.
3. Đăng ký một tài khoản bằng địa chỉ Gmail nhận thật.
4. Chỉ khi giao diện báo **email xác thực đã được gửi**, mở hộp thư đến.
5. Nếu chưa thấy, kiểm tra **Thư rác**, **Quảng cáo** và tìm theo tiêu đề
   `Xác thực tài khoản LibraryHub`.
6. Kiểm tra người gửi hiển thị là `LibraryHub`, mở thư và nhấn
   **Xác thực email**.
7. Đăng nhập lại bằng tài khoản vừa xác thực.
8. Thử **Quên mật khẩu** và xác minh email
   `Đặt lại mật khẩu LibraryHub` theo cùng cách.

Nếu SMTP chưa cấu hình hoặc máy chủ từ chối thư, giao diện phải báo
**chưa gửi được email**. Development có thể hiển thị liên kết cục bộ trong khối
**Chế độ phát triển**; production không hiển thị liên kết này và không thông báo
giả rằng thư đã được gửi.

Log chỉ ghi trạng thái gửi, mã lỗi SMTP hoặc loại exception và mã tài khoản.
Log không ghi người nhận, nội dung thư, token xác thực hoặc mật khẩu SMTP.

