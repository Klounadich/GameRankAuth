using GameRankAuth.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using GameRankAuth.Interfaces;
using GameRankAuth.Models;
using GameRankAuth.Services;

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
        private readonly IChangeUserDataService  _changeUserDataService;
        private const string JWTToken = "myToken";
        public UserController(ApplicationDbContext context , UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager ,  ILogger<AuthController> logger , IChangeUserDataService changeUserDataService)
        {
            _changeUserDataService = changeUserDataService;
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpPost("change-username")]
        [Authorize]
        public async Task<IActionResult> ChangeUsername([FromBody]  string UserName)
        {
            string Id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            
            var result = await _changeUserDataService.ChangeUserNameAsync(Id, UserName);
            if (result.Success)
            {
                var token = result.Token;
                if (token != null)
                {
                    HttpContext.Response.Cookies.Append(JWTToken, token, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = false,
                        SameSite = SameSiteMode.Lax,
                        Expires = DateTimeOffset.UtcNow.AddDays(1)
                    });
                    return Ok(new { Message = "Имя пользователя успешно изменено" });
                }
                else
                {
                    return Conflict(new { Message = "Ошибка сервера , попробуйте позже" });
                }
            }
            else
            {
                return Conflict(new { Message = result.Errors });
            }
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] UserData.ChangePasswordRequest request)
        {
            string Id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _changeUserDataService.ChangePasswordAsync(Id, request);
            if (result.Success)
            {
                
                    return Ok(new { Message = "Пароль успешно изменен" });
                
                
            }
            else 
            {
                return Conflict(new { Message = result.Errors });
            }
        }

        [HttpPost("change-email")]
        [Authorize]
        public async Task<IActionResult> ChangeEmail([FromBody] string Email)
        {
            string Id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _changeUserDataService.ChangeEmailAsync(Id, Email);
            if (result.Success)
            {
                var token = result.Token;
                if (token != null)
                {
                    HttpContext.Response.Cookies.Append(JWTToken, token, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = false,
                        SameSite = SameSiteMode.Lax,
                        Expires = DateTimeOffset.UtcNow.AddDays(1)
                    });
                    return Ok(new { Message = "Email успешно изменен" });
                }
                return Conflict(new { Message = "Ошибка сервера , попробуйте позже" });
            }
            else 
            {
                return Conflict(new { Message = result.Errors });
            }
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
