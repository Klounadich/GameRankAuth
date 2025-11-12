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
    private readonly ILogger<VerifyEmailService> _logger;
    
    public VerifyEmailService(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager , JWTTokenService jwtTokenService , ILogger<VerifyEmailService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }
    public async Task<AuthResult> VerifyEmailAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var result = await _userManager.ConfirmEmailAsync(user, token );
        if (result.Succeeded)
        {
            var updToken =  _jwtTokenService.GenerateToken(user);
            _logger.LogInformation($"Email confirmed ::: ");
            return new AuthResult
            {
                Success = true,
                Token = updToken,

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