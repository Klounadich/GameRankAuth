using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using GameRankAuth.Models;
using Microsoft.AspNetCore.Authorization;
namespace GameRankAuth.Controllers
{
    [ApiController]

    [Route("api/auth")]
    public class AuthController:ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AuthController(UserManager<IdentityUser> userManager , SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager= signInManager;
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


                        // return RedirectToAction("EmailConfirm"); - Email reg -
                        var account = new IdentityUser { UserName = user.UserName, Email = user.Email };
                        var result = await _userManager.CreateAsync(account, user.password);

                        if (result.Succeeded)
                        {

                            var checkUser = await _userManager.FindByNameAsync(user.UserName);
                            var result1 = await _signInManager.PasswordSignInAsync(checkUser, user.password, isPersistent: true, lockoutOnFailure: false);
                            if (result.Succeeded)
                            {
                                return Ok(new { RedirectUrl = "/Profile.html" });
                            }
                            else
                                return BadRequest();
                        }
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
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
                            Console.WriteLine("success!!!");
                            return Ok(new { RedirectUrl = "/Profile.html" });
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

