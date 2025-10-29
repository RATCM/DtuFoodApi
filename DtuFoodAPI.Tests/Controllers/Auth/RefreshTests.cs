using System.Security.Claims;
using DtuFoodAPI.Controllers;
using DtuFoodAPI.DTOs;
using DtuFoodAPI.Models;
using DtuFoodAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace DtuFoodAPI.Tests.Controllers.Auth;

public class RefreshTests
{
    private ILogger<AuthController> _logger;
    private IUserService _userService;
    private ITokenGenerator _tokenGenerator;
    private IPasswordHasher<UserRegistry> _passwordHasher;

    private AuthController _sut;

    [SetUp]
    public void Setup()
    {
        _logger = Substitute.For<ILogger<AuthController>>();
        _userService = Substitute.For<IUserService>();
        _tokenGenerator = Substitute.For<ITokenGenerator>();
        _passwordHasher = Substitute.For<IPasswordHasher<UserRegistry>>();

        _sut = new AuthController(_logger, _userService, _tokenGenerator, _passwordHasher);
    }
    
    [Test]
    public async Task RefreshReturns_ServerError_WhenEmailClaimDoesntExist()
    {
        // Arrange
        _sut.ControllerContext.HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity())
        };
        
        // Act
        var result = await _sut.RefreshAccessToken() as ObjectResult;

        //Assert
        Assert.That(result!.StatusCode, Is.EqualTo(500));
    }

    [Test]
    public async Task RefreshReturns_BadRequest_WhenUserDoesntExist()
    {
        // Arrange
        _sut.ControllerContext.HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity([
                new Claim(ClaimTypes.Email, "some@email")
            ]))
        };
        
        // Act
        var result = await _sut.RefreshAccessToken();

        //Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task RefreshReturns_OkWithNewAccessToken_WhenSuccessful()
    {
        // Arrange
        var user = new User()
        {
            Id = Guid.NewGuid(),
            Email = "some@email",
            FoodTrucks = [],
            PasswordHash = "some password hash",
            Role = UserRole.Manager
        };

        _sut.ControllerContext.HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity([
                new Claim(ClaimTypes.Email, "some@email")
            ]))
        };
        _userService.GetUserByEmail("some@email")
            .Returns(user);
        _tokenGenerator.GenerateJwtAccessToken(user)
            .Returns("some-token");
        // Act
        var result = await _sut.RefreshAccessToken() as ObjectResult;

        //Assert
        Assert.That(result!.Value, Is.EqualTo(new JwtToken(){ AccessToken = "some-token" }));
    }
}