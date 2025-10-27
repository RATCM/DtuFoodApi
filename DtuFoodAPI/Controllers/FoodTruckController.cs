using DtuFoodAPI.Database;
using DtuFoodAPI.DTOs;
using Microsoft.AspNetCore.Mvc;
using DtuFoodAPI.Services;

namespace DtuFoodAPI.Controllers;

[ApiController]
[Route("api/foodtruck")]
public class FoodTruckController : ControllerBase
{
    private readonly ILogger<FoodTruckController> _logger;
    private readonly FoodTruckService _foodTruckService;

    public FoodTruckController(ILogger<FoodTruckController> logger, FoodTruckService foodTruckService)
    {
        _logger = logger;
        _foodTruckService = foodTruckService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAllFoodTrucks()
    {
        return Ok(await _foodTruckService.GetAllFoodTrucks());
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetFoodTruckById(Guid id)
    {
        var foodTruck = await _foodTruckService.GetFoodTruckById(id);
        if (foodTruck is null)
            return NotFound();
        
        return Ok(foodTruck);
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateFoodTruck([FromBody] FoodTruckRegistry foodTruck)
    {
        
        var created = await _foodTruckService.CreateFoodTruck(foodTruck);
        return Created($"api/foodtruck/{created.Id}", created);
        
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateFoodTruck(Guid id, [FromBody] FoodTruckRegistry foodTruck)
    {
        throw new NotImplementedException();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFoodTruck(Guid id)
    {
        throw new NotImplementedException();
    }

}