using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SOF1022_NHOM1_SD2003_Ver2.Attributes;
using SOF1022_NHOM1_SD2003_Ver2.Datas;
using SOF1022_NHOM1_SD2003_Ver2.Models;
using SOF1022_NHOM1_SD2003_Ver2.Models.Enums;
using SOF1022_NHOM1_SD2003_Ver2.Models.ViewModels;

[SessionAuthorize]
[PermissionAuthorize("dashboard.view")]
public class DashboardController : Controller
{
    private readonly LibraryDbContext _context;

    public DashboardController(
        LibraryDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var now = DateTime.Now;

        // 1. 6 Months borrowing trend
        var monthLabels = new List<string>();
        var monthBorrowCounts = new List<int>();
        for (int i = 5; i >= 0; i--)
        {
            var targetDate = now.AddMonths(-i);
            monthLabels.Add($"T{targetDate.Month}/{targetDate.Year}");
            var count = await _context.BorrowSlips
                .CountAsync(x => x.BorrowDate.Year == targetDate.Year && x.BorrowDate.Month == targetDate.Month);
            monthBorrowCounts.Add(count);
        }

        // 2. Books by Category
        var categoryData = await _context.Books
            .Where(x => !x.IsDeleted && x.Category != null)
            .Include(x => x.Category)
            .GroupBy(x => x.Category!.Name)
            .Select(g => new { Name = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToListAsync();

        var catLabels = categoryData.Select(x => x.Name).ToList();
        var catCounts = categoryData.Select(x => x.Count).ToList();

        // 3. Top 5 Books
        var topBooks = await _context.BorrowDetails
            .Where(bd => bd.Books != null && !bd.Books.IsDeleted)
            .Include(bd => bd.Books)
            .GroupBy(bd => new { bd.BookId, bd.Books!.Title, bd.Books!.CoverImage })
            .Select(g => new TopBookViewModel
            {
                Title = g.Key.Title,
                BorrowCount = g.Count(),
                CoverImage = g.Key.CoverImage
            })
            .OrderByDescending(x => x.BorrowCount)
            .Take(5)
            .ToListAsync();

        if (topBooks.Count < 5)
        {
            var usedTitles = topBooks.Select(x => x.Title).ToHashSet();
            var backupBooks = await _context.Books
                .Where(b => !b.IsDeleted && !usedTitles.Contains(b.Title))
                .OrderByDescending(b => b.ViewCount)
                .Take(5 - topBooks.Count)
                .Select(b => new TopBookViewModel
                {
                    Title = b.Title,
                    BorrowCount = 0,
                    CoverImage = b.CoverImage
                })
                .ToListAsync();
            topBooks.AddRange(backupBooks);
        }

        // 4. Top Readers
        var topReaders = await _context.BorrowSlips
            .Where(bs => bs.Readers != null && !bs.Readers.IsDeleted && bs.Readers.Role == "Reader")
            .Include(bs => bs.Readers)
            .GroupBy(bs => new { bs.ReaderId, bs.Readers!.FullName, bs.Readers!.Email })
            .Select(g => new TopReaderViewModel
            {
                FullName = g.Key.FullName,
                Email = g.Key.Email,
                BorrowCount = g.Count()
            })
            .OrderByDescending(x => x.BorrowCount)
            .Take(5)
            .ToListAsync();

        if (topReaders.Count < 5)
        {
            var existingEmails = topReaders.Select(r => r.Email).ToList();
            var backupReaders = await _context.Readers
                .Where(r => r.Role == "Reader" && !r.IsDeleted && !existingEmails.Contains(r.Email))
                .OrderByDescending(r => r.RegisterDate)
                .Take(5 - topReaders.Count)
                .Select(r => new TopReaderViewModel
                {
                    FullName = r.FullName,
                    Email = r.Email,
                    BorrowCount = 0
                })
                .ToListAsync();
            topReaders.AddRange(backupReaders);
        }

        var defaultMembers = new[]
        {
            new TopReaderViewModel { FullName = "Hoàng Hải Yến", Email = "sinhvien03@fpt.edu.vn", BorrowCount = 0 },
            new TopReaderViewModel { FullName = "Đỗ Quốc Bảo", Email = "sinhvien04@fpt.edu.vn", BorrowCount = 0 },
            new TopReaderViewModel { FullName = "Vũ Thu Trang", Email = "sinhvien05@fpt.edu.vn", BorrowCount = 0 }
        };
        for (int i = 0; i < defaultMembers.Length && topReaders.Count < 5; i++)
        {
            topReaders.Add(defaultMembers[i]);
        }

        var model = new DashboardViewModel
        {
            TotalBooks = await _context.Books.Where(b => !b.IsDeleted).SumAsync(x => (int?)x.Quantity) ?? 0,
            AvailableBooks = await _context.Books.Where(b => !b.IsDeleted).SumAsync(x => (int?)x.AvailableQuantity) ?? 0,
            TotalReaders = await _context.Readers.CountAsync(r => r.Role == "Reader" && !r.IsDeleted),
            ActiveReaders = await _context.Readers.CountAsync(r => r.Role == "Reader" && !r.IsDeleted && r.IsActive),
            BorrowingBooks = await _context.BorrowSlips.CountAsync(x => x.BorrowStatus == BorrowStatus.Borrowing),
            OverdueBooks = await _context.BorrowSlips.CountAsync(x => x.DueDate < DateTime.Now && x.BorrowStatus == BorrowStatus.Borrowing),
            TotalAuthors = await _context.Authors.CountAsync(),
            TotalCategories = await _context.Categories.CountAsync(),
            TotalPublishers = await _context.Publishers.CountAsync(),
            TotalShelves = await _context.Shelves.CountAsync(),
            TotalReservations = await _context.Reservations.CountAsync(),
            TotalFines = await _context.Fines.CountAsync(),
            TotalFineAmount = await _context.Fines.SumAsync(x => (decimal?)x.Amount) ?? 0m,
            UnpaidFineAmount = await _context.Fines.Where(f => !f.IsPaid).SumAsync(x => (decimal?)x.Amount) ?? 0m,
            TotalBorrowSlips = await _context.BorrowSlips.CountAsync(),

            MonthLabels = monthLabels,
            MonthBorrowCounts = monthBorrowCounts,
            CategoryLabels = catLabels,
            CategoryCounts = catCounts,

            RecentBorrowSlips = await _context.BorrowSlips
                .Include(x => x.Readers)
                .OrderByDescending(x => x.BorrowDate)
                .Take(6)
                .ToListAsync(),

            TopBooksTable = topBooks,
            TopReadersTable = topReaders,

            LowStockBooks = await _context.Books
                .Include(x => x.Author)
                .Where(x => !x.IsDeleted && x.AvailableQuantity <= 3)
                .OrderBy(x => x.AvailableQuantity)
                .Take(5)
                .ToListAsync(),

            OverdueBorrowSlips = await _context.BorrowSlips
                .Include(x => x.Readers)
                .Where(x => x.DueDate < DateTime.Now && x.BorrowStatus == BorrowStatus.Borrowing)
                .OrderBy(x => x.DueDate)
                .Take(5)
                .ToListAsync(),

            NewReaders = await _context.Readers
                .Where(r => r.Role == "Reader" && !r.IsDeleted)
                .OrderByDescending(x => x.RegisterDate)
                .Take(5)
                .ToListAsync()
        };

        return View(model);
    }

    public async Task<IActionResult> BooksByCategory()
    {
        var data = await _context.Books
            .Where(x => x.Category != null)
            .Include(x => x.Category)
            .GroupBy(x => x.Category!.Name)
            .Select(g => new
            {
                category = g.Key,
                count = g.Count()
            })
            .ToListAsync();

        return Json(data);
    }

    public async Task<IActionResult> BorrowByMonth()
    {
        var data = await _context.BorrowSlips
            .GroupBy(x => x.BorrowDate.Month)
            .Select(g => new
            {
                month = g.Key,
                count = g.Count()
            })
            .OrderBy(x => x.month)
            .ToListAsync();

        return Json(data);
    }

    public async Task<IActionResult> TopBooks()
    {
        var data = await _context.BorrowDetails
            .Include(x => x.Books)
            .GroupBy(x => x.BookId)
            .Select(g => new
            {
                title = g.First().Books!.Title,
                count = g.Count()
            })
            .OrderByDescending(x => x.count)
            .Take(5)
            .ToListAsync();

        return Json(data);
    }

    public async Task<IActionResult> TopReaders()
    {
        var data = await _context.BorrowSlips
            .Include(x => x.Readers)
            .GroupBy(x => x.ReaderId)
            .Select(g => new
            {
                reader = g.First().Readers!.FullName,
                count = g.Count()
            })
            .OrderByDescending(x => x.count)
            .Take(5)
            .ToListAsync();

        return Json(data);
    }

    public async Task<IActionResult> RecentBorrows()
    {
        var data = await _context.BorrowSlips
            .Include(x => x.Readers)
            .OrderByDescending(x => x.BorrowDate)
            .Take(10)
            .ToListAsync();

        return Json(data);
    }

    public async Task<IActionResult> RecentReaders()
    {
        var data = await _context.Readers
            .OrderByDescending(x => x.RegisterDate)
            .Take(10)
            .ToListAsync();

        return Json(data);
    }

    public async Task<IActionResult> RecentBooks()
    {
        var data = await _context.Books
            .OrderByDescending(x => x.Id)
            .Take(10)
            .ToListAsync();

        return Json(data);
    }

    public async Task<IActionResult> LowStockBooks()
    {
        var data = await _context.Books
            .Where(x => x.AvailableQuantity <= 3)
            .ToListAsync();

        return Json(data);
    }

    public async Task<IActionResult> OverdueReaders()
    {
        var data = await _context.BorrowSlips
            .Include(x => x.Readers)
            .Where(x =>
                x.DueDate < DateTime.Now &&
                x.BorrowStatus == BorrowStatus.Borrowing)
            .ToListAsync();

        return Json(data);
    }

    public IActionResult AI()
    {
        return RedirectToAction(
            "Index",
            "AI");
    }
}
