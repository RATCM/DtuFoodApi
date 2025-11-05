using DtuFoodAPI.Database;
using DtuFoodAPI.DTOs;
using DtuFoodAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DtuFoodAPI.Controllers;

[ApiController]
[Route("api/foodtruck/{truckId}/product")]
public class ProductController : ControllerBase
{
    private readonly ILogger<ProductController> _logger;
    private readonly IProductService _productService;
    
    public ProductController(ILogger<ProductController> logger, IProductService productService)
    {
        _productService = productService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllProductsByTruck(Guid truckId)
    {
        return Ok(await _productService.GetAllProductsFromFoodTruck(truckId));
    }
    
    //old name GetProductById(), but product has no id, it has composite key with name and truckid.
    [HttpGet("{productName}")]
    public async Task<IActionResult> GetProductByTruckIdAndProductName(Guid truckId, String productName)
    {
        var product = await _productService.GetProductByTruckIdAndProductName(truckId,productName);
        if (product is null)
            return NotFound();
        
        return Ok(product);
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateProduct(Guid truckId, [FromBody] ProductRegistry product)
    {
        var created = await _productService.CreateProduct(truckId, product);
        
        //URI.EscapeDatastring makes sure URI link works even if name contains space or weird charachters
        return Created($"api/foodtruck/{truckId}/product/{Uri.EscapeDataString(created.Name)}", created);
        
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(Guid truckId, Guid id, [FromBody] ProductRegistry product)
    {
        var updated = await _productService.UpdateProduct(truckId, id.ToString(), product, HttpContext.RequestAborted);
        if (updated is null)
            return NotFound();

        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(Guid truckId, Guid id)
    {
        var deleted = await _productService.DeleteProduct(truckId, id.ToString(), HttpContext.RequestAborted);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}