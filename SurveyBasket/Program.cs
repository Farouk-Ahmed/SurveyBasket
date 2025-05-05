using Microsoft.AspNetCore.Builder;
using Microsoft.OpenApi.Models; // Add this using directive
using Microsoft.Extensions.DependencyInjection; // Add this using directive for AddSwaggerGen extension method

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<IPoll_serveis, Poll_serviceis>(); // Register the Poll_serviceis as a service
builder.Services.AddControllers();
// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
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
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	// Enable Swagger middleware
	app.UseSwagger();
	app.UseSwaggerUI(options =>
	{
		options.SwaggerEndpoint("/swagger/v1/swagger.json", "SurveyBasket API V1");
		options.RoutePrefix = string.Empty; // Makes Swagger UI available at the root URL
	});
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
