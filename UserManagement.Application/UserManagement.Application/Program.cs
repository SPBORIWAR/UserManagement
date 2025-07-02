using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
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

// ------------------------------------------------
// Configure Database Connection
// ------------------------------------------------
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

// ------------------------------------------------
// Configure CORS
// ------------------------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .SetIsOriginAllowed(_ => true)
            .AllowCredentials();
    });
});

// ------------------------------------------------
// Configure JWT Authentication
// ------------------------------------------------
var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"]);

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

        NameClaimType = ClaimTypes.Name,          // maps to "unique_name"
        RoleClaimType = ClaimTypes.Role           // maps to "role"
    };

    // Optional: support for SignalR tokens via query string
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            Console.WriteLine("✅ TOKEN VALIDATED");
            foreach (var claim in context.Principal.Claims)
            {
                Console.WriteLine($"Claim: {claim.Type} = {claim.Value}");
            }
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine("❌ Authentication failed: " + context.Exception.Message);
            return Task.CompletedTask;
        },
        OnMessageReceived = context =>
        {
            // Capture token from Authorization header
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader))
            {
                var token = authHeader;

                // Remove any redundant "Bearer " prefixes
                while (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    token = token.Substring("Bearer ".Length).Trim();
                }

                context.Token = token;
            }

            // Optional: check for query string token (for SignalR support)
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
// ------------------------------------------------
// Add Authorization (can extend with policies if needed)
// ------------------------------------------------
builder.Services.AddAuthorization();
// ------------------------------------------------
// Add Controllers
// ------------------------------------------------
builder.Services.AddControllers();

// ------------------------------------------------
// Configure Swagger/OpenAPI with JWT Authorization
// ------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "UserManagement API", Version = "v1" });

    var securitySchema = new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    };

    c.AddSecurityDefinition("Bearer", securitySchema);

    var securityRequirement = new OpenApiSecurityRequirement
    {
        { securitySchema, new[] { "Bearer" } }
    };

    c.AddSecurityRequirement(securityRequirement);

    // 👈 Add this line:
    c.OperationFilter<AuthorizeCheckOperationFilter>();
});

var app = builder.Build();

// ------------------------------------------------
// Middleware Pipeline Configuration
// ------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "UserManagement API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.Use(async (context, next) =>
{
    var token = context.Request.Headers["Authorization"].FirstOrDefault();
    Console.WriteLine($"Auth Header: {token}");
    await next();
});

app.UseAuthentication(); // Enable authentication middleware

app.UseAuthorization();  // Enable authorization middleware

app.UseStaticFiles();
app.MapControllers();
// ------------------------------------------------
// Seed Default Tenant, SuperAdmin Role & User
// ------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<IDatabaseSeeder>();
    await seeder.SeedAsync();
}
app.Run();