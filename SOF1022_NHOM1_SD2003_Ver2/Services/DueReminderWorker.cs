using Microsoft.EntityFrameworkCore;
using SOF1022_NHOM1_SD2003_Ver2.Datas;
using SOF1022_NHOM1_SD2003_Ver2.Models.Enums;

namespace SOF1022_NHOM1_SD2003_Ver2.Services
{
    public class DueReminderWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DueReminderWorker> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(12);

        public DueReminderWorker(IServiceScopeFactory scopeFactory, ILogger<DueReminderWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("DueReminderWorker background service started.");

            // Wait 15 seconds after app startup before first run
            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckAndSendDueRemindersAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while executing DueReminderWorker.");
                }

                try
                {
                    await Task.Delay(_checkInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            _logger.LogInformation("DueReminderWorker background service stopped.");
        }

        private async Task CheckAndSendDueRemindersAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            var today = DateTime.Today;
            var reminderThreshold = today.AddDays(2);

            var expiringSlips = await context.BorrowSlips
                .Include(s => s.Readers)
                .Include(s => s.BorrowDetails)
                    .ThenInclude(d => d.Books)
                .Where(s => s.BorrowStatus == BorrowStatus.Borrowing 
                            && s.DueDate.Date <= reminderThreshold 
                            && s.DueDate.Date >= today)
                .ToListAsync(cancellationToken);

            _logger.LogInformation("DueReminderWorker found {Count} borrow slips nearing due date.", expiringSlips.Count);

            foreach (var slip in expiringSlips)
            {
                if (slip.Readers == null) continue;

                var reader = slip.Readers;
                var borrowCode = slip.BorrowCode ?? $"PM{slip.Id:D5}";
                var bookTitles = slip.BorrowDetails?
                    .Where(d => !d.IsReturned && d.Books != null)
                    .Select(d => d.Books!.Title)
                    .ToList() ?? new List<string>();

                if (!bookTitles.Any()) continue;

                // 1. Send in-app notification
                try
                {
                    await notificationService.NotifyUserAsync(
                        recipientId: slip.ReaderId,
                        recipientType: "Reader",
                        type: NotificationType.System,
                        title: "Nhắc nhở: Sắp đến hạn trả sách!",
                        message: $"Phiếu mượn {borrowCode} có hạn trả vào ngày {slip.DueDate:dd/MM/yyyy}. Vui lòng trả sách đúng hạn hoặc gia hạn trực tuyến để tránh phát sinh phí phạt.",
                        link: $"/BorrowSlips/Details/{slip.Id}",
                        createdBy: "SystemReminder"
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send in-app notification for slip {SlipId}.", slip.Id);
                }

                // 2. Send email reminder if reader has email
                if (!string.IsNullOrWhiteSpace(reader.Email))
                {
                    try
                    {
                        await emailService.SendDueReminderAsync(
                            toEmail: reader.Email,
                            readerName: reader.FullName,
                            borrowCode: borrowCode,
                            dueDate: slip.DueDate,
                            bookTitles: bookTitles
                        );
                        _logger.LogInformation("Sent reminder email to {Email} for borrow slip {BorrowCode}.", reader.Email, borrowCode);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to send reminder email to {Email}.", reader.Email);
                    }
                }
            }
        }
    }
}
