using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SurveyBasket.Abstractions;
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

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
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
    }
}
