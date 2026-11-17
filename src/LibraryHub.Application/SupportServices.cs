namespace LibraryHub.Application;

public sealed class QuickHelpService : IQuickHelpService
{
    private static readonly QuickHelpTopic[] Topics =
    [
        new("borrow", "Mượn nhiều sách", "Chọn sách vào giỏ, kiểm tra giới hạn rồi gửi một phiếu.", "/me/cart"),
        new("return", "Trả sách", "Mang sách đến quầy; nhân viên ghi nhận tình trạng từng cuốn.", "/me/borrows"),
        new("extension", "Gia hạn", "Mở chi tiết sách đang mượn và gửi yêu cầu trước ngày hết hạn.", "/me/borrows"),
        new("reservation", "Đặt trước", "Đặt trước khi sách hết và chờ thông báo sẵn sàng.", "/me/reservations"),
        new("fine", "Khoản phạt", "Xem lý do, chi tiết và trạng thái thanh toán.", "/me/fines"),
        new("account", "Tài khoản", "Xác minh email, SĐT/địa chỉ hoặc đổi mật khẩu.", "/me/profile")
    ];
    public IReadOnlyList<QuickHelpTopic> GetTopics() => Topics;
}

public sealed class RuleBasedChatbotService(IQuickHelpService quickHelp) : IChatbotService
{
    public ChatbotReply Reply(string question)
    {
        var text = VietnameseValidation.Normalize(question).ToLowerInvariant();
        var topic = quickHelp.GetTopics().FirstOrDefault(x => text.Contains(x.Key, StringComparison.OrdinalIgnoreCase))
            ?? (text.Contains("mượn") ? quickHelp.GetTopics().First(x => x.Key == "borrow") : null)
            ?? (text.Contains("trả") ? quickHelp.GetTopics().First(x => x.Key == "return") : null)
            ?? (text.Contains("gia hạn") ? quickHelp.GetTopics().First(x => x.Key == "extension") : null)
            ?? (text.Contains("phạt") ? quickHelp.GetTopics().First(x => x.Key == "fine") : null);
        return topic is null
            ? new("Mình chưa chắc câu hỏi này thuộc mục nào. Bạn có thể chọn gợi ý nhanh hoặc tạo yêu cầu để nhân viên hỗ trợ.",
                ["/support"])
            : new(topic.Summary, [topic.ActionUrl, "/support"]);
    }
}
