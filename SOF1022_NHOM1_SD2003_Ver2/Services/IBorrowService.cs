using SOF1022_NHOM1_SD2003_Ver2.Models.DTOs;

namespace SOF1022_NHOM1_SD2003_Ver2.Services
{
    public interface IBorrowService
    {
        Task<BorrowResultDto> BorrowAsync(int readerId, List<int> bookIds, DateTime? dueDate = null, string? note = null, string? createdBy = null, DateTime? borrowDate = null);
        Task<(bool Success, string Message)> ReturnAsync(int borrowDetailId, bool isLost = false, bool isDamaged = false, string? damageDetails = null);
        Task<(bool Success, string Message)> ReturnMultipleAsync(int borrowSlipId, List<(int BorrowDetailId, string Condition, int? DamagePercent, string? DamageReason, string? DamageDetails)> returnItems);
        Task<List<BorrowItemDto>> GetCurrentBorrowsAsync(int readerId);
        Task<List<BorrowItemDto>> GetBorrowHistoryAsync(int readerId);
        Task<(bool Success, string Message)> RequestExtensionAsync(int borrowDetailId, int requestedDays, string reason);
    }
}
