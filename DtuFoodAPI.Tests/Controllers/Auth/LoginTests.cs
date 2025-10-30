using DtuFoodAPI.Controllers;
using DtuFoodAPI.DTOs;
using DtuFoodAPI.Models;
using DtuFoodAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework.Legacy;

namespace DtuFoodAPI.Tests.Controllers.Auth;

public class LoginTests
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
    public async Task LoginReturns_NotFound_WhenUserNotExists()
    {
        // Arrange
        var registry = new UserRegistry()
        {
            Email = "some@email",
            Password = "Some password1!"
        };
        _userService.GetUserByEmail("some@email")
            .Returns(Task.FromResult<User?>(null));
        
        // Act
        var result = await _sut.RegisterUser(registry);

        //Assert
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }
    
    [Test]
    public async Task LoginReturns_BadRequest_WhenPasswordDoesntMatch()
    {
        // Arrange
        var registry = new UserRegistry()
        {
            Email = "some@email",
            Password = "Some password1!"
        };
        var user = new User()
        {
            Id = Guid.NewGuid(),
            Email = "some@email",
            FoodTrucks = [],
            PasswordHash = "some password hash",
            Role = UserRole.Manager
        };
        
        _userService.GetUserByEmail("some@email")
            .Returns(user);

        _passwordHasher.VerifyHashedPassword(registry, user.PasswordHash, registry.Password)
            .Returns(PasswordVerificationResult.Failed);
        
        // Act
        var result = await _sut.RegisterUser(registry);

        //Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }
    
    [Test]
    public async Task LoginReturns_Ok_WhenSuccessful()
    {
        // Arrange
        var registry = new UserRegistry()
        {
            Email = "some@email",
            Password = "Some password1!"
        };
        var user = new User()
        {
            Id = Guid.NewGuid(),
            Email = "some@email",
            FoodTrucks = [],
            PasswordHash = "some password hash",
            Role = UserRole.Manager
        };
        
        _userService.GetUserByEmail("some@email")
            .Returns(user);

        _passwordHasher.VerifyHashedPassword(registry, user.PasswordHash, registry.Password)
            .Returns(PasswordVerificationResult.Success);
        
        // Act
        var result = await _sut.RegisterUser(registry);

        //Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }
    
}