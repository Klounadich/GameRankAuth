using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using GameRankAuth.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.VisualBasic;
using GameRankAuth.Services;
namespace GameRankAuth.Controllers
{
    [ApiController]

    [Route("api/auth")]
    public class AuthController:ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly JWTTokenService _jwtTokenService;

        public AuthController(UserManager<IdentityUser> userManager , SignInManager<IdentityUser> signInManager , JWTTokenService jWTToken)
        {
            _userManager = userManager;
            _signInManager= signInManager;
            _jwtTokenService = jWTToken;
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

                    if (UserWithSameName != null || UserWithSameEmail != null)
                    
                        return Conflict(new {success= false, type="username",
                        message="Пользователь с таким именем или почтой уже зарегистрирован",
                        field="username"});

                    if (user.UserName.Length < 3)
                        return Conflict(new
                        {
                            success = false,
                            type = "username",
                            message = "Имя пользователя не должно быть короче 3-х символов",
                            field="username"
                        });

                    bool hasSymInPass = user.password.Any(char.IsLetter);
                    if (user.password.Length < 9 || hasSymInPass == false)
                        return Conflict(new
                        {
                            success = false,
                            type = "password",
                            message = "Длина пароля должна быть не меньше 9 символов и содержать хотя бы 1 букву",
                            field="password"
                        }
                        );

                    else
                    {
                        var account = new IdentityUser { UserName = user.UserName, Email = user.Email };
                        var result = await _userManager.CreateAsync(account, user.password);
                        var checkUser = await _userManager.FindByNameAsync(user.UserName);
                        if (result.Succeeded)
                        {
                            
                            var token = _jwtTokenService.GenerateToken(account);
                            if (token != null)
                            {
                                HttpContext.Response.Cookies.Append("myToken", token, new CookieOptions
                                {
                                    HttpOnly=true,
                                    Secure=false,
                                    SameSite=SameSiteMode.Lax,
                                    Expires=DateTimeOffset.UtcNow.AddDays(1)
                                });
                                return Ok();
                            }
                            else
                                return Conflict(new { Message = "There was an error creating your account, please try later" });
                        }
                        else
                            return Conflict(new { Message = "There was an error creating your account, please try later" });
                    }
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
                    
                    var checkPass =  _userManager.PasswordHasher.VerifyHashedPassword(checkUser, checkUser.PasswordHash,user.password);
                    
                    
                    if (checkUser != null && checkPass !=null)
                    {
                        var getEmail = await _userManager.GetEmailAsync(user);
                        var account = new IdentityUser { UserName=user.UserName , Email=getEmail};
                        if (getEmail != null)
                        {
                            var token = _jwtTokenService.GenerateToken(account);

                            if (token != null) 
                            {
                                HttpContext.Response.Cookies.Append("myToken", token, new CookieOptions
                                {
                                    HttpOnly = true,
                                    SameSite = SameSiteMode.Lax,
                                    Secure = false,
                                    Expires = DateTime.Now.AddDays(1)
                                });
                                return Ok();
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



