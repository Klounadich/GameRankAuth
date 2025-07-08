using GameRankAuth.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace GameRankAuth.Services
{

    public class JWTTokenService
    
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AuthSettings _options;
        public JWTTokenService( AuthSettings options ,UserManager<IdentityUser> userManager )
        {
            _userManager = userManager;
            _options = options;
        }

        
        public string GenerateToken(IdentityUser user )
        {

           
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };           

            var token = new JwtSecurityToken(
                expires: DateTime.UtcNow.Add(_options.Expires),
                claims: claims,
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey)),
                SecurityAlgorithms.HmacSha256)
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
