using Microsoft.AspNetCore.Identity;
using SurveyBasket.Contract.Auth.Response;
using SurveyBasket.Contract.Auth.Request;
using SurveyBasket.Entities;
using System.Security.Cryptography;
using SurveyBasket.Services.Authntchan;
using System.IdentityModel.Tokens.Jwt;
using SurveyBasket.Abstractions;
using SurveyBasket.Errors;

namespace SurveyBasket.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenProvedr _tokenProveder;

        public AuthService(UserManager<AppUser> userManager, ITokenProvedr tokenProveder)
        {
            _userManager = userManager;
            _tokenProveder = tokenProveder;
        }

        public async Task<Result<AuthResponse>> AuthResponseAsync(string Email, string Password, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null)
                return Result.Failure<AuthResponse>(UserErrors.invalidCredentials);

            var passwordValid = await _userManager.CheckPasswordAsync(user, Password);
            if (!passwordValid)
                return Result.Failure<AuthResponse>(UserErrors.invalidCredentials);

            // Get user roles
            var roles = await _userManager.GetRolesAsync(user);

            // Generate refresh token
            var refreshToken = new RefrechTokens
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                ExpirationOn = DateTime.UtcNow.AddDays(7),
                CreeatedOn = DateTime.UtcNow
            };
            user.RefrechTokens.Add(refreshToken);
            await _userManager.UpdateAsync(user);

            var (token, ExpireIn) = _tokenProveder.GenerateToken(user, roles);
            var authResponse = new AuthResponse(user.Id, user.Email, user.FirstName, user.LastName, token, ExpireIn, refreshToken.Token, roles);
            return Result.Success(authResponse);
        }

        public async Task<AuthResponse?> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
        {
            var user = new AppUser
            {
                UserName = request.UserName,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName
            };
            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                return null;

            // Assign default User role
            await _userManager.AddToRoleAsync(user, DefaultRoles.User);
            var roles = await _userManager.GetRolesAsync(user);

            // Create refresh token
            var refreshToken = new RefrechTokens
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                ExpirationOn = DateTime.UtcNow.AddDays(7),
                CreeatedOn = DateTime.UtcNow
            };
            user.RefrechTokens.Add(refreshToken);
            await _userManager.UpdateAsync(user);

            var (token, ExpireIn) = _tokenProveder.GenerateToken(user, roles);
            return new AuthResponse(user.Id, user.Email, user.FirstName, user.LastName, token, ExpireIn, refreshToken.Token, roles);
        }

        public async Task<AuthResponse?> RefreshTokenAsync(RefreshRequest request, CancellationToken cancellationToken)
        {
            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwt;
            try
            {
                jwt = handler.ReadJwtToken(request.AccessToken);
            }
            catch
            {
                return null;
            }

            var userId = jwt.Subject;
            if (string.IsNullOrEmpty(userId))
                return null;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return null;

            var existing = user.RefrechTokens.FirstOrDefault(rt => rt.Token == request.RefreshToken);
            if (existing == null || !existing.IsActive)
                return null;

            // Revoke old token
            existing.RevokeOn = DateTime.UtcNow;

            // Create new refresh token
            var newRefresh = new RefrechTokens
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                ExpirationOn = DateTime.UtcNow.AddDays(7),
                CreeatedOn = DateTime.UtcNow
            };
            user.RefrechTokens.Add(newRefresh);
            await _userManager.UpdateAsync(user);

            // Get user roles
            var roles = await _userManager.GetRolesAsync(user);

            var (token, ExpiresIn) = _tokenProveder.GenerateToken(user, roles);
            return new AuthResponse(user.Id, user.Email, user.FirstName, user.LastName, token, ExpiresIn, newRefresh.Token, roles);
        }

        public async Task<Result<AuthResponse>> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken)
        {
            // Validate role
            if (request.Role != DefaultRoles.Admin && request.Role != DefaultRoles.User)
                return Result.Failure<AuthResponse>(UserErrors.InvalidRole);

            var user = new AppUser
            {
                UserName = request.UserName,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                return Result.Failure<AuthResponse>(UserErrors.RegistrationFailed);

            // Assign the specified role
            await _userManager.AddToRoleAsync(user, request.Role);
            var roles = await _userManager.GetRolesAsync(user);

            // Create refresh token
            var refreshToken = new RefrechTokens
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                ExpirationOn = DateTime.UtcNow.AddDays(7),
                CreeatedOn = DateTime.UtcNow
            };
            user.RefrechTokens.Add(refreshToken);
            await _userManager.UpdateAsync(user);

            var (token, ExpireIn) = _tokenProveder.GenerateToken(user, roles);
            return Result.Success(new AuthResponse(user.Id, user.Email, user.FirstName, user.LastName, token, ExpireIn, refreshToken.Token, roles));
        }
    }
}
