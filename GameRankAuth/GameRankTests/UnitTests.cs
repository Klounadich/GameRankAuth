using GameRankAuth.Services;
using GameRankAuth.Data;
using GameRankAuth.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace GameRankTests
{
    public static class MockHelpers
    {
        public static Mock<UserManager<TUser>> MockUserManager<TUser>() where TUser : class
        {
            var store = new Mock<IUserStore<TUser>>();
            var options = new Mock<IOptions<IdentityOptions>>();
            var passwordHasher = new Mock<IPasswordHasher<TUser>>();
            var userValidators = new List<IUserValidator<TUser>> { new UserValidator<TUser>() };
            var passwordValidators = new List<IPasswordValidator<TUser>> { new PasswordValidator<TUser>() };
            var keyNormalizer = new Mock<ILookupNormalizer>();
            var errors = new Mock<IdentityErrorDescriber>();
            var services = new Mock<IServiceProvider>();
            var logger = new Mock<ILogger<UserManager<TUser>>>();

            return new Mock<UserManager<TUser>>(
                store.Object,
                options.Object,
                passwordHasher.Object,
                userValidators,
                passwordValidators,
                keyNormalizer.Object,
                errors.Object,
                services.Object,
                logger.Object);
        }

        public static Mock<SignInManager<TUser>> MockSignInManager<TUser>() where TUser : class
        {
            var userManager = MockUserManager<TUser>();
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<TUser>>();
            var options = new Mock<IOptions<IdentityOptions>>();
            var logger = new Mock<ILogger<SignInManager<TUser>>>();
            var schemes = new Mock<IAuthenticationSchemeProvider>();
            var confirmation = new Mock<IUserConfirmation<TUser>>();

            return new Mock<SignInManager<TUser>>(
                userManager.Object,
                contextAccessor.Object,
                claimsFactory.Object,
                options.Object,
                logger.Object,
                schemes.Object,
                confirmation.Object);
        }
    }

   public class UnitTests : IDisposable
{
    private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
    private readonly Mock<SignInManager<IdentityUser>> _signInManagerMock;
    private readonly ApplicationDbContext _dbContext;
    private readonly JWTTokenService _jwtTokenService;
    private readonly AuthService _authService;

    public UnitTests()
    {
        
        _userManagerMock = MockHelpers.MockUserManager<IdentityUser>();
        _signInManagerMock = MockHelpers.MockSignInManager<IdentityUser>();

        
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid())
            .Options;
        _dbContext = new ApplicationDbContext(options);

        
        var authSettings = new AuthSettings 
        { 
            SecretKey = "TestSecretKeyWithSufficientLength12345" 
        };

        
        _jwtTokenService = new JWTTokenService(authSettings, _userManagerMock.Object);

        
        _authService = new AuthService(
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _dbContext,
            _jwtTokenService
        );
    }

    [Fact]
    public async Task RegisterUser_Success_WhenUserDoesNotExist()
    {
        
        var request = new RegisterRequest
        {
            UserName = "TestUser",
            Email = "test@example.com",
            Password = "Password123!"
        };

        
        _userManagerMock
            .Setup(x => x.FindByNameAsync(It.IsAny<string>()))
            .ReturnsAsync((IdentityUser)null);

        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        
        _userManagerMock
            .Setup(x => x.GetRolesAsync(It.IsAny<IdentityUser>()))
            .ReturnsAsync(new List<string>());

        
        var result = await _authService.RegisterAsync(request);

        
        Assert.True(result.Success);
        _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()), Times.Once);
    }

    public void Dispose()
    {
        _dbContext?.Dispose();
    }
}
}