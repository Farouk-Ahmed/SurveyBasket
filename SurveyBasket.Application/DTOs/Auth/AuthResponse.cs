namespace SurveyBasket.Application.DTOs.Auth;

public record AuthResponse(string Id, string? Email, string FirstName, string LastName, string Token, int ExpireIn, string RefreshToken, string? ProfilePicturePath);
