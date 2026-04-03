using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SurveyBasket.Domain.Abstractions;
using SurveyBasket.Domain.Entities;

namespace SurveyBasket.Infrastructure.Persistence;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        string[] roles = [DefaultRoles.Admin, DefaultRoles.User];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var adminEmail = configuration["DefaultAdmin:Email"];
        var adminPassword = configuration["DefaultAdmin:Password"];
        var adminUserName = configuration["DefaultAdmin:UserName"];
        var adminFirstName = configuration["DefaultAdmin:FirstName"];
        var adminLastName = configuration["DefaultAdmin:LastName"];

        if (!string.IsNullOrEmpty(adminEmail) && !string.IsNullOrEmpty(adminPassword))
        {
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new AppUser
                {
                    UserName = adminUserName,
                    Email = adminEmail,
                    FirstName = adminFirstName ?? "System",
                    LastName = adminLastName ?? "Admin",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, DefaultRoles.Admin);
                    await EnsureAccountExistsAsync(dbContext, adminUser, DefaultRoles.Admin);
                    await dbContext.SaveChangesAsync();
                }
            }
            else
            {
                await EnsureAccountExistsAsync(dbContext, adminUser, DefaultRoles.Admin);
                await dbContext.SaveChangesAsync();
            }
        }

        var accountUserIds = new HashSet<string>(await dbContext.Accounts.Select(a => a.AppUserId).ToListAsync());
        var allUsers = await dbContext.Users.ToListAsync();
        foreach (var user in allUsers)
        {
            if (!accountUserIds.Contains(user.Id))
            {
                var userRoles = await userManager.GetRolesAsync(user);
                var role = userRoles.Contains(DefaultRoles.Admin) ? DefaultRoles.Admin : DefaultRoles.User;
                await dbContext.Accounts.AddAsync(new Account
                {
                    AppUserId = user.Id,
                    Role = role,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email ?? "",
                    UserName = user.UserName ?? $"{user.FirstName}{user.LastName}".Trim(),
                    CreatedOn = DateTime.UtcNow
                });
                accountUserIds.Add(user.Id);
            }
            else
            {
                var userRoles = await userManager.GetRolesAsync(user);
                var expectedRole = userRoles.Contains(DefaultRoles.Admin) ? DefaultRoles.Admin : DefaultRoles.User;
                var account = await dbContext.Accounts.FirstOrDefaultAsync(a => a.AppUserId == user.Id);
                if (account != null && account.Role != expectedRole)
                {
                    account.Role = expectedRole;
                }
            }
        }
        await dbContext.SaveChangesAsync();
    }

    private static async Task EnsureAccountExistsAsync(AppDbContext dbContext, AppUser user, string role)
    {
        var exists = await dbContext.Accounts.AnyAsync(a => a.AppUserId == user.Id);
        if (exists) return;
        await dbContext.Accounts.AddAsync(new Account
        {
            AppUserId = user.Id,
            Role = role,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? "",
            UserName = user.UserName ?? $"{user.FirstName}{user.LastName}".Trim(),
            CreatedOn = DateTime.UtcNow
        });
    }
}
