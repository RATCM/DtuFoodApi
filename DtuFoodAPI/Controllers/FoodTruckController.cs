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

    public FoodTruckController(ILogger<FoodTruckController> logger)
    {
        _logger = logger;
        _foodTruckService = new FoodTruckService();
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAllFoodTrucks()
    {
        _foodTruckService.GetAllFoodTrucks();
        // throw new NotImplementedException();
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetFoodTruckById(Guid id)
    {
        throw new NotImplementedException();
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateFoodTruck([FromBody] FoodTruckRegistry foodTruck)
    {
        _foodTruckService.CreateFoodTruck(foodTruck);
        //throw new NotImplementedException();
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