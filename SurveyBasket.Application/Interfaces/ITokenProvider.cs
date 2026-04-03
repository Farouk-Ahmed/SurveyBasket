using SurveyBasket.Domain.Entities;

namespace SurveyBasket.Application.Interfaces;

public interface ITokenProvider
{
    (string token, int ExpiresIn) GenerateToken(AppUser user, IList<string> roles);
}
