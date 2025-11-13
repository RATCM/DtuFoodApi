using DotNet.RateLimiter.ActionFilters;
using DtuFoodAPI.Auth;
using DtuFoodAPI.Database;
using DtuFoodAPI.DTOs;
using DtuFoodAPI.Filters;
using DtuFoodAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DtuFoodAPI.Services;

namespace DtuFoodAPI.Controllers;

[ApiController]
[Route("api/foodtruck")]
public class FoodTruckController : ControllerBase
{
    private readonly ILogger<FoodTruckController> _logger;
    private readonly IFoodTruckService _foodTruckService;

    public FoodTruckController(ILogger<FoodTruckController> logger, IFoodTruckService foodTruckService)
    {
        _logger = logger;
        _foodTruckService = foodTruckService;
    }
    
    [HttpGet]
    [RateLimit(PeriodInSec = 60, Limit = 30)]
    public async Task<IActionResult> GetAllFoodTrucks()
    {
        return Ok(await _foodTruckService.GetAllFoodTrucks());
    }
    
    [HttpGet("{id}")]
    [RateLimit(PeriodInSec = 60, Limit = 30)]
    public async Task<IActionResult> GetFoodTruckById(Guid id)
    {
        var foodTruck = await _foodTruckService.GetFoodTruckById(id);
        if (foodTruck is null)
            return NotFound();
        
        return Ok(foodTruck);
    }

    [HttpGet("{id}/image/home")]
    [RateLimit(PeriodInSec = 60, Limit = 10)]
    public async Task<IActionResult> GetFoodTruckHomeBanner(Guid id)
    {
        var image = await _foodTruckService.GetFoodTruckHomeBanner(id);
        if (image == null)
            return NotFound("Image could not be found");
        return File(image.Blob, "application/octet-stream");
    }

    [HttpGet("{id}/image/page")]
    [RateLimit(PeriodInSec = 60, Limit = 10)]
    public async Task<IActionResult> GetFoodTruckPageBanner(Guid id)
    {
        var image = await _foodTruckService.GetFoodTruckPageBanner(id);
        if (image == null)
            return NotFound("Image could not be found");
        return File(image.Blob, "application/octet-stream");
    }
    
    [HttpPost]
    [Authorize(Policy = AuthPolicies.AdminOnly)]
    [RateLimit(PeriodInSec = 60, Limit = 10)]
    public async Task<IActionResult> CreateFoodTruck([FromBody] FoodTruckRegistry foodTruck)
    {
        
        var created = await _foodTruckService.CreateFoodTruck(foodTruck);
        return Created($"api/foodtruck/{created.Id}", created);
        
    }
    
    [HttpPut("{id:guid}")]
    [RateLimit(PeriodInSec = 60, Limit = 10)]
    [Authorize]
    [FoodTruckManagerFilter("id")]
    public async Task<IActionResult> UpdateFoodTruck(Guid id, [FromBody] FoodTruckRegistry foodTruck)
    {
        var updated = await _foodTruckService.UpdateFoodTruck(id, foodTruck);
        if (updated is null) 
            return NotFound();
        
        return Ok(updated);
    }

    [HttpPut("{id}/manager")]
    [RateLimit(PeriodInSec = 60, Limit = 10)]
    [Authorize(Policy = AuthPolicies.AdminOnly)]
    public async Task<IActionResult> AddFoodTruckManager(Guid id, [FromBody] FoodTruckManagerRegistry manager)
    {
        var foodTruck = await _foodTruckService.AddFoodTruckManager(id, manager.Id);
        if (foodTruck is null)
            return NotFound();

        return NoContent();
    }
    
    [HttpPut("{id}/image/home")]
    [RateLimit(PeriodInSec = 60, Limit = 5)]
    [Authorize]
    [FoodTruckManagerFilter("id")]
    public async Task<IActionResult> UpdateFoodTruckHomeBanner(Guid id, IFormFile file)
    {
        using var ms = new MemoryStream();
        
        await file.CopyToAsync(ms);
        byte[] blob = ms.ToArray();
        var updated = await _foodTruckService.UpdateFoodTruckHomeBanner(id, blob);
        if (updated is null) 
            return NotFound();
        
        return Created($"/api/image/{id}/image/home", updated);
    }
    
    [HttpPut("{id}/image/page")]
    [RateLimit(PeriodInSec = 60, Limit = 5)]
    [Authorize]
    [FoodTruckManagerFilter("id")]
    public async Task<IActionResult> UpdateFoodTruckPageBanner(Guid id, [FromBody] IFormFile file)
    {
        using var ms = new MemoryStream();
        
        await file.CopyToAsync(ms);

        byte[] blob = ms.ToArray();
        var updated = await _foodTruckService.UpdateFoodTruckPageBanner(id, blob);
        if (updated is null) 
            return NotFound();
        
        return Created($"/api/image/{id}/image/page", updated);
    }
    
    [HttpDelete("{id}")]
    [RateLimit(PeriodInSec = 60, Limit = 5)]
    [Authorize(Policy = AuthPolicies.AdminOnly)]
    public async Task<IActionResult> DeleteFoodTruck(Guid id)
    {
        var deleted = await _foodTruckService.DeleteFoodTruck(id);
        if (!deleted) 
            return NotFound();
        
        return NoContent();
    }

}