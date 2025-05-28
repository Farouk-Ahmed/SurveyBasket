namespace SurveyBasket.Services.Authntchan
{
	public interface ITokenProvedr
	{
		(string token,int ExpiresIn)GenerateToken(AppUser user);
	}
}
 