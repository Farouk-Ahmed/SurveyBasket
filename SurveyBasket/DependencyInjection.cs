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

            builder
                .AddSwaggeeConf()
                .AddMapsterConf()
                .AddFluentConf()
                .AddAuthConf();
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
		private static IServiceCollection AddAuthConf(this IServiceCollection builder)
		{
            builder.AddSingleton<ITokenProvedr,TokenProveder>();
			builder.AddIdentity<AppUser,IdentityRole>().AddEntityFrameworkStores<AppDBContext>();
            builder.AddAuthentication(op =>
            {
                op.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                op.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }) // Added missing semicolon and fixed scheme name
                .AddJwtBearer(o =>
                {
                    o.SaveToken = true;
                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("JshgdgdhdjdhhgFDGDSGfGhtGh1DFEF5cdf7")),
                        ValidIssuer= "SurveyBasket",
                        ValidAudiences= new[] { "SurveyBasketUsers" }
					};
                });
			return builder;
		}
	}
}
