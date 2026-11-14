using Microsoft.EntityFrameworkCore;
using SOF1022_NHOM1_SD2003_Ver2.Datas;
using SOF1022_NHOM1_SD2003_Ver2.Models;
using SOF1022_NHOM1_SD2003_Ver2.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Identity;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using ClosedXML.Excel;

namespace SOF1022_NHOM1_SD2003_Ver2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);


            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddSignalR();
            builder.Services.AddHttpClient();

            builder.Services.AddAntiforgery(options =>
            {
                options.HeaderName = "RequestVerificationToken";
            });

            // Add UTF-8 encoding support for Vietnamese characters
            builder.Services.AddSingleton<HtmlEncoder>(HtmlEncoder.Create(allowedRanges: new[] { UnicodeRanges.All }));

            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IChatbotService, ChatbotService>();
            builder.Services.AddScoped<IQuickHelpService, QuickHelpService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IPermissionService, PermissionService>();
            builder.Services.AddScoped<IRecommendationService, RecommendationService>();

            // Đăng ký LibraryDbContext vào DI Container
            builder.Services.AddDbContext<LibraryDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
            builder.Services.AddScoped<IBorrowService, BorrowService>();
            builder.Services.AddScoped<IFineCalculationService, FineCalculationService>();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddHostedService<PickupExpirationWorker>();
            builder.Services.AddHostedService<DueReminderWorker>();
            builder.Services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromHours(8);
                })
                .AddGoogle(options =>
                {
                    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
                    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
                })
                .AddFacebook(options =>
                {
                    options.AppId = builder.Configuration["Authentication:Facebook:AppId"];
                    options.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];
                });

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                SeedData.Initialize(services);
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession();

            // Request-scoped staff permission caching
            app.Use(async (context, next) =>
            {
                var session = context.Session;
                var userRole = session.GetString("Role");
                if (string.Equals(userRole, "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    // Admin has all permissions, no need to query codes
                }
                else
                {
                    var employeeIdStr = session.GetString("EmployeeId");
                    if (!string.IsNullOrEmpty(employeeIdStr) && int.TryParse(employeeIdStr, out int empId))
                    {
                        var permService = context.RequestServices.GetService<IPermissionService>();
                        if (permService != null)
                        {
                            var codes = await permService.GetPermissionCodesAsync(empId);
                            context.Items[SOF1022_NHOM1_SD2003_Ver2.Helpers.PermissionExtensions.ItemsKey] = new HashSet<string>(codes);
                        }
                    }
                }
                await next();
            });

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapHub<Hubs.SupportChatHub>("/hubs/support");
            app.MapHub<Hubs.NotificationHub>("/hubs/notification");

            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();

                context.Database.Migrate();

                var hasher = new PasswordHasher<Readers>();

                var admin = context.Readers.FirstOrDefault(x => x.Email == "admin@library.com");
                if (admin == null)
                {
                    admin = new Readers
                    {
                        ReaderCode = "ADMIN001",
                        FullName = "Admin",
                        DateOfBirth = new DateTime(1990, 1, 1),
                        Phone = "0123456789",
                        Email = "admin@library.com",
                        Address = "Hà Nội",
                        IdentityNumber = "000000000001",
                        Role = "Admin",
                        IsActive = true,
                        EmailConfirmed = true,
                        RegisterDate = DateTime.Now,
                        ExpirationDate = DateTime.Now.AddYears(10),
                        MaxBorrowBooks = 1000
                    };
                    admin.Password = hasher.HashPassword(admin, "Admin@123");
                    context.Readers.Add(admin);
                }
                else
                {
                    admin.Role = "Admin";
                    admin.IsActive = true;
                    admin.EmailConfirmed = true;
                    admin.IsVerified = true;
                    admin.Password = hasher.HashPassword(admin, "Admin@123");
                    context.Readers.Update(admin);
                }

                var reader = context.Readers.FirstOrDefault(x => x.Email == "reader@library.com");
                if (reader == null)
                {
                    reader = new Readers
                    {
                        ReaderCode = "DG001",
                        FullName = "Lê Phú An",
                        DateOfBirth = new DateTime(2004, 1, 1),
                        Phone = "0941205808",
                        Email = "reader@library.com",
                        Address = "TP Hồ Chí Minh",
                        IdentityNumber = "000000000002",
                        Role = "Reader",
                        IsActive = true,
                        EmailConfirmed = true,
                        IsVerified = true,
                        RegisterDate = DateTime.Now,
                        ExpirationDate = DateTime.Now.AddYears(1),
                        MaxBorrowBooks = 10
                    };
                    reader.Password = hasher.HashPassword(reader, "Reader@123");
                    context.Readers.Add(reader);
                }
                else
                {
                    reader.Role = "Reader";
                    reader.IsActive = true;
                    reader.EmailConfirmed = true;
                    reader.IsVerified = true;
                    reader.Password = hasher.HashPassword(reader, "Reader@123");
                    context.Readers.Update(reader);
                }

                context.SaveChanges();

                // Tự động kết xuất toàn bộ dữ liệu sách ra file lưu trữ (Excel, CSV, JSON, Markdown)
                try
                {
                    var allBooks = context.Books
                        .Include(b => b.Category)
                        .Include(b => b.Author)
                        .Include(b => b.Publisher)
                        .Include(b => b.Shelf)
                        .OrderBy(b => b.Id)
                        .ToList();

                    if (allBooks.Any())
                    {
                        var exportExcelPath = @"D:\test\danh_sach_toan_bo_sach.xlsx";
                        var exportCsvPath = @"D:\test\danh_sach_toan_bo_sach.csv";
                        var exportJsonPath = @"D:\test\danh_sach_toan_bo_sach.json";
                        var exportMdPath = @"D:\test\danh_sach_toan_bo_sach.md";

                        // 1. Xuất file Excel (.xlsx) chuẩn ClosedXML
                        using (var workbook = new XLWorkbook())
                        {
                            var ws = workbook.Worksheets.Add("DanhSachToanBoSach");
                            ws.Cell(1, 1).Value = "TỔNG HỢP TOÀN BỘ DỮ LIỆU SÁCH TRONG THƯ VIỆN";
                            ws.Range(1, 1, 1, 13).Merge().Style
                                .Font.SetBold(true)
                                .Font.SetFontSize(16)
                                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                            string[] headers = new[] {
                                "STT", "Mã sách", "ISBN", "Tên sách", "Tác giả", "Thể loại",
                                "Nhà xuất bản", "Kệ sách", "Năm XB", "Số trang", "Giá bìa (VNĐ)",
                                "Tổng số lượng", "Sẵn có"
                            };

                            for (int i = 0; i < headers.Length; i++)
                            {
                                var cell = ws.Cell(3, i + 1);
                                cell.Value = headers[i];
                                cell.Style.Font.Bold = true;
                                cell.Style.Fill.BackgroundColor = XLColor.FromArgb(40, 44, 52);
                                cell.Style.Font.FontColor = XLColor.White;
                                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            }

                            int r = 4;
                            for (int i = 0; i < allBooks.Count; i++)
                            {
                                var b = allBooks[i];
                                ws.Cell(r, 1).Value = i + 1;
                                ws.Cell(r, 2).Value = b.Id;
                                ws.Cell(r, 3).Value = b.ISBN ?? "";
                                ws.Cell(r, 4).Value = b.Title ?? "";
                                ws.Cell(r, 5).Value = b.Author?.Name ?? "";
                                ws.Cell(r, 6).Value = b.Category?.Name ?? "";
                                ws.Cell(r, 7).Value = b.Publisher?.Name ?? "";
                                ws.Cell(r, 8).Value = b.Shelf?.Name ?? "";
                                ws.Cell(r, 9).Value = b.PublishYear;
                                ws.Cell(r, 10).Value = b.PageCount;
                                ws.Cell(r, 11).Value = b.Price;
                                ws.Cell(r, 12).Value = b.Quantity;
                                ws.Cell(r, 13).Value = b.AvailableQuantity;
                                r++;
                            }

                            ws.Columns().AdjustToContents();
                            workbook.SaveAs(exportExcelPath);
                        }

                        // 2. Xuất file JSON
                        var jsonList = allBooks.Select(b => new
                        {
                            b.Id,
                            b.ISBN,
                            b.Title,
                            Author = b.Author?.Name,
                            Category = b.Category?.Name,
                            Publisher = b.Publisher?.Name,
                            Shelf = b.Shelf?.Name,
                            b.PublishYear,
                            b.PageCount,
                            b.Language,
                            b.Price,
                            b.Quantity,
                            b.AvailableQuantity,
                            Status = b.BooksStatus.ToString(),
                            b.ViewCount,
                            b.MinimumAge,
                            b.Keywords,
                            b.Description
                        }).ToList();
                        System.IO.File.WriteAllText(exportJsonPath, System.Text.Json.JsonSerializer.Serialize(jsonList, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }), System.Text.Encoding.UTF8);

                        // 3. Xuất file CSV UTF-8 có BOM
                        var csvSb = new System.Text.StringBuilder();
                        csvSb.AppendLine("STT,Mã sách,ISBN,Tên sách,Tác giả,Thể loại,Nhà xuất bản,Kệ sách,Năm XB,Số trang,Giá bìa,Tổng số lượng,Sẵn có");
                        for (int i = 0; i < allBooks.Count; i++)
                        {
                            var b = allBooks[i];
                            var safeTitle = $"\"{(b.Title ?? "").Replace("\"", "\"\"")}\"";
                            var safeAuthor = $"\"{(b.Author?.Name ?? "").Replace("\"", "\"\"")}\"";
                            var safeCat = $"\"{(b.Category?.Name ?? "").Replace("\"", "\"\"")}\"";
                            var safePub = $"\"{(b.Publisher?.Name ?? "").Replace("\"", "\"\"")}\"";
                            var safeShelf = $"\"{(b.Shelf?.Name ?? "").Replace("\"", "\"\"")}\"";
                            csvSb.AppendLine($"{i + 1},{b.Id},{b.ISBN},{safeTitle},{safeAuthor},{safeCat},{safePub},{safeShelf},{b.PublishYear},{b.PageCount},{b.Price},{b.Quantity},{b.AvailableQuantity}");
                        }
                        System.IO.File.WriteAllText(exportCsvPath, csvSb.ToString(), new System.Text.UTF8Encoding(true));

                        // 4. Xuất file Markdown
                        var mdSb = new System.Text.StringBuilder();
                        mdSb.AppendLine("# BẢNG DỮ LIỆU TOÀN BỘ SÁCH TRONG THƯ VIỆN ĐIỆN TỬ");
                        mdSb.AppendLine($"**Tổng số sách:** {allBooks.Count} cuốn | **Thời gian trích xuất:** {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
                        mdSb.AppendLine();
                        mdSb.AppendLine("| STT | Mã | ISBN | Tên sách | Tác giả | Thể loại | Nhà xuất bản | Kệ | Năm | Giá bìa (VNĐ) | Số lượng | Sẵn có |");
                        mdSb.AppendLine("|:---:|:---:|:---|:---|:---|:---|:---|:---:|:---:|---:|:---:|:---:|");
                        for (int i = 0; i < allBooks.Count; i++)
                        {
                            var b = allBooks[i];
                            mdSb.AppendLine($"| {i + 1} | {b.Id} | {b.ISBN} | {b.Title} | {b.Author?.Name} | {b.Category?.Name} | {b.Publisher?.Name} | {b.Shelf?.Name} | {b.PublishYear} | {b.Price:N0} | {b.Quantity} | {b.AvailableQuantity} |");
                        }
                        System.IO.File.WriteAllText(exportMdPath, mdSb.ToString(), System.Text.Encoding.UTF8);
                        Console.WriteLine($"[ExportBooks] Đã xuất thành công {allBooks.Count} cuốn sách ra các định dạng Excel, CSV, JSON và Markdown.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ExportBooks Warning] {ex.Message}");
                }
            }

            app.Run();
        }
    }
}