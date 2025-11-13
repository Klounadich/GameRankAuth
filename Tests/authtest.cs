using GameRankAuth.Controllers;
using GameRankAuth.Data;
using GameRankAuth.Interfaces;
using GameRankAuth.Models;
using GameRankAuth.Services;
using FluentValidation;
using GameRankAuth.Modules;
using StackExchange.Redis;
using GameRankAuth.Services.RabbitMQ;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;

namespace GameRankAuth.GameRankTests;


public class AuthControllerTests
{
    private static Mock<UserManager<IdentityUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<IdentityUser>>();
        return new Mock<UserManager<IdentityUser>>(
            store.Object, null, null, null, null, null, null, null, null);
    }
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly AuthController _authController;
    private readonly ITestOutputHelper _output;
    private readonly AdminPanelDBContext _context;
    private readonly Mock<ILogger<AuthController>> _loggerMock;
    
    public AuthControllerTests(ITestOutputHelper output)
    {
        _output = output;
        _authServiceMock = new Mock<IAuthService>();
        _authController = new AuthController(
            null,
            null,
            null,
            null,
            _authServiceMock.Object,
            new Mock<ILogger<AuthController>>().Object,
            null,
            null,
            null,
            null,
            null,
            null);


    }
    [Fact]
public async Task Register_Test()
{
    var request = new RegisterRequest
    {
        UserName = "TEST",
        Password = "TEST",
        Email = "test@test.com", // ← Исправь email на валидный
        Id = "212"
    };
    
    var authresult = new AuthResult
    {
        Success = true,
        Token = "tfjdfjfkfjfjkf",
        Errors = []
    };

    var testUser = new IdentityUser 
    { 
        UserName = "TEST", 
        Email = "test@test.com",
        Id = "user-123"
    };

    var UserManager = CreateUserManagerMock();
    UserManager.Setup(x => x.FindByNameAsync("TEST")).ReturnsAsync(testUser);
    
    var authServiceMock = new Mock<IAuthService>();
    authServiceMock.Setup(x => x.RegisterAsync(request)).ReturnsAsync(authresult);

    var options = new DbContextOptionsBuilder<AdminPanelDBContext>()
        .UseInMemoryDatabase(databaseName: "TestDatabase")
        .Options;
    using var context = new AdminPanelDBContext(options);
    
    var registerValidatorMock = new Mock<IValidator<RegisterRequest>>();
    registerValidatorMock.Setup(x => x.Validate(It.IsAny<RegisterRequest>()))
        .Returns(new FluentValidation.Results.ValidationResult());
    
    var controller = new AuthController(
        null, UserManager.Object, null, null, 
        authServiceMock.Object, 
        new Mock<ILogger<AuthController>>().Object,
        null, null, context, null, null, registerValidatorMock.Object  
    );

    
    var httpContext = new DefaultHttpContext();
    httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1"); 
    
    controller.ControllerContext = new ControllerContext()
    {
        HttpContext = httpContext
    };
    
    var result = await controller.Register(request);
    Assert.NotNull(result);
    
    
    
    
        
        Assert.IsType<OkObjectResult>(result);
    
    
    
    authServiceMock.Verify(x => x.RegisterAsync(request), Times.Once);
}

    [Fact]
    public async Task Login_Test()
    {
    
        var request = new LoginRequest
        {
            Username = "Klounadich",
            Password = "123321n"
        };
        var authresult = new AuthResult
        {
            Success = true,
            Token = "tfjdfjfkfjfjkf",
            Errors = []
            

        };
       
        
        var authServiceMock = new Mock<IAuthService>();
        authServiceMock.Setup(x => x.LogInAsync(request)).ReturnsAsync(authresult);
            
        var controller = new AuthController(
            null, null, null, null, 
            authServiceMock.Object, 
            new Mock<ILogger<AuthController>>().Object,
            null, null, null, null, null, null   
        );
        var httpcontextmock = new DefaultHttpContext(); 
        controller.ControllerContext.HttpContext = httpcontextmock;
        var result = await  controller.Authorization(request);
        Assert.NotNull(result);
        
            Assert.IsType<OkObjectResult>(result);
        
        
    
        authServiceMock.Verify(x => x.LogInAsync(request), Times.Once);
    }
}