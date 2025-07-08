using Microsoft.AspNetCore.Identity;

namespace GameRankAuth.Models
{
    public class UserDto : IdentityUser
    {
        public string password { get; set; }
    }
}
