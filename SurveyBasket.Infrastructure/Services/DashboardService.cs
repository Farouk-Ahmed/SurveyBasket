using Microsoft.EntityFrameworkCore;
using SurveyBasket.Application.DTOs.Dashboard;
using SurveyBasket.Application.DTOs.Clients;
using SurveyBasket.Application.DTOs.Files;
using SurveyBasket.Application.Interfaces;
using SurveyBasket.Domain.Abstractions;
using SurveyBasket.Domain.Entities;
using SurveyBasket.Domain.Errors;
using SurveyBasket.Infrastructure.Persistence;

namespace SurveyBasket.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IWebHostEnvironment _environment;

    public DashboardService(AppDbContext dbContext, IHttpContextAccessor httpContextAccessor, IWebHostEnvironment environment)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _environment = environment;
    }

    public async Task<DashboardSummaryResponse> GetDashboardSummaryAsync(DashboardFilterRequest filter, CancellationToken cancellationToken)
    {
        IQueryable<Poll> query = _dbContext.Polls.IgnoreQueryFilters()
            .Include(p => p.CreatedBy).Include(p => p.UpdatedBy).Include(p => p.DeletedBy)
            .Include(p => p.Attachments).ThenInclude(a => a.UploadedBy).AsNoTracking();
        if (filter.OnlyDeleted)
            query = query.Where(p => p.IsDeleted);
        else
        {
            if (!filter.IncludeDeleted) query = query.Where(p => !p.IsDeleted);
            if (filter.IsPublished.HasValue) query = query.Where(p => p.IsPublished == filter.IsPublished.Value);
        }
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim().ToLower();
            query = query.Where(p =>
                p.Title.ToLower().Contains(term) ||
                p.Summray.ToLower().Contains(term) ||
                (p.CreatedBy != null && (p.CreatedBy.FirstName + " " + p.CreatedBy.LastName).ToLower().Contains(term)));
        }
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
        var allPolls = await _dbContext.Polls.IgnoreQueryFilters().AsNoTracking().ToListAsync(cancellationToken);
        var totalPolls = allPolls.Count(p => !p.IsDeleted);
        var publishedPolls = allPolls.Count(p => !p.IsDeleted && p.IsPublished);
        var unpublishedPolls = allPolls.Count(p => !p.IsDeleted && !p.IsPublished);
        var deletedPolls = allPolls.Count(p => p.IsDeleted);
        var pollResponses = polls.Select(p => new DashboardPollResponse(p.Id, p.Title, p.Summray, p.IsPublished, p.StartsAt, p.EndsAt, FormatUserName(p.CreatedBy), p.UpdatedBy != null ? FormatUserName(p.UpdatedBy) : null, p.CreatedOn, p.UpdatedOn, p.IsDeleted, p.DeletedBy != null ? FormatUserName(p.DeletedBy) : null, p.DeletedOn, p.DeletionReason, MapAttachments(p.Attachments)));
        return new DashboardSummaryResponse(totalPolls, publishedPolls, unpublishedPolls, deletedPolls, pollResponses);
    }

    public async Task<Result<DashboardPollResponse>> GetPollDetailsAsync(int pollId, CancellationToken cancellationToken)
    {
        var poll = await _dbContext.Polls.IgnoreQueryFilters().Include(p => p.CreatedBy).Include(p => p.UpdatedBy).Include(p => p.DeletedBy).Include(p => p.Attachments).ThenInclude(a => a.UploadedBy).AsNoTracking().FirstOrDefaultAsync(p => p.Id == pollId, cancellationToken);
        if (poll is null) return Result.Failure<DashboardPollResponse>(PollErrors.PollNotFound);
        var response = new DashboardPollResponse(poll.Id, poll.Title, poll.Summray, poll.IsPublished, poll.StartsAt, poll.EndsAt, FormatUserName(poll.CreatedBy), poll.UpdatedBy != null ? FormatUserName(poll.UpdatedBy) : null, poll.CreatedOn, poll.UpdatedOn, poll.IsDeleted, poll.DeletedBy != null ? FormatUserName(poll.DeletedBy) : null, poll.DeletedOn, poll.DeletionReason, MapAttachments(poll.Attachments));
        return Result.Success(response);
    }

    public async Task<IEnumerable<AuditLogResponse>> GetPollAuditLogAsync(int pollId, CancellationToken cancellationToken)
    {
        var logs = await _dbContext.AuditLogs.Include(a => a.Poll).Include(a => a.PerformedBy).Where(a => a.PollId == pollId).OrderByDescending(a => a.PerformedOn).AsNoTracking().ToListAsync(cancellationToken);
        return logs.Select(a => new AuditLogResponse(a.Id, a.PollId, a.Poll.Title, a.Action, FormatUserName(a.PerformedBy), a.PerformedOn, a.Details, a.Action == "Deleted" ? a.Poll.DeletionReason : null));
    }

    /// <summary>
    /// Returns audit logs filtered by action type (matches selected filter):
    /// - Deleted filter → only Action "Deleted".
    /// - Published filter → only Action "Published".
    /// - Unpublished filter → only Action "Unpublished" (no requirement on poll current state).
    /// - Total → all actions.
    /// </summary>
    public async Task<IEnumerable<AuditLogResponse>> GetAllAuditLogsAsync(DashboardFilterRequest filter, CancellationToken cancellationToken)
    {
        const string ActionDeleted = "Deleted";
        const string ActionPublished = "Published";
        const string ActionUnpublished = "Unpublished";

        IQueryable<AuditLog> query = _dbContext.AuditLogs.IgnoreQueryFilters()
            .Include(a => a.Poll).ThenInclude(p => p.CreatedBy).Include(a => a.PerformedBy).AsNoTracking();
        if (filter.OnlyDeleted)
        {
            query = query.Where(a => a.Action == ActionDeleted);
        }
        else if (filter.IsPublished.HasValue)
        {
            query = query.Where(a => a.Action == (filter.IsPublished.Value ? ActionPublished : ActionUnpublished));
        }
        else
        {
            if (!filter.IncludeDeleted) query = query.Where(a => !a.Poll.IsDeleted);
        }
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim().ToLower();
            query = query.Where(a =>
                a.Poll.Title.ToLower().Contains(term) ||
                (a.Poll.Summray != null && a.Poll.Summray.ToLower().Contains(term)) ||
                (a.Poll.CreatedBy != null && (a.Poll.CreatedBy.FirstName + " " + a.Poll.CreatedBy.LastName).ToLower().Contains(term)));
        }
        var logs = await query.OrderByDescending(a => a.PerformedOn).ToListAsync(cancellationToken);
        return logs.Select(a => new AuditLogResponse(a.Id, a.PollId, a.Poll.Title, a.Action, FormatUserName(a.PerformedBy), a.PerformedOn, a.Details, a.Action == "Deleted" ? a.Poll.DeletionReason : null));
    }

    private static string FormatUserName(AppUser user) => $"{user.FirstName} {user.LastName}".Trim();

    private IEnumerable<FileResponse> MapAttachments(ICollection<Attachment> attachments)
    {
        var request = _httpContextAccessor.HttpContext?.Request;
        return attachments.Select(a => new FileResponse(a.Id, a.FileName, $"{request?.Scheme}://{request?.Host}/{a.StoredPath}", a.ContentType, a.FileSize, $"{a.UploadedBy?.FirstName} {a.UploadedBy?.LastName}".Trim(), a.UploadedById, a.UploadedOn, a.PollId));
    }

    public async Task<Result<IEnumerable<DashboardClientResponse>>> GetClientsAsync(ClientFilterRequest filter, CancellationToken cancellationToken)
    {
        IQueryable<Account> query = _dbContext.Accounts.Where(a => a.Role == DefaultRoles.User).Include(a => a.AppUser).AsNoTracking();
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim().ToLower();
            query = query.Where(a => (a.FirstName + " " + a.LastName).ToLower().Contains(term) || (a.FirstName + a.LastName).ToLower().Contains(term) || (a.NationalId != null && a.NationalId.Contains(term)));
        }
        if (!string.IsNullOrWhiteSpace(filter.Email)) query = query.Where(a => a.Email.ToLower().Contains(filter.Email.Trim().ToLower()));
        query = (filter.SortBy?.ToLower(), filter.SortDirection?.ToLower()) switch
        {
            ("name", "desc") => query.OrderByDescending(a => a.FirstName).ThenByDescending(a => a.LastName),
            ("name", _) => query.OrderBy(a => a.FirstName).ThenBy(a => a.LastName),
            ("registeredon", "desc") => query.OrderByDescending(a => a.CreatedOn),
            ("registeredon", _) => query.OrderBy(a => a.CreatedOn),
            ("email", "desc") => query.OrderByDescending(a => a.Email),
            ("email", _) => query.OrderBy(a => a.Email),
            _ => query.OrderByDescending(a => a.CreatedOn)
        };
        var accounts = await query.Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync(cancellationToken);
        var baseUrl = $"{_httpContextAccessor.HttpContext?.Request.Scheme}://{_httpContextAccessor.HttpContext?.Request.Host}/";
        var response = accounts.Select(a => new DashboardClientResponse(a.Id, a.FirstName, a.LastName, a.NationalId ?? "", a.Email, a.MainMobile, a.AlternateMobile, a.MainAddress, a.AlternateAddress ?? "", a.Gender, a.DateOfBirth ?? DateTime.UtcNow, !string.IsNullOrEmpty(a.ProfilePicturePath) ? baseUrl + a.ProfilePicturePath : null, a.AppUserId, a.CreatedOn));
        return Result.Success(response);
    }

    public async Task<Result<string>> UpdateClientAvatarAsync(int clientId, UpdateClientAvatarRequest request, CancellationToken cancellationToken)
    {
        var account = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == clientId && a.Role == DefaultRoles.User, cancellationToken);
        if (account is null) return Result.Failure<string>(UserErrors.UserNotFound);
        if (request.ProfilePicture is null || request.ProfilePicture.Length == 0) return Result.Failure<string>(new Error("Avatar.Invalid", "Please provide a valid image file."));
        var extension = Path.GetExtension(request.ProfilePicture.FileName).ToLower();
        if (!new[] { ".jpg", ".jpeg", ".png", ".webp" }.Contains(extension) || request.ProfilePicture.Length > 5 * 1024 * 1024) return Result.Failure<string>(new Error("Avatar.InvalidMetadata", "Invalid image format or size exceeds 5MB."));
        var relativePath = Path.Combine("User profile pictures", account.Id.ToString());
        var absolutePath = Path.Combine(_environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), relativePath);
        Directory.CreateDirectory(absolutePath);
        var storedFileName = $"{Guid.NewGuid()}{extension}";
        var relativeFilePath = Path.Combine(relativePath, storedFileName).Replace("\\", "/");
        var oldPath = account.ProfilePicturePath;
        if (!string.IsNullOrEmpty(oldPath))
        {
            var oldFilePath = Path.Combine(_environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), oldPath).Replace("/", "\\");
            if (File.Exists(oldFilePath)) File.Delete(oldFilePath);
        }
        using (var stream = new FileStream(Path.Combine(absolutePath, storedFileName), FileMode.Create))
            await request.ProfilePicture.CopyToAsync(stream, cancellationToken);
        account.ProfilePicturePath = relativeFilePath;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success($"{_httpContextAccessor.HttpContext?.Request.Scheme}://{_httpContextAccessor.HttpContext?.Request.Host}/{relativeFilePath}");
    }
}
