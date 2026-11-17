# LibraryHub.Next

Bản viết lại hệ thống thư viện theo modular monolith trên .NET 10, ASP.NET Core MVC,
EF Core 10/SQL Server, Identity, SignalR và API `/api/v1`.

## Vì sao vẫn dùng .NET + Razor/MVC

Hệ thống là CRUD nghiệp vụ cỡ vừa, không cần scale độc lập frontend/backend. Một
modular monolith giúp đội hiện tại phát triển nhanh, transaction mượn–trả nhất quán
và chỉ có một pipeline deploy. Razor render nhanh, ít JavaScript; các vùng cần tương
tác dùng API + SignalR. So với React/Vue + Web API, lựa chọn này giảm một repository,
một hệ auth và chi phí đồng bộ contract, nhưng vẫn giữ khả năng tách SPA sau này vì
mọi thao tác ghi đã nằm dưới `/api/v1`.

## Cấu trúc

```text
src/
  LibraryHub.Domain/          Entity, enum, invariant
  LibraryHub.Application/     Use case mượn/trả/phạt/quyền, interface
  LibraryHub.Infrastructure/  EF Core, Identity, SMTP, persistence
  LibraryHub.Web/             MVC, API v1, SignalR, worker, design system
tests/LibraryHub.UnitTests/   Test nghiệp vụ trọng yếu
docs/                         Kiến trúc, ERD, thay đổi và migration
migration/                    SQL ETL + copy upload
```

## Chạy local

Yêu cầu: .NET 10 SDK và một SQL Server local có database phát triển.

Đặt connection string local trong .NET User Secrets; không ghi nó vào
`appsettings*.json`, `.env.example` hoặc README:

```powershell
cd LibraryHub.Next
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "[CHUOI_KET_NOI_LOCAL]" --project src/LibraryHub.Web
dotnet restore
dotnet ef database update --project src/LibraryHub.Infrastructure --startup-project src/LibraryHub.Web
dotnet run --project src/LibraryHub.Web
```

ASP.NET Core ánh xạ biến môi trường `ConnectionStrings__DefaultConnection` thành
khóa cấu hình `ConnectionStrings:DefaultConnection`. Production phải cấp biến này;
ứng dụng sẽ dừng với lỗi cấu hình an toàn nếu giá trị bị thiếu hoặc rỗng.

Xem hướng dẫn tạo database, nhập dữ liệu mẫu và chuyển dữ liệu bằng file SQL tại
`docs/SQL-EXPRESS-SETUP.md`.

OAuth/SMTP chỉ được bật khi secret tồn tại. Ví dụ:

```powershell
dotnet user-secrets set "Email:Smtp:Host" "smtp.gmail.com" --project src/LibraryHub.Web
dotnet user-secrets set "Email:Smtp:Port" "587" --project src/LibraryHub.Web
dotnet user-secrets set "Authentication:Google:ClientId" "..." --project src/LibraryHub.Web
dotnet user-secrets set "Authentication:Google:ClientSecret" "..." --project src/LibraryHub.Web
dotnet user-secrets set "Email:Smtp:Username" "..." --project src/LibraryHub.Web
dotnet user-secrets set "Email:Smtp:Password" "..." --project src/LibraryHub.Web
dotnet user-secrets set "Email:Smtp:FromAddress" "..." --project src/LibraryHub.Web
dotnet user-secrets set "Email:Smtp:FromName" "LibraryHub" --project src/LibraryHub.Web
```

Xem quy trình Gmail và cách xác minh thư thật trong `docs/EMAIL-SETUP.md`.

Không seed tài khoản hoặc mật khẩu mặc định. Tạo tài khoản đầu tiên qua Identity API,
xác thực email, sau đó gán role `Administrator` bằng quy trình quản trị có kiểm soát.

## Kiểm chứng

```powershell
dotnet build LibraryHub.Next.slnx --configuration Release
dotnet test tests/LibraryHub.UnitTests/LibraryHub.UnitTests.csproj --configuration Release
dotnet test tests/LibraryHub.IntegrationTests/LibraryHub.IntegrationTests.csproj --configuration Release
```

Kết quả kiểm thử cuối và hướng dẫn demo nằm trong `docs/HANDOVER.md`.

## Di trú

1. Backup DB và `wwwroot/uploads`; thu hồi toàn bộ OAuth/SMTP secret cũ.
2. Tạo DB mới bằng EF migration.
3. Chỉnh tên DB nguồn/đích trong `migration/01-migrate-data.sql`, chạy trên bản sao.
4. Chạy các truy vấn đối soát ở cuối script.
5. Chạy `migration/02-copy-uploads.ps1` trước với `-WhatIf`, sau đó mới copy thật.
6. Thực hiện UAT và cutover theo `docs/MIGRATION.md`.

Không chạy ETL trực tiếp trên DB production ở lần đầu.
