# KẾ HOẠCH CHI TIẾT: PHÂN CHIA VAI TRÒ NHÂN SỰ & HOÀN THIỆN CHỨC NĂNG HỆ THỐNG THƯ VIỆN

> **Tài liệu phân tích kiến trúc & kế hoạch hành động**  
> **Dự án:** Hệ thống Quản lý Thư viện Điện tử (SOF1022 - ASP.NET Core 8 MVC)  
> **Mục tiêu:** Định nghĩa rõ công việc của từng chức danh, xác định các chức năng còn thiếu của từng bộ phận, và vạch ra lộ trình hoàn thiện hệ thống phân quyền chuẩn mực.

---

## I. TỔNG QUAN KIẾN TRÚC PHÂN QUYỀN HIỆN TẠI

Hệ thống thư viện đang sử dụng mô hình bảo mật 3 lớp:
1. **Lớp Xác thực (Authentication):** Bảng `Readers` lưu trữ tài khoản đăng nhập (Email, Mật khẩu băm, Role định danh: `Admin`, `Librarian`, `Employee`, `Reader`).
2. **Lớp Hồ sơ Nhân sự (HR Profile):** Bảng `Employees` lưu mã nhân viên (`NV001` - `NV006`), thông tin cá nhân, CCCD, ngày vào làm, hệ số lương, chức vụ (`PositionId`) và nhóm quyền (`RoleId`).
3. **Lớp Kiểm soát Quyền hạn (Authorization):** Bảng `Roles` kết nối với `Permissions` thông qua `RolePermissions`. Dịch vụ `PermissionService` và bộ lọc `[PermissionAuthorize("code")]` trực tiếp kiểm tra quyền trước khi cho phép thực thi Controller/Action.

---

## II. BẢNG PHÂN CÔNG CÔNG VIỆC CHI TIẾT & CHỨC NĂNG CÒN THIẾU TỪNG VỊ TRÍ

### 1. QUẢN TRỊ VIÊN HỆ THỐNG (ADMIN)
* **Đại diện nhân sự mẫu:** `NV001` - **Nguyễn Hoàng Long** (`admin@library.com`)
* **Phân hệ đảm nhiệm:** 
  - Toàn bộ thanh điều hướng, đặc biệt là nhóm **HỆ THỐNG** (`SystemSettings`, `Employees`, Phân quyền `Roles`).
* **Công việc cụ thể trên trang web:**
  1. Khởi tạo, chỉnh sửa, khóa hoặc xóa tài khoản nhân viên thư viện.
  2. Thiết lập quy chế thư viện: số ngày mượn tối đa, số sách mượn tối đa, mức tiền phạt/ngày, thời hạn giữ sách đặt trước.
  3. Cấu hình hệ thống Email tự động (SMTP Gmail), đăng nhập Google/Facebook, giao diện toàn trang.
  4. Can thiệp khẩn cấp vào mọi dữ liệu khi có tranh chấp hoặc lỗi phát sinh.
* **Chức năng còn thiếu cần bổ sung:**
  - **Nhật ký hệ thống (Audit Log / Activity Log):** Chưa có bảng ghi nhận lại nhân viên nào đã xóa sách, duyệt phạt, hay sửa đổi thông tin bạn đọc vào thời điểm nào.
  - **Sao lưu & Phục hồi dữ liệu (Database Backup / Restore):** Chưa có nút bấm tạo bản backup `.bak` cơ sở dữ liệu ngay trên giao diện web.

---

### 2. QUẢN LÝ THƯ VIỆN (LIBRARY MANAGER / DIRECTOR)
* **Đại diện nhân sự mẫu:** `NV002` - **Trần Thị Mai Phương** (`quanly@library.com`)
* **Phân hệ đảm nhiệm:**
  - **Trang tổng quan (Dashboard)**, **Báo cáo thống kê**, **Duyệt phiếu nhập/thanh lý**.
* **Công việc cụ thể trên trang web:**
  1. Theo dõi các chỉ số vận hành thư viện theo ngày/tháng (tổng sách lưu thông, độc giả mới, tỷ lệ sách quay vòng).
  2. Giám sát tiến độ và năng suất làm việc của các nhân viên cấp dưới.
  3. Phê duyệt đề xuất mua thêm sách mới hoặc thanh lý sách quá cũ, hư hỏng nặng.
