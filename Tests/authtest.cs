using GameRankAuth.Controllers;
using GameRankAuth.Data;
using GameRankAuth.Interfaces;
using GameRankAuth.Models;
using GameRankAuth.Services;
using FluentValidation;
using StackExchange.Redis;
using GameRankAuth.Services.RabbitMQ;
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
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly AuthController _authController;
    private readonly ITestOutputHelper _output;
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
            null   
        );
    
      
    }

    [Fact]
    public async Task Login_Test()
    {
    
        var request = new LoginRequest
        {
            Username = "Klounadich",
            Password = "123"
        };
        var authresult = new AuthResult
        {
            Success = true,
            Token = "tfjdfjfkfjfjkf"

        };
        _authServiceMock.Setup(x => x.LogInAsync(request)).ReturnsAsync(authresult);

        var result = await  _authController.Authorization(request);
        Assert.NotNull(result);
        // var okResult = Assert.IsType<OkObjectResult>(result);
    
   
   
        var badRequestResult = result as BadRequestObjectResult;
        _output.WriteLine($"❌ BadRequest: StatusCode={badRequestResult?.StatusCode}");
        _output.WriteLine($"❌ BadRequest Value: {badRequestResult?.Value}");
        
    
        _authServiceMock.Verify(x => x.LogInAsync(request), Times.Once);
    }
}