# HƯỚNG DẪN CÀI ĐẶT, CẤU HÌNH VÀ TRIỂN KHAI HỆ THỐNG
## DỰ ÁN .NET 1 (SOF1022 / NET108) - LỚP SD2003 - NHÓM 1
### ĐỀ TÀI: WEBSITE QUẢN LÝ THƯ VIỆN ĐIỆN TỬ HIỆN ĐẠI (DIGITAL LIBRARY MANAGEMENT SYSTEM)

- **Môn học:** Dự án .NET 1 (SOF1022 / NET108)
- **Lớp học:** SD2003
- **Nhóm thực hiện:** Nhóm 1
- **Phiên bản:** 2.0.0 (Release Candidate)
- **Nền tảng:** ASP.NET Core 10 MVC (.NET 10), Entity Framework Core 10.0.9, Microsoft SQL Server

---

## MỤC LỤC
1. [Yêu cầu phần cứng và phần mềm môi trường](#1-yêu-cầu-phần-cứng-và-phần-mềm-môi-trường)
2. [Cài đặt và khôi phục cơ sở dữ liệu SQL Server](#2-cài-đặt-và-khôi-phục-cơ-sở-dữ-liệu-sql-server)
3. [Cấu hình ứng dụng trong appsettings.json](#3-cấu-hình-ứng-dụng-trong-appsettingsjson)
   - [3.1. Chuỗi kết nối Connection String](#31-chuỗi-kết-nối-connection-string)
   - [3.2. Cấu hình Email SMTP](#32-cấu-hình-email-smtp)
   - [3.3. Cấu hình Google & Facebook OAuth](#33-cấu-hình-google--facebook-oauth)
   - [3.4. Cấu hình Trí tuệ nhân tạo (Google Gemini AI)](#34-cấu-hình-trí-tuệ-nhân-tạo-google-gemini-ai)
   - [3.5. Cấu hình mã thanh toán QR ngân hàng](#35-cấu-hình-mã-thanh-toán-qr-ngân-hàng)
4. [Hướng dẫn biên dịch và chạy trên môi trường phát triển (Localhost)](#4-hướng-dẫn-biên-dịch-và-chạy-trên-môi-trường-phát-triển-localhost)
5. [Quy trình Publish dự án sang cấu hình Release](#5-quy-trình-publish-dự-án-sang-cấu-hình-release)
6. [Hướng dẫn triển khai trên máy chủ thực tế (VPS Production)](#6-hướng-dẫn-triển-khai-trên-máy-chủ-thực-tế-vps-production)
   - [6.1. Triển khai trên VPS Windows Server (IIS)](#61-triển-khai-trên-vps-windows-server-iis)
   - [6.2. Triển khai trên VPS Linux (Ubuntu / Nginx / Systemd)](#62-triển-khai-trên-vps-linux-ubuntu--nginx--systemd)
7. [Danh sách tài khoản kiểm thử mặc định](#7-danh-sách-tài-khoản-kiểm-thử-mặc-định)
8. [Khắc phục các lỗi phổ biến (Troubleshooting Guide)](#8-khắc-phục-các-lỗi-phổ-biến-troubleshooting-guide)

---

## 1. YÊU CẦU PHẦN CỨNG VÀ PHẦN MỀM MÔI TRƯỜNG

### 1.1. Yêu cầu phần mềm máy phát triển
- **Hệ điều hành:** Windows 10/11 (64-bit) hoặc macOS / Linux hỗ trợ .NET 10.
- **IDE:** Visual Studio 2022 (phiên bản v17.12 trở lên) hoặc Visual Studio Code kèm C# Dev Kit.
- **SDK:** .NET 10.0 SDK (`dotnet --version` hiển thị phiên bản `10.0.x`).
- **Hệ quản trị CSDL:** Microsoft SQL Server 2019 / 2022 (Developer hoặc Express Edition) kèm SQL Server Management Studio (SSMS) hoặc Azure Data Studio.
- **Công cụ dòng lệnh:** PowerShell 7 hoặc Windows PowerShell 5.1.

### 1.2. Yêu cầu phần cứng tối thiểu
- **CPU:** Intel Core i3 thế hệ 8 hoặc AMD Ryzen 3 trở lên (khuyến nghị 4 nhân 8 luồng).
- **RAM:** Tối thiểu 8 GB RAM (khuyến nghị 16 GB để chạy đồng thời Visual Studio và SQL Server).
- **Ổ cứng:** Tối thiểu 5 GB dung lượng trống trên ổ SSD.

---

## 2. CÀI ĐẶT VÀ KHÔI PHỤC CƠ SỞ DỮ LIỆU SQL SERVER

Dự án cung cấp tệp SQL hoàn chỉnh chứa cấu trúc 33 bảng, các quan hệ khóa ngoại, index hiệu năng và toàn bộ dữ liệu mẫu chuẩn hóa:
Tệp: [SOF1022_NHOM1_SD2003_Ver2_Full_Database.sql](file:///d:/test/SOF1022_NHOM1_SD2003_Ver2/SOF1022_NHOM1_SD2003_Ver2_Full_Database.sql)

### Cách 1: Khôi phục bằng SQL Server Management Studio (SSMS)
1. Mở SSMS, kết nối tới SQL Server của bạn (ví dụ: `.` hoặc `localhost` hoặc `SQLEXPRESS`).
2. Chọn menu **File** -> **Open** -> **File...** và duyệt tới tệp `SOF1022_NHOM1_SD2003_Ver2_Full_Database.sql`.
3. Bấm nút **Execute** (hoặc nhấn phím `F5`).
4. Script sẽ tự động kiểm tra, tạo cơ sở dữ liệu `SOF1022_NHOM1_SD2003_Ver2`, thiết lập 33 bảng quan hệ và nạp dữ liệu mẫu.

### Cách 2: Khôi phục bằng lệnh sqlcmd qua Terminal
Mở PowerShell hoặc Command Prompt với quyền Administrator và chạy lệnh:
```bash
sqlcmd -S . -i "d:\test\SOF1022_NHOM1_SD2003_Ver2\SOF1022_NHOM1_SD2003_Ver2_Full_Database.sql"
```

---

## 3. CẤU HÌNH ỨNG DỤNG TRONG APPSETTINGS.JSON

Tệp cấu hình chính nằm tại: [appsettings.json](file:///d:/test/SOF1022_NHOM1_SD2003_Ver2/SOF1022_NHOM1_SD2003_Ver2/appsettings.json)

### 3.1. Chuỗi kết nối Connection String
Đảm bảo chuỗi kết nối khớp với môi trường SQL Server cục bộ:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=SOF1022_NHOM1_SD2003_Ver2;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```
*Lưu ý:* Nếu sử dụng tài khoản SQL Server Authentication (sa), thay thế chuỗi trên bằng:
`"Server=localhost;Database=SOF1022_NHOM1_SD2003_Ver2;User Id=sa;Password=MatKhauCuaBan;TrustServerCertificate=True"`

### 3.2. Cấu hình Email SMTP
Hệ thống sử dụng dịch vụ SMTP gửi email kích hoạt tài khoản, thông báo duyệt phiếu mượn và nhắc hạn trả sách:
```json
"SmtpSettings": {
  "Host": "smtp.gmail.com",
  "Port": 587,
  "EnableSsl": true,
  "UserName": "library.fpt.polytechnic@gmail.com",
  "Password": "mat-khau-ung-dung-16-ky-tu",
  "SenderName": "Thư Viện Điện Tử FPT",
  "SenderEmail": "library.fpt.polytechnic@gmail.com"
}
```
*Lưu ý:* Cần bật xác thực 2 bước trên tài khoản Google và tạo App Password (mật khẩu ứng dụng 16 ký tự).

### 3.3. Cấu hình Google & Facebook OAuth
Đăng nhập một chạm qua mạng xã hội:
```json
"Authentication": {
  "Google": {
    "ClientId": "YOUR_GOOGLE_CLIENT_ID.apps.googleusercontent.com",
    "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET"
  },
  "Facebook": {
    "AppId": "YOUR_FACEBOOK_APP_ID",
    "AppSecret": "YOUR_FACEBOOK_APP_SECRET"
  }
}
```

### 3.4. Cấu hình Trí tuệ nhân tạo (Google Gemini AI)
Chatbot hỗ trợ trả lời tự động 24/7 với khả năng tra cứu sách theo ngữ cảnh thời gian thực:
```json
"AiSettings": {
  "ApiKey": "AIzaSy_YOUR_GEMINI_API_KEY_HERE",
  "Model": "gemini-1.5-flash",
  "Temperature": 0.3
}
```
*Điều kiện chạy:*
- Nếu có API Key: Chatbot sử dụng Google Gemini 1.5 Flash API trực tuyến, hiểu ngôn ngữ tự nhiên và truy vấn danh mục sách trong thư viện để trả lời câu hỏi của độc giả.
- Nếu không có API Key: Hệ thống tự động chuyển sang chế độ fallback cục bộ (Local Semantic Engine), trực tiếp tìm kiếm sách, tác giả, danh mục và quy chế mượn trả từ CSDL SQL Server mà không gặp lỗi.

### 3.5. Cấu hình mã thanh toán QR ngân hàng
```json
"PaymentSettings": {
  "BankId": "MB",
  "AccountNo": "0383838383",
  "AccountName": "THU VIEN DIEN TU POLY",
  "Template": "compact"
}
```

---

## 4. HƯỚNG DẪN BIÊN DỊCH VÀ CHẠY TRÊN MÔI TRƯỜNG PHÁT TRIỂN (LOCALHOST)

### Cách 1: Sử dụng Visual Studio 2022
1. Khởi động Visual Studio 2022.
2. Chọn **Open a project or solution**, duyệt tới tệp:
   [SOF1022_NHOM1_SD2003_Ver2.slnx](file:///d:/test/SOF1022_NHOM1_SD2003_Ver2/SOF1022_NHOM1_SD2003_Ver2/SOF1022_NHOM1_SD2003_Ver2.slnx)
3. Chờ Visual Studio tải solution và tự động phục hồi các gói NuGet.
4. Nhấn phím `F5` (hoặc `Ctrl + F5`) để khởi chạy. Trình duyệt sẽ mở tại địa chỉ `https://localhost:7123` hoặc `http://localhost:5123`.

### Cách 2: Sử dụng .NET CLI (Dòng lệnh)
Mở cửa sổ dòng lệnh tại thư mục dự án và thực hiện các bước sau:
```bash
cd "d:\test\SOF1022_NHOM1_SD2003_Ver2\SOF1022_NHOM1_SD2003_Ver2"

# 1. Khôi phục thư viện phụ thuộc
dotnet restore

# 2. Xây dựng dự án
dotnet build

# 3. Khởi chạy ứng dụng
dotnet run
```
Truy cập địa chỉ hiển thị trên màn hình terminal để bắt đầu sử dụng.

---

## 5. QUY TRÌNH PUBLISH DỰ ÁN SANG CẤU HÌNH RELEASE

Để đóng gói ứng dụng phục vụ triển khai máy chủ, chạy lệnh sau:
```bash
dotnet publish "d:\test\SOF1022_NHOM1_SD2003_Ver2\SOF1022_NHOM1_SD2003_Ver2\SOF1022_NHOM1_SD2003_Ver2.csproj" -c Release -o "d:\test\SOF1022_NHOM1_SD2003_Ver2\Publish"
```
Kết quả đóng gói bao gồm:
- Tệp thực thi: `SOF1022_NHOM1_SD2003_Ver2.exe` và `SOF1022_NHOM1_SD2003_Ver2.dll`
- Tệp cấu hình: `appsettings.json`, `web.config`
- Thư mục tĩnh: `wwwroot` (chứa toàn bộ CSS, JS, hình ảnh bìa sách và biểu tượng)
- Toàn bộ gói DLL tối ưu hóa không chứa tệp rác.

Gói nén phân phối chính thức: [N3_Publish_Release.zip](file:///d:/test/SOF1022_NHOM1_SD2003_Ver2/N3_Publish_Release.zip)

---

## 6. HƯỚNG DẪN TRIỂN KHAI TRÊN MÁY CHỦ THỰC TẾ (VPS PRODUCTION)

### 6.1. Triển khai trên VPS Windows Server (IIS)
1. **Chuẩn bị máy chủ:**
   - Cài đặt tính năng IIS (Internet Information Services) qua Server Manager.
   - Cài đặt **.NET 10 Hosting Bundle** (chứa .NET CLR và IIS AspNetCoreModuleV2).
2. **Cấu hình Website trên IIS:**
   - Giải nén tệp `N3_Publish_Release.zip` vào thư mục máy chủ: `C:\inetpub\wwwroot\LibrarySystem`.
   - Mở IIS Manager, click chuột phải vào **Sites** -> **Add Website**.
   - Điền Site name: `LibrarySystem`, Physical path trỏ tới `C:\inetpub\wwwroot\LibrarySystem`.
   - Cấu hình Binding: Port 80 hoặc Port 443 kèm chứng chỉ SSL.
3. **Cấu hình Application Pool:**
   - Chọn Application Pool của site, chọn **Advanced Settings**.
   - Đặt `.NET CLR Version` = **No Managed Code**.
4. **Phân quyền thư mục:**
   - Cấp quyền `Modify` cho tài khoản `IIS_IUSRS` trên thư mục `wwwroot/uploads` để người dùng có thể tải lên ảnh bìa sách và ảnh đại diện.

### 6.2. Triển khai trên VPS Linux (Ubuntu / Nginx / Systemd)
1. **Cài đặt .NET 10 Runtime trên Ubuntu:**
   ```bash
   sudo apt-get update
   sudo apt-get install -y dotnet-runtime-10.0 aspnetcore-runtime-10.0 nginx
   ```
2. **Sao chép mã nguồn:**
   - Tải tệp Publish lên thư mục: `/var/www/library`.
3. **Tạo dịch vụ Systemd:**
   Tạo tệp cấu hình: `sudo nano /etc/systemd/system/kestrel-library.service`:
   ```ini
   [Unit]
   Description=Modern Digital Library System ASP.NET Core
   After=network.target

   [Service]
   WorkingDirectory=/var/www/library
   ExecStart=/usr/bin/dotnet /var/www/library/SOF1022_NHOM1_SD2003_Ver2.dll
   Restart=always
   RestartSec=10
   SyslogIdentifier=dotnet-library
   User=www-data
   Environment=ASPNETCORE_ENVIRONMENT=Production
   Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

   [Install]
   WantedBy=multi-user.target
   ```
   Kích hoạt dịch vụ:
   ```bash
   sudo systemctl enable kestrel-library.service
   sudo systemctl start kestrel-library.service
   ```
4. **Cấu hình Nginx Reverse Proxy:**
   Thêm cấu hình vào `/etc/nginx/sites-available/default`:
   ```nginx
   server {
       listen 80;
       server_name thuvien.yourdomain.com;

       location / {
           proxy_pass http://localhost:5000;
           proxy_http_version 1.1;
           proxy_set_header Upgrade $http_upgrade;
           proxy_set_header Connection keep-alive;
           proxy_set_header Host $host;
           proxy_cache_bypass $http_upgrade;
           proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
           proxy_set_header X-Forwarded-Proto $scheme;
       }
   }
   ```
   Tải lại Nginx: `sudo nginx -s reload`.

---

## 7. DANH SÁCH TÀI KHOẢN KIỂM THỬ MẶC ĐỊNH

Cơ sở dữ liệu đã khởi tạo sẵn các tài khoản tương ứng với từng vai trò phục vụ công tác chấm điểm và nghiệm thu:

| STT | Vai trò | Họ và tên | Email đăng nhập | Mật khẩu mặc định | Quyền hạn chính |
| :---: | :--- | :--- | :--- | :--- | :--- |
| 1 | **Admin (Quản trị viên)** | Nguyễn Hoàng Long | `admin@library.com` | `Admin@123` | Toàn quyền hệ thống, phân quyền nhân viên, cấu hình cài đặt. |
| 2 | **Manager (Quản lý)** | Trần Thị Bích Ngọc | `manager@library.com` | `Manager@123` | Báo cáo doanh thu, thống kê mượn trả, giám sát hoạt động. |
| 3 | **Librarian (Thủ thư)** | Lê Văn Hùng | `librarian@library.com` | `Librarian@123` | Duyệt mượn, nhận trả sách, lập biên bản phạt vi phạm. |
| 4 | **Warehouse (Thủ kho)** | Phạm Đức Thắng | `warehouse@library.com` | `Warehouse@123` | Nhập sách mới, điều chuyển kệ, quản lý tồn kho và thanh lý. |
| 5 | **Accountant (Kế toán)** | Vũ Thị Mai | `accountant@library.com` | `Accountant@123` | Thu tiền phạt vi phạm, đối soát tài chính, xuất hóa đơn. |
| 6 | **Customer Support** | Đỗ Quang Huy | `support@library.com` | `Support@123` | Trực chatbox hỗ trợ, trả lời khiếu nại của độc giả. |
| 7 | **Reader (Độc giả)** | Lê Phú An | `docgia@gmail.com` | `Reader@123` | Tra cứu sách, đặt giữ sách, quản lý phiếu mượn cá nhân. |

---

## 8. KHẮC PHỤC CÁC LỖI PHỔ BIẾN (TROUBLESHOOTING GUIDE)

### Lỗi 1: `SqlException: Cannot open database requested by the login`
- **Nguyên nhân:** Tên database trong chuỗi kết nối không tồn tại hoặc SQL Server chưa được khởi tạo.
- **Cách xử lý:** Chạy lại tệp `SOF1022_NHOM1_SD2003_Ver2_Full_Database.sql` để tạo cơ sở dữ liệu.

### Lỗi 2: `Certificate chain was issued by an authority that is not trusted`
- **Nguyên nhân:** SQL Server 2019/2022 yêu cầu chứng chỉ mã hóa kết nối.
- **Cách xử lý:** Đảm bảo trong Connection String có thuộc tính `TrustServerCertificate=True`.

### Lỗi 3: `HTTP Error 500.30 - ASP.NET Core app failed to start` trên IIS
- **Nguyên nhân:** Thiếu .NET 10 Hosting Bundle hoặc chuỗi kết nối sai.
- **Cách xử lý:** Cài đặt gói ASP.NET Core 10 Hosting Bundle, kiểm tra tệp `stdout` log bằng cách bật `<aspNetCore stdoutLogEnabled="true" stdoutLogFile=".\logs\stdout" />` trong `web.config`.

### Lỗi 4: Không gửi được Email thông báo
- **Nguyên nhân:** Google chặn ứng dụng kém an toàn hoặc chưa bật App Password.
- **Cách xử lý:** Vào trang quản lý tài khoản Google -> Bảo mật -> Mật khẩu ứng dụng -> Tạo mật khẩu riêng cho ứng dụng Thư viện và dán vào `appsettings.json`.

### Lỗi 5: Chatbot báo "Hệ thống đang bảo trì AI"
- **Nguyên nhân:** Chưa cấu hình Gemini API Key hoặc Key hết hạn ngạch.
- **Cách xử lý:** Thêm API Key hợp lệ vào `AiSettings:ApiKey` hoặc tạo biến môi trường `GEMINI_API_KEY`. Trong thời gian chờ, hệ thống tự động chạy chế độ tìm kiếm ngữ cảnh nội bộ không làm gián đoạn trải nghiệm người dùng.
