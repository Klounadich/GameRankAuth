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
                 var SameUser = await _context.Users
                     .Where(u => u.UserName == request.UserName || u.Email == request.Email)
                     .Select(u => new { u.UserName, u.Email })
                     .FirstOrDefaultAsync();
                 if (SameUser != null)
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
             var user = await _userManager.FindByNameAsync(request.Username);
             if (user == null)
             {
                 return new AuthResult
                 {
                     Success = false,
                     Errors = new[] { "Неправильное имя или пароль" }
                 };
             }

             var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
    
             if (result.IsLockedOut)
             {
                 // Асинхронное логирование
                 await LogSuspiciousActivityAsync(request.Username);
                 return new AuthResult { Success = false, Errors = new[] { "Аккаунт заблокирован" } };
             }

             if (result.Succeeded)
             {
                 var token = _jwtTokenService.GenerateToken(user);
                 return new AuthResult { Success = true, 
                     Token = token 
                 };
             }

             return new AuthResult { Success = false, Errors = new[] { "Неправильное имя или пароль" } };
         }

         private async Task LogSuspiciousActivityAsync(string username)
         {
             var context = _httpContextAccessor.HttpContext;
             var ip = context?.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    
             var suspectUser = new SuspectUsers
             {
                 Id = Guid.NewGuid().ToString(),
                 IpAdress = ip,
                 cause = "Попытка BruteForce",
                 Username = username
             };
    
             _adminPanelDBContext.Add(suspectUser);
             await _adminPanelDBContext.SaveChangesAsync();
         }

    }

    
}
