using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SOF1022_NHOM1_SD2003_Ver2.Attributes;
using SOF1022_NHOM1_SD2003_Ver2.Datas;
using SOF1022_NHOM1_SD2003_Ver2.Models;

namespace SOF1022_NHOM1_SD2003_Ver2.Controllers
{
    [SessionAuthorize(Roles = "Admin")]
    [PermissionAuthorize("settings.edit")]
    public class SystemSettingsController : Controller
    {
        private readonly LibraryDbContext _context;

        public SystemSettingsController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET: SystemSettings
        public async Task<IActionResult> Index()
        {
            var settings = await _context.SystemSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                // Create default settings if none exist
                settings = new SystemSettings
                {
                    MaxExtensionDays = 7,
                    MaxExtensionCount = 1,
                    LateFeePerDay = 5000,
                    MaxBorrowBooks = 10,
                    BorrowPeriodDays = 14
                };
                _context.SystemSettings.Add(settings);
                await _context.SaveChangesAsync();
            }
            return View(settings);
        }

        // POST: SystemSettings/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize(Roles = "Admin,Librarian")]
        [PermissionAuthorize("settings.edit")]
        public async Task<IActionResult> Edit(SystemSettings settings)
        {
            if (ModelState.IsValid)
            {
                var existingSettings = await _context.SystemSettings.FirstOrDefaultAsync();
                if (existingSettings != null)
                {
                    existingSettings.MaxExtensionDays = settings.MaxExtensionDays;
                    existingSettings.MaxExtensionCount = settings.MaxExtensionCount;
                    existingSettings.LateFeePerDay = settings.LateFeePerDay;
                    existingSettings.MaxBorrowBooks = settings.MaxBorrowBooks;
                    existingSettings.BorrowPeriodDays = settings.BorrowPeriodDays;
                    existingSettings.UpdatedDate = DateTime.Now;
                }
                else
                {
                    settings.CreatedDate = DateTime.Now;
                    _context.SystemSettings.Add(settings);
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật cài đặt hệ thống thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View("Index", settings);
        }

        // Helper method to get current settings
        public static async Task<SystemSettings> GetCurrentSettings(LibraryDbContext context)
        {
            var settings = await context.SystemSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                // Return default settings if none exist
                return new SystemSettings
                {
                    MaxExtensionDays = 7,
                    MaxExtensionCount = 1,
                    LateFeePerDay = 5000,
                    MaxBorrowBooks = 10,
                    BorrowPeriodDays = 14
                };
            }
            return settings;
        }
    }
}
