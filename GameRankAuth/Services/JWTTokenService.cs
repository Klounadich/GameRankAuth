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
        
        private readonly AuthSettings _options;
        public JWTTokenService( AuthSettings options)
        {
            
            _options = options;
        }

        
        public string GenerateToken(IdentityUser user)
        {

            Console.WriteLine($"Expires = {_options.Expires}");
            var claims = new List<Claim>
            {
                new Claim("UserName", user.UserName),
                new Claim("Email", user.Email),
                new Claim("id", user.Id.ToString())
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