* **Chức năng còn thiếu cần bổ sung:**
  - **Báo cáo phân tích chuyên sâu (Advanced Analytics):** Thống kê sách "nằm chết trên kệ" (không ai mượn trên 6 tháng) để lên kế hoạch quảng bá hoặc luân chuyển; Thống kê độc giả có tần suất mượn cao nhất để tặng danh hiệu bạn đọc thân thiết.
  - **Xuất file báo cáo:** Chưa có nút xuất báo cáo PDF / Excel định kỳ nộp cho cấp quản lý.

---

### 3. THỦ THƯ (LIBRARIAN - NÒNG CỐT NGHIỆP VỤ)
* **Đại diện nhân sự mẫu:** `NV003` - **Lê Minh Cường** (`thuthu@library.com`)
* **Phân hệ đảm nhiệm:**
  - **QUẢN LÝ THƯ VIỆN:** Sách, Danh mục, Tác giả, Nhà xuất bản, Kệ sách.
  - **MƯỢN TRẢ SÁCH:** Phiếu mượn sách, Đơn đặt trước, Gia hạn sách.
* **Công việc cụ thể trên trang web:**
  1. Nhập liệu và cập nhật thông tin sách (Tên sách, ảnh bìa, tóm tắt, số trang, năm XB, phân loại danh mục, tác giả).
  2. Trực quầy lưu thông: Tạo phiếu mượn hộ khi độc giả đến trực tiếp quầy; xác nhận độc giả đã đến nhận sách đặt trước (`ApprovedAwaitingPickup` -> `Borrowing`).
  3. Thu hồi sách: Tiếp nhận sách trả, kiểm tra hiện trạng (nguyên vẹn hay rách bìa/mất trang) và bấm **Xác nhận trả sách**.
* **BẤT CẬP HIỆN TẠI & CHỨC NĂNG CÒN THIẾU:**
  > [!IMPORTANT]
  > **Bất cập lớn trong Database:** Nhóm quyền `Role 3 (Thủ thư)` hiện tại **đang bị thiếu toàn bộ quyền mượn trả** (`borrow.view`, `borrow.create`, `borrow.confirm_return`, `reservation.view`, `reservation.confirm`). Cần cấp bù ngay quyền này thì Thủ thư mới làm việc được tại quầy!
  - **In Barcode / Mã vạch gáy sách:** Tạo mã vạch hoặc mã QR tự động theo ISBN/Id sách để in nhãn dán gáy sách.
  - **Gia hạn mượn tại quầy:** Tính năng cho phép Thủ thư gia hạn thêm 7 - 14 ngày khi độc giả mang sách đến xin mượn tiếp.
  - **Tra cứu nhanh tại quầy (Quick POS Search):** Cho phép gõ mã thẻ độc giả hoặc quét mã vạch sách để mở ngay phiếu mượn chỉ trong 2 giây.

---

### 4. NHÂN VIÊN KHO (WAREHOUSE / INVENTORY STAFF)
* **Đại diện nhân sự mẫu:** `NV004` - **Phạm Thu Hà** (`kho@library.com`)
* **Phân hệ đảm nhiệm:**
  - **Kệ sách (Shelves)**, **Tồn kho sách (Books Inventory)**, **Kiểm kê**.
* **Công việc cụ thể trên trang web:**
  1. Quản lý sơ đồ kệ sách (Tầng, Dãy, Sức chứa tối đa của từng kệ).
  2. Xếp sách vào đúng vị trí kệ sau khi sách được nhập về hoặc sau khi độc giả hoàn trả.
  3. Cập nhật số lượng sách sẵn sàng cho mượn (`AvailableQuantity`) và số sách tồn trong kho.
* **Chức năng còn thiếu cần bổ sung:**
  - **Phiếu nhập kho (Import Batches):** Chưa có bảng quản lý các đợt nhập hàng (ngày nhập, nhà cung cấp, số lượng nhập, tổng chi phí).
  - **Biên bản kiểm kê kho định kỳ (Stocktaking Audit):** Cho phép nhân viên kho cầm máy quét/danh sách đối soát xem số lượng trên kệ thực tế có khớp với số liệu trên phần mềm không.
  - **Báo cáo hư hại / Mất mát (Book Loss & Damage):** Chức năng đánh dấu sách bị rách nát, mục nát cần sửa chữa hoặc loại bỏ khỏi danh mục lưu hành.

