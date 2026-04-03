using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SurveyBasket.Application.DTOs.Clients;
using SurveyBasket.Application.Interfaces;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace SurveyBasket.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ClientsController(IClientService clientService) : ControllerBase
{
    /// <summary>User id from JWT (sub or NameIdentifier).</summary>
    private string? UserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
    {
        var userId = UserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        var result = await clientService.GetMyProfileAsync(userId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateClientProfileRequest request, CancellationToken cancellationToken)
    {
        var userId = UserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        var result = await clientService.UpdateMyProfileAsync(userId, request, cancellationToken);
        return result.IsSuccess ? NoContent() : NotFound(result.Error);
    }

    [HttpPut("me/avatar")]
    public async Task<IActionResult> UpdateMyAvatar([FromForm] UpdateClientAvatarRequest request, CancellationToken cancellationToken)
    {
        var userId = UserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        var result = await clientService.UpdateMyAvatarAsync(userId, request, cancellationToken);
        return result.IsSuccess ? Ok(new { Url = result.Value }) : BadRequest(result.Error);
    }
}
