using DotNet.RateLimiter.ActionFilters;
using DtuFoodAPI.Database;
using DtuFoodAPI.DTOs;
using DtuFoodAPI.Filters;
using DtuFoodAPI.Services;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace DtuFoodAPI.Controllers;

[ApiController]
[Route("api/foodtruck/{truckId:guid}/product")]
public class ProductController : ControllerBase
{
    private readonly ILogger<ProductController> _logger;
    private readonly IProductService _productService;
    private readonly IValidator<ProductRegistry> _productValidator;
    
    public ProductController(ILogger<ProductController> logger,
        IProductService productService,
        IValidator<ProductRegistry> productValidator)
    {
        _productService = productService;
        _logger = logger;
        _productValidator = productValidator;
    }

    [HttpGet]
    [RateLimit(PeriodInSec = 60, Limit = 30)]
    public async Task<IActionResult> GetAllProductsByTruck(Guid truckId)
    {
        return Ok(await _productService.GetAllProductsFromFoodTruck(truckId));
    }
    
    //old name GetProductById(), but product has no id, it has composite key with name and truckid.
    [HttpGet("{productName}")]
    [RateLimit(PeriodInSec = 60, Limit = 30)]
    [FoodTruckExistsFilter("truckId")]
    public async Task<IActionResult> GetProductByTruckIdAndProductName(Guid truckId, string productName)
    {
        var product = await _productService.GetProductByTruckIdAndProductName(truckId,productName);
        if (product is null)
            return NotFound();
        
        return Ok(product);
    }
    
    [HttpPost]
    [RateLimit(PeriodInSec = 60, Limit = 30)]
    [FoodTruckExistsFilter("truckId")]
    [FoodTruckManagerFilter("truckId")]
    public async Task<IActionResult> CreateProduct(Guid truckId, [FromBody] ProductRegistry product)
    {
        var validationResult = await _productValidator.ValidateAsync(product);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);
        
        var created = await _productService.CreateProduct(truckId, product);
        
        //URI.EscapeDatastring makes sure URI link works even if name contains space or weird charachters
        return Created($"api/foodtruck/{truckId}/product/{Uri.EscapeDataString(created.Name)}", created);
        
    }
    
    [HttpPut("{productName}")]
    [RateLimit(PeriodInSec = 60, Limit = 30)]
    [FoodTruckExistsFilter("truckId")]
    [FoodTruckManagerFilter("truckId")]
    public async Task<IActionResult> UpdateProduct(Guid truckId, string productName, [FromBody] ProductRegistry product)
    {
        var validationResult = await _productValidator.ValidateAsync(product);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);
        
        var updated = await _productService.UpdateProduct(truckId, productName, product);
        if (updated is null)
            return NotFound();

        return Ok(updated);
    }

    [HttpDelete("{productName}")]
    [RateLimit(PeriodInSec = 60, Limit = 30)]
    [FoodTruckExistsFilter("truckId")]
    [FoodTruckManagerFilter("truckId")]
    public async Task<IActionResult> DeleteProduct(Guid truckId, string productName)
    {
        var deleted = await _productService.DeleteProduct(truckId, productName);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}