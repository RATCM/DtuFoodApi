using System.Globalization;
using DtuFoodAPI.Validation;

namespace DtuFoodAPI.Tests.Controllers.Product;
using System.Collections;
using System.Security.Claims;
using DtuFoodAPI.Controllers;
using DtuFoodAPI.DTOs;
using DtuFoodAPI.Models;
using DtuFoodAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;

public class ProductTests
{
    private ILogger<ProductController> _logger;
    private IProductService _productService;
    private ITokenGenerator _tokenGenerator;

    private ProductController _sut;

    [SetUp]
    public void Setup()
    {
        _logger = Substitute.For<ILogger<ProductController>>();
        _productService = Substitute.For<IProductService>();
        _tokenGenerator = Substitute.For<ITokenGenerator>();
        
        
        _sut = new ProductController(_logger, _productService, new ProductRegistryValidator());
    }
    
    [Test]
    public async Task CreateProduct_ReturnsCreated_WhenSuccessful()
    {
        //Arrange
        var registry = new ProductRegistry
        {
            Name = "Cola",
            Description = "Smager godt",
            Price = "100",
        };
        
        var foodTruck = new FoodTruckDto()
        {
            Id = Guid.NewGuid(),
            Name = "Cola Truck",
            GpsLatitude = 60.6761f,
            GpsLongitude = 36.5683f,
            Availability   = [],
            Products = []
        };
        
        var product = new ProductDto()
        {
            Name = registry.Name,
            Description = registry.Description,
            Price = registry.Price.ToString(CultureInfo.InvariantCulture),
            Category = registry.Category,
        };

        _productService.CreateProduct(foodTruck.Id,registry)
            .Returns(product);
        
        //Act
        var result = await _sut.CreateProduct(foodTruck.Id,registry);
        
        //Assert
        //assert correct controller responmse
        Assert.That(result, Is.InstanceOf<CreatedResult>());
    }
    
    [Test]
    public async Task GetAllProductsByTruck_ReturnsOK_WhenSuccessful()
    {
        //Arrange
        var products = new List<ProductDto>();
        
        var foodTruck = new Models.FoodTruck()
        {
            Id = Guid.NewGuid(),
            Name = "Cola Truck",
            GpsLatitude = 60.6761f,
            GpsLongitude = 36.5683f,
            Availability   = new List<Availability>(),
            Products = new List<Models.Product>(),
            Managers = new List<User>(),
            PageBanner = null
        };
        
        var product1 = new ProductDto()
        {
            Name = "Fanta",
            Description = "Appelsin smag",
            Price = "20",
            Category = null,
        };
        var product2 = new ProductDto()
        {
            Name = "Cola",
            Description = "Cola smag",
            Price = "30",
            Category = null,
        };
        
        products.Add(product1); products.Add(product2);
        
        _productService.GetAllProductsFromFoodTruck(foodTruck.Id)
            .Returns(products);
        
        //Act
        var result = await _sut.GetAllProductsByTruck(foodTruck.Id);
        var OKProducts = result as OkObjectResult; 
        
        //Assert
        //assert correct controller responmse
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        
        //assert correct amt of objects
        var prods = OKProducts?.Value as IEnumerable<ProductDto>;
        Assert.That(prods, Is.Not.Null);
        Assert.That(prods.Count(), Is.EqualTo(products.Count()));
    }
    
    [Test]
    public async Task GetProductByTruckIdAndProductName_ReturnsOK_WhenSuccessful()
    {
        //Arrange
        var foodTruck = new FoodTruckDto()
        {
            Id = Guid.NewGuid(),
            Name = "Cola Truck",
            GpsLatitude = 60.6761f,
            GpsLongitude = 36.5683f,
            Availability   = [],
            Products = []
        };
        
        var product1 = new ProductDto()
        {
            Name = "Fanta",
            Description = "Appelsin smag",
            Price = "20",
            Category = null,
        };
        
        _productService.GetProductByTruckIdAndProductName(foodTruck.Id,"Fanta")
            .Returns(product1);
        
        //Act
        var result = await _sut.GetProductByTruckIdAndProductName(foodTruck.Id,"Fanta");
        
        //Assert
        //assert correct controller response
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task UpdateProduct_ReturnsOk_WhenUpdated()
    {
        // Arrange
        var truckId = Guid.NewGuid();
        var productName = "Cola"; 
        var registry = new ProductRegistry
        {
            Name = "Cola",
            Description = "Ny beskrivelse",
            Price = "42"
        };

        var updated = new ProductDto
        {
            Name = registry.Name,
            Description = registry.Description,
            Price = registry.Price.ToString(CultureInfo.InvariantCulture),
            Category = registry.Category,
        };
        
        _productService.UpdateProduct(truckId, productName, registry).Returns(updated);

        // Act
        var result = await _sut.UpdateProduct(truckId, productName, registry);
        var ok = result as OkObjectResult;

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.EqualTo(updated));
    }

    [Test]
    public async Task DeleteProduct_ReturnsNoContent_WhenDeleted()
    {

        var truckId = Guid.NewGuid();
        var productName = "some name";

        _productService.DeleteProduct(truckId, productName)
            .Returns(true);

        // Act
        var result = await _sut.DeleteProduct(truckId, productName);

        // Assert
        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    [Test]
    public async Task DeleteProduct_ReturnsNotFound_WhenMissing()
    {
        // Arrange
        var truckId = Guid.NewGuid();
        var productName = "some name";

        _productService.DeleteProduct(truckId, productName)
            .Returns(false);

        // Act
        var result = await _sut.DeleteProduct(truckId, productName);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task UpdateProduct_ReturnsNotFound_WhenMissing()
    {
        // Arrange
        var truckId = Guid.NewGuid();
        var productName = "Cola";

        var registry = new ProductRegistry
        {
            Name = "Cola",
            Description = "Opdateret beskrivelse",
            Price = "55"
        };

        _productService.UpdateProduct(truckId, productName, registry)
            .Returns((ProductDto?)null);

        // Act
        var result = await _sut.UpdateProduct(truckId, productName, registry);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    
}