using SurveyBasket.Abstractions;
using SurveyBasket.Contract.Dashboard.Request;
using SurveyBasket.Contract.Dashboard.Response;

namespace SurveyBasket.Services
{
    public interface IDashboardService
    {
        Task<DashboardSummaryResponse> GetDashboardSummaryAsync(DashboardFilterRequest filter, CancellationToken cancellationToken);
        Task<Result<DashboardPollResponse>> GetPollDetailsAsync(int pollId, CancellationToken cancellationToken);
        Task<IEnumerable<AuditLogResponse>> GetPollAuditLogAsync(int pollId, CancellationToken cancellationToken);
        Task<IEnumerable<AuditLogResponse>> GetAllAuditLogsAsync(CancellationToken cancellationToken);
        
        // Client Management
        Task<Result<IEnumerable<Contract.Clients.Response.DashboardClientResponse>>> GetClientsAsync(ClientFilterRequest filter, CancellationToken cancellationToken);
        Task<Result<string>> UpdateClientAvatarAsync(int clientId, Contract.Clients.Request.UpdateClientAvatarRequest request, CancellationToken cancellationToken);
    }
}
