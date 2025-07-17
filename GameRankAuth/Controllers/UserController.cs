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
        private readonly ILogger<AuthController> _logger;
        public UserController(ApplicationDbContext context , UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager ,  ILogger<AuthController> logger)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpGet("usershow")]
        [Authorize]
        
        public async Task< IActionResult> ShowProfileData()
        {
            
            var getUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            
            if (email == null || username==null || getUserId ==null)
            {
                
                return Conflict(new {Message = "Данные профиля не были загружены "});
            }
            else
            {
                return Ok(new
                {
                    UserName = username,
                    Email = email,
                    Role = role

                });
            }
           
        }

        
        [HttpPost("signout")]
        [Authorize] 
        
        public async Task<IActionResult> SignOut()
        {
            try
            {
                Response.Cookies.Delete("myToken");
            }
            catch (Exception ex)
            {
                    _logger.LogError("Ошибка удаления JWT токена . Требуется Debug");
                return Conflict(new { Message = "Возникла ошибка сервера , попробуйте позже"});
            }

            return Ok(new { RedirectUrl = "/Profile.html" });
        }
    }
}
