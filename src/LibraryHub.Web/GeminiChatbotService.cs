using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using LibraryHub.Application;
using LibraryHub.Domain;
using LibraryHub.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace LibraryHub.Web;

public sealed record ChatTurn(string Role, string Text);

public sealed record ChatbotBookCard(
    int Id,
    string Title,
    string Author,
    string Category,
    int AvailableQuantity,
    string? CoverImagePath);

public sealed record AiChatbotReply(
    string Answer,
    IReadOnlyList<string> SuggestedActions,
    IReadOnlyList<ChatbotBookCard> Books,
    bool PoweredByAi);

public sealed class GeminiChatbotService(
    LibraryDbContext db,
    IHttpClientFactory httpFactory,
    IConfiguration configuration,
    IChatbotService ruleFallback,
    ILogger<GeminiChatbotService> logger)
{
    public async Task<AiChatbotReply> ReplyAsync(
        string question,
        IReadOnlyList<ChatTurn>? history,
        int? readerId,
        CancellationToken ct)
    {
        var cleanQuestion = (question ?? string.Empty).Trim();
        var fallback = ruleFallback.Reply(cleanQuestion);

        var settings = await db.SystemSettings.AsNoTracking().SingleOrDefaultAsync(ct) ?? new SystemSettings();
        var matchedCards = await FindRelevantBooksAsync(cleanQuestion, ct);
        var readerSummary = await BuildReaderSummaryAsync(readerId, ct);

        var apiKey = configuration["Gemini:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return new AiChatbotReply(
                BuildFallbackAnswer(fallback.Answer, matchedCards),
                MergeSuggestedActions(cleanQuestion, fallback.SuggestedActions, matchedCards),
                matchedCards,
                PoweredByAi: false);
        }

        var primaryModel = configuration["Gemini:Model"];
        if (string.IsNullOrWhiteSpace(primaryModel)) primaryModel = "gemini-flash-lite-latest";

        var systemPrompt = BuildSystemInstruction(settings, readerSummary, matchedCards);
        var payload = BuildRequestPayload(systemPrompt, cleanQuestion, history);

        var candidateModels = new[] { primaryModel, "gemini-3.5-flash-lite", "gemini-flash-latest" }
            .Where(m => !string.IsNullOrWhiteSpace(m))
            .Distinct(StringComparer.OrdinalIgnoreCase);

        foreach (var model in candidateModels)
        {
            var answer = await TryCallGeminiAsync(apiKey.Trim(), model, payload, ct);
            if (!string.IsNullOrWhiteSpace(answer))
            {
                return new AiChatbotReply(
                    answer,
                    MergeSuggestedActions(cleanQuestion, fallback.SuggestedActions, matchedCards),
                    matchedCards,
                    PoweredByAi: true);
            }
        }

        return new AiChatbotReply(
            BuildFallbackAnswer(fallback.Answer, matchedCards),
            MergeSuggestedActions(cleanQuestion, fallback.SuggestedActions, matchedCards),
            matchedCards,
            PoweredByAi: false);
    }

    private async Task<IReadOnlyList<ChatbotBookCard>> FindRelevantBooksAsync(string query, CancellationToken ct)
    {
        var candidates = await db.Books.AsNoTracking()
            .Where(x => !x.IsDeleted && (x.Status == BookStatus.Available || x.Status == BookStatus.OutOfStock))
            .Select(x => new
            {
                x.Id,
                x.Title,
                x.Isbn,
                x.Keywords,
                x.Description,
                x.AvailableQuantity,
                x.CoverImagePath,
                x.ViewCount,
                AuthorName = x.Author != null ? x.Author.Name : "LibraryHub",
                CategoryName = x.Category != null ? x.Category.Name : "Tổng hợp"
            })
            .ToListAsync(ct);

        if (candidates.Count == 0) return [];

        var scored = candidates
            .Select(b => new
            {
                Item = b,
                Score = SmartSearchHelper.ScoreBook(new Book
                {
                    Id = b.Id,
                    Title = b.Title,
                    Isbn = b.Isbn,
                    Keywords = b.Keywords,
                    Description = b.Description,
                    Language = "vi",
                    Author = new Author { Name = b.AuthorName },
                    Category = new Category { Name = b.CategoryName }
                }, query)
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.Item.AvailableQuantity > 0)
            .ThenByDescending(x => x.Item.ViewCount)
            .Take(4)
            .Select(x => new ChatbotBookCard(
                x.Item.Id,
                x.Item.Title,
                x.Item.AuthorName,
                x.Item.CategoryName,
                x.Item.AvailableQuantity,
                x.Item.CoverImagePath))
            .ToList();

        if (scored.Count > 0) return scored;

        var norm = SmartSearchHelper.Normalize(query);
        var wantsRecommendation = norm.Contains("goi y") || norm.Contains("hay") || norm.Contains("sach")
            || norm.Contains("doc gi") || norm.Contains("noi bat") || norm.Contains("de xuat");
        if (!wantsRecommendation) return [];

        return candidates
            .Where(x => x.AvailableQuantity > 0)
            .OrderByDescending(x => x.ViewCount)
            .Take(4)
            .Select(x => new ChatbotBookCard(
                x.Id,
                x.Title,
                x.AuthorName,
                x.CategoryName,
                x.AvailableQuantity,
                x.CoverImagePath))
            .ToList();
    }

    private async Task<string?> BuildReaderSummaryAsync(int? readerId, CancellationToken ct)
    {
        if (!readerId.HasValue) return null;
        var reader = await db.Readers.AsNoTracking()
            .Where(x => x.Id == readerId.Value && !x.IsDeleted)
            .Select(x => new { x.Id, x.FullName, x.MaxBorrowBooks, x.ExpiresOn })
            .SingleOrDefaultAsync(ct);
        if (reader is null) return null;

        var activeBorrows = await db.BorrowSlips.AsNoTracking()
            .CountAsync(x => x.ReaderId == reader.Id
                && (x.Status == BorrowStatus.PendingApproval
                    || x.Status == BorrowStatus.ApprovedAwaitingPickup
                    || x.Status == BorrowStatus.Borrowing
                    || x.Status == BorrowStatus.Overdue
                    || x.Status == BorrowStatus.PendingExtension), ct);
        var overdueCount = await db.BorrowSlips.AsNoTracking()
            .CountAsync(x => x.ReaderId == reader.Id && x.Status == BorrowStatus.Overdue, ct);
        var unpaidFines = await db.Fines.AsNoTracking()
            .Where(x => x.ReaderId == reader.Id
                && (x.Status == FineStatus.Unpaid || x.Status == FineStatus.PartiallyPaid))
            .Select(x => x.Amount - x.PaidAmount)
            .SumAsync(ct);

        return $"Độc giả đang đăng nhập: {reader.FullName} | Hạn thẻ: {reader.ExpiresOn:dd/MM/yyyy} | Giới hạn mượn: {reader.MaxBorrowBooks} cuốn | Phiếu đang mở: {activeBorrows} (Quá hạn: {overdueCount}) | Tiền phạt chưa thanh toán: {unpaidFines:N0}đ.";
    }

    private static string BuildSystemInstruction(
        SystemSettings settings,
        string? readerSummary,
        IReadOnlyList<ChatbotBookCard> books)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Bạn là Trợ lý Thư viện AI thân thiện và chính xác của hệ thống LibraryHub.");
        sb.AppendLine("Nguyên tắc trả lời:");
        sb.AppendLine("- Trả lời hoàn toàn bằng tiếng Việt tự nhiên, súc tích (2 đến 5 câu hoặc gạch đầu dòng ngắn gọn).");
        sb.AppendLine("- Chỉ dựa trên thông tin thật được cung cấp bên dưới; tuyệt đối KHÔNG bịa thêm đầu sách hoặc tính năng không có.");
        sb.AppendLine("- Có thể gợi ý độc giả dùng các trang: / (kho sách), /me/cart (giỏ mượn), /me/borrows (phiếu mượn), /me/reservations (đặt trước), /me/fines (khoản phạt), /support (nhắn nhân viên hỗ trợ).");
        sb.AppendLine();
        sb.AppendLine("Quy định thư viện hiện hành:");
        sb.AppendLine($"- Số sách mượn tối đa: {settings.MaxBorrowBooks} cuốn.");
        sb.AppendLine($"- Thời hạn mượn tiêu chuẩn: {settings.BorrowPeriodDays} ngày.");
        sb.AppendLine($"- Thời hạn đến nhận sách sau khi duyệt: {settings.PickupWindowDays} ngày.");
        sb.AppendLine($"- Gia hạn tối đa: {settings.MaxExtensionCount} lần, mỗi lần tối đa {settings.MaxExtensionDays} ngày.");
        sb.AppendLine($"- Phí trả trễ: {settings.LateFeePerDay:N0}đ/ngày/cuốn.");
        if (!string.IsNullOrWhiteSpace(readerSummary))
        {
            sb.AppendLine();
            sb.AppendLine(readerSummary);
        }
        if (books.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Các sách phù hợp hiện có trong kho LibraryHub:");
            foreach (var book in books)
            {
                sb.AppendLine($"- \"{book.Title}\" | Tác giả: {book.Author} | Danh mục: {book.Category} | Còn sẵn: {book.AvailableQuantity} cuốn | Link: /books/{book.Id}");
            }
        }
        return sb.ToString();
    }

    private static string BuildRequestPayload(
        string systemInstruction,
        string question,
        IReadOnlyList<ChatTurn>? history)
    {
        var contents = new List<object>();
        if (history is not null)
        {
            foreach (var turn in history.TakeLast(6))
            {
                if (string.IsNullOrWhiteSpace(turn.Text)) continue;
                var role = string.Equals(turn.Role, "model", StringComparison.OrdinalIgnoreCase)
                    ? "model"
                    : "user";
                var safeText = turn.Text.Length > 1000 ? turn.Text[..1000] : turn.Text;
                contents.Add(new { role, parts = new[] { new { text = safeText } } });
            }
        }
        contents.Add(new { role = "user", parts = new[] { new { text = question } } });

        var body = new
        {
            systemInstruction = new { parts = new[] { new { text = systemInstruction } } },
            contents,
            generationConfig = new
            {
                temperature = 0.4,
                maxOutputTokens = 512
            }
        };
        return JsonSerializer.Serialize(body);
    }

    private async Task<string?> TryCallGeminiAsync(
        string apiKey,
        string model,
        string jsonPayload,
        CancellationToken ct)
    {
        try
        {
            var client = httpFactory.CreateClient("Gemini");
            var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{Uri.EscapeDataString(model)}:generateContent";
            using var req = new HttpRequestMessage(HttpMethod.Post, endpoint);
            req.Headers.Add("x-goog-api-key", apiKey);
            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            req.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            using var resp = await client.SendAsync(req, ct);
            if (!resp.IsSuccessStatusCode)
            {
                logger.LogWarning("Gemini API returned status {StatusCode} for model {Model}.", (int)resp.StatusCode, model);
                return null;
            }

            await using var stream = await resp.Content.ReadAsStreamAsync(ct);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
            if (!doc.RootElement.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
                return null;

            var parts = candidates[0].GetProperty("content").GetProperty("parts");
            var textBuilder = new StringBuilder();
            foreach (var part in parts.EnumerateArray())
            {
                if (part.TryGetProperty("thought", out var isThought) && isThought.ValueKind == JsonValueKind.True)
                    continue;
                if (part.TryGetProperty("text", out var txt))
                    textBuilder.Append(txt.GetString());
            }
            var result = textBuilder.ToString().Trim();
            return string.IsNullOrWhiteSpace(result) ? null : result;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            logger.LogWarning("Gemini chatbot request fallback triggered ({ExceptionType}).", ex.GetType().Name);
            return null;
        }
    }

    private static string BuildFallbackAnswer(string baseAnswer, IReadOnlyList<ChatbotBookCard> books) =>
        books.Count > 0
            ? $"Mình tìm thấy {books.Count} đầu sách phù hợp trong kho bên dưới. Ngoài ra: {baseAnswer}"
            : baseAnswer;

    private static IReadOnlyList<string> MergeSuggestedActions(
        string question,
        IReadOnlyList<string> baseActions,
        IReadOnlyList<ChatbotBookCard> books)
    {
        var list = new List<string>();
        var q = SmartSearchHelper.Normalize(question);
        if (q.Contains("muon") || q.Contains("gio")) list.Add("/me/cart");
        if (q.Contains("phieu") || q.Contains("han") || q.Contains("tra")) list.Add("/me/borrows");
        if (q.Contains("dat truoc") || q.Contains("cho")) list.Add("/me/reservations");
        if (q.Contains("phat") || q.Contains("tien")) list.Add("/me/fines");
        foreach (var act in baseActions)
        {
            if (!list.Contains(act, StringComparer.OrdinalIgnoreCase))
                list.Add(act);
        }
        if (books.Count == 0 && !list.Contains("/#catalog", StringComparer.OrdinalIgnoreCase))
            list.Add("/#catalog");
        return list.Take(3).ToList();
    }
}
