# Danh sách file thay đổi – vòng hoàn thiện xác thực và giao diện

## Domain, Application, Infrastructure

- `src/LibraryHub.Domain/DomainModel.cs`
- `src/LibraryHub.Domain/VietnameseEnumExtensions.cs`
- `src/LibraryHub.Application/BorrowService.cs`
- `src/LibraryHub.Application/Contracts.cs`
- `src/LibraryHub.Application/PermissionCatalog.cs`
- `src/LibraryHub.Application/SupportServices.cs`
- `src/LibraryHub.Infrastructure/InfrastructureServices.cs`

## Web, xác thực và API

- `src/LibraryHub.Web/Program.cs`
- `src/LibraryHub.Web/LibraryHub.Web.csproj`
- `src/LibraryHub.Web/Api.cs`
- `src/LibraryHub.Web/Realtime.cs`
- `src/LibraryHub.Web/VietnameseUi.cs`
- `src/LibraryHub.Web/Controllers/AuthController.cs`
- `src/LibraryHub.Web/Controllers/HomeController.cs`
- `src/LibraryHub.Web/Controllers/PortalViewModels.cs`

## Razor Views

- `src/LibraryHub.Web/Views/Auth/AccessDenied.cshtml`
- `src/LibraryHub.Web/Views/Auth/ForgotPassword.cshtml`
- `src/LibraryHub.Web/Views/Auth/Login.cshtml`
- `src/LibraryHub.Web/Views/Auth/Register.cshtml`
- `src/LibraryHub.Web/Views/Auth/ResetPassword.cshtml`
- `src/LibraryHub.Web/Views/Home/Index.cshtml`
- `src/LibraryHub.Web/Views/Home/Book.cshtml`
- `src/LibraryHub.Web/Views/Home/ReaderSpace.cshtml`
- `src/LibraryHub.Web/Views/Home/Admin.cshtml`
- `src/LibraryHub.Web/Views/Shared/_Layout.cshtml`
- `src/LibraryHub.Web/Views/Admin/Books.cshtml`
- `src/LibraryHub.Web/Views/Admin/Catalog.cshtml`
- `src/LibraryHub.Web/Views/Admin/Circulation.cshtml`
- `src/LibraryHub.Web/Views/Stage3/_SupportWorkspace.cshtml`
- `src/LibraryHub.Web/Views/Stage3/Employees.cshtml`
- `src/LibraryHub.Web/Views/Stage3/Rbac.cshtml`
- `src/LibraryHub.Web/Views/Stage3/Readers.cshtml`
- `src/LibraryHub.Web/Views/Stage3/Settings.cshtml`

## Design system và JavaScript

- `src/LibraryHub.Web/wwwroot/css/design-system.css`
- `src/LibraryHub.Web/wwwroot/js/app.js`
- `src/LibraryHub.Web/wwwroot/js/book-page.js`
- `src/LibraryHub.Web/wwwroot/js/reader-space.js`
- `src/LibraryHub.Web/wwwroot/js/support.js`
- `src/LibraryHub.Web/wwwroot/js/admin-rbac.js`

## Test, migration dữ liệu demo và tài liệu

- `tests/LibraryHub.IntegrationTests/LocalDbIntegrationTests.cs`
- `migration/02-seed-demo-catalog.sql`
- `migration/03-normalize-localdb-demo-content.sql`
- `work/cleanup-stage4-localdb.sql`
- `README.md`
- `docs/CHANGES.md`
- `docs/EMAIL-SETUP.md`
- `docs/FUNCTION-MATRIX.md`
- `docs/HANDOVER.md`
- `docs/TEST-DATA-CLEANUP.md`
- `docs/VISUAL-TEST-REPORT.md`
- `docs/FINAL-CHANGESET.md`
- Chín ảnh PNG trong `docs/screenshots/`.

Không có file nào trong dự án cũ được chỉnh sửa.
