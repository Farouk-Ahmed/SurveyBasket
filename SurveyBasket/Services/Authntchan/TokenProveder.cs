using MassTransit.Serialization;
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

		public (string token, int ExpiresIn) GenerateToken(AppUser user)
		{
			Claim[] claims =
				[
				new(JwtRegisteredClaimNames.Sub, user.Id),
				new(JwtRegisteredClaimNames.Email, user.Email),
				new(JwtRegisteredClaimNames.GivenName, user.FirstName),
				new(JwtRegisteredClaimNames.FamilyName, user.LastName.ToString()),
				new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
				];

			var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.IssuerSigningKey));
			var singinCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

			var token = new JwtSecurityToken(
				issuer: _jwtOptions.ValidIssuer,
				audience: _jwtOptions.ValidAudiences,
				claims: claims,
				expires: DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiresIn),
				signingCredentials: singinCredentials
				);
			return (token: new JwtSecurityTokenHandler().WriteToken(token), ExpiresIn: _jwtOptions.ExpiresIn);
		}
	}
}
