using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SOF1022_NHOM1_SD2003_Ver2.Datas;
using SOF1022_NHOM1_SD2003_Ver2.Models;
using SOF1022_NHOM1_SD2003_Ver2.Models.DTOs;
using SOF1022_NHOM1_SD2003_Ver2.Models.Enums;

namespace SOF1022_NHOM1_SD2003_Ver2.Services
{
    public class ChatbotService : IChatbotService
    {
        private readonly LibraryDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ChatbotService> _logger;
        private readonly Dictionary<string, string> _staticFaqs;

        public ChatbotService(
            LibraryDbContext context,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<ChatbotService> logger)
        {
            _context = context;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _logger = logger;

            _staticFaqs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "xin chào", "Xin chào! Tôi là PolyLib AI - Trợ lý thông minh của Thư viện điện tử FPT Polytechnic. Tôi có thể giúp gì cho bạn hôm nay?" },
                { "hello", "Xin chào! Tôi là PolyLib AI - Trợ lý thông minh của Thư viện điện tử. Tôi có thể giúp bạn tra cứu sách, tìm tác giả, hoặc hướng dẫn mượn trả." },
                { "hi", "Chào bạn! Tôi có thể giúp gì cho bạn về tài liệu, mượn trả hay quy định thư viện?" },
                { "giờ mở cửa", "Thư viện mở cửa từ Thứ 2 đến Thứ 6 (8:00 - 17:00), Thứ 7 (8:00 - 12:00). Chủ nhật và ngày lễ đóng cửa." },
                { "thời gian mượn", "Thời hạn mượn sách tiêu chuẩn là 14 ngày. Độc giả sinh viên được mượn tối đa 5 cuốn, giảng viên tối đa 10 cuốn cùng lúc." },
                { "tiền phạt", "Phí phạt quá hạn là 5.000 VNĐ / ngày / cuốn sách. Bạn có thể thanh toán trực tuyến qua quét mã QR hoặc nộp tại quầy thủ thư." }
            };
        }

        public bool IsAiConfigured()
        {
            var apiKey = GetEffectiveApiKey();
            return !string.IsNullOrWhiteSpace(apiKey);
        }

        private string? GetEffectiveApiKey()
        {
            var key = _configuration["AiSettings:ApiKey"];
            if (string.IsNullOrWhiteSpace(key))
            {
                key = Environment.GetEnvironmentVariable("GEMINI_API_KEY") 
                   ?? Environment.GetEnvironmentVariable("AI_API_KEY");
            }
            return key;
        }

        public string GetResponse(string userMessage)
        {
            var task = GetResponseAsync(userMessage);
            return task.GetAwaiter().GetResult().Reply;
        }

        public async Task<ChatbotResponseDto> GetResponseAsync(string userMessage, string? conversationId = null)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
            {
                return new ChatbotResponseDto
                {
                    Reply = "Xin chào! Bạn muốn tìm sách gì hoặc cần hỗ trợ về quy định mượn trả sách?",
                    IsAiReal = false,
                    Provider = "PolyLib Assistant",
                    Suggestions = new List<string> { "Tìm sách lập trình", "Quy định mượn sách", "Tra cứu tiền phạt", "Top sách xem nhiều" }
                };
            }

            var cleanMessage = userMessage.Trim();

            // 1. Kiểm tra cấu hình API Key AI trực tuyến
            var apiKey = GetEffectiveApiKey();
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                try
                {
                    var aiResponse = await CallGeminiAiAsync(cleanMessage, apiKey);
                    if (aiResponse != null)
                    {
                        return aiResponse;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Gọi AI trực tuyến thất bại. Chuyển sang công cụ dữ liệu nội bộ.");
                }
            }

            // 2. Chế độ Trợ lý cơ sở dữ liệu nội bộ thông minh (Grounded Library Assistant)
            return await GenerateGroundedResponseAsync(cleanMessage);
        }

        private async Task<ChatbotResponseDto?> CallGeminiAiAsync(string userMessage, string apiKey)
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(15);

            // Thu thập toàn bộ danh mục sách thực tế từ CSDL để AI có dữ liệu hoàn chỉnh
            var allBooks = await _context.Books
                .Where(b => !b.IsDeleted)
                .Include(b => b.Author)
                .Include(b => b.Category)
                .OrderBy(b => b.Category != null ? b.Category.Name : "")
                .ThenBy(b => b.Title)
                .Select(b => new
                {
                    b.Id,
                    b.Title,
                    Author = b.Author != null ? b.Author.Name : "Chưa cập nhật",
                    Category = b.Category != null ? b.Category.Name : "Khác",
                    b.AvailableQuantity
                })
                .ToListAsync();

            var bookCatalogText = string.Join("\n", allBooks.Select(b => 
                $"- Id: {b.Id} | Tựa đề: {b.Title} | Tác giả: {b.Author} | Thể loại: {b.Category} | Còn: {b.AvailableQuantity} cuốn"));

            var systemPrompt = $@"Bạn là PolyLib AI, trợ lý ảo thông minh chuyên trách hỗ trợ bạn đọc tại Thư viện điện tử FPT Polytechnic.

