using GameRankAuth.Data;
using GameRankAuth.Interfaces;
using GameRankAuth.Models;
using Microsoft.AspNetCore.Identity;

namespace GameRankAuth.Services;

public class ChangeUserDataService: IChangeUserDataService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly JWTTokenService _jwtTokenService;
    private readonly ApplicationDbContext _context;
    private readonly AdminPanelDBContext _adminPanelDBContext;
    public ChangeUserDataService(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager ,
        JWTTokenService jwtTokenService , ApplicationDbContext context , AdminPanelDBContext adminPanelDBContext)
    {
        _adminPanelDBContext = adminPanelDBContext;
        _context=context;
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

    public async Task<UserData.UserResult> ChangeDescriptionAsync(string userId, string Description)
    {
        var check = await _context.UsersDescription.FindAsync(userId);
        if (check == null)
        {
            var newDescription = new UserData.UserDescription
            {
                Id = userId,
                Description = Description
            };
            _context.UsersDescription.Add(newDescription);
            await _context.SaveChangesAsync();
            return new UserData.UserResult
            {
                Success = true,

            };
        }
        else
        {
            var GetOldDescription = _context.UsersDescription.FirstOrDefault(x => x.Id == userId);
           GetOldDescription.Description = Description;
            await _context.SaveChangesAsync();
            return new UserData.UserResult
            {
                Success = true,

            };
        }
    }

    public async Task<UserData.UserResult> ChangeSocialLinksAsync(string userId, UserData.SocialLinks request)
    {
        var check = await _context.UsersSocialLinks.FindAsync(userId);
        if (check == null)
        {
            var addLinks = new UserData.SocialLinks
            {
                Id = userId,
                GithubLink = request.GithubLink,
                SteamLink = request.SteamLink,
                RedditLink = request.RedditLink,
            };
            _context.UsersSocialLinks.Add(addLinks);
            await _context.SaveChangesAsync();
            return new UserData.UserResult
            {
                Success = true,
            };
        }
        else
        {
            if (request.GithubLink != null)
            check.GithubLink = request.GithubLink;
            
            if (request.SteamLink != null)
                check.SteamLink = request.SteamLink;
            
            if (request.RedditLink != null)
                check.RedditLink = request.RedditLink;
        
            _context.UsersSocialLinks.Update(check);
            await _context.SaveChangesAsync();
        
            return new UserData.UserResult
            {
                Success = true,
            };
        }
    }

    public async Task<UserData.UserResult> DeleteAsync(string Id)
    {
        if (string.IsNullOrEmpty(Id))
        {
            return new UserData.UserResult { Success = false, Errors = new[] { "Id не может быть пустым" } };
        }

       
        var user = await _userManager.FindByIdAsync(Id);
        if (user == null)
        {
            return new UserData.UserResult { Success = false, Errors = new []{"Пользователь не найден"} };
        }

        var delResult = await _userManager.DeleteAsync(user);
        

        
        try
        {
            var userData = await _adminPanelDBContext.UserDataAdmin.FindAsync(Id);
            if (userData != null)
            {
                _adminPanelDBContext.UserDataAdmin.Remove(userData);
            }

            var suspectUser = await _adminPanelDBContext.SuspectUsers.FindAsync(Id);
            if (suspectUser != null)
            {
                _adminPanelDBContext.SuspectUsers.Remove(suspectUser);
            }

            await _adminPanelDBContext.SaveChangesAsync();
            return new UserData.UserResult
            {
                Success = true
            };
        }
        catch (Exception ex)
        {
            return new UserData.UserResult { Success = false,
                Errors = new []{ex.Message} };
        }
    }
}