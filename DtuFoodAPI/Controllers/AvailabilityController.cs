using DotNet.RateLimiter.ActionFilters;
using DtuFoodAPI.DTOs;
using DtuFoodAPI.Filters;
using DtuFoodAPI.Models;
using DtuFoodAPI.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DtuFoodAPI.Controllers;

/// <summary>
/// Endpoints for the food truck availability
/// </summary>
[ApiController]
[Route("api/foodtruck/{truckId:guid}/availability")]
public class AvailabilityController : ControllerBase
{
    private readonly ILogger<AvailabilityController> _logger;
    private readonly IAvailabilityService _availabilityService;
    private readonly IValidator<AvailabilityRegistry> _availabilityValidator;
    
    /// <summary>
    /// Availability controller constructor
    /// </summary>
    /// <param name="logger">The logger</param>
    /// <param name="availabilityService">The availability service</param>
    /// <param name="availabilityValidator">The availability validator</param>
    public AvailabilityController(ILogger<AvailabilityController> logger,
        IAvailabilityService availabilityService,
        IValidator<AvailabilityRegistry> availabilityValidator)
    {
        _logger = logger;
        _availabilityService = availabilityService;
        _availabilityValidator = availabilityValidator;
    }

    /// <summary>
    /// Gets all the available time slots for a food truck
    /// </summary>
    /// <param name="truckId">The food truck id</param>
    /// <returns>The list of availability for the food truck</returns>
    /// <response code="200">If the request was successful</response>
    /// <response code="429">If the rate limit is exceeded</response>
    [HttpGet]
    [RateLimit(PeriodInSec = 60, Limit = 30)]
    [FoodTruckExistsFilter("truckId")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(List<AvailabilityDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GetAllAvailabilityByTruck(Guid truckId)
    {
        return Ok(await _availabilityService.GetAllAvailabilityForTruck(truckId));
    }
    
    /// <summary>
    /// Gets the available time slot for a specific day
    /// </summary>
    /// <param name="truckId">The food truck id</param>
    /// <param name="weekDay">The week day</param>
    /// <returns>The available time slot</returns>
    /// <response code="200">If the request was successful</response>
    /// <response code="400">If the weekDay is not a valid value</response>
    /// <response code="404">If the food truck or time slot was not found</response>
    /// <response code="429">If the rate limit is exceeded</response>
    [HttpGet("{weekDay}")]
    [RateLimit(PeriodInSec = 60, Limit = 30)]
    [FoodTruckExistsFilter("truckId")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(AvailabilityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GetAvailabilityByTruckAndWeekDay(Guid truckId, string weekDay)
    {
        // We are using a string here because enums can be a bit unpredictable
        if (!Enum.TryParse<WeekDay>(weekDay, out var weekDayEnum))
            return BadRequest($"{weekDay} is not a valid week day value");

        var availability = await _availabilityService.GetAvailabilityForTruck(truckId, weekDayEnum);
        if (availability is null)
            return NotFound("Availability not found");

        return Ok(availability);
    }

    /// <summary>
    /// Adds a new available time slot for a food truck
    /// </summary>
    /// <param name="truckId">The food truck id</param>
    /// <param name="registry">The availability registry</param>
    /// <returns>The created time slot</returns>
    /// <response code="201">If the time slot was created</response>
    /// <response code="400">If the registry validation failed, or there is a conflict</response>
    /// <response code="401">If an invalid (or none) access token was provided</response>
    /// <response code="403">If the user is not a manager</response>
    /// <response code="404">If the food truck was not found</response>
    /// <response code="429">If the rate limit is exceeded</response>
    [HttpPost]
    [RateLimit(PeriodInSec = 60, Limit = 10)]
    [Authorize]
    [FoodTruckExistsFilter("truckId")]
    [FoodTruckManagerFilter("truckId")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(AvailabilityDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> CreateAvailability(Guid truckId, AvailabilityRegistry registry)
    {
        var validationResult = await _availabilityValidator.ValidateAsync(registry);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var weekDay = Enum.Parse<WeekDay>(registry.DayOfWeek);

        if (await _availabilityService.AvailabilityExists(truckId, weekDay))
            return BadRequest("This day slot has already been created");
        var created = await _availabilityService.CreateAvailabilityForTruck(truckId, registry);
        
        return Created($"/api/foodtruck/{truckId}/availability/{registry.DayOfWeek}", created);
    }
    
    
    /// <summary>
    /// Updates a specific time slot for a food truck
    /// </summary>
    /// <param name="truckId">The food truck id</param>
    /// <param name="weekDay">The week day</param>
    /// <param name="registry">The availability registry for updating</param>
    /// <returns>The updated time slot</returns>
    /// <response code="200">If the time slot was updated</response>
    /// <response code="400">If the registry validation failed, or there is a conflict</response>
    /// <response code="401">If an invalid (or none) access token was provided</response>
    /// <response code="403">If the user is not a manager</response>
    /// <response code="404">If the food truck or availability was not found</response>
    /// <response code="429">If the rate limit is exceeded</response>
    [HttpPut("{weekDay}")]
    [RateLimit(PeriodInSec = 60, Limit = 10)]
    [Authorize]
    [FoodTruckExistsFilter("truckId")]
    [FoodTruckManagerFilter("truckId")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(AvailabilityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UpdateAvailability(Guid truckId,
        string weekDay,
        AvailabilityRegistry registry)
    {
        var validationResult = await _availabilityValidator.ValidateAsync(registry);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var weekDayEnum = Enum.Parse<WeekDay>(weekDay);

        if (!await _availabilityService.AvailabilityExists(truckId, weekDayEnum))
            return NotFound("Truck not found, or this availability slot has not been created");
        
        var updated = await _availabilityService.UpdateAvailabilityForTruck(truckId, weekDayEnum, registry);

        // Idk what to do here honestly
        // Preferably the services should include
        // the error codes when they return
        if (updated is null)
            return BadRequest();

        return Ok(updated);
    }

    /// <summary>
    /// Deletes a time slot from a food truck
    /// </summary>
    /// <param name="truckId">The food truck id</param>
    /// <param name="weekDay">The week day</param>
    /// <returns>No Content</returns>
    /// <response code="204">If the time slot was deleted</response>
    /// <response code="400">If the week day is not a valid value</response>
    /// <response code="401">If an invalid (or none) access token was provided</response>
    /// <response code="403">If the user is not a manager</response>
    /// <response code="404">If the food truck or availability was not found</response>
    /// <response code="429">If the rate limit is exceeded</response>
    [HttpDelete("{weekDay}")]
    [RateLimit(PeriodInSec = 60, Limit = 10)]
    [Authorize]
    [FoodTruckExistsFilter("truckId")]
    [FoodTruckManagerFilter("truckId")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteAvailability(Guid truckId,
        string weekDay)
    {
        if (!Enum.TryParse<WeekDay>(weekDay, out var weekDayEnum))
            return BadRequest($"{weekDay} is not a valid week day value");

        if (!await _availabilityService.AvailabilityExists(truckId, weekDayEnum))
            return NotFound("Availability not found");

        var deleted = await _availabilityService.DeleteAvailability(truckId, weekDayEnum);

        if (deleted)
            return NoContent();

        // I'm unsure if this can ever be reached
        return BadRequest();
    }
}