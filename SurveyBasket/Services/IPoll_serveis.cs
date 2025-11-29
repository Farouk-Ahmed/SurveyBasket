using SurveyBasket.Abstractions;

namespace SurveyBasket.Services
{
    public interface IPoll_serveis
    {
      Task<IEnumerable<Poll>> GetPollsAsync(CancellationToken cancellationToken);
        Task<Result<pollResponse>> GetPollByIdAsync(int id,CancellationToken cancellationToken);
        Task <Poll> AddAsync(Poll poll,CancellationToken cancellationToken);
        Task<Result>  UpDateAsync(PollReuestq poll, int id,CancellationToken cancellationToken=default);
        Task<bool> DeleteAsync(int id ,CancellationToken cancellationToken);
        Task<Result> TogglePublishAsync(int id, CancellationToken cancellationToken);

    }
}