---

### 5. KẾ TOÁN THƯ VIỆN (ACCOUNTANT)
* **Đại diện nhân sự mẫu:** `NV005` - **Đặng Văn Hùng** (`ketoan@library.com`)
* **Phân hệ đảm nhiệm:**
  - **QUẢN LÝ PHẠT TIỀN (Fines)**, **Thu phí bồi thường**, **Báo cáo tài chính**.
* **Công việc cụ thể trên trang web:**
  1. Theo dõi danh sách các phiếu phạt phát sinh tự động do trả trễ hạn.
  2. Thu tiền mặt hoặc xác nhận chuyển khoản khi độc giả nộp phạt và bấm nút **"Xác nhận thanh toán"** (`fine.process_payment`).
  3. Lập phiếu phạt phát sinh khi độc giả làm mất sách hoặc làm hỏng sách cần bồi thường theo tỷ lệ phần trăm giá sách.
* **Chức năng còn thiếu cần bổ sung:**
  - **In biên lai thu tiền (Receipt / Invoice Generator):** Chưa có tính năng in hóa đơn/phiếu thu tiền phạt có đóng dấu mộc điện tử cho độc giả.
  - **Sổ quỹ thu chi (Cashflow Ledger):** Thống kê tổng số tiền phạt đã thu, tiền phạt còn nợ trong ngày/tháng/quý để đối soát với thủ quỹ.
  - **Chính sách miễn giảm phạt:** Chức năng ghi nhận lý do miễn giảm (bạn đọc có hoàn cảnh khó khăn, có đơn xin xác nhận từ nhà trường...).

---

### 6. CHĂM SÓC KHÁCH HÀNG & HỖ TRỢ TRỰC TUYẾN (CUSTOMER SUPPORT)
* **Đại diện nhân sự mẫu:** `NV006` - **Vũ Ngọc Lan** (`cskh@library.com`)
* **Phân hệ đảm nhiệm:**
  - **HỖ TRỢ & TƯ VẤN (SupportTickets)**, **Thông báo (Notifications)**, **Quản lý độc giả (Readers)**.
* **Công việc cụ thể trên trang web:**
  1. Trực tiếp nhận và phản hồi các yêu cầu trợ giúp, khiếu nại của bạn đọc qua kênh Chat trực tuyến Realtime (SignalR).
  2. Tạo và gửi thông báo hệ thống (nhắc nhở trả sách, thông báo lịch nghỉ lễ, giới thiệu sách mới).
  3. Hỗ trợ kích hoạt tài khoản, mở khóa bình luận hoặc sửa đổi thông tin bạn đọc khi có yêu cầu hợp lệ.
* **Chức năng còn thiếu cần bổ sung:**
  - **Mẫu câu trả lời nhanh (Canned Responses / FAQ Templates):** Các mẫu câu trả lời sẵn về thủ tục mượn sách, biểu phí phạt, giờ mở cửa để CSKH chỉ cần 1 click là gửi phản hồi.
  - **Đánh giá dịch vụ hỗ trợ (Ticket Rating):** Độc giả có thể chấm sao (1 - 5 sao) sau khi nhân viên đóng ticket hỗ trợ.
  - **Ghi chú độc giả (Internal Staff Notes):** Ghi chú nội bộ về độc giả (ví dụ: "Bạn đọc thường mượn sách chuyên ngành CNTT", "Cần nhắc nhở giữ gìn sách").

---

## III. MA TRẬN PHÂN QUYỀN TRÊN THANH ĐIỀU HƯỚNG (SIDEBAR MENU MATRIX)

