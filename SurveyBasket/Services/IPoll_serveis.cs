namespace SurveyBasket.Services
{
    public interface IPoll_serveis
    {
      Task<IEnumerable<Poll>>  GetPollsAsync(CancellationToken cancellationToken);
        Task<Poll?>  GetPollByIdAsync(int id,CancellationToken cancellationToken);
        Task <Poll> AddAsync(Poll poll,CancellationToken cancellationToken);
        Task<bool>  UpDateAsync(Poll poll, int id,CancellationToken cancellationToken);
        Task<bool> DeleteAsync(int id ,CancellationToken cancellationToken);
		Task<bool> TogglePublishAsync(int id, CancellationToken cancellationToken);

	}
}
