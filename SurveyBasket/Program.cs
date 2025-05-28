using Microsoft.OpenApi.Models;
using SurveyBasket;
using SurveyBasket.NewFolder; // Add this using directive
using Microsoft.AspNetCore.Identity;
using SurveyBasket.Entities;

var builder = WebApplication.CreateBuilder(args);
// Call the extension method to register services
builder.Services.AddSurveyBasket(builder.Configuration);



var app = builder.Build();

// Register the mapping configuration as a singleton
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

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
