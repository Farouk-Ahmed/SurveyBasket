using SurveyBasket.Application.DTOs.Admins;
using SurveyBasket.Domain.Abstractions;

namespace SurveyBasket.Application.Interfaces;

public interface IAdminService
{
    Task<Result<AdminProfileResponse>> GetMyProfileAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result> UpdateMyProfileAsync(string userId, UpdateAdminProfileRequest request, CancellationToken cancellationToken = default);
    Task<Result<string>> UpdateMyAvatarAsync(string userId, UpdateAdminAvatarRequest request, CancellationToken cancellationToken = default);
}