| Menu trên Giao diện Quản trị | Admin | Quản lý thư viện | Thủ thư | Nhân viên kho | Kế toán | Chăm sóc KH |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: |
| **Trang tổng quan (Dashboard)** |  Toàn bộ |  Toàn bộ |  Toàn bộ |  Toàn bộ |  Toàn bộ |  Toàn bộ |
| **Quản lý Sách** |  Toàn quyền |  Toàn quyền |  Toàn quyền |  Xem & Sửa kho |  Xem |  Xem |
| **Danh mục sách** |  Toàn quyền |  Toàn quyền |  Toàn quyền | Ẩn | Ẩn | Ẩn |
| **Tác giả & Nhà xuất bản** |  Toàn quyền |  Toàn quyền |  Toàn quyền | Ẩn | Ẩn | Ẩn |
| **Kệ sách** |  Toàn quyền |  Toàn quyền |  Toàn quyền |  Toàn quyền | Ẩn | Ẩn |
| **Độc giả** |  Toàn quyền |  Toàn quyền |  Xem & Sửa | Ẩn | Ẩn |  Toàn quyền |
| **Nhân viên & Phân quyền** |  Toàn quyền |  Xem | Ẩn | Ẩn | Ẩn | Ẩn |
| **Phiếu mượn sách** |  Toàn quyền |  Toàn quyền |  Xử lý quầy *(cần cấp)* |  Xem | Ẩn |  Toàn quyền |
| **Đơn đặt trước** |  Toàn quyền |  Toàn quyền |  Duyệt quầy *(cần cấp)* | Ẩn | Ẩn |  Toàn quyền |
| **Quản lý phạt tiền** |  Toàn quyền |  Toàn quyền | Ẩn | Ẩn |  Thu nợ & Báo cáo |  Xem |
| **Yêu cầu hỗ trợ (Support Chat)**|  Toàn quyền |  Xem | Ẩn | Ẩn |  Xem |  Trực chat |
| **Thông báo** |  Toàn quyền |  Gửi thông báo | Ẩn | Ẩn |  Gửi thông báo |  Gửi thông báo |
| **Cài đặt hệ thống** |  Toàn quyền | Ẩn | Ẩn | Ẩn | Ẩn | Ẩn |

---

## V. ĐÁNH GIÁ QUYỀN HẠN CỦA ADMIN ĐỐI VỚI NHÂN VIÊN (ĐÃ CÓ & CẦN HOÀN THIỆN)

> **Câu hỏi:** Trong kế hoạch, Quản trị viên (Admin) đã có toàn quyền để điều chỉnh nhân viên chưa?  
> **Trả lời:** **VỀ MẶT PHÂN QUYỀN VÀ TRUY CẬP: ADMIN ĐÃ CÓ TOÀN QUYỀN TUYỆT ĐỐI.** Tuy nhiên, **VỀ MẶT NGHIỆP VỤ & ĐỒNG BỘ DỮ LIỆU CÒN 3 ĐIỂM NGHẼN** cần điều chỉnh để Admin thực sự làm chủ 100% vòng đời của nhân viên một cách trơn tru.

### 1. Những quyền hạn Admin ĐÃ CÓ TOÀN QUYỀN trong hệ thống hiện nay:
1. **Toàn quyền Thêm mới nhân sự (`employee.create`):**
   - Tạo hồ sơ nhân viên mới với đầy đủ mã nhân viên, họ tên, CCCD, ngày sinh, ngày vào làm, chức vụ, bộ phận.
2. **Toàn quyền Chỉnh sửa thông tin (`employee.edit`):**
   - Đổi chức vụ, thăng chức/chuyển vị trí từ Nhân viên lên Quản lý hoặc chuyển sang Thủ thư, Kế toán, Kho, CSKH.
   - Sửa đổi số điện thoại, địa chỉ, email liên hệ, ngày bắt đầu công tác.
3. **Toàn quyền Điều chỉnh trạng thái công tác (`ChangeStatus`):**
   - Chuyển trạng thái linh hoạt giữa: **Đang làm việc (Active)**, **Tạm ngưng công tác (On Leave/Paused)**, và **Đã thôi việc (Resigned)**.
4. **Toàn quyền Đặt lại mật khẩu nhân viên (`ResetPasswordFromPhanQuyen`):**
   - Khi nhân viên quên mật khẩu hoặc bị nghi ngờ lộ tài khoản, Admin có nút bấm reset trực tiếp và cấp mật khẩu mới ngay trên giao diện mà không cần can thiệp database.
