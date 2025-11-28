using SurveyBasket.Contract.Auth.Response;
using SurveyBasket.Contract.Auth.Request;
using Microsoft.AspNetCore.Identity;

namespace SurveyBasket.Services
{
	public interface IAuthService
	{
		Task<AuthResponse> AuthResponseAsync(string Email, string Password, CancellationToken cancellationToken);
        Task<AuthResponse?> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
        Task<AuthResponse?> RefreshTokenAsync(RefreshRequest request, CancellationToken cancellationToken);
	}
}
