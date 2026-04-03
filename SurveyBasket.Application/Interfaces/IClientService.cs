using SurveyBasket.Application.DTOs.Clients;
using SurveyBasket.Domain.Abstractions;

namespace SurveyBasket.Application.Interfaces;

public interface IClientService
{
    Task<Result<ClientProfileResponse>> GetMyProfileAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result> UpdateMyProfileAsync(string userId, UpdateClientProfileRequest request, CancellationToken cancellationToken = default);
    Task<Result<string>> UpdateMyAvatarAsync(string userId, UpdateClientAvatarRequest request, CancellationToken cancellationToken = default);
}
