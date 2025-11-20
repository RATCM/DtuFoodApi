using DotNet.RateLimiter.ActionFilters;
using DtuFoodAPI.Auth;
using DtuFoodAPI.Database;
using DtuFoodAPI.DTOs;
using DtuFoodAPI.Filters;
using DtuFoodAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DtuFoodAPI.Services;
using FluentValidation;

namespace DtuFoodAPI.Controllers;

[ApiController]
[Route("api/foodtruck")]
public class FoodTruckController : ControllerBase
{
    private readonly ILogger<FoodTruckController> _logger;
    private readonly IFoodTruckService _foodTruckService;
    private readonly IValidator<FoodTruckRegistry> _foodTruckValidator;

    public FoodTruckController(ILogger<FoodTruckController> logger,
        IFoodTruckService foodTruckService,
        IValidator<FoodTruckRegistry> foodTruckValidator)
    {
        _logger = logger;
        _foodTruckService = foodTruckService;
        _foodTruckValidator = foodTruckValidator;
    }
    
    /// <summary>
    /// Gets all the food trucks
    /// </summary>
    /// <returns>An array of all food truck</returns>
    /// <response code="200">Successful</response>
    /// <response code="429">If the rate limit is exceeded</response>
    [HttpGet]
    [RateLimit(PeriodInSec = 60, Limit = 30)]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GetAllFoodTrucks()
    {
        return Ok(await _foodTruckService.GetAllFoodTrucks());
    }
    
