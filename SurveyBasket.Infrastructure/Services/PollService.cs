using Microsoft.EntityFrameworkCore;
using SurveyBasket.Application.DTOs.Files;
using SurveyBasket.Application.DTOs.Poll;
using SurveyBasket.Application.Interfaces;
using SurveyBasket.Domain.Abstractions;
using SurveyBasket.Domain.Entities;
using SurveyBasket.Domain.Errors;
using SurveyBasket.Infrastructure.Persistence;
using System.Security.Claims;

namespace SurveyBasket.Infrastructure.Services;

public class PollService : IPollService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PollService(AppDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IEnumerable<Poll>> GetPollsAsync(CancellationToken cancellationToken)
    {
        var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = _httpContextAccessor.HttpContext?.User.IsInRole(DefaultRoles.Admin) ?? false;
        var query = _dbContext.Polls.Include(p => p.Attachments).ThenInclude(a => a.UploadedBy).AsNoTracking();
        if (!isAdmin && !string.IsNullOrEmpty(currentUserId))
            query = query.Where(p => p.CreatedById == currentUserId);
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<Result<PollResponse>> GetPollByIdAsync(int id, CancellationToken cancellationToken)
    {
        var poll = await _dbContext.Polls.Include(p => p.Attachments).ThenInclude(a => a.UploadedBy).FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (poll is null) return Result.Failure<PollResponse>(PollErrors.PollNotFound);
        var images = poll.Attachments.Select(a => new FileResponse(a.Id, a.FileName, BuildFileUrl(a.StoredPath), a.ContentType, a.FileSize, $"{a.UploadedBy?.FirstName} {a.UploadedBy?.LastName}".Trim(), a.UploadedById, a.UploadedOn, a.PollId));
        return Result.Success(new PollResponse(poll.Id, poll.Title, poll.Summray, poll.IsPublished, poll.StartsAt, poll.EndsAt, images));
    }

    public async Task<Result<Poll>> AddAsync(Poll poll, CancellationToken cancellationToken)
    {
        var titleNorm = (poll.Title ?? "").Trim().ToLower();
        if (string.IsNullOrEmpty(titleNorm))
            return Result.Failure<Poll>(new Error("Poll.Invalid", "Title is required."));
        var exists = await _dbContext.Polls.IgnoreQueryFilters()
            .AnyAsync(p => p.Title != null && p.Title.Trim().ToLower() == titleNorm, cancellationToken);
        if (exists)
            return Result.Failure<Poll>(PollErrors.DuplicateTitle);

        var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(currentUserId))
        {
            var account = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.AppUserId == currentUserId, cancellationToken);
            if (account != null)
                poll.AccountId = account.Id;
        }
        await _dbContext.Polls.AddAsync(poll, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await LogAuditAsync(poll.Id, "Created", $"Poll '{poll.Title}' was created.", cancellationToken);
        return Result.Success(poll);
    }

    public async Task<Result> UpdateAsync(CreatePollRequest poll, int id, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Polls.FindAsync(id, cancellationToken);
        if (entity is null) return Result.Failure(PollErrors.PollNotFound);
        entity.Title = poll.Title;
        entity.Summray = poll.Summray;
        entity.IsPublished = poll.IsPublished;
        entity.StartsAt = poll.StartsAt;
        entity.EndsAt = poll.EndsAt;
        await _dbContext.SaveChangesAsync(cancellationToken);
        await LogAuditAsync(id, "Updated", $"Poll '{poll.Title}' was updated.", cancellationToken);
        return Result.Success();
    }

    public async Task<bool> DeleteAsync(int id, string? reason = null, CancellationToken cancellationToken = default)
    {
        var poll = await _dbContext.Polls.FindAsync(id, cancellationToken);
        if (poll is null) return false;
        var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = _httpContextAccessor.HttpContext?.User.IsInRole(DefaultRoles.Admin) ?? false;
        if (!isAdmin && poll.CreatedById != currentUserId) return false;
        poll.IsDeleted = true;
        poll.DeletionReason = reason;
        await _dbContext.SaveChangesAsync(cancellationToken);
        await LogAuditAsync(id, "Deleted", string.IsNullOrWhiteSpace(reason) ? $"Poll '{poll.Title}' was deleted." : $"Poll '{poll.Title}' was deleted. Reason: {reason}", cancellationToken);
        return true;
    }

    public async Task<Result> TogglePublishAsync(int id, CancellationToken cancellationToken)
    {
        var poll = await _dbContext.Polls.FindAsync(id, cancellationToken);
        if (poll is null) return Result.Failure(PollErrors.PollNotFound);
        poll.IsPublished = !poll.IsPublished;
        await _dbContext.SaveChangesAsync(cancellationToken);
        var action = poll.IsPublished ? "Published" : "Unpublished";
        await LogAuditAsync(id, action, $"Poll '{poll.Title}' was {action.ToLower()}.", cancellationToken);
        return Result.Success();
    }

    private async Task LogAuditAsync(int pollId, string action, string? details, CancellationToken cancellationToken)
    {
        var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(currentUserId)) return;
        var account = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.AppUserId == currentUserId, cancellationToken);
        var auditLog = new AuditLog
        {
            PollId = pollId,
            Action = action,
            PerformedById = currentUserId,
            PerformedOn = DateTime.UtcNow,
            Details = details,
            AccountId = account?.Id ?? 0
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
