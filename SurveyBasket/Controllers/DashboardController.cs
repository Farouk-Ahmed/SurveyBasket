using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SurveyBasket.Abstractions;
using SurveyBasket.Contract.Auth.Request;
using SurveyBasket.Contract.Dashboard.Request;
using SurveyBasket.Services;

namespace SurveyBasket.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = DefaultRoles.Admin)]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly IAuthService _authService;

        public DashboardController(IDashboardService dashboardService, IAuthService authService)
        {
            _dashboardService = dashboardService;
            _authService = authService;
        }

        /// <summary>
        /// Create a new user with a specific role (Admin only)
        /// </summary>
        [HttpPost("create-user")]
        public async Task<IActionResult> CreateUser(CreateUserRequest request, CancellationToken cancellationToken)
        {
            var result = await _authService.CreateUserAsync(request, cancellationToken);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }

        /// <summary>
        /// Get dashboard summary with poll statistics and filtered poll list
        /// </summary>
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary([FromQuery] DashboardFilterRequest filter, CancellationToken cancellationToken)
        {
            var result = await _dashboardService.GetDashboardSummaryAsync(filter, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Get detailed information about a specific poll
        /// </summary>
        [HttpGet("polls/{id}")]
        public async Task<IActionResult> GetPollDetails(int id, CancellationToken cancellationToken)
        {
            var result = await _dashboardService.GetPollDetailsAsync(id, cancellationToken);
            return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
        }

        /// <summary>
        /// Get audit trail for a specific poll
        /// </summary>
        [HttpGet("polls/{id}/audit")]
        public async Task<IActionResult> GetPollAuditLog(int id, CancellationToken cancellationToken)
        {
            var result = await _dashboardService.GetPollAuditLogAsync(id, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Get all audit logs across all polls
        /// </summary>
        [HttpGet("audit-logs")]
        public async Task<IActionResult> GetAllAuditLogs(CancellationToken cancellationToken)
        {
            var result = await _dashboardService.GetAllAuditLogsAsync(cancellationToken);
            return Ok(result);
        }
        /// <summary>
        /// Get all user clients (with support for search by Name, NationalId, Email and pagination)
        /// </summary>
        [HttpGet("clients")]
        public async Task<IActionResult> GetClients([FromQuery] Contract.Dashboard.Request.ClientFilterRequest filter, CancellationToken cancellationToken)
        {
            var result = await _dashboardService.GetClientsAsync(filter, cancellationToken);
            return Ok(result.Value);
        }

        /// <summary>
        /// Override a client's avatar (Admin only)
        /// </summary>
        [HttpPut("clients/{clientId}/avatar")]
        public async Task<IActionResult> UpdateClientAvatar(int clientId, [FromForm] Contract.Clients.Request.UpdateClientAvatarRequest request, CancellationToken cancellationToken)
        {
            var result = await _dashboardService.UpdateClientAvatarAsync(clientId, request, cancellationToken);
            return result.IsSuccess ? Ok(new { Url = result.Value }) : BadRequest(result.Error);
        }
    }
}
