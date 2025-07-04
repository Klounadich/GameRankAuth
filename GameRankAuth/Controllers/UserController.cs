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
        //[AllowAnonymous]
        public IActionResult ShowProfileData()
        {
            
            var getUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine(getUserId);
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            Console.WriteLine(username);
            var email = User.FindFirst(ClaimTypes.Email)?.Value; 
            Console.WriteLine(email);
            if (email == null)
            {
                Console.WriteLine("НЕГПОАОЛАЛА"); // код падает сюда
                return BadRequest();
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

        [HttpGet("testauth")]
        [Authorize]
        public IActionResult test()
        {
            return Ok(new { Message = "login" });
        }
        [HttpPost("signout")]
        [Authorize] 
        //[AllowAnonymous]
        public async Task<IActionResult> SignOut()
        {
             Response.Cookies.Delete("myToken");
            Console.WriteLine("exit"); // не высвечивается это 
            return Ok(new { RedirectUrl = "/Profile.html" });
        }
    }
}
