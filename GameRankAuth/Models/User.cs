using Microsoft.AspNetCore.Identity;

namespace GameRankAuth.Models
{
    public class User : IdentityUser
    {
        public string password { get; set; }
    }
}
