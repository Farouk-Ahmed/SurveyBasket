using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SurveyBasket.Abstractions;
using SurveyBasket.Contract.Clients.Request;
using SurveyBasket.Services;
using System.Security.Claims;

namespace SurveyBasket.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // All client endpoints require authentication
    public class ClientsController : ControllerBase
    {
        private readonly IClientService _clientService;

        public ClientsController(IClientService clientService)
        {
            _clientService = clientService;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null) return Unauthorized();

            var result = await _clientService.GetMyProfileAsync(userId, cancellationToken);
            return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateClientProfileRequest request, CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null) return Unauthorized();

            var result = await _clientService.UpdateMyProfileAsync(userId, request, cancellationToken);
            return result.IsSuccess ? NoContent() : NotFound(result.Error);
        }

        [HttpPut("me/avatar")]
        public async Task<IActionResult> UpdateMyAvatar([FromForm] UpdateClientAvatarRequest request, CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null) return Unauthorized();

            var result = await _clientService.UpdateMyAvatarAsync(userId, request, cancellationToken);
            return result.IsSuccess ? Ok(new { Url = result.Value }) : BadRequest(result.Error);
        }
    }
}
