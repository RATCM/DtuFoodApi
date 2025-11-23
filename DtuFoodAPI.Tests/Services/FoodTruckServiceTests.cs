using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using DtuFoodAPI.Database;
using DtuFoodAPI.DTOs;
using DtuFoodAPI.Models;
using DtuFoodAPI.Services;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NUnit.Framework;

namespace DtuFoodAPI.Tests.Services;

[TestFixture]
public class FoodTruckServiceTests
{
    private IDtuFoodDbContext _dbContext;
    private FoodTruckService _service;
    private IGuidGenerator _guidGenerator;

    private Guid _truckId;
    private Guid _userId;

    [NUnit.Framework.SetUp]
    public void Setup()
    {
        _truckId = Guid.NewGuid();
        _userId = Guid.NewGuid();

        var options = new DbContextOptionsBuilder<DtuFoodDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var realContext = new DtuFoodDbContext(options);
        _dbContext = realContext;

        _guidGenerator = Substitute.For<IGuidGenerator>();
        _guidGenerator.NewGuid().Returns(_truckId);

        _service = new FoodTruckService(_dbContext, _guidGenerator);
    }

    private FoodTruckRegistry CreateRegistry(string name = "TestTruck")
    {
        return new FoodTruckRegistry
        {
            Name = name,
            GpsLatitude = 55.0,
            GpsLongitude = 12.0
        };
    }

    // =======================
    // CREATE
    // =======================

    [Test]
    public async Task CreateFoodTruck_ShouldAddTruckToDatabase()
    {
        var registry = CreateRegistry();

        var result = await _service.CreateFoodTruck(registry);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(_truckId));
        Assert.That(result.Name, Is.EqualTo("TestTruck"));

        var inDb = await _dbContext.FoodTrucks.FirstOrDefaultAsync();
        Assert.That(inDb, Is.Not.Null);
    }

    // =======================
    // GET ALL
    // =======================

    [Test]
    public async Task GetAllFoodTrucks_ShouldReturnAll()
    {
        _dbContext.FoodTrucks.Add(new FoodTruck
        {
            Id = _truckId,
            Name = "Truck 1"
        });

        await _dbContext.SaveChangesAsync();

        var result = await _service.GetAllFoodTrucks();

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Name, Is.EqualTo("Truck 1"));
    }

    // =======================
    // GET BY ID
    // =======================

    [Test]
    public async Task GetFoodTruckById_WhenExists_ShouldReturnTruck()
    {
        _dbContext.FoodTrucks.Add(new FoodTruck
        {
            Id = _truckId,
            Name = "TruckX"
        });

        await _dbContext.SaveChangesAsync();

        var result = await _service.GetFoodTruckById(_truckId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Name, Is.EqualTo("TruckX"));
    }

    [Test]
    public async Task GetFoodTruckById_WhenNotExists_ShouldReturnNull()
    {
        var result = await _service.GetFoodTruckById(Guid.NewGuid());
        Assert.That(result, Is.Null);
    }

    // =======================
    // UPDATE
    // =======================

    [Test]
    public async Task UpdateFoodTruck_WhenExists_ShouldUpdateFields()
    {
        _dbContext.FoodTrucks.Add(new FoodTruck
        {
            Id = _truckId,
            Name = "Old Name"
        });

        await _dbContext.SaveChangesAsync();

        var newData = CreateRegistry("Updated Name");

        var result = await _service.UpdateFoodTruck(_truckId, newData);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Name, Is.EqualTo("Updated Name"));
    }

    [Test]
    public async Task UpdateFoodTruck_WhenNotExists_ShouldReturnNull()
    {
        var registry = CreateRegistry();
        var result = await _service.UpdateFoodTruck(Guid.NewGuid(), registry);

        Assert.That(result, Is.Null);
    }

    // =======================
    // DELETE
    // =======================

    [Test]
    public async Task DeleteFoodTruck_WhenExists_ShouldReturnTrue()
    {
        _dbContext.FoodTrucks.Add(new FoodTruck
        {
            Id = _truckId,
            Name = "DeleteMe"
        });

        await _dbContext.SaveChangesAsync();

        var result = await _service.DeleteFoodTruck(_truckId);

        Assert.That(result, Is.True);

        var inDb = await _dbContext.FoodTrucks.FirstOrDefaultAsync();
        Assert.That(inDb, Is.Null);
    }

    [Test]
    public async Task DeleteFoodTruck_WhenNotExists_ShouldReturnFalse()
    {
        var result = await _service.DeleteFoodTruck(Guid.NewGuid());
        Assert.That(result, Is.False);
    }

    // =======================
    // EXISTS
    // =======================

    [Test]
    public async Task FoodTruckExists_WhenExists_ShouldReturnTrue()
    {
        _dbContext.FoodTrucks.Add(new FoodTruck
        {
            Id = _truckId,
            Name = "Exists"
        });

        await _dbContext.SaveChangesAsync();

        var result = await _service.FoodTruckExists(_truckId);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task FoodTruckExists_WhenNotExists_ShouldReturnFalse()
    {
        var result = await _service.FoodTruckExists(Guid.NewGuid());
        Assert.That(result, Is.False);
    }

    // =======================
    // ADD MANAGER
    // =======================

    [Test]
    public async Task AddFoodTruckManager_WhenValid_ShouldAddUser()
    {
        _dbContext.FoodTrucks.Add(new FoodTruck
        {
            Id = _truckId,
            Name = "Truck",
            Managers = new List<User>(),
            Products = new List<Product>(),
            Availability = new List<Availability>(),
        });

        _dbContext.Users.Add(new User
        {
            Id = _userId,
            Email = "test@test.com"
        });

        await _dbContext.SaveChangesAsync();

        var result = await _service.AddFoodTruckManager(_truckId, _userId);

        Assert.That(result, Is.Not.Null);

        var truck = await _dbContext.FoodTrucks
            .Include(t => t.Managers)
            .FirstAsync();

        Assert.That(truck.Managers.Count, Is.EqualTo(1));
    }

}
