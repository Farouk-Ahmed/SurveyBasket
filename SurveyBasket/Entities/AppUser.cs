using Microsoft.AspNetCore.Identity;

namespace SurveyBasket.Entities
{
	public sealed class AppUser : IdentityUser
	{
		public string FirstName { get; set; } = string.Empty;
		public string LastName { get; set; } = string.Empty;
		public List<RefrechTokens> RefrechTokens { get; set; } = new();

    }
}
