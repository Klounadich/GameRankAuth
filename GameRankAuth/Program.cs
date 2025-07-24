using GameRankAuth.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using GameRankAuth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using GameRankAuth.Models;
using AutoMapper;
using GameRankAuth.Interfaces;
using GameRankAuth.Services.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

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
builder.Services.AddAuth(builder.Configuration);
var jwtSection = builder.Configuration.GetSection("jwt");
var authSettings = builder.Configuration.GetSection("jwt").Get<AuthSettings>();
jwtSection.Bind(authSettings);
builder.Services.AddSingleton(authSettings);
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => 
        policy.RequireAuthenticatedUser()
            .RequireRole("Admin"));
    options.DefaultPolicy = new AuthorizationPolicyBuilder().AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireAuthenticatedUser().Build();
});
// -------------------------------------------------------------------------------------------
// CORS Settings --------------------------------------------------------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost" , "http://192.168.0.103").AllowAnyHeader().AllowAnyMethod().AllowCredentials(); 
    });
});
//  --------------------------------------------------------------------------------

// AutoMapper ------------------------------------------------------------------------------------
builder.Services.AddAutoMapper(typeof (UserProfile));
builder.Services.AddScoped<IAuthService ,  AuthService>();
// Identity Settings -----------------------------------------------------------------------------------------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();


builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    
    options.Password.RequireNonAlphanumeric = false;

    // BruteForce 
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan= TimeSpan.FromMinutes(5);
    options.Lockout.AllowedForNewUsers = true;
});
// -------------------------------------------------------------------------------------------------------------
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var roles = new [] {"Admin" , "User" , "Creator"};
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }
}

app.UseRouting();
app.UseCors("AllowAll");
//app.UseHttpsRedirection();



app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
