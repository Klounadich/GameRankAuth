using GameRankAuth.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using GameRankAuth.Interfaces;
using GameRankAuth.Models;
using GameRankAuth.Modules;
using GameRankAuth.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;

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
        private readonly IChangeUserDataService _changeUserDataService;
        private const string JWTToken = "myToken";

        public UserController(ApplicationDbContext context, UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager, ILogger<AuthController> logger,
            IChangeUserDataService changeUserDataService)
        {
            _changeUserDataService = changeUserDataService;
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpPost("change-username")]
        [Authorize]
        public async Task<IActionResult> ChangeUsername([FromBody] string UserName)
        {
            var validator = new ChangeUserNameValidator();

            var validresult = validator.Validate(UserName);

            string Id = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!validresult.IsValid)
            {
                foreach (var error in validresult.Errors)
                {
                    var firsterror = validresult.Errors.First().ErrorMessage;
                    return BadRequest(new { Message = firsterror });
                }

            }

            var result = await _changeUserDataService.ChangeUserNameAsync(Id, UserName);
            if (result.Success)
            {
                var token = result.Token;
                if (token != null)
                {
                    HttpContext.Response.SetCookie(token);
                    return Ok(new { Message = "Имя пользователя успешно изменено" });
                }
                else
                {
                    return BadRequest(new { Message = "Ошибка сервера , попробуйте позже" });
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


            var validator = new ChangePasswordValidator();

            var validresult = validator.Validate(request);

            string Id = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!validresult.IsValid)
            {
                foreach (var error in validresult.Errors)
                {
                    var firsterror = validresult.Errors.First().ErrorMessage;
                    return BadRequest(new { Message = firsterror });
                }
            }

            var result = await _changeUserDataService.ChangePasswordAsync(Id, request);
            if (result.Success)
            {

                return Ok(new { Message = "Пароль успешно изменен" });


            }
            else
            {
                return BadRequest(new { Message = result.Errors });
            }
        }


        [HttpPost("change-description")]
        [Authorize]
        public async Task<IActionResult> ChangeDescription([FromBody] string Description)
        {
            var validator = new DescriptionValidator();
            var validresult = validator.Validate(Description);
            if (!validresult.IsValid)
            {
                foreach (var error in validresult.Errors)
                {
                    var firsterror = validresult.Errors.First().ErrorMessage;
                    return BadRequest(new { Message = firsterror });
                }
            }
            string Id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result= await _changeUserDataService.ChangeDescriptionAsync(Id, Description);
            if (result.Success)
            {
                return Ok(new { Message = "Описание успешно изменено" });
            }
            return BadRequest(new { Message = result.Errors });
        }


        [HttpPost("change-email")]
        [Authorize]
        public async Task<IActionResult> ChangeEmail([FromBody] string Email)
        {
            var validator = new ChangeEmailValidator();

            var validresult = validator.Validate(Email);

            string Id = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!validresult.IsValid)
            {
                foreach (var error in validresult.Errors)
                {
                    var firsterror = validresult.Errors.First().ErrorMessage;
                    return BadRequest(new { Message = firsterror });
                }
            }

            var result = await _changeUserDataService.ChangeEmailAsync(Id, Email);
            if (result.Success)
            {
                var token = result.Token;
                if (token != null)
                {
                    HttpContext.Response.SetCookie(token);
                    return Ok(new { Message = "Email успешно изменен" });
                }

                return BadRequest(new { Message = "Ошибка сервера , попробуйте позже" });
            }
            else
            {
                return BadRequest(new { Message = result.Errors });
            }
        }


        [HttpGet("usershow")]
        [Authorize]

        public async Task<IActionResult> ShowProfileData()
        {

            var getUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var user = await _context.Users.Select(x => new
            {
                x.UserName,

                x.Id,
                x.Email,
                x.EmailConfirmed
            }).FirstOrDefaultAsync(x => x.Id == getUserId);
            var Description = _context.UsersDescription.Where(x => x.Id == getUserId).Select(x => x.Description).FirstOrDefault();

            var userForRole = await _userManager.FindByIdAsync(getUserId);
            var role =  await _userManager.GetRolesAsync(userForRole);  

            if (user == null && role == null)
            {

                return BadRequest(new { Message = "Данные профиля не были загружены " });
            }
            else
            {
                return Ok(new
                {
                    UserName = user.UserName,
                    Email = user.Email,
                    Role = role,
                    EmailVerified = user.EmailConfirmed,
                    Description = Description

                });
            }

        }

        [HttpGet("checkaccess")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
        public async Task<IActionResult> DeleteAccount()
        {
            _logger.LogInformation("Попытка удалить аккаунт");
            var id =  User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _changeUserDataService.DeleteAsync(id);
            if (result.Success)
            {
                return Ok(new { Message = "Пользователь успешно удалён" });
            }
            return BadRequest();
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

        [HttpPost("set-avatar")]
        [Authorize]
        public async Task<IActionResult> SetAvatar(IFormFile file)
        {
            return Ok(new { Message = "<UNK> <UNK> <UNK>" });
        }
    }
}
