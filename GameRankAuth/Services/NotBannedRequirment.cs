
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using GameRankAuth.Data;
using Microsoft.Extensions.Caching.Memory;

namespace GameRankAuth.Middleware;

public class BanCheckMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public BanCheckMiddleware(RequestDelegate next, IMemoryCache cache, IServiceScopeFactory serviceScopeFactory)
    {
        _next = next;
        _cache = cache;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task InvokeAsync(HttpContext context)
{
    
    if (IsStaticFileRequest(context.Request))
    {
        await _next(context);
        return;
    }

    if (context.User?.Identity?.IsAuthenticated != true)
    {
        await _next(context);
        return;
    }

    var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userId))
    {
        await _next(context);
        return;
    }

    var cacheKey = $"UserBanStatus_{userId}";
    if (!_cache.TryGetValue(cacheKey, out bool isBanned))
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<AdminPanelDBContext>();

        var user = await userManager.FindByIdAsync(userId);
        if (user != null)
        {
            Console.WriteLine("233");
            var userStatus = await dbContext.UserDataAdmin
                .Where(x => x.Id == user.Id)
                .Select(x => x.Status)
                .FirstOrDefaultAsync();

            isBanned = userStatus == "banned";
            _cache.Set(cacheKey, isBanned, TimeSpan.FromMinutes(1));
        }
        else
        {
            isBanned = false;
        }
    }

    if (isBanned)
    {
        context.Response.StatusCode = 403;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("{\"error\":\"Account banned\"}");
        return;
    }

    await _next(context);
}

private bool IsStaticFileRequest(HttpRequest request)
{
    var staticExtensions = new[] 
    { 
        ".css", ".js", ".png", ".jpg", ".jpeg", ".gif", ".ico", 
        ".svg", ".woff", ".woff2", ".ttf", ".eot", ".map" 
    };

    return staticExtensions.Any(ext => request.Path.Value?.EndsWith(ext) == true) ||
           request.Path.StartsWithSegments("/_") ||
           request.Path.StartsWithSegments("/lib/");
}
}
