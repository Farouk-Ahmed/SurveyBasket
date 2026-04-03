using Microsoft.IdentityModel.Tokens;
using SurveyBasket.Application.Interfaces;
using SurveyBasket.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SurveyBasket.Infrastructure.ExternalServices;

public class TokenProvider : ITokenProvider
{
    private readonly JwtOptions _jwtOptions;

    public TokenProvider(Microsoft.Extensions.Options.IOptions<JwtOptions> jwtOptions) => _jwtOptions = jwtOptions.Value;

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
        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.IssuerSigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _jwtOptions.ValidIssuer,
            audience: _jwtOptions.ValidAudiences,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiresIn),
            signingCredentials: creds);
        return (new JwtSecurityTokenHandler().WriteToken(token), _jwtOptions.ExpiresIn);
    }
}
