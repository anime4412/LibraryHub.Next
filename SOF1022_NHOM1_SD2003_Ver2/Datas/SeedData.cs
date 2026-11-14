using Microsoft.EntityFrameworkCore;
using SOF1022_NHOM1_SD2003_Ver2.Models;

namespace SOF1022_NHOM1_SD2003_Ver2.Datas
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using var context = new LibraryDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<LibraryDbContext>>());

            context.Database.Migrate();

            SeedCategories(context);

            SeedAuthors(context);

            SeedPublishers(context);

            SeedShelves(context);

            SeedPositions(context);

            SeedEmployeeStatusOptions(context);

            SeedRolesAndPermissions(context);

            SeedBooks(context);
        }

        private static Books CreateBook(
            string isbn,
            string title,
            string description,
            int year,
            int pageCount,
            decimal price,
            string coverImage,
            int categoryId,
            int authorId,
            int publisherId,
            int shelfId)
        {
            return new Books
            {
                ISBN = isbn,
                Title = title,
                Description = description,
                Edition = "1",
                Keywords = title,
                PublishYear = year,
                CreatedBy = "SeedData",
                PageCount = pageCount,
                Language = "Tiếng Việt",
                Price = price,
                Quantity = 20,
                AvailableQuantity = 20,
                CoverImage = coverImage,
                CategoriesId = categoryId,
                AuthorsId = authorId,
                PublishersId = publisherId,
                ShelvesId = shelfId
            };
        }

        private static void SeedCategories(LibraryDbContext context)
        {
            if (context.Categories.Any())
                return;

            context.Categories.AddRange(

                new Categories
                {
                    Name = "Kỹ năng sống",
                    Description = "Phát triển bản thân"
                },

                new Categories
                {
                    Name = "Kinh doanh"
                },

                new Categories
                {
                    Name = "Công nghệ thông tin"
                },

                new Categories
                {
                    Name = "Văn học"
                },

                new Categories
                {
                    Name = "Tiểu thuyết"
                },

                new Categories
                {
                    Name = "Fantasy"
                },

                new Categories
                {
                    Name = "Khoa học"
                },

                new Categories
                {
                    Name = "Lịch sử"
                },

                new Categories
                {
                    Name = "Thiếu nhi"
                },

                new Categories
                {
                    Name = "Tâm lý học"
                },

                new Categories
                {
                    Name = "Trinh thám"
                },

                new Categories
                {
                    Name = "Lập trình"
                },   

                new Categories
                {
                    Name = "DevOps"
                },

                new Categories
                {
                    Name = "AI"
                },

                new Categories
                {
                    Name = "Machine Learning"
                },

                new Categories
                {
                    Name = "Web Development"
                },

                new Categories
                {
                    Name = "Database"
                },

                new Categories
                {
                    Name = "Microservices"
                },

                new Categories
                {
                    Name = "Architecture"
                },

                new Categories
                {
                    Name = "Lập trình hướng đối tượng"
                },

                new Categories
                {
                    Name = "Giải thuật"
                },

                new Categories
                {
                    Name = "Hệ điều hành"
                },

                new Categories
                {
                    Name = "Mạng máy tính"
                },

                new Categories
                {
                    Name = "Bảo mật"
                },

                new Categories
                {
                    Name = "Thiết kế phần mềm"
                },

                new Categories
                {
                    Name = "Quản lý dự án"
                },

                new Categories
                {
                    Name = "Kỹ năng mềm"
                },

                new Categories
                {
                    Name = "Lịch sử công nghệ"
                },

                new Categories
                {
                    Name = "Khoa học máy tính"
                },

                new Categories
                {
                    Name = "Khoa học tự nhiên"
                }
            );

            context.SaveChanges();
        }

        private static void SeedAuthors(LibraryDbContext context)
        {
            if (context.Authors.Any())
                return;

            context.Authors.AddRange(

                new Authors { Name = "Dale Carnegie", Nationality = "USA" },
                new Authors { Name = "Paulo Coelho", Nationality = "Brazil" },
                new Authors { Name = "James Clear", Nationality = "USA" },
                new Authors { Name = "Robert C. Martin", Nationality = "USA" },
                new Authors { Name = "Yuval Noah Harari", Nationality = "Israel" },
                new Authors { Name = "Robert Kiyosaki", Nationality = "USA" },
                new Authors { Name = "Napoleon Hill", Nationality = "USA" },
                new Authors { Name = "George Orwell", Nationality = "United Kingdom" },
                new Authors { Name = "J.K. Rowling", Nationality = "United Kingdom" },
                new Authors { Name = "Haruki Murakami", Nationality = "Japan" },

                new Authors { Name = "Nguyễn Nhật Ánh", Nationality = "Vietnam" },
                new Authors { Name = "Tô Hoài", Nationality = "Vietnam" },
                new Authors { Name = "Rosie Nguyễn", Nationality = "Vietnam" },

                new Authors { Name = "Stephen R. Covey", Nationality = "USA" },
                new Authors { Name = "Mark Manson", Nationality = "USA" },
                new Authors { Name = "Cal Newport", Nationality = "USA" },

                new Authors { Name = "Martin Fowler", Nationality = "United Kingdom" },
                new Authors { Name = "Eric Evans", Nationality = "USA" },

                new Authors { Name = "Kent Beck", Nationality = "USA" },
                new Authors { Name = "Donald Knuth", Nationality = "USA" },

                new Authors { Name = "Andrew Hunt", Nationality = "USA" },
                new Authors { Name = "David Thomas", Nationality = "USA" },

                new Authors { Name = "Joshua Bloch", Nationality = "USA" },
                new Authors { Name = "Steve McConnell", Nationality = "USA" },

                new Authors { Name = "Martin Kleppmann", Nationality = "United Kingdom" },

                new Authors { Name = "Erich Gamma", Nationality = "Switzerland" },
                new Authors { Name = "Richard Helm", Nationality = "Australia" },
                new Authors { Name = "Ralph Johnson", Nationality = "USA" },
                new Authors { Name = "John Vlissides", Nationality = "USA" },

                new Authors { Name = "Gene Kim", Nationality = "USA" },
                new Authors { Name = "Jez Humble", Nationality = "United Kingdom" },
                new Authors { Name = "Sam Newman", Nationality = "United Kingdom" },

                new Authors { Name = "Thomas H. Cormen", Nationality = "USA" },
                new Authors { Name = "Charles E. Leiserson", Nationality = "USA" },
                new Authors { Name = "Ronald Rivest", Nationality = "USA" },
                new Authors { Name = "Clifford Stein", Nationality = "USA" },

                new Authors { Name = "Andrew S. Tanenbaum", Nationality = "Netherlands" },
                new Authors { Name = "Herbert Schildt", Nationality = "USA" },

                new Authors { Name = "Ian Goodfellow", Nationality = "USA" },
                new Authors { Name = "Aurélien Géron", Nationality = "France" },
                new Authors { Name = "Andrew Ng", Nationality = "USA" },

                new Authors { Name = "Stephen Hawking", Nationality = "United Kingdom" },
                new Authors { Name = "Carl Sagan", Nationality = "USA" },
                new Authors { Name = "Daniel Kahneman", Nationality = "Israel" },
                new Authors { Name = "Robert Greene", Nationality = "USA" },
                new Authors { Name = "Chris Voss", Nationality = "USA" },

                new Authors { Name = "Dan Brown", Nationality = "USA" },
                new Authors { Name = "J.R.R. Tolkien", Nationality = "United Kingdom" },
                new Authors { Name = "Antoine de Saint-Exupéry", Nationality = "France" },
                new Authors { Name = "Jane Austen", Nationality = "United Kingdom" },
                new Authors { Name = "F. Scott Fitzgerald", Nationality = "USA" },

                new Authors { Name = "Jeff Sutherland", Nationality = "USA" },
                new Authors { Name = "Michael Feathers", Nationality = "USA" },
                new Authors { Name = "Kathy Sierra", Nationality = "USA" },
                new Authors { Name = "Frederick Brooks Jr.", Nationality = "USA" },
                new Authors { Name = "Seymour Cray", Nationality = "USA" },
                new Authors { Name = "Grace Hopper", Nationality = "USA" },
                new Authors { Name = "Tim Berners-Lee", Nationality = "United Kingdom" },
                new Authors { Name = "Linus Torvalds", Nationality = "USA" },
                new Authors { Name = "Dennis Ritchie", Nationality = "USA" },
                new Authors { Name = "Ken Thompson", Nationality = "USA" },
                new Authors { Name = "Bjarne Stroustrup", Nationality = "Denmark" },
                new Authors { Name = "Guido van Rossum", Nationality = "Netherlands" },
                new Authors { Name = "Brendan Eich", Nationality = "USA" },
                new Authors { Name = "Rasmus Lerdorf", Nationality = "USA" },
                new Authors { Name = "John Sonmez", Nationality = "USA" },
                new Authors { Name = "Robert Sedgewick", Nationality = "USA" },   // FIXED
                new Authors { Name = "Stuart Russell", Nationality = "United Kingdom" },
                new Authors { Name = "Eric Matthes", Nationality = "USA" },
                new Authors { Name = "Kalen Delaney", Nationality = "USA" },
                new Authors { Name = "Nigel Poulton", Nationality = "United Kingdom" },
                new Authors { Name = "Marko Luksa", Nationality = "Slovenia" },
                new Authors { Name = "Chris Richardson", Nationality = "USA" },
                new Authors { Name = "Héctor García", Nationality = "Spain" },

                // OPTIONAL SAFE EXTENSIONS (để tránh lỗi lookup nếu bạn dùng)
                new Authors { Name = "Microsoft", Nationality = "USA" },
                new Authors { Name = "GoF (Gang of Four)", Nationality = "Various" }
            );

            context.SaveChanges();
        }

        private static void SeedPublishers(LibraryDbContext context)
        {
            if (context.Publishers.Any())
                return;

            context.Publishers.AddRange(

                new Publishers
                {
                    Name = "NXB Trẻ"
                },

                new Publishers
                {
                    Name = "NXB Kim Đồng"
                },

                new Publishers
                {
                    Name = "NXB Giáo Dục"
                },

                new Publishers
                {
                    Name = "NXB Lao Động"
                },

                new Publishers
                {
                    Name = "NXB Văn Học"
                },

                new Publishers
                {
                    Name = "NXB Hội Nhà Văn"
                },

                new Publishers
                {
                    Name = "NXB Tổng Hợp"
                },

                new Publishers
                {
                    Name = "NXB Thế Giới"
                },

                new Publishers
                {
                    Name = "O'Reilly"
                },

                new Publishers
                {
                    Name = "Packt"
                },

                new Publishers
                {
                    Name = "Manning"
                },

                new Publishers
                {
                    Name = "Pearson"
                },

                new Publishers
                {
                    Name = "Addison Wesley"
                },

                new Publishers
                {
                    Name = "Penguin"
                },

                new Publishers
                {
                    Name = "Harper Collins"
                },

                new Publishers 
                { 
                    Name = "Coursera Press" 
                },

                new Publishers 
                {
                    Name = "Independently Published" 
                },

                new Publishers
                {
                    Name = "Microsoft Press"
                },

                new Publishers
                {
                    Name = "Prentice Hall"
                },

                new Publishers
                {
                    Name = "No Starch Press"
                },

                new Publishers
                {
                    Name = "IT Revolution"
                },

                new Publishers
                {
                    Name = "O'Reilly Media"
                },

                new Publishers
                {
                    Name = "Apress"
                },

                new Publishers
                {
                    Name = "Wiley"
                },

                new Publishers
                {
                    Name = "Springer"
                },

                new Publishers
                {
                    Name = "Cambridge University Press"
                },  

                new Publishers
                {
                    Name = "MIT Press"
                },

                new Publishers
                {
                    Name = "Pearson Education"
                },

                new Publishers
                {
                    Name = "Bantam"
                },

                new Publishers
                {
                    Name = "Scribner"
                },

                new Publishers
                {
                    Name = "Doubleday"
                }
            );

            context.SaveChanges();
        }

        private static void SeedShelves(LibraryDbContext context)
        {
            if (context.Shelves.Any())
                return;

            context.Shelves.AddRange(

                new Shelves
                {
                    Code = "A1",
                    Name = "Kệ A1",
                    Area = "Tầng 1"
                },

                new Shelves
                {
                    Code = "A2",
                    Name = "Kệ A2",
                    Area = "Tầng 1"
                },

                new Shelves
                {
                    Code = "A3",
                    Name = "Kệ A3",
                    Area = "Tầng 1"
                },

                new Shelves
                {
                    Code = "B1",
                    Name = "Kệ B1",
                    Area = "Tầng 2"
                },

                new Shelves
                {
                    Code = "B2",
                    Name = "Kệ B2",
                    Area = "Tầng 2"
                },

                new Shelves
                {
                    Code = "B3",
                    Name = "Kệ B3",
                    Area = "Tầng 2"
                },

                new Shelves
                {
                    Code = "C1",
                    Name = "Kệ C1",
                    Area = "Tầng 3"
                },

                new Shelves
                {
                    Code = "C2",
                    Name = "Kệ C2",
                    Area = "Tầng 3"
                },

                new Shelves
                {
                    Code = "C3",
                    Name = "Kệ C3",
                    Area = "Tầng 3"
                },

                new Shelves
                {
                    Code = "D1",
                    Name = "Kệ D1",
                    Area = "Tầng 4"
                },

                new Shelves
                {
                    Code = "D2",
                    Name = "Kệ D2",
                    Area = "Tầng 4"
                },

                new Shelves
                {
                    Code = "D3",
                    Name = "Kệ D3",
                    Area = "Tầng 4"
                },

                new Shelves
                {
                    Code = "E1",
                    Name = "Kệ E1",
                    Area = "Tầng 5"
                },

                new Shelves
                {
                    Code = "E2",
                    Name = "Kệ E2",
                    Area = "Tầng 5"
                },

                new Shelves
                {
                    Code = "E3",
                    Name = "Kệ E3",
                    Area = "Tầng 5"
                },

                new Shelves
                {
                    Code = "F1",
                    Name = "Kệ F1",
                    Area = "Tầng 6"
                },

                new Shelves
                {
                    Code = "F2",
                    Name = "Kệ F2",
                    Area = "Tầng 6"
                },

                new Shelves
                {
                    Code = "F3",
                    Name = "Kệ F3",
                    Area = "Tầng 6"
                },

                new Shelves
                {
                    Code = "G1",
                    Name = "Kệ G1",
                    Area = "Tầng 7"
                },

                new Shelves
                {
                    Code = "G2",
                    Name = "Kệ G2",
                    Area = "Tầng 7"
                },

                new Shelves
                {
                    Code = "G3",
                    Name = "Kệ G3",
                    Area = "Tầng 7"
                },

                new Shelves
                {
                    Code = "H1",
                    Name = "Kệ H1",
                    Area = "Tầng 8"
                },

                new Shelves
                {
                    Code = "H2",
                    Name = "Kệ H2",
                    Area = "Tầng 8"
                },

                new Shelves
                {
                    Code = "H3",
                    Name = "Kệ H3",
                    Area = "Tầng 8"
                },

                new Shelves
                {
                    Code = "I1",
                    Name = "Kệ I1",
                    Area = "Tầng 9"
                },

                new Shelves
                {
                    Code = "I2",
                    Name = "Kệ I2",
                    Area = "Tầng 9"
                },

                new Shelves
                {
                    Code = "I3",
                    Name = "Kệ I3",
                    Area = "Tầng 9"
                },

                new Shelves
                {
                    Code = "J1",
                    Name = "Kệ J1",
                    Area = "Tầng 10"
                },

                new Shelves
                {
                    Code = "J2",
                    Name = "Kệ J2",
                    Area = "Tầng 10"
                },

                new Shelves
                {
                    Code = "J3",
                    Name = "Kệ J3",
                    Area = "Tầng 10"
                },

                new Shelves
                {
                    Code = "K1",
                    Name = "Kệ K1",
                    Area = "Tầng 11"
                },

                new Shelves
                {
                    Code = "K2",
                    Name = "Kệ K2",
                    Area = "Tầng 11"
                },

                new Shelves
                {
                    Code = "K3",
                    Name = "Kệ K3",
                    Area = "Tầng 11"
                },

                new Shelves
                {
                    Code = "L1",
                    Name = "Kệ L1",
                    Area = "Tầng 12"
                },

                new Shelves
                {
                    Code = "L2",
                    Name = "Kệ L2",
                    Area = "Tầng 12"
                },

                new Shelves
                {
                    Code = "L3",
                    Name = "Kệ L3",
                    Area = "Tầng 12"
                }

            );

            context.SaveChanges();
        }

        private static void SeedPositions(LibraryDbContext context)
        {
            if (context.Positions.Any())
                return;

            context.Positions.AddRange(
                new Position
                {
                    Name = "Admin",
                    Description = "Quản trị viên hệ thống",
                    CanManageBooks = true,
                    CanManageUsers = true,
                    CanManageLoans = true,
                    CanManageReports = true,
                    CanManageSystem = true
                },
                new Position
                {
                    Name = "Thủ thư",
                    Description = "Quản lý sách và kho",
                    CanManageBooks = true,
                    CanManageUsers = false,
                    CanManageLoans = true,
                    CanManageReports = false,
                    CanManageSystem = false
                },
                new Position
                {
                    Name = "Nhân viên mượn trả",
                    Description = "Xử lý giao dịch mượn trả sách",
                    CanManageBooks = false,
                    CanManageUsers = true,
                    CanManageLoans = true,
                    CanManageReports = false,
                    CanManageSystem = false
                }
            );

            context.SaveChanges();
        }

        private static void SeedEmployeeStatusOptions(LibraryDbContext context)
        {
            if (context.EmployeeStatusOptions.Any())
                return;

            context.EmployeeStatusOptions.AddRange(
                new EmployeeStatusOption
                {
                    Name = "Đang làm việc",
                    Description = "Nhân viên đang hoạt động bình thường",
                    DisplayOrder = 1
                },
                new EmployeeStatusOption
                {
                    Name = "Tạm ngưng",
                    Description = "Nhân viên tạm thời nghỉ việc",
                    DisplayOrder = 2
                },
                new EmployeeStatusOption
                {
                    Name = "Đã nghỉ việc",
                    Description = "Nhân viên đã thôi việc",
                    DisplayOrder = 3
                }
            );

            context.SaveChanges();
        }

        private static void SeedRolesAndPermissions(LibraryDbContext context)
        {
            if (context.Roles.Any())
                return;

            // Seed Permissions - 14 modules đầy đủ
            var permissions = new List<Permission>
            {
                //  Tổng quan (Dashboard)
                new Permission { Code = "dashboard.view", DisplayName = "Xem tổng quan", GroupName = "Tổng quan" },
                
                //  Quản lý sách
                new Permission { Code = "book.view", DisplayName = "Xem sách", GroupName = "Quản lý sách" },
                new Permission { Code = "book.create", DisplayName = "Thêm sách", GroupName = "Quản lý sách" },
                new Permission { Code = "book.edit", DisplayName = "Sửa sách", GroupName = "Quản lý sách" },
                new Permission { Code = "book.delete", DisplayName = "Xóa sách", GroupName = "Quản lý sách" },
                
                //  Danh mục sách
                new Permission { Code = "category.view", DisplayName = "Xem danh mục", GroupName = "Danh mục sách" },
                new Permission { Code = "category.create", DisplayName = "Thêm danh mục", GroupName = "Danh mục sách" },
                new Permission { Code = "category.edit", DisplayName = "Sửa danh mục", GroupName = "Danh mục sách" },
                new Permission { Code = "category.delete", DisplayName = "Xóa danh mục", GroupName = "Danh mục sách" },
                
                // ️ Tác giả
                new Permission { Code = "author.view", DisplayName = "Xem tác giả", GroupName = "Tác giả" },
                new Permission { Code = "author.create", DisplayName = "Thêm tác giả", GroupName = "Tác giả" },
                new Permission { Code = "author.edit", DisplayName = "Sửa tác giả", GroupName = "Tác giả" },
                new Permission { Code = "author.delete", DisplayName = "Xóa tác giả", GroupName = "Tác giả" },
                
                //  Nhà xuất bản
                new Permission { Code = "publisher.view", DisplayName = "Xem nhà xuất bản", GroupName = "Nhà xuất bản" },
                new Permission { Code = "publisher.create", DisplayName = "Thêm nhà xuất bản", GroupName = "Nhà xuất bản" },
                new Permission { Code = "publisher.edit", DisplayName = "Sửa nhà xuất bản", GroupName = "Nhà xuất bản" },
                new Permission { Code = "publisher.delete", DisplayName = "Xóa nhà xuất bản", GroupName = "Nhà xuất bản" },
                
                // ️ Kệ sách
                new Permission { Code = "shelf.view", DisplayName = "Xem kệ sách", GroupName = "Kệ sách" },
                new Permission { Code = "shelf.create", DisplayName = "Thêm kệ sách", GroupName = "Kệ sách" },
                new Permission { Code = "shelf.edit", DisplayName = "Sửa kệ sách", GroupName = "Kệ sách" },
                new Permission { Code = "shelf.delete", DisplayName = "Xóa kệ sách", GroupName = "Kệ sách" },
                
                //  Độc giả
                new Permission { Code = "reader.view", DisplayName = "Xem độc giả", GroupName = "Độc giả" },
                new Permission { Code = "reader.create", DisplayName = "Thêm độc giả", GroupName = "Độc giả" },
                new Permission { Code = "reader.edit", DisplayName = "Sửa độc giả", GroupName = "Độc giả" },
                new Permission { Code = "reader.delete", DisplayName = "Xóa độc giả", GroupName = "Độc giả" },
                
                //  Nhân viên
                new Permission { Code = "employee.view", DisplayName = "Xem nhân viên", GroupName = "Nhân viên" },
                new Permission { Code = "employee.create", DisplayName = "Thêm nhân viên", GroupName = "Nhân viên" },
                new Permission { Code = "employee.edit", DisplayName = "Sửa nhân viên", GroupName = "Nhân viên" },
                new Permission { Code = "employee.delete", DisplayName = "Xóa nhân viên", GroupName = "Nhân viên" },
                new Permission { Code = "employee.assign_role", DisplayName = "Phân quyền nhân viên", GroupName = "Nhân viên" },
                
                //  Phiếu mượn sách
                new Permission { Code = "borrow.view", DisplayName = "Xem phiếu mượn", GroupName = "Phiếu mượn sách" },
                new Permission { Code = "borrow.create", DisplayName = "Tạo phiếu mượn", GroupName = "Phiếu mượn sách" },
                new Permission { Code = "borrow.edit", DisplayName = "Sửa phiếu mượn", GroupName = "Phiếu mượn sách" },
                new Permission { Code = "borrow.delete", DisplayName = "Xóa phiếu mượn", GroupName = "Phiếu mượn sách" },
                new Permission { Code = "borrow.confirm_return", DisplayName = "Xác nhận trả", GroupName = "Phiếu mượn sách" },
                
                //  Đơn đặt trước
                new Permission { Code = "reservation.view", DisplayName = "Xem đơn đặt trước", GroupName = "Đơn đặt trước" },
                new Permission { Code = "reservation.create", DisplayName = "Tạo đơn đặt trước", GroupName = "Đơn đặt trước" },
                new Permission { Code = "reservation.edit", DisplayName = "Sửa đơn đặt trước", GroupName = "Đơn đặt trước" },
                new Permission { Code = "reservation.delete", DisplayName = "Xóa đơn đặt trước", GroupName = "Đơn đặt trước" },
                new Permission { Code = "reservation.confirm", DisplayName = "Xác nhận đặt trước", GroupName = "Đơn đặt trước" },
                new Permission { Code = "reservation.cancel", DisplayName = "Hủy đặt trước", GroupName = "Đơn đặt trước" },
                
                //  Quản lý phạt tiền
                new Permission { Code = "fine.view", DisplayName = "Xem phạt", GroupName = "Quản lý phạt tiền" },
                new Permission { Code = "fine.create", DisplayName = "Tạo phiếu phạt", GroupName = "Quản lý phạt tiền" },
                new Permission { Code = "fine.edit", DisplayName = "Sửa phiếu phạt", GroupName = "Quản lý phạt tiền" },
                new Permission { Code = "fine.delete", DisplayName = "Xóa phiếu phạt", GroupName = "Quản lý phạt tiền" },
                new Permission { Code = "fine.process_payment", DisplayName = "Xử lý thanh toán", GroupName = "Quản lý phạt tiền" },
                
                //  Thông báo
                new Permission { Code = "notification.view", DisplayName = "Xem thông báo", GroupName = "Thông báo" },
                new Permission { Code = "notification.create", DisplayName = "Tạo thông báo", GroupName = "Thông báo" },
                new Permission { Code = "notification.edit", DisplayName = "Sửa thông báo", GroupName = "Thông báo" },
                new Permission { Code = "notification.delete", DisplayName = "Xóa thông báo", GroupName = "Thông báo" },
                new Permission { Code = "notification.send", DisplayName = "Gửi thông báo", GroupName = "Thông báo" },
                
                //  Hỗ trợ
                new Permission { Code = "support.view", DisplayName = "Xem yêu cầu hỗ trợ", GroupName = "Hỗ trợ" },
                new Permission { Code = "support.create", DisplayName = "Tạo yêu cầu hỗ trợ", GroupName = "Hỗ trợ" },
                new Permission { Code = "support.edit", DisplayName = "Sửa yêu cầu hỗ trợ", GroupName = "Hỗ trợ" },
                new Permission { Code = "support.delete", DisplayName = "Xóa yêu cầu hỗ trợ", GroupName = "Hỗ trợ" },
                new Permission { Code = "support.reply", DisplayName = "Phản hồi yêu cầu", GroupName = "Hỗ trợ" },
                
                //  Trang độc giả
                new Permission { Code = "reader_page.view", DisplayName = "Xem trang độc giả", GroupName = "Trang độc giả" },
                new Permission { Code = "reader_page.borrow", DisplayName = "Đặt mượn sách", GroupName = "Trang độc giả" },
                new Permission { Code = "reader_page.comment", DisplayName = "Bình luận sách", GroupName = "Trang độc giả" },
                new Permission { Code = "reader_page.favorite", DisplayName = "Yêu thích sách", GroupName = "Trang độc giả" }
            };

            context.Permissions.AddRange(permissions);
            context.SaveChanges();

            // Seed Roles
            var adminRole = new Role { Name = "Admin", Description = "Quản trị viên hệ thống - Đủ mọi quyền" };
            var quanLyThuVienRole = new Role { Name = "Quản lý thư viện", Description = "Quản lý toàn bộ hoạt động thư viện" };
            var thuThuRole = new Role { Name = "Thủ thư", Description = "Quản lý sách và kho" };
            var nhanVienKhoRole = new Role { Name = "Nhân viên kho", Description = "Quản lý nhập xuất kho" };
            var chamSocKhachHangRole = new Role { Name = "Chăm sóc khách hàng", Description = "Hỗ trợ độc giả và xử lý mượn trả" };

            context.Roles.AddRange(adminRole, quanLyThuVienRole, thuThuRole, nhanVienKhoRole, chamSocKhachHangRole);
            context.SaveChanges();

            // Seed RolePermissions
            var permissionDict = permissions.ToDictionary(p => p.Code, p => p.Id);

            // Admin: Tất cả quyền
            var adminPermissions = permissions.Select(p => new RolePermission
            {
                RoleId = adminRole.Id,
                PermissionId = p.Id
            });
            context.RolePermissions.AddRange(adminPermissions);

            // Quản lý thư viện: Hầu hết quyền trừ cấu hình hệ thống
            var quanLyThuVienPermissions = new List<RolePermission>
            {
                // Tổng quan
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["dashboard.view"] },
                // Quản lý sách
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["book.view"] },
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["book.create"] },
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["book.edit"] },
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["book.delete"] },
                // Danh mục sách
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["category.view"] },
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["category.create"] },
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["category.edit"] },
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["category.delete"] },
                // Tác giả
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["author.view"] },
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["author.create"] },
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["author.edit"] },
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["author.delete"] },
                // Nhà xuất bản
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["publisher.view"] },
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["publisher.create"] },
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["publisher.edit"] },
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["publisher.delete"] },
                // Kệ sách
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["shelf.view"] },
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["shelf.create"] },
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["shelf.edit"] },
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["shelf.delete"] },
                // Độc giả
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["reader.view"] },
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["reader.create"] },
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["reader.edit"] },
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["reader.delete"] },
                // Nhân viên
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["employee.view"] },
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["employee.create"] },
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["employee.edit"] },
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["employee.delete"] },
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["employee.assign_role"] },
                // Phiếu mượn
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["borrow.view"] },
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["borrow.create"] },
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["borrow.edit"] },
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["borrow.delete"] },
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["borrow.confirm_return"] },
                // Đặt trước
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["reservation.view"] },
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["reservation.create"] },
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["reservation.edit"] },
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["reservation.delete"] },
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["reservation.confirm"] },
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["reservation.cancel"] },
                // Phạt
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["fine.view"] },
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["fine.create"] },
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["fine.edit"] },
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["fine.delete"] },
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["fine.process_payment"] },
                // Thông báo
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["notification.view"] },
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["notification.create"] },
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["notification.edit"] },
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["notification.delete"] },
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["notification.send"] },
                // Hỗ trợ
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["support.view"] },
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["support.create"] },
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["support.edit"] },
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["support.delete"] },
                new RolePermission { RoleId = quanLyThuVienRole.Id, PermissionId = permissionDict["support.reply"] }
            };
            context.RolePermissions.AddRange(quanLyThuVienPermissions);

            // Thủ thư: Quản lý sách, Danh mục, Tác giả, Nhà xuất bản, Kệ sách
            var thuThuPermissions = new List<RolePermission>
            {
                // Tổng quan
                new RolePermission { RoleId = thuThuRole.Id, PermissionId = permissionDict["dashboard.view"] },
                // Quản lý sách
                new RolePermission { RoleId = thuThuRole.Id, PermissionId = permissionDict["book.view"] },
                new RolePermission { RoleId = thuThuRole.Id, PermissionId = permissionDict["book.create"] },
                new RolePermission { RoleId = thuThuRole.Id, PermissionId = permissionDict["book.edit"] },
                new RolePermission { RoleId = thuThuRole.Id, PermissionId = permissionDict["book.delete"] },
                // Danh mục sách
                new RolePermission { RoleId = thuThuRole.Id, PermissionId = permissionDict["category.view"] },
                new RolePermission { RoleId = thuThuRole.Id, PermissionId = permissionDict["category.create"] },
                new RolePermission { RoleId = thuThuRole.Id, PermissionId = permissionDict["category.edit"] },
                new RolePermission { RoleId = thuThuRole.Id, PermissionId = permissionDict["category.delete"] },
                // Tác giả
                new RolePermission { RoleId = thuThuRole.Id, PermissionId = permissionDict["author.view"] },
                new RolePermission { RoleId = thuThuRole.Id, PermissionId = permissionDict["author.create"] },
                new RolePermission { RoleId = thuThuRole.Id, PermissionId = permissionDict["author.edit"] },
                new RolePermission { RoleId = thuThuRole.Id, PermissionId = permissionDict["author.delete"] },
                // Nhà xuất bản
                new RolePermission { RoleId = thuThuRole.Id, PermissionId = permissionDict["publisher.view"] },
                new RolePermission { RoleId = thuThuRole.Id, PermissionId = permissionDict["publisher.create"] },
                new RolePermission { RoleId = thuThuRole.Id, PermissionId = permissionDict["publisher.edit"] },
                new RolePermission { RoleId = thuThuRole.Id, PermissionId = permissionDict["publisher.delete"] },
                // Kệ sách
                new RolePermission { RoleId = thuThuRole.Id, PermissionId = permissionDict["shelf.view"] },
                new RolePermission { RoleId = thuThuRole.Id, PermissionId = permissionDict["shelf.create"] },
                new RolePermission { RoleId = thuThuRole.Id, PermissionId = permissionDict["shelf.edit"] },
                new RolePermission { RoleId = thuThuRole.Id, PermissionId = permissionDict["shelf.delete"] }
            };
            context.RolePermissions.AddRange(thuThuPermissions);

            // Nhân viên kho: Nhập xuất kho, quản lý tồn kho
            var nhanVienKhoPermissions = new List<RolePermission>
            {
                // Tổng quan
                new RolePermission { RoleId = nhanVienKhoRole.Id, PermissionId = permissionDict["dashboard.view"] },
                // Quản lý sách (chỉ xem và sửa số lượng)
                new RolePermission { RoleId = nhanVienKhoRole.Id, PermissionId = permissionDict["book.view"] },
                new RolePermission { RoleId = nhanVienKhoRole.Id, PermissionId = permissionDict["book.edit"] },
                // Kệ sách
                new RolePermission { RoleId = nhanVienKhoRole.Id, PermissionId = permissionDict["shelf.view"] },
                new RolePermission { RoleId = nhanVienKhoRole.Id, PermissionId = permissionDict["shelf.create"] },
                new RolePermission { RoleId = nhanVienKhoRole.Id, PermissionId = permissionDict["shelf.edit"] },
                new RolePermission { RoleId = nhanVienKhoRole.Id, PermissionId = permissionDict["shelf.delete"] },
                // Phiếu mượn (chỉ xem để kiểm tra tồn kho)
                new RolePermission { RoleId = nhanVienKhoRole.Id, PermissionId = permissionDict["borrow.view"] }
            };
            context.RolePermissions.AddRange(nhanVienKhoPermissions);

            // Chăm sóc khách hàng: Mượn trả, Đặt trước, Độc giả, Hỗ trợ
            var chamSocKhachHangPermissions = new List<RolePermission>
            {
                // Tổng quan
                new RolePermission { RoleId = chamSocKhachHangRole.Id, PermissionId = permissionDict["dashboard.view"] },
                // Độc giả
                new RolePermission { RoleId = chamSocKhachHangRole.Id, PermissionId = permissionDict["reader.view"] },
                new RolePermission { RoleId = chamSocKhachHangRole.Id, PermissionId = permissionDict["reader.create"] },
                new RolePermission { RoleId = chamSocKhachHangRole.Id, PermissionId = permissionDict["reader.edit"] },
                // Phiếu mượn
                new RolePermission { RoleId = chamSocKhachHangRole.Id, PermissionId = permissionDict["borrow.view"] },
                new RolePermission { RoleId = chamSocKhachHangRole.Id, PermissionId = permissionDict["borrow.create"] },
                new RolePermission { RoleId = chamSocKhachHangRole.Id, PermissionId = permissionDict["borrow.confirm_return"] },
                // Đặt trước
                new RolePermission { RoleId = chamSocKhachHangRole.Id, PermissionId = permissionDict["reservation.view"] },
                new RolePermission { RoleId = chamSocKhachHangRole.Id, PermissionId = permissionDict["reservation.confirm"] },
                new RolePermission { RoleId = chamSocKhachHangRole.Id, PermissionId = permissionDict["reservation.cancel"] },
                // Phạt (chỉ xem)
                new RolePermission { RoleId = chamSocKhachHangRole.Id, PermissionId = permissionDict["fine.view"] },
                // Hỗ trợ
                new RolePermission { RoleId = chamSocKhachHangRole.Id, PermissionId = permissionDict["support.view"] },
                new RolePermission { RoleId = chamSocKhachHangRole.Id, PermissionId = permissionDict["support.create"] },
                new RolePermission { RoleId = chamSocKhachHangRole.Id, PermissionId = permissionDict["support.reply"] },
                // Thông báo
                new RolePermission { RoleId = chamSocKhachHangRole.Id, PermissionId = permissionDict["notification.view"] },
                new RolePermission { RoleId = chamSocKhachHangRole.Id, PermissionId = permissionDict["notification.send"] }
            };
            context.RolePermissions.AddRange(chamSocKhachHangPermissions);

            context.SaveChanges();
        }

        private static void SeedBooks(LibraryDbContext context)
        {
            if (context.Books.Any())
                return;

            var categories = context.Categories.ToDictionary(x => x.Name, x => x.Id);
            var authors = context.Authors.ToDictionary(x => x.Name, x => x.Id);
            var publishers = context.Publishers.ToDictionary(x => x.Name, x => x.Id);
            var shelves = context.Shelves.ToDictionary(x => x.Code, x => x.Id);

            Console.WriteLine("=== AUTHORS ===");
            foreach (var a in context.Authors)
            {
                Console.WriteLine(a.Name);
            }

            var books = new List<Books>()
            {
                CreateBook("978604000001","Đắc Nhân Tâm","Đắc Nhân Tâm (How to Win Friends and Influence People) của Dale Carnegie là cuốn sách kinh điển và bán chạy nhất mọi thời đại về nghệ thuật đối nhân xử thế và giao tiếp ứng xử. Tác phẩm mang đến những nguyên tắc vàng vượt thời gian để kết nối chân thành với người khác, thấu hiểu tâm lý con người, tạo dựng lòng tin và dẫn dắt người khác bằng sự tôn trọng và tình yêu thương.",1936,320,120000,"/images/books/dac-nhan-tam.jpg",
                    categories["Kỹ năng sống"], authors["Dale Carnegie"], publishers["NXB Trẻ"], shelves["A1"]),

                CreateBook("978604000002","Nhà Giả Kim","Nhà Giả Kim (The Alchemist) là kiệt tác văn học thế giới của nhà văn Paulo Coelho, kể về hành trình phiêu lưu đầy huyền thoại của chàng chăn cừu Santiago đi tìm kho báu bí ẩn tại Kim Tự Tháp Ai Cập. Cuốn sách gửi gắm thông điệp sâu sắc: Khi bạn thực sự khao khát một điều gì, cả vũ trụ sẽ hợp lực giúp bạn đạt được điều đó.",1988,228,99000,"/images/books/nha-gia-kim.jpg",
                    categories["Tiểu thuyết"], authors["Paulo Coelho"], publishers["NXB Tổng Hợp"], shelves["A2"]),

                CreateBook("978604000003","Atomic Habits","Atomic Habits (Thay Đổi Tí Hon, Hiệu Quả Bất Ngờ) của James Clear phân tích sâu sắc khoa học về hành vi con người và đưa ra phương pháp 4 bước để xây dựng thói quen tốt và từ bỏ thói quen xấu: Làm cho rõ ràng, Hấp dẫn, Dễ dàng và Thỏa mãn. Chỉ với 1% thay đổi tích cực mỗi ngày, bạn sẽ tạo nên sự chuyển biến phi thường trong cuộc sống.",2018,320,185000,"/images/books/atomic-habits.jpg",
                    categories["Kỹ năng sống"], authors["James Clear"], publishers["Packt"], shelves["A3"]),

                CreateBook("978604000004","Clean Code","Clean Code (Mã Sạch) của Robert C. Martin là cẩm nang gối đầu giường của mọi kỹ sư phần mềm chuyên nghiệp. Tác phẩm hướng dẫn chi tiết cách đặt tên biến, viết hàm ngắn gọn, tổ chức lớp đối tượng, xử lý lỗi và tái cấu trúc mã nguồn sao cho dễ đọc, dễ bảo trì và có tính mở rộng cao.",2008,464,420000,"/images/books/clean-code.jpg",
                    categories["Công nghệ thông tin"], authors["Robert C. Martin"], publishers["Addison Wesley"], shelves["B1"]),

                CreateBook("978604000005","Sapiens","Sapiens: Lược sử loài người của Yuval Noah Harari đưa độc giả qua hành trình tiến hóa kỳ diệu và đầy kịch tính của loài người từ thời kỳ đồ đá cho đến kỷ nguyên hiện đại. Tác phẩm phân tích sâu sắc cách mà khả năng tư duy trừu tượng, các trật tự tưởng tượng, cuộc cách mạng nông nghiệp và khoa học đã đưa Homo sapiens trở thành loài thống trị hành tinh.",2011,512,250000,"/images/books/sapiens.jpg",
                    categories["Khoa học"], authors["Yuval Noah Harari"], publishers["Packt"], shelves["B2"]),

                CreateBook("978604000006","Cha Giàu Cha Nghèo","Cha Giàu Cha Nghèo (Rich Dad Poor Dad) của Robert Kiyosaki đã làm thay đổi tư duy tài chính của hàng triệu người trên thế giới. Cuốn sách so sánh sự khác biệt trong tư duy về tiền bạc giữa người cha nghèo (chú trọng làm công ăn lương an toàn) và người cha giàu (chú trọng đầu tư và sở hữu tài sản sinh lời), mang lại bài học quý giá về tự do tài chính.",1997,320,150000,"/images/books/cha-giau-cha-ngheo.jpg",
                    categories["Kinh doanh"], authors["Robert Kiyosaki"], publishers["NXB Trẻ"], shelves["A2"]),

                CreateBook("978604000007","Think and Grow Rich","Think and Grow Rich (13 Nguyên Tắc Nghĩ Giàu Làm Giàu) của Napoleon Hill là kết quả sau hơn 20 năm nghiên cứu cuộc đời và sự nghiệp của 500 cá nhân thành công nhất nước Mỹ. Cuốn sách đúc kết 13 nguyên tắc làm chủ tư duy, biến khát vọng cháy bỏng thành của cải và thành công vượt bậc.",1937,340,145000,"/images/books/think-and-grow-rich.jpg",
                    categories["Kinh doanh"], authors["Napoleon Hill"], publishers["NXB Hội Nhà Văn"], shelves["A2"]),

                CreateBook("978604000008","1984","1984 là kiệt tác văn học kinh điển của George Orwell khắc họa một xã hội tương lai giả tưởng đặt dưới sự giám sát toàn năng của Anh Cả (Big Brother). Tác phẩm cảnh báo sâu sắc về sự thao túng thông tin, bóp méo lịch sử, xâm phạm tự do cá nhân và sức mạnh kiểm soát tâm lý của các thể chế toàn trị.",1949,328,170000,"/images/books/1984.jpg",
                    categories["Văn học"], authors["George Orwell"], publishers["Packt"], shelves["B1"]),

                CreateBook("978604000009","Harry Potter và Hòn đá Phù thủy","Harry Potter và Hòn đá Phù thủy là tập đầu tiên mở ra thế giới phù thủy kỳ diệu của J.K. Rowling. Cậu bé mồ côi Harry Potter khám phá ra thân thế thực sự của mình vào sinh nhật 11 tuổi, bước vào Trường Phù thủy và Pháp sư Hogwarts, kết thân với Ron và Hermione, và cùng nhau bảo vệ Hòn đá Phù thủy khỏi bàn tay của kẻ thù hắc ám.",1997,350,180000,"/images/books/harry-potter-1.jpg",
                    categories["Fantasy"], authors["J.K. Rowling"], publishers["Packt"], shelves["B2"]),

                CreateBook("978604000010","Harry Potter và Phòng chứa Bí mật","Harry Potter và Phòng chứa Bí mật ghi lại năm học thứ hai đầy bí ẩn và nguy hiểm tại Hogwarts. Khi những dòng chữ nhuốm máu xuất hiện trên tường và các học sinh lần lượt bị hóa đá, Harry phải đối mặt với tiếng gọi của ngôn ngữ rắn, cuốn nhật ký ma thuật của Tom Riddle và quái vật khổng lồ ẩn nấp sâu dưới lòng đất.",1998,360,180000,"/images/books/harry-potter-2.jpg",
                    categories["Fantasy"], authors["J.K. Rowling"], publishers["Packt"], shelves["B2"]),

                CreateBook("978604000011","Harry Potter và Tù nhân Azkaban","Harry Potter và Tù nhân Azkaban đưa Harry vào năm học thứ ba với nỗi khiếp sợ về Sirius Black – tên tù nhân nguy hiểm vừa đào thoát khỏi ngục Azkaban. Tuy nhiên, chuyến hành trình đã hé lộ những bí mật cảm động về tình bạn tri kỷ của thế hệ trước, nguồn gốc tấm Bản đồ Đạo tặc và sự thật đằng sau cái chết của cha mẹ Harry.",1999,420,190000,"/images/books/harry-potter-3.jpg",
                    categories["Fantasy"], authors["J.K. Rowling"], publishers["Packt"], shelves["B2"]),

                CreateBook("978604000012","Harry Potter và Chiếc cốc lửa","Harry Potter và Chiếc cốc lửa cuốn hút độc giả vào Giải đấu Tam Pháp Thuật nghẹt thở quy tụ học sinh từ ba trường phù thủy danh tiếng. Bị cuốn vào thử thách sinh tử dù chưa đủ tuổi, Harry phải vượt qua rồng lửa, người cá và mê cung ma quái, trước khi đối mặt với sự hồi sinh đen tối của Chúa tể Voldemort.",2000,640,230000,"/images/books/harry-potter-4.jpg",
                    categories["Fantasy"], authors["J.K. Rowling"], publishers["Packt"], shelves["B2"]),

                CreateBook("978604000013","Harry Potter và Hội Phượng Hoàng","Harry Potter và Hội Phượng Hoàng đánh dấu giai đoạn đen tối và căng thẳng khi Bộ Pháp Thuật phủ nhận sự trở lại của Voldemort và tìm cách kiểm soát trường học. Harry cùng các bạn thành lập Đoàn quân Dumbledore bí mật để rèn luyện phép phòng thủ, chuẩn bị cho cuộc chiến lớn tại Sở Bí Mật.",2003,760,260000,"/images/books/harry-potter-5.jpg",
                    categories["Fantasy"], authors["J.K. Rowling"], publishers["Packt"], shelves["B3"]),

                CreateBook("978604000014","Harry Potter và Hoàng tử lai","Harry Potter và Hoàng tử lai cùng Thầy Dumbledore dẫn dắt người đọc đi sâu vào quá khứ và tâm lý đen tối của Tom Riddle để tìm ra điểm yếu chí mạng của hắn – các Trường Sinh Linh Giá. Song song đó, cuốn sách độc dược kỳ lạ của Hoàng tử lai mang đến cho Harry vô số bí thuật lẫn những hệ quả khôn lường.",2005,650,240000,"/images/books/harry-potter-6.jpg",
                    categories["Fantasy"], authors["J.K. Rowling"], publishers["Packt"], shelves["B3"]),

                CreateBook("978604000015","Harry Potter và Bảo bối Tử thần","Harry Potter và Bảo bối Tử thần là hồi kết huy hoàng và bi tráng cho toàn bộ thiên sử thi pháp thuật. Không còn sự che chở của mái trường Hogwarts, bộ ba Harry, Ron và Hermione phải lang bạt khắp nơi để truy tìm và phá hủy từng Trường Sinh Linh Giá, dẫn tới trận đại chiến định mệnh bảo vệ thế giới phù thủy.",2007,700,260000,"/images/books/harry-potter-7.jpg",
                    categories["Fantasy"], authors["J.K. Rowling"], publishers["Packt"], shelves["B3"]),

                CreateBook("978604000016","Deep Work","Deep Work (Làm Ra Làm, Chơi Ra Chơi) của Cal Newport phân tích sức mạnh của sự tập trung cao độ trong thời đại bão hòa thông tin và phiền nhiễu số. Cuốn sách cung cấp những chiến lược thực tiễn giúp bạn rèn luyện khả năng tập trung chuyên sâu để tạo ra những giá trị xuất sắc trong thời gian ngắn nhất.",2016,304,175000,"/images/books/deep-work.jpg",
                    categories["Kỹ năng sống"], authors["Cal Newport"], publishers["Packt"], shelves["A1"]),

                CreateBook("978604000017","Digital Minimalism","Digital Minimalism (Chủ Nghĩa Tối Giản Kỹ Thuật Số) của Cal Newport hướng dẫn cách lấy lại quyền kiểm soát cuộc sống trước sự chi phối của mạng xã hội và các thiết bị số. Tác phẩm giúp người đọc thiết lập mối quan hệ lành mạnh với công nghệ, tập trung vào những giá trị sống đích thực.",2019,290,170000,"/images/books/digital-minimalism.jpg",
                    categories["Kỹ năng sống"], authors["Cal Newport"], publishers["Packt"], shelves["A1"]),

                CreateBook("978604000018","The 7 Habits of Highly Effective People","7 Thói Quen Của Người Thành Đạt (The 7 Habits of Highly Effective People) của Stephen R. Covey là một trong những cuốn sách quản trị và phát triển cá nhân có tầm ảnh hưởng lớn nhất mọi thời đại. Tác phẩm hướng dẫn quy trình chuyển biến từ phụ thuộc sang độc lập và tương thuộc lẫn nhau, giúp độc giả làm chủ cuộc đời và đạt hiệu quả đỉnh cao trong công việc.",1989,430,195000,"/images/books/7habits.jpg",
                    categories["Kỹ năng sống"], authors["Stephen R. Covey"], publishers["NXB Trẻ"], shelves["A1"]),

                CreateBook("978604000019","The Subtle Art of Not Giving a F*ck","Nghệ Thuật Tinh Tế Của Việc Đếch Quan Tâm của Mark Manson mang đến một góc nhìn thực tế và tỉnh táo trước xu hướng tôn vinh sự tích cực độc hại. Cuốn sách giúp bạn nhận ra rằng cuộc sống vốn đầy rẫy khó khăn, điều quan trọng là biết chọn lọc những giá trị thực sự xứng đáng để dành tâm sức và năng lượng giải quyết.",2016,260,175000,"/images/books/subtle-art.jpg",
                    categories["Kỹ năng sống"], authors["Mark Manson"], publishers["Packt"], shelves["A1"]),

                CreateBook("978604000020","Kafka bên bờ biển","Kafka bên bờ biển là một trong những tác phẩm xuất sắc nhất của Haruki Murakami mang đậm màu sắc hiện thực huyền ảo. Cuốn sách đan xen hành trình trốn chạy số phận của cậu bé 15 tuổi Kafka Tamura và câu chuyện kỳ lạ của ông lão Nakata biết nói chuyện với mèo, dẫn dắt độc giả vào thế giới tâm linh sâu thẳm.",2002,520,190000,"/images/books/kafka.jpg",
                    categories["Văn học"], authors["Haruki Murakami"], publishers["NXB Hội Nhà Văn"], shelves["B1"]),

                CreateBook("978604000021","Rừng Na Uy","Rừng Na Uy là kiệt tác văn học nổi tiếng nhất của Haruki Murakami. Tác phẩm đưa người đọc trôi vào dòng hồi ức man mác buồn của nhân vật Toru Watanabe về tuổi trẻ những năm 1960 ở Tokyo, giữa tình cảm giằng xé với Naoko – cô gái mang tổn thương tâm hồn sâu sắc – và Midori hoạt bát, tràn đầy sức sống.",1987,390,170000,"/images/books/norwegian-wood.jpg",
                    categories["Văn học"], authors["Haruki Murakami"], publishers["NXB Hội Nhà Văn"], shelves["B1"]),

                CreateBook("978604000022","Tôi thấy hoa vàng trên cỏ xanh","Tôi Thấy Hoa Vàng Trên Cỏ Xanh của nhà văn Nguyễn Nhật Ánh là khúc ca đồng quê trong trẻo, đưa người đọc trở về những ký ức tuổi thơ êm đềm nơi làng quê miền Trung. Tác phẩm khắc họa tình anh em máu thịt giữa Thiều và Tường, những rung động đầu đời ngây thơ và những câu chuyện dung dị nhưng ấm áp tình người.",2010,380,120000,"/images/books/hoa-vang.jpg",
                    categories["Văn học"], authors["Nguyễn Nhật Ánh"], publishers["NXB Trẻ"], shelves["A2"]),

                CreateBook("978604000023","Mắt Biếc","Mắt Biếc là một trong những tác phẩm giàu cảm xúc nhất của Nguyễn Nhật Ánh. Câu chuyện xoay quanh mối tình đơn phương thủy chung, day dứt của Ngạn dành cho cô bạn gái thanh mai trúc mã Hà Lan sở hữu đôi mắt biếc biếc. Dù dòng đời đổi thay và bao biến cố ập đến, tình yêu mộc mạc của Ngạn vẫn mãi vẹn nguyên cùng thời gian.",1990,360,115000,"/images/books/mat-biec.jpg",
                    categories["Văn học"], authors["Nguyễn Nhật Ánh"], publishers["NXB Trẻ"], shelves["A2"]),

                CreateBook("978604000024","Dế Mèn Phiêu Lưu Ký","Dế Mèn Phiêu Lưu Ký của nhà văn Tô Hoài là tác phẩm văn học thiếu nhi kinh điển của Việt Nam. Cuốn sách kể về hành trình vạn dặm của chú Dế Mèn dũng cảm nhưng từng bồng bột, kiêu ngạo; qua bao trải nghiệm sinh tử để trưởng thành, đúc kết bài học về lòng nhân ái, tình bạn bè và khát vọng hòa bình cho muôn loài.",1941,220,85000,"/images/books/de-men.jpg",
                    categories["Thiếu nhi"], authors["Tô Hoài"], publishers["NXB Kim Đồng"], shelves["A3"]),

                CreateBook("978604000025","Tuổi Trẻ Đáng Giá Bao Nhiêu","Tuổi Trẻ Đáng Giá Bao Nhiêu của tác giả Rosie Nguyễn là cuốn sách gối đầu giường của đông đảo độc giả trẻ Việt Nam. Tác phẩm chia sẻ những trải nghiệm thực tế về việc học tập suốt đời, văn hóa đọc sách, tinh thần tự lập và thói quen xách ba lô lên đi để khám phá thế giới cũng như tìm thấy sứ mệnh đích thực của bản thân.",2018,300,110000,"/images/books/tuoi-tre.jpg",
                    categories["Kỹ năng sống"], authors["Rosie Nguyễn"], publishers["NXB Trẻ"], shelves["A1"]),

                CreateBook("978604000026","The Pragmatic Programmer","The Pragmatic Programmer (Lập Trình Viên Thực Dụng) là cuốn sách kinh điển hướng dẫn tư duy nghề nghiệp và kỹ năng thực chiến dành cho lập trình viên: từ quản lý mã nguồn, viết mã linh hoạt, kiểm thử tự động đến thói quen học hỏi liên tục để trở thành một chuyên gia công nghệ xuất sắc.",1999,352,420000,"/images/books/pragmatic-programmer.jpg",
                    categories["Công nghệ thông tin"], authors["Andrew Hunt"], publishers["Addison Wesley"], shelves["B1"]),

                CreateBook("978604000027","Refactoring","Refactoring: Cải Thiện Thiết Kế Mã Nguồn Hiện Có của Martin Fowler là tài liệu kinh điển dành cho mọi kỹ sư phần mềm. Cuốn sách trình bày chi tiết các nguyên tắc, kỹ thuật và danh mục các bước tái cấu trúc mã (code smells & refactoring patterns) giúp mã nguồn luôn sạch sẽ, rõ ràng, dễ bảo trì và dễ mở rộng.",1999,448,450000,"/images/books/refactoring.jpg",
                    categories["Công nghệ thông tin"], authors["Martin Fowler"], publishers["Addison Wesley"], shelves["B1"]),

               CreateBook("978604000028","Design Patterns","Design Patterns: Elements of Reusable Object-Oriented Software là tác phẩm kinh điển của Gang of Four (Erich Gamma, Richard Helm, Ralph Johnson, John Vlissides), giới thiệu 23 mẫu thiết kế hướng đối tượng chuẩn mực giúp giải quyết các bài toán kiến trúc phần mềm phức tạp một cách thanh lịch và tái sử dụng.",1994,395,480000,"/images/books/design-patterns.jpg",
                    categories["Công nghệ thông tin"], authors["Erich Gamma"], publishers["Addison Wesley"], shelves["B1"]),

                CreateBook("978604000029","Domain Driven Design","Domain-Driven Design (Thiết Kế Hướng Tên Miền) của Eric Evans là cuốn sách nền tảng định hình tư duy phát triển phần mềm doanh nghiệp hiện đại. Tác phẩm hướng dẫn cách kết nối chặt chẽ giữa mô hình nghiệp vụ thực tế và kiến trúc mã nguồn thông qua việc xây dựng Ngôn ngữ chung (Ubiquitous Language) và Bounded Contexts.",2003,560,520000,"/images/books/ddd.jpg",
                    categories["Công nghệ thông tin"], authors["Eric Evans"], publishers["Addison Wesley"], shelves["B2"]),

                CreateBook("978604000030","Code Complete","Code Complete của Steve McConnell được ví như cuốn bách khoa toàn thư thực hành của nghề lập trình. Với cách tiếp cận thực dụng, cuốn sách bao quát toàn bộ vòng đời viết mã từ việc đặt tên biến, cấu trúc hàm, thiết kế lớp, tối ưu hiệu năng đến phương pháp kiểm thử và gỡ lỗi chuyên nghiệp.",2004,900,550000,"/images/books/code-complete.jpg",
                    categories["Công nghệ thông tin"], authors["Steve McConnell"], publishers["Microsoft Press"], shelves["B2"]),

                CreateBook("978604000031","Working Effectively with Legacy Code","Working Effectively with Legacy Code của Michael Feathers là cẩm nang cứu cánh cho các lập trình viên khi phải tiếp quản những hệ thống mã nguồn cũ, phức tạp và thiếu tài liệu. Cuốn sách cung cấp các chiến lược đưa mã nguồn vào khung kiểm thử an toàn trước khi thực hiện bất kỳ sửa đổi nào.",2004,456,470000,"/images/books/legacy-code.jpg",
                    categories["Công nghệ thông tin"], authors["Michael Feathers"], publishers["Prentice Hall"], shelves["B2"]),

                CreateBook("978604000032","Head First Design Patterns","Head First Design Patterns mang đến phong cách học tập mẫu thiết kế phần mềm hoàn toàn mới lạ thông qua hình ảnh minh họa sinh động, câu đố và tình huống thực tế hài hước. Cuốn sách giúp bạn nắm vững các nguyên lý OOP cốt lõi và các mẫu thiết kế kinh điển như Observer, Decorator, Factory, Singleton và Strategy.",2004,694,450000,"/images/books/head-first-design-patterns.jpg",
                    categories["Công nghệ thông tin"], authors["Kathy Sierra"], publishers["O'Reilly"], shelves["B3"]),

                CreateBook("978604000033","Effective Java","Effective Java của Joshua Bloch (trưởng nhóm kiến trúc sư ngôn ngữ Java tại Sun Microsystems) tổng hợp 90 quy tắc thực hành tốt nhất. Cuốn sách phân tích sâu sắc các khía cạnh tạo đối tượng, xử lý ngoại lệ, đồng thời hóa (concurrency) và lập trình hàm với Lambda/Streams, giúp viết mã Java chuẩn mực và tối ưu.",2018,416,460000,"/images/books/effective-java.jpg",
                    categories["Công nghệ thông tin"], authors["Joshua Bloch"], publishers["Addison Wesley"], shelves["B3"]),

                CreateBook("978604000034","The Clean Coder","The Clean Coder: Tác Phong Của Lập Trình Viên Chuyên Nghiệp của Robert C. Martin tập trung vào khía cạnh đạo đức nghề nghiệp, thái độ làm việc, kỹ năng thương lượng hạn chót (deadlines), quản lý thời gian và trách nhiệm cá nhân để trở thành một kỹ sư phần mềm đẳng cấp và đáng tin cậy.",2011,256,350000,"/images/books/clean-coder.jpg",
                    categories["Công nghệ thông tin"], authors["Robert C. Martin"], publishers["Prentice Hall"], shelves["B1"]),

                CreateBook("978604000035","Agile Principles","Agile Principles, Patterns, and Practices in C# của Robert C. Martin giải thích chi tiết các nguyên tắc cốt lõi của Agile, lập trình cực hạn (XP) và trọn bộ 5 nguyên lý SOLID nổi tiếng. Cuốn sách là tài liệu học tập không thể thiếu để xây dựng kiến trúc phần mềm linh hoạt, dễ thích ứng với mọi thay đổi.",2001,300,320000,"/images/books/agile.jpg",
                    categories["Công nghệ thông tin"], authors["Robert C. Martin"], publishers["Prentice Hall"], shelves["B1"]),

                CreateBook("978604000036","Scrum","Scrum: Nghệ Thuật Làm Việc Gấp Đôi Trong Một Nửa Thời Gian của Jeff Sutherland – đồng tác giả bản Tuyên ngôn Agile. Tác phẩm giải thích nguồn gốc và triết lý vận hành của khung làm việc Scrum, giúp các đội ngũ loại bỏ lãng phí, tự chủ trong công việc và đột phá về năng suất giao hàng.",2014,280,250000,"/images/books/scrum.jpg",
                    categories["Công nghệ thông tin"], authors["Jeff Sutherland"], publishers["NXB Trẻ"], shelves["B2"]),

                CreateBook("978604000037","Soft Skills","Soft Skills: Cẩm Nang Kỹ Năng Mềm Dành Cho Lập Trình Viên của John Sonmez bao quát các khía cạnh then chốt ngoài chuyên môn kỹ thuật: xây dựng thương hiệu cá nhân, kỹ năng đàm phán lương, quản lý tài chính, rèn luyện thể chất và duy trì lối sống lành mạnh cho giới công nghệ.",2014,504,380000,"/images/books/soft-skills.jpg",
                    categories["Kỹ năng sống"], authors["John Sonmez"], publishers["Manning"], shelves["A1"]),

                CreateBook("978604000038","The Mythical Man Month","The Mythical Man-Month của Frederick Brooks Jr. là tác phẩm kinh điển về quản lý dự án công nghệ thông tin. Nổi tiếng với Định luật Brooks (Thêm người vào một dự án phần mềm đang trễ chỉ làm nó trễ hơn), cuốn sách đúc kết những bài học xương máu về giao tiếp tổ chức, kiến trúc hệ thống và quy luật vận hành dự án.",1975,322,360000,"/images/books/mythical-man-month.jpg",
                    categories["Công nghệ thông tin"], authors["Frederick Brooks Jr."], publishers["Addison Wesley"], shelves["B2"]),

                CreateBook("978604000039","The Phoenix Project","The Phoenix Project (Dự Án Phượng Hoàng) của Gene Kim là cuốn tiểu thuyết ăn khách kể về hành trình cứu vãn một dự án IT trọng yếu của tập đoàn Parts Unlimited đang trên bờ vực sụp đổ. Qua đó, tác phẩm minh họa sinh động sức mạnh chuyển đổi của triết lý DevOps, Ba Cách Thức (The Three Ways) và tự động hóa quy trình.",2013,432,390000,"/images/books/phoenix-project.jpg",
                    categories["Công nghệ thông tin"], authors["Gene Kim"], publishers["IT Revolution"], shelves["B3"]),

                CreateBook("978604000040","Algorithms","Algorithms của Robert Sedgewick và Kevin Wayne là giáo trình giải thuật chuẩn mực được giảng dạy tại Đại học Princeton. Cuốn sách cung cấp nền tảng vững chắc về các cấu trúc dữ liệu cơ bản (ngăn xếp, hàng đợi, cây, bảng băm) và thuật toán quan trọng về sắp xếp, đồ thị và xử lý chuỗi với mã nguồn minh họa bằng Java tối ưu.",2009,700,500000,"/images/books/algorithms.jpg",
                    categories["Công nghệ thông tin"], authors["Robert Sedgewick"], publishers["Addison Wesley"], shelves["B3"]),

                CreateBook("978604000041","Artificial Intelligence","Artificial Intelligence: A Modern Approach của Stuart Russell và Peter Norvig là giáo trình AI tiêu chuẩn được giảng dạy tại hơn 1.500 trường đại học trên toàn cầu, bao quát từ thuật toán tìm kiếm, logic suy luận, học máy cho đến thị giác máy tính và xử lý ngôn ngữ tự nhiên hiện đại.",2020,580,480000,"/images/books/artificial-intelligence.jpg",
                    categories["Công nghệ thông tin"], authors["Stuart Russell"], publishers["Pearson"], shelves["C1"]),

                CreateBook("978604000042","Machine Learning Basics","Machine Learning Basics tổng hợp những kiến thức nền tảng và cốt lõi nhất về học máy: từ hồi quy tuyến tính, hồi quy logistic, mạng nơ-ron nhân tạo cho đến các thuật toán học có giám sát và học không giám sát, cung cấp nền tảng vững chắc cho các nhà nghiên cứu và kỹ sư dữ liệu.",2021,450,420000,"/images/books/machine-learning.jpg",
                    categories["Công nghệ thông tin"], authors["Andrew Ng"], publishers["Coursera Press"], shelves["C1"]),

                CreateBook("978604000043","Python Crash Course","Python Crash Course của Eric Matthes là cẩm nang dẫn dắt người mới bắt đầu làm quen với lập trình Python một cách nhanh chóng và hiệu quả. Cuốn sách kết hợp giữa việc nắm vững lý thuyết cú pháp cốt lõi và thực hành thông qua 3 dự án lớn: phát triển game 2D, trực quan hóa dữ liệu và xây dựng ứng dụng web với Django.",2022,540,390000,"/images/books/python-crash-course.jpg",
                    categories["Công nghệ thông tin"], authors["Eric Matthes"], publishers["No Starch Press"], shelves["C1"]),

                CreateBook("978604000044","Learning ASP.NET Core","Learning ASP.NET Core cung cấp lộ trình bài bản để xây dựng các ứng dụng web và Web API hiện đại, hiệu năng cao trên nền tảng .NET Core. Tác phẩm đi sâu vào các cơ chế then chốt như Dependency Injection, Middleware pipeline, Routing, cấu hình bảo mật với Identity và tương tác dữ liệu với Entity Framework Core.",2023,600,450000,"/images/books/aspnetcore.jpg",
                    categories["Công nghệ thông tin"], authors["Microsoft"], publishers["Microsoft Press"], shelves["C2"]),

                CreateBook("978604000045","Entity Framework Core","Entity Framework Core là tài liệu hướng dẫn toàn diện về ORM hiện đại của nền tảng .NET: thiết kế cơ sở dữ liệu Code-First, cấu hình Fluent API, tối ưu hóa truy vấn LINQ, quản lý Migrations và xử lý giao dịch dữ liệu quy mô lớn trong các ứng dụng doanh nghiệp.",2023,420,350000,"/images/books/efcore.jpg",
                    categories["Công nghệ thông tin"], authors["Microsoft"], publishers["Microsoft Press"], shelves["C2"]),

                CreateBook("978604000046","SQL Server Internals","SQL Server Internals đi sâu vào kiến trúc bên trong của Microsoft SQL Server: cơ chế lưu trữ trang và phần mở rộng (Pages & Extents), cấu trúc chỉ mục B-Tree, cơ chế khóa và cô lập giao dịch (Locking & Concurrency), bộ tối ưu hóa truy vấn và kỹ thuật tinh chỉnh hiệu năng cơ sở dữ liệu chuyên sâu.",2019,720,520000,"/images/books/sql-server.jpg",
                    categories["Công nghệ thông tin"], authors["Kalen Delaney"], publishers["Microsoft Press"], shelves["C2"]),

                CreateBook("978604000047","Docker Deep Dive","Docker Deep Dive là cẩm nang thực chiến toàn diện về công nghệ đóng gói container: từ kiến trúc Docker Engine, viết Dockerfile tối ưu, quản lý Image, Container, Volume, Network cho đến triển khai ứng dụng đa dịch vụ bằng Docker Compose.",2020,380,340000,"/images/books/docker.jpg",
                    categories["Công nghệ thông tin"], authors["Nigel Poulton"], publishers["Independently Published"], shelves["C2"]),

                CreateBook("978604000048","Kubernetes in Action","Kubernetes in Action hướng dẫn chi tiết cách điều phối và quản trị container trên nền tảng Kubernetes: từ Pod, Service, ReplicaSet, Deployment, ConfigMap cho đến quản lý lưu trữ PersistentVolume và tự động co giãn tải trong môi trường sản xuất thực tế.",2021,640,510000,"/images/books/kubernetes.jpg",
                    categories["Công nghệ thông tin"], authors["Marko Luksa"], publishers["Manning"], shelves["C2"]),

                CreateBook("978604000049","Microservices Patterns","Microservices Patterns của Chris Richardson phân tích toàn diện các mẫu thiết kế kiến trúc vi dịch vụ (Microservices). Cuốn sách hướng dẫn chi tiết cách phân rã hệ thống nguyên khối (Monolith), quản lý giao dịch phân tán với Saga, truy vấn dữ liệu với CQRS và Event Sourcing, cùng các chiến lược triển khai và kiểm thử an toàn.",2018,520,490000,"/images/books/microservices.jpg",
                    categories["Công nghệ thông tin"], authors["Chris Richardson"], publishers["Manning"], shelves["C3"]),

                CreateBook("978604000050","Clean Architecture","Clean Architecture: Kiến Trúc Phần Mềm Sạch của Robert C. Martin đúc kết các quy tắc vàng để tạo nên những hệ thống phần mềm độc lập với framework, UI, cơ sở dữ liệu và các tác nhân bên ngoài. Cuốn sách giúp bạn thiết kế những hệ thống phần mềm có khả năng kiểm thử dễ dàng và trường tồn qua nhiều thập kỷ.",2017,432,430000,"/images/books/clean-architecture.jpg",
                    categories["Công nghệ thông tin"], authors["Robert C. Martin"], publishers["Prentice Hall"], shelves["C3"]),

                CreateBook("978604000081","Pride and Prejudice","Kiêu Hãnh và Định Kiến (Pride and Prejudice) của Jane Austen là kiệt tác văn học cổ điển Anh quốc thế kỷ 19, khắc họa câu chuyện tình yêu đầy sắc sảo và dí dỏm giữa Elizabeth Bennet thông minh, độc lập và chàng quý tộc giàu có nhưng kiêu hãnh Fitzwilliam Darcy.",1813,432,180000,"/images/books/pride-prejudice.jpg",
                    categories["Văn học"], authors["Jane Austen"], publishers["Penguin"], shelves["D1"]),

                CreateBook("978604000082","Sense and Sensibility","Sense and Sensibility (Lý Trí và Tình Cảm) là tiểu thuyết kinh điển của nữ văn sĩ Jane Austen. Tác phẩm kể về cuộc đời và tình duyên của hai chị em nhà Dashwood – Elinor trầm tĩnh, thận trọng đại diện cho Lý trí và Marianne sôi nổi, cuồng nhiệt đại diện cho Tình cảm – qua đó khắc họa sâu sắc các giá trị xã hội và hôn nhân nước Anh thế kỷ 19.",1811,410,180000,"/images/books/sense-sensibility.jpg",
                    categories["Văn học"], authors["Jane Austen"], publishers["Penguin"], shelves["D1"]),

                CreateBook("978604000083","Emma","Emma là kiệt tác văn học châm biếm sâu cay và hóm hỉnh của Jane Austen. Nhân vật chính Emma Woodhouse là một tiểu thư giàu có, thông minh nhưng thích can thiệp vào chuyện tình cảm của người khác. Trải qua những ngộ nhận và hiểu lầm dở khóc dở cười, cô dần trưởng thành và nhận ra hạnh phúc đích thực luôn ở ngay bên cạnh mình.",1815,450,190000,"/images/books/emma.jpg",
                    categories["Văn học"], authors["Jane Austen"], publishers["Penguin"], shelves["D1"]),

                CreateBook("978604000084","The Great Gatsby","The Great Gatsby (Gatsby Vĩ Đại) của F. Scott Fitzgerald là tác phẩm tiêu biểu nhất về kỷ nguyên nhạc Jazz và Giấc mơ Mỹ những năm 1920. Thông qua nhân vật người kể chuyện Nick Carraway, cuốn sách bóc trần sự hào nhoáng, xa xỉ cùng nỗi cô đơn tuyệt vọng của chàng triệu phú bí ẩn Jay Gatsby trong nỗ lực vô vọng giành lại tình yêu đã mất.",1925,240,170000,"/images/books/great-gatsby.jpg",
                    categories["Văn học"], authors["F. Scott Fitzgerald"], publishers["Scribner"], shelves["D2"]),

                CreateBook("978604000085","The Da Vinci Code","Mật Mã Da Vinci (The Da Vinci Code) của Dan Brown là tiểu thuyết trinh thám ly kỳ nghẹt thở đưa giáo sư Robert Langdon vào cuộc giải mã các bí ẩn nghìn năm ẩn giấu sau những kiệt tác hội họa của Leonardo da Vinci và cuộc truy lùng Chén Thánh huyền bí khắp châu Âu.",2003,592,250000,"/images/books/da-vinci-code.jpg",
                    categories["Tiểu thuyết"], authors["Dan Brown"], publishers["Doubleday"], shelves["D2"]),

                CreateBook("978604000086","Angels and Demons","Thiên Thần và Ác Quỷ (Angels and Demons) là cuộc chạy đua nghẹt thở của Robert Langdon tại Vatican để ngăn chặn âm mưu hủy diệt của hội kín Illuminati bằng bom phản vật chất, hòa quyện giữa lịch sử tôn giáo, nghệ thuật và khoa học hiện đại.",2000,616,250000,"/images/books/angels-demons.jpg",
                    categories["Tiểu thuyết"], authors["Dan Brown"], publishers["Doubleday"], shelves["D2"]),

                CreateBook("978604000087","Inferno","Hỏa Ngục (Inferno) đưa Robert Langdon vào trung tâm nước Ý, lần theo những biểu tượng trong tác phẩm Hỏa Ngục của Dante để ngăn chặn một đại dịch diệt chủng toàn cầu do một nhà khoa học cuồng tín phát tán.",2013,620,260000,"/images/books/inferno.jpg",
                    categories["Tiểu thuyết"], authors["Dan Brown"], publishers["Doubleday"], shelves["D2"]),

                CreateBook("978604000088","Origin","Nguồn Cội (Origin) theo chân Robert Langdon giải mã phát kiến chấn động làm lung lay nền tảng tôn giáo nhân loại: Chúng ta từ đâu đến? Và chúng ta đang đi về đâu?",2017,540,250000,"/images/books/origin.jpg",
                    categories["Tiểu thuyết"], authors["Dan Brown"], publishers["Doubleday"], shelves["D2"]),

                CreateBook("978604000089","A Brief History of Time","Lược Sử Thời Gian (A Brief History of Time) của Stephen Hawking là cuốn sách khoa học vũ trụ kinh điển, giải thích một cách dễ hiểu về Vụ Nổ Lớn (Big Bang), Hố Đen, Không-Thời Gian và hành trình đi tìm Thuyết Vạn Vật (Theory of Everything) của nền vật lý hiện đại.",1988,256,220000,"/images/books/brief-history-time.jpg",
                    categories["Khoa học"], authors["Stephen Hawking"], publishers["Bantam"], shelves["C1"]),

                CreateBook("978604000100","Ikigai","Ikigai (生きがい) là một triết lý sống lâu đời của người Nhật, mang ý nghĩa là \"lý do để thức dậy mỗi sớm mai\" hay \"ý nghĩa của sự tồn tại\". Cuốn sách do hai tác giả Héctor García và Francesc Miralles đúc kết sau chuyến đi thực địa tại Ogimi – ngôi làng nhỏ tại đảo Okinawa nổi tiếng có tuổi thọ cao nhất hành tinh. Tác phẩm khám phá toàn diện 4 trụ cột Ikigai: Điều bạn yêu thích, Điều bạn làm giỏi, Điều thế giới cần và Điều giúp bạn tạo thu nhập, cùng những bí quyết dinh dưỡng Hara Hachi Bu, lối sống hòa mình với thiên nhiên và cộng đồng tương trợ Moai.",2016,208,180000,"/images/books/ikigai.jpg",
                    categories["Kỹ năng sống"], authors["Héctor García"], publishers["Penguin"], shelves["A1"]),
            };

            context.Books.AddRange(books);
            context.SaveChanges();
        }
    }
}