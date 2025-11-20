using DotNet.RateLimiter.ActionFilters;
using DtuFoodAPI.Database;
using DtuFoodAPI.DTOs;
using DtuFoodAPI.Filters;
using DtuFoodAPI.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DtuFoodAPI.Controllers;

/// <summary>
/// Endpoints for the food truck products
/// </summary>
[ApiController]
[Route("api/foodtruck/{truckId:guid}/product")]
public class ProductController : ControllerBase
{
    private readonly ILogger<ProductController> _logger;
    private readonly IProductService _productService;
    private readonly IValidator<ProductRegistry> _productValidator;
    
    /// <summary>
    /// Product controller constructor
    /// </summary>
    /// <param name="logger">The logger</param>
    /// <param name="productService">The product service</param>
    /// <param name="productValidator">The product validator</param>
    public ProductController(ILogger<ProductController> logger,
        IProductService productService,
        IValidator<ProductRegistry> productValidator)
    {
        _productService = productService;
        _logger = logger;
        _productValidator = productValidator;
    }

    /// <summary>
    /// Gets all the products for a food truck
    /// </summary>
    /// <param name="truckId">The food truck id</param>
    /// <returns>The list of products</returns>
    /// <response code="200">If the request was successful</response>
    /// <response code="404">If the food truck was not found</response>
    /// <response code="429">If the rate limit is exceeded</response>
    [HttpGet]
    [RateLimit(PeriodInSec = 60, Limit = 30)]
    [FoodTruckExistsFilter("truckId")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(List<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GetAllProductsByTruck(Guid truckId)
    {
        return Ok(await _productService.GetAllProductsFromFoodTruck(truckId));
    }
    
    //old name GetProductById(), but product has no id, it has composite key with name and truckid.
    /// <summary>
    /// Gets a specific product from a food truck
    /// </summary>
    /// <param name="truckId">The food truck id</param>
    /// <param name="productName">The product name</param>
    /// <returns>The product</returns>
    /// <response code="200">If the request was successful</response>
    /// <response code="404">If the product or food truck was not found</response>
    /// <response code="429">If the rate limit is exceeded</response>
    [HttpGet("{productName}")]
    [RateLimit(PeriodInSec = 60, Limit = 30)]
    [FoodTruckExistsFilter("truckId")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GetProductByTruckIdAndProductName(Guid truckId, string productName)
    {
        var product = await _productService.GetProductByTruckIdAndProductName(truckId,productName);
        if (product is null)
            return NotFound();
        
        return Ok(product);
    }
    
    /// <summary>
    /// Creates a product for a food truck
    /// </summary>
    /// <param name="truckId">The food truck id</param>
    /// <param name="product">The product registry</param>
    /// <returns>The newly created product</returns>
    /// <response code="201">If the request was successful</response>
    /// <response code="400">If the product fails validation</response>
    /// <response code="401">If an invalid (or none) access token was provided</response>
    /// <response code="403">If the user is not a manager</response>
    /// <response code="404">If the food truck was not found</response>
    /// <response code="429">If the rate limit is exceeded</response>
    [HttpPost]
    [RateLimit(PeriodInSec = 60, Limit = 30)]
    [Authorize]
    [FoodTruckExistsFilter("truckId")]
    [FoodTruckManagerFilter("truckId")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> CreateProduct(Guid truckId, [FromBody] ProductRegistry product)
    {
        var validationResult = await _productValidator.ValidateAsync(product);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);
        
        var created = await _productService.CreateProduct(truckId, product);
        
        _logger.LogInformation("Product created, truckId: {id}, name: {name}", truckId, product.Name);
        //URI.EscapeDatastring makes sure URI link works even if name contains space or weird charachters
        return Created($"api/foodtruck/{truckId}/product/{Uri.EscapeDataString(created.Name)}", created);
        
    }
    
    /// <summary>
    /// Updates a product for a food truck
    /// </summary>
    /// <param name="truckId">The food truck id</param>
    /// <param name="productName">The product name</param>
    /// <param name="product">The product registry for updating</param>
    /// <returns>The updated product</returns>
    /// <response code="200">If the product was updated</response>
    /// <response code="400">If the product registry fails validation</response>
    /// <response code="401">If an invalid (or none) access token was provided</response>
    /// <response code="403">If the user is not a manager</response>
    /// <response code="404">If the food truck was not found</response>
    /// <response code="429">If the rate limit is exceeded</response>
    [HttpPut("{productName}")]
    [RateLimit(PeriodInSec = 60, Limit = 30)]
    [Authorize]
    [FoodTruckExistsFilter("truckId")]
    [FoodTruckManagerFilter("truckId")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UpdateProduct(Guid truckId, string productName, [FromBody] ProductRegistry product)
    {
        var validationResult = await _productValidator.ValidateAsync(product);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);
        
        var updated = await _productService.UpdateProduct(truckId, productName, product);
        if (updated is null)
            return NotFound();

        _logger.LogInformation("Product updated, truckId: {id}, name: {name}", truckId, productName);
        return Ok(updated);
    }

    /// <summary>
    /// Deletes a product from a food truck
    /// </summary>
    /// <param name="truckId">The food truck id</param>
    /// <param name="productName">The product name</param>
    /// <returns>No Content</returns>
    /// <response code="204">If the product was deleted</response>
    /// <response code="401">If an invalid (or none) access token was provided</response>
    /// <response code="403">If the user is not a manager</response>
    /// <response code="404">If the food truck was not found</response>
    /// <response code="429">If the rate limit is exceeded</response>
    [HttpDelete("{productName}")]
    [RateLimit(PeriodInSec = 60, Limit = 30)]
    [FoodTruckExistsFilter("truckId")]
    [FoodTruckManagerFilter("truckId")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteProduct(Guid truckId, string productName)
    {
        var deleted = await _productService.DeleteProduct(truckId, productName);
        if (!deleted)
            return NotFound();
        
        _logger.LogInformation("Product deleted, truckId: {id}, name: {name}", truckId, productName);

        return NoContent();
    }
}