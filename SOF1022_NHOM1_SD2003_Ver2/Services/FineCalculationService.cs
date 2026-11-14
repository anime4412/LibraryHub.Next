using Microsoft.EntityFrameworkCore;
using SOF1022_NHOM1_SD2003_Ver2.Datas;
using SOF1022_NHOM1_SD2003_Ver2.Models;
using SOF1022_NHOM1_SD2003_Ver2.Models.Enums;

namespace SOF1022_NHOM1_SD2003_Ver2.Services
{
    public class FineCalculationService : IFineCalculationService
    {
        private readonly LibraryDbContext _context;

        // Constants for fine calculation
        private const decimal DAMAGE_FINE_THRESHOLD_LOW = 50;
        private const decimal DAMAGE_FINE_THRESHOLD_HIGH = 80;
        private const decimal DAMAGE_FINE_AMOUNT = 10000m;
        private const decimal LATE_FINE_PER_DAY = 5000m;

        public FineCalculationService(LibraryDbContext context)
        {
            _context = context;
        }

        public async Task<Fines> CalculateAndCreateFineAsync(int borrowDetailId, string condition, int? damagePercent = null)
        {
            var borrowDetail = await _context.BorrowDetails
                .Include(bd => bd.Books)
                .Include(bd => bd.BorrowSlips)
                .FirstOrDefaultAsync(bd => bd.Id == borrowDetailId);

            if (borrowDetail == null)
                throw new ArgumentException("Borrow detail not found");

            if (borrowDetail.Books == null || borrowDetail.BorrowSlips == null)
                throw new ArgumentException("Invalid borrow detail data");

            var actualReturnDate = DateTime.Now;
            var isLate = actualReturnDate > borrowDetail.BorrowSlips.DueDate;
            int lateDays = isLate ? (actualReturnDate - borrowDetail.BorrowSlips.DueDate).Days : 0;

            // Calculate fines
            decimal damageFine = 0;
            decimal lostFine = 0;
            decimal lateFine = CalculateLateFine(lateDays);

            if (condition == "damaged" && damagePercent.HasValue)
            {
                damageFine = CalculateDamageFine(damagePercent.Value, borrowDetail.Books.Price);
            }
            else if (condition == "lost")
            {
                lostFine = CalculateLostFine(borrowDetail.Books.Price);
            }

            decimal totalFine = damageFine + lostFine + lateFine;

            // Only create fine if there's an actual fine amount
            if (totalFine <= 0)
                return null!;

            // Determine fine type
            var fineType = DetermineFineType(condition, isLate, damagePercent);

            // Generate reason
            var reason = GenerateReason(condition, damagePercent, lateDays);

            // Create fine record
            var fine = new Fines
            {
                BorrowSlipId = borrowDetail.BorrowSlipId,
                BorrowDetailId = borrowDetailId,
                BookId = borrowDetail.BookId,
                ReaderId = borrowDetail.BorrowSlips.ReaderId,
                Amount = totalFine,
                Reason = reason,
                FinesType = fineType,
                IsPaid = false,
                CreatedDate = DateTime.Now,
                DamagePercent = damagePercent,
                LateDays = lateDays > 0 ? lateDays : null,
                LateFine = lateFine > 0 ? lateFine : null,
                DamageFine = damageFine > 0 ? damageFine : null,
                LostFine = lostFine > 0 ? lostFine : null,
                Status = "Unpaid"
            };

            _context.Fines.Add(fine);
            await _context.SaveChangesAsync();

            return fine;
        }

        public async Task<Fines> UpdateExistingFineAsync(int fineId, string condition, int? damagePercent = null)
        {
            var fine = await _context.Fines
                .Include(f => f.BorrowDetails)
                    .ThenInclude(bd => bd.Books)
                .Include(f => f.BorrowSlips)
                .FirstOrDefaultAsync(f => f.Id == fineId);

            if (fine == null)
                throw new ArgumentException("Fine not found");

            if (fine.BorrowDetails == null || fine.BorrowDetails.Books == null || fine.BorrowSlips == null)
                throw new ArgumentException("Invalid fine data");

            var actualReturnDate = DateTime.Now;
            var isLate = actualReturnDate > fine.BorrowSlips.DueDate;
            int lateDays = isLate ? (actualReturnDate - fine.BorrowSlips.DueDate).Days : 0;

            // Recalculate fines
            decimal damageFine = 0;
            decimal lostFine = 0;
            decimal lateFine = CalculateLateFine(lateDays);

            if (condition == "damaged" && damagePercent.HasValue)
            {
                damageFine = CalculateDamageFine(damagePercent.Value, fine.BorrowDetails.Books.Price);
            }
            else if (condition == "lost")
            {
                lostFine = CalculateLostFine(fine.BorrowDetails.Books.Price);
            }

            decimal totalFine = damageFine + lostFine + lateFine;

            // Update fine
            fine.Amount = totalFine;
            fine.FinesType = DetermineFineType(condition, isLate, damagePercent);
            fine.Reason = GenerateReason(condition, damagePercent, lateDays);
            fine.DamagePercent = damagePercent;
            fine.LateDays = lateDays > 0 ? lateDays : null;
            fine.LateFine = lateFine > 0 ? lateFine : null;
            fine.DamageFine = damageFine > 0 ? damageFine : null;
            fine.LostFine = lostFine > 0 ? lostFine : null;

            await _context.SaveChangesAsync();

            return fine;
        }

        public decimal CalculateDamageFine(int damagePercent, decimal bookPrice)
        {
            // Calculate fine based on percentage of book price
            return (bookPrice * damagePercent) / 100;
        }

        public decimal CalculateLostFine(decimal bookPrice)
        {
            return bookPrice;
        }

        public decimal CalculateLateFine(int lateDays)
        {
            if (lateDays <= 0)
                return 0;

            return lateDays * LATE_FINE_PER_DAY;
        }

        public string GenerateReason(string condition, int? damagePercent, int lateDays)
        {
            var reasons = new List<string>();

            if (condition == "lost")
            {
                reasons.Add("Độc giả làm mất sách");
            }
            else if (condition == "damaged" && damagePercent.HasValue)
            {
                reasons.Add($"Sách hư hại {damagePercent}%");
            }

            if (lateDays > 0)
            {
                reasons.Add($"Trả sách quá hạn {lateDays} ngày");
            }

            return reasons.Count > 0 ? string.Join(" và ", reasons) : "Không có vi phạm";
        }

        public FinesType DetermineFineType(string condition, bool isLate, int? damagePercent)
        {
            if (condition == "lost")
            {
                return isLate ? FinesType.LateAndLost : FinesType.Lost;
            }
            else if (condition == "damaged")
            {
                return isLate ? FinesType.LateAndDamaged : FinesType.Damaged;
            }
            else
            {
                return FinesType.Late;
            }
        }
    }
}
