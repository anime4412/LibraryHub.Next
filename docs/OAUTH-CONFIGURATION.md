# Cấu hình đăng nhập Google và Facebook

LibraryHub luôn hiển thị lựa chọn Google và Facebook trên trang đăng nhập để người dùng biết các phương thức được hỗ trợ. Nút chỉ được bật và có thể gửi yêu cầu khi có đủ ID và secret tương ứng; nếu chưa cấu hình, nút hiển thị trạng thái `Chưa cấu hình`. Source code không chứa Client ID, Client Secret, App ID hoặc App Secret. Không đặt secret trong `appsettings.json`, file `.env`, tài liệu, log hay commit.

## Địa chỉ callback

Với profile HTTPS hiện tại trên máy phát triển:

- Google: `https://localhost:49239/signin-google`
- Facebook: `https://localhost:49239/signin-facebook`

Với tên miền thật, thay host nhưng giữ nguyên path:

- Google: `https://<ten-mien-that>/signin-google`
- Facebook: `https://<ten-mien-that>/signin-facebook`

Callback phải khớp tuyệt đối về scheme, host, port và path. Production phải dùng HTTPS. Nếu chạy sau reverse proxy, ứng dụng phải nhận đúng scheme/host bên ngoài; kiểm tra cấu hình forwarded headers của proxy và ASP.NET Core trước khi bật OAuth.

## User Secrets cho localhost

Chạy tại `src/LibraryHub.Web` và thay các giá trị trong dấu `<...>` bằng thông tin thật do nhà cung cấp cấp. Các chuỗi dưới đây chỉ là tên giữ chỗ, không phải secret dùng được.

```powershell
dotnet user-secrets init
dotnet user-secrets set "Authentication:Google:ClientId" "<google-client-id>"
dotnet user-secrets set "Authentication:Google:ClientSecret" "<google-client-secret>"
dotnet user-secrets set "Authentication:Facebook:AppId" "<facebook-app-id>"
dotnet user-secrets set "Authentication:Facebook:AppSecret" "<facebook-app-secret>"
```

Không cần cấu hình cả hai provider. Nút tương ứng tự được bật khi cặp ID/secret của provider đó đầy đủ; provider chưa cấu hình vẫn hiển thị nhưng bị vô hiệu hóa và không gửi request.

## Biến môi trường production

Nạp secret bằng secret store của nền tảng triển khai, không ghi vào image hay repository:

```text
Authentication__Google__ClientId
Authentication__Google__ClientSecret
Authentication__Facebook__AppId
Authentication__Facebook__AppSecret
```

Sau khi cấu hình, khởi động lại instance và kiểm tra trang `/login`. Không in giá trị các biến này ra log.

## Google Cloud Console

1. Tạo hoặc chọn project và cấu hình OAuth consent screen.
2. Khai báo tên ứng dụng, email hỗ trợ, miền được ủy quyền và các URL chính sách cần thiết.
3. Tạo OAuth Client ID loại **Web application**.
4. Thêm callback localhost và callback production vào **Authorized redirect URIs**.
5. Khi ứng dụng còn ở chế độ Testing, thêm đúng tài khoản kiểm thử. Trước khi phát hành, hoàn tất yêu cầu xác minh/đưa ứng dụng sang trạng thái phù hợp.
6. Sao chép Client ID/Client Secret vào User Secrets hoặc secret store; không đưa vào source.

LibraryHub chỉ cần thông tin định danh cơ bản và email. Nếu Google không trả email, luồng sẽ dừng và hướng dẫn người dùng đăng nhập bằng tài khoản LibraryHub.

## Meta Developer Console

1. Tạo ứng dụng Meta phù hợp và thêm sản phẩm **Facebook Login**.
2. Trong Facebook Login Settings, thêm callback localhost và production vào **Valid OAuth Redirect URIs**.
3. Cấu hình miền ứng dụng, URL chính sách riêng tư, URL điều khoản/xóa dữ liệu theo yêu cầu của Meta.
4. Bật quyền `email`; thêm tài khoản tester khi ứng dụng chưa Live.
5. Hoàn tất App Review nếu phạm vi hoặc cách phát hành yêu cầu, rồi chuyển Live khi checklist production đã đạt.
6. Lưu App ID/App Secret trong User Secrets hoặc secret store.

Nếu Facebook không trả email, LibraryHub không tạo hay tự liên kết tài khoản.

## Hành vi an toàn đã triển khai

- Không tự liên kết tài khoản chỉ vì email provider trùng email hiện có; người dùng phải nhập lại mật khẩu LibraryHub.
- Tài khoản bị khóa, chưa xác thực, không hoạt động hoặc không có hồ sơ phù hợp bị từ chối.
- URL quay lại chỉ được chấp nhận khi là URL cục bộ; URL ngoài bị loại bỏ.
- POST challenge/xác nhận/liên kết đều được kiểm tra anti-forgery; hai form callback sinh token rõ ràng.
- Token không được lưu bởi middleware và log chỉ chứa mã người dùng/provider, không chứa token hoặc secret.

## Dữ liệu được ghi vào SQL Server

Khi người dùng mới hoàn tất đăng nhập Google/Facebook, LibraryHub ghi dữ liệu bằng
một transaction:

- `AspNetUsers`: tài khoản Identity, email, tên hiển thị, trạng thái và `LastLoginAt`;
- `AspNetUserRoles`: vai trò `Reader`;
- `AspNetUserLogins`: tên provider, khóa định danh provider và liên kết tới tài khoản;
- `Readers`: hồ sơ độc giả, mã độc giả, loại thẻ và `LastLoginAt`.

Ứng dụng không lưu access token/refresh token (`SaveTokens = false`). Với email đã tồn
tại, chỉ ghi `AspNetUserLogins` sau khi người dùng xác minh mật khẩu LibraryHub. Những
lần đăng nhập sau dùng liên kết này và cập nhật thời gian đăng nhập, không tạo tài khoản
hoặc hồ sơ trùng.

Kiểm tra dữ liệu thật bằng script chỉ đọc, không xuất khóa provider hay dữ liệu bí mật:

```powershell
sqlcmd -S "ADMIN\SQLEXPRESS" -E -C -d "LibraryHubNext" -f 65001 -b -i "migration\07-audit-oauth-logins.sql"
```

## Trạng thái kiểm chứng

Code, build và luồng tạo/ghi database/đăng nhập lại đã được kiểm thử end-to-end bằng
provider mô phỏng trong integration test cho cả Google và Facebook. Do chưa được cung
cấp Client ID/Secret thật, trạng thái hiện tại vẫn là: **code đã hoàn thiện nhưng đăng
nhập OAuth thực tế chưa được kiểm chứng**. Không được coi kiểm thử mô phỏng hoặc việc nút
bị ẩn khi chưa cấu hình là bằng chứng đăng nhập Google/Facebook thật thành công.
