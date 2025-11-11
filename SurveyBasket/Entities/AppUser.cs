using Microsoft.AspNetCore.Identity;

namespace SurveyBasket.Entities
{
	public sealed class  AppUser:IdentityUser
	{
		public string FirstName { get; set; }
		public string LastName { get; set; } // Changed from int to string
		public List<RefrechTokens> RefrechTokens { get; set; } = [];

    }
}
