using System.Net;
using System.Net.Mail;

namespace SOF1022_NHOM1_SD2003_Ver2.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailConfirmationAsync(string toEmail, string confirmationLink, string userName)
        {
            try
            {
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var port = int.Parse(_configuration["EmailSettings:Port"] ?? "587");
                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                var senderPassword = _configuration["EmailSettings:SenderPassword"];
                var enableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"] ?? "true");

                using var client = new SmtpClient(smtpServer, port)
                {
                    EnableSsl = enableSsl,
                    Credentials = new NetworkCredential(senderEmail, senderPassword)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, "Thư viện điện tử"),
                    Subject = "Xác nhận email đăng ký tài khoản",
                    Body = $@"
                        <html>
                        <body>
                            <h2>Xin chào {userName},</h2>
                            <p>Cảm ơn bạn đã đăng ký tài khoản tại Thư viện điện tử.</p>
                            <p>Vui lòng nhấp vào liên kết dưới đây để xác nhận email của bạn:</p>
                            <p><a href='{confirmationLink}'>Xác nhận email</a></p>
                            <p>Liên kết này sẽ hết hạn sau 24 giờ.</p>
                            <p>Nếu bạn không đăng ký tài khoản này, vui lòng bỏ qua email này.</p>
                            <br/>
                            <p>Trân trọng,</p>
                            <p>Đội ngũ Thư viện điện tử</p>
                        </body>
                        </html>",
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi gửi email: {ex.Message}");
                throw;
            }
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            if (string.IsNullOrWhiteSpace(toEmail)) return;

            try
            {
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                if (string.IsNullOrEmpty(smtpServer))
                {
                    Console.WriteLine($"[EmailService] SmtpServer not configured. Mocking email to {toEmail}: {subject}");
                    return;
                }

                var port = int.Parse(_configuration["EmailSettings:Port"] ?? "587");
                var senderEmail = _configuration["EmailSettings:SenderEmail"] ?? "noreply@library.edu.vn";
                var senderPassword = _configuration["EmailSettings:SenderPassword"] ?? "";
                var enableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"] ?? "true");

                using var client = new SmtpClient(smtpServer, port)
                {
                    EnableSsl = enableSsl,
                    Credentials = new NetworkCredential(senderEmail, senderPassword)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, "Thư viện điện tử"),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);
                await client.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EmailService] Lỗi gửi email đến {toEmail}: {ex.Message}");
            }
        }

        public async Task SendDueReminderAsync(string toEmail, string readerName, string borrowCode, DateTime dueDate, List<string> bookTitles)
        {
            var booksHtml = string.Join("", bookTitles.Select(b => $"<li><strong>{System.Net.WebUtility.HtmlEncode(b)}</strong></li>"));
            var body = $@"
                <div style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;'>
                    <div style='text-align: center; border-bottom: 2px solid #0d6efd; padding-bottom: 15px;'>
                        <h2 style='color: #0d6efd; margin: 0;'>THƯ VIỆN ĐIỆN TỬ FPT POLYTECHNIC</h2>
                        <p style='margin: 5px 0 0; color: #666;'>Nhắc nhở thời hạn trả sách</p>
                    </div>
                    <div style='padding: 20px 0;'>
                        <p>Xin chào <strong>{System.Net.WebUtility.HtmlEncode(readerName)}</strong>,</p>
                        <p>Hệ thống thư viện xin thông báo phiếu mượn <strong>{System.Net.WebUtility.HtmlEncode(borrowCode)}</strong> của bạn sắp đến ngày đến hạn trả:</p>
                        <div style='background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 12px 15px; margin: 15px 0;'>
                            <p style='margin: 0; font-size: 16px;'><strong>Hạn trả sách:</strong> <span style='color: #d63384; font-weight: bold;'>{dueDate:dd/MM/yyyy}</span></p>
                        </div>
                        <p><strong>Danh sách sách đang mượn:</strong></p>
                        <ul>
                            {booksHtml}
                        </ul>
                        <p>Vui lòng sắp xếp thời gian mang sách đến thư viện để hoàn tất thủ tục trả sách hoặc đăng nhập tài khoản trực tuyến để thực hiện <strong>Gia hạn thêm 7 ngày</strong> nếu cần thêm thời gian nghiên cứu.</p>
                        <p style='color: #dc3545; font-size: 13px;'><em>* Lưu ý: Trả sách quá hạn sẽ bị tính phí phạt 5.000 VNĐ / ngày / cuốn theo nội quy thư viện.</em></p>
                    </div>
                    <div style='border-top: 1px solid #e0e0e0; padding-top: 15px; font-size: 12px; color: #777; text-align: center;'>
                        <p style='margin: 0;'>Thư viện FPT Polytechnic | Trịnh Văn Bô, Nam Từ Liêm, Hà Nội</p>
                        <p style='margin: 5px 0 0;'>Đây là email gửi tự động, vui lòng không phản hồi thư này.</p>
                    </div>
                </div>";

            await SendEmailAsync(toEmail, $"[Nhắc nhở Thư viện] Phiếu mượn {borrowCode} sắp đến hạn trả", body);
        }
    }
}
