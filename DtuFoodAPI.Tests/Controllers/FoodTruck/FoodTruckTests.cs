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
}