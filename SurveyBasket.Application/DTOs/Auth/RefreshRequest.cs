namespace SurveyBasket.Application.DTOs.Auth;

public record RefreshRequest(string? AccessToken, string? RefreshToken);
