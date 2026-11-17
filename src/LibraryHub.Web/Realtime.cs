using System.Security.Claims;
using LibraryHub.Application;
using LibraryHub.Domain;
using LibraryHub.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace LibraryHub.Web;

[Authorize]
public sealed class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        if (Context.UserIdentifier is { Length: > 0 } id)
            await Groups.AddToGroupAsync(Context.ConnectionId, $"account:{id}");
        await base.OnConnectedAsync();
    }
}

[Authorize]
public sealed class SupportHub(LibraryDbContext db) : Hub
{
    public async Task JoinTicket(int ticketId)
    {
        var user = Context.User ?? throw new HubException("Bạn chưa đăng nhập.");
        if (!user.IsInRole("Staff") && !user.IsInRole("Administrator"))
        {
            if (!Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var accountId))
                throw new HubException("Bạn chưa đăng nhập.");
            var readerId = await db.Readers.AsNoTracking()
                .Where(x => x.UserAccountId == accountId && !x.IsDeleted)
                .Select(x => (int?)x.Id)
                .SingleOrDefaultAsync(Context.ConnectionAborted);
            var ownsTicket = readerId.HasValue && await db.SupportTickets
                .AnyAsync(x => x.Id == ticketId && x.ReaderId == readerId.Value, Context.ConnectionAborted);
            if (!ownsTicket) throw new HubException("Bạn không có quyền tham gia cuộc trao đổi này.");
        }
        await Groups.AddToGroupAsync(Context.ConnectionId, $"ticket:{ticketId}");
    }
    public Task LeaveTicket(int ticketId) => Groups.RemoveFromGroupAsync(Context.ConnectionId, $"ticket:{ticketId}");
}

public sealed class SignalRRealtimeGateway(IHubContext<NotificationHub> hub) : IRealtimeGateway
{
    public Task NotifyAccountAsync(Guid accountId, Notification notification, CancellationToken cancellationToken) =>
        hub.Clients.Group($"account:{accountId}").SendAsync("notification", new
        {
            notification.Id, notification.Type, notification.Title, notification.Message,
            notification.Link, notification.CreatedAt
        }, cancellationToken);
}

public sealed class PickupExpirationWorker(IServiceScopeFactory scopes, ILogger<PickupExpirationWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));
        try
        {
            await RunSweepAsync(stoppingToken);
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await RunSweepAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            logger.LogDebug("Tác vụ hủy phiếu quá hạn nhận sách đã dừng cùng ứng dụng.");
        }
    }

    private async Task RunSweepAsync(CancellationToken stoppingToken)
    {
        try
        {
            await using var scope = scopes.CreateAsyncScope();
            var borrows = scope.ServiceProvider.GetRequiredService<IBorrowService>();
            var borrowCount = await borrows.CancelExpiredPickupsAsync(stoppingToken);
            var overdueCount = await borrows.ProcessOverdueAndDueSoonAsync(stoppingToken);
            var reservationCount = await scope.ServiceProvider.GetRequiredService<IReservationService>()
                .CancelExpiredAsync(stoppingToken);
            if (borrowCount > 0)
                logger.LogInformation("Đã hủy {Count} phiếu quá hạn nhận sách.", borrowCount);
            if (overdueCount > 0)
                logger.LogInformation("Đã chuyển {Count} phiếu sang trạng thái quá hạn.", overdueCount);
            if (reservationCount > 0)
                logger.LogInformation("Đã hết hạn {Count} lượt đặt trước quá hạn nhận sách.", reservationCount);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Tác vụ nền kiểm tra hạn mượn/đặt trước gặp lỗi.");
        }
    }
}
