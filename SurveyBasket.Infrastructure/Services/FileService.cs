using Microsoft.EntityFrameworkCore;
using SurveyBasket.Application.DTOs.Files;
using SurveyBasket.Application.Interfaces;
using SurveyBasket.Domain.Abstractions;
using SurveyBasket.Domain.Entities;
using SurveyBasket.Domain.Errors;
using SurveyBasket.Infrastructure.Persistence;
using System.Security.Claims;

namespace SurveyBasket.Infrastructure.Services;

public class FileService : IFileService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IWebHostEnvironment _environment;
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp"];
    private const long MaxFileSize = 5 * 1024 * 1024;

    public FileService(AppDbContext dbContext, IHttpContextAccessor httpContextAccessor, IWebHostEnvironment environment)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _environment = environment;
    }

    public async Task<Result<FileResponse>> UploadAsync(IFormFile file, int? pollId, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0) return Result.Failure<FileResponse>(FileErrors.EmptyFile);
        if (file.Length > MaxFileSize) return Result.Failure<FileResponse>(FileErrors.FileTooLarge);
        var extension = Path.GetExtension(file.FileName).ToLower();
        if (!AllowedExtensions.Contains(extension)) return Result.Failure<FileResponse>(FileErrors.InvalidFileType);
        var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(currentUserId)) return Result.Failure<FileResponse>(FileErrors.Unauthorized);

        string relativePath;
        if (pollId.HasValue)
        {
            var existing = await _dbContext.Attachments.Where(a => a.PollId == pollId.Value && !string.IsNullOrEmpty(a.StoredPath)).FirstOrDefaultAsync(cancellationToken);
            relativePath = existing != null ? Path.GetDirectoryName(existing.StoredPath) ?? "" : Path.Combine("uploads", currentUserId, DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm"));
        }
        else
            relativePath = Path.Combine("uploads", currentUserId, DateTime.UtcNow.ToString("yyyy-MM-dd"));
        relativePath = relativePath.Replace("\\", "/");
        var absolutePath = Path.Combine(_environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), relativePath);
        Directory.CreateDirectory(absolutePath);
        var storedFileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(absolutePath, storedFileName);
        var relativeFilePath = Path.Combine(relativePath, storedFileName).Replace("\\", "/");
        using (var stream = new FileStream(filePath, FileMode.Create))
            await file.CopyToAsync(stream, cancellationToken);

        var account = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.AppUserId == currentUserId, cancellationToken);
        var attachment = new Attachment
        {
            FileName = file.FileName,
            StoredPath = relativeFilePath,
            ContentType = file.ContentType,
            FileSize = file.Length,
            UploadedById = currentUserId,
            UploadedOn = DateTime.UtcNow,
            PollId = pollId,
            AccountId = account?.Id ?? 0
        };
        await _dbContext.Attachments.AddAsync(attachment, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        var user = await _dbContext.Users.FindAsync(currentUserId);
        return Result.Success(new FileResponse(attachment.Id, attachment.FileName, BuildFileUrl(attachment.StoredPath), attachment.ContentType, attachment.FileSize, $"{user?.FirstName} {user?.LastName}".Trim(), attachment.UploadedById, attachment.UploadedOn, attachment.PollId));
    }

    public async Task<IEnumerable<FileResponse>> GetMyFilesAsync(CancellationToken cancellationToken)
    {
        var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return string.IsNullOrEmpty(currentUserId) ? [] : await GetFilesByUserAsync(currentUserId, cancellationToken);
    }

    public async Task<IEnumerable<FileResponse>> GetUserFilesAsync(string userId, CancellationToken cancellationToken) => await GetFilesByUserAsync(userId, cancellationToken);

    public async Task<IEnumerable<FileResponse>> GetAllFilesAsync(CancellationToken cancellationToken)
    {
        var files = await _dbContext.Attachments.Include(a => a.UploadedBy).OrderByDescending(a => a.UploadedOn).AsNoTracking().ToListAsync(cancellationToken);
        return files.Select(MapToResponse);
    }

    public async Task<Result> DeleteFileAsync(int id, CancellationToken cancellationToken)
    {
        var attachment = await _dbContext.Attachments.FindAsync(id, cancellationToken);
        if (attachment is null) return Result.Failure(FileErrors.FileNotFound);
        var absolutePath = Path.Combine(_environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), attachment.StoredPath.Replace("/", Path.DirectorySeparatorChar.ToString()));
        if (File.Exists(absolutePath)) File.Delete(absolutePath);
        _dbContext.Attachments.Remove(attachment);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private async Task<IEnumerable<FileResponse>> GetFilesByUserAsync(string userId, CancellationToken cancellationToken)
    {
        var files = await _dbContext.Attachments.Include(a => a.UploadedBy).Where(a => a.UploadedById == userId).OrderByDescending(a => a.UploadedOn).AsNoTracking().ToListAsync(cancellationToken);
        return files.Select(MapToResponse);
    }

    private FileResponse MapToResponse(Attachment a) => new(a.Id, a.FileName, BuildFileUrl(a.StoredPath), a.ContentType, a.FileSize, $"{a.UploadedBy?.FirstName} {a.UploadedBy?.LastName}".Trim(), a.UploadedById, a.UploadedOn, a.PollId);

    private string BuildFileUrl(string storedPath)
    {
        var request = _httpContextAccessor.HttpContext?.Request;
        return $"{request?.Scheme}://{request?.Host}/{storedPath}";
    }
}
