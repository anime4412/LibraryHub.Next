using System.Collections.Generic;

namespace SOF1022_NHOM1_SD2003_Ver2.Models.DTOs
{
    public class ChatbotResponseDto
    {
        public string Reply { get; set; } = string.Empty;
        public bool IsAiReal { get; set; } = false;
        public string Provider { get; set; } = "LocalDatabase";
        public List<string> Suggestions { get; set; } = new();
        public List<BookSuggestionDto> RelatedBooks { get; set; } = new();
    }

    public class BookSuggestionDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int AvailableQuantity { get; set; }
        public string? CoverImage { get; set; }
    }

    public class AiChatRequestDto
    {
        public string Message { get; set; } = string.Empty;
        public string? ConversationId { get; set; }
    }
}
