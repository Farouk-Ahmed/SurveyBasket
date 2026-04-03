namespace SurveyBasket.Application.Interfaces;

public interface IEmailService
{
    Task SendWelcomeEmailAsync(string toEmail, CancellationToken cancellationToken = default);
}
