using DtuFoodAPI.Auth;
using DtuFoodAPI.Database;
using DtuFoodAPI.DTOs;
using DtuFoodAPI.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DtuFoodAPI.Controllers;

[ApiController]
[Route("api/foodtruck")]
public class FoodTruckController : ControllerBase
{
    private readonly ILogger<FoodTruckController> _logger;
    public FoodTruckController(ILogger<FoodTruckController> logger)
    {
        _logger = logger;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAllFoodTrucks()
    {
        throw new NotImplementedException();
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetFoodTruckById(Guid id)
    {
        throw new NotImplementedException();
    }
    
    [HttpPost]
    [Authorize(Policy = AuthPolicies.AdminOnly)]
    public async Task<IActionResult> CreateFoodTruck([FromBody] FoodTruckRegistry foodTruck)
    {
        throw new NotImplementedException();
    }
    
    [HttpPut("{id}")]
    [Authorize]
    [FoodTruckManagerFilter("id")]
    public async Task<IActionResult> UpdateFoodTruck(Guid id, [FromBody] FoodTruckRegistry foodTruck)
    {
        throw new NotImplementedException();
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = AuthPolicies.AdminOnly)]
    public async Task<IActionResult> DeleteFoodTruck(Guid id)
    {
        throw new NotImplementedException();
    }

}