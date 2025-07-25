﻿using GameRankAuth.Data;
using GameRankAuth.Interfaces;
using GameRankAuth.Services;
using GameRankAuth.Models;
using GameRankAuth.Modules;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
        

        public AuthController(ApplicationDbContext context, UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager, JWTTokenService jWTToken, IAuthService authService , 
            ILogger<AuthController> logger , IVerifyService verifyService )
        {
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
                return Ok(new { Message = "Почта успешно подтверждена" });
            }
            else
            {
                return Conflict(new { Message = "Ошибка подтверждения почты , попробуйте позже" });
            }
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
            
            catch (Exception)
            {
                return BadRequest(new { Message = "Ошибка авторизации , попробуйте позже" });
            }

            
        }



    }
}



