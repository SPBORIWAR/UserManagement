using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NSwag;
using NSwag.Generation.Processors.Security;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using UserManagement.Application.Helper;
using UserManagement.BusinessLogic.AuthManagement;
using UserManagement.BusinessLogic.DatabaseSeed;
using UserManagement.BusinessLogic.PermissionManagement;
using UserManagement.BusinessLogic.RoleManagement;
using UserManagement.BusinessLogic.SessionManagment;
using UserManagement.BusinessLogic.TenantManagement;
using UserManagement.EntityFrameworkCore.Context;
using UserManagement.EntityFrameworkCore.Models;
using UserManagement.EntityFrameworkCore.Repository;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// ------------------------------------------------
// Register Dependency Injection for Services
// ------------------------------------------------
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IDatabaseSeeder, DatabaseSeeder>();
builder.Services.AddScoped<IAuthService, AuthService>();

// 1. Configure JWT authentication key
var key = Encoding.ASCII.GetBytes(configuration["Jwt:Key"]);

// 2. Add JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero,
        NameClaimType = ClaimTypes.Name,
        RoleClaimType = ClaimTypes.Role
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                context.Token = authHeader.Substring("Bearer ".Length).Trim();
            }

            // Support for SignalR token via query string (optional)
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chathub"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

// 3. Add Authorization (if you want policies or role-based auth)
builder.Services.AddAuthorization();

// 4. Add Controllers
builder.Services.AddControllers();

// 5. Configure NSwag OpenAPI Document with JWT security definition
builder.Services.AddOpenApiDocument(config =>
{
    config.Title = "UserManagement API";
    config.Version = "v1";

    // Add JWT Bearer security definition so Swagger UI shows the lock icon & token input
    config.AddSecurity("JWT", Enumerable.Empty<string>(), new NSwag.OpenApiSecurityScheme
    {
        Type = OpenApiSecuritySchemeType.ApiKey,
        Name = "Authorization",
        In = OpenApiSecurityApiKeyLocation.Header,
        Description = "Enter JWT Bearer token in the format: Bearer {your token}"
    });

    // Adds security requirements to all operations decorated with [Authorize]
    config.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("JWT"));
});

// 6. Configure CORS (adjust origins as needed)
const string DefaultCorsPolicyName = "AllowAll";
builder.Services.AddCors(options =>
{
    options.AddPolicy(DefaultCorsPolicyName, policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .SetIsOriginAllowed(_ => true)
              .AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();      // Serves /swagger/v1/swagger.json
    app.UseSwaggerUi();   // Serves Swagger UI at /swagger
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors(DefaultCorsPolicyName);

app.Use(async (context, next) =>
{
    var token = context.Request.Headers["Authorization"].FirstOrDefault();
    Console.WriteLine($"Auth Header: {token}");
    await next();
});

app.UseAuthentication(); // IMPORTANT: Add Authentication before Authorization
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<IDatabaseSeeder>();
    await seeder.SeedAsync();
}

app.Run();
