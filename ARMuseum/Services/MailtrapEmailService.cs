// File: Services/MailtrapEmailService.cs
using System.Net;
using System.Net.Mail;
using ARMuseum.Settings;
using Microsoft.Extensions.Options;

namespace ARMuseum.Services
{
    public class MailtrapEmailService : IEmailService
    {
        private readonly MailtrapSettings _settings;

        // The settings registered in Program.cs are injected here via the constructor.
        public MailtrapEmailService(IOptions<MailtrapSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string subject, string body)
        {
            // We use the settings to configure the SmtpClient for Mailtrap.
            var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                // Using the username and password from the injected settings.
                Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_settings.SenderEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = true, // Set to true to allow for HTML content in the email body.
            };
            mailMessage.To.Add(toEmail);

            try
            {
                await client.SendMailAsync(mailMessage);
                System.Console.WriteLine("Email sent successfully!");
            }
            catch (Exception ex)
            {
                // It's important to log this exception to diagnose any email sending issues.
                System.Console.WriteLine($"Failed to send email: {ex.Message}");
            }
        }
    }
}