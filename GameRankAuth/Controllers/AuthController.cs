using GameRankAuth.Data;
using GameRankAuth.Interfaces;
using GameRankAuth.Services;
using GameRankAuth.Models;
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

        public AuthController(ApplicationDbContext context, UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager, JWTTokenService jWTToken, IAuthService authService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenService = jWTToken;
            _context = context;
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest user)
        {
            try
            {
                
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



