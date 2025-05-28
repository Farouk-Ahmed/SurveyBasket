using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SurveyBasket.Contract.Auth.Request;

namespace SurveyBasket.Controllers
{
	[Route("[controller]")]
	[ApiController]
	public class AuthController(IAuthService authService) : ControllerBase
	{
		private readonly IAuthService _authService = authService;

		[HttpPost]
        [Route("login")]/*{"email": "faroukola19@gmail.com","password": "Ff#1254"}*/
        [AllowAnonymous]
		public async Task<IActionResult> Login(AuthRequest request, CancellationToken cancellationToken)
		{
			var user = await _authService.AuthResponseAsync(request.Email, request.Password, cancellationToken);
			return user is null ? BadRequest("Invalid Email or Password. Try again.") : Ok(user);
		}

	}
}
