using DtuFoodAPI.Auth;
using DtuFoodAPI.Database;
using DtuFoodAPI.DTOs;
using DtuFoodAPI.Models;
using DtuFoodAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DtuFoodAPI.Controllers;

[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IUserService _userService;

    public UserController(ILogger<UserController> logger,
        IUserService userService)
    {
        _logger = logger;
        _userService = userService;
    }
    
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAllUsers()
    {
        return Ok(await _userService.GetAllUsers());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var user = await _userService.GetUserById(id);
        if (user is null)
            return NotFound();
        
        return Ok(user);
    }

    [HttpPost]
    [Authorize(Policy = AuthPolicies.AdminOnly)]
    public async Task<IActionResult> CreateUser([FromBody] UserRegistry user)
    {
        var created = await _userService.CreateUser(user);
        return Created($"api/user/{created.Id}", created);
    }
    
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UserRegistry user)
    {
        var updated = await _userService.UpdateUser(id, user);
        if (updated is null)
            return NotFound();

        return Ok(updated);

    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var deleted = await _userService.DeleteUser(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}