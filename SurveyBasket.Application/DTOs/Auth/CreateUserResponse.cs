namespace SurveyBasket.Application.DTOs.Auth;

public record CreateUserResponse(string Id, string UserName, string? Email, string FirstName, string LastName, string Token, int ExpireIn, string RefreshToken, IList<string> Roles);
