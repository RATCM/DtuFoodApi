using System.Security.Claims;
using DotNet.RateLimiter.ActionFilters;
using DtuFoodAPI.Auth;
using DtuFoodAPI.DTOs;
using DtuFoodAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DtuFoodAPI.Controllers;

// TODO: Add user registration
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{   
    private readonly ILogger<AuthController> _logger;
    private readonly IUserService _userService;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly IPasswordHasher<UserRegistry> _passwordHasher;

    public AuthController(ILogger<AuthController> logger,
        IUserService userService,
        ITokenGenerator tokenGenerator,
        IPasswordHasher<UserRegistry> passwordHasher)
    {
        _logger = logger;
        _userService = userService;
        _tokenGenerator = tokenGenerator;
        _passwordHasher = passwordHasher;
    }
    
    
    [HttpPost("login")]
    [RateLimit(PeriodInSec = 60, Limit = 5)]
    public async Task<IActionResult> RegisterUser([FromBody] UserRegistry userRegistry)
    {
        // Check if the user exists
        var user = await _userService.GetUserByEmail(userRegistry.Email);
        if (user is null) return NotFound("Email does not exist");

        var passwordHash = await _userService.GetPasswordHash(user.Id);
        
        // Check if password matches
        var hashResult = _passwordHasher.VerifyHashedPassword(userRegistry, passwordHash!, userRegistry.Password);
        
        if(hashResult == PasswordVerificationResult.Failed)
            return BadRequest("Password doesn't match");

        // Return the tokens
        var accessToken = _tokenGenerator.GenerateJwtAccessToken(user);
        var refreshToken = _tokenGenerator.GenerateJwtRefreshToken(user);
        
        return Ok(new JwtToken()
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        });
    }
    
    [HttpPost("refresh")]
    [RateLimit(PeriodInSec = 60, Limit = 10)]
    [Authorize(AuthenticationSchemes = AuthSchemes.Refresh)]
    public async Task<IActionResult> RefreshAccessToken()
    {
        // We can find the email from the provided JWT token
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (email is null) return Problem(statusCode: 500); // This really shouldn't happen

        var user = await _userService.GetUserByEmail(email);
        if (user is null) return BadRequest("Token not valid");
        
        // Return the new access token
        var accessToken = _tokenGenerator.GenerateJwtAccessToken(user);
        
        return Ok(new JwtToken()
        {
            AccessToken = accessToken
        });
    }
}