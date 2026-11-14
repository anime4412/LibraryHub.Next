using SOF1022_NHOM1_SD2003_Ver2.Models.Enums;

namespace SOF1022_NHOM1_SD2003_Ver2.Models.DTOs
{
    public class CreateTicketRequest
    {
        public string? Subject { get; set; }
        public string FirstMessage { get; set; } = string.Empty;
    }

    public class SendMessageRequest
    {
        public int TicketId { get; set; }
        public string Content { get; set; } = string.Empty;
    }

    // Dùng để trả JSON về client thay vì trả thẳng entity SupportTickets.
    // SupportTickets.Messages và SupportMessages.SupportTicket tham chiếu
    // vòng lẫn nhau (EF Core navigation fixup), nên serialize thẳng entity
    // qua System.Text.Json sẽ ném JsonException "object cycle was detected"
    // giữa chừng và làm hỏng response.
    public class TicketDto
    {
        public int Id { get; set; }
        public string TicketCode { get; set; } = string.Empty;
        public string? Subject { get; set; }
        public SupportTicketStatus Status { get; set; }
        public bool UnreadByReader { get; set; }
        public List<TicketMessageDto> Messages { get; set; } = new();
    }

    public class TicketMessageDto
    {
        public int Id { get; set; }
        public SupportSenderType SenderType { get; set; }
        public int? SenderId { get; set; }
        public string Content { get; set; } = string.Empty;
        public string SentDate { get; set; } = string.Empty;
    }
}
