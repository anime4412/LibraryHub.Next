using Microsoft.EntityFrameworkCore;
using SOF1022_NHOM1_SD2003_Ver2.Datas;
using SOF1022_NHOM1_SD2003_Ver2.Models;
using SOF1022_NHOM1_SD2003_Ver2.Models.Enums;

namespace SOF1022_NHOM1_SD2003_Ver2.Services
{
    public class RecommendationService : IRecommendationService
    {
        private readonly LibraryDbContext _context;

        public RecommendationService(LibraryDbContext context)
        {
            _context = context;
        }

        public async Task<List<Books>> GetPersonalizedRecommendationsAsync(int? readerId, int count = 6)
        {
            // Fallback for guests or new readers without history
            if (readerId == null || readerId <= 0)
            {
                return await GetDefaultRecommendationsAsync(count);
            }

            // 1. Collect read / favorited / rated books by this reader
            var favoriteBookIds = await _context.BookFavorites
                .Where(f => f.ReaderId == readerId.Value)
                .Select(f => f.BookId)
                .ToListAsync();

            var borrowedBookIds = await _context.BorrowSlips
                .Where(s => s.ReaderId == readerId.Value)
                .SelectMany(s => s.BorrowDetails!.Select(d => d.BookId))
                .ToListAsync();

            var ratedBooks = await _context.BookRatings
                .Where(r => r.ReaderId == readerId.Value && r.Rating >= 4)
                .Select(r => r.BookId)
                .ToListAsync();

            var allInteractedIds = favoriteBookIds.Concat(borrowedBookIds).Concat(ratedBooks).Distinct().ToHashSet();

            // If user has zero interactions, return popular/highest rated books
            if (!allInteractedIds.Any())
            {
                return await GetDefaultRecommendationsAsync(count);
            }

            // 2. Extract Category and Author affinities
            var preferredCategories = await _context.Books
                .Where(b => allInteractedIds.Contains(b.Id))
                .GroupBy(b => b.CategoriesId)
                .Select(g => new { CategoryId = g.Key, Weight = g.Count() })
                .OrderByDescending(x => x.Weight)
                .Select(x => x.CategoryId)
                .Take(3)
                .ToListAsync();

            var preferredAuthors = await _context.Books
                .Where(b => allInteractedIds.Contains(b.Id))
                .GroupBy(b => b.AuthorsId)
                .Select(g => new { AuthorId = g.Key, Weight = g.Count() })
                .OrderByDescending(x => x.Weight)
                .Select(x => x.AuthorId)
                .Take(3)
                .ToListAsync();

            // 3. Find candidates matching preferred categories or authors that user hasn't interacted with
            var candidates = await _context.Books
                .Where(b => b.BooksStatus == BooksStatus.Available 
                            && !b.IsDeleted 
                            && !allInteractedIds.Contains(b.Id)
                            && (preferredCategories.Contains(b.CategoriesId) || preferredAuthors.Contains(b.AuthorsId)))
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Include(b => b.BookRatings)
                .Take(25)
                .ToListAsync();

            // Score each candidate
            var scoredCandidates = candidates.Select(b =>
            {
                double score = 0;
                if (preferredCategories.Contains(b.CategoriesId)) score += 5.0;
                if (preferredAuthors.Contains(b.AuthorsId)) score += 4.0;
                if (b.BookRatings != null && b.BookRatings.Any())
                {
                    score += b.BookRatings.Average(r => r.Rating);
                }
                return new { Book = b, Score = score };
            })
            .OrderByDescending(x => x.Score)
            .Take(count)
            .Select(x => x.Book)
            .ToList();

            // If candidates are fewer than required count, fill with top rated books
            if (scoredCandidates.Count < count)
            {
                var filler = await _context.Books
                    .Where(b => b.BooksStatus == BooksStatus.Available 
                                && !b.IsDeleted 
                                && !allInteractedIds.Contains(b.Id)
                                && !scoredCandidates.Select(s => s.Id).Contains(b.Id))
                    .Include(b => b.Author)
                    .Include(b => b.Category)
                    .OrderByDescending(b => b.ViewCount)
                    .Take(count - scoredCandidates.Count)
                    .ToListAsync();

                scoredCandidates.AddRange(filler);
            }

            return scoredCandidates;
        }

        public async Task<List<Books>> GetSimilarBooksAsync(int bookId, int count = 4)
        {
            var currentBook = await _context.Books.FindAsync(bookId);
            if (currentBook == null) return new List<Books>();

            return await _context.Books
                .Where(b => b.Id != bookId 
                            && b.BooksStatus == BooksStatus.Available 
                            && !b.IsDeleted 
                            && (b.CategoriesId == currentBook.CategoriesId || b.AuthorsId == currentBook.AuthorsId))
                .Include(b => b.Author)
                .Include(b => b.Category)
                .OrderByDescending(b => b.CategoriesId == currentBook.CategoriesId ? 2 : 1)
                .ThenByDescending(b => b.ViewCount)
                .Take(count)
                .ToListAsync();
        }

        public async Task<string> GetRecommendationReasonAsync(int? readerId, int bookId)
        {
            if (readerId == null || readerId <= 0) return "Sách thịnh hành được nhiều bạn đọc quan tâm";

            var book = await _context.Books.Include(b => b.Category).Include(b => b.Author).FirstOrDefaultAsync(b => b.Id == bookId);
            if (book == null) return "Gợi ý dành cho bạn";

            // Check if user has read books in this category
            var sameCategoryRead = await _context.BorrowSlips
                .Where(s => s.ReaderId == readerId.Value)
                .SelectMany(s => s.BorrowDetails!.Select(d => d.Books!))
                .AnyAsync(b => b.CategoriesId == book.CategoriesId);

            if (sameCategoryRead)
            {
                return $"Dựa trên sở thích thể loại {book.Category?.Name ?? "yêu thích"}";
            }

            var sameAuthorRead = await _context.BorrowSlips
                .Where(s => s.ReaderId == readerId.Value)
                .SelectMany(s => s.BorrowDetails!.Select(d => d.Books!))
                .AnyAsync(b => b.AuthorsId == book.AuthorsId);

            if (sameAuthorRead)
            {
                return $"Vì bạn đã từng đọc tác phẩm của {book.Author?.Name ?? "tác giả này"}";
            }

            return "Phù hợp với xu hướng đọc của bạn";
        }

        private async Task<List<Books>> GetDefaultRecommendationsAsync(int count)
        {
            return await _context.Books
                .Where(b => b.BooksStatus == BooksStatus.Available && !b.IsDeleted)
                .Include(b => b.Author)
                .Include(b => b.Category)
                .OrderByDescending(b => b.BorrowDetails!.Count)
                .ThenByDescending(b => b.ViewCount)
                .Take(count)
                .ToListAsync();
        }
    }
}
