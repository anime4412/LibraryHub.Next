using System.Net;
using System.Net.Mail;
using LibraryHub.Application;
using LibraryHub.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LibraryHub.Infrastructure;

public sealed class SystemClock : IClock { public DateTimeOffset UtcNow => DateTimeOffset.UtcNow; }

public sealed class NotificationPublisher(LibraryDbContext db, IRealtimeGateway realtime) : INotificationPublisher
{
    public async Task PublishAsync(Guid accountId, NotificationType type, string title, string message,
        string? link, string? correlationId, CancellationToken cancellationToken)
    {
        var correlationKey = string.IsNullOrWhiteSpace(correlationId) ? null : correlationId.Trim();
        if (correlationKey is not null && await db.Notifications.AsNoTracking().AnyAsync(
                x => x.RecipientAccountId == accountId
                    && x.Type == type
                    && x.CorrelationKey == correlationKey,
                cancellationToken))
            return;

        var item = new Notification
        {
            RecipientAccountId = accountId,
            Type = type,
            Title = title,
            Message = message,
            Link = link,
            CorrelationKey = correlationKey
        };
        db.Notifications.Add(item);
        await db.SaveChangesAsync(cancellationToken);
        await realtime.NotifyAccountAsync(accountId, item, cancellationToken);
    }
}

public sealed class SmtpOptions
{
    public const string Section = "Email:Smtp";
    public string Host { get; init; } = "";
    public int Port { get; init; } = 587;
    public string Username { get; init; } = "";
    public string Password { get; init; } = "";
    public string FromAddress { get; init; } = "";
    public string FromName { get; init; } = "LibraryHub";
}

public sealed class SmtpEmailSender(
    IOptions<SmtpOptions> options,
    ILogger<SmtpEmailSender> logger) : IEmailSender
{
    public async Task<EmailDeliveryResult> SendAsync(
        string recipient,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken)
    {
        var o = options.Value;
        var fromAddress = string.IsNullOrWhiteSpace(o.FromAddress) ? o.Username : o.FromAddress;
        var configured = !string.IsNullOrWhiteSpace(o.Host)
            && o.Port is > 0 and <= 65535
            && !string.IsNullOrWhiteSpace(o.Username)
            && !string.IsNullOrWhiteSpace(o.Password)
            && !string.IsNullOrWhiteSpace(fromAddress);
        if (!configured)
        {
            logger.LogWarning("SMTP chưa được cấu hình đầy đủ; thư không được gửi.");
            return new EmailDeliveryResult(false, false);
        }

        try
        {
            var from = new MailAddress(fromAddress, o.FromName);
            using var message = new MailMessage
            {
                From = from,
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            message.To.Add(recipient);
            using var client = new SmtpClient(o.Host, o.Port)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(o.Username, o.Password)
            };
            await client.SendMailAsync(message, cancellationToken);
            logger.LogInformation("SMTP đã chấp nhận một thư từ LibraryHub.");
            return new EmailDeliveryResult(true, true);
        }
        catch (SmtpException exception)
        {
            logger.LogWarning(
                "SMTP từ chối thư. Mã trạng thái: {StatusCode}.",
                exception.StatusCode);
            return new EmailDeliveryResult(true, false);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogWarning(
                "Không thể gửi thư qua SMTP. Loại lỗi: {ExceptionType}.",
                exception.GetType().Name);
            return new EmailDeliveryResult(true, false);
        }
    }
}

public static class InfrastructureRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<LibraryDbContext>(options =>
        {
            var connection = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connection))
                throw new InvalidOperationException("Chưa cấu hình ConnectionStrings:DefaultConnection.");

            options.UseSqlServer(connection, sql => sql.EnableRetryOnFailure());
        });
        services.AddScoped<ILibraryStore>(x => x.GetRequiredService<LibraryDbContext>());
        services.AddScoped<IPermissionSeeder, PermissionSeeder>();
        services.AddScoped<INotificationPublisher, NotificationPublisher>();
        services.AddSingleton<IClock, SystemClock>();
        services.Configure<SmtpOptions>(configuration.GetSection(SmtpOptions.Section));
        services.AddTransient<IEmailSender, SmtpEmailSender>();
        return services;
    }
}
