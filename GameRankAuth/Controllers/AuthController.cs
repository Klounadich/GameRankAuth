using System.IdentityModel.Tokens.Jwt;
using GameRankAuth.Data;
using GameRankAuth.Interfaces;
using GameRankAuth.Services;
using GameRankAuth.Models;
using GameRankAuth.Modules;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using StackExchange.Redis;
using GameRankAuth.Services.RabbitMQ;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Distributed;

namespace GameRankAuth.Controllers
{
    
    [ApiController]

    [Route("api/auth")]
    public  class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly JWTTokenService _jwtTokenService;
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;
        private const string JWTToken = "myToken";
        private readonly ILogger<AuthController> _logger;
        private readonly IVerifyService _verifyService;
        private readonly RabbitMQService _rabbitMQService;
        private readonly IQrCodeGeneratorService _qrCodGen;
        private readonly AdminPanelDBContext _adminPanelDBContext;
        
        private readonly IDatabase _redis;
        

        public AuthController(ApplicationDbContext context, UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager, JWTTokenService jWTToken, IAuthService authService , 
            ILogger<AuthController> logger , IVerifyService verifyService , RabbitMQService rabbitMQService ,
            AdminPanelDBContext adminPanelDBContext ,  IQrCodeGeneratorService qrCodGen , IConnectionMultiplexer redis)
        {
            _redis = redis.GetDatabase();
            _qrCodGen =  qrCodGen;
            _adminPanelDBContext = adminPanelDBContext;
            _rabbitMQService = rabbitMQService;
            _verifyService = verifyService;
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
                        
                        var firsterror= validresult.Errors.First().ErrorMessage;
                        return BadRequest(new { Message = firsterror });
                    }
                    
                }
                _logger.LogInformation("Регистрация");
                var result = await _authService.RegisterAsync(user);


                if (result.Success)
                {

                    var token = result.Token;
                    if (token != null)
                    {
                        HttpContext.Response.SetCookie(token);
                        string userIp = HttpContext.Connection.RemoteIpAddress.ToString();
                        var getUser = await _userManager.FindByNameAsync(user.UserName);
                        
                        string Id = getUser.Id;
                        _logger.LogInformation(Id);
                        var userforadmin = new UsersStatus
                        {
                            Id = Id,
                            IPAdress = userIp,
                            Status = "active",
                            UserName = user.UserName,

                        };
                        _logger.LogInformation($"В админ панель добавлены данные:  {userforadmin}");
                       _adminPanelDBContext.Add(userforadmin);
                        await _adminPanelDBContext.SaveChangesAsync();
                        return Ok(new { Message = "Успешная регистрация" });
                    }
                    else
                        return BadRequest(new { Message = "Ошибка регистрации. Попробуйте позже" });
                }
                else
                {
                    return BadRequest(result.Errors);
                }

                
                    
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(new { Message = "Ошибка сервера , попробуйте позже" });
            }
            
        }

        
        [HttpPost("verify")]
        [Authorize]
        public async Task<IActionResult> VerifyEmailAsync()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _verifyService.VerifyEmailAsync(id);
            if (result.Success)
            {
                _rabbitMQService.Send();
                var token = result.Token;
                HttpContext.Response.SetCookie(token);
                return Ok(new { Message = "Почта успешно подтверждена" });
               
            }
            else
            {
                return Conflict(new { Message = "Ошибка подтверждения почты , попробуйте позже" });
            }
        }
        
        [HttpGet("check-verify")]
        [Authorize]
        public async Task<IActionResult> CheckVerifyEmailAsync()
        { 
            


            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            //Console.WriteLine(id);
            var getuser = await _userManager.FindByIdAsync(id);
            if (getuser != null)
            {
                var emailVerified = await _userManager.IsEmailConfirmedAsync(getuser);
                //Console.WriteLine(emailVerified);
                return Ok(new { getStatusEmail = emailVerified });
            }
            return BadRequest(new { Message = "<UNK> <UNK> <UNK>" });
        }






        
        [HttpPost("authoriz")]
        [AllowAnonymous]
        public async Task<IActionResult> Authorization([FromBody] LoginRequest user)
        {
            _logger.LogTrace("Авторизация");
            try
            {
                var result = await _authService.LogInAsync(user);
                if (result.Success)
                {
                    var token = result.Token;

                    if (token != null)
                    {
                        HttpContext.Response.SetCookie(token);
                        return Ok(new { Message = "Успешная Авторизация" });
                    }
                    else 
                    {

                        return BadRequest(new { Message = "Ошибка авторизации , попробуйте позже" });
                    }

                    
                }
                else
                {
                    return BadRequest(result.Errors);
                }
                
                
            }
            
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(new { Message = "Ошибка авторизации , попробуйте позже" });
            }

            
        }


        [HttpGet("qrcode-show")]
        public async Task<IActionResult> QrcodeShow()
        {
            try
            {
                
                var qrResult = await _qrCodGen.GenerateQrCodeImage();
        
                
                var base64Image = Convert.ToBase64String(qrResult.ImageData);
                var imageSrc = $"data:image/png;base64,{base64Image}";

                return Ok(new 
                {
                    success = true,
                    qrId = qrResult.QrId, 
                    token = qrResult.Token, 
                    qrCodeImage = imageSrc,
                    expiresAt = qrResult.ExpiresAt, 
                    expiresIn = 300 
                });
            }
            catch (Exception ex)
            {
               
                _logger.LogError(ex, "Ошибка генерации QR-кода");
        
                return StatusCode(500, new 
                { 
                    success = false, 
                    error = "Ошибка генерации QR-кода" 
                });
            }
        }


        [HttpPost("qrcode-confirm")]
        [Authorize]
        public async Task<IActionResult> QrcodeConfirm([FromBody]QRModel request)
        {
            var userId =  User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }
            var cacheKey = $"qr:{request.QrcodeId}";
            var session = await _redis.StringGetAsync(cacheKey);
            if (string.IsNullOrEmpty(session))
            {
                return Ok(new { Message = "Qr-Код недействителен , попробуйте снва" });
            }

            var sessiondes = JsonSerializer.Deserialize<SessionQr>(session);

            if (sessiondes.token != request.token)
            {
                return BadRequest(new { Message = "Данные не соответствуют" });
            }
            sessiondes.Status = "confirmed";
            sessiondes.userid = userId;
            sessiondes.username = User.FindFirstValue(ClaimTypes.Name);
            sessiondes.Email = User.FindFirstValue(ClaimTypes.Email);
            sessiondes.Role = User.FindFirstValue(ClaimTypes.Role);
            var updatesession = JsonSerializer.Serialize<SessionQr>(sessiondes);
            var expiry = TimeSpan.FromMinutes(5);
            await _redis.StringSetAsync(cacheKey, updatesession, expiry);
            return Ok(new { Message = "Qr-код Подтверждён" });
        }
        
        [HttpGet("qr-status/{qrcodeId}")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckQrStatus(string qrcodeId)
        { Console.WriteLine("начали автор");
            var cacheKey = $"qr:{qrcodeId}";
        
            
            var session = await _redis.StringGetAsync(cacheKey);
        
            if (session.IsNullOrEmpty)
            {
                return Ok(new { status = "expired" });
            }

            var sessionData = JsonSerializer.Deserialize<SessionQr>(session!);

            if (sessionData.Status == "confirmed")
            {
                var UserName = sessionData.username;
                var user =  await _userManager.FindByNameAsync(UserName);
                var token =  _jwtTokenService.GenerateToken(user);
                if (token != null)
                {
                    HttpContext.Response.SetCookie(token);
                    return Ok(new { Message = "Успешная авторизация" });
                }
            }

            return Ok(new { status = sessionData.Status });
        }


    }
}



