using Microsoft.EntityFrameworkCore;
using SurveyBasket.Abstractions;
using SurveyBasket.Contract.Clients.Request;
using SurveyBasket.Contract.Clients.Response;
using SurveyBasket.Errors;
using SurveyBasket.NewFolder;
using Microsoft.AspNetCore.Hosting;

namespace SurveyBasket.Services
{
    public class ClientService : IClientService
    {
        private readonly AppDBContext _dbContext;
        private readonly IWebHostEnvironment _environment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClientService(AppDBContext dbContext, IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _environment = environment;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result<ClientProfileResponse>> GetMyProfileAsync(string userId, CancellationToken cancellationToken = default)
        {
            var client = await _dbContext.Clients
                .Include(c => c.AppUser)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.AppUserId == userId, cancellationToken);

            if (client is null)
                return Result.Failure<ClientProfileResponse>(UserErrors.UserNotFound);

            var response = new ClientProfileResponse(
                client.Id,
                client.FirstName,
                client.LastName,
                client.NationalId,
                client.Email,
                client.MainMobile,
                client.AlternateMobile,
                client.MainAddress,
                client.AlternateAddress,
                client.Gender,
                client.DateOfBirth,
                GetFullUrl(client.ProfilePicturePath)
            );

            return Result.Success(response);
        }

        public async Task<Result> UpdateMyProfileAsync(string userId, UpdateClientProfileRequest request, CancellationToken cancellationToken = default)
        {
            var client = await _dbContext.Clients
                .Include(c => c.AppUser)
                .FirstOrDefaultAsync(c => c.AppUserId == userId, cancellationToken);

            if (client is null)
                return Result.Failure(UserErrors.UserNotFound);

            // Update Client Entity
            client.FirstName = request.FirstName;
            client.LastName = request.LastName;
            client.MainAddress = request.MainAddress;
            client.AlternateAddress = request.AlternateAddress;
            client.MainMobile = request.MainMobile;
            client.AlternateMobile = request.AlternateMobile;
            client.DateOfBirth = request.DateOfBirth;
            client.Gender = request.Gender;

            // Update AppUser Entity to keep names in sync
            client.AppUser.FirstName = request.FirstName;
            client.AppUser.LastName = request.LastName;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }

        public async Task<Result<string>> UpdateMyAvatarAsync(string userId, UpdateClientAvatarRequest request, CancellationToken cancellationToken = default)
        {
            var client = await _dbContext.Clients
                .FirstOrDefaultAsync(c => c.AppUserId == userId, cancellationToken);

            if (client is null)
                return Result.Failure<string>(UserErrors.UserNotFound);

            if (request.ProfilePicture is null || request.ProfilePicture.Length == 0)
                return Result.Failure<string>(new Error("Avatar.Invalid", "Please provide a valid image file."));

            var extension = Path.GetExtension(request.ProfilePicture.FileName).ToLower();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

            if (!allowedExtensions.Contains(extension) || request.ProfilePicture.Length > 5 * 1024 * 1024)
                return Result.Failure<string>(new Error("Avatar.InvalidMetadata", "Invalid image format or size exceeds 5MB."));

            var relativePath = Path.Combine("User profile pictures", client.Id.ToString());
            var wwwroot = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var absolutePath = Path.Combine(wwwroot, relativePath);

            if (!Directory.Exists(absolutePath))
            {
                Directory.CreateDirectory(absolutePath);
            }

            // Delete old profile picture if it exists
            if (!string.IsNullOrEmpty(client.ProfilePicturePath))
            {
                var oldFilePath = Path.Combine(wwwroot, client.ProfilePicturePath.Replace("/", "\\"));
                if (File.Exists(oldFilePath))
                {
                    try
                    {
                        File.Delete(oldFilePath);
                    }
                    catch (IOException)
                    {
                        // Handle potential file lock or other IO issues
                    }
                }
            }

            var storedFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(absolutePath, storedFileName);
            var relativeFilePath = Path.Combine(relativePath, storedFileName).Replace("\\", "/");

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.ProfilePicture.CopyToAsync(stream, cancellationToken);
            }

            client.ProfilePicturePath = relativeFilePath;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success(GetFullUrl(relativeFilePath)!);
        }

        private string? GetFullUrl(string? relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return null;

            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null)
                return relativePath;

            return $"{request.Scheme}://{request.Host}/{relativePath.TrimStart('/')}";
        }
    }
}
