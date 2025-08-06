using GameRankAuth.Data;
using GameRankAuth.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GameRankAuth.Models;
namespace GameRankAuth.Services
{
    public class AuthService:IAuthService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly JWTTokenService _jwtTokenService;
        private readonly ILogger<AuthService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AdminPanelDBContext _adminPanelDBContext;
        public AuthService(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager , ApplicationDbContext context , IHttpContextAccessor accessor, 
            JWTTokenService jwtTokenService , ILogger<AuthService> logger , AdminPanelDBContext adminPanelDBContext)
        {
            _adminPanelDBContext = adminPanelDBContext;
            _jwtTokenService = jwtTokenService;
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _httpContextAccessor = accessor;
        }
         public  async Task<AuthResult> RegisterAsync(RegisterRequest request)
         {
             if (!String.IsNullOrEmpty(request.UserName)&& !String.IsNullOrEmpty(request.Password) &&!String.IsNullOrEmpty(request.Email))
             {
                 var SameUser = await _context.Users.AnyAsync(u => u.UserName == request.UserName || u.Email == request.Email);
                 if (SameUser != false)
                 {
                     _logger.LogError("Пользователь с таким именем или почтой уже зарегистрирован");
                     return new AuthResult
                     {
                         Success = false,
                         Errors = new []{"Пользователь с таким именем или почтой уже зарегистрирован"}
                         
                     };  
                 }
                 else
                 {
                     _logger.LogInformation("Создание IdentityUser");
                     var user = new IdentityUser
                     {
                         UserName = request.UserName,
                         Email = request.Email,
                         

                     };
                     var result = await _userManager.CreateAsync(user, request.Password);
                     _logger.LogInformation("Создание аккаунта ");
                     if (result.Succeeded)
                     {
                         await _userManager.AddToRoleAsync(user, "User");
                         
                         _logger.LogInformation("Аккаунт создан , генерация JWT токена");
                         
                         var token = _jwtTokenService.GenerateToken(user);
                         if (token != null)
                         {
                             _logger.LogInformation("JWT токен создан , отправка результата");
                             return new AuthResult
                             {
                                 Success = true,
                                 Token = token,
                             };
                         }


                     }
                     else
                     {
                         _logger.LogError($"Не удалось зарегистрировать аккаунт. Были переданы данные:{user.UserName} , {request.Password}");
                         return new AuthResult
                         {
                             Success = false,
                             Errors = new[] { "Ошибка при создании аккаунта" }
                         };
                         
                     }
                 }
             }
             _logger.LogError("Ошибка сервиса. Требуется Debug");
             return new AuthResult
             {
                 Success = false,
                 Errors = new[] {"Произошла ошибка системы , попробуйте позже"}
             };
         }

         public async Task<AuthResult> LogInAsync(LoginRequest request)
         {
             if (!string.IsNullOrEmpty(request.Username) && !string.IsNullOrEmpty(request.Password))
             {
                 _logger.LogInformation("Попытка Авторизации пользователя");
                 var result = await  _signInManager.PasswordSignInAsync(request.Username, request.Password, false, true);
                 if (result.IsLockedOut)
                 {
                     var context = _httpContextAccessor.HttpContext;
                     var ip = context.Connection.RemoteIpAddress.ToString();
                     _logger.LogWarning($"Попытка BruteForce. IP атакующего {ip}");
                     SuspectUsers suspectUsers = new SuspectUsers
                     {
                        Id = "non-authorized",
                        IpAdress = ip,
                        cause = "Попытка BruteForce",
                        UserName = "Guest"
                     };
                     _adminPanelDBContext.Add(suspectUsers);
                     _adminPanelDBContext.SaveChanges();
                    
                     return new AuthResult
                     {
                         Success = false,
                         Errors = new[]{ "Вы совершили слишком много попыток , в целях безопасности , доступ к авторизации был заблокирован на 5 минут" }
                     };
                 }

                 if (result.Succeeded)
                 {
                     
                     _logger.LogInformation("Пользователь авторизован , создание JWT токена");
                     var getUser = await _userManager.FindByNameAsync(request.Username);
                     var user = new IdentityUser
                     {
                         UserName = request.Username,
                         Email = getUser.Email,
                     };
                     var getEmail =  await _userManager.GetEmailAsync(user);
                     if (getEmail != null)
                     {
                         var token = _jwtTokenService.GenerateToken(getUser);
                         if (token != null)
                         {
                             _logger.LogInformation("Токен создан . Отправка результата");
                             return new AuthResult
                             {
                                 Success = true,
                                 Token = token,
                             };
                         }
                         else
                         { 
                             _logger.LogError("Ошибка создания токена . Возможен Debug");
                             return new AuthResult
                             {
                                 Success = false,
                                 Errors = new[] { "Ошибка авторизации , попробуйте позже" }
                             };
                         }
                     }
                     else
                     {
                         _logger.LogError("Ошибка запроса в Базу Данных . Требуется Debug");
                         return new AuthResult
                         {
                             Success = false,
                             Errors = new[] { "Ошибка авторизации , попробуйте позже" }
                         };
                     }
                 }
                 else
                 {
                     _logger.LogError("Неправильное имя или пароль");
                     return new AuthResult
                     {
                         Success = false,
                         Errors = new[]{ "Неправильное имя или пароль " }
                     };
                 }
             }
             else
             {
                 _logger.LogError("Ошибка сервиса. Требуется Debug");
                 return new AuthResult
                 {
                     Success = false,
                     Errors = ["Произошла ошибка системы , попробуйте позже"]
                 };
             }
         }

    }

    
}
