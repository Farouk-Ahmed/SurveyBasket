using Microsoft.AspNetCore.Identity;
using SurveyBasket.Contract.Auth.Response;
using SurveyBasket.Contract.Auth.Request;
using SurveyBasket.Entities;
using MassTransit;
using System.Security.Cryptography;
using System.Text;
using SurveyBasket.Services.Authntchan;

namespace SurveyBasket.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
		private readonly ITokenProvedr _tokenProveder;

		public AuthService(UserManager<AppUser> userManager ,ITokenProvedr _tokenProveder)
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
            var (token, ExpireIn) =_tokenProveder.GenerateToken(user);
            return new AuthResponse(user.Id, user.Email, user.FirstName, user.LastName, token, ExpireIn);
        }

    }
}
