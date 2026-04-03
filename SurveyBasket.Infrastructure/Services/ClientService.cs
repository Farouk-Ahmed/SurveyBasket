using Microsoft.EntityFrameworkCore;
using SurveyBasket.Application.DTOs.Clients;
using SurveyBasket.Application.Interfaces;
using SurveyBasket.Domain.Abstractions;
using SurveyBasket.Domain.Entities;
using SurveyBasket.Domain.Errors;
using SurveyBasket.Infrastructure.Persistence;

namespace SurveyBasket.Infrastructure.Services;

public class ClientService : IClientService
{
    private readonly AppDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;

    public ClientService(AppDbContext dbContext, IWebHostEnvironment environment)
    {
        _dbContext = dbContext;
        _environment = environment;
    }

    public async Task<Result<ClientProfileResponse>> GetMyProfileAsync(string userId, CancellationToken cancellationToken = default)
    {
        var account = await EnsureAccountForClientAsync(userId, cancellationToken);
        if (account is null) return Result.Failure<ClientProfileResponse>(UserErrors.UserNotFound);
        var dob = account.DateOfBirth ?? DateTime.UtcNow.AddYears(-25);
        return Result.Success(new ClientProfileResponse(account.Id, account.UserName, account.FirstName, account.LastName, account.NationalId ?? "", account.Email, account.MainMobile, account.AlternateMobile, account.MainAddress, account.AlternateAddress, account.Gender, dob, account.ProfilePicturePath));
    }

    public async Task<Result> UpdateMyProfileAsync(string userId, UpdateClientProfileRequest request, CancellationToken cancellationToken = default)
    {
        var account = await EnsureAccountForClientAsync(userId, cancellationToken);
        if (account is null) return Result.Failure(UserErrors.UserNotFound);
        account.FirstName = request.FirstName;
        account.LastName = request.LastName;
        account.MainAddress = request.MainAddress;
        account.AlternateAddress = request.AlternateAddress;
        account.MainMobile = request.MainMobile;
        account.AlternateMobile = request.AlternateMobile;
        account.DateOfBirth = request.DateOfBirth;
        account.Gender = request.Gender;
        var appUser = await _dbContext.Users.FindAsync([userId], cancellationToken);
        if (appUser != null)
        {
            appUser.FirstName = request.FirstName;
            appUser.LastName = request.LastName;
        }
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<string>> UpdateMyAvatarAsync(string userId, UpdateClientAvatarRequest request, CancellationToken cancellationToken = default)
    {
        var account = await EnsureAccountForClientAsync(userId, cancellationToken);
        if (account is null) return Result.Failure<string>(UserErrors.UserNotFound);
        if (request.ProfilePicture is null || request.ProfilePicture.Length == 0) return Result.Failure<string>(new Error("Avatar.Invalid", "Please provide a valid image file."));
        var extension = Path.GetExtension(request.ProfilePicture.FileName).ToLower();
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        if (!allowedExtensions.Contains(extension) || request.ProfilePicture.Length > 5 * 1024 * 1024) return Result.Failure<string>(new Error("Avatar.InvalidMetadata", "Invalid image format or size exceeds 5MB."));
        var relativePath = Path.Combine("User profile pictures", account.Id.ToString());
        var absolutePath = Path.Combine(_environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), relativePath);
        Directory.CreateDirectory(absolutePath);
        var storedFileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(absolutePath, storedFileName);
        var relativeFilePath = Path.Combine(relativePath, storedFileName).Replace("\\", "/");
        if (!string.IsNullOrEmpty(account.ProfilePicturePath))
        {
            var oldFilePath = Path.Combine(_environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), account.ProfilePicturePath).Replace("/", "\\");
            if (File.Exists(oldFilePath)) File.Delete(oldFilePath);
        }
        using (var stream = new FileStream(filePath, FileMode.Create))
            await request.ProfilePicture.CopyToAsync(stream, cancellationToken);
        account.ProfilePicturePath = relativeFilePath;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(relativeFilePath);
    }

    /// <summary>Ensures Account exists for userId with Role=User. Returns null if user not found.</summary>
    private async Task<Account?> EnsureAccountForClientAsync(string userId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId)) return null;
        userId = userId.Trim();
        var account = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.AppUserId == userId, cancellationToken);
        if (account is not null)
        {
            if (account.Role != DefaultRoles.User)
                account.Role = DefaultRoles.User;
            return account;
        }
        var appUser = await _dbContext.Users.FindAsync([userId], cancellationToken);
        if (appUser is null) return null;
        account = new Account
        {
            AppUserId = appUser.Id,
            Role = DefaultRoles.User,
            FirstName = appUser.FirstName,
            LastName = appUser.LastName,
            Email = appUser.Email ?? "",
            UserName = appUser.UserName ?? $"{appUser.FirstName}{appUser.LastName}".Trim(),
            CreatedOn = DateTime.UtcNow
        };
        await _dbContext.Accounts.AddAsync(account, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return account;
    }
}
