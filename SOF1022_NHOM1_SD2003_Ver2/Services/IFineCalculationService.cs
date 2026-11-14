using SOF1022_NHOM1_SD2003_Ver2.Models;

namespace SOF1022_NHOM1_SD2003_Ver2.Services
{
    public interface IFineCalculationService
    {
        Task<Fines> CalculateAndCreateFineAsync(int borrowDetailId, string condition, int? damagePercent = null);
        Task<Fines> UpdateExistingFineAsync(int fineId, string condition, int? damagePercent = null);
        decimal CalculateDamageFine(int damagePercent, decimal bookPrice);
        decimal CalculateLostFine(decimal bookPrice);
        decimal CalculateLateFine(int lateDays);
        string GenerateReason(string condition, int? damagePercent, int lateDays);
        Models.Enums.FinesType DetermineFineType(string condition, bool isLate, int? damagePercent);
    }
}
