using System.Security.Claims;
using GameRankAuth.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using GameRankAuth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using GameRankAuth.Models;
using StackExchange.Redis;
using AutoMapper;
using GameRankAuth.Interfaces;
using GameRankAuth.Services.RabbitMQ;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using GameRankAuth.Middleware;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Конфигурация B2Settings
builder.Services.Configure<B2Settings>(
    builder.Configuration.GetSection("B2Settings"));
builder.Services.AddSingleton<B2Settings>(sp => 
    sp.GetRequiredService<IOptions<B2Settings>>().Value);
builder.Services.AddScoped<IQrCodeGeneratorService, QrCodeGeneratorService>();

// Redis ------------------------------------------------------
builder.Services.AddScoped<IDistributedCache, RedisCache>();
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = ConfigurationOptions.Parse("localhost:6379");
    configuration.AbortOnConnectFail = false;
    return ConnectionMultiplexer.Connect(configuration);
});

// Конект админки бд 
builder.Services.AddDbContext<AdminPanelDBContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("AdminDBConnection")));

builder.Services.AddControllers();
builder.Logging.AddConsole();
builder.Services.AddHttpContextAccessor();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// JWT Settings -------------------------------------------------------------------------------
builder.Services.AddScoped<JWTTokenService>();
builder.Services.AddScoped<IChangeUserDataService, ChangeUserDataService>();
builder.Services.AddScoped<IVerifyService, VerifyEmailService>();
builder.Services.AddScoped<RabbitMQService>();
builder.Services.AddScoped<IAvatarService, AvatarService>();

// Добавление аутентификации
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<AuthSettings>();
builder.Services.AddSingleton(jwtSettings);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
        .Build();
});
// -------------------------------------------------------------------------------------------

// CORS Settings --------------------------------------------------------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost", "http://192.168.0.103")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); 
    });
});
// ---------------------------------------------------------------------------------------------

builder.Services.AddMemoryCache();
//-----------------------------------------------------------------------------------
builder.Services.AddAutoMapper(typeof(UserProfile));
builder.Services.AddScoped<IAuthService, AuthService>();

// Identity Settings -----------------------------------------------------------------------------------------
builder.Services.AddDbContext<ApplicationDbContext>(options => 
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    
    // Добавляем параметры пула
    var connectionStringWithPool = connectionString + 
                                   ";Maximum Pool Size=100" +
                                   ";Minimum Pool Size=20" +
                                   ";Connection Idle Lifetime=300" +
                                   ";Connection Pruning Interval=10" +
                                   ";Timeout=30" +
                                   ";Command Timeout=30";

    options.UseNpgsql(connectionStringWithPool, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null); // Исправлено: добавлен третий параметр
    });
});

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<PasswordHasherOptions>(options =>
{
    options.IterationCount = 10000; 
});

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // BruteForce 
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.AllowedForNewUsers = true;
});
// -------------------------------------------------------------------------------------------------------------

builder.WebHost.UseUrls("http://192.168.0.103:5001");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Создание ролей
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var roles = new[] { "Admin", "User", "Creator" };
    
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }
}

app.UseRouting();
app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseMiddleware<BanCheckMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();