using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SurveyBasket.Contract.Auth.Request;
using Microsoft.Extensions.Options;
using SurveyBasket.Services.Authntchan.Options;
using SurveyBasket.Abstractions;

namespace SurveyBasket.Controllers
{
	[Route("[controller]")]
	[ApiController]
	public class AuthController(IAuthService authService, IOptions<JwtOptions> options) : ControllerBase
	{
		private readonly IAuthService _authService = authService;
        private readonly JwtOptions _options = options.Value;

        /// <summary>
        /// Login with email and password
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
		public async Task<IActionResult> LoginAsync(AuthRequest request, CancellationToken cancellationToken)
		{
			var authResult = await _authService.AuthResponseAsync(request.Email, request.Password, cancellationToken);
			return authResult.IsSuccess ? Ok(authResult.Value) : BadRequest(authResult.Error);
        }

        /// <summary>
        /// Register a new user (email + password + confirm password)
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
        {
            var result = await _authService.RegisterAsync(request, cancellationToken);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }

        /// <summary>
        /// Refresh JWT token using refresh token
        /// </summary>
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh(RefreshRequest request, CancellationToken cancellationToken)
        {
            var result = await _authService.RefreshTokenAsync(request, cancellationToken);
            return result is null ? BadRequest("Invalid token or refresh token.") : Ok(result);
        }
	}
}
