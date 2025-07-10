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
        public AuthService(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager , ApplicationDbContext context , JWTTokenService jwtTokenService)
        {
            _jwtTokenService = jwtTokenService;
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }
         public  async Task<AuthResult> RegisterAsync(RegisterRequest request)
         {
             if (!String.IsNullOrEmpty(request.UserName)|| !String.IsNullOrEmpty(request.Password) ||!String.IsNullOrEmpty(request.Email))
             {
                 var SameUser = await _context.Users.AnyAsync(u => u.UserName == request.UserName || u.Email == request.Email);
                 if (SameUser != false)
                 {
                     return new AuthResult
                     {
                         Success = false,
                         Errors = new []{"Пользователь с таким именем или почтой уже зарегистрирован"}
                     };  
                 }
                 else
                 {
                     var user = new IdentityUser
                     {
                         UserName = request.UserName,
                         Email = request.Email,
                         

                     };
                     var result = await _userManager.CreateAsync(user, request.Password);
                     if (result.Succeeded)
                     {
                         var token = _jwtTokenService.GenerateToken(user);
                         return new AuthResult
                         {
                             Success = true,
                             Token = token,
                         };


                     }
                     else
                     {
                         return new AuthResult
                         {
                             Success = false,
                             Errors = new[] { "Ошибка при создании аккаунта" }
                         };
                         
                     }
                 }
             }
             return new AuthResult
             {
                 Success = false,
                 Errors = new[] {"Переданы пустые строки"}
             };
         }

         public async Task<AuthResult> LogInAsync(LoginRequest request)
         {
             if (!string.IsNullOrEmpty(request.Username) || !string.IsNullOrEmpty(request.Password))
             {
                 var result = await  _signInManager.PasswordSignInAsync(request.Username, request.Password, false, false);
                 if (result.IsLockedOut)
                 {
                     
                     return new AuthResult
                     {
                         Success = false,
                         Errors = new[] { "Попытка BruteForce , " }
                     };
                 }

                 if (result.Succeeded)
                 {
                     
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
                         return new AuthResult
                         {
                             Success = true,
                             Token = token,
                         };
                     }
                 }
             }

             return new AuthResult
             {
                 Success = false,
                 Errors = new[] { "Переданы пустые строки" }
             };
         }

    }

    
}
