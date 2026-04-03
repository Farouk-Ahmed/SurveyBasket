using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SurveyBasket.Application.Interfaces;
using SurveyBasket.Domain.Entities;
using SurveyBasket.Infrastructure.ExternalServices;
using SurveyBasket.Infrastructure.Persistence;
using SurveyBasket.Infrastructure.Services;

namespace SurveyBasket.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
        services.AddIdentity<AppUser, IdentityRole>().AddEntityFrameworkStores<AppDbContext>();

        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
        services.AddOptions<JwtOptions>().BindConfiguration(JwtOptions.SectionName).ValidateDataAnnotations().ValidateOnStart();
        services.AddScoped<ITokenProvider, TokenProvider>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<IPollService, PollService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddHttpContextAccessor();

        return services;
    }
}
