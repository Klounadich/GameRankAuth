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
                            message = "Имя пользователя не должно быть короче 3-х символов"
                        });

                    bool hasSymInPass = user.password.Any(char.IsLetter);
                    if (user.password.Length < 9 || hasSymInPass == false)
                        return Conflict(new
                        {
                            success = false,
                            type = "password",
                            message = "Длина пароля должна быть не меньше 9 символов и содержать хотя бы 1 букву"
                        }
                        );

                    else
                    {


                        
                        var account = new IdentityUser { UserName = user.UserName, Email = user.Email };
                        var result = await _userManager.CreateAsync(account, user.password);
                        var checkUser = await _userManager.FindByNameAsync(user.UserName);
                        if (result.Succeeded)
                        {
                            Console.WriteLine("assssssssssssssssss");
                            var token = _jwtTokenService.GenerateToken(account);
                            
                            
                            var result1 = await _signInManager.PasswordSignInAsync(checkUser, user.password, isPersistent: true, lockoutOnFailure: false);
                            if (result.Succeeded)
                            {
                                Console.WriteLine("baul");
                                return Ok(new {Message= token });
                            }
                            else
                                return Ok(new { Message = token });
                        }
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }


        
        //public async Task<IActionResult> EmailConfirm

        [HttpPost("authoriz")]
        [AllowAnonymous]
        public async Task<IActionResult> Authorization([FromBody] User user)
        {
            try
            {
                if (!string.IsNullOrEmpty(user.UserName) && !string.IsNullOrEmpty(user.password))
                {
                    var checkUser = await _userManager.FindByNameAsync(user.UserName);
                    if (checkUser != null)
                    {
                        var result = await _signInManager.PasswordSignInAsync(checkUser, user.password, isPersistent: true, lockoutOnFailure: false);
                        if (result.Succeeded)
                        {
                                                       
                                return Ok(new
                                {
                                    RedirectUrl = "/Profile.html",
                                    UserName = checkUser.UserName
                                });

                            
                           
                            
                            
                        }
                        else
                            Console.WriteLine(checkUser);
                        Console.Write(result);
                            return BadRequest(new { Message="НЕ РЕГАЕТСЯ"});
                    }
                    else
                    {
                        return Conflict();
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

// ######   ###  ###          ### ###  ####      #####   ##   ##  ##   ##    ###    #####     ######    ####    ##  ##
//  ##  ##   ##  ##            ## ##    ##      ### ###  ##   ##  ###  ##   ## ##    ## ##      ##     ##  ##   ##  ##
//  ##  ##    ####             ####     ##      ##   ##  ##   ##  #### ##  ##   ##   ##  ##     ##    ##        ##  ##
//  #####      ##              ###      ##      ##   ##  ##   ##  #######  ##   ##   ##  ##     ##    ##        ######
//  ##  ##     ##              ####     ##      ##   ##  ##   ##  ## ####  #######   ##  ##     ##    ##        ##  ##
//  ##  ##     ##              ## ##    ##  ##  ### ###  ##   ##  ##  ###  ##   ##   ## ##      ##     ##  ##   ##  ##
// ######     ####            ### ###  #######   #####    #####   ##   ##  ##   ##  #####     ######    ####    ##  ##

