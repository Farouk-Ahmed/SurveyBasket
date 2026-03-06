using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using SurveyBasket.Settings;

namespace SurveyBasket.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> settings, IWebHostEnvironment environment, ILogger<EmailService> logger)
        {
            _settings = settings.Value;
            _environment = environment;
            _logger = logger;
        }

        public async Task SendWelcomeEmailAsync(string toEmail, CancellationToken cancellationToken = default)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
                message.To.Add(MailboxAddress.Parse(toEmail));
                message.Subject = "Welcome to SurveyBasket! 🎉";

                var htmlBody = await LoadTemplateAsync("welcome-email.html", cancellationToken);
                htmlBody = htmlBody
                    .Replace("{{EMAIL}}", toEmail)
                    .Replace("{{YEAR}}", DateTime.UtcNow.Year.ToString());

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = htmlBody
                };

                message.Body = bodyBuilder.ToMessageBody();

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls, cancellationToken);
                await smtp.AuthenticateAsync(_settings.SenderEmail, _settings.Password, cancellationToken);
                await smtp.SendAsync(message, cancellationToken);
                await smtp.DisconnectAsync(true, cancellationToken);

                _logger.LogInformation("Welcome email sent successfully to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send welcome email to {Email}", toEmail);
                // Don't throw — email failure shouldn't block registration
            }
        }

        private async Task<string> LoadTemplateAsync(string templateName, CancellationToken cancellationToken)
        {
            var templatePath = Path.Combine(
                _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
                "templates",
                templateName
            );

            return await File.ReadAllTextAsync(templatePath, cancellationToken);
        }
    }
}
