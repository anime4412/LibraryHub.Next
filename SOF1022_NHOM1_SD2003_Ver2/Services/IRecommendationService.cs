using SOF1022_NHOM1_SD2003_Ver2.Models;

namespace SOF1022_NHOM1_SD2003_Ver2.Services
{
    public interface IRecommendationService
    {
        Task<List<Books>> GetPersonalizedRecommendationsAsync(int? readerId, int count = 6);
        Task<List<Books>> GetSimilarBooksAsync(int bookId, int count = 4);
        Task<string> GetRecommendationReasonAsync(int? readerId, int bookId);
    }
}
