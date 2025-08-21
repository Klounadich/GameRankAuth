using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GameRankAuth.Interfaces;
using GameRankAuth.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GameRankAuth.Services;

public class NotBannedRequirment : IAuthorizationRequirement
{
    
}

public class NotBannedHandler : AuthorizationHandler<NotBannedRequirment> 
{
    private readonly UserManager<IdentityUser> _userService;
    private readonly AdminPanelDBContext _adminPanelDBContext;

    public NotBannedHandler( UserManager<IdentityUser> userService ,  AdminPanelDBContext adminPanelDBContext)
    {
        _adminPanelDBContext = adminPanelDBContext;
        _userService = userService;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        NotBannedRequirment requirement)
    {
        if (!context.User.Identity.IsAuthenticated)
        {
            context.Fail();
            return;
        }
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            context.Fail();
            return;
        }
        var user = await _userService.FindByIdAsync(userId);
        var userstatus = await _adminPanelDBContext.UserDataAdmin.Where(x => x.Id == user.Id).Select(x => x.Status)
            .FirstOrDefaultAsync();

        if (user != null && userstatus != "banned")
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }

    }
}