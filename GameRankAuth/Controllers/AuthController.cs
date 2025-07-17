using GameRankAuth.Data;
using GameRankAuth.Interfaces;
using GameRankAuth.Services;
using GameRankAuth.Models;
using GameRankAuth.Modules;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GameRankAuth.Controllers
{
    [ApiController]

    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly JWTTokenService _jwtTokenService;
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;
        private const string JWTToken = "myToken";
        private readonly ILogger<AuthController> _logger;

        public AuthController(ApplicationDbContext context, UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager, JWTTokenService jWTToken, IAuthService authService , ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenService = jWTToken;
            _context = context;
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest user)
        {
            try
            {
                var validator = new RegisterValidator();
                
                var validresult=validator.Validate(user);
                if (!validresult.IsValid)
                {
                    foreach (var error in validresult.Errors)
                    {
                        ModelState.AddModelError(error.ErrorCode, error.ErrorMessage);
                    }
                    return Conflict(new {Message = ModelState});
                }
                _logger.LogInformation("Регистрация");
                var result = _authService.RegisterAsync(user);


                if (result.Result.Success)
                {

                    var token = result.Result.Token;
                    if (token != null)
                    {
                        HttpContext.Response.Cookies.Append(JWTToken, token, new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = false,
                            SameSite = SameSiteMode.Lax,
                            Expires = DateTimeOffset.UtcNow.AddDays(1)
                        });
                        return Ok(new { Message = "Успешная регистрация" });
                    }
                    else
                        return Conflict(new { Message = "Ошибка регистрации. Попробуйте позже" });
                }
                else
                {
                    return Conflict(result.Result.Errors);
                }

                
                    
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(new { Message = "Ошибка сервера , попробуйте позже" });
            }
            
        }










        [HttpPost("authoriz")]
        [AllowAnonymous]
        public async Task<IActionResult> Authorization([FromBody] LoginRequest user)
        {
            _logger.LogTrace("Авторизация");
            try
            {
                var result = _authService.LogInAsync(user);
                if (result.Result.Success)
                {
                    var token = result.Result.Token;

                    if (token != null)
                    {
                        HttpContext.Response.Cookies.Append(JWTToken, token, new CookieOptions
                        {
                            HttpOnly = true,
                            SameSite = SameSiteMode.Lax,
                            Secure = false,
                            Expires = DateTime.Now.AddDays(1)
                        });
                        return Ok(new { Message = "Успешная Авторизация" });
                    }
                    else 
                    {

                        return Conflict(new { Message = "Ошибка авторизации , попробуйте позже" });
                    }

                    
                }
                else
                {
                    return Conflict(result.Result.Errors);
                }
                
                
            }
            
            catch (Exception)
            {
                return Conflict(new { Message = "Ошибка авторизации , попробуйте позже" });
            }

            
        }



    }
}



