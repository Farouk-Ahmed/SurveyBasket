
using SurveyBasket.Abstractions;
using SurveyBasket.Contract.Files.Response;
using SurveyBasket.Entities;
using SurveyBasket.Errors;
using SurveyBasket.NewFolder;
using System.Security.Claims;

namespace SurveyBasket.Services
{
    public class Poll_serviceis : IPoll_serveis
    {
        private readonly AppDBContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public Poll_serviceis(AppDBContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<Poll>> GetPollsAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<Poll>()
                .Include(p => p.Attachments)
                    .ThenInclude(a => a.UploadedBy)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Result<pollResponse>> GetPollByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var poll = await _dbContext.Set<Poll>()
                .Include(p => p.Attachments)
                    .ThenInclude(a => a.UploadedBy)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

            if (poll is null)
                return Result.Failure<pollResponse>(PollErrors.PollNotFound);

            var images = poll.Attachments.Select(a => new FileResponse(
                a.Id, a.FileName, BuildFileUrl(a.StoredPath), a.ContentType,
                a.FileSize, $"{a.UploadedBy?.FirstName} {a.UploadedBy?.LastName}".Trim(),
                a.UploadedById, a.UploadedOn, a.PollId
            ));

            var response = new pollResponse(poll.Id, poll.Title, poll.Summray, poll.IsPublished, poll.StartsAt, poll.EndsAt, images);
            return Result.Success(response);
        }

        public async Task<Poll> AddAsync(Poll poll, CancellationToken cancellationToken = default)
        {
            await _dbContext.AddAsync(poll, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Log audit entry
            await LogAuditAsync(poll.Id, "Created", $"Poll '{poll.Title}' was created.", cancellationToken);

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

            // Log audit entry
            await LogAuditAsync(id, "Updated", $"Poll '{poll.Title}' was updated.", cancellationToken);

            return Result.Success();
        }

        public async Task<bool> DeleteAsync(int id, string? reason = null, CancellationToken cancellationToken = default)
        {
            var poll = await _dbContext.Set<Poll>().FindAsync(id, cancellationToken);
            if (poll is null)
                return false;

            // Soft-delete instead of hard-delete
            poll.IsDeleted = true;
            poll.DeletionReason = reason;
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Log audit entry
            var details = string.IsNullOrWhiteSpace(reason)
                ? $"Poll '{poll.Title}' was deleted."
                : $"Poll '{poll.Title}' was deleted. Reason: {reason}";
            await LogAuditAsync(id, "Deleted", details, cancellationToken);

            return true;
        }

        public async Task<Result> TogglePublishAsync(int id, CancellationToken cancellationToken)
        {
            var pollEntity = await _dbContext.Set<Poll>().FindAsync(id, cancellationToken);
            if (pollEntity is null)
                return Result.Failure(PollErrors.PollNotFound);

            pollEntity.IsPublished = !pollEntity.IsPublished;
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Log audit entry
            var action = pollEntity.IsPublished ? "Published" : "Unpublished";
            await LogAuditAsync(id, action, $"Poll '{pollEntity.Title}' was {action.ToLower()}.", cancellationToken);

            return Result.Success();
        }

        private async Task LogAuditAsync(int pollId, string action, string? details, CancellationToken cancellationToken)
        {
            var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                return;

            var auditLog = new AuditLog
            {
                PollId = pollId,
                Action = action,
                PerformedById = currentUserId,
                PerformedOn = DateTime.UtcNow,
                Details = details
            };

            await _dbContext.AuditLogs.AddAsync(auditLog, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        private string BuildFileUrl(string storedPath)
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            return $"{request?.Scheme}://{request?.Host}/{storedPath}";
        }
    }
}
