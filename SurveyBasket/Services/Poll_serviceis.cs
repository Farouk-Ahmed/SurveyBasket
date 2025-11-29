
using SurveyBasket.Abstractions;
using SurveyBasket.Entities;
using SurveyBasket.Errors;
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


        public async Task<Result<pollResponse>> GetPollByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var poll= await _dbContext.Set<Poll>().FindAsync(id, cancellationToken);
            return poll is null
               ? Result.Failure<pollResponse>(PollErrors.PollNotFound)
                : Result.Success(poll.Adapt<pollResponse>());
        }

        public async Task<Poll> AddAsync(Poll poll, CancellationToken cancellationToken = default)
        {
            await _dbContext.AddAsync(poll, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return poll;
        }

        public async Task<Result> UpDateAsync(PollReuestq poll, int id, CancellationToken cancellationToken = default)
        {
            var createdPoll = await _dbContext.Set<Poll>().FindAsync(id, cancellationToken);
            if (createdPoll is null)
                return Result.Failure(PollErrors.PollNotFound);
            createdPoll.Title = poll.Title;
            createdPoll.Summray = poll.Summray;
            createdPoll.IsPublished = poll.IsPublished;
            createdPoll.StartsAt = poll.StartsAt;
            createdPoll.EndsAt = poll.EndsAt;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
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

        public async Task<Result> TogglePublishAsync(int id, CancellationToken cancellationToken)
        {
            var pollEntity = await _dbContext.Set<Poll>().FindAsync(id, cancellationToken);
            if (pollEntity is null)
                return Result.Failure(PollErrors.PollNotFound);

            pollEntity.IsPublished = !pollEntity.IsPublished;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}

