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

namespace DtuFoodAPI.Tests.Controllers.FoodTruck;

public class FoodTruckTests
{
    private ILogger<FoodTruckController> _logger;
    private IFoodTruckService _foodTruckService;
    private ITokenGenerator _tokenGenerator;
    

    private FoodTruckController _sut;

    [SetUp]
    public void Setup()
    {
        _logger = Substitute.For<ILogger<FoodTruckController>>();
        _foodTruckService = Substitute.For<IFoodTruckService>();
        
        _tokenGenerator = Substitute.For<ITokenGenerator>();
        
        _sut = new FoodTruckController(_logger, _foodTruckService);
        
        // HTTP context for update and delete
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }
    
    [Test]
    public async Task CreateFoodTruck_ReturnsCreated_WhenSuccessful()
    {
        //Arrange
        var registry = new FoodTruckRegistry
        {
            Name = "Grethes Food Truck",
            GpsLatitude = 55.6761f,
            GpsLongitude = 12.5683f
        };
        
        var foodTruck = new Models.FoodTruck()
        {
            Id = Guid.NewGuid(),
            Name = registry.Name,
            GpsLatitude = 55.6761f,
            GpsLongitude = 12.5683f,
            Availability   = new List<Availability>(),
            Products = new List<Models.Product>(),
            Managers = new List<User>()
        };

        _foodTruckService.CreateFoodTruck(registry)
            .Returns(foodTruck);
        
        //Act
        var result = await _sut.CreateFoodTruck(registry);
        
        //Assert
        //assert correct controller responmse
        Assert.That(result, Is.InstanceOf<CreatedResult>());
        
    }
    
    [Test]
    public async Task GetAllFoodTrucks_ReturnsOK_WhenSuccessful()
    {
        //Arrange
        var foodTrucks = new List<Models.FoodTruck>();
        
        var foodTruck1 = new Models.FoodTruck()
        {
            Id = Guid.NewGuid(),
            Name = "Burger Truck",
            GpsLatitude = 55.6761f,
            GpsLongitude = 12.5683f,
            Availability   = new List<Availability>(),
            Products = new List<Models.Product>(),
            Managers = new List<User>()
        };
        var foodTruck2 = new Models.FoodTruck()
        {
            Id = Guid.NewGuid(),
            Name = "Slik Truck",
            GpsLatitude = 56.6761f,
            GpsLongitude = 16.5683f,
            Availability   = new List<Availability>(),
            Products = new List<Models.Product>(),
            Managers = new List<User>()
        };
        foodTrucks.Add(foodTruck1); foodTrucks.Add(foodTruck2);
        
        _foodTruckService.GetAllFoodTrucks()
            .Returns(foodTrucks);
        
        //Act
        var result = await _sut.GetAllFoodTrucks();
        var OKFoodTrucks = result as OkObjectResult; 
        
        
        //Assert
        //assert correct controller responmse
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        
        //assert correct amt of objects
        var trucks = OKFoodTrucks?.Value as IEnumerable<Models.FoodTruck>;
        Assert.That(trucks, Is.Not.Null);
        Assert.That(trucks.Count(), Is.EqualTo(foodTrucks.Count()));
    }
    
    [Test]
    public async Task GetFoodTruckById_ReturnsOK_WhenSuccessful()
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
        
        _foodTruckService.GetFoodTruckById(foodTruck.Id)
            .Returns(foodTruck);
        
        //Act
        var result = await _sut.GetAllFoodTrucks();
        
        //Assert
        //assert correct controller response
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }
    
    //When service successfully updates a food truck, the controller returns HTTP 200 OK with the updated object.
    [Test]
    public async Task UpdateFoodTruck_ReturnsOk_WhenUpdated()
    {
        // Arrange
        var id = Guid.NewGuid();
        var registry = new FoodTruckRegistry
        {
            Name = "Updated Truck",
            GpsLatitude = 12.34f,
            GpsLongitude = 13.37f
        };

        var updated = new DtuFoodAPI.Models.FoodTruck
        {
            Id = id,
            Name = registry.Name,
            GpsLatitude = registry.GpsLatitude,
            GpsLongitude = registry.GpsLongitude,
            Availability = new List<Availability>(),
            Products = new List<DtuFoodAPI.Models.Product>(),
            Managers = new List<User>()
        };

        _foodTruckService.UpdateFoodTruck(id, registry, Arg.Any<CancellationToken>())
            .Returns(updated);

        // Act
        var result = await _sut.UpdateFoodTruck(id, registry);
        var ok = result as OkObjectResult;

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.EqualTo(updated));
    }


    // Verify that when NULL is returned by the service, the controller correctly returns a 404 Not Found.
    [Test]
    public async Task UpdateFoodTruck_ReturnsNotFound_WhenMissing()
    {
        // Arrange
        var id = Guid.NewGuid();
        var registry = new FoodTruckRegistry
        {
            Name = "Nonexistent Truck",
            GpsLatitude = 10.0f,
            GpsLongitude = 20.0f
        };

        _foodTruckService.UpdateFoodTruck(id, registry, Arg.Any<CancellationToken>())
            .Returns((DtuFoodAPI.Models.FoodTruck?)null);

        // Act
        var result = await _sut.UpdateFoodTruck(id, registry);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    
    // When the service returns true, the controller returns HTTP 204 No Content.
    [Test]
    public async Task DeleteFoodTruck_ReturnsNoContent_WhenDeleted()
    {
        // Arrange
        var id = Guid.NewGuid();
        _foodTruckService.DeleteFoodTruck(id, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _sut.DeleteFoodTruck(id);

        // Assert
        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    
    // When the service gives false, controller should respond with HTTP 404 Not Found.
    [Test]
    public async Task DeleteFoodTruck_ReturnsNotFound_WhenMissing()
    {
        // Arrange
        var id = Guid.NewGuid();
        _foodTruckService.DeleteFoodTruck(id, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _sut.DeleteFoodTruck(id);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }
}