NGUYÊN TẮC TRẢ LỜI BẮT BUỘC (CỰC KỲ QUAN TRỌNG - TUYỆT ĐỐI KHÔNG ĐƯỢC BỎ QUA):
1. TRẢ LỜI ĐÚNG TRỌNG TÂM CÂU HỎI:
   - Đi thẳng vào vấn đề người dùng hỏi. Không trả lời lan man, không liệt kê những thứ người dùng không hỏi.
   - Nếu hỏi về một cuốn sách hoặc tác giả cụ thể: Kiểm tra danh mục bên dưới. Trả lời rõ ràng tên sách, tác giả, thể loại, số lượng còn lại và dẫn link [Tên sách](/Books/Details/{{Id}}).
   - Nếu hỏi về một thể loại cụ thể (ví dụ: lập trình, kỹ năng sống, kinh doanh...): Chỉ liệt kê những cuốn sách thuộc thể loại đó có trong thư viện.
   - Nếu hỏi chung chung 'thư viện có những sách gì', 'kể tên sách' hoặc 'danh mục': TUYỆT ĐỐI KHÔNG LIỆT KÊ TRÀN LAN TOÀN BỘ SÁCH (vì sẽ bị quá giới hạn và bị cắt cụt giữa chừng). Thay vào đó, hãy phân theo 3-4 thể loại chính, mỗi thể loại chỉ chọn đúng 2 cuốn sách tiêu biểu nhất kèm số lượng và link [Tên sách](/Books/Details/{{Id}}), sau đó dẫn link [Khám phá toàn bộ kho sách](/Books).
   - Nếu hỏi quy định (thời gian mượn, trả sách, tiền phạt, giờ mở cửa): Trả lời chính xác, ngắn gọn theo đúng quy định thư viện.

2. GHI ĐẦY ĐỦ THÔNG TIN VÀ KẾT THÚC CÂU HOÀN CHỈNH:
   - Khi giới thiệu cuốn sách nào, luôn ghi đủ: **Tên sách** (Tác giả) - *Trạng thái: Còn X cuốn* hoặc *Đã hết sách*.
   - Luôn kết thúc câu hoàn chỉnh, logic. Tuyệt đối không dừng dở chừng giữa câu, không bỏ lửng danh sách hay dừng đột ngột.

3. ĐỊNH DẠNG MARKDOWN CHUẨN:
   - Dùng gạch đầu dòng '- ' cho danh sách.
   - Dùng in đậm **Tên sách** hoặc từ khóa.
   - Dẫn link Markdown chuẩn dạng [Tên hiển thị](/duong_dan).

DỮ LIỆU DANH MỤC SÁCH THỰC TẾ TRONG THƯ VIỆN FPT:
{bookCatalogText}

