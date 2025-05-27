
using SurveyBasket.Entities;
using SurveyBasket.NewFolder;

namespace SurveyBasket.Services
{
    public class Poll_serviceis : IPoll_serveis
    {
        private readonly AppDBContext _dbContext;
        public Poll_serviceis(AppDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Poll>> GetPollsAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<Poll>().AsNoTracking().ToListAsync();
        }


        public async Task<Poll?> GetPollByIdAsync(int id, CancellationToken cancellationToken = default) => await _dbContext.Set<Poll>().FindAsync(id);

        public async Task<Poll> AddAsync(Poll poll, CancellationToken cancellationToken = default)
        {
            await _dbContext.AddAsync(poll, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return poll;
        }

        public async Task<bool> UpDateAsync(Poll poll, int id, CancellationToken cancellationToken = default)
        {
            var createdPoll = await GetPollByIdAsync(id, cancellationToken);
            if (createdPoll is null)
                return false;
            createdPoll.Title = poll.Title;
            createdPoll.Summray = poll.Summray;
            createdPoll.IsPublished = poll.IsPublished;
            createdPoll.StartsAt = poll.StartsAt;
            createdPoll.EndsAt = poll.EndsAt;
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var caruntpoll = await GetPollByIdAsync(id, cancellationToken);
            if (caruntpoll is null)
                return false;
            _dbContext.Remove(caruntpoll);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;


        }

		public async Task<bool> TogglePublishAsync(int id, CancellationToken cancellationToken)
		{
			var poll = await GetPollByIdAsync(id, cancellationToken);
			if (poll is null)
				return false;
			poll.IsPublished = !poll.IsPublished;
			await _dbContext.SaveChangesAsync(cancellationToken);
			return true;
		}
	}
}

