using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SOF1022_NHOM1_SD2003_Ver2.Models.DTOs;
using SOF1022_NHOM1_SD2003_Ver2.Services;

namespace SOF1022_NHOM1_SD2003_Ver2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [IgnoreAntiforgeryToken]
    public class AiChatController : ControllerBase
    {
        private readonly IChatbotService _chatbotService;

        public AiChatController(IChatbotService chatbotService)
        {
            _chatbotService = chatbotService;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] AiChatRequestDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new { message = "Tin nhắn không được để trống" });
            }

            var response = await _chatbotService.GetResponseAsync(request.Message, request.ConversationId);
            return Ok(response);
        }

        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            bool isConfigured = _chatbotService.IsAiConfigured();
            return Ok(new
            {
                isAiConfigured = isConfigured,
                provider = isConfigured ? "Google Gemini AI" : "Local Database Engine",
                description = isConfigured 
                    ? "Đang kết nối API Trí tuệ nhân tạo trực tuyến" 
                    : "Đang chạy chế độ trợ lý dữ liệu nội bộ (Local DB)"
            });
        }
    }
}
