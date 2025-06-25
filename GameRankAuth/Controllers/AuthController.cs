using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using GameRankAuth.Models;
namespace GameRankAuth.Controllers
{
    [ApiController]

    [Route("api/auth")]
    public class AuthController:ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;

        public AuthController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            try
            {
                if (!string.IsNullOrEmpty(user.UserName) && !string.IsNullOrEmpty(user.Email))
                {
                    Console.Write(user.UserName);
                    Console.Write(user.password);
                    var UserWithSameName = await _userManager.FindByNameAsync(user.UserName);
                    var UserWithSameEmail = await _userManager.FindByEmailAsync(user.Email);

                    if (UserWithSameName != null)
                    
                        return Conflict(new { Message = "Пользователь с таким именем уже зарегистрирован" });
                    
                    if (UserWithSameEmail != null)
                    
                        return Conflict(new { Message = "Пользователь с такой почтой уже зарегистрирован" });
                    
                    else
                    {
                        

                        // return RedirectToAction("EmailConfirm"); - Email reg -
                        var account = new IdentityUser { UserName = user.UserName, Email = user.Email };
                        var result = await _userManager.CreateAsync(account, user.password);

                        if (result.Succeeded)
                        {
                            return Ok(new { redirectUrl = "/Profile.html" });
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
    }
}

