using FluentValidation.AspNetCore;
using MapsterMapper;
using MassTransit;
using MassTransit.RabbitMqTransport; // Added for UsingRabbitMq extension
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens; // Added for JwtBearerDefaults
using Microsoft.OpenApi.Models;
using SurveyBasket.NewFolder;
using SurveyBasket.Services;
using SurveyBasket.Services.Authntchan;
using SurveyBasket.Services.Authntchan.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Text;

namespace SurveyBasket
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddSurveyBasket(this IServiceCollection builder, IConfiguration configuration)
        {
    
            // Add services to the container.

            builder.AddControllers();
            var AllowAOrigins = configuration.GetSection("AllowAOrigin").Get<string[]>();
            builder.AddCors(option=>option.AddPolicy("AllowAll",policy=>
            {
                policy
                     // .AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                 .WithOrigins(AllowAOrigins!);
            }));

            builder
                .AddSwaggeeConf()
                .AddMapsterConf()
                .AddFluentConf()
                .AddAuthConf(configuration);
			// Register the DbContext with the connection string from configuration
			var connectionString = configuration.GetConnectionString("DefaultConnection");
            builder.AddDbContext<AppDBContext>(options =>
                options.UseSqlServer(connectionString));

            // Register the Poll_serviceis as a service
            builder.AddScoped<IPoll_serveis, Poll_serviceis>();
            builder.AddScoped<IAuthService, AuthService>();
         

            return builder;
        }
        private static IServiceCollection AddSwaggeeConf(this IServiceCollection builder)
        {
            // Add Swagger services
            builder.AddEndpointsApiExplorer();
            builder.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "SurveyBasket API",
                    Version = "v1",
                    Description = "API documentation for SurveyBasket",
                    Contact = new OpenApiContact
                    {
                        Name = "Support Team",
                        Email = "support@surveybasket.com"
                    }
                });
            });
        
            return builder;
        }

        private static IServiceCollection AddMapsterConf(this IServiceCollection builder)
        {
          
            // Add Mapster for mapping
            var mappingConfig = TypeAdapterConfig.GlobalSettings;
            mappingConfig.Scan(Assembly.GetExecutingAssembly());

            builder.AddSingleton<IMapper>(new Mapper(mappingConfig));
           
            return builder;
        }
        private static IServiceCollection AddFluentConf(this IServiceCollection builder)
        {
            
            builder.AddFluentValidationAutoValidation()
                .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
     

            return builder;
        }
		private static IServiceCollection AddAuthConf(this IServiceCollection builder , IConfiguration configuration)
		{
            builder.AddIdentity<AppUser, IdentityRole>().AddEntityFrameworkStores<AppDBContext>();
            
            // Configure JwtOptions
            //builder.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
               builder.AddOptions<JwtOptions>()
                .BindConfiguration(JwtOptions.SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();
            var settings = new JwtOptions();
            configuration.GetSection(JwtOptions.SectionName).Bind(settings);
            builder.AddSingleton(settings);
    
            // Register TokenProveder as scoped instead of singleton since it depends on JwtOptions
            builder.AddScoped<ITokenProvedr, TokenProveder>();

            builder.AddAuthentication(op =>
            {                                                                                                 
                op.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                op.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(o =>
            {
                o.SaveToken = true;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.IssuerSigningKey)),
                    ValidIssuer = settings.ValidIssuer,
                    ValidAudience = settings.ValidAudiences
                };
            });
    
            return builder;
		}
	}
}
