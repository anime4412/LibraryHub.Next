using SOF1022_NHOM1_SD2003_Ver2.Models.ViewModels;

namespace SOF1022_NHOM1_SD2003_Ver2.Services
{
    public class QuickHelpService : IQuickHelpService
    {
        public List<QuickHelpCategory> GetCategories()
        {
            return new List<QuickHelpCategory>
            {
                // 1. Tài khoản & Đăng nhập/Đăng ký
                new QuickHelpCategory
                {
                    Id = "account",
                    Title = "Tài khoản & Đăng nhập",
                    Icon = "fa-user-circle",
                    Items = new List<QuickHelpItem>
                    {
                        new QuickHelpItem
                        {
                            Question = "Cách đăng ký tài khoản?",
                            AnswerHtml = "Để đăng ký tài khoản thư viện, bạn truy cập trang Đăng ký và điền thông tin cá nhân. Sau khi đăng ký, bạn cần xác nhận email để kích hoạt tài khoản.",
                            ActionUrl = "/Account/Register",
                            ActionLabel = "Đăng ký ngay"
                        },
                        new QuickHelpItem
                        {
                            Question = "Quy trình đăng ký chi tiết?",
                            AnswerHtml = "Bạn có thể đăng ký bằng cách nhấn nút 'Đăng ký' ở góc trên cùng, điền thông tin họ tên, email, số điện thoại, và mật khẩu. Tài khoản sẽ được kích hoạt sau khi xác nhận email.",
                            ActionUrl = "/Account/Register",
                            ActionLabel = "Đăng ký ngay"
                        },
                        new QuickHelpItem
                        {
                            Question = "Cách đăng nhập?",
                            AnswerHtml = "Để đăng nhập, nhấn nút 'Đăng nhập' và nhập email cùng mật khẩu của bạn. Bạn cũng có thể đăng nhập bằng Google hoặc Facebook.",
                            ActionUrl = "/Account/Login",
                            ActionLabel = "Đăng nhập"
                        },
                        new QuickHelpItem
                        {
                            Question = "Quên mật khẩu?",
                            AnswerHtml = "Nếu bạn quên mật khẩu, hãy liên hệ với quản trị viên qua email library@gmail.com hoặc đến trực tiếp thư viện để được hỗ trợ đặt lại mật khẩu."
                        }
                    }
                },

                // 2. Tìm kiếm sách
                new QuickHelpCategory
                {
                    Id = "search",
                    Title = "Tìm kiếm sách",
                    Icon = "fa-search",
                    Items = new List<QuickHelpItem>
                    {
                        new QuickHelpItem
                        {
                            Question = "Cách tìm kiếm sách?",
                            AnswerHtml = "Bạn có thể tìm kiếm sách bằng cách truy cập mục 'Kho sách', sử dụng thanh tìm kiếm để tìm theo tên sách, tác giả, hoặc danh mục. Bạn cũng có thể lọc theo danh mục và trạng thái.",
                            ActionUrl = "/Books/Index",
                            ActionLabel = "Tìm sách ngay"
                        },
                        new QuickHelpItem
                        {
                            Question = "Tìm sách theo tác giả?",
                            AnswerHtml = "Trong trang Kho sách, bạn có thể nhập tên tác giả vào ô tìm kiếm hoặc sử dụng bộ lọc tác giả để tìm tất cả sách của tác giả đó.",
                            ActionUrl = "/Books/Index",
                            ActionLabel = "Kho sách"
                        },
                        new QuickHelpItem
                        {
                            Question = "Tìm sách theo danh mục?",
                            AnswerHtml = "Trang Kho sách có bộ lọc theo danh mục. Bạn chọn danh mục mong muốn (Văn học, Khoa học, Kinh tế, v.v.) để xem sách trong danh mục đó.",
                            ActionUrl = "/Books/Index",
                            ActionLabel = "Kho sách"
                        },
                        new QuickHelpItem
                        {
                            Question = "Xem kho sách?",
                            AnswerHtml = "Thư viện điện tử có hàng ngàn đầu sách đa dạng:\n- Văn học - Kinh tế - Khoa học\n- Kỹ thuật - Nghệ thuật - Lịch sử",
                            ActionUrl = "/Books/Index",
                            ActionLabel = "Khám phá kho sách"
                        }
                    }
                },

                // 3. Mượn sách
                new QuickHelpCategory
                {
                    Id = "borrowing",
                    Title = "Mượn sách",
                    Icon = "fa-book",
                    Items = new List<QuickHelpItem>
                    {
                        new QuickHelpItem
                        {
                            Question = "Cách mượn sách?",
                            AnswerHtml = "Quy trình mượn sách:\n1. Đăng nhập tài khoản\n2. Vào Dịch vụ > Tạo phiếu mượn\n3. Chọn sách từ kho\n4. Gửi phiếu mượn\n5. Chờ duyệt và đến lấy sách trong hạn 24h",
                            ActionUrl = "/BorrowSlips/Create",
                            ActionLabel = "Tạo phiếu mượn"
                        },
                        new QuickHelpItem
                        {
                            Question = "Tạo phiếu mượn?",
                            AnswerHtml = "Bạn vào menu 'Dịch vụ' > 'Tạo phiếu mượn', chọn sách muốn mượn (tối đa số lượng cho phép), rồi nhấn 'Gửi phiếu mượn'. Sau khi duyệt, bạn có 24h để đến lấy sách.",
                            ActionUrl = "/BorrowSlips/Create",
                            ActionLabel = "Tạo phiếu mượn"
                        },
                        new QuickHelpItem
                        {
                            Question = "Số sách có thể mượn?",
                            AnswerHtml = "Số sách tối đa bạn có thể mượn phụ thuộc vào loại tài khoản của bạn. Thông thường, sinh viên có thể mượn tối đa 5 sách cùng lúc."
                        },
                        new QuickHelpItem
                        {
                            Question = "Thời gian mượn sách?",
                            AnswerHtml = "Thời gian mượn sách thường là 14 ngày. Bạn có thể gia hạn thêm nếu sách chưa được người khác đặt trước."
                        },
                        new QuickHelpItem
                        {
                            Question = "Chính sách mượn sách?",
                            AnswerHtml = "Chính sách mượn:\n- Thời gian mượn: 14 ngày\n- Số lượng tối đa: 5 sách (tùy loại tài khoản)\n- Gia hạn: Có thể gia hạn nếu chưa quá hạn và không có người đặt trước\n- Phạt: Tính theo ngày quá hạn"
                        }
                    }
                },

                // 4. Trả sách & quá hạn
                new QuickHelpCategory
                {
                    Id = "returning",
                    Title = "Trả sách & Quá hạn",
                    Icon = "fa-undo",
                    Items = new List<QuickHelpItem>
                    {
                        new QuickHelpItem
                        {
                            Question = "Cách trả sách?",
                            AnswerHtml = "Để trả sách, bạn mang sách đến quầy thư viện. Nhân viên sẽ kiểm tra tình trạng sách và cập nhật trạng thái trả. Nếu quá hạn, bạn có thể bị tính phí phạt."
                        },
                        new QuickHelpItem
                        {
                            Question = "Quy trình trả sách chi tiết?",
                            AnswerHtml = "Bạn mang sách trực tiếp đến thư viện trong giờ làm việc. Nhân viên sẽ kiểm tra sách, xử lý phiếu trả và cập nhật vào hệ thống."
                        },
                        new QuickHelpItem
                        {
                            Question = "Quá hạn trả sách?",
                            AnswerHtml = "Nếu trả sách quá hạn, bạn sẽ bị tính tiền phạt theo quy định của thư viện. Chi tiết về phí phạt, bạn có thể xem mục 'Tra cứu tiền phạt' trong Dịch vụ.",
                            ActionUrl = "/Fines/MyFines",
                            ActionLabel = "Xem tiền phạt"
                        }
                    }
                },

                // 5. Tiền phạt & thanh toán QR
                new QuickHelpCategory
                {
                    Id = "fines",
                    Title = "Tiền phạt & Thanh toán",
                    Icon = "fa-money-bill-wave",
                    Items = new List<QuickHelpItem>
                    {
                        new QuickHelpItem
                        {
                            Question = "Tiền phạt được tính như thế nào?",
                            AnswerHtml = "Tiền phạt được tính dựa trên số ngày quá hạn trả sách. Chi tiết về số tiền phạt và lịch sử phạt, bạn vào 'Dịch vụ' > 'Tra cứu tiền phạt' để xem.",
                            ActionUrl = "/Fines/MyFines",
                            ActionLabel = "Tra cứu tiền phạt"
                        },
                        new QuickHelpItem
                        {
                            Question = "Xem chi tiết tiền phạt?",
                            AnswerHtml = "Phí phạt quá hạn được tính theo ngày. Bạn có thể xem chi tiết số tiền cần thanh toán trong mục 'Tra cứu tiền phạt' của menu Dịch vụ.",
                            ActionUrl = "/Fines/MyFines",
                            ActionLabel = "Tra cứu tiền phạt"
                        },
                        new QuickHelpItem
                        {
                            Question = "Cách thanh toán phạt?",
                            AnswerHtml = "Hiện tại, bạn có thể thanh toán tiền phạt bằng cách quét mã QR hoặc thanh toán trực tiếp tại quầy thư viện. Liên hệ nhân viên để được hướng dẫn chi tiết."
                        },
                        new QuickHelpItem
                        {
                            Question = "Thanh toán bằng mã QR?",
                            AnswerHtml = "Để thanh toán bằng mã QR, bạn vào mục 'Tra cứu tiền phạt', chọn khoản phạt cần thanh toán và quét mã QR hiển thị để thanh toán qua ứng dụng ngân hàng.",
                            ActionUrl = "/Fines/MyFines",
                            ActionLabel = "Tra cứu tiền phạt"
                        }
                    }
                },

                // 6. Đặt trước sách
                new QuickHelpCategory
                {
                    Id = "reservation",
                    Title = "Đặt trước sách",
                    Icon = "fa-calendar-check",
                    Items = new List<QuickHelpItem>
                    {
                        new QuickHelpItem
                        {
                            Question = "Cách đặt trước sách?",
                            AnswerHtml = "Nếu sách bạn muốn mượn đã hết, bạn có thể đặt trước. Vào trang chi tiết sách và nhấn 'Đặt trước'. Khi sách có sẵn, bạn sẽ được thông báo."
                        },
                        new QuickHelpItem
                        {
                            Question = "Quy trình đặt sách?",
                            AnswerHtml = "Khi sách hết hàng, bạn nhấn nút 'Đặt trước' trên trang chi tiết sách. Khi sách được trả lại, bạn sẽ nhận thông báo và có thể đến mượn.",
                            ActionUrl = "/Reservations/MyReservation",
                            ActionLabel = "Xem đặt trước của bạn"
                        },
                        new QuickHelpItem
                        {
                            Question = "Xem các đặt trước của tôi?",
                            AnswerHtml = "Bạn có thể xem tất cả các yêu cầu đặt trước sách của mình tại trang Đặt trước sách trong menu Dịch vụ.",
                            ActionUrl = "/Reservations/MyReservation",
                            ActionLabel = "Đặt trước sách"
                        }
                    }
                },

                // 7. Lịch sử mượn / theo dõi phiếu
                new QuickHelpCategory
                {
                    Id = "history",
                    Title = "Lịch sử mượn",
                    Icon = "fa-history",
                    Items = new List<QuickHelpItem>
                    {
                        new QuickHelpItem
                        {
                            Question = "Xem lịch sử mượn?",
                            AnswerHtml = "Để xem lịch sử mượn sách, bạn vào 'Dịch vụ' > 'Sách đang mượn'. Tại đây bạn có thể xem các phiếu mượn hiện tại và lịch sử mượn của mình.",
                            ActionUrl = "/BorrowSlips/MyBorrows",
                            ActionLabel = "Sách đang mượn"
                        },
                        new QuickHelpItem
                        {
                            Question = "Xem sách đang mượn?",
                            AnswerHtml = "Bạn truy cập menu 'Dịch vụ' > 'Sách đang mượn' để xem danh sách sách đang mượn, hạn trả, và trạng thái của từng phiếu mượn.",
                            ActionUrl = "/BorrowSlips/MyBorrows",
                            ActionLabel = "Sách đang mượn"
                        },
                        new QuickHelpItem
                        {
                            Question = "Theo dõi phiếu mượn?",
                            AnswerHtml = "Để xem:\n- Sách đang mượn: <a href='/BorrowSlips/MyBorrows'>Sách đang mượn</a>\n- Lịch sử mượn: Có trong trang Sách đang mượn\n- Đặt trước: <a href='/Reservations/MyReservation'>Đặt trước sách</a>",
                            ActionUrl = "/BorrowSlips/MyBorrows",
                            ActionLabel = "Sách đang mượn"
                        }
                    }
                },

                // 8. Giờ mở cửa & Liên hệ
                new QuickHelpCategory
                {
                    Id = "contact",
                    Title = "Giờ mở cửa & Liên hệ",
                    Icon = "fa-clock",
                    Items = new List<QuickHelpItem>
                    {
                        new QuickHelpItem
                        {
                            Question = "Giờ mở cửa?",
                            AnswerHtml = "Thư viện mở cửa từ Thứ 2 đến Thứ 6: 8:00 - 17:00, Thứ 7: 8:00 - 12:00. Chủ nhật đóng cửa."
                        },
                        new QuickHelpItem
                        {
                            Question = "Liên hệ thư viện?",
                            AnswerHtml = "Bạn có thể liên hệ với thư viện qua:\n- Email: library@gmail.com\n- Điện thoại: 0123 456 789\n- Địa chỉ: FPT Polytechnic"
                        },
                        new QuickHelpItem
                        {
                            Question = "Cần hỗ trợ thêm?",
                            AnswerHtml = "Nếu bạn cần hỗ trợ thêm, hãy liên hệ với nhân viên thư viện qua email library@gmail.com hoặc số điện thoại 0123 456 789."
                        }
                    }
                }
            };
        }
    }
}
