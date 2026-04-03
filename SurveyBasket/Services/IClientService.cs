using SurveyBasket.Abstractions;
using SurveyBasket.Contract.Clients.Request;
using SurveyBasket.Contract.Clients.Response;

namespace SurveyBasket.Services
{
    public interface IClientService
    {
        Task<Result<ClientProfileResponse>> GetMyProfileAsync(string userId, CancellationToken cancellationToken = default);
        Task<Result> UpdateMyProfileAsync(string userId, UpdateClientProfileRequest request, CancellationToken cancellationToken = default);
        Task<Result<string>> UpdateMyAvatarAsync(string userId, UpdateClientAvatarRequest request, CancellationToken cancellationToken = default);
    }
}
