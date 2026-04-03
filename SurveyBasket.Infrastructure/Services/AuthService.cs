using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SurveyBasket.Application.DTOs.Auth;
using SurveyBasket.Application.Interfaces;
using SurveyBasket.Domain.Abstractions;
using SurveyBasket.Domain.Entities;
using SurveyBasket.Domain.Errors;
using SurveyBasket.Infrastructure.Persistence;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;

namespace SurveyBasket.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ITokenProvider _tokenProvider;
    private readonly IEmailService _emailService;
    private readonly AppDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;

    public AuthService(UserManager<AppUser> userManager, ITokenProvider tokenProvider, IEmailService emailService, AppDbContext dbContext, IWebHostEnvironment environment)
    {
        _userManager = userManager;
        _tokenProvider = tokenProvider;
        _emailService = emailService;
        _dbContext = dbContext;
        _environment = environment;
    }

    public async Task<Result<AuthResponse>> AuthResponseAsync(string email, string password, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);
        if (!await _userManager.CheckPasswordAsync(user, password))
            return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);

        var roles = await _userManager.GetRolesAsync(user);
        var refreshToken = new RefreshToken
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            ExpirationOn = DateTime.UtcNow.AddDays(7),
            CreatedOn = DateTime.UtcNow
        };
        var userWithTokens = await _dbContext.Users.Include(u => u.RefreshTokens).FirstOrDefaultAsync(u => u.Id == user.Id, cancellationToken);
        if (userWithTokens != null)
        {
            userWithTokens.RefreshTokens ??= new List<RefreshToken>();
            userWithTokens.RefreshTokens.Add(refreshToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        await EnsureAccountHasRoleAsync(user, roles, cancellationToken);

        var (token, expireIn) = _tokenProvider.GenerateToken(user, roles);
        var profilePicturePath = await GetProfilePicturePathAsync(user.Id, cancellationToken);
        return Result.Success(new AuthResponse(user.Id, user.Email, user.FirstName ?? "", user.LastName ?? "", token, expireIn, refreshToken.Token, profilePicturePath));
    }

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var user = new AppUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };
        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(" ", result.Errors.Select(e => e.Description));
            return Result.Failure<AuthResponse>(new Error("User.RegistrationFailed", errors));
        }

        var account = new Account
        {
            AppUserId = user.Id,
            Role = DefaultRoles.User,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            UserName = request.Email,
            NationalId = request.NationalId,
            MainAddress = request.MainAddress ?? "",
            AlternateAddress = request.AlternateAddress,
            MainMobile = request.MainMobile ?? "",
            AlternateMobile = request.AlternateMobile,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender ?? "",
            CreatedOn = DateTime.UtcNow
        };
        await _dbContext.Accounts.AddAsync(account, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        if (request.ProfilePicture is not null && request.ProfilePicture.Length > 0)
        {
            var extension = Path.GetExtension(request.ProfilePicture.FileName).ToLower();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            if (allowedExtensions.Contains(extension) && request.ProfilePicture.Length <= 5 * 1024 * 1024)
            {
                var relativePath = Path.Combine("User profile pictures", account.Id.ToString());
                var absolutePath = Path.Combine(_environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), relativePath);
                Directory.CreateDirectory(absolutePath);
                var storedFileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(absolutePath, storedFileName);
                var relativeFilePath = Path.Combine(relativePath, storedFileName).Replace("\\", "/");
                using (var stream = new FileStream(filePath, FileMode.Create))
                    await request.ProfilePicture.CopyToAsync(stream, cancellationToken);
                account.ProfilePicturePath = relativeFilePath;
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        await _userManager.AddToRoleAsync(user, DefaultRoles.User);
        var roles = await _userManager.GetRolesAsync(user);
        var refreshToken = new RefreshToken
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            ExpirationOn = DateTime.UtcNow.AddDays(7),
            CreatedOn = DateTime.UtcNow
        };
        user.RefreshTokens.Add(refreshToken);
        await _userManager.UpdateAsync(user);
        var (token, expireIn) = _tokenProvider.GenerateToken(user, roles);
        try { _ = _emailService.SendWelcomeEmailAsync(request.Email, cancellationToken); } catch { /* Don't fail registration if email fails */ }
        return Result.Success(new AuthResponse(user.Id, user.Email, user.FirstName ?? "", user.LastName ?? "", token, expireIn, refreshToken.Token, account.ProfilePicturePath));
    }

    public async Task<Result<AuthResponse>> RegisterSimpleAsync(SimpleRegisterRequest request, CancellationToken cancellationToken)
    {
        var user = new AppUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = "",
            LastName = ""
        };
        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(" ", result.Errors.Select(e => e.Description));
            return Result.Failure<AuthResponse>(new Error("User.RegistrationFailed", errors));
        }

        var account = new Account
        {
            AppUserId = user.Id,
            Role = DefaultRoles.User,
            FirstName = "",
            LastName = "",
            Email = request.Email,
            UserName = request.Email,
            NationalId = "",
            MainAddress = "",
            MainMobile = "",
            DateOfBirth = default,
            Gender = "",
            CreatedOn = DateTime.UtcNow
        };
        await _dbContext.Accounts.AddAsync(account, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _userManager.AddToRoleAsync(user, DefaultRoles.User);
        var roles = await _userManager.GetRolesAsync(user);
        var refreshToken = new RefreshToken
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            ExpirationOn = DateTime.UtcNow.AddDays(7),
            CreatedOn = DateTime.UtcNow
        };
        var userWithTokens = await _dbContext.Users.Include(u => u.RefreshTokens).FirstOrDefaultAsync(u => u.Id == user.Id, cancellationToken);
        if (userWithTokens != null)
        {
            userWithTokens.RefreshTokens ??= new List<RefreshToken>();
            userWithTokens.RefreshTokens.Add(refreshToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        var (token, expireIn) = _tokenProvider.GenerateToken(user, roles);
        try { _ = _emailService.SendWelcomeEmailAsync(request.Email, cancellationToken); } catch { /* Don't fail registration if email fails */ }
        return Result.Success(new AuthResponse(user.Id, user.Email, user.FirstName ?? "", user.LastName ?? "", token, expireIn, refreshToken.Token, null));
    }

    public async Task<AuthResponse?> RefreshTokenAsync(RefreshRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken)) return null;

        AppUser? user = null;
        if (!string.IsNullOrWhiteSpace(request.AccessToken))
        {
            var handler = new JwtSecurityTokenHandler();
            try
            {
                var jwt = handler.ReadJwtToken(request.AccessToken);
                var userId = jwt.Subject;
                if (!string.IsNullOrEmpty(userId))
                    user = await _userManager.FindByIdAsync(userId);
            }
            catch { /* ignore */ }
        }

        if (user == null)
        {
            user = await _dbContext.Users.Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.RefreshTokens.Any(rt => rt.Token == request.RefreshToken && rt.RevokeOn == null && !rt.IsExpired), cancellationToken);
            if (user == null) return null;
        }
        else
        {
            user = await _dbContext.Users.Include(u => u.RefreshTokens).FirstOrDefaultAsync(u => u.Id == user.Id, cancellationToken);
            if (user == null) return null;
        }

        var existing = user.RefreshTokens.FirstOrDefault(rt => rt.Token == request.RefreshToken);
        if (existing == null || !existing.IsActive) return null;
        existing.RevokeOn = DateTime.UtcNow;
        var newRefresh = new RefreshToken
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            ExpirationOn = DateTime.UtcNow.AddDays(7),
            CreatedOn = DateTime.UtcNow
        };
        user.RefreshTokens.Add(newRefresh);
        await _userManager.UpdateAsync(user);
        var roles = await _userManager.GetRolesAsync(user);
        await EnsureAccountHasRoleAsync(user, roles, cancellationToken);
        var (token, expiresIn) = _tokenProvider.GenerateToken(user, roles);
        var profilePicturePath = await GetProfilePicturePathAsync(user.Id, cancellationToken);
        return new AuthResponse(user.Id, user.Email, user.FirstName, user.LastName, token, expiresIn, newRefresh.Token, profilePicturePath);
    }

    public async Task<Result<CreateUserResponse>> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var role = request.Role?.Trim();
        if (string.Equals(role, DefaultRoles.Admin, StringComparison.OrdinalIgnoreCase))
            role = DefaultRoles.Admin;
        else if (string.Equals(role, DefaultRoles.User, StringComparison.OrdinalIgnoreCase))
            role = DefaultRoles.User;
        else
            return Result.Failure<CreateUserResponse>(UserErrors.InvalidRole);

        var generatedUserName = await GenerateUniqueUserName(request.FirstName.Trim(), request.LastName.Trim(), cancellationToken);
        var user = new AppUser
        {
            UserName = generatedUserName,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            EmailConfirmed = true
        };

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(" ", result.Errors.Select(e => e.Description));
                return Result.Failure<CreateUserResponse>(new Error("User.RegistrationFailed", errors));
            }
            await _userManager.AddToRoleAsync(user, role);

            var account = new Account
            {
                AppUserId = user.Id,
                Role = role,
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Email = request.Email.Trim(),
                UserName = generatedUserName,
                CreatedOn = DateTime.UtcNow,
                NationalId = string.IsNullOrWhiteSpace(request.NationalId) ? null : request.NationalId.Trim(),
                MainMobile = request.MainMobile?.Trim() ?? string.Empty,
                AlternateMobile = string.IsNullOrWhiteSpace(request.AlternateMobile) ? null : request.AlternateMobile.Trim(),
                MainAddress = request.MainAddress?.Trim() ?? string.Empty,
                AlternateAddress = string.IsNullOrWhiteSpace(request.AlternateAddress) ? null : request.AlternateAddress.Trim(),
                DateOfBirth = request.DateOfBirth,
                Gender = request.Gender?.Trim() ?? string.Empty
            };
            await _dbContext.Accounts.AddAsync(account, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        var roles = await _userManager.GetRolesAsync(user);
        var refreshToken = new RefreshToken
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            ExpirationOn = DateTime.UtcNow.AddDays(7),
            CreatedOn = DateTime.UtcNow
        };
        user.RefreshTokens.Add(refreshToken);
        await _userManager.UpdateAsync(user);
        var (token, expireIn) = _tokenProvider.GenerateToken(user, roles);
        return Result.Success(new CreateUserResponse(user.Id, generatedUserName, user.Email, user.FirstName, user.LastName, token, expireIn, refreshToken.Token, roles));
    }

    private async Task<string> GenerateUniqueUserName(string firstName, string lastName, CancellationToken cancellationToken)
    {
        var baseName = $"{firstName}{lastName}".Trim();
        if (string.IsNullOrWhiteSpace(baseName)) baseName = "User";
        var name = baseName;
        var existing = await _dbContext.Users.AnyAsync(u => u.UserName == name, cancellationToken);
        var n = 1;
        while (existing)
        {
            name = $"{baseName}{n}";
            n++;
            existing = await _dbContext.Users.AnyAsync(u => u.UserName == name, cancellationToken);
        }
        return name;
    }

    /// <summary>Ensures Account exists and has correct Role (Admin/User) from AspNetUser roles.</summary>
    private async Task EnsureAccountHasRoleAsync(AppUser user, IList<string> roles, CancellationToken cancellationToken)
    {
        var account = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.AppUserId == user.Id, cancellationToken);
        if (account is null)
        {
            account = new Account
            {
                AppUserId = user.Id,
                FirstName = user.FirstName ?? "",
                LastName = user.LastName ?? "",
                Email = user.Email ?? "",
                UserName = user.UserName ?? $"{user.FirstName}{user.LastName}".Trim(),
                Role = roles.Contains(DefaultRoles.Admin) ? DefaultRoles.Admin : DefaultRoles.User,
                CreatedOn = DateTime.UtcNow
            };
            await _dbContext.Accounts.AddAsync(account, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return;
        }
        var expectedRole = roles.Contains(DefaultRoles.Admin) ? DefaultRoles.Admin : DefaultRoles.User;
        if (account.Role != expectedRole)
        {
            account.Role = expectedRole;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<string?> GetProfilePicturePathAsync(string userId, CancellationToken cancellationToken)
    {
        var account = await _dbContext.Accounts.AsNoTracking().FirstOrDefaultAsync(a => a.AppUserId == userId, cancellationToken);
        return account?.ProfilePicturePath;
    }
}