5. **Toàn quyền Phân vai trò & Bật/Tắt từng quyền chi tiết (`AssignRole` / `RolePermissions`):**
   - Admin có thể gán vai trò (`Admin`, `Quản lý`, `Thủ thư`, `Nhân viên kho`, `Kế toán`, `CSKH`).
   - Admin có thể cấp thêm hoặc thu hồi từng quyền nhỏ (ví dụ: cấp riêng cho 1 Thủ thư quyền xóa sách hoặc tước quyền duyệt phạt của Kế toán).
6. **Toàn quyền Xóa mềm & Khôi phục (`Delete` & `Restore`):**
   - Cho phép xóa nhân sự khi nghỉ việc nhưng vẫn lưu giữ lịch sử các phiếu mượn/phiếu phạt mà nhân viên đó từng thụ lý. Khi cần có thể bấm "Khôi phục" lại ngay lập tức.
7. **Cơ chế Miễn nhiễm Quyền (Super Admin Bypass):**
   - Trong `PermissionService.cs`, hệ thống đã lập trình sẵn cơ chế: Nếu tài khoản là `Admin`, hệ thống tự động trả về `true` cho mọi hành động mà không bao giờ bị chặn quyền.

---

### 2. Ba (03) điểm nghẽn hiện tại làm Admin CHƯA ĐẠT 100% thuận tiện khi điều chỉnh nhân viên:

> [!WARNING]
> Đây là các điểm bất hợp lý về mặt logic code cần được khắc phục trong đợt triển khai này:

1. **Hiện tượng "Bị ghi đè mức lương" (Lương bị fix cứng theo code):**
   - *Thực trạng trong code hiện tại (`EmployeesController.cs`):* Khi Admin nhập mức lương cho nhân viên rồi bấm Lưu, Controller lại chạy một lệnh `switch-case` tự động gán lương cứng (Thủ thư = 5.000.000đ, Quản lý = 12.000.000đ,...), dẫn đến việc Admin muốn tăng lương hoặc thỏa thuận lương riêng cho nhân viên đều bị mất!
   - *Giải pháp khắc phục:* Bỏ switch-case ghi đè, cho phép Admin tự do điều chỉnh mức lương thực tế và hệ số phụ cấp theo ý muốn.

2. **Chưa tự động tạo tài khoản Đăng nhập (`Readers`) khi Admin tạo Nhân viên:**
   - *Thực trạng:* Khi Admin bấm "Thêm nhân viên", hệ thống chỉ mới lưu hồ sơ vào bảng `Employees`. Nhân viên đó chưa thể đăng nhập vào web quản trị được vì chưa tự sinh bản ghi tài khoản tương ứng trong bảng `Readers`.
   - *Giải pháp khắc phục:* Khi Admin bấm tạo nhân viên mới, hệ thống tự động sinh tài khoản đăng nhập với Email đã nhập, tạo mật khẩu ngẫu nhiên hoặc mặc định (ví dụ: `Ten@123`), gửi thông báo hoặc hiển thị ngay cho Admin để giao cho nhân viên.

3. **Chưa đồng bộ việc Khóa tài khoản đăng nhập khi Admin "Tạm ngưng" hoặc "Sa thải" nhân viên:**
   - *Thực trạng:* Khi Admin đổi trạng thái nhân sự thành "Đã thôi việc" ở bảng `Employees`, tài khoản trong bảng `Readers` vẫn giữ `IsActive = true`. Nhân viên cũ nếu biết tài khoản vẫn có thể đăng nhập xem dữ liệu.
   - *Giải pháp khắc phục:* Khi Admin đổi trạng thái nhân sự thành "Tạm ngưng" hoặc "Đã thôi việc" -> Hệ thống tự động khóa ngay tài khoản đăng nhập (`Readers.IsActive = false`), thu hồi phiên làm việc lập tức. Khi Admin bấm "Khôi phục" -> Tự động mở khóa tài khoản.

---

### 3. Cam kết sau khi hoàn thiện:
- Admin sẽ nắm **100% QUYỀN ĐIỀU CHỈNH TOÀN DIỆN**:
  - Tự do phân công, luân chuyển bất kỳ nhân viên nào sang bất kỳ vị trí nào.
  - Tự do tăng/giảm lương, thưởng.
  - Bật/tắt/khóa tài khoản đăng nhập chỉ bằng 1 cú click chuột.
  - Tự tạo nhân viên mới là có ngay tài khoản đăng nhập sử dụng được liền.
  - Cơ chế bảo vệ tài khoản gốc: Ngăn chặn Admin tự khóa hoặc tự xóa chính tài khoản của mình.