QUY ĐỊNH THƯ VIỆN:
- Thời gian mượn sách: 14 ngày/lần mượn.
- Hạn mức mượn: Sinh viên tối đa 5 cuốn, Giảng viên tối đa 10 cuốn cùng lúc.
- Tiền phạt quá hạn: 5.000 VNĐ / ngày / cuốn (Hỗ trợ quét mã QR chuyển khoản hoặc nộp tại quầy).
- Đặt trước sách: Hỗ trợ đặt giữ sách khi sách hết kho (Available = 0), giữ chỗ trong vòng 24 giờ kể từ khi sách được trả.
- Giờ làm việc: Thứ 2 đến Thứ 6 (8h00 - 17h00), Thứ 7 (8h00 - 12h00). Chủ nhật và ngày lễ nghỉ.
- Đường dẫn hệ thống:
  + Kho sách: /Books
  + Tạo phiếu mượn: /BorrowSlips/Create
  + Tra cứu tiền phạt: /Fines/MyFines";

            var endpoint = _configuration["AiSettings:Endpoint"] 
                ?? "https://generativelanguage.googleapis.com/v1beta/models/gemini-3.1-flash-lite:generateContent";

            var url = $"{endpoint}?key={apiKey}";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = $"{systemPrompt}\n\nNgười dùng hỏi: {userMessage}" }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.3,
                    maxOutputTokens = 4096
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, content);

            // Nếu endpoint chính bị lỗi (503 hoặc 404), tự động thử fallback sang gemini-3.1-flash-lite / gemini-flash-latest
            if (!response.IsSuccessStatusCode)
            {
                var fallbackModels = new[] { "gemini-3.1-flash-lite", "gemini-flash-latest", "gemini-3.5-flash-lite" };
                foreach (var fbModel in fallbackModels)
                {
                    var fbUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{fbModel}:generateContent?key={apiKey}";
                    var fbContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                    var fbResp = await client.PostAsync(fbUrl, fbContent);
                    if (fbResp.IsSuccessStatusCode)
                    {
                        response = fbResp;
                        break;
                    }
                }
            }

            if (response.IsSuccessStatusCode)
            {
                var jsonStr = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(jsonStr);
                var root = doc.RootElement;
                if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                {
                    var firstCandidate = candidates[0];
                    if (firstCandidate.TryGetProperty("content", out var candContent) &&
                        candContent.TryGetProperty("parts", out var parts) && parts.GetArrayLength() > 0)
                    {
                        var text = parts[0].GetProperty("text").GetString();
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            var modelName = _configuration["AiSettings:Model"] ?? "3.5 Flash";
                            return new ChatbotResponseDto
                            {
                                Reply = text,
                                IsAiReal = true,
                                Provider = $"Google Gemini AI ({modelName})",
                                Suggestions = new List<string> { "Xem kho sách", "Quy định mượn", "Sách công nghệ", "Hướng dẫn tạo phiếu mượn" }
                            };
                        }
                    }
                }
            }

            return null;
        }

        private async Task<ChatbotResponseDto> GenerateGroundedResponseAsync(string cleanMessage)
        {
            var lower = cleanMessage.ToLower();
            var response = new ChatbotResponseDto
            {
                IsAiReal = false,
                Provider = "PolyLib Intelligent Engine (Local DB)",
                Suggestions = new List<string> { "Kho sách", "Cách mượn sách", "Tra cứu phạt", "Giờ mở cửa" }
            };

            // 1. Kiểm tra FAQ cố định
            foreach (var faq in _staticFaqs)
            {
                if (lower.Contains(faq.Key))
                {
                    response.Reply = faq.Value;
                    return response;
                }
            }

            // 2. Tra cứu sách theo từ khóa hoặc tên sách
            if (lower.Contains("tìm") || lower.Contains("sách") || lower.Contains("cuốn") || lower.Contains("tác giả") || lower.Contains("thể loại") || lower.Contains("danh mục"))
            {
                var queryWord = cleanMessage
                    .Replace("tìm sách", "", StringComparison.OrdinalIgnoreCase)
                    .Replace("tìm cuốn", "", StringComparison.OrdinalIgnoreCase)
                    .Replace("sách về", "", StringComparison.OrdinalIgnoreCase)
                    .Replace("sách", "", StringComparison.OrdinalIgnoreCase)
                    .Replace("tìm", "", StringComparison.OrdinalIgnoreCase)
                    .Trim();

                IQueryable<Books> bookQuery = _context.Books
                    .Where(b => !b.IsDeleted)
                    .Include(b => b.Author)
                    .Include(b => b.Category);

                if (!string.IsNullOrWhiteSpace(queryWord) && queryWord.Length >= 2)
                {
                    bookQuery = bookQuery.Where(b => 
                        (b.Title != null && b.Title.Contains(queryWord)) ||
                        (b.Author != null && b.Author.Name != null && b.Author.Name.Contains(queryWord)) ||
                        (b.Category != null && b.Category.Name != null && b.Category.Name.Contains(queryWord)) ||
                        (b.Keywords != null && b.Keywords.Contains(queryWord)));
                }
                else
                {
                    bookQuery = bookQuery.OrderByDescending(b => b.ViewCount);
                }

                var matchedBooks = await bookQuery.Take(4).ToListAsync();

                if (matchedBooks.Any())
                {
                    var sb = new StringBuilder();
                    sb.AppendLine($"Dưới đây là một số đầu sách phù hợp tìm thấy trong thư viện:");
                    foreach (var b in matchedBooks)
                    {
                        var statusStr = b.AvailableQuantity > 0 ? $"Còn {b.AvailableQuantity} cuốn" : "Tạm hết (có thể Đặt trước)";
                        sb.AppendLine($"- **[{b.Title}](/Books/Details/{b.Id})** | Tác giả: {b.Author?.Name ?? "Chưa rõ"} | {statusStr}");
                    }
                    sb.AppendLine();
                    sb.AppendLine("Bạn có thể truy cập **[Kho sách toàn diện](/Books)** để áp dụng bộ lọc theo danh mục hoặc sắp xếp.");

                    response.Reply = sb.ToString();
                    response.RelatedBooks = matchedBooks.Select(b => new BookSuggestionDto
                    {
                        Id = b.Id,
                        Title = b.Title ?? "",
                        Author = b.Author?.Name ?? "N/A",
                        Category = b.Category?.Name ?? "N/A",
                        AvailableQuantity = b.AvailableQuantity,
                        CoverImage = b.CoverImage
                    }).ToList();

                    return response;
                }
            }

            // 3. Tra cứu quy định mượn sách
            if (lower.Contains("mượn") || lower.Contains("phiếu mượn") || lower.Contains("thủ tục"))
            {
                response.Reply = @"**Quy trình mượn sách tại Thư viện điện tử:**
1. Đăng nhập tài khoản của bạn.
2. Vào mục **Dịch vụ -> [Tạo phiếu mượn](/BorrowSlips/Create)**.
3. Chọn các đầu sách mong muốn (tối đa 5 cuốn đối với sinh viên).
4. Gửi phiếu mượn và chờ thủ thư phê duyệt.
5. Sau khi phiếu được duyệt, bạn đến quầy thư viện nhận sách trong vòng 24 giờ.

*Thời hạn mượn:* 14 ngày. Hãy lưu ý hoàn trả đúng hạn để tránh phát sinh phí phạt.";
                response.Suggestions = new List<string> { "Tạo phiếu mượn", "Sách đang mượn", "Chính sách phạt" };
                return response;
            }

            // 4. Tra cứu quy định trả sách và tiền phạt
            if (lower.Contains("trả") || lower.Contains("phạt") || lower.Contains("quá hạn") || lower.Contains("nộp tiền"))
            {
                response.Reply = @"**Quy định trả sách và xử lý tiền phạt:**
- Bạn có thể mang sách đến quầy thủ thư để trả hoặc yêu cầu gia hạn nếu chưa quá hạn.
- **Mức phí phạt quá hạn:** 5.000 VNĐ / ngày / cuốn sách.
- Xem danh sách biên bản phạt và thanh toán tại mục **Dịch vụ -> [Tra cứu tiền phạt](/Fines/MyFines)**.
- Hệ thống hỗ trợ thanh toán trực tiếp qua chuyển khoản ngân hàng quét mã QR tiện lợi.";
                response.Suggestions = new List<string> { "Tra cứu tiền phạt", "Lịch sử mượn trả", "Liên hệ hỗ trợ" };
                return response;
            }

            // 5. Mặc định phản hồi thông minh kèm hướng dẫn kích hoạt AI LLM
            response.Reply = @"Tôi là PolyLib AI - Trợ lý thư viện điện tử FPT Polytechnic.
Tôi có thể hỗ trợ bạn:
- Tra cứu sách theo tên, tác giả hoặc chủ đề (ví dụ: *'Tìm sách lập trình C#'*).
- Hướng dẫn tạo phiếu mượn sách trực tuyến.
- Kiểm tra quy định thời hạn mượn và cách thanh toán tiền phạt.
- Giải đáp chính sách và giờ hoạt động thư viện.

*(Gợi ý: Hệ thống đã tích hợp sẵn module kết nối Google Gemini AI. Để kích hoạt tính năng hội thoại AI trực tuyến, chỉ cần bổ sung API Key vào appsettings.json hoặc cấu hình biến môi trường GEMINI_API_KEY).*";

            return response;
        }
    }
}
