using SurveyBasket.Abstractions;
using SurveyBasket.Contract.Auth.Request;
using SurveyBasket.Contract.Auth.Response;

namespace SurveyBasket.Services
{
	public interface IAuthService
	{
        Task<Result<AuthResponse>> AuthResponseAsync(string Email, string Password, CancellationToken cancellationToken);
        Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
        Task<AuthResponse?> RefreshTokenAsync(RefreshRequest request, CancellationToken cancellationToken);
        Task<Result<CreateUserResponse>> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken);
	}
}
