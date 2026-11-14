using Microsoft.EntityFrameworkCore;
using SOF1022_NHOM1_SD2003_Ver2.Datas;
using SOF1022_NHOM1_SD2003_Ver2.Models.Enums;

namespace SOF1022_NHOM1_SD2003_Ver2.Helpers
{
    /// <summary>
    /// Helper tính "số lượng còn thực tế" của sách để hiển thị cho độc giả / dùng để quyết định
    /// có cho mượn tự động (auto-approve) hay bắt buộc phải đặt trước.
    ///
    /// Quy tắc nghiệp vụ:
    /// - Books.Quantity        = tổng số bản in thư viện sở hữu.
    /// - Books.AvailableQuantity = số bản in ĐANG NẰM TRONG KHO (vật lý), chỉ bị trừ khi thủ thư
    ///                             "Xác nhận giao sách" (Deliver/ConfirmHandover) và chỉ được cộng lại
    ///                             khi độc giả trả sách (Return). Các phiếu ở trạng thái
    ///                             PendingApproval / ApprovedAwaitingPickup KHÔNG được trừ vào đây.
    /// - "Số đã cam kết" (Committed) = số sách đã được giữ chỗ cho một độc giả nào đó, dù đã lấy
    ///                             sách hay chưa: gồm các phiếu PendingApproval, ApprovedAwaitingPickup
    ///                             và Borrowing (chưa trả).
    /// - "Số còn để mượn/hiển thị" = Quantity - Committed. Đây là con số phải giảm ngay khi hệ thống
    ///                             tự động duyệt một lượt mượn, và khi về 0 thì không cho mượn thêm
    ///                             (độc giả phải chuyển sang Đặt trước).
    /// </summary>
    public static class BookAvailabilityHelper
    {
        private static readonly BorrowStatus[] CommittedStatuses =
        {
            BorrowStatus.PendingApproval,
            BorrowStatus.ApprovedAwaitingPickup,
            BorrowStatus.Borrowing
        };

        /// <summary>
        /// Đếm số lượng bản sách của 1 đầu sách đang bị "cam kết" (chờ duyệt/chờ nhận/đang mượn, chưa trả).
        /// </summary>
        public static Task<int> GetCommittedCountAsync(LibraryDbContext context, int bookId)
        {
            return context.BorrowDetails
                .CountAsync(bd => bd.BookId == bookId
                    && !bd.IsReturned
                    && bd.BorrowSlips != null
                    && CommittedStatuses.Contains(bd.BorrowSlips.BorrowStatus));
        }

        /// <summary>
        /// Đếm số lượng cam kết cho nhiều đầu sách cùng lúc (tránh N+1 query khi hiển thị danh sách).
        /// Trả về Dictionary&lt;BookId, SoLuongDaCamKet&gt;.
        /// </summary>
        public static async Task<Dictionary<int, int>> GetCommittedCountsAsync(LibraryDbContext context, IEnumerable<int> bookIds)
        {
            var ids = bookIds.Distinct().ToList();

            var counts = await context.BorrowDetails
                .Where(bd => ids.Contains(bd.BookId)
                    && !bd.IsReturned
                    && bd.BorrowSlips != null
                    && CommittedStatuses.Contains(bd.BorrowSlips.BorrowStatus))
                .GroupBy(bd => bd.BookId)
                .Select(g => new { BookId = g.Key, Count = g.Count() })
                .ToListAsync();

            var result = ids.ToDictionary(id => id, id => 0);
            foreach (var c in counts)
            {
                result[c.BookId] = c.Count;
            }
            return result;
        }

        /// <summary>
        /// Số lượng còn thực tế có thể cho mượn/đặt trước = Tổng số bản in - Số đã cam kết.
        /// Không được dưới 0.
        /// </summary>
        public static async Task<int> GetRealAvailableCountAsync(LibraryDbContext context, int bookId, int totalQuantity)
        {
            var committed = await GetCommittedCountAsync(context, bookId);
            return Math.Max(0, totalQuantity - committed);
        }
    }
}
