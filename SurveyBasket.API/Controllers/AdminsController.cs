using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SurveyBasket.Application.DTOs.Admins;
using SurveyBasket.Application.Interfaces;
using SurveyBasket.Domain.Abstractions;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace SurveyBasket.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = DefaultRoles.Admin)]
public class AdminsController(IAdminService adminService) : ControllerBase
{
    private string? UserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
    {
        var userId = UserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        var result = await adminService.GetMyProfileAsync(userId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateAdminProfileRequest request, CancellationToken cancellationToken)
    {
        var userId = UserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        var result = await adminService.UpdateMyProfileAsync(userId, request, cancellationToken);
        return result.IsSuccess ? NoContent() : NotFound(result.Error);
    }

    [HttpPut("me/avatar")]
    public async Task<IActionResult> UpdateMyAvatar([FromForm] UpdateAdminAvatarRequest request, CancellationToken cancellationToken)
    {
        var userId = UserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        var result = await adminService.UpdateMyAvatarAsync(userId, request, cancellationToken);
        return result.IsSuccess ? Ok(new { Url = result.Value }) : BadRequest(result.Error);
    }
}
