using FluentValidation;
using FluentValidation.AspNetCore;
using MapsterMapper;
using SurveyBasket.Application.Validators;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SurveyBasket.Infrastructure;
using SurveyBasket.Infrastructure.ExternalServices;
using SurveyBasket.Infrastructure.Persistence;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
var allowOrigins = builder.Configuration.GetSection("AllowAOrigin").Get<string[]>();
builder.Services.AddCors(options => options.AddPolicy("AllowAll", policy =>
{
    policy.AllowAnyMethod().AllowAnyHeader().AllowCredentials().WithOrigins(allowOrigins ?? ["http://localhost:4200"]);
}));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SurveyBasket API",
        Version = "v1",
        Description = "API documentation for SurveyBasket",
        Contact = new OpenApiContact { Name = "Support Team", Email = "support@surveybasket.com" }
    });
});

var mappingConfig = Mapster.TypeAdapterConfig.GlobalSettings;
mappingConfig.Scan(Assembly.GetExecutingAssembly());
builder.Services.AddSingleton<IMapper>(new Mapper(mappingConfig));

builder.Services.AddFluentValidationAutoValidation().AddValidatorsFromAssembly(typeof(RegisterRequestValidator).Assembly);

builder.Services.AddInfrastructure(builder.Configuration);

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()!;
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.IssuerSigningKey)),
        ValidIssuer = jwtOptions.ValidIssuer,
        ValidAudience = jwtOptions.ValidAudiences
    };
});

var app = builder.Build();

await DataSeeder.SeedAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "SurveyBasket API V1");
        options.RoutePrefix = string.Empty;
    });
}

app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
