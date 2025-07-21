using GameRankAuth.Interfaces;
using GameRankAuth.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace GameRankAuth.Services;



public class VerifyEmailService  : IVerifyService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly JWTTokenService _jwtTokenService;
    
    public VerifyEmailService(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }
    public async Task<AuthResult> VerifyEmailAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var result = await _userManager.ConfirmEmailAsync(user, token );
        if (result.Succeeded)
        {
            return new AuthResult
            {
                Success = true,

            };
        }
        else
        {
            return new AuthResult
            {
                Success = false,
                Errors = new[] { "Ошибка сервера , попробуйте позже" }
            };
        }
    }
}