using Microsoft.IdentityModel.Tokens;
using SurveyBasket.Services.Authntchan.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SurveyBasket.Services.Authntchan
{
	public class TokenProveder : ITokenProvedr
	{
		private readonly JwtOptions _jwtOptions;

		public TokenProveder(JwtOptions jwtOptions)
		{
			_jwtOptions = jwtOptions;
		}

		public (string token, int ExpiresIn) GenerateToken(AppUser user, IList<string> roles)
		{
			var claims = new List<Claim>
			{
				new(JwtRegisteredClaimNames.Sub, user.Id),
				new(JwtRegisteredClaimNames.Email, user.Email!),
				new(JwtRegisteredClaimNames.GivenName, user.FirstName),
				new(JwtRegisteredClaimNames.FamilyName, user.LastName),
				new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
			};

			// Add role claims
			foreach (var role in roles)
			{
				claims.Add(new Claim(ClaimTypes.Role, role));
			}

			var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.IssuerSigningKey));
			var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

			var token = new JwtSecurityToken(
				issuer: _jwtOptions.ValidIssuer,
				audience: _jwtOptions.ValidAudiences,
				claims: claims,
				expires: DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiresIn),
				signingCredentials: signingCredentials
			);

			return (token: new JwtSecurityTokenHandler().WriteToken(token), ExpiresIn: _jwtOptions.ExpiresIn);
		}
	}
}
