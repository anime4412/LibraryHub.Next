namespace SOF1022_NHOM1_SD2003_Ver2.Services
{
    public interface IEmailService
    {
        Task SendEmailConfirmationAsync(string toEmail, string confirmationLink, string userName);
        Task SendEmailAsync(string toEmail, string subject, string htmlBody);
        Task SendDueReminderAsync(string toEmail, string readerName, string borrowCode, DateTime dueDate, List<string> bookTitles);
    }
}
