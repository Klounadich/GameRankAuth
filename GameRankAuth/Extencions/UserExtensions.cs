using GameRankAuth.Models;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Identity;

namespace GameRankAuth.Extencions
{
    public static class UserExtensions
    {
        public static UserDto ToDto ( this IdentityUser user)
        {
            return new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,

            };
        }
    }
}
