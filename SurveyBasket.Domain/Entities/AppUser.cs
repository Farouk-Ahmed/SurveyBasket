using Microsoft.AspNetCore.Identity;

namespace SurveyBasket.Domain.Entities;

public sealed class AppUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public List<RefreshToken> RefreshTokens { get; set; } = new();
}
