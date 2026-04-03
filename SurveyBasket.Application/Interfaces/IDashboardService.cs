using SurveyBasket.Application.DTOs.Dashboard;
using SurveyBasket.Application.DTOs.Clients;
using SurveyBasket.Domain.Abstractions;

namespace SurveyBasket.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryResponse> GetDashboardSummaryAsync(DashboardFilterRequest filter, CancellationToken cancellationToken);
    Task<Result<DashboardPollResponse>> GetPollDetailsAsync(int pollId, CancellationToken cancellationToken);
    Task<IEnumerable<AuditLogResponse>> GetPollAuditLogAsync(int pollId, CancellationToken cancellationToken);
    Task<IEnumerable<AuditLogResponse>> GetAllAuditLogsAsync(DashboardFilterRequest filter, CancellationToken cancellationToken);
    Task<Result<IEnumerable<DashboardClientResponse>>> GetClientsAsync(ClientFilterRequest filter, CancellationToken cancellationToken);
    Task<Result<string>> UpdateClientAvatarAsync(int clientId, UpdateClientAvatarRequest request, CancellationToken cancellationToken);
}
