# SOF1022 - HỆ THỐNG QUẢN LÝ THƯ VIỆN ĐIỆN TỬ (POLYLIB)
**Môn học:** SOF1022 - Dự án 1 (ASP.NET Core MVC)  
**Lớp:** SD2003 | **Nhóm:** 1  
**Cơ sở:** FPT Polytechnic  

---

## 🌟 Tổng Quan Dự Án
PolyLib là hệ thống quản trị và phục vụ độc giả thư viện điện tử toàn diện được xây dựng trên nền tảng **ASP.NET Core MVC (.NET 10)** kết hợp **Entity Framework Core**, **SQL Server** và công nghệ hiển thị **3D tương tác**.

### Các Tính Năng Nổi Bật
- **Giao diện độc giả cao cấp:** Thanh điều hướng Floating Pill Navbar chống tràn, hỗ trợ chế độ Dark/Light mode linh hoạt.
- **Kệ Sách Tinh Hoa 3D:** Trải nghiệm duyệt sách trên không gian 3D cong nghệ thuật, xoay lật 360 độ và tra cứu trực quan.
- **Hệ thống mượn trả tự động:** Đăng ký phiếu mượn online, giữ chỗ sách thực tế, quản lý tiền phạt, gia hạn và lịch sử đọc.
- **Gợi ý sách thông minh:** Phân tích sở thích đọc để đề xuất tác phẩm phù hợp cho từng độc giả.
- **Trang quản trị toàn diện:** Quản lý danh mục, đầu sách, tác giả, nhà xuất bản, độc giả, nhân viên, phân quyền RBAC và thống kê báo cáo chi tiết.

---

## 🛠️ Công Nghệ Sử Dụng
- **Backend:** C# ASP.NET Core MVC (.NET 10.0), Entity Framework Core
- **Database:** Microsoft SQL Server
- **Frontend:** HTML5, CSS3, JavaScript (ES6+), Three.js (3D Book Viewer), Bootstrap 5, FontAwesome
- **Bảo mật & Phân quyền:** Session, Cookie Authentication, Role-based Access Control (Admin / Staff / Reader)

---

## 🚀 Hướng Dẫn Cài Đặt & Khởi Chạy

### 1. Chuẩn bị
- .NET 10.0 SDK trở lên
- Microsoft SQL Server & SQL Server Management Studio (SSMS)

### 2. Cài đặt Database
- Mở file `SOF1022_NHOM1_SD2003_Ver2_Full_Database.sql` trong SSMS và thực thi để tạo cấu trúc bảng cùng dữ liệu mẫu ban đầu.
- Cập nhật chuỗi kết nối trong `appsettings.json` nếu cần:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=SOF1022_NHOM1_SD2003_Ver2;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

### 3. Chạy ứng dụng
```bash
dotnet restore
dotnet build
dotnet run --project SOF1022_NHOM1_SD2003_Ver2
```
Truy cập ứng dụng tại: `http://localhost:3636`
