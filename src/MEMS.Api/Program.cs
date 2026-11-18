using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using MEMS.Api.Middleware;
using MEMS.Data;
using MEMS.Data.Entities;
using MEMS.Data.Repositories;
using MEMS.Data.Seeding;
using MEMS.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// Aspire Service Defaults
builder.AddServiceDefaults();

// Controllers with JSON options
builder.Services.AddControllers();

// Database connection
var connectionString = builder.Configuration.GetConnectionString("memsdb")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=(localdb)\\mssqllocaldb;Database=MEMS_Db;Trusted_Connection=True;MultipleActiveResultSets=true";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sql =>
    {
        sql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null);
    }));

// ASP.NET Core Identity
builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// Repositories
builder.Services.AddScoped<IMunicipalityRepository, MunicipalityRepository>();
builder.Services.AddScoped<IPositionRepository, PositionRepository>();
builder.Services.AddScoped<ICandidateRepository, CandidateRepository>();

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "MEMS_SuperSecret_Key_For_Jwt_Auth_Token_Signing_2026_COMP4976!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "MEMS.Api";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "MEMS.Clients";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();

// CORS for Blazor WASM Public site & Admin clients
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Native OpenAPI generation
builder.Services.AddOpenApi();

var app = builder.Build();

// Aspire Health Checks and Metrics endpoints
app.MapDefaultEndpoints();

// Global Exception Handling Middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

// OpenAPI endpoints
app.MapOpenApi();
app.MapScalarApiReference();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Auto seed database during initialization
using (var scope = app.Services.CreateScope())
{
    try
    {
        await DbInitializer.InitializeAsync(scope.ServiceProvider, app.Configuration);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Initial database migration/seeding will occur when SQL Server becomes available.");
    }
}

app.Run();
