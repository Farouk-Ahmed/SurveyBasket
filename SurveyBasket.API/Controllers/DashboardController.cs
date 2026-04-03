using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SurveyBasket.Application.DTOs.Auth;
using SurveyBasket.Application.DTOs.Dashboard;
using SurveyBasket.Application.DTOs.Clients;
using SurveyBasket.Application.Interfaces;
using SurveyBasket.Domain.Abstractions;

namespace SurveyBasket.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = DefaultRoles.Admin)]
public class DashboardController(IDashboardService dashboardService, IAuthService authService) : ControllerBase
{
    [HttpPost("create-user")]
    public async Task<IActionResult> CreateUser(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.CreateUserAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromQuery] DashboardFilterRequest filter, CancellationToken cancellationToken)
    {
        var result = await dashboardService.GetDashboardSummaryAsync(filter, cancellationToken);
        return Ok(result);
    }

    [HttpGet("polls/{id}")]
    public async Task<IActionResult> GetPollDetails(int id, CancellationToken cancellationToken)
    {
        var result = await dashboardService.GetPollDetailsAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpGet("polls/{id}/audit")]
    public async Task<IActionResult> GetPollAuditLog(int id, CancellationToken cancellationToken)
    {
        var result = await dashboardService.GetPollAuditLogAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("audit-logs")]
    public async Task<IActionResult> GetAllAuditLogs([FromQuery] DashboardFilterRequest filter, CancellationToken cancellationToken)
    {
        var result = await dashboardService.GetAllAuditLogsAsync(filter, cancellationToken);
        return Ok(result);
    }

    [HttpGet("clients")]
    public async Task<IActionResult> GetClients([FromQuery] ClientFilterRequest filter, CancellationToken cancellationToken)
    {
        var result = await dashboardService.GetClientsAsync(filter, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPut("clients/{clientId}/avatar")]
    public async Task<IActionResult> UpdateClientAvatar(int clientId, [FromForm] UpdateClientAvatarRequest request, CancellationToken cancellationToken)
    {
        var result = await dashboardService.UpdateClientAvatarAsync(clientId, request, cancellationToken);
        return result.IsSuccess ? Ok(new { Url = result.Value }) : BadRequest(result.Error);
    }
}
