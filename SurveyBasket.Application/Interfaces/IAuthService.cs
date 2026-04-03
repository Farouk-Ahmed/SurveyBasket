using SurveyBasket.Application.DTOs.Auth;
using SurveyBasket.Domain.Abstractions;

namespace SurveyBasket.Application.Interfaces;

public interface IAuthService
{
    Task<Result<AuthResponse>> AuthResponseAsync(string email, string password, CancellationToken cancellationToken);
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
    Task<Result<AuthResponse>> RegisterSimpleAsync(SimpleRegisterRequest request, CancellationToken cancellationToken);
    Task<AuthResponse?> RefreshTokenAsync(RefreshRequest request, CancellationToken cancellationToken);
    Task<Result<CreateUserResponse>> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken);
}
