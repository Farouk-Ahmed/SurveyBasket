using SurveyBasket.Application.DTOs.Poll;
using SurveyBasket.Domain.Abstractions;
using SurveyBasket.Domain.Entities;

namespace SurveyBasket.Application.Interfaces;

public interface IPollService
{
    Task<IEnumerable<Poll>> GetPollsAsync(CancellationToken cancellationToken);
    Task<Result<PollResponse>> GetPollByIdAsync(int id, CancellationToken cancellationToken);
    Task<Result<Poll>> AddAsync(Poll poll, CancellationToken cancellationToken);
    Task<Result> UpdateAsync(CreatePollRequest poll, int id, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, string? reason = null, CancellationToken cancellationToken = default);
    Task<Result> TogglePublishAsync(int id, CancellationToken cancellationToken);
}
