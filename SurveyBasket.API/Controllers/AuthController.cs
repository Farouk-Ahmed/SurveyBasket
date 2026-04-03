using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SurveyBasket.Application.DTOs.Auth;
using SurveyBasket.Application.Interfaces;

namespace SurveyBasket.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginAsync([FromBody] AuthRequest? request, CancellationToken cancellationToken)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { description = "Email and password are required." });
        try
        {
            var authResult = await authService.AuthResponseAsync(request.Email, request.Password ?? "", cancellationToken);
            if (authResult.IsSuccess)
                return Ok(authResult.Value);
            if (authResult.Error?.Code == "User.InvalidCredentials")
                return Unauthorized(authResult.Error);
            return BadRequest(authResult.Error);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { description = "Login failed.", detail = ex.Message });
        }
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromForm] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.RegisterAsync(request, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        return Ok(result.Value);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest? request, CancellationToken cancellationToken)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.RefreshToken))
            return BadRequest("Refresh token is required.");
        var result = await authService.RefreshTokenAsync(request, cancellationToken);
        return result is null ? Unauthorized("Invalid token or refresh token.") : Ok(result);
    }
}
