using DtuFoodAPI.Database;
using DtuFoodAPI.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace DtuFoodAPI.Controllers;

[ApiController]
[Route("api/foodtruck/{truckId}/product")]
public class ProductController : ControllerBase
{
    private readonly ILogger<ProductController> _logger;
    
    public ProductController(ILogger<ProductController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllProductsByTruck(Guid truckId)
    {
        throw new NotImplementedException();
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById(Guid truckId, Guid id)
    {
        throw new NotImplementedException();
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateProduct(Guid truckId, [FromBody] ProductRegistry product)
    {
        throw new NotImplementedException();
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(Guid truckId, Guid id, [FromBody] ProductRegistry product)
    {
        throw new NotImplementedException();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(Guid truckId, Guid id)
    {
        throw new NotImplementedException();
    }
}