using System.ComponentModel.DataAnnotations;

namespace SurveyBasket.Services.Authntchan.Options
{
    public class JwtOptions
    {
        [Required]
        public const string SectionName = "Jwt";
        [Required]
        public string IssuerSigningKey { get; init; } = string.Empty;
        [Required]
        public string ValidIssuer { get; init; } = string.Empty;
        [Required]
        public string ValidAudiences { get; init; } = string.Empty;
        [Range(1,int.MaxValue)]

        public int ExpiresIn { get; init; }
    }
}
