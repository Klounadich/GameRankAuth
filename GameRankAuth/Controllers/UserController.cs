using GameRankAuth.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GameRankAuth.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        public UserController(ApplicationDbContext context , UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet("usershow")]
        [Authorize]
        
        public async Task< IActionResult> ShowProfileData()
        {
            
            var getUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            
            var email = User.FindFirst(ClaimTypes.Email)?.Value; 
            
            if (email == null || username==null || getUserId ==null)
            {
                
                return Conflict();
            }
            else
            {
                return Ok(new
                {
                    UserName = username,
                    Email = email

                });
            }
           
        }

        
        [HttpPost("signout")]
        [Authorize] 
        
        public async Task<IActionResult> SignOut()
        {
             Response.Cookies.Delete("myToken");
             
            return Ok(new { RedirectUrl = "/Profile.html" });
        }
    }
}
