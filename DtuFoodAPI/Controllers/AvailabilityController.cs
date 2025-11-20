using DotNet.RateLimiter.ActionFilters;
using DtuFoodAPI.DTOs;
using DtuFoodAPI.Filters;
using DtuFoodAPI.Models;
using DtuFoodAPI.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DtuFoodAPI.Controllers;

[ApiController]
[Route("api/foodtruck/{truckId:guid}/availability")]
public class AvailabilityController : ControllerBase
{
    private readonly ILogger<AvailabilityController> _logger;
    private readonly IAvailabilityService _availabilityService;
    private readonly IValidator<AvailabilityRegistry> _availabilityValidator;
    
    public AvailabilityController(ILogger<AvailabilityController> logger,
        IAvailabilityService availabilityService,
        IValidator<AvailabilityRegistry> availabilityValidator)
    {
        _logger = logger;
        _availabilityService = availabilityService;
        _availabilityValidator = availabilityValidator;
    }

    [HttpGet]
    [RateLimit(PeriodInSec = 60, Limit = 30)]
    [FoodTruckExistsFilter("truckId")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GetAllAvailabilityByTruck(Guid truckId)
    {
        return Ok(await _availabilityService.GetAllAvailabilityForTruck(truckId));
    }
    
    [HttpGet("{weekDay}")]
    [RateLimit(PeriodInSec = 60, Limit = 30)]
    [FoodTruckExistsFilter("truckId")]
    [ProducesResponseType(StatusCodes.Status200OK)]
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

    [HttpPost]
    [RateLimit(PeriodInSec = 60, Limit = 10)]
    [FoodTruckExistsFilter("truckId")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
    
    [HttpPut("{weekDay}")]
    [RateLimit(PeriodInSec = 60, Limit = 10)]
    [Authorize]
    [FoodTruckExistsFilter("truckId")]
    [FoodTruckManagerFilter("truckId")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

    [HttpDelete("{weekDay}")]
    [RateLimit(PeriodInSec = 60, Limit = 10)]
    [Authorize]
    [FoodTruckExistsFilter("truckId")]
    [FoodTruckManagerFilter("truckId")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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