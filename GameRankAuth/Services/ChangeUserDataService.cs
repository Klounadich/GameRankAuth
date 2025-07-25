using GameRankAuth.Interfaces;
using GameRankAuth.Models;
using Microsoft.AspNetCore.Identity;

namespace GameRankAuth.Services;

public class ChangeUserDataService: IChangeUserDataService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly JWTTokenService _jwtTokenService;
    public ChangeUserDataService(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager , JWTTokenService jwtTokenService)
    {
        _jwtTokenService =  jwtTokenService;
        _userManager = userManager;
        _signInManager = signInManager;
    }
    public async Task<UserData.UserResult> ChangeUserNameAsync(string Id,string UserName)
    {
        var CheckSameName = await _userManager.FindByNameAsync(UserName);
        if (CheckSameName == null)
        {
            
            var GetUser = await _userManager.FindByIdAsync(Id);
            var result =await _userManager.SetUserNameAsync(GetUser,UserName);
            if (result.Succeeded)
            {
               var token = _jwtTokenService.GenerateToken(GetUser);
               return new UserData.UserResult
               {
                   Success = true,
                   Token = token,
               };
            }
            else
            {
                return new UserData.UserResult
                {
                    Success = false,
                    Errors = new[] { "Ошибка сервера , попробуйте позже" }
                };
            }
        }
        else
        {
            return new UserData.UserResult
            {
                Success = false,
                Errors = new[] { "Пользователь с таким именем уже существует" }
            };
        }
    }

    public async Task<UserData.UserResult> ChangePasswordAsync(string userId ,UserData.ChangePasswordRequest request)
    {
        var GetUser = await _userManager.FindByIdAsync(userId);
        if (GetUser != null)
        {
            var result =await _userManager.ChangePasswordAsync(GetUser,request.OldPassword,request.NewPassword);
            if (result.Succeeded)
            {
                return new UserData.UserResult
                {
                    Success = true
                };
            }
            else
            {
                return new UserData.UserResult
                {
                    Success = false,
                    Errors = new[] { "Нынешний пароль неверен" }
                };
            }
            
        }
        else
        {
            return new UserData.UserResult
            {
                Success = false,
                Errors = new[] { "Ошибка сервера , попробуйте позже" }
            };
        }
    }

    public async Task<UserData.UserResult> ChangeEmailAsync(string userId,string Email)
    {
        var GetUser = await _userManager.FindByIdAsync(userId);
        if (GetUser != null)
        {
            var result =  await _userManager.SetEmailAsync(GetUser,Email );
            if (result.Succeeded)
            {
                var token = _jwtTokenService.GenerateToken(GetUser);
                return new UserData.UserResult
                {
                    Success = true,
                    Token = token,
                };
            }
            else
            {
                return new UserData.UserResult
                {
                    Success = false,
                    Errors = new[] { "Ошибка сервера , попробуйте позже" }
                };
            }
        }
        else
        {
            return new UserData.UserResult
            {
                Success = false,
                Errors = new[] { "Ошибка сервера , попробуйте позже" }
            };
        }
    }
}