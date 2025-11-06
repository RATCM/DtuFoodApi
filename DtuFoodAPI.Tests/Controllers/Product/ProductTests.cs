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
        
        _sut = new ProductController(_logger, _productService);
    }
    
    [Test]
    public async Task CreateProduct_ReturnsCreated_WhenSuccessful()
    {
        //Arrange
        var registry = new ProductRegistry
        {
            Name = "Cola",
            Description = "Smager godt",
            Price = 100,
        };
        
        var foodTruck = new Models.FoodTruck()
        {
            Id = Guid.NewGuid(),
            Name = "Cola Truck",
            GpsLatitude = 60.6761f,
            GpsLongitude = 36.5683f,
            Availability   = new List<Availability>(),
            Products = new List<Models.Product>(),
            Managers = new List<User>()
        };
        
        var product = new Models.Product()
        {
            FoodTruck = foodTruck,
            Name = registry.Name,
            Description = registry.Description,
            Price = registry.Price,
        };

        _productService.CreateProduct(product.FoodTruck.Id,registry)
            .Returns(product);
        
        //Act
        var result = await _sut.CreateProduct(product.FoodTruck.Id,registry);
        
        //Assert
        //assert correct controller responmse
        Assert.That(result, Is.InstanceOf<CreatedResult>());
    }
    
    [Test]
    public async Task GetAllProductsByTruck_ReturnsOK_WhenSuccessful()
    {
        //Arrange
        var products = new List<Models.Product>();
        
        var foodTruck = new Models.FoodTruck()
        {
            Id = Guid.NewGuid(),
            Name = "Cola Truck",
            GpsLatitude = 60.6761f,
            GpsLongitude = 36.5683f,
            Availability   = new List<Availability>(),
            Products = new List<Models.Product>(),
            Managers = new List<User>()
        };
        
        var product1 = new Models.Product()
        {
            FoodTruck = foodTruck,
            Name = "Fanta",
            Description = "Appelsin smag",
            Price = 20,
        };
        var product2 = new Models.Product()
        {
            FoodTruck = foodTruck,
            Name = "Cola",
            Description = "Cola smag",
            Price = 30,
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
        var prods = OKProducts?.Value as IEnumerable<Models.Product>;
        Assert.That(prods, Is.Not.Null);
        Assert.That(prods.Count(), Is.EqualTo(products.Count()));
    }
    
    [Test]
    public async Task GetProductByTruckIdAndProductName_ReturnsOK_WhenSuccessful()
    {
        //Arrange
        var foodTruck = new Models.FoodTruck()
        {
            Id = Guid.NewGuid(),
            Name = "Cola Truck",
            GpsLatitude = 60.6761f,
            GpsLongitude = 36.5683f,
            Availability   = new List<Availability>(),
            Products = new List<Models.Product>(),
            Managers = new List<User>()
        };
        
        var product1 = new Models.Product()
        {
            FoodTruck = foodTruck,
            Name = "Fanta",
            Description = "Appelsin smag",
            Price = 20,
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
        var productId = Guid.NewGuid(); 
        var registry = new ProductRegistry
        {
            Name = "Cola",
            Description = "Ny beskrivelse",
            Price = 42
        };

        var updated = new Models.Product
        {
            FoodTruck = new Models.FoodTruck
            {
                Id = truckId, Name = "Truck", GpsLatitude = 1, GpsLongitude = 2,
                Availability = new List<Availability>(),
                Products     = new List<Models.Product>(),
                Managers     = new List<User>()
            },
            Name = registry.Name,
            Description = registry.Description,
            Price = registry.Price
        };
        
        _productService.UpdateProduct(truckId, productId.ToString(), registry).Returns(updated);

        // Act
        var result = await _sut.UpdateProduct(truckId, productId, registry);
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
        var productId = Guid.NewGuid();

        _productService.DeleteProduct(truckId, productId.ToString())
            .Returns(true);

        // Act
        var result = await _sut.DeleteProduct(truckId, productId);

        // Assert
        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    [Test]
    public async Task DeleteProduct_ReturnsNotFound_WhenMissing()
    {
        // Arrange
        var truckId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        _productService.DeleteProduct(truckId, productId.ToString())
            .Returns(false);

        // Act
        var result = await _sut.DeleteProduct(truckId, productId);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task UpdateProduct_ReturnsNotFound_WhenMissing()
    {
        // Arrange
        var truckId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var registry = new ProductRegistry
        {
            Name = "Cola",
            Description = "Opdateret beskrivelse",
            Price = 55
        };

        _productService.UpdateProduct(truckId, productId.ToString(), registry)
            .Returns((Models.Product?)null);

        // Act
        var result = await _sut.UpdateProduct(truckId, productId, registry);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    
}