    /// <summary>
    /// Gets the food truck by id
    /// </summary>
    /// <param name="id">The food truck</param>
    /// <returns>The food truck</returns>
    /// <response code="200">If the food truck was found</response>
    /// <response code="404">If the food truck was not found</response>
    /// <response code="429">If the rate limit is exceeded</response>
    [HttpGet("{id}")]
    [RateLimit(PeriodInSec = 60, Limit = 30)]
    [FoodTruckExistsFilter("id")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GetFoodTruckById(Guid id)
    {
        var foodTruck = await _foodTruckService.GetFoodTruckById(id);
        if (foodTruck is null)
            return NotFound();
        
        return Ok(foodTruck);
    }

    /// <summary>
    /// Gets the home banner for a food truck
    /// </summary>
    /// <param name="id">The food truck id</param>
    /// <returns>The home banner</returns>
    /// <response code="200">If the home banner was found</response>
    /// <response code="404">If the food truck or home banner was not found</response>
    /// <response code="429">If the rate limit is exceeded</response>
    [HttpGet("{id}/image/home")]
    [RateLimit(PeriodInSec = 60, Limit = 10)]
    [FoodTruckExistsFilter("id")]
    [Produces("application/octet-stream", "application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GetFoodTruckHomeBanner(Guid id)
    {
        var image = await _foodTruckService.GetFoodTruckHomeBanner(id);
        if (image == null)
            return NotFound("Image could not be found");
        return File(image.Blob, "application/octet-stream");
    }

    /// <summary>
    /// Gets the page banner for a food truck
    /// </summary>
    /// <param name="id">The food truck id</param>
    /// <returns>The page banner</returns>
    /// <response code="200">If the page banner was found</response>
    /// <response code="404">If the food truck or page banner was not found</response>
    /// <response code="429">If the rate limit is exceeded</response>
    [HttpGet("{id}/image/page")]
    [RateLimit(PeriodInSec = 60, Limit = 10)]
    [FoodTruckExistsFilter("id")]
    [Produces("application/octet-stream", "application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GetFoodTruckPageBanner(Guid id)
    {
        var image = await _foodTruckService.GetFoodTruckPageBanner(id);
        if (image == null)
            return NotFound("Image could not be found");
        return File(image.Blob, "application/octet-stream");
    }
    
    /// <summary>
    /// Creates a new food truck
    /// </summary>
    /// <param name="foodTruck">The food truck registry</param>
    /// <returns>The newly created food truck</returns>
    /// <response code="201">If the food truck was created</response>
    /// <response code="400">If the provided object is invalid</response>
    /// <response code="401">If an invalid (or none) access token was provided</response>
    /// <response code="403">If the user is not an admin</response>
    /// <response code="429">If the rate limit is exceeded</response>
    [HttpPost]
    [Authorize(Policy = AuthPolicies.AdminOnly)]
    [RateLimit(PeriodInSec = 60, Limit = 10)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> CreateFoodTruck([FromBody] FoodTruckRegistry foodTruck)
    {
        var validationResult = await _foodTruckValidator.ValidateAsync(foodTruck);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);
        
        var created = await _foodTruckService.CreateFoodTruck(foodTruck);
        return Created($"api/foodtruck/{created.Id}", created);
        
    }
    
    /// <summary>
    /// Updates an existing food truck
    /// </summary>
    /// <param name="id">The food truck id</param>
    /// <param name="foodTruck">The food truck registry</param>
    /// <returns>The updated food truck</returns>
    /// <response code="200">If the food truck was updated</response>
    /// <response code="400">If the provided object is invalid</response>
    /// <response code="401">If an invalid (or none) access token was provided</response>
    /// <response code="403">If the user is not a manager for the specific food truck</response>
    /// <response code="404">If the food truck was not found</response>
    /// <response code="429">If the rate limit is exceeded</response>
    [HttpPut("{id:guid}")]
    [RateLimit(PeriodInSec = 60, Limit = 10)]
    [Authorize]
    [FoodTruckExistsFilter("id")]
    [FoodTruckManagerFilter("id")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UpdateFoodTruck(Guid id, [FromBody] FoodTruckRegistry foodTruck)
    {
        var validationResult = await _foodTruckValidator.ValidateAsync(foodTruck);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);
        
        var updated = await _foodTruckService.UpdateFoodTruck(id, foodTruck);
        if (updated is null) 
            return NotFound();
        
        return Ok(updated);
    }

    /// <summary>
    /// Adds a user as a manager to a specific food truck
    /// </summary>
    /// <param name="id">The food truck id</param>
    /// <param name="manager">The manager registry</param>
    /// <returns>No Content</returns>
    /// <response code="204">If the manager was added</response>
    /// <response code="401">If an invalid (or none) access token was provided</response>
    /// <response code="403">If the user is not an admin</response>
    /// <response code="404">If the food truck or user was not found</response>
    /// <response code="429">If the rate limit is exceeded</response>
    [HttpPut("{id}/manager")]
    [RateLimit(PeriodInSec = 60, Limit = 10)]
    [Authorize(Policy = AuthPolicies.AdminOnly)]
    [FoodTruckExistsFilter("id")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> AddFoodTruckManager(Guid id, [FromBody] FoodTruckManagerRegistry manager)
    {
        var foodTruck = await _foodTruckService.AddFoodTruckManager(id, manager.Id);
        if (foodTruck is null)
            return NotFound();

        return NoContent();
    }
    
    /// <summary>
    /// Updates/sets the food truck home banner
    /// </summary>
    /// <param name="id">The food truck id</param>
    /// <param name="file">The image</param>
    /// <returns>The created home banner</returns>
    /// <response code="201">If the home banner was created</response>
    /// <response code="401">If an invalid (or none) access token was provided</response>
    /// <response code="403">If the user is not a manager for the specific food truck</response>
    /// <response code="404">If the food truck was not found</response>
    /// <response code="429">If the rate limit is exceeded</response>
    [HttpPut("{id}/image/home")]
    [RateLimit(PeriodInSec = 60, Limit = 5)]
    [Authorize]
    [FoodTruckExistsFilter("id")]
    [FoodTruckManagerFilter("id")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UpdateFoodTruckHomeBanner(Guid id, IFormFile file)
    {
        using var ms = new MemoryStream();
        
        await file.CopyToAsync(ms);
        byte[] blob = ms.ToArray();
        var updated = await _foodTruckService.UpdateFoodTruckHomeBanner(id, blob);
        if (updated is null) 
            return NotFound();
        
        return Created($"/api/image/{id}/image/home", null);
    }
    
    /// <summary>
    /// Updates/sets the food truck page banner
    /// </summary>
    /// <param name="id">The food truck id</param>
    /// <param name="file">The image</param>
    /// <returns>The created page banner</returns>
    /// <response code="201">If the page banner was created</response>
    /// <response code="401">If an invalid (or none) access token was provided</response>
    /// <response code="403">If the user is not a manager for the specific food truck</response>
    /// <response code="404">If the food truck was not found</response>
    /// <response code="429">If the rate limit is exceeded</response>
    [HttpPut("{id}/image/page")]
    [RateLimit(PeriodInSec = 60, Limit = 5)]
    [Authorize]
    [FoodTruckExistsFilter("id")]
    [FoodTruckManagerFilter("id")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UpdateFoodTruckPageBanner(Guid id, IFormFile file)
    {
        using var ms = new MemoryStream();
        
        await file.CopyToAsync(ms);
        byte[] blob = ms.ToArray();
        var updated = await _foodTruckService.UpdateFoodTruckPageBanner(id, blob);
        if (updated is null) 
            return NotFound();
        
        return Created($"/api/image/{id}/image/page", null);
    }
    
    /// <summary>
    /// Deletes a food truck
    /// </summary>
    /// <param name="id">Food truck id</param>
    /// <returns>No Content</returns>
    /// <response code="204">If the food truck was deleted</response>
    /// <response code="401">If an invalid (or none) access token was provided</response>
    /// <response code="403">If the user is not an admin</response>
    /// <response code="404">If the food truck was not found</response>
    /// <response code="429">If the rate limit is exceeded</response>
    [HttpDelete("{id}")]
    [RateLimit(PeriodInSec = 60, Limit = 5)]
    [Authorize(Policy = AuthPolicies.AdminOnly)]
    [FoodTruckExistsFilter("id")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteFoodTruck(Guid id)
    {
        var deleted = await _foodTruckService.DeleteFoodTruck(id);
        if (!deleted) 
            return NotFound();
        
        return NoContent();
    }

}