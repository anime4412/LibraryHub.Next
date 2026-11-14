using System.Threading.Tasks;
using SOF1022_NHOM1_SD2003_Ver2.Models.DTOs;

namespace SOF1022_NHOM1_SD2003_Ver2.Services
{
    public interface IChatbotService
    {
        string GetResponse(string userMessage);
        Task<ChatbotResponseDto> GetResponseAsync(string userMessage, string? conversationId = null);
        bool IsAiConfigured();
    }
}
