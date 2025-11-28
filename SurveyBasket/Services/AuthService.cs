using Microsoft.AspNetCore.Identity;
using SurveyBasket.Contract.Auth.Response;
using SurveyBasket.Contract.Auth.Request;
using SurveyBasket.Entities;
using MassTransit;
using System.Security.Cryptography;
using System.Text;
using SurveyBasket.Services.Authntchan;
using System.IdentityModel.Tokens.Jwt;

namespace SurveyBasket.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenProvedr _tokenProveder;

        public AuthService(UserManager<AppUser> userManager, ITokenProvedr _tokenProveder)
        {
            _userManager = userManager;
            this._tokenProveder = _tokenProveder;
        }

        public async Task<AuthResponse> AuthResponseAsync(string Email, string Password, CancellationToken cancellationToken)
        {
            //check is User Found
            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null)
                return null;
            //check is password valid
            var passseordValid = await _userManager.CheckPasswordAsync(user, Password);
            if (!passseordValid)
                return null;

            // generate new refresh token on login
            var refreshToken = new RefrechTokens
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                ExpirationOn = DateTime.UtcNow.AddDays(7),
                CreeatedOn = DateTime.UtcNow
            };
            user.RefrechTokens.Add(refreshToken);
            await _userManager.UpdateAsync(user);

            var (token, ExpireIn) = _tokenProveder.GenerateToken(user);
            return new AuthResponse(user.Id, user.Email, user.FirstName, user.LastName, token, ExpireIn, refreshToken.Token);
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

            // create refresh token and add to user
            var refreshToken = new RefrechTokens
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                ExpirationOn = DateTime.UtcNow.AddDays(7),
                CreeatedOn = DateTime.UtcNow
            };
            user.RefrechTokens.Add(refreshToken);
            await _userManager.UpdateAsync(user);

            var (token, ExpireIn) = _tokenProveder.GenerateToken(user);
            return new AuthResponse(user.Id, user.Email, user.FirstName, user.LastName, token, ExpireIn, refreshToken.Token);
        }

        public async Task<AuthResponse?> RefreshTokenAsync(RefreshRequest request, CancellationToken cancellationToken)
        {
            // validate incoming access token format
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

            // find matching refresh token
            var existing = user.RefrechTokens.FirstOrDefault(rt => rt.Token == request.RefreshToken);
            if (existing == null || !existing.IsActive)
                return null;

            // revoke the old token
            existing.RevokeOn = DateTime.UtcNow;

            // create and add new refresh token
            var newRefresh = new RefrechTokens
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                ExpirationOn = DateTime.UtcNow.AddDays(7),
                CreeatedOn = DateTime.UtcNow
            };
            user.RefrechTokens.Add(newRefresh);

            await _userManager.UpdateAsync(user);

            var (token, ExpiresIn) = _tokenProveder.GenerateToken(user);
            return new AuthResponse(user.Id, user.Email, user.FirstName, user.LastName, token, ExpiresIn, newRefresh.Token);
        }
    }
}
