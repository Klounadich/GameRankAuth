using GameRankAuth.Models;
using Microsoft.AspNetCore.Identity;

namespace GameRankAuth.Interfaces;

public interface IChangeUserDataService
{
    Task<UserData.UserResult> ChangeUserNameAsync(string Id,string UserName);
    Task<UserData.UserResult> ChangePasswordAsync(string Id,UserData.ChangePasswordRequest request);
    Task<UserData.UserResult> ChangeEmailAsync( string Id,string Email);
}