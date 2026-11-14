using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SOF1022_NHOM1_SD2003_Ver2.Datas;
using SOF1022_NHOM1_SD2003_Ver2.Helpers;
using SOF1022_NHOM1_SD2003_Ver2.Models;
using SOF1022_NHOM1_SD2003_Ver2.Models.Enums;
using SOF1022_NHOM1_SD2003_Ver2.Models.ViewModels;
using System.Diagnostics;

using SOF1022_NHOM1_SD2003_Ver2.Services;

namespace SOF1022_NHOM1_SD2003_Ver2.Controllers
{
    public class HomeController : Controller
    {
        private readonly LibraryDbContext _context;
        private readonly IRecommendationService _recommendationService;

        public HomeController(LibraryDbContext context, IRecommendationService recommendationService)
        {
            _context = context;
            _recommendationService = recommendationService;
        }

        public async Task<IActionResult> Index()
        {
            HomeViewModel model = new HomeViewModel();

            // Thống kê
            model.TotalBooks = await _context.Books.SumAsync(x => x.Quantity);
            model.TotalReaders = await _context.Readers.CountAsync();
            model.TotalBorrow = await _context.BorrowSlips.CountAsync();

            // ===========================
            // Sách mới nhất - sắp xếp theo thời gian cập nhật (LastBorrowDate hoặc Id)
            // ===========================
            model.NewBooks = await _context.Books
                .Where(x => x.BooksStatus == BooksStatus.Available)
                .Include(x => x.Author)
                .Include(x => x.Category)
                .OrderByDescending(x => x.LastBorrowDate ?? DateTime.MinValue)
                .ThenByDescending(x => x.Id)
                .Take(8)
                .ToListAsync();

            // ===========================
            // Sách hot - sắp xếp theo số lượt mượn
            // ===========================
            model.HotBooks = await _context.Books
                .Where(x => x.BooksStatus == BooksStatus.Available)
                .Include(x => x.Author)
                .Include(x => x.Category)
                .OrderByDescending(x => x.BorrowDetails.Count)
                .Take(8)
                .ToListAsync();

            // ===========================
            // Sách được yêu thích - sắp xếp theo số lượt thích
            // ===========================
            model.FavoriteBooks = await _context.Books
                .Where(x => x.BooksStatus == BooksStatus.Available)
                .Include(x => x.Author)
                .Include(x => x.Category)
                .OrderByDescending(x => x.BookFavorites.Count)
                .Take(8)
                .ToListAsync();

            // ===========================
            // Sách được đánh giá cao - sắp xếp theo đánh giá từ thấp đến cao
            // ===========================
            var ratedBooksQuery = _context.Books
                .Where(x => x.BooksStatus == BooksStatus.Available && x.BookRatings != null && x.BookRatings.Any())
                .Include(x => x.Author)
                .Include(x => x.Category)
                .Include(x => x.BookRatings)
                .AsEnumerable();
            
            model.HighlyRatedBooks = ratedBooksQuery
                .Select(x => new 
                {
                    Book = x,
                    AverageRating = x.BookRatings != null && x.BookRatings.Any() 
                        ? x.BookRatings.Average(r => r.Rating) 
                        : 0
                })
                .OrderByDescending(x => x.AverageRating) // Sắp xếp từ cao đến thấp
                .Take(8)
                .Select(x => x.Book)
                .ToList();

            // ===========================
            // Sách gợi ý cá nhân hóa (AI / Content-Based Recommendations)
            // ===========================
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            model.RecommendBooks = await _recommendationService.GetPersonalizedRecommendationsAsync(currentUserId, 6);

            // ===========================
            // Kệ Sách Tinh Hoa 3D (3D Bookshelf Hero)
            // ===========================
            var shelfBookIds = new List<int> { 64, 61, 62, 63, 65, 66, 67, 68, 60, 1, 2, 59 };
            var shelfBooks = await _context.Books
                .Where(x => shelfBookIds.Contains(x.Id) && x.BooksStatus == BooksStatus.Available)
                .Include(x => x.Author)
                .Include(x => x.Category)
                .ToListAsync();

            // Sắp xếp theo đúng thứ tự thẩm mỹ cho kệ 3D
            model.FeaturedShelfBooks = shelfBookIds
                .Select(id => shelfBooks.FirstOrDefault(b => b.Id == id))
                .Where(b => b != null)
                .Select(b => b!)
                .ToList();

            if (model.FeaturedShelfBooks.Count < 8)
            {
                var additional = await _context.Books
                    .Where(x => !shelfBookIds.Contains(x.Id) && x.BooksStatus == BooksStatus.Available && !string.IsNullOrEmpty(x.CoverImage))
                    .Include(x => x.Author)
                    .Include(x => x.Category)
                    .Take(12 - model.FeaturedShelfBooks.Count)
                    .ToListAsync();
                model.FeaturedShelfBooks.AddRange(additional);
            }

            // ===========================
            // Calculate real available counts for all books
            // ===========================
            var allBookIds = model.NewBooks.Select(x => x.Id)
                .Union(model.HotBooks.Select(x => x.Id))
                .Union(model.FavoriteBooks.Select(x => x.Id))
                .Union(model.HighlyRatedBooks.Select(x => x.Id))
                .Union(model.RecommendBooks.Select(x => x.Id))
                .Union(model.FeaturedShelfBooks.Select(x => x.Id))
                .ToList();

            var committedCounts = await BookAvailabilityHelper.GetCommittedCountsAsync(_context, allBookIds);
            
            ViewData["RealAvailableCounts"] = committedCounts;

            // ===========================
            // Danh mục
            // ===========================

            var allCategories = await _context.Categories
                .Include(c => c.Books)
                .Where(c => c.Books != null && c.Books.Any())
                .ToListAsync();

            model.Categories = allCategories
                .OrderByDescending(x => x.Books?.Count ?? 0)
                .Take(8)
                .ToList();

            model.ExploreCategories = allCategories
                .Except(model.Categories)
                .OrderBy(x => Guid.NewGuid())
                .Take(6)
                .ToList();

            // ===========================
            // Tác giả tiêu biểu
            // ===========================

            model.Authors = await _context.Authors
                .Include(a => a.Books)
                .Where(a => a.Books != null && a.Books.Any())
                .OrderByDescending(a => a.Books.Count)
                .Take(8)
                .ToListAsync();

            // ===========================
            // Nhà xuất bản đồng hành
            // ===========================

            model.Publishers = await _context.Publishers
                .Include(p => p.Books)
                .Where(p => p.Books != null && p.Books.Any())
                .OrderByDescending(p => p.Books.Count)
                .Take(6)
                .ToListAsync();

            return View(model);
        }

        public IActionResult CurvedArchive()
        {
            return View();
        }

        public IActionResult Archive3D()
        {
            return View("CurvedArchive");
        }

        public IActionResult Book3D()
        {
            return View("CurvedArchive");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
