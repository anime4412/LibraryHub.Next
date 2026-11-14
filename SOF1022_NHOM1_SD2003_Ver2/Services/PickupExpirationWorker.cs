using Microsoft.EntityFrameworkCore;
using SOF1022_NHOM1_SD2003_Ver2.Datas;
using SOF1022_NHOM1_SD2003_Ver2.Models;
using SOF1022_NHOM1_SD2003_Ver2.Models.Enums;

namespace SOF1022_NHOM1_SD2003_Ver2.Services
{
    public class PickupExpirationWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PickupExpirationWorker> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(2); // Check every 2 hours

        public PickupExpirationWorker(
            IServiceProvider serviceProvider,
            ILogger<PickupExpirationWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Pickup Expiration Worker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessExpiredSlipsAsync(stoppingToken);
                    await Task.Delay(_checkInterval, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // Task was cancelled, exit gracefully
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing expired slips");
                    await Task.Delay(_checkInterval, stoppingToken);
                }
            }

            _logger.LogInformation("Pickup Expiration Worker stopped.");
        }

        private async Task ProcessExpiredSlipsAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            var expiredSlips = await context.BorrowSlips
                .Include(x => x.BorrowDetails)
                .ThenInclude(x => x.Books)
                .Where(s => s.BorrowStatus == BorrowStatus.ApprovedAwaitingPickup 
                           && s.PickupDeadline.HasValue 
                           && s.PickupDeadline < DateTime.Now)
                .ToListAsync(stoppingToken);

            if (expiredSlips.Any())
            {
                _logger.LogInformation($"Found {expiredSlips.Count} expired pickup slips to process.");

                foreach (var slip in expiredSlips)
                {
                    try
                    {
                        slip.BorrowStatus = BorrowStatus.CanceledDueToTimeout;
                        slip.Note = "Tự động hủy do quá hạn nhận sách";

                        // KHÔNG cộng lại AvailableQuantity (kho vật lý): sách ở trạng thái
                        // ApprovedAwaitingPickup chưa từng được trừ kho (kho chỉ trừ khi thủ thư
                        // "Xác nhận giao sách"), nên hủy phiếu ở đây không được cộng kho, nếu không
                        // sẽ làm AvailableQuantity bị đội lên sai so với thực tế theo thời gian.
                        // Việc hủy này chỉ đơn giản là giải phóng "số lượng đã cam kết" - số lượng đó
                        // vốn được tính động (Quantity - committed) nên tự động trở lại đúng ngay khi
                        // trạng thái phiếu đổi sang CanceledDueToTimeout.
                        foreach (var detail in slip.BorrowDetails)
                        {
                            if (detail.Books != null)
                            {
                                _logger.LogInformation($"Released commitment for book: {detail.Books.Title} (ID: {detail.Books.Id})");
                            }
                        }

                        _logger.LogInformation($"Canceled expired slip #{slip.Id} for reader {slip.ReaderId}");

                        // Send notification to user
                        await notificationService.NotifyUserAsync(
                            slip.ReaderId,
                            "Reader",
                            NotificationType.BorrowCancelled,
                            "Phiếu mượn đã bị hủy do quá hạn nhận sách",
                            $"Phiếu mượn {slip.BorrowCode} đã tự động bị hủy vì bạn chưa đến nhận sách đúng hạn.",
                            link: "/BorrowSlips/MyBorrows",
                            createdBy: "Hệ thống");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error processing slip #{slip.Id}");
                    }
                }

                await context.SaveChangesAsync(stoppingToken);
                _logger.LogInformation($"Successfully processed {expiredSlips.Count} expired slips.");
            }
            else
            {
                _logger.LogDebug("No expired pickup slips found.");
            }
        }
    }
}
