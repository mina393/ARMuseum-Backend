namespace ARMuseum.Services
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string toEmail, string subject, string body);
    }
}
