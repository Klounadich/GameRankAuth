using GameRankAuth.Data;
using GameRankAuth.Models;
using GameRankAuth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.VisualBasic;
namespace GameRankAuth.Controllers
{
    [ApiController]

    [Route("api/auth")]
    public class AuthController:ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly JWTTokenService _jwtTokenService;
        private readonly ApplicationDbContext _context;
        private const string JWTToken = "myToken";

        public AuthController(ApplicationDbContext context,UserManager<IdentityUser> userManager , SignInManager<IdentityUser> signInManager , JWTTokenService jWTToken)
        {
            _userManager = userManager;
            _signInManager= signInManager;
            _jwtTokenService = jWTToken;
            _context= context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            try
            {
                
                    if (!string.IsNullOrEmpty(user.UserName) && !string.IsNullOrEmpty(user.Email))
                    {

                        var UserWithSameName = await _userManager.FindByNameAsync(user.UserName);
                        var UserWithSameEmail = await _userManager.FindByEmailAsync(user.Email);

                        var SameUser = await _context.Users.AnyAsync(u => u.UserName == user.UserName || u.Email == user.Email);

                        if (SameUser != false)

                            return Conflict(new
                            {
                                success = false,
                                type = "username",
                                message = "Пользователь с таким именем или почтой уже зарегистрирован",
                                field = "username"
                            });

                        if (user.UserName.Length < 3)
                            return Conflict(new
                            {
                                success = false,
                                type = "username",
                                message = "Имя пользователя не должно быть короче 3-х символов",
                                field = "username"
                            });

                        bool hasSymInPass = user.password.Any(char.IsLetter);
                        if (user.password.Length < 9 || hasSymInPass == false)
                            return Conflict(new
                            {
                                success = false,
                                type = "password",
                                message = "Длина пароля должна быть не меньше 9 символов и содержать хотя бы 1 букву",
                                field = "password"
                            }
                            );

                        else
                        {
                            var account = new IdentityUser { UserName = user.UserName, Email = user.Email, Id = user.Id };
                            var result = await _userManager.CreateAsync(account, user.password);
                            var checkUser = await _userManager.FindByNameAsync(user.UserName);
                            if (result.Succeeded)
                            {

                                var token = _jwtTokenService.GenerateToken(checkUser);
                                if (token != null)
                                {
                                    HttpContext.Response.Cookies.Append(JWTToken, token, new CookieOptions
                                    {
                                        HttpOnly = true,
                                        Secure = false,
                                        SameSite = SameSiteMode.Lax,
                                        Expires = DateTimeOffset.UtcNow.AddDays(1)
                                    });
                                    return Ok(new { Message = "Success" });
                                }
                                else
                                    return Conflict(new { Message = "Ошибка при создании JWT токена. Попробуйте позже" });
                            }
                            else
                                return Conflict(new { Message = "Ошибка при запросе в базу данных . Попробуйте позже" });
                        }
                    }
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Message = "Внутренняя ошибка модели" });
                }

                return Conflict(new { Message = "There was an error creating your account, please try later" });
                
            }
            
            catch (Exception ex)
                {
                    return BadRequest(new { Message = ex.Message });
                }
        }


        
        

        [HttpPost("authoriz")]
        [AllowAnonymous]
        public async Task<IActionResult> Authorization([FromBody] User user)
        {
            try
            {
                if (!string.IsNullOrEmpty(user.UserName) && !string.IsNullOrEmpty(user.password))
                {
                    var checkUser = await _userManager.FindByNameAsync(user.UserName);
                    

                    
                    
                    var result = await _signInManager.PasswordSignInAsync(user.UserName, user.password, false, lockoutOnFailure: true);
                    if (result.IsLockedOut)
                    {
                        return Conflict(new { Message = "Вы совершили слишком много неправильных попыток . Доступ заблокирован . Попробуйте снова через 10 минут" });
                    }
                    if (checkUser != null && result.Succeeded)
                    {
                        var getEmail = await _userManager.GetEmailAsync(user);
                        
                        var account = new IdentityUser { UserName=user.UserName , Email=getEmail, Id = user.Id };
                        if (getEmail != null)
                        {
                            var token = _jwtTokenService.GenerateToken(checkUser);
                            
                            if (token != null) 
                            {
                                HttpContext.Response.Cookies.Append(JWTToken, token, new CookieOptions
                                {
                                    HttpOnly = true,
                                    SameSite = SameSiteMode.Lax,
                                    Secure = false,
                                    Expires = DateTime.Now.AddDays(1)
                                });
                                return Ok(new {Message = "Success"});
                            }
                            else
                            {
                                
                                return Conflict(new { Message = "There was an error during authentication, please try later" });
                            }
                        }
                        else
                        {
                            return Conflict(new { Message = "There was an error during authentication, please try later" });
                        }
                    }
                    else
                    {
                        
                        return Conflict(new
                        {
                            success = false,
                            type = "password",
                            message = "Неправильно введено имя или пароль. Попробуйте снова",
                            field = "password"
                        });
                    }
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                return BadRequest(ex.Message);
            }
            return Ok();
        }

       
    }

}



