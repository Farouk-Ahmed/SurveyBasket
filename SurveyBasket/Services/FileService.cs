using SurveyBasket.Abstractions;
using SurveyBasket.Contract.Files.Response;
using SurveyBasket.Entities;
using SurveyBasket.Errors;
using SurveyBasket.NewFolder;
using System.Security.Claims;

namespace SurveyBasket.Services
{
    public class FileService : IFileService
    {
        private readonly AppDBContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _environment;

        private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp"];
        private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

        public FileService(AppDBContext dbContext, IHttpContextAccessor httpContextAccessor, IWebHostEnvironment environment)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _environment = environment;
        }

        public async Task<Result<FileResponse>> UploadAsync(IFormFile file, int? pollId, CancellationToken cancellationToken)
        {
            // Validate file
            if (file is null || file.Length == 0)
                return Result.Failure<FileResponse>(FileErrors.EmptyFile);

            if (file.Length > MaxFileSize)
                return Result.Failure<FileResponse>(FileErrors.FileTooLarge);

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!AllowedExtensions.Contains(extension))
                return Result.Failure<FileResponse>(FileErrors.InvalidFileType);

            var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                return Result.Failure<FileResponse>(FileErrors.Unauthorized);

            // Determine folder path
            string relativePath = string.Empty;

            if (pollId.HasValue)
            {
                // Check if this poll already has images
                var existingAttachment = await _dbContext.Attachments
                    .Where(a => a.PollId == pollId.Value && !string.IsNullOrEmpty(a.StoredPath))
                    .FirstOrDefaultAsync(cancellationToken);

                if (existingAttachment != null)
                {
                    // Reuse the existing folder for this poll
                    // StoredPath looks like "uploads/userId/folder/file.ext"
                    relativePath = Path.GetDirectoryName(existingAttachment.StoredPath) ?? string.Empty;
                }
                else
                {
                    // No previous images -> Create a folder with current date and time (hours and minutes)
                    var timestampStr = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm");
                    relativePath = Path.Combine("uploads", currentUserId, timestampStr);
                }
            }
            else
            {
                // No poll -> standard date fallback
                relativePath = Path.Combine("uploads", currentUserId, DateTime.UtcNow.ToString("yyyy-MM-dd"));
            }

            relativePath = relativePath.Replace("\\", "/");
            var absolutePath = Path.Combine(_environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), relativePath);

            // Create directory if not exists
            Directory.CreateDirectory(absolutePath);

            // Generate unique file name to avoid conflicts
            var storedFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(absolutePath, storedFileName);
            var relativeFilePath = Path.Combine(relativePath, storedFileName);

            // Save file to disk
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            // Save record to database
            var attachment = new Attachment
            {
                FileName = file.FileName,
                StoredPath = relativeFilePath.Replace("\\", "/"),
                ContentType = file.ContentType,
                FileSize = file.Length,
                UploadedById = currentUserId,
                UploadedOn = DateTime.UtcNow,
                PollId = pollId
            };

            await _dbContext.Attachments.AddAsync(attachment, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var fileUrl = BuildFileUrl(attachment.StoredPath);
            var user = await _dbContext.Users.FindAsync(currentUserId);
            var response = new FileResponse(
                attachment.Id,
                attachment.FileName,
                fileUrl,
                attachment.ContentType,
                attachment.FileSize,
                $"{user?.FirstName} {user?.LastName}".Trim(),
                attachment.UploadedById,
                attachment.UploadedOn,
                attachment.PollId
            );

            return Result.Success(response);
        }

        public async Task<IEnumerable<FileResponse>> GetMyFilesAsync(CancellationToken cancellationToken)
        {
            var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                return [];

            return await GetFilesByUserAsync(currentUserId, cancellationToken);
        }

        public async Task<IEnumerable<FileResponse>> GetUserFilesAsync(string userId, CancellationToken cancellationToken)
        {
            return await GetFilesByUserAsync(userId, cancellationToken);
        }

        public async Task<IEnumerable<FileResponse>> GetAllFilesAsync(CancellationToken cancellationToken)
        {
            var files = await _dbContext.Attachments
                .Include(a => a.UploadedBy)
                .OrderByDescending(a => a.UploadedOn)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return files.Select(MapToResponse);
        }

        public async Task<Result> DeleteFileAsync(int id, CancellationToken cancellationToken)
        {
            var attachment = await _dbContext.Attachments.FindAsync(id, cancellationToken);
            if (attachment is null)
                return Result.Failure(FileErrors.FileNotFound);

            // Delete from disk
            var absolutePath = Path.Combine(
                _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
                attachment.StoredPath.Replace("/", Path.DirectorySeparatorChar.ToString())
            );

            if (File.Exists(absolutePath))
                File.Delete(absolutePath);

            // Delete from database
            _dbContext.Attachments.Remove(attachment);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }

        private async Task<IEnumerable<FileResponse>> GetFilesByUserAsync(string userId, CancellationToken cancellationToken)
        {
            var files = await _dbContext.Attachments
                .Include(a => a.UploadedBy)
                .Where(a => a.UploadedById == userId)
                .OrderByDescending(a => a.UploadedOn)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return files.Select(MapToResponse);
        }

        private FileResponse MapToResponse(Attachment a)
        {
            return new FileResponse(
                a.Id,
                a.FileName,
                BuildFileUrl(a.StoredPath),
                a.ContentType,
                a.FileSize,
                $"{a.UploadedBy?.FirstName} {a.UploadedBy?.LastName}".Trim(),
                a.UploadedById,
                a.UploadedOn,
                a.PollId
            );
        }

        private string BuildFileUrl(string storedPath)
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            return $"{request?.Scheme}://{request?.Host}/{storedPath}";
        }
    }
}
