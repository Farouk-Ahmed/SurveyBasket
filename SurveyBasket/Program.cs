using Microsoft.OpenApi.Models;
using SurveyBasket;
using SurveyBasket.NewFolder;
using Microsoft.AspNetCore.Identity;
using SurveyBasket.Entities;
using SurveyBasket.Persitence;

var builder = WebApplication.CreateBuilder(args);
// Call the extension method to register services
builder.Services.AddSurveyBasket(builder.Configuration);

var app = builder.Build();

// Seed roles and default admin user
await DataSeeder.SeedAsync(app.Services);

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

app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
