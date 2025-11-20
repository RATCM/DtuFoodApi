using DotNet.RateLimiter.ActionFilters;
using DtuFoodAPI.Auth;
using DtuFoodAPI.Database;
using DtuFoodAPI.DTOs;
using DtuFoodAPI.Filters;
using DtuFoodAPI.Models;
using DtuFoodAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DtuFoodAPI.Controllers;

/// <summary>
/// Endpoints for the users
/// </summary>
[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IUserService _userService;

    /// <summary>
    /// User controller constructor
    /// </summary>
    /// <param name="logger">The logger</param>
    /// <param name="userService">The user service</param>
    public UserController(ILogger<UserController> logger,
        IUserService userService)
    {
        _logger = logger;
        _userService = userService;
    }
    
    /// <summary>
    /// Gets all the users
    /// </summary>
    /// <returns>All users</returns>
    /// <response code="200">If the request was successful</response>
    /// <response code="401">If an invalid (or none) access token was provided</response>
    /// <response code="429">If the rate limit is exceeded</response>
    [HttpGet]
    [RateLimit(PeriodInSec = 60, Limit = 10)]
    [Authorize]
    [Produces("application/json")]
    [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GetAllUsers()
    {
        return Ok(await _userService.GetAllUsers());
    }

    /// <summary>
    /// Gets a user by their id
    /// </summary>
    /// <param name="id">The user id</param>
    /// <returns>The user</returns>
    /// <response code="200">If the request was successful</response>
    /// <response code="404">If the user was not found</response>
    /// <response code="429">If the rate limit is exceeded</response>
    [HttpGet("{id}")]
    [RateLimit(PeriodInSec = 60, Limit = 30)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var user = await _userService.GetUserById(id);
        if (user is null)
            return NotFound();
        
        return Ok(user);
    }

    /// <summary>
    /// Creates a new user
    /// </summary>
    /// <param name="user">The user registry</param>
    /// <returns>The created user</returns>
    /// <response code="201">If the user was created</response>
    /// <response code="401">If an invalid (or none) access token was provided</response>
    /// <response code="403">If the user is not an admin</response>
    /// <response code="429">If the rate limit is exceeded</response>
    [HttpPost]
    [RateLimit(PeriodInSec = 60, Limit = 10)]
    [Authorize(Policy = AuthPolicies.AdminOnly)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> CreateUser([FromBody] UserRegistry user)
    {
        var created = await _userService.CreateUser(user);
        _logger.LogInformation("User created, id: {id}", created.Id);
        
        return Created($"api/user/{created.Id}", created);
    }
    
    /// <summary>
    /// Updates a user
    /// </summary>
    /// <param name="id">The user id</param>
    /// <param name="user">The user registry for updating</param>
    /// <returns>The updated user</returns>
    /// <response code="200">If the user was updated</response>
    /// <response code="401">If an invalid (or none) access token was provided</response>
    /// <response code="403">If the logged-in user id does not match the provided id</response>
    /// <response code="429">If the rate limit is exceeded</response>
    [HttpPut("{id}")]
    [RateLimit(PeriodInSec = 60, Limit = 10)]
    [Authorize]
    [UserFilter("id")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UserRegistry user)
    {
        var updated = await _userService.UpdateUser(id, user);
        if (updated is null)
            throw new Exception("Unable to update user");
        
        _logger.LogInformation("User updated, id: {id}", id);
        return Ok(updated);
    }

    /// <summary>
    /// Deletes a user
    /// </summary>
    /// <param name="id">The user id</param>
    /// <returns>No Content</returns>
    /// <response code="204">If the user was deleted</response>
    /// <response code="401">If an invalid (or none) access token was provided</response>
    /// <response code="403">If the logged-in user id does not match the provided id</response>
    /// <response code="429">If the rate limit is exceeded</response>
    [HttpDelete("{id}")]
    [RateLimit(PeriodInSec = 60, Limit = 10)]
    [Authorize]
    [UserFilter("id")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var deleted = await _userService.DeleteUser(id);
        if (!deleted)
            throw new Exception("Unable to delete user");

        _logger.LogInformation("User deleted, id: {id}", id);
        return NoContent();
    }
}