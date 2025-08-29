using GameRankAuth.Models;
using Microsoft.AspNetCore.Identity;

namespace GameRankAuth.Interfaces;

public interface IChangeUserDataService
{
    Task<UserData.UserResult> ChangeUserNameAsync(string Id,string UserName);
    Task<UserData.UserResult> ChangePasswordAsync(string Id,UserData.ChangePasswordRequest request);
    Task<UserData.UserResult> ChangeEmailAsync( string Id,string Email);
    Task<UserData.UserResult> ChangeDescriptionAsync( string Id,string Description);
    Task<UserData.UserResult> ChangeSocialLinksAsync( string Id,UserData.SocialLinksReq request);
    
    Task<UserData.UserResult> DeleteAsync( string Id);
}