using FluentValidation.AspNetCore;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SurveyBasket.NewFolder;
using System.Reflection;

namespace SurveyBasket
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddSurveyBasket(this IServiceCollection builder ,IConfiguration configuration)
        {

            // Add services to the container.

            builder.AddControllers();

            builder
                .AddSwaggeeServices()
                .AddMapsterConf()
                .AddFluentValidation();
            // Register the DbContext with the connection string from configuration
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            builder.AddDbContext<AppDBContext>(options =>
                options.UseSqlServer(connectionString));

            // Register the Poll_serviceis as a service
            builder.AddScoped<IPoll_serveis, Poll_serviceis>();
            return builder;
        }
        public static IServiceCollection AddSwaggeeServices(this IServiceCollection builder)
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

        public static IServiceCollection AddMapsterConf(this IServiceCollection builder)
        {
          
            // Add Mapster for mapping
            var mappingConfig = TypeAdapterConfig.GlobalSettings;
            mappingConfig.Scan(Assembly.GetExecutingAssembly());

            builder.AddSingleton<IMapper>(new Mapper(mappingConfig));
           
            return builder;
        }
        public static IServiceCollection AddFluentValidation(this IServiceCollection builder)
        {
            
            builder.AddFluentValidationAutoValidation()
                .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
     

            return builder;
        }
    }
}