---

## VI. LỘ TRÌNH THỰC HIỆN KHI BẠN DUYỆT KẾ HOẠCH

### Giai đoạn 1: Chuẩn hóa Dữ liệu & Tài khoản Đăng nhập (Data & Account Setup)
1. **Khớp nối chức vụ chính xác trong Database:**
   - Cập nhật đúng `PositionId` và `RoleId` cho nhân viên `NV004` (Kho) và `NV005` (Kế toán).
2. **Cấp tài khoản đăng nhập cho 5 nhân viên còn lại trong bảng `Readers`:**
   - `thuthu@library.com` (Mật khẩu: `Thuthu@123`)
   - `quanly@library.com` (Mật khẩu: `Quanly@123`)
   - `kho@library.com` (Mật khẩu: `Kho@123`)
   - `ketoan@library.com` (Mật khẩu: `Ketoan@123`)
   - `cskh@library.com` (Mật khẩu: `Cskh@123`)
   - Tự động kích hoạt trạng thái `IsActive = true`, `EmailConfirmed = true`, `IsVerified = true`.

### Giai đoạn 2: Hoàn thiện Toàn quyền Quản lý Nhân viên cho Admin (Admin Full Control)
1. Bỏ logic ghi đè lương cứng, cho phép Admin tự do điều chỉnh mức lương theo thỏa thuận.
2. Thiết lập cơ chế tự động tạo tài khoản đăng nhập `Readers` khi Admin tạo hồ sơ nhân viên mới.
3. Thiết lập cơ chế đồng bộ khóa/mở khóa tài khoản đăng nhập tức thì khi Admin thay đổi trạng thái làm việc (Đang làm việc / Tạm ngưng / Thôi việc).
4. Thêm kiểm tra bảo vệ tài khoản: Không cho phép Admin tự xóa hoặc tự hạ quyền tài khoản Admin tối cao của chính mình.

### Giai đoạn 3: Cân chỉnh Ma trận Phân quyền & Sidebar (RolePermissions Alignment)
1. **Bổ sung quyền cho Thủ thư (`RoleId = 3`):**
   - Thêm `borrow.view`, `borrow.create`, `borrow.confirm_return`, `reservation.view`, `reservation.confirm`.
2. **Kiểm tra và tinh chỉnh Sidebar Navigation (`_AdminLayout.cshtml`):**
   - Đảm bảo khi Thủ thư đăng nhập, menu chỉ hiện các mục liên quan đến Sách và Mượn trả.
   - Khi Kế toán đăng nhập, chỉ hiện Tiền phạt và Báo cáo.
   - Khi Nhân viên kho đăng nhập, chỉ hiện Quản lý Sách và Kệ sách.

### Giai đoạn 4: Bổ sung các tính năng nghiệp vụ còn thiếu (Feature Enhancement)
1. Bổ sung tính năng **Xuất biên lai thu tiền phạt (PDF / Print)** cho Kế toán.
2. Bổ sung tính năng **Gia hạn mượn sách tại quầy** và **In mã QR sách** cho Thủ thư.
3. Bổ sung **Mẫu phản hồi nhanh (Canned Responses)** cho Chăm sóc khách hàng.

### Giai đoạn 5: Kiểm thử End-to-End & Bàn giao
1. Đăng nhập lần lượt 6 tài khoản ở 6 vai trò khác nhau trên trình duyệt.
2. Xác thực việc phân tách quyền chặt chẽ, thử truy cập trái phép bằng URL trực tiếp để đảm bảo `[PermissionAuthorize]` chặn đúng về trang `AccessDenied`.
3. Kiểm tra các quyền điều chỉnh nhân viên của Admin (tạo, đổi lương, đổi trạng thái, khóa, reset pass) hoạt động trơn tru 100%.

---

*Kế hoạch được cập nhật ngày 08/11/2026. Mọi thao tác code và cập nhật database sẽ chỉ bắt đầu khi nhận được phê duyệt từ bạn.*

