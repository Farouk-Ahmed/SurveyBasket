namespace SurveyBasket.Infrastructure.ExternalServices;

public class EmailSettings
{
    public const string SectionName = "EmailSettings";
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    /// <summary>Base URL of the API/site (e.g. https://localhost:7270) so the email logo loads in inbox.</summary>
    public string? BaseUrl { get; set; }
}
