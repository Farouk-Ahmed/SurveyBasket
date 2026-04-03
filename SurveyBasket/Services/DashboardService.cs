using SurveyBasket.Abstractions;
using SurveyBasket.Contract.Dashboard.Request;
using SurveyBasket.Contract.Dashboard.Response;
using SurveyBasket.Contract.Files.Response;
using SurveyBasket.Entities;
using SurveyBasket.Errors;
using SurveyBasket.NewFolder;

namespace SurveyBasket.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDBContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _environment;

        public DashboardService(AppDBContext dbContext, IHttpContextAccessor httpContextAccessor, IWebHostEnvironment environment)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _environment = environment;
        }

        public async Task<DashboardSummaryResponse> GetDashboardSummaryAsync(DashboardFilterRequest filter, CancellationToken cancellationToken)
        {
            IQueryable<Poll> query = _dbContext.Polls
                .IgnoreQueryFilters()
                .Include(p => p.CreatedBy)
                .Include(p => p.UpdatedBy)
                .Include(p => p.DeletedBy)
                .Include(p => p.Attachments)
                    .ThenInclude(a => a.UploadedBy)
                .AsNoTracking();

            // Apply filters
            if (filter.OnlyDeleted)
            {
                query = query.Where(p => p.IsDeleted);
            }
            else if (!filter.IncludeDeleted)
            {
                query = query.Where(p => !p.IsDeleted);
            }

            if (filter.IsPublished.HasValue)
            {
                query = query.Where(p => p.IsPublished == filter.IsPublished.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim().ToLower();
                query = query.Where(p => p.Title.ToLower().Contains(term) || p.Summray.ToLower().Contains(term));
            }

            // Apply sorting
            query = (filter.SortBy?.ToLower(), filter.SortDirection?.ToLower()) switch
            {
                ("title", "desc") => query.OrderByDescending(p => p.Title),
                ("title", _) => query.OrderBy(p => p.Title),
                ("createdon", "desc") => query.OrderByDescending(p => p.CreatedOn),
                ("createdon", _) => query.OrderBy(p => p.CreatedOn),
                ("startsat", "desc") => query.OrderByDescending(p => p.StartsAt),
                ("startsat", _) => query.OrderBy(p => p.StartsAt),
                _ => query.OrderByDescending(p => p.CreatedOn)
            };

            var polls = await query.ToListAsync(cancellationToken);

            // Build counts
            var allPolls = await _dbContext.Polls
                .IgnoreQueryFilters()
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var totalPolls = allPolls.Count(p => !p.IsDeleted);
            var publishedPolls = allPolls.Count(p => !p.IsDeleted && p.IsPublished);
            var unpublishedPolls = allPolls.Count(p => !p.IsDeleted && !p.IsPublished);
            var deletedPolls = allPolls.Count(p => p.IsDeleted);

            var pollResponses = polls.Select(p => new DashboardPollResponse(
                p.Id, p.Title, p.Summray, p.IsPublished, p.StartsAt, p.EndsAt,
                FormatUserName(p.CreatedBy),
                p.UpdatedBy != null ? FormatUserName(p.UpdatedBy) : null,
                p.CreatedOn, p.UpdatedOn,
                p.IsDeleted,
                p.DeletedBy != null ? FormatUserName(p.DeletedBy) : null,
                p.DeletedOn, p.DeletionReason,
                MapAttachments(p.Attachments)
            ));

            return new DashboardSummaryResponse(totalPolls, publishedPolls, unpublishedPolls, deletedPolls, pollResponses);
        }

        public async Task<Result<DashboardPollResponse>> GetPollDetailsAsync(int pollId, CancellationToken cancellationToken)
        {
            var poll = await _dbContext.Polls
                .IgnoreQueryFilters()
                .Include(p => p.CreatedBy)
                .Include(p => p.UpdatedBy)
                .Include(p => p.DeletedBy)
                .Include(p => p.Attachments)
                    .ThenInclude(a => a.UploadedBy)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == pollId, cancellationToken);

            if (poll is null)
                return Result.Failure<DashboardPollResponse>(PollErrors.PollNotFound);

            var response = new DashboardPollResponse(
                poll.Id, poll.Title, poll.Summray, poll.IsPublished, poll.StartsAt, poll.EndsAt,
                FormatUserName(poll.CreatedBy),
                poll.UpdatedBy != null ? FormatUserName(poll.UpdatedBy) : null,
                poll.CreatedOn, poll.UpdatedOn,
                poll.IsDeleted,
                poll.DeletedBy != null ? FormatUserName(poll.DeletedBy) : null,
                poll.DeletedOn, poll.DeletionReason,
                MapAttachments(poll.Attachments)
            );

            return Result.Success(response);
        }

        public async Task<IEnumerable<AuditLogResponse>> GetPollAuditLogAsync(int pollId, CancellationToken cancellationToken)
        {
            var logs = await _dbContext.AuditLogs
                .Include(a => a.Poll)
                .Include(a => a.PerformedBy)
                .Where(a => a.PollId == pollId)
                .OrderByDescending(a => a.PerformedOn)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return logs.Select(a => new AuditLogResponse(
                a.Id, a.PollId, a.Poll.Title, a.Action,
                FormatUserName(a.PerformedBy), a.PerformedOn, a.Details
            ));
        }

        public async Task<IEnumerable<AuditLogResponse>> GetAllAuditLogsAsync(CancellationToken cancellationToken)
        {
            var logs = await _dbContext.AuditLogs
                .Include(a => a.Poll)
                .Include(a => a.PerformedBy)
                .OrderByDescending(a => a.PerformedOn)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return logs.Select(a => new AuditLogResponse(
                a.Id, a.PollId, a.Poll.Title, a.Action,
                FormatUserName(a.PerformedBy), a.PerformedOn, a.Details
            ));
        }

        private static string FormatUserName(AppUser user)
        {
            return $"{user.FirstName} {user.LastName}".Trim();
        }

        private IEnumerable<FileResponse> MapAttachments(ICollection<Attachment> attachments)
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            return attachments.Select(a => new FileResponse(
                a.Id, a.FileName,
                $"{request?.Scheme}://{request?.Host}/{a.StoredPath}",
                a.ContentType, a.FileSize,
                $"{a.UploadedBy?.FirstName} {a.UploadedBy?.LastName}".Trim(),
                a.UploadedById, a.UploadedOn, a.PollId
            ));
        }

        public async Task<Result<IEnumerable<Contract.Clients.Response.DashboardClientResponse>>> GetClientsAsync(ClientFilterRequest filter, CancellationToken cancellationToken)
        {
            IQueryable<Client> query = _dbContext.Clients
                .Include(c => c.AppUser)
                .AsNoTracking();

            // Apply search
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim().ToLower();
                query = query.Where(c => 
                    c.FirstName.ToLower().Contains(term) || 
                    c.LastName.ToLower().Contains(term) ||
                    (c.FirstName + " " + c.LastName).ToLower().Contains(term) ||
                    c.NationalId.Contains(term));
            }
            
            if (!string.IsNullOrWhiteSpace(filter.Email))
            {
                var emailTerm = filter.Email.Trim().ToLower();
                query = query.Where(c => c.Email.ToLower().Contains(emailTerm));
            }

            // Apply Sort
            query = (filter.SortBy?.ToLower(), filter.SortDirection?.ToLower()) switch
            {
                ("name", "desc") => query.OrderByDescending(c => c.FirstName).ThenByDescending(c => c.LastName),
                ("name", _) => query.OrderBy(c => c.FirstName).ThenBy(c => c.LastName),
                ("registeredon", "desc") => query.OrderByDescending(c => c.CreatedOn),
                ("registeredon", _) => query.OrderBy(c => c.CreatedOn),
                ("email", "desc") => query.OrderByDescending(c => c.Email),
                ("email", _) => query.OrderBy(c => c.Email),
                _ => query.OrderByDescending(c => c.CreatedOn)
            };

            // Apply Pagination
            var skip = (filter.PageNumber - 1) * filter.PageSize;
            var clients = await query.Skip(skip).Take(filter.PageSize).ToListAsync(cancellationToken);

            var request = _httpContextAccessor.HttpContext?.Request;
            var baseUrl = $"{request?.Scheme}://{request?.Host}/";

            var response = clients.Select(c => new Contract.Clients.Response.DashboardClientResponse(
                c.Id, c.FirstName, c.LastName, c.NationalId, c.Email, c.MainMobile, c.AlternateMobile,
                c.MainAddress, c.AlternateAddress, c.Gender, c.DateOfBirth,
                !string.IsNullOrEmpty(c.ProfilePicturePath) ? baseUrl + c.ProfilePicturePath : null,
                c.AppUserId, c.CreatedOn
            ));

            return Result.Success(response);
        }

        public async Task<Result<string>> UpdateClientAvatarAsync(int clientId, Contract.Clients.Request.UpdateClientAvatarRequest request, CancellationToken cancellationToken)
        {
            var client = await _dbContext.Clients.FirstOrDefaultAsync(c => c.Id == clientId, cancellationToken);
            if (client is null)
                return Result.Failure<string>(UserErrors.UserNotFound);

            if (request.ProfilePicture is null || request.ProfilePicture.Length == 0)
                return Result.Failure<string>(new Error("Avatar.Invalid", "Please provide a valid image file."));

            var extension = Path.GetExtension(request.ProfilePicture.FileName).ToLower();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

            if (!allowedExtensions.Contains(extension) || request.ProfilePicture.Length > 5 * 1024 * 1024)
                return Result.Failure<string>(new Error("Avatar.InvalidMetadata", "Invalid image format or size exceeds 5MB."));

            var relativePath = Path.Combine("User profile pictures", client.Id.ToString());
            var absolutePath = Path.Combine(_environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), relativePath);

            Directory.CreateDirectory(absolutePath);

            var storedFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(absolutePath, storedFileName);
            var relativeFilePath = Path.Combine(relativePath, storedFileName).Replace("\\", "/");

            if (!string.IsNullOrEmpty(client.ProfilePicturePath))
            {
                var oldFilePath = Path.Combine(_environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), client.ProfilePicturePath).Replace("/", "\\");
                if (File.Exists(oldFilePath)) File.Delete(oldFilePath);
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.ProfilePicture.CopyToAsync(stream, cancellationToken);
            }

            client.ProfilePicturePath = relativeFilePath;
            await _dbContext.SaveChangesAsync(cancellationToken);

            var httpRequest = _httpContextAccessor.HttpContext?.Request;
            var fullUrl = $"{httpRequest?.Scheme}://{httpRequest?.Host}/{relativeFilePath}";

            return Result.Success(fullUrl);
        }
    }
}
