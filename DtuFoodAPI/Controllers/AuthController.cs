using System.Security.Claims;
using DotNet.RateLimiter.ActionFilters;
using DtuFoodAPI.Auth;
using DtuFoodAPI.DTOs;
using DtuFoodAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DtuFoodAPI.Controllers;

/// <summary>
/// Endpoints for authentication
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{   
    private readonly ILogger<AuthController> _logger;
    private readonly IUserService _userService;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly IPasswordHasher<UserRegistry> _passwordHasher;

    /// <summary>
    /// Auth controller constructor
    /// </summary>
    /// <param name="logger">The logger</param>
    /// <param name="userService">The user service</param>
    /// <param name="tokenGenerator">The token generator</param>
    /// <param name="passwordHasher">The password hasher</param>
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
    
    /// <summary>
    /// Login endpoint
    /// </summary>
    /// <param name="userRegistry">The user registry</param>
    /// <returns>The generated JWT tokens</returns>
    /// <response code="200">If the request was successful</response>
    /// <response code="400">If the credentials are invalid</response>
    /// <response code="404">If the user/email doesn't exist</response>
    /// <response code="429">If the rate limit is exceeded</response>
    [HttpPost("login")]
    [RateLimit(PeriodInSec = 60, Limit = 5)]
    [ProducesResponseType(typeof(JwtToken), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
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
    
    /// <summary>
    /// Used for getting a new access token from a refresh token
    /// </summary>
    /// <returns>The generated access token</returns>
    /// <response code="200">If the request was successful</response>
    /// <response code="400">If the token claims are invalid</response>
    /// <response code="429">If the rate limit is exceeded</response>
    [HttpPost("refresh")]
    [RateLimit(PeriodInSec = 60, Limit = 10)]
    [Authorize(AuthenticationSchemes = AuthSchemes.Refresh)]
    [ProducesResponseType(typeof(JwtToken), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
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