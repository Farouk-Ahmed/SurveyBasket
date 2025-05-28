
using MassTransit.Serialization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SurveyBasket.Services.Authntchan
{
	public class TokenProveder:ITokenProvedr
	{
		public (string token, int ExpiresIn) GenerateToken(AppUser user)
		{
			Claim[] claims =
				[
				new(JwtRegisteredClaimNames.Sub,user.Id),
				new(JwtRegisteredClaimNames.Email,user.Email),
				new(JwtRegisteredClaimNames.GivenName,user.FirstName),
				new(JwtRegisteredClaimNames.FamilyName,user.LastName.ToString()),
				new(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
					];
			var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("JshgdgdhdjdhhgFDGDSGfGhtGh1DFEF5cdf7"));
			var singinCredentials=new SigningCredentials(symmetricSecurityKey,SecurityAlgorithms.HmacSha256);
			var ExpiresIn = 30;

			var token = new JwtSecurityToken(
				issuer: "SurveyBasket",
				audience: "SurveyBasketUsers",
				claims: claims,
				expires: DateTime.UtcNow.AddMinutes(ExpiresIn),
				signingCredentials: singinCredentials
				);
			return(token:new JwtSecurityTokenHandler().WriteToken(token),ExpiresIn:ExpiresIn);

		}
	}
}
