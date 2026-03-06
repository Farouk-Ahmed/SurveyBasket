namespace SurveyBasket.Services
{
    public interface IEmailService
    {
        Task SendWelcomeEmailAsync(string toEmail, CancellationToken cancellationToken = default);
    }
